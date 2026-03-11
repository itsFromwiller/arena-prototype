using Arena.Combat;
using Arena.Items;
using System.Collections.Generic;

namespace Arena.Player
{
    public class PlayerClass
    {
        public string Name;
        public string WeaponProficiencies;
        public string StartingSkills;
        public string StartingItems;
        public int StartingGold;
        public List<ItemType> WeaponProficiencyList;
        public List<SkillData> StartingSkillList;
        public List<ItemData> StartingItemList;
    }
}