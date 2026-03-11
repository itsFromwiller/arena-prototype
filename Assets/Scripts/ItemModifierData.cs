using Arena.Combat;
using System;

namespace Arena.Items
{
    public enum ModifierType
    {
        Rarity,
        Random,
    }

    public class ItemModifierData
    {
        public string Name;
        public string Type = "None";
        public ModifierType ModifierType; // Parsed at runtime, not from data
        public string TextColor = "white";
        public string Prefix;
        public string Postfix;
        public int Weight;
        public double AttackMultiplier = 1.0;
        public double MAttackMultiplier = 1.0;
        public double DefenseMultiplier = 1.0;
        public double MDefenseMultiplier = 1.0;
        public double SpeedMultiplier = 1.0;
        public double CostMultiplier = 1.0;
    }
}