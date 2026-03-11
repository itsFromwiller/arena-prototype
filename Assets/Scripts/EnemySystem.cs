// Uncomment to turn on debug lines
//#define DEBUG_LOGS

using Arena.Core;
using Arena.Loot;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Enemies
{
    public class EnemySystem : MonoBehaviour
    {
        public static EnemySystem Instance;

        private Dictionary<string, EnemyData> enemyDatabase = new Dictionary<string, EnemyData>();
        private Dictionary<string, EnemyActionData> enemyActionDatabase = new Dictionary<string, EnemyActionData>();

        private void Awake()
        {
            Instance = this;
        }

        public void SetData(Dictionary<string, string> data)
        {
            var enemyData = JsonConvert.DeserializeObject<List<EnemyData>>(data["Enemies"]);
            var enemyActionData = JsonConvert.DeserializeObject<List<EnemyActionData>>(data["EnemyActions"]);
            foreach (var dataItem in enemyData)
            {
                if (!enemyDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Enemy data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
            }
            foreach (var dataItem in enemyActionData)
            {
                if (!enemyActionDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Enemy action data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
                dataItem.ConditionType = EnumMap<ConditionType>.GetValue(dataItem.ConditionName);
            }
        }

        public void Init()
        {
            foreach (var enemyData in enemyDatabase.Values)
            {
                string[] actions = enemyData.Actions.Split(",");
                foreach (var action in actions)
                {
                    AddEnemyActionData(enemyData, action.TrimStart());
                }
                string[] lootTables = enemyData.LootTables.Split(",");
                foreach (var lootTable in lootTables)
                {
#if DEBUG_LOGS
                    Debug.LogError($"Adding {lootTable.TrimStart()} list to {enemyData.Name}");
#endif
                    enemyData.LootTableDataList.AddRange(LootSystem.Instance.GetLootTables(lootTable.TrimStart()));
                }
#if DEBUG_LOGS
                Debug.LogError($"{enemyData.Name} loot table size: {enemyData.LootTableDataList.Count}");
#endif
            }
        }

        private void AddEnemyActionData(EnemyData enemyData, string actionDataName)
        {
            if (!string.IsNullOrEmpty(actionDataName))
            {
                if (enemyActionDatabase.TryGetValue(actionDataName, out var enemyActionData))
                {
                    enemyData.ActionDataList.Add(enemyActionData);
                }
            }
        }

        public EnemyEntity CreateEnemy(string name, int level)
        {
            return new EnemyEntity(GetEnemyData(name), level);
        }

        public EnemyData GetEnemyData(string name)
        {
            if (!enemyDatabase.TryGetValue(name, out EnemyData enemyData))
            {
                Debug.LogError($"Enemy {name} wasn't found, getting a Small Rat instead");
                enemyData = enemyDatabase["Small Rat"];
            }
            return enemyData;
        }
    }
}
