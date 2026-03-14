using Arena.Combat;
using Arena.Items;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Player
{
    public class PlayerEntity: CombatEntity
    {
        public string Class;
        public int XP;
        public int Gold;
        public int Strength;
        public int Intelligence;
        public int Endurance;
        public int Agility;
        public int StatPointsRemaining;
        public int LastItemID = 0;
        public List<string> DungeonsUnlocked = new();

        public override int MaxHP
        {
            get
            {
                double result = Endurance * 2;
                ModifyStatViaActiveSkills(ref result, SkillType.ModifyMaxHP);
                return (int)result;
            }
        }
        public override int MaxMP
        {
            get
            {
                double result = Intelligence * 2;
                ModifyStatViaActiveSkills(ref result, SkillType.ModifyMaxMP);
                return (int)result;
            }
        }


        public List<SkillDataSlot> SkillSlots = new List<SkillDataSlot>();
        public List<ItemDataSlot> ItemSlots = new List<ItemDataSlot>();
        public Dictionary<SlotType, ItemDataSlot> EquippedItems = new Dictionary<SlotType, ItemDataSlot>();

        public PlayerEntity(string name)
        {
            // Player Entity
            Class = "Warrior";
            XP = 0;
            Gold = 500;
            Strength = 10;
            Endurance = 10;
            Agility = 10;
            Intelligence = 10;

            // Combat Entity
            Name = name;
            HP = MaxHP;
            MP = MaxMP;
            Level = 1;
            Attack = 0;
            MAttack = 0;
            Defense = 0;
            MDefense = 0;
            Speed = 0;

            GainItem("Wooden Helmet", 1, "Legendary", "Shark");
            int helmetID = LastItemID;
            GainItem("Bone Axe", 1, "Uncommon", null);
            int weaponID = LastItemID;
            GainItem("Bone Shield", 1, "Rare", "Monkey");
            int shieldID = LastItemID;
            GainItem("Leaf Cape", 1, "Epic", null);
            int capeID = LastItemID;
            EquipItem("Wooden Helmet", helmetID);
            EquipItem("Bone Axe", weaponID);
            EquipItem("Bone Shield", shieldID);
            EquipItem("Leaf Cape", capeID);
            DungeonsUnlocked.Add("Sewer");
        }

        public override void Init()
        {
            base.Init();
            foreach (var skillSlot in SkillSlots)
            {
                skillSlot.SkillData = SkillSystem.Instance.GetSkillData(skillSlot.Name);
            }
            foreach (var itemSlot in ItemSlots)
            {
                itemSlot.ItemData = ItemSystem.Instance.GetItemData(itemSlot.Name);
                itemSlot.RarityModifierData = ItemSystem.Instance.GetRarityModifier(itemSlot.Rarity);
                itemSlot.RandomModifierData = ItemSystem.Instance.GetRandomModifier(itemSlot.Random);
            }
            foreach (var itemDataSlot in EquippedItems.Values)
            {
                itemDataSlot.ItemData = ItemSystem.Instance.GetItemData(itemDataSlot.Name);
                itemDataSlot.RarityModifierData = ItemSystem.Instance.GetRarityModifier(itemDataSlot.Rarity);
                itemDataSlot.RandomModifierData = ItemSystem.Instance.GetRandomModifier(itemDataSlot.Random);
            }
            DungeonsUnlocked.Clear();
        }

        public bool HasUnlockedDungeon(string dungeonName)
        {
            return DungeonsUnlocked.Contains(dungeonName);
        }

        public void UnlockDungeon(string dungeonName)
        {
            if (!HasUnlockedDungeon(dungeonName))
            {
                DungeonsUnlocked.Add(dungeonName);
            }
        }

        public void LearnSkill(string skillName)
        {
            foreach (var skillSlot in SkillSlots)
            {
                if (skillSlot.Name == skillName)
                {
                    skillSlot.IsLearned = true;
                    return;
                }
            }
            SkillData skillData = SkillSystem.Instance.GetSkillData(skillName);
            var skillDataSlot = new SkillDataSlot(skillData, true);
            SkillSlots.Add(skillDataSlot);
        }

        public void GetTempSkill(string skillName)
        {
            // If we already have it, ignore it. Items could give skills
            // we already know
            foreach (var skillSlot in SkillSlots)
            {
                if (skillSlot.Name == skillName)
                {
                    return;
                }
            }
            SkillData skillData = SkillSystem.Instance.GetSkillData(skillName);
            var skillDataSlot = new SkillDataSlot(skillData, false);
            SkillSlots.Add(skillDataSlot);
        }

        public void RemoveTempSkill(string skillName)
        {
            // Only remove it if it isn't learned
            int removeIndex = -1;
            for (int i = 0; i < SkillSlots.Count; ++i)
            {
                if (SkillSlots[i].Name == skillName)
                {
                    if (!SkillSlots[i].IsLearned)
                    {
                        removeIndex = i;
                        break;
                    }
                    return;
                }
            }
            if (removeIndex != -1)
            {
                SkillSlots.RemoveAt(removeIndex);
            }
        }

        public List<SkillDataSlot> GetCurrentSkills()
        {
            List<SkillDataSlot> skills = new List<SkillDataSlot>();
            foreach (var skill in SkillSlots)
            {
                skills.Add(skill);
            }
            skills.Sort((a, b) => a.Name.CompareTo(b.Name));
            return skills;
        }

        public List<ItemDataSlot> GetCurrentItems(bool usableOnly, bool includeLearnSkill)
        {
            List<ItemDataSlot> items = new List<ItemDataSlot>();
            foreach (var item in ItemSlots)
            {
                if (!usableOnly ||
                    (usableOnly && item.ItemData.ItemType == ItemType.Consumable))
                {
                    if (includeLearnSkill ||
                        !includeLearnSkill && item.ItemData.ActionType != ActionType.LearnSkill)
                    {
                        items.Add(item);
                    }
                }
            }
            items.Sort((a, b) =>
            {
                int nameComparison = string.Compare(a.Name, b.Name);
                if (nameComparison == 0)
                {
                    return a.Count.CompareTo(b.Count);
                }
                return nameComparison;
            });
            return items;
        }

        public List<ItemDataSlot> GetCurrentItemsFiltered(HashSet<ItemType> filteredTypes, bool onlyMagicConsumables, bool onlyNonMagicConsumables)
        {
            List<ItemDataSlot> items = new List<ItemDataSlot>();
            foreach (var item in ItemSlots)
            {
                if (filteredTypes == null || filteredTypes.Count == 0 || filteredTypes.Contains(item.ItemData.ItemType))
                {
                    if (item.ItemData.ItemType == ItemType.Consumable)
                    {
                        if (!onlyMagicConsumables && !onlyNonMagicConsumables)
                        {
                            items.Add(item);
                        }
                        else if (onlyMagicConsumables && (item.ItemData.ActionType == ActionType.UseSkill || item.ItemData.ActionType == ActionType.LearnSkill))
                        {
                            items.Add(item);
                        }
                        else if (onlyNonMagicConsumables && !(item.ItemData.ActionType == ActionType.UseSkill || item.ItemData.ActionType == ActionType.LearnSkill))
                        {
                            items.Add(item);
                        }
                    }
                    else
                    {
                        items.Add(item);
                    }
                }
            }
            items.Sort((a, b) =>
            {
                int nameComparison = string.Compare(a.Name, b.Name);
                if (nameComparison == 0)
                {
                    return a.Count.CompareTo(b.Count);
                }
                return nameComparison;
            });
            return items;
        }

        public List<ItemDataSlot> GetEquipmentFiltered(HashSet<SlotType> filteredTypes)
        {
            List<ItemDataSlot> items = new List<ItemDataSlot>();
            foreach (var item in ItemSlots)
            {
                if (filteredTypes == null || filteredTypes.Count == 0 || filteredTypes.Contains(item.ItemData.SlotType))
                {
                    items.Add(new ItemDataSlot(item.ItemData, item.Count, item.RarityModifierData, item.RandomModifierData, item.ItemID));
                }
            }
            items.Sort((a, b) =>
            {
                int levelComparison = a.ItemData.Level - b.ItemData.Level;
                if (levelComparison == 0)
                {
                    return string.Compare(a.Name, b.Name);
                }
                return levelComparison;
            });
            return items;
        }

        public List<ItemDataSlot> GetStudyItems()
        {
            List<ItemDataSlot> items = new List<ItemDataSlot>();
            foreach (var item in ItemSlots)
            {
                if (item.ItemData.ActionType == ActionType.LearnSkill)
                {
                    items.Add(new ItemDataSlot(item.ItemData, item.Count, item.RarityModifierData, item.RandomModifierData, item.ItemID));
                }
            }
            items.Sort((a, b) =>
            {
                int levelComparison = a.ItemData.Level - b.ItemData.Level;
                if (levelComparison == 0)
                {
                    return string.Compare(a.Name, b.Name);
                }
                return levelComparison;
            });
            return items;
        }

        public override bool DidAttackSuccessfully()
        {
            int chanceToHit = 85;
            if (UnityEngine.Random.Range(0, 100) < chanceToHit)
            {
                return true;
            }
            return false;
        }

        public override bool DidUseSkillSuccessfully(CombatContext combatContext)
        {
            bool success = base.DidUseSkillSuccessfully(combatContext);
            if (!success)
            {
                GameEvents.PlayerSkillFailed(combatContext);
            }
            return success;
        }

        public void EarnXP(int amount)
        {
            XP += amount;
            GameEvents.GetXP(amount);

            while (true)
            {
                int requiredXPForNextLevel = GetMaxXPForLevel(Level);
                if (XP < requiredXPForNextLevel)
                {
                    break;
                }
                Level++;
                StatPointsRemaining += 5;
                GameEvents.PlayerLevelChanged(Level);
            }
        }

        public int GetMaxXPForLevel(int level)
        {
            return 10 * level * (1 + level);
        }

        public int GetLevelFromXP()
        {
            // XP needed for level is:
            // X = Level;
            // Y = XP;

            // Y = 25 * X * (1 + X)
            // Y = 25X * (1 + X)
            // Y = 25X + 25X^2
            // Y = 25X^2 + 25X

            // To get level from known XP, we can
            // convert to Quadratic Formula:
            // Y = 25X^2 + 25X
            // 0 = 25X^2 + 25X - Y
            // a = 25;
            // b = 25;
            // c = -Y;
            // X = (-b +/- SqRt(b^2 - 4ac)) / 2a
            // X = (-25 +/- SqRt(25^2 - 4*25*Y)) / 2*25
            // X = (-25 +/- SqRt(625 - 100(-Y))) / 50
            // X = (SqRt(625 - 100(-Y)) - 25) / 50
            // X = (SqRt(625 + 100Y) - 25) / 50
            // We know Y, which is the XP we have. So we can plug that
            // in to determine X, our level. Then convert it to an int,
            // which will round down to the level
            // 0-49 is level 1, but will come up as 0. 
            return ((int)(Math.Sqrt(625 + 100 * XP) - 25) / 50) + 1;
        }

        public void GainItem(string itemName, int count, string rarityModifier, string randomModifier)
        {
            ItemData itemData = ItemSystem.Instance.GetItemData(itemName);
            if (itemData == null)
            {
                return;
            }

            // If it can stack (99 max), add it
            if (itemData.IsStackable())
            {
                // First, add to existing stacks
                foreach (var itemSlot in ItemSlots)
                {
                    if (itemSlot.ItemData.IsStackable() && itemSlot.Name == itemName)
                    {
                        int stackSizeRemaining = 99 - itemSlot.Count;
                        if (stackSizeRemaining > 0)
                        {
                            if (count > stackSizeRemaining)
                            {
                                itemSlot.Count += stackSizeRemaining;
                                count -= stackSizeRemaining;
                            }
                            else
                            {
                                itemSlot.Count += count;
                                count = 0;
                            }
                        }
                        if (count <= 0)
                        {
                            return;
                        }
                    }
                }

                // Add remaining to new stack
                var itemDataSlot = new ItemDataSlot(itemData, count, null, null, GenerateNewItemID());
                ItemSlots.Add(itemDataSlot);
                return;
            }

            ItemModifierData rarityModifierData = ItemSystem.Instance.GetRarityModifier(rarityModifier);
            ItemModifierData randomModifierData = ItemSystem.Instance.GetRandomModifier(randomModifier);

            // This is not stackable, so add each one as a new entry
            for (int i = 0; i < count; i++)
            {
                var itemDataSlot = new ItemDataSlot(itemData, 1, rarityModifierData, randomModifierData, GenerateNewItemID());
                ItemSlots.Add(itemDataSlot);
            }
        }

        private int GenerateNewItemID()
        {
            return ++LastItemID;
        }

        public void UseItem(string itemName, int count)
        {
            for (int i = ItemSlots.Count - 1; i >= 0; --i)
            {
                ItemDataSlot itemSlot = ItemSlots[i];
                if (itemSlot.Name == itemName)
                {
                    itemSlot.Count -= count;
                    if (itemSlot.Count <= 0)
                    {
                        ItemSlots.RemoveAt(i);
                    }
                    return;
                }
            }
        }

        public void SellItem(string itemName, int count, int itemID, double shopSalePercentage)
        {
            for (int i = ItemSlots.Count - 1; i >= 0; --i)
            {
                ItemDataSlot itemSlot = ItemSlots[i];
                if (itemSlot.ItemID == itemID && itemSlot.Name == itemName)
                {
                    int costPerItem = itemSlot.GetSellCost(shopSalePercentage);

                    // If we have more than we are going to sell,
                    // sell and return early
                    if (itemSlot.Count > count)
                    {
                        itemSlot.Count -= count;
                        Gold += costPerItem * count;
                        count = 0;
                    }
                    // Otherwise, we sell all of this slot,
                    // remove it, and keep going.
                    else
                    {
                        Gold += costPerItem * itemSlot.Count;
                        count -= itemSlot.Count;
                        ItemSlots.RemoveAt(i);
                    }
                    if (count <= 0)
                    {
                        return;
                    }
                }
            }
        }

        public void UnequipSlot(SlotType slotType)
        {
            if (EquippedItems.Remove(slotType, out var itemDataSlot))
            {
                ItemSlots.Add(itemDataSlot);
            }
        }

        public bool TryGetEquipmentInSlot(SlotType slotType, out ItemDataSlot itemDataSlot)
        {
            itemDataSlot = null;
            if (EquippedItems.TryGetValue(slotType, out itemDataSlot))
            {
                return true;
            }
            return false;
        }

        public void EquipItem(string itemName, int itemID)
        {
            ItemData itemData = ItemSystem.Instance.GetItemData(itemName);
            if (itemData == null)
            {
                Debug.LogError("Item not found: " + itemName);
                return;
            }

            // Remove the item we have equipped in that slot
            // If it's a two hand, remove both main hand and off hand
            if (itemData.SlotType == SlotType.TwoHand)
            {
                UnequipSlot(SlotType.OneHand);
                UnequipSlot(SlotType.OffHand);
                UnequipSlot(SlotType.TwoHand);
            }
            else
            {
                // If we're equipping a OneHand or OffHand, ensure we
                // remove the TwoHand
                if (itemData.SlotType == SlotType.OneHand || itemData.SlotType == SlotType.OffHand)
                {
                    UnequipSlot(SlotType.TwoHand);
                }
                UnequipSlot(itemData.SlotType);
            }

            // Now add the item
            for (int i = ItemSlots.Count - 1; i >= 0; --i)
            {
                ItemDataSlot itemSlot = ItemSlots[i];
                if (itemSlot.ItemID == itemID &&
                    itemSlot.ItemData.SlotType == itemData.SlotType &&
                    itemSlot.Name == itemData.Name)
                {
                    // Add the item to the equipped list and
                    // remove it from our normal items slots
                    EquippedItems.Add(itemData.SlotType, itemSlot);
                    ItemSlots.RemoveAt(i);
                    break;
                }
            }
        }

        public int GetItemCount(string itemName)
        {
            foreach (var itemSlot in ItemSlots)
            {
                if (itemSlot.Name == itemName)
                {
                    return itemSlot.Count;
                }
            }
            return 0;
        }

        public override int CalculatedAttack()
        {
            int equipmentValue = 0;
            foreach (var item in EquippedItems)
            {
                equipmentValue += item.Value.GetCalculatedAttack();
            }
            double result = Attack + Strength / 2 + equipmentValue;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyAttack, SkillType.ModifyAllAttack);
            return (int) result;
        }

        public override int CalculatedMAttack()
        {
            int equipmentValue = 0;
            foreach (var item in EquippedItems)
            {
                equipmentValue += item.Value.GetCalculatedMAttack();
            }
            double result = MAttack + Intelligence / 2 + equipmentValue;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyMAttack, SkillType.ModifyAllAttack);
            return (int)result;

        }

        public override int CalculatedDefense()
        {
            int equipmentValue = 0;
            foreach (var item in EquippedItems)
            {
                equipmentValue += item.Value.GetCalculatedDefense();
            }
            double result = Defense + Endurance / 2 + equipmentValue;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyDefense, SkillType.ModifyAllDefense);
            return (int)result;

        }

        public override int CalculatedMDefense()
        {
            int equipmentValue = 0;
            foreach (var item in EquippedItems)
            {
                equipmentValue += item.Value.GetCalculatedMDefense();
            }
            double result = MDefense + Intelligence / 2 + equipmentValue;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifyMDefense, SkillType.ModifyAllDefense);
            return (int)result;

        }

        public override int CalculatedSpeed()
        {
            int equipmentValue = 0;
            foreach (var item in EquippedItems)
            {
                equipmentValue += item.Value.GetCalculatedSpeed();
            }
            double result = Speed + Agility / 2 + equipmentValue;
            ModifyStatViaActiveSkills(ref result, SkillType.ModifySpeed);
            return (int)result;

        }

        public override void TakeDamage(CombatEntity source, CombatContext combatContext)
        {
            base.TakeDamage(source, combatContext);
            GameEvents.PlayerDamaged(combatContext);
        }

        public override void Heal(CombatContext combatContext)
        {
            base.Heal(combatContext);
            GameEvents.PlayerHealed(combatContext);
        }

        public override void RestoreMP(CombatContext combatContext)
        {
            base.RestoreMP(combatContext);
            GameEvents.PlayerRestoreMP(combatContext);
        }

        public override void StealMP(CombatEntity target, CombatContext combatContext)
        {
            base.StealMP(target, combatContext);
            GameEvents.PlayerStealMP(combatContext);
        }

        public override void UseSkill(CombatContext combatContext)
        {
            base.UseSkill(combatContext);
            GameEvents.PlayerMPChanged();
        }

        public override void ValidateHPandMaxHP()
        {
            base.ValidateHPandMaxHP();
            GameEvents.PlayerMaxHPChanged();
        }

        public override void ValidateMPandMaxMP()
        {
            base.ValidateMPandMaxMP();
            GameEvents.PlayerMaxMPChanged();
        }

    }
}