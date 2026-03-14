using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace Arena.Combat
{
    public enum SkillLifetime
    {
        Turn,
        Battle,
        Floors,
        Dungeon
    }

    public class SkillEntity
    {
        [JsonIgnore]
        public SkillData SkillData { get; set; }
        
        public string SkillName;
        public int LifetimeValue = 0;
        public SkillLifetime SkillLifetime = SkillLifetime.Turn;
        public bool SourceIsPlayer = false;

        public bool IsExpired { get; private set; }

        public SkillEntity()
        {

        }

        public SkillEntity(SkillData skillData, SkillLifetime skillLifetime, int lifetimeValue, bool sourceIsPlayer)
        {
            SkillName = skillData.Name;
            SkillData = skillData;
            SkillLifetime = skillLifetime;
            LifetimeValue = lifetimeValue;
            IsExpired = false;
            SourceIsPlayer = sourceIsPlayer;
        }

        public void TurnEnded()
        {
            if (IsExpired || SkillLifetime != SkillLifetime.Turn)
            {
                return;
            }
            DecrementLifetimeValue();
        }

        public void BattleEnded()
        {
            if (IsExpired || SkillLifetime != SkillLifetime.Battle)
            {
                return;
            }
            DecrementLifetimeValue();
        }

        public void FloorEnded()
        {
            if (IsExpired || SkillLifetime != SkillLifetime.Floors)
            {
                return;
            }
            DecrementLifetimeValue();
        }

        public void DungeonEnded()
        {
            if (IsExpired || SkillLifetime != SkillLifetime.Dungeon)
            {
                return;
            }
            DecrementLifetimeValue();
        }

        public void DecrementLifetimeValue()
        {
            if (LifetimeValue > 0)
            {
                LifetimeValue--;
                if (LifetimeValue == 0)
                {
                    IsExpired = true;
                }
            }
        }
    }
}