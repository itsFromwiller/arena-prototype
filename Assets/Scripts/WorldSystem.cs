using Arena.Core;
using Arena.Dungeon;
using Arena.Player;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Arena.World
{
    public class WorldSystem : MonoBehaviour
    {
        public GameObject WorldView;
        public GameObjectPoolManager DungeonButtonPoolManager;
        public Transform DungeonGridContent;
        
        private List<GameObject> activeButtons = new();

        private void Awake()
        {
            WorldView.SafeSetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEnterWorld += OnEnterWorld;
        }

        private void OnDisable()
        {
            GameEvents.OnEnterWorld -= OnEnterWorld;
        }

        void OnEnterWorld()
        {
            // Populate dungeons and town
            foreach(var activeButton in activeButtons)
            {
                DungeonButtonPoolManager.ReturnToPool(activeButton);
            }
            activeButtons.Clear();

            List<DungeonInfoData> unlockedDungeons = new();
            foreach (var dungeonName in PlayerSystem.Instance.Player.DungeonsUnlocked)
            {
                var dungeonInfo = DungeonSystem.Instance.GetDungeonInfo(dungeonName);
                if (dungeonInfo != null)
                {
                    unlockedDungeons.Add(dungeonInfo);
                }
            }
            unlockedDungeons.Sort((a, b) =>
            {
                int levelComparison = a.Level - b.Level;
                if (levelComparison == 0)
                {
                    return string.Compare(a.Name, b.Name);
                }
                return levelComparison;
            });

            foreach (var dungeonInfo in unlockedDungeons)
            { 
                var dungeonButton = DungeonButtonPoolManager.GetPooledObject<Button>();

                activeButtons.Add(dungeonButton.gameObject);

                dungeonButton.gameObject.transform.SetParent(DungeonGridContent);
                dungeonButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = dungeonInfo.Name;
                dungeonButton.onClick.RemoveAllListeners();
                dungeonButton.onClick.AddListener(() => OnDungeonButtonClicked(dungeonInfo.Name));
                dungeonButton.gameObject.SafeSetActive(true);
            }

            WorldView.SafeSetActive(true);
        }

        void OnDungeonButtonClicked(string dungeonName)
        {
            WorldView.SafeSetActive(false);
            GameEvents.EnterDungeon(dungeonName);
        }

        public void SelectTownButton()
        {
            WorldView.SafeSetActive(false);
            GameEvents.EnterTown();
        }

    }
}