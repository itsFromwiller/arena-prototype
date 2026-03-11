using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using Arena.Core;
using Arena.Combat;
using System.Text;
using Arena.Loot;
using System;

namespace Arena.Items
{
    public class ItemSystem : MonoBehaviour
    {
        public static ItemSystem Instance;

        private Dictionary<string, ItemData> itemDatabase = new();
        public Dictionary<string, ItemData> ItemDatabase { get { return itemDatabase; } }
        private Dictionary<string, ItemModifierData> modifierDatabase = new();
        public Dictionary<string, ItemModifierData> ModifierDatabase { get { return modifierDatabase;  } }
        private List<ItemData> ShopItems = new();

        private void Awake()
        {
            Instance = this;
        }

        public void SetData(Dictionary<string, string> data)
        {
            var weaponData = JsonConvert.DeserializeObject<List<ItemData>>(data["Weapons"]);
            var armorData = JsonConvert.DeserializeObject<List<ItemData>>(data["Armor"]);
            var itemData = JsonConvert.DeserializeObject<List<ItemData>>(data["Items"]);
            var modifierData = JsonConvert.DeserializeObject<List<ItemModifierData>>(data["ItemModifiers"]);

            foreach (var dataItem in weaponData)
            {
                if (!itemDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Weapon data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
                dataItem.ItemType = EnumMap<ItemType>.GetValue(dataItem.ItemTypeName);
                dataItem.SlotType = EnumMap<SlotType>.GetValue(dataItem.SlotTypeName);
                if (dataItem.ShopItem)
                {
                    ShopItems.Add(dataItem);
                }
            }
            foreach (var dataItem in armorData)
            {
                if (!itemDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Armor data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
                dataItem.ItemType = EnumMap<ItemType>.GetValue(dataItem.ItemTypeName);
                dataItem.SlotType = EnumMap<SlotType>.GetValue(dataItem.SlotTypeName);
                if (dataItem.ShopItem)
                {
                    ShopItems.Add(dataItem);
                }
            }
            foreach (var dataItem in itemData)
            {
                if (!itemDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Item data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
                dataItem.ItemType = EnumMap<ItemType>.GetValue(dataItem.ItemTypeName);
                dataItem.SlotType = EnumMap<SlotType>.GetValue(dataItem.SlotTypeName);
                dataItem.ActionType = EnumMap<ActionType>.GetValue(dataItem.ActionTypeName);
                if (dataItem.ShopItem)
                {
                    ShopItems.Add(dataItem);
                }
            }

            foreach (var dataItem in modifierData)
            {
                if (!modifierDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Item modifier data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
                dataItem.ModifierType = EnumMap<ModifierType>.GetValue(dataItem.Type);
            }
        }

        public ItemModifierData GetRarityModifier(string rarityModifier)
        {
            if (string.IsNullOrEmpty(rarityModifier))
            {
                return null;
            }
            modifierDatabase.TryGetValue(rarityModifier, out var modifierData);
            return modifierData;
        }

        public ItemModifierData GetRandomModifier(string randomModifier)
        {
            if (string.IsNullOrEmpty(randomModifier))
            {
                return null;
            }
            modifierDatabase.TryGetValue(randomModifier, out var modifierData);
            return modifierData;
        }

        public ItemData GetItemData(string name)
        {
            if (itemDatabase.TryGetValue(name, out ItemData itemData))
            {
                return itemData;
            }
            Debug.LogError("Item not found: " + name);
            return null;
        }

        public string BuildName(ItemData itemData)
        {
            if (itemData.SlotType == SlotType.TwoHand)
            {
                return $"{itemData.Name} [2H]";
            }
            return itemData.Name;
        }

        public string BuildName(ItemDataSlot itemDataSlot)
        {
            if (itemDataSlot.CachedName == null)
            {
                StringBuilder sb = new StringBuilder();
                if (itemDataSlot.RarityModifierData != null)
                {
                    // Color change
                    sb.Append($"<color={itemDataSlot.RarityModifierData.TextColor}>");
                }
                sb.Append(itemDataSlot.ItemData.Name);
                if (itemDataSlot.RandomModifierData != null)
                {
                    sb.Append(" ").Append(itemDataSlot.RandomModifierData.Postfix);
                }
                if (itemDataSlot.ItemData.SlotType == SlotType.TwoHand)
                {
                    sb.Append(" [2H]");
                }
                if (itemDataSlot.RarityModifierData != null)
                {
                    // Color change
                    sb.Append($"</color>");
                }
                itemDataSlot.CachedName = sb.ToString();
            }
            return itemDataSlot.CachedName;
        }

        public string BuildDescription(ItemDataSlot itemDataSlot)
        {
            if (itemDataSlot.CachedDescription == null)
            {
                ItemData itemData = itemDataSlot.ItemData;
                if (itemData.ItemType == ItemType.Consumable)
                {
                    itemDataSlot.CachedDescription = string.Format(itemData.Description, itemData.ActionValue);
                }
                else if (itemData.ItemType == ItemType.Material)
                {
                    itemDataSlot.CachedDescription = string.Empty;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (itemData.Attack > 0)
                    {
                        sb.Append($"<color=#FF0000>[Atk +{itemDataSlot.GetCalculatedAttack()}]</color> ");
                    }
                    if (itemData.MAttack > 0)
                    {
                        sb.Append($"<color=#0000FF>[MAtk +{itemDataSlot.GetCalculatedMAttack()}]</color> ");
                    }
                    if (itemData.Defense > 0)
                    {
                        sb.Append($"<color=#FFFF00>[Def +{itemDataSlot.GetCalculatedDefense()}]</color> ");
                    }
                    if (itemData.MDefense > 0)
                    {
                        sb.Append($"<color=#FF00FF>[MDef +{itemDataSlot.GetCalculatedMDefense()}]</color> ");
                    }
                    if (itemData.Speed > 0)
                    {
                        sb.Append($"<color=#00FF00>[Spd +{itemDataSlot.GetCalculatedSpeed()}]</color> ");
                    }
                    itemDataSlot.CachedDescription = sb.ToString();
                }
            }
            return itemDataSlot.CachedDescription;
        }

        public void Init()
        {
            foreach (var itemData in itemDatabase.Values)
            {
                if (itemData.ActionType == ActionType.UseSkill || 
                    itemData.ActionType == ActionType.LearnSkill)
                {
                    SkillData skillData = SkillSystem.Instance.GetSkillData(itemData.ActionValue);
                    if (skillData == null)
                    {
                        Debug.LogError($"{itemData.Name} couldn't find Skill data named: {itemData.ActionValue}");
                        continue;
                    }
                    itemData.ActionSkillData = skillData;
                }
            }
        }

        public List<ItemDataSlot> GetShopItemsFiltered(HashSet<ItemType> filteredTypes, bool onlyMagicConsumables, bool onlyNonMagicConsumables, int maxLevel)
        {
            List<ItemDataSlot> items = new List<ItemDataSlot>();
            foreach (var item in ShopItems)
            {
                if (item.Level > maxLevel)
                {
                    continue;
                }
                if (filteredTypes == null || filteredTypes.Count == 0 || filteredTypes.Contains(item.ItemType))
                {
                    if (item.ItemType == ItemType.Consumable)
                    {
                        if (!onlyMagicConsumables && !onlyNonMagicConsumables)
                        {
                            items.Add(new ItemDataSlot(item, item.BuyCount, null, null, 0));
                        }
                        else if (onlyMagicConsumables && (item.ActionType == ActionType.UseSkill || item.ActionType == ActionType.LearnSkill))
                        {
                            items.Add(new ItemDataSlot(item, item.BuyCount, null, null, 0));
                        }
                        else if (onlyNonMagicConsumables && !(item.ActionType == ActionType.UseSkill || item.ActionType == ActionType.LearnSkill))
                        {
                            items.Add(new ItemDataSlot(item, item.BuyCount, null, null, 0));
                        }
                    }
                    else
                    {
                        items.Add(new ItemDataSlot(item, item.BuyCount, null, null, 0));
                    }
                }
            }
            items.Sort((a, b) =>
            {
                int priceComparison = a.ItemData.Cost * a.ItemData.BuyCount - b.ItemData.Cost * b.ItemData.BuyCount;
                if (priceComparison == 0)
                {
                    return string.Compare(a.Name, b.Name);
                }
                return priceComparison;
            });
            return items;
        }

        public List<ItemDataSlot> GetBazaarShopItems(int countToGenerate, int maxLevel)
        {
            List<ItemDataSlot> items = new List<ItemDataSlot>();
            var uncommonRarity = GetRarityModifier("Uncommon");
            List<ItemData> possibleItems = new List<ItemData>();
            foreach (var item in ShopItems)
            {
                if (item.Level > maxLevel)
                {
                    continue;
                }
                if (item.ItemType == ItemType.Consumable || item.ItemType == ItemType.Material || item.ItemType == ItemType.Arrow)
                {
                    continue;
                }
                possibleItems.Add(item);
            }

            int pickCount = Math.Min(countToGenerate, possibleItems.Count);
            for (int i = 0; i < pickCount; ++i)
            {
                int index = UnityEngine.Random.Range(0, possibleItems.Count);
                var pickedItem = possibleItems[index];
                possibleItems.RemoveAt(index);
                items.Add(new ItemDataSlot(pickedItem, pickedItem.BuyCount, uncommonRarity, LootSystem.Instance.GetRandomItemModifier(), 0));
            }

            items.Sort((a, b) =>
            {
                int priceComparison = a.ItemData.Cost * a.ItemData.BuyCount - b.ItemData.Cost * b.ItemData.BuyCount;
                if (priceComparison == 0)
                {
                    return string.Compare(a.Name, b.Name);
                }
                return priceComparison;
            });
            return items;
        }    
    }
}