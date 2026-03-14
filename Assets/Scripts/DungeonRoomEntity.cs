// Uncomment to turn on debug lines
// #define DEBUG_LOGS

using Arena.Assets.Scripts.Core;
using Arena.Enemies;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Arena.Dungeon
{
    public class DungeonRoomEntity
    {
        // Will need to be set up upon loading a save game
        [JsonIgnore]
        public DungeonEntity DungeonEntity;
        [JsonIgnore]
        public DungeonFloorEntity FloorEntity;

        public int RoomNumber;
        public string RoomTypeName;
        public string SpawnName;

        public DungeonRoomEntity(DungeonFloorEntity floorEntity, int roomNumber)
        {
            DungeonEntity = floorEntity.DungeonEntity;
            FloorEntity = floorEntity;
            RoomNumber = roomNumber;

            var selectedRoomData = PickFromBucket(DungeonEntity.RoomData);
            DungeonEntity.SpawnCount[selectedRoomData.Spawn]++;
            RoomTypeName = selectedRoomData.Spawn;

#if DEBUG_LOGS
            Debug.Log($"Dungeon: Generating {RoomTypeName} room as room {roomNumber}");
#endif

            switch (RoomTypeName)
            {
                case "Combat":
                {
                    var selectedEnemyData = PickFromBucket(DungeonEntity.EnemyData);
                    SpawnName = selectedEnemyData.Spawn;
#if DEBUG_LOGS
                    Debug.Log($"Dungeon: Spawning {SpawnName} enemy");
#endif
                    break;
                }
                case "Treasure":
                {
                    break;
                }
                case "Fountain":
                {
                    break;
                }
                case "Boss":
                {
                    var selectedBossData = PickFromBucket(DungeonEntity.BossData);
                    SpawnName = selectedBossData.Spawn;
#if DEBUG_LOGS
                    Debug.Log($"Dungeon: Spawning {SpawnName} boss");
#endif
                    break;
                }
            }
        }

        public DungeonData PickFromBucket(List<DungeonData> items)
        {
            var bucket = new WeightedBucket<DungeonData>();
            foreach (var data in items)
            {
                if (!DungeonEntity.SpawnCount.TryGetValue(data.Spawn, out var spawnCount))
                {
                    DungeonEntity.SpawnCount.Add(data.Spawn, 0);
                }
                if (data.MinFloor <= FloorEntity.FloorNumber && data.MaxFloor >= FloorEntity.FloorNumber)
                {
                    if (data.MinRoom <= RoomNumber && data.MaxRoom >= RoomNumber)
                    {
                        if (data.MaxSpawn > 0 && spawnCount >= data.MaxSpawn)
                        {
                            continue;
                        }
                        bucket.AddItem(data, data.Weight);
#if DEBUG_LOGS
                        Debug.Log($"Dungeon: Adding {data.Spawn} to bucket");
#endif
                    }
                }
            }
            return bucket.GetRandomItem();
        }

        public void Enter()
        {
            GameEvents.EnterDungeonRoom(this);
            switch (RoomTypeName)
            {
                case "Combat":
                {
                    EnemyEntity enemy = EnemySystem.Instance.CreateEnemy(SpawnName, 1);
                    GameEvents.EnterCombat();
                    GameEvents.PlayerSpawned();
                    GameEvents.EnemySpawned(enemy);
                    GameEvents.StartCombat();
                    break;
                }
                case "Treasure":
                {
                    break;
                }
                case "Fountain":
                {
                    break;
                }
                case "Boss":
                {
                    EnemyEntity enemy = EnemySystem.Instance.CreateEnemy(SpawnName, 1);
                    GameEvents.EnterCombat();
                    GameEvents.PlayerSpawned();
                    GameEvents.EnemySpawned(enemy);
                    GameEvents.StartCombat();
                    break;
                }
            }
        }
    }
}