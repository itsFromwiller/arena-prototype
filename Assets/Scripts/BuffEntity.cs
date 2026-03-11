using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace Arena.Combat
{
    public enum BuffLifetime
    {
        Turn,
        Battle,
        Floors,
        Dungeon
    }

    public class BuffEntity
    {
        public string BuffName;
        public SkillType BuffSkillType;
        public double BuffValue;
        public int LifetimeValue = 0;

        public bool IsExpired { get; private set; }

        public BuffEntity()
        {

        }

        public BuffEntity(string name, SkillType buffType, double buffValue, int lifetimeValue)
        {
            BuffName = name;
            BuffSkillType = buffType;
            BuffValue = buffValue;
            LifetimeValue = lifetimeValue;
            IsExpired = false;
        }

        public void TurnEnded()
        {
            if (IsExpired)
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