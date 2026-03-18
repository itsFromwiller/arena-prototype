using Newtonsoft.Json;
using System;

namespace Arena.Items
{
    public class ItemDataSlot
    {
        public string Name;
        public int Count;
        public string Rarity;
        public string Random;
        public int ItemID;

        [JsonIgnore]
        public ItemData ItemData;
        [JsonIgnore]
        public ItemModifierData RarityModifierData;
        [JsonIgnore]
        public ItemModifierData RandomModifierData;
        [JsonIgnore]
        public string CachedName;
        [JsonIgnore]
        public string CachedDescription;

        public ItemDataSlot()
        {
        }

        public ItemDataSlot(ItemData itemData, int count, ItemModifierData rarityData, ItemModifierData randomData, int itemID)
        {
            Name = itemData.Name;
            Count = count;
            ItemData = itemData;
            ItemID = itemID;
            if (rarityData != null)
            {
                Rarity = rarityData.Name;
                RarityModifierData = rarityData;
            }
            if (randomData != null)
            {
                Random = randomData.Name;
                RandomModifierData = randomData;
            }
        }

        public double GetCostMultiplier()
        {
            double costMultiplier = RarityModifierData != null ? RarityModifierData.CostMultiplier : 1.0;
            costMultiplier *= RandomModifierData != null ? RandomModifierData.CostMultiplier : 1.0;
            return costMultiplier;
        }

        public int GetBuyCost()
        {
            return (int)(ItemData.Cost * GetCostMultiplier());
        }

        public int GetSellCost(double shopSalePercentage)
        {
            return (int) (ItemData.GetSellCost(shopSalePercentage) * GetCostMultiplier());
        }

        public int GetCalculatedAttack()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.AttackMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.AttackMultiplier : 1.0;
            int stat = RandomModifierData != null ? Math.Max(1, ItemData.Attack) : ItemData.Attack;
            return (int)(stat * statMultiplier);
        }

        public int GetCalculatedDefense()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.DefenseMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.DefenseMultiplier : 1.0;
            int stat = RandomModifierData != null ? Math.Max(1, ItemData.Defense) : ItemData.Defense;
            return (int)(stat * statMultiplier);
        }

        public int GetCalculatedMAttack()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.MAttackMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.MAttackMultiplier : 1.0;
            int stat = RandomModifierData != null ? Math.Max(1, ItemData.MAttack) : ItemData.MAttack;
            return (int)(stat * statMultiplier);
        }

        public int GetCalculatedMDefense()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.MDefenseMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.MDefenseMultiplier : 1.0;
            int stat = RandomModifierData != null ? Math.Max(1, ItemData.MDefense) : ItemData.MDefense;
            return (int)(stat * statMultiplier);
        }

        public int GetCalculatedSpeed()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.SpeedMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.SpeedMultiplier : 1.0;
            int stat = RandomModifierData != null ? Math.Max(1, ItemData.Speed) : ItemData.Speed;
            return (int)(stat * statMultiplier);
        }

    }
}