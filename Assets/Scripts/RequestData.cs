using System.Collections;
using UnityEngine;

namespace Arena.Requests
{
    public enum RequestType
    {
        Gather,
        Kill
    }

    public class RequestData
    {
        public string Id;
        public string Name;
        public string Objective;
        public string RequestTypeName;
        public RequestType RequestType;
        public string TargetName;
        public int Count;
        public string RewardItem;
        public int RewardGold;
        public int RewardXP;
        public int MinLevel;
        public string SpawnLoot;
        public string SpawnEnemy;
        public string SpawnDungeon;
        public string RequiresDungeon;
    }
}