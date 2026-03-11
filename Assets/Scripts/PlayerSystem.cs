// Uncomment to turn on debug lines
//#define DEBUG_LOGS

using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using Arena.Loot;
using Arena.Core;
using Arena.Items;
using Arena.Combat;

namespace Arena.Player
{
    public class PlayerSystem : MonoBehaviour
    {
        public static PlayerSystem Instance;

        private Dictionary<string, PlayerClass> classDatabase = new Dictionary<string, PlayerClass>();
        private Dictionary<string, PlayerRace> raceDatabase = new Dictionary<string, PlayerRace>();

        public PlayerEntity Player { get; set; }

        private void OnEnable()
        {
            GameEvents.OnRestAtInn += HandleOnRestAtInn;
            GameEvents.OnGetGold += HandleOnGetGold;
            GameEvents.OnGetLoot += HandleOnGetLoot;
        }

        private void OnDisable()
        {
            GameEvents.OnRestAtInn -= HandleOnRestAtInn;
            GameEvents.OnGetGold -= HandleOnGetGold;
            GameEvents.OnGetLoot -= HandleOnGetLoot;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnApplicationQuit()
        {
            SavePlayer();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                SavePlayer();
            }
        }

        public void SetData(Dictionary<string, string> data)
        {
            var raceData = JsonConvert.DeserializeObject<List<PlayerRace>>(data["Race"]);
            var classData = JsonConvert.DeserializeObject<List<PlayerClass>>(data["Class"]);
            foreach (var dataItem in raceData)
            {
                if (!raceDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Race data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
            }
            foreach (var dataItem in classData)
            {
                if (!classDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Class data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
                string[] itemTypes = dataItem.WeaponProficiencies.Split(",");
                foreach (var itemType in itemTypes)
                {
                    dataItem.WeaponProficiencyList.Add(EnumMap<ItemType>.GetValue(itemType.TrimStart()));
                }
            }
        }

        public void Init()
        {
            foreach (var dataItem in classDatabase.Values)
            {
                string[] skillNames = dataItem.StartingSkills.Split(",");
                foreach (var skillName in skillNames)
                {
                    dataItem.StartingSkillList.Add(SkillSystem.Instance.GetSkillData(skillName.TrimStart()));
                }
                string[] itemNames = dataItem.StartingItems.Split(",");
                foreach (var itemName in itemNames)
                {
                    dataItem.StartingItemList.Add(ItemSystem.Instance.GetItemData(itemName.TrimStart()));
                }
            }

            // Will be null if we don't have a player
            Player = LoadPlayer();
            if (Player != null)
            {
                Player.Init();
            }
        }

        public void CreatePlayer(string name)
        {
            Player = new PlayerEntity(name);
            Player.LearnSkill("Run Away");
            Player.GainItem("Weak Potion", 5, null, null);
            Player.Init();
        }

        public void SavePlayer()
        {
            // Happens if we exit before loading the player
            if (Player == null)
            {
                return;
            }
            string json = JsonConvert.SerializeObject(Player);
            string path = Application.persistentDataPath + "/player.json";
            File.WriteAllText(path, json);
        }

        public PlayerEntity LoadPlayer()
        {
            string path = Application.persistentDataPath + "/player.json";
            if (File.Exists(path)) // Check if file exists
            {
                string json = File.ReadAllText(path); // Load from file
#if DEBUG_LOG
                Debug.Log("Player Json: " + json);
#endif
                PlayerEntity player = JsonConvert.DeserializeObject<PlayerEntity>(json);
                return player;
            }
            else
            {
                return null;
            }
        }

        void HandleOnRestAtInn()
        {
            // Restore HP and MP
            Player.HP = Player.MaxHP;
            Player.MP = Player.MaxMP;
            GameEvents.PlayerHPChanged();
            GameEvents.PlayerMPChanged();
        }

        void HandleOnGetGold(int gold)
        {
            Player.Gold += gold;
        }

        void HandleOnGetLoot(List<LootResult> lootResults)
        {
            foreach (var loot in lootResults)
            {
                Player.GainItem(loot.ItemDataSlot.ItemData.Name, loot.Count,
                    loot.ItemDataSlot.RarityModifierData != null ? loot.ItemDataSlot.RarityModifierData.Name : null,
                    loot.ItemDataSlot.RandomModifierData != null ? loot.ItemDataSlot.RandomModifierData.Name : null);
            }
        }
    }
}