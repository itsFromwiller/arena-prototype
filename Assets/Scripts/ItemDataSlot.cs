using Newtonsoft.Json;

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

        public int GetSellCost()
        {
            return (int) (ItemData.GetSellCost() * GetCostMultiplier());
        }

        public int GetCalculatedAttack()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.AttackMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.AttackMultiplier : 1.0;
            return (int)(ItemData.Attack * statMultiplier);
        }

        public int GetCalculatedDefense()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.DefenseMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.DefenseMultiplier : 1.0;
            return (int)(ItemData.Defense * statMultiplier);
        }

        public int GetCalculatedMAttack()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.MAttackMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.MAttackMultiplier : 1.0;
            return (int)(ItemData.MAttack * statMultiplier);
        }

        public int GetCalculatedMDefense()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.MDefenseMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.MDefenseMultiplier : 1.0;
            return (int)(ItemData.MDefense * statMultiplier);
        }

        public int GetCalculatedSpeed()
        {
            double statMultiplier = RarityModifierData != null ? RarityModifierData.SpeedMultiplier : 1.0;
            statMultiplier *= RandomModifierData != null ? RandomModifierData.SpeedMultiplier : 1.0;
            return (int)(ItemData.Speed * statMultiplier);
        }

    }
}