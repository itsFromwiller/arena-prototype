using Arena.Loot;
using System.Collections.Generic;

namespace Arena.Enemies
{
    public class EnemyData
    {
        public string Name;
        public int Attack;
        public int MAttack;
        public int Defense;
        public int MDefense;
        public int Speed;
        public int Loot;
        public int Level;
        public int XP;
        public int HP;
        public int MP;
        public string Actions;
        public string LootTables;
        public List<EnemyActionData> ActionDataList = new List<EnemyActionData>();
        public List<LootTableData> LootTableDataList = new List<LootTableData>();
    }
}