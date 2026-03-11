using UnityEngine;

namespace Arena.World
{
    public class WorldSystem : MonoBehaviour
    {
        public GameObject WorldView;

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
            WorldView.SafeSetActive(true);
        }

        public void SelectDungeonButton()
        {
            WorldView.SafeSetActive(false);
            GameEvents.EnterDungeon("Sewer");
        }

        public void SelectTownButton()
        {
            WorldView.SafeSetActive(false);
            GameEvents.EnterTown();
        }

    }
}