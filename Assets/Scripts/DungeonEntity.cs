// Uncomment to turn on debug lines
// #define DEBUG_LOGS

using System.Collections.Generic;

namespace Arena.Dungeon
{
    public class DungeonEntity
    {
        private List<DungeonFloorEntity> Floors = new();
        public int CurrentFloor = 1;
        public int CurrentRoom = 1;
        public string DungeonName;

        public List<DungeonData> EnemyData = new();
        public List<DungeonData> BossData = new();
        public List<DungeonData> RoomData = new();
        public List<DungeonData> RoomCountData = new();
        public DungeonData FloorData;
        public Dictionary<string, int> SpawnCount = new();

        public DungeonEntity(string dungeonName, List<DungeonData> dungeonDataList)
        {
            DungeonName = dungeonName;
            foreach (var dungeonData in dungeonDataList)
            {
                switch (dungeonData.SpawnType)
                {
                    case SpawnType.Enemy:
                    {
                        EnemyData.Add(dungeonData);
#if DEBUG_LOGS
                        Debug.Log($"Dungeon: Adding {dungeonData.Spawn} to Enemy Data");
#endif
                        break;
                    }
                    case SpawnType.Boss:
                    {
                        BossData.Add(dungeonData);
#if DEBUG_LOGS
                        Debug.Log($"Dungeon: Adding {dungeonData.Spawn} to Boss Data");
#endif
                        break;
                    }
                    case SpawnType.Room:
                    {
                        RoomData.Add(dungeonData);
#if DEBUG_LOGS
                        Debug.Log($"Dungeon: Adding {dungeonData.Spawn} to Room Data");
#endif
                        break;
                    }
                    case SpawnType.Floors:
                    {
                        FloorData = dungeonData;
                        break;
                    }
                    case SpawnType.Rooms:
                    {
                        RoomCountData.Add(dungeonData);
#if DEBUG_LOGS
                        Debug.Log($"Dungeon: Adding {dungeonData.Spawn} to RoomCount Data");
#endif
                        break;
                    }
                }
            }
        }

        public void Generate()
        {
            // Determine how many floors we need
            int floorCount = FloorData.MaxFloor;
#if DEBUG_LOGS
            Debug.Log($"Dungeon: Generating {DungeonName} dunegon with {floorCount} floors.");
#endif

            // Create floors
            for (int floorNumber = 1; floorNumber <= floorCount; ++floorNumber)
            {
                Floors.Add(new DungeonFloorEntity(this, floorNumber));
            }
        }

        public void EnterFloor(int floorNumber)
        {
            // Always enter room 1 of the floor
            CurrentFloor = floorNumber;
            Floors[CurrentFloor - 1].EnterRoom(1);
        }

        public bool IsLastRoomOfDungeon()
        {
            if (IsLastRoomOfFloor())
            {
                if (Floors.Count <= CurrentFloor)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsLastRoomOfFloor()
        {
            return Floors[CurrentFloor - 1].IsLastRoom();
        }

        public void AdvanceRoom()
        {
            if (Floors[CurrentFloor - 1].IsLastRoom())
            {
                if (Floors.Count > CurrentFloor)
                {
                    EnterFloor(CurrentFloor + 1);
                }
                else
                {
                    DungeonSystem.Instance.FinishDungeon();
                }
            }
            else
            {
                Floors[CurrentFloor - 1].EnterRoom(CurrentRoom + 1);
            }
        }
    }
}