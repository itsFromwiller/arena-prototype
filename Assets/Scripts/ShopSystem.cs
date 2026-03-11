using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Arena.Items;
using Arena.Player;
using Arena.Dungeon;

namespace Arena.Shop
{
    public class ShopSystem : MonoBehaviour
    {
        public static ShopSystem Instance;

        public GameObject ShopView;
        public TextMeshProUGUI ShopName;
        public SelectionView SelectionView;
        public GameObject MainOptions;
        public GameObject BackButton;
        public TextMeshProUGUI CurrentGold;
        public int ItemCountInBazaar = 10;
        private string ShopType;
        private List<ItemDataSlot> CurrentSellableItems = new ();
        private List<ItemDataSlot> CurrentBuyableItems = new();
        private List<ItemDataSlot> CurrentBazaarItems = new();

        private void Awake()
        {
            Instance = this;
            ShopView.SafeSetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEnterShop += OnEnterShop;
            GameEvents.OnEnterDungeonRoom += OnEnterDungeonRoom;
        }

        private void OnDisable()
        {
            GameEvents.OnEnterShop -= OnEnterShop;
            GameEvents.OnEnterDungeonRoom -= OnEnterDungeonRoom;
        }

        void OnEnterShop(string shopType)
        {
            ShopType = shopType;
            ShopView.SafeSetActive(true);
            BackButton.SafeSetActive(false);
            ShowItemSelectionView(false);
            if (shopType == "Bazaar")
            {
                ShopName.text = shopType;
            }
            else
            {
                ShopName.text = $"{shopType} Shop";
            }
            CurrentGold.text = $"Gold: {PlayerSystem.Instance.Player.Gold}";
        }

        void OnEnterDungeonRoom(DungeonRoomEntity entity)
        {
            // Clear our bazaar once we enter a dungeon room
            CurrentBazaarItems.Clear();
        }

        void ShowItemSelectionView(bool isShown)
        {
            BackButton.SafeSetActive(isShown);
            SelectionView.gameObject.SafeSetActive(isShown);
            MainOptions.SafeSetActive(!isShown);
            BackButton.SafeSetActive(isShown);
        }

        public void SelectBuyButton()
        {
            // Show items that the player
            // can buy, which is based upon player level
            HashSet<ItemType> filterTypes = new HashSet<ItemType>();
            bool onlyMagicConsumables = false;
            bool onlyNonMagicConsumables = false;
            CurrentBuyableItems.Clear();
            switch (ShopType)
            {
                case "Weapons":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Dagger,
                        ItemType.Arrow,
                        ItemType.Axe,
                        ItemType.Bow,
                        ItemType.Mace,
                        ItemType.Sword,
                    });
                    break;
                }
                case "Armor":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Armor,
                        ItemType.Shield,
                    });
                    break;
                }
                case "Magic":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Wand,
                        ItemType.Staff,
                        ItemType.Consumable
                    });
                    onlyMagicConsumables = true;
                    break;
                }
                case "Items":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Consumable,
                        ItemType.Material,
                    });
                    onlyNonMagicConsumables = true;
                    break;
                }
                case "Bazaar":
                {
                    // Bazaar will generate random weapons and armor based upon player level, with item
                    // modifiers added. These should be the same items every time until the player enters
                    // a dungeon or boots up the game.
                    if (CurrentBazaarItems.Count == 0)
                    {
                        CurrentBazaarItems = ItemSystem.Instance.GetBazaarShopItems(ItemCountInBazaar, PlayerSystem.Instance.Player.Level + 5);
                    }
                    CurrentBuyableItems = CurrentBazaarItems;
                    break;
                }
            }
            if (CurrentBuyableItems.Count == 0)
            {
                CurrentBuyableItems = ItemSystem.Instance.GetShopItemsFiltered(filterTypes, onlyMagicConsumables, onlyNonMagicConsumables, PlayerSystem.Instance.Player.Level + 5); ;
            }
            SelectionView.SetupItemDataView(CurrentBuyableItems, ItemSelectionItemView.ActionType.Buy, false);
            ShowItemSelectionView(true);
        }

        public void SelectSellButton()
        {
            // Set up item view, filtered to the type of items the shop handles
            HashSet<ItemType> filterTypes = new HashSet<ItemType>();
            bool onlyMagicConsumables = false;
            bool onlyNonMagicConsumables = false;
            switch (ShopType)
            {
                case "Weapons":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Dagger,
                        ItemType.Arrow,
                        ItemType.Axe,
                        ItemType.Bow,
                        ItemType.Mace,
                        ItemType.Sword,
                    });
                    break;
                }
                case "Armor":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Armor,
                        ItemType.Shield,
                    });
                    break;
                }
                case "Magic":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Wand,
                        ItemType.Staff,
                        ItemType.Consumable
                    });
                    onlyMagicConsumables = true;
                    break;
                }
                case "Items":
                {
                    filterTypes.UnionWith(new HashSet<ItemType>()
                    {
                        ItemType.Consumable,
                        ItemType.Material,
                    });
                    onlyNonMagicConsumables = true;
                    break;
                }
                case "Bazaar":
                {
                    break;
                }
            }
            CurrentSellableItems = PlayerSystem.Instance.Player.GetCurrentItemsFiltered(filterTypes, onlyMagicConsumables, onlyNonMagicConsumables);
            SelectionView.SetupItemDataView(CurrentSellableItems, ItemSelectionItemView.ActionType.Sell, false);
            ShowItemSelectionView(true);
        }

        public void SelectBackButton()
        {
            ShowItemSelectionView(false);
        }

        public void SelectLeaveShopButton()
        {
            ShopView.SafeSetActive(false);
            GameEvents.EnterTown();
        }

        public void SellPlayerItem(ItemDataSlot itemDataSlot)
        {
            PlayerSystem.Instance.Player.SellItem(itemDataSlot.ItemData.Name, itemDataSlot.Count, itemDataSlot.ItemID);
            CurrentGold.text = $"Gold: {PlayerSystem.Instance.Player.Gold}";
        }

        public void BuyPlayerItem(ItemDataSlot itemDataSlot)
        {
            PlayerSystem.Instance.Player.Gold -= itemDataSlot.ItemData.Cost * itemDataSlot.Count;
            PlayerSystem.Instance.Player.GainItem(itemDataSlot.ItemData.Name, itemDataSlot.Count, itemDataSlot.Rarity, itemDataSlot.Random);
            CurrentGold.text = $"Gold: {PlayerSystem.Instance.Player.Gold}";
        }

    }
}