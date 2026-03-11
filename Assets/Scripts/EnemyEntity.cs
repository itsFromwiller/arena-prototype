using Arena.Combat;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Enemies
{
    public class EnemyEntity : CombatEntity
    {
        public EnemyData Data;

        public int XP;
        public double Scale;
        public EnemyActionData Action1;
        public EnemyActionData Action2;
        public EnemyActionData Action3;
        public EnemyActionData Action4;

        [JsonIgnore]
        public EnemyActionData ActionToPerform;

        public Dictionary<string, int> ActionCooldown = new();

        public EnemyEntity(EnemyData enemyData, int level)
        {
            double scale = (0.5 + level / 2.0);

            // CombatEntity
            Name = enemyData.Name;
            MaxHP = (int)(scale * enemyData.HP);
            HP = MaxHP;
            MaxMP = (int)(scale * enemyData.MP);
            MP = MaxMP;
            Level = level;
            Attack = (int)(scale * enemyData.Attack);
            MAttack = (int)(scale * enemyData.MAttack);
            Defense = (int)(scale * enemyData.Defense);
            MDefense = (int)(scale * enemyData.MDefense);
            Speed = (int)(scale * enemyData.Speed);

            // EnemyEntity
            Data = enemyData;
            XP = (int)(scale * enemyData.XP);
            Scale = scale;
        }

        public void ProcessActionCooldowns()
        {
            foreach (var entry in ActionCooldown.Keys)
            {
                if (ActionCooldown[entry] > 0)
                {
                    ActionCooldown[entry]--;
                }
            }
        }

        public void ProcessBuffs()
        {
            for (int i = ActiveBuffs.Count - 1; i >= 0; --i)
            {
                var activeBuff = ActiveBuffs[i];
                if (!activeBuff.IsExpired)
                {
                    activeBuff.DecrementLifetimeValue();
                }
                if (activeBuff.IsExpired)
                {
                    ActiveBuffs.RemoveAt(i);
                    continue;
                }
            }
        }

        public void PrepareForAttack()
        {
            List<EnemyActionData> randomActions = new List<EnemyActionData>();

            foreach (var action in Data.ActionDataList)
            {
                // Skip all conditional types, they are handled elsewhere
                if (action.ConditionType != ConditionType.None)
                {
                    continue;
                }
                // Skip actions that can't be used due to not enough mana
                if (action.MPCost > MP)
                {
                    continue;
                }
                // If the action has been used already, check if it is on cooldown
                if (ActionCooldown.TryGetValue(action.Name, out int turnsRemainingOnCooldown))
                {
                    if (turnsRemainingOnCooldown > 0)
                    {
                        continue;
                    }
                }
                randomActions.Add(action);
            }

            randomActions.Randomize();
            ActionToPerform = randomActions.Count > 0 ? randomActions[0] : null;
        }

        public override bool DidAttackSuccessfully()
        {
            double chanceToHit = ActionToPerform.ChanceToHit;
            if (ActionToPerform.MPCost > 0)
            {
                MP -= ActionToPerform.MPCost;
                GameEvents.EnemyStateUpdated(this);
            }

            if (!ActionCooldown.ContainsKey(ActionToPerform.Name))
            {
                ActionCooldown.Add(ActionToPerform.Name, 0);
            }
            ActionCooldown[ActionToPerform.Name] += ActionToPerform.ConditionCooldownTurns;

            if (Random.Range(0.0f, 1.0f) < chanceToHit)
            {
                if (ActionToPerform.BuffTurns > 0)
                {
                    if (ActionToPerform.DamageMultiplier > 0.0)
                    {
                        ActiveBuffs.Add(new BuffEntity($"{ActionToPerform.Name}_atk", SkillType.ModifyAllAttack, ActionToPerform.DamageMultiplier, ActionToPerform.BuffTurns));
                    }
                    // TODO, the defense
                }
                return true;
            }
            return false;
        }

        public override bool DidUseSkillSuccessfully(CombatContext combatContext)
        {
            bool success = base.DidUseSkillSuccessfully(combatContext);
            if (!success)
            {
                GameEvents.EnemySkillFailed(combatContext);
            }
            return success;
        }

        public override void TakeDamage(CombatEntity source, CombatContext combatContext)
        {
            base.TakeDamage(source, combatContext);
            GameEvents.EnemyDamaged(combatContext);
        }

        public override void Heal(CombatContext combatContext)
        {
            base.Heal(combatContext);
            GameEvents.EnemyHealed(combatContext);
        }

        public override void RestoreMP(CombatContext combatContext)
        {
            base.RestoreMP(combatContext);
            GameEvents.EnemyRestoreMP(combatContext);
        }

        public override void StealMP(CombatEntity target, CombatContext combatContext)
        {
            base.StealMP(target, combatContext);
            GameEvents.EnemyStealMP(combatContext);
        }


        public override void UseSkill(CombatContext combatContext)
        {
            base.UseSkill(combatContext);
            GameEvents.EnemyStateUpdated(this);
        }
    }
}