// Uncomment to turn on debug lines
// #define DEBUG_LOGS

using Arena.Core;
using Arena.Enemies;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace Arena.Dungeon
{
    public partial class DungeonSystem : MonoBehaviour
    {
        public static DungeonSystem Instance;

        public GameObject DungeonView;
        public TextMeshProUGUI DungeonName;
        public TextMeshProUGUI DungeonInfo;
        public TextMeshProUGUI DungeonMinLevel;

        private Dictionary<string, List<DungeonData>> dungeonDatabase = new Dictionary<string, List<DungeonData>>();
        private Dictionary<string, DungeonInfoData> dungeonInfoDatabase = new Dictionary<string, DungeonInfoData>();
        private DungeonEntity DungeonEntity;

        private void Awake()
        {
            Instance = this;
            DungeonView.SafeSetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEnterDungeon += OnEnterDungeon;
        }

        private void OnDisable()
        {
            GameEvents.OnEnterDungeon -= OnEnterDungeon;
        }

        private void OnApplicationQuit()
        {
            SaveDungeon();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                SaveDungeon();
            }
        }

        public void SaveDungeon()
        {
            if (DungeonEntity == null)
            {
                return;
            }
            string json = JsonConvert.SerializeObject(DungeonEntity);
            string path = Application.persistentDataPath + "/dungeon.json";
            File.WriteAllText(path, json);
        }

        public DungeonEntity LoadDungeon()
        {
            string path = Application.persistentDataPath + "/dungeon.json";
            if (File.Exists(path)) // Check if file exists
            {
                string json = File.ReadAllText(path); // Load from file
#if DEBUG_LOG
                Debug.Log("Dungeon Json: " + json);
#endif
                DungeonEntity dungeon = JsonConvert.DeserializeObject<DungeonEntity>(json);
                // Setup for data hookup
                // Init for entity ref hookup
                // sync
                // SyncViewText();
                return dungeon;
            }
            else
            {
                return null;
            }
        }

        public void SetData(Dictionary<string, string> data)
        {
            var dungeonData = JsonConvert.DeserializeObject<List<DungeonData>>(data["Dungeons"]);
            foreach (var dataItem in dungeonData)
            {
                if (!dungeonDatabase.ContainsKey(dataItem.Name))
                {
                    dungeonDatabase.Add(dataItem.Name, new List<DungeonData>());
                }
                dungeonDatabase[dataItem.Name].Add(dataItem);
                dataItem.SpawnType = EnumMap<SpawnType>.GetValue(dataItem.SpawnTypeName);
            }
            var dungeonInfoData = JsonConvert.DeserializeObject<List<DungeonInfoData>>(data["DungeonInfo"]);
            foreach (var dataItem in dungeonInfoData)
            {
                if (!dungeonInfoDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Dungeon Info data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
            }

        }

        public void Init()
        {
            // Will be null if we aren't in a dungeon currently
//            DungeonEntity = LoadDungeon();
            if (DungeonEntity != null)
            {
//                DungeonEntity.Init();
            }
        }

        void SyncViewText()
        {
            DungeonName.text = DungeonEntity.DungeonInfoData.Name;
            DungeonInfo.text = DungeonEntity.DungeonInfoData.Description;
            DungeonMinLevel.text = $"Min level: {DungeonEntity.DungeonInfoData.Level}";
        }

        void OnEnterDungeon(string dungeonName)
        {
            // Populate Checkpoint Floors
            DungeonView.SafeSetActive(true);
            GenerateDungeon(dungeonName);

            SyncViewText();
        }

        public void EnterDungeonFloor(int floor)
        {
            DungeonView.SafeSetActive(false);
            DungeonEntity.EnterFloor(floor);
        }

        public void AdvanceRoom()
        {
            DungeonEntity.AdvanceRoom();
        }

        public void LeaveDungeon()
        {
            DungeonView.SafeSetActive(false);
            DungeonEntity = null;
            GameEvents.EnterWorld();
        }

        public void FinishDungeon()
        {
            GameEvents.FinishDungeon();
            LeaveDungeon();
        }

        public bool IsLastRoomOfDungeon()
        {
            return DungeonEntity.IsLastRoomOfDungeon();
        }

        public bool IsLastRoomOfFloor()
        {
            return DungeonEntity.IsLastRoomOfFloor();
        }

        private void GenerateDungeon(string dungeonName)
        {
            if (!dungeonDatabase.TryGetValue(dungeonName, out var dungeonDataList))
            {
                Debug.LogError("Dungeon doesn't exist: " + dungeonName);
                return;
            }

            if (!dungeonInfoDatabase.TryGetValue(dungeonName, out var dungeonInfoData))
            {
                Debug.LogError("Dungeon Info doesn't exist: " + dungeonName);
                return;
            }

            DungeonEntity = new DungeonEntity(dungeonName, dungeonDataList, dungeonInfoData);
            DungeonEntity.Generate();
        }
    }
}