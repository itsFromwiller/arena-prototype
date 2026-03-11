using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Arena.Items;
using Newtonsoft.Json;
using Arena.Core;
using Arena.Dungeon;
using Arena.Player;
using System;

namespace Arena.Tavern
{
    public class TavernSystem : MonoBehaviour
    {
        public static TavernSystem Instance;

        private Dictionary<string, GossipData> GossipDatabase = new();

        public GameObject TavernView;
        public GameObject MainOptionView;
        public GossipView GossipView;
        public TavernFoodView FoodView;
        public GameObject RequestsView;

        private List<GossipData> CurrentGossip = new();
        private int CurrentGossipIndex = 0;
        private bool HasNewDungeonRun = true;

        private void Awake()
        {
            Instance = this;
            TavernView.SafeSetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEnterTavern += OnEnterTavern;
            GameEvents.OnEnterDungeonRoom += OnEnterDungeonRoom;
        }

        private void OnDisable()
        {
            GameEvents.OnEnterTavern -= OnEnterTavern;
            GameEvents.OnEnterDungeonRoom -= OnEnterDungeonRoom;
        }

        public void SetData(Dictionary<string, string> data)
        {
            var gossipData = JsonConvert.DeserializeObject<List<GossipData>>(data["Gossip"]);
            foreach (var dataItem in gossipData)
            {
                if (!GossipDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Gossip data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
            }
        }

        public void Init()
        {
            foreach (var dataItem in GossipDatabase.Values)
            {
                if (!string.IsNullOrEmpty(dataItem.GossipEffectName))
                {
                    dataItem.GossipEffect = EnumMap<GossipEffect>.GetValue(dataItem.GossipEffectName);
                }
            }
        }

        void OnEnterTavern()
        {
            // Populate view
            TavernView.SafeSetActive(true);
            MainOptionView.SafeSetActive(true);
            GossipView.gameObject.SafeSetActive(false);
            FoodView.gameObject.SafeSetActive(false);
            RequestsView.SafeSetActive(false);
        }

        void OnEnterDungeonRoom(DungeonRoomEntity entity)
        {
            CurrentGossipIndex = 0;
            CurrentGossip.Clear();
            HasNewDungeonRun = true;
        }

        public void SelectLeaveTavernButton()
        {
            TavernView.SafeSetActive(false);
            GameEvents.EnterTown();
        }

        public void SelectGossipButton()
        {
            MainOptionView.SafeSetActive(false);
            GossipView.gameObject.SafeSetActive(true);
            GossipView.Setup();
        }

        public void SelectEatButton()
        {
            MainOptionView.SafeSetActive(false);
            FoodView.gameObject.SafeSetActive(true);
            FoodView.Setup();
        }

        public void SelectRequestsButton()
        {

        }

        private void BuildCurrentGossip()
        {
            var player = PlayerSystem.Instance.Player;
            List<GossipData> specialGossip = new List<GossipData>();

            foreach (var gossipData in GossipDatabase.Values)
            {
                if (player.Level < gossipData.MinLevel)
                {
                    continue;
                }
                if (gossipData.GossipEffect == GossipEffect.UnlockDungeon)
                {
                    if (player.HasUnlockedDungeon(gossipData.EffectValue))
                    {
                        continue;
                    }
                    specialGossip.Add(gossipData);
                    continue;
                }
                if (gossipData.GossipEffect == GossipEffect.Info)
                {
                    CurrentGossip.Add(gossipData);
                }
            }

            // Randomize the CurrentGossip then add up to 2 random special gossips to the
            // front.
            CurrentGossip.Randomize();
            if (HasNewDungeonRun && specialGossip.Count > 0)
            {
                HasNewDungeonRun = false;
                specialGossip.Randomize();
                int maxIndex = Math.Min(2, specialGossip.Count);
                for (int i = 0; i < maxIndex; ++i)
                {
                    CurrentGossip.Insert(0, specialGossip[i]);
                }
            }
        }

        public GossipData GetNextGossip()
        {
            if (CurrentGossip.Count == 0)
            {
                BuildCurrentGossip();
            }

            var gossipData = CurrentGossip[CurrentGossipIndex];

            CurrentGossipIndex++;
            if (CurrentGossipIndex >= CurrentGossip.Count)
            {
                CurrentGossipIndex = 0;
                CurrentGossip.Clear();
            }

            return gossipData;
        }
    }
}