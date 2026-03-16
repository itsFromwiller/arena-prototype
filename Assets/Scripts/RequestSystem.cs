using Arena.Combat;
using Arena.Core;
using Arena.Dungeon;
using Arena.Player;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Requests
{
    public class RequestSystem : MonoBehaviour
    {
        public static RequestSystem Instance;

        private Dictionary<string, RequestData> RequestDatabase = new();
        private List<RequestData> CurrentRequests = new();

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnEnterDungeonRoom += OnEnterDungeonRoom;
        }

        private void OnDisable()
        {
            GameEvents.OnEnterDungeonRoom -= OnEnterDungeonRoom;
        }

        public void SetData(Dictionary<string, string> data)
        {
            var requestData = JsonConvert.DeserializeObject<List<RequestData>>(data["Requests"]);
            foreach (var dataItem in requestData)
            {
                if (!RequestDatabase.TryAdd(dataItem.Id, dataItem))
                {
                    Debug.LogError($"Request data couldn't be added, something already exists with its id: {dataItem.Id}");
                    continue;
                }
                dataItem.RequestType = EnumMap<RequestType>.GetValue(dataItem.RequestTypeName);
            }
        }

        public void Init()
        {
        }

        void OnEnterDungeonRoom(DungeonRoomEntity entity)
        {
            // Clear our requests once we enter a dungeon room
            CurrentRequests.Clear();
        }

        public RequestData GetRequestData(string id)
        {
            if (RequestDatabase.TryGetValue(id, out RequestData requestData))
            {
                return requestData;
            }
            return null;
        }

        public List<RequestData> GetRequestsForTavern(int totalRequests)
        {
            // First, look at active requests on the player, those are
            // added first
            List<RequestData> requests = new();
            requests.AddRange(PlayerSystem.Instance.Player.ActiveRequestData);
            if (requests.Count >= totalRequests)
            {
                return requests;
            }

            // Then use our cached requests and/or generate new ones
            int remainingRequests = totalRequests - requests.Count;
            if (CurrentRequests.Count < remainingRequests)
            {
                CurrentRequests.AddRange(GenerateNewRequests(remainingRequests));
            }

            for (int i = 0; i < CurrentRequests.Count; ++i)
            {
                requests.Add(CurrentRequests[i]);
                if (requests.Count >= totalRequests)
                {
                    break;
                }
            }

            return requests;
        }

        public List<RequestData> GenerateNewRequests(int countToGenerate)
        {
            var player = PlayerSystem.Instance.Player;
            List<RequestData> results = new();

            // Get a list of all requests the player can do
            // which looks at min level and if they already
            // have the request.
            List<RequestData> validRequests = new();
            foreach (var requestData in RequestDatabase.Values)
            {
                if (requestData.MinLevel > player.Level)
                {
                    continue;
                }
                if (player.HasRequest(requestData.Id))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(requestData.RequiresDungeon))
                {
                    if (!player.HasUnlockedDungeon(requestData.RequiresDungeon))
                    {
                        continue;
                    }
                }
                validRequests.Add(requestData);
            }

            for (int i = 0; i < countToGenerate; ++i)
            {
                if (validRequests.Count == 0)
                {
                    break;
                }
                int index = Random.Range(0, validRequests.Count);
                results.Add(validRequests[index]);
                validRequests.RemoveAt(index);
            }
            return results;
        }

        public string BuildName(RequestData requestData)
        {
            string name = string.Format(requestData.Name, requestData.TargetName);
            return name;
        }

        public string BuildDescription(RequestData requestData)
        {
            string description = string.Format(requestData.Objective, requestData.TargetName, requestData.Count, requestData.RequiresDungeon);
            return description;
        }

        public void AcceptRequest(RequestData requestData)
        {
            PlayerSystem.Instance.Player.AcceptRequest(requestData);
            // Remove this request from our current list
            CurrentRequests.Remove(requestData);
        }

        public void CompleteRequest(RequestData requestData)
        {
            PlayerSystem.Instance.Player.CompleteRequest(requestData);
        }
    }
}