using System.Collections;
using UnityEngine;

namespace Arena.Combat
{
    public enum SkillType
    {
        DealDamage,
        DealMDamage,
        EscapeDungeon,
        Heal,
        StealMP,
        ModifyAttack,
        ModifyMAttack,
        ModifyDefense,
        ModifyMDefense,
        ModifySpeed,
        ModifyAllAttack,
        ModifyAllDefense,
        ModifyMaxHP,
        ModifyMaxMP
    }

    public enum TargetType
    {
        Self,
        Enemy
    }

    public enum ActivationType
    {
        Use,
        Always
    }

    public enum ElementType
    {
        None,
        Fire,
        Water,
        Wind,
        Earth,
        Lightning,
        Poison
    }

    public class SkillData
    {
        public string Name;
        public string SkillTypeName;
        public SkillType SkillType; // Parsed at runtime, not from data
        public string TargetTypeName;
        public TargetType TargetType; // Parsed at runtime, not from data
        public string ActivationTypeName;
        public ActivationType ActivationType; // Parsed at runtime, not from data
        public string ElementTypeName;
        public ElementType ElementType; // Parsed at runtime, not from data
        public int MPCost;
        public int SkillValue;
        public double SkillPercentage;
        public double SuccessRate;
        public int RepeatTurns;
        public string Description;
        public string CombatText;
        public string RepeatText;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}