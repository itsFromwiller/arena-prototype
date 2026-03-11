// Uncomment to turn on debug lines
// #define DEBUG_LOGS

using System.Collections.Generic;
using UnityEngine;

namespace Arena.Dungeon
{
    public class DungeonFloorEntity
    {
        public DungeonEntity DungeonEntity;
        public int FloorNumber;
        public List<DungeonRoomEntity> Rooms = new List<DungeonRoomEntity>();

        public DungeonFloorEntity(DungeonEntity dungeonEntity, int floorNumber)
        {
            DungeonEntity = dungeonEntity;
            FloorNumber = floorNumber;

            int roomsInFloor = 1;
            // Determine how many rooms for this floor
            foreach (var roomData in dungeonEntity.RoomCountData)
            {
                // Each room data has a min and max floor for it to be used
                // and we use the first room that matches our floor count
                if (roomData.MinFloor <= floorNumber && roomData.MaxFloor >= floorNumber)
                {
                    roomsInFloor = Random.Range(roomData.MinRoom, roomData.MaxRoom + 1);
                    break;
                }
            }

#if DEBUG_LOGS
            Debug.Log($"Dungeon: Generating {roomsInFloor} rooms for floor {floorNumber}");
#endif

            for (int roomNumber = 1; roomNumber <= roomsInFloor; ++roomNumber)
            {
                Rooms.Add(new DungeonRoomEntity(this, roomNumber));
            }
        }

        public void EnterRoom(int roomNumber)
        {
            DungeonEntity.CurrentRoom = roomNumber;
            Rooms[roomNumber - 1].Enter();
        }

        public bool IsLastRoom()
        {
            return (DungeonEntity.CurrentRoom >= Rooms.Count);
        }
    }
}