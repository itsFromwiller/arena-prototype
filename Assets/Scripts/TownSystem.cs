using UnityEngine;

namespace Arena.Town
{
    public class TownSystem : MonoBehaviour
    {
        public GameObject TownView;

        private void Awake()
        {
            TownView.SafeSetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEnterTown += OnEnterTown;
        }

        private void OnDisable()
        {
            GameEvents.OnEnterTown -= OnEnterTown;
        }

        void OnEnterTown()
        {
            // Populate town
            TownView.SafeSetActive(true);
            GameEvents.RequestSaveGame();
        }

        public void SelectInnButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterInn();
        }

        public void SelectTavernButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterTavern();
        }

        public void SelectWeaponShopButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterShop("Weapons");
        }

        public void SelectArmorShopButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterShop("Armor");
        }

        public void SelectItemShopButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterShop("Items");
        }

        public void SelectMagicShopButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterShop("Magic");
        }

        public void SelectBazaarShopButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterShop("Bazaar");
        }

        public void SelectLeaveTownButton()
        {
            TownView.SafeSetActive(false);
            GameEvents.EnterWorld();
        }
    }
}