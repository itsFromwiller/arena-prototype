// Uncomment to turn on debug lines
//#define DEBUG_LOGS

using Arena.Assets.Scripts.Core;
using Arena.Core;
using Arena.Items;
using Arena.Player;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Loot
{
    public class LootSystem : MonoBehaviour
    {
        public static LootSystem Instance;

        private Dictionary<string, List<LootTableData>> lootDatabase = new Dictionary<string, List<LootTableData>>();
        private List<ItemData> randomWeaponLoot = new List<ItemData>();
        private List<ItemData> randomArmorLoot = new List<ItemData>();
        private WeightedBucket<ItemModifierData> rarityBucket = new WeightedBucket<ItemModifierData>();
        private WeightedBucket<ItemModifierData> randomBucket = new WeightedBucket<ItemModifierData>();

        private void Awake()
        {
            Instance = this;
        }

        public void SetData(Dictionary<string, string> data)
        {
            var lootData = JsonConvert.DeserializeObject<List<LootTableData>>(data["LootTables"]);
            foreach (var dataItem in lootData)
            {
                if (!lootDatabase.ContainsKey(dataItem.Name))
                {
                    lootDatabase.Add(dataItem.Name, new List<LootTableData>());
                }
                lootDatabase[dataItem.Name].Add(dataItem);
            }
        }

        public void Init()
        {
            foreach (var dataItemList in lootDatabase.Values)
            {
                foreach (var dataItem in dataItemList)
                {
                    if (!string.IsNullOrEmpty(dataItem.Special))
                    {
                        dataItem.LootType = EnumMap<LootType>.GetValue(dataItem.Special);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(dataItem.Item))
                        {
                            dataItem.LootType = LootType.Item;
                            dataItem.ItemData = ItemSystem.Instance.GetItemData(dataItem.Item);
                        }
                        else if (!string.IsNullOrEmpty(dataItem.Weapon))
                        {
                            dataItem.LootType = LootType.Weapon;
                            dataItem.ItemData = ItemSystem.Instance.GetItemData(dataItem.Weapon);
                        }
                        else if (!string.IsNullOrEmpty(dataItem.Armor))
                        {
                            dataItem.LootType = LootType.Armor;
                            dataItem.ItemData = ItemSystem.Instance.GetItemData(dataItem.Armor);
                        }
                    }
                }
            }

            var itemDatabase = ItemSystem.Instance.ItemDatabase;
            foreach (var itemData in itemDatabase.Values)
            {
                if (itemData.ExcludeFromLoot)
                {
                    continue;
                }
                if (itemData.ItemType == ItemType.Armor || itemData.ItemType == ItemType.Shield)
                {
                    randomArmorLoot.Add(itemData);
                }
                else if (itemData.ItemType == ItemType.Consumable || itemData.ItemType == ItemType.Material)
                {
                    continue;
                }
                randomWeaponLoot.Add(itemData);
            }

            var modifiersDatabase = ItemSystem.Instance.ModifierDatabase;
            foreach (var modifier in modifiersDatabase.Values)
            {
                if (modifier.ModifierType == ModifierType.Rarity)
                {
                    rarityBucket.AddItem(modifier, modifier.Weight);
                }
                else if (modifier.ModifierType == ModifierType.Random)
                {
                    randomBucket.AddItem(modifier, modifier.Weight);
                }
            }
        }

        public List<LootTableData> GetLootTables(string lootTableName)
        {
            if (lootDatabase.TryGetValue(lootTableName, out var lootTableDataList))
            {
                return lootTableDataList;
            }
            return new List<LootTableData>();
        }

        public List<LootResult> RollLoot(List<LootTableData> lootTable, int maxLevel, bool guaranteedPull)
        {
            List<LootResult> results = new List<LootResult>();
            Dictionary<string, int> dropCount = new Dictionary<string, int>();

            foreach (var lootTableData in lootTable)
            {
                ItemData possibleReward = null;
                switch (lootTableData.LootType)
                {
                    case LootType.Item:
                    case LootType.Weapon:
                    case LootType.Armor:
                    {
                        if (lootTableData.ItemData.Level > maxLevel)
                        {
                            continue;
                        }
                        if (lootTableData.LimitInInventory > 0)
                        {
                            if (PlayerSystem.Instance.Player.GetItemCount(lootTableData.ItemData.Name) >= lootTableData.LimitInInventory)
                            {
                                continue;
                            }
                        }
                        if (lootTableData.LimitPerDrop > 0)
                        {
                            if (dropCount.TryGetValue(lootTableData.ItemData.Name, out int countDropped))
                            {
                                if (countDropped >= lootTableData.LimitPerDrop)
                                {
                                    continue;
                                }
                            }
                        }
                        possibleReward = lootTableData.ItemData;
                        break;
                    }
                    case LootType.RandomWeapon:
                    {
                        List<ItemData> possibleRandomRewards = new List<ItemData>();
                        foreach (var itemData in randomWeaponLoot)
                        {
                            if (itemData.Level >= maxLevel)
                            {
                                continue;
                            }
                            possibleRandomRewards.Add(itemData);
                        }
                        if (possibleRandomRewards.Count > 0)
                        {
                            possibleReward = possibleRandomRewards[Random.Range(0, possibleRandomRewards.Count)];
                        }
                        break;
                    }
                    case LootType.RandomArmor:
                    {
                        List<ItemData> possibleRandomRewards = new List<ItemData>();
                        foreach (var itemData in randomArmorLoot)
                        {
                            if (itemData.Level >= maxLevel)
                            {
                                continue;
                            }
                            possibleRandomRewards.Add(itemData);
                        }
                        if (possibleRandomRewards.Count > 0)
                        {
                            possibleReward = possibleRandomRewards[Random.Range(0, possibleRandomRewards.Count)];
                        }
                        break;
                    }
                    case LootType.GuaranteedLootTable:
                    {
                        // We grab all the loot options in the loot table specified
                        // and perform a weighted bucket pull. However, we assign a
                        // weight per loot table option based upon its odds, so rare
                        // items still stay rare.
                        var guaranteedlootTable = GetLootTables(lootTableData.SpecialValue);
                        var lootBucket = new WeightedBucket<LootTableData>();
                        foreach(var loot in guaranteedlootTable)
                        {
                            lootBucket.AddItem(loot, (int) (loot.Odds * 100));
                        }
                        var lootTableResult = lootBucket.GetRandomItem();
                        var lootResult = RollLoot(new List<LootTableData>() { lootTableResult }, maxLevel, true);
                        results.AddRange(lootResult);
                        continue;
                    }
                    case LootType.Gold:
                    {
                        double roll = Random.Range(0.0f, 1.0f);
                        if (roll <= lootTableData.Odds || guaranteedPull)
                        {
                            if (int.TryParse(lootTableData.SpecialValue, out int gold))
                            {
                                // Gold can be 10% less or more than the value earned,
                                // to make it look better.
                                int goldMin = (int)(gold * 0.9);
                                int extraGold = (gold - goldMin) * 2;
                                int randomGold = Random.Range(0, extraGold) + 1;
                                gold = goldMin + randomGold;

                                var lootResult = new LootResult();
                                lootResult.Gold = gold;
                                results.Add(lootResult);
                            }
                        }
                        continue;
                    }
                }
#if DEBUG_LOGS
                Debug.LogError($"LootSystem: Possible Reward = {possibleReward.Name}");
#endif
                // Roll to see if we get it
                if (possibleReward != null)
                {
                    double roll = Random.Range(0.0f, 1.0f);
#if DEBUG_LOGS
                    Debug.LogError($"LootSystem: Rolled a {roll} and it needs to be less than {lootTableData.Odds}");
#endif
                    if (roll <= lootTableData.Odds || guaranteedPull)
                    {
#if DEBUG_LOGS
                        Debug.LogError($"LootSystem: Got it!");
#endif
                        ItemModifierData rarity = null;
                        ItemModifierData random = null;
                        if (!(possibleReward.ItemType == ItemType.Consumable ||
                            possibleReward.ItemType == ItemType.Material))
                        {
                            rarity = rarityBucket.GetRandomItem();
                            if (!(rarity.Name == "Common"))
                            {
                                random = randomBucket.GetRandomItem();
                            }
                        }
                        else
                        {
                            rarity = ItemSystem.Instance.GetRarityModifier("Common");
                        }
                        results.Add(new LootResult()
                        {
                            ItemDataSlot = new ItemDataSlot(possibleReward, possibleReward.BuyCount, rarity, random, 0),
                            Count = possibleReward.BuyCount,
                        });
                    }
                }
            }
            return results;
        }

        public ItemModifierData GetRandomItemModifier()
        {
            return randomBucket.GetRandomItem();
        }
    }
}
