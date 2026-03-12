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
            var keys = new List<string>(ActionCooldown.Keys);
            foreach (var key in keys)
            {
                if (ActionCooldown[key] > 0)
                {
                    ActionCooldown[key]--;
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
                    Debug.LogError($"Expired {activeBuff.BuffName}");
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
                // Conditional actions are handled elsewhere
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
                if (IsActionOnCooldown(action.Name))
                {
                    continue;
                }
                randomActions.Add(action);
            }

            randomActions.Randomize();
            ActionToPerform = randomActions.Count > 0 ? randomActions[0] : null;
        }

        public bool IsActionOnCooldown(string name)
        {
            if (ActionCooldown.TryGetValue(name, out int turnsRemainingOnCooldown))
            {
                if (turnsRemainingOnCooldown > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool DidAttackSuccessfully()
        {
            double chanceToHit = ActionToPerform.ChanceToHit;
            if (ActionToPerform.MPCost > 0)
            {
                MP -= ActionToPerform.MPCost;
                GameEvents.EnemyStateUpdated(this);
            }

            StartActionCooldown(ActionToPerform);

            if (Random.Range(0.0f, 1.0f) < chanceToHit)
            {
                return true;
            }
            return false;
        }

        public void StartActionCooldown(EnemyActionData enemyAction)
        {
            if (!ActionCooldown.ContainsKey(enemyAction.Name))
            {
                ActionCooldown.Add(enemyAction.Name, 0);
            }
            ActionCooldown[enemyAction.Name] += enemyAction.CooldownTurns;
            Debug.LogError($"{enemyAction.Name} cooldown set to {ActionCooldown[enemyAction.Name]}");
        }

        public void HandleBuffsForEnemyAction(EnemyActionData enemyAction)
        {
            if (enemyAction.DamageMultiplier > 0.0)
            {
                ActiveBuffs.Add(new BuffEntity($"{enemyAction.Name}_atk", SkillType.ModifyAllAttack, enemyAction.DamageMultiplier, enemyAction.BuffTurns));
                Debug.LogError($"Added {ActiveBuffs[ActiveBuffs.Count - 1].BuffName}");
            }
            if (enemyAction.DefenseMultiplier > 0.0)
            {
                ActiveBuffs.Add(new BuffEntity($"{enemyAction.Name}_def", SkillType.ModifyAllDefense, enemyAction.DefenseMultiplier, enemyAction.BuffTurns));
                Debug.LogError($"Added {ActiveBuffs[ActiveBuffs.Count - 1].BuffName}");
            }

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