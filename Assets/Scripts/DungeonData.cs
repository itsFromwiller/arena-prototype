using System.Collections;

namespace Arena.Dungeon
{
    public enum SpawnType
    {
        Enemy,
        Boss,
        Room,
        Floors,
        Rooms
    }

    public class DungeonData
    {
        public string Name;
        public string SpawnTypeName;
        public SpawnType SpawnType; // Parsed at runtime, not from data
        public string Spawn;
        public int Weight;
        public int MinFloor;
        public int MaxFloor;
        public int MinRoom;
        public int MaxRoom;
        public int MaxSpawn;
    }
}