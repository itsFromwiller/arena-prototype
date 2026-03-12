using Arena.Enemies;
using Arena.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Combat
{
    public class CombatEntity
    {
        public string Name;
        public int HP;
        public virtual int MaxHP { get; protected set; }
        public int MP;
        public virtual int MaxMP { get; protected set; }
        public int Level;
        public int Attack { get; protected set; }
        public int MAttack { get; protected set; }
        public int Defense { get; protected set; }
        public int MDefense { get; protected set; }
        public int Speed { get; protected set; }

        public List<SkillEntity> ActiveSkills = new();
        public List<BuffEntity> ActiveBuffs = new();

        public virtual void Init()
        {
            foreach (var skillEntry in ActiveSkills)
            {
                skillEntry.SkillData = SkillSystem.Instance.GetSkillData(skillEntry.SkillName);
            }
        }

        public int GetInitiative()
        {
            return CalculatedSpeed();
        }

        public bool CanAttack()
        {
            return HP > 0;
        }

        public bool IsDead()
        {
            return HP <= 0;
        }

        public virtual bool DidAttackSuccessfully()
        {
            int chanceToHit = 70;
            if (UnityEngine.Random.Range(0, 100) < chanceToHit)
            {
                return true;
            }
            return false;
        }

        public virtual bool DidUseSkillSuccessfully(CombatContext combatContext)
        {
            if (UnityEngine.Random.Range(0, 100) < (int)(combatContext.SkillUsed.SuccessRate * 100))
            {
                return true;
            }
            return false;
        }

        protected void ModifyStatViaActiveSkills(ref double statReference, params SkillType[] skillTypesToUse)
        {
            foreach (var activeSkill in ActiveSkills)
            {
                if (activeSkill.IsExpired)
                {
                    continue;
                }
                var skillData = activeSkill.SkillData;
                foreach (var skillTypeToUse in skillTypesToUse)
                {
                    if (skillData.SkillType == skillTypeToUse)
                    {
                        if (skillData.SkillValue > 0)
                        {
                            statReference += skillData.SkillValue;
                        }
                        else
                        {
                            statReference *= skillData.SkillPercentage;
                        }
                        break;
                    }
                }
            }
        }

        public virtual int CalculatedAttack()
        {
            double result = Attack;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyAttack, SkillType.ModifyAllAttack);
            return (int) result;
        }

        public virtual int CalculatedMAttack()
        {
            double result = MAttack;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyMDefense, SkillType.ModifyAllAttack);
            return (int)result;
        }

        public virtual int CalculatedDefense()
        {
            double result = Defense;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyDefense, SkillType.ModifyAllDefense);
            return (int)result;
        }

        public virtual int CalculatedMDefense()
        {
            double result = MDefense;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyMDefense, SkillType.ModifyAllDefense);
            return (int)result;
        }

        public virtual int CalculatedSpeed()
        {
            double result = Speed;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifySpeed);
            return (int)result;
        }

        public int CalculatedAttackWithSkill(SkillData skillData)
        {
            return CalculatedAttack() + skillData.SkillValue;
        }

        public int CalculatedMAttackWithSkill(SkillData skillData)
        {
            return CalculatedMAttack() + skillData.SkillValue;
        }

        public int CalculatedAttackWithEnemyAction(EnemyActionData enemyActionData)
        {
            return (int) (CalculatedAttack() * enemyActionData.DamageMultiplier);
        }

        public int CalculatedMAttackWithEnemyAction(EnemyActionData enemyActionData)
        {
            return (int) (CalculatedMAttack() * enemyActionData.DamageMultiplier);
        }

        public void AttackTarget(CombatEntity target, CombatContext combatContext)
        {
            combatContext.DamageDealt = ReducedDamageFromDefense(CalculatedAttack(), target.CalculatedDefense());
            target.TakeDamage(this, combatContext);
        }

        public void AttackTargetWithSkill(CombatEntity target, SkillData skillData, CombatContext combatContext)
        {
            if (skillData.ElementType == ElementType.None)
            {
                combatContext.DamageDealt = ReducedDamageFromDefense(CalculatedAttackWithSkill(skillData), target.CalculatedDefense());
            }
            else
            {
                combatContext.DamageDealt = ReducedDamageFromDefense(CalculatedMAttackWithSkill(skillData), target.CalculatedMDefense());
            }
            target.TakeDamage(this, combatContext);
        }

        public void AttackTargetWithEnemyAction(CombatEntity target, CombatContext combatContext)
        {
            if (combatContext.EnemyActionUsed.MPCost == 0)
            {
                combatContext.DamageDealt = ReducedDamageFromDefense(CalculatedAttackWithEnemyAction(combatContext.EnemyActionUsed), target.CalculatedDefense());
            }
            else
            {
                combatContext.DamageDealt = ReducedDamageFromDefense(CalculatedMAttackWithEnemyAction(combatContext.EnemyActionUsed), target.CalculatedMDefense());
            }
            target.TakeDamage(this, combatContext);
        }

        public void AttackTargetWithItem(CombatEntity target, ItemData itemData, CombatContext combatContext)
        {
            // Haven't decided if this should include damage reduction or not.
            // For now, no damage reduction, just straight damage.
//            combatContext.DamageDealt = ReducedDamageFromDefense(CalculatedAttackWithItem(itemData), target.Defense);
            int.TryParse(itemData.ActionValue, out combatContext.DamageDealt);
            target.TakeDamage(this, combatContext);
        }

        public virtual void TakeDamage(CombatEntity source, CombatContext combatContext)
        {
            HP = Math.Max(0, HP - combatContext.DamageDealt);
        }

        public virtual void Heal(CombatContext combatContext)
        {
            HP = Math.Min(MaxHP, HP + combatContext.HealingAmount);
        }

        public virtual void RestoreMP(CombatContext combatContext)
        {
            MP = Math.Min(MaxMP, MP + combatContext.RestoreAmount);
        }

        public virtual void StealMP(CombatEntity target, CombatContext combatContext)
        {
            if (target.IsDead())
            {
                return;
            }
            int stolenAmount = Math.Min(target.MP, combatContext.StealAmount);
            combatContext.StealAmount = stolenAmount;
            target.MP -= combatContext.StealAmount;
            MP = Math.Min(MaxMP, MP + combatContext.StealAmount);
        }

        public virtual void UseSkill(CombatContext combatContext)
        {
            MP = Math.Max(0, MP - combatContext.SkillUsed.MPCost);
        }

        public int ReducedDamageFromDefense(int damage, int defense)
        {
            // Calculation will be: damage / (scaler + Defense) / scaler
            // So, no defense takes full damage.
            // Large defense can take less damage.
            double scaler = 50.0;
            return (int)(Math.Ceiling(damage / ((scaler + defense) / scaler)));
        }

        public bool TryGetActiveSkillEntity(SkillData skillData, out SkillEntity activeSkillEntity)
        {
            activeSkillEntity = null;
            foreach (var activeSkill in ActiveSkills)
            {
                if (activeSkill.SkillData == skillData)
                {
                    activeSkillEntity = activeSkill;
                    return true;
                }
            }
            return false;
        }

        public void AddActiveSkillEntity(SkillEntity skillEnity)
        {
            ActiveSkills.Add(skillEnity);
        }

        public void ProcessActiveSkillLifetimes(SkillLifetime skillLifetime)
        {
            bool willRemoveExpired = false;
            foreach (var activeSkill in ActiveSkills)
            {
                if (activeSkill.IsExpired)
                {
                    willRemoveExpired = true;
                    continue;
                }
                if (activeSkill.SkillLifetime == skillLifetime)
                {
                    activeSkill.DecrementLifetimeValue();
                    willRemoveExpired = true;
                }
            }
            if (willRemoveExpired)
            {
                bool validateHP = false;
                bool validateMP = false;
                for (int i = ActiveSkills.Count - 1; i >= 0; --i)
                {
                    if (ActiveSkills[i].IsExpired)
                    {
                        if (ActiveSkills[i].SkillData.SkillType == SkillType.ModifyMaxHP)
                        {
                            validateHP = true;
                        }
                        else if (ActiveSkills[i].SkillData.SkillType == SkillType.ModifyMaxMP)
                        {
                            validateMP = true;
                        }
                        ActiveSkills.RemoveAt(i);
                    }
                }
                if (validateHP)
                {
                    ValidateHPandMaxHP();
                }
                if (validateMP)
                {
                    ValidateMPandMaxMP();
                }
            }
        }

        public virtual void ValidateHPandMaxHP()
        {
            if (HP > MaxHP)
            {
                HP = MaxHP;
            }
        }

        public virtual void ValidateMPandMaxMP()
        {
            if (MP > MaxMP)
            {
                MP = MaxMP;
            }
        }

    }
}