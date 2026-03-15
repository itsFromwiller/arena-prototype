using Arena.Items;
using System.Collections;
using UnityEngine;

namespace Arena.Loot
{
    public enum LootType
    {
        Item,
        Weapon,
        Armor,
        RandomWeapon,
        RandomArmor,
        GuaranteedLootTable,
        Gold,
    }

    public class LootTableData
    {
        public string Name;
        public string Item;
        public string Weapon;
        public string Armor;
        public string Special;
        public string SpecialValue;
        public LootType LootType; // Parsed at runtime, not from data
        public double Odds;
        public int LimitPerDrop;
        public int LimitInInventory;
        public ItemData ItemData; // Parsed at runtime, not from data
    }
}