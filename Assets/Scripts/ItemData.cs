using Arena.Combat;
using System;

namespace Arena.Items
{
    public enum SlotType
    {
        None,
        OneHand,
        TwoHand,
        OffHand,
        Head,
        Body,
        Cape
    }

    public enum ItemType
    {
        None,
        Dagger,
        Sword,
        Axe,
        Mace,
        Wand,
        Staff,
        Bow,
        Arrow,
        Shield,
        Armor,
        Consumable,
        Material
    }

    public enum ActionType
    {
        None,
        HealHP,
        RestoreMP,
        Escape,
        DamageHP,
        StealMP,
        UseSkill,
        LearnSkill,
    }

    public class ItemData
    {
        public string Name;
        public string SlotTypeName = "None";
        public SlotType SlotType; // Parsed at runtime, not from data
        public string ItemTypeName = "None";
        public ItemType ItemType; // Parsed at runtime, not from data
        public int Attack = 0;
        public int MAttack = 0;
        public int Defense = 0;
        public int MDefense = 0;
        public int Speed = 0;
        public int BuyCount = 1;
        public int Cost = 0;
        public int Level = 1;
        public string ActionTypeName = "None";
        public ActionType ActionType;
        public string ActionValue;
        public SkillData ActionSkillData; // Parsed at runtime, not from data
        public string Description;
        public bool ExcludeFromLoot = false;
        public bool ShopItem = false;

        public int GetSellCost()
        {
            return ItemType == ItemType.Material ? Cost : Math.Max(1, Cost / 2);
        }

        public bool IsStackable()
        {
            return ItemType == ItemType.Consumable || ItemType == ItemType.Material || ItemType == ItemType.Arrow;
        }
    }
}