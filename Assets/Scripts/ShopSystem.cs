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
        public TextMeshProUGUI ShopInfo;
        public LogView LogView;
        public SelectionView SelectionView;
        public GameObject MainOptions;
        public GameObject BackButton;
        public TextMeshProUGUI BackButtonText;
        public TextMeshProUGUI CurrentGold;
        public int ItemCountInBazaar = 10;
        private string ShopType;
        private List<ItemDataSlot> CurrentSellableItems = new ();
        private List<ItemDataSlot> CurrentBuyableItems = new();
        private List<ItemDataSlot> CurrentBazaarItems = new();
        private double ShopSellPercentage = 0.5;

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
            LogView.Clear();
            switch (shopType)
            {
                case "Bazaar":
                {
                    ShopName.text = "Bazaar";
                    ShopInfo.text = "An ever-changing market where new <color=green>Uncommon</color> items appear after every dungeon run! All items can be sold, but for less than a specialty shop.";
                    break;
                }
                case "Weapons":
                {
                    ShopName.text = "Weapons Shop";
                    ShopInfo.text = "An essential shop where you are able to buy and sell weapons ideal for physical attacks.";
                    break;
                }
                case "Armor":
                {
                    ShopName.text = "Armor Shop";
                    ShopInfo.text = "A trusty shop where all manner of Armor and Shields can be bought and sold.";
                    break;
                }
                case "Magic":
                {
                    ShopName.text = "Magic Shop";
                    ShopInfo.text = "A wonderous shop where magical weapons, items, and books can be bought and sold.";
                    break;
                }
                case "Items":
                {
                    ShopName.text = "Item Shop";
                    ShopInfo.text = "A peculiar shop that buys and sells various consumable items. They also purchase random materials dropped in a dungeon.";
                    break;
                }
            }
            CurrentGold.text = $"Your Gold: {PlayerSystem.Instance.Player.Gold}";
        }

        void OnEnterDungeonRoom(DungeonRoomEntity entity)
        {
            // Clear our bazaar once we enter a dungeon room
            CurrentBazaarItems.Clear();
            GameEvents.RequestSaveGame();
        }

        void ShowItemSelectionView(bool isShown)
        {
            BackButton.SafeSetActive(isShown);
            SelectionView.gameObject.SafeSetActive(isShown);
            MainOptions.SafeSetActive(!isShown);
            BackButton.SafeSetActive(isShown);
            LogView.gameObject.SafeSetActive(isShown);
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
                        GameEvents.RequestSaveGame();
                    }
                    CurrentBuyableItems = CurrentBazaarItems;
                    break;
                }
            }
            if (CurrentBuyableItems.Count == 0)
            {
                CurrentBuyableItems = ItemSystem.Instance.GetShopItemsFiltered(filterTypes, onlyMagicConsumables, onlyNonMagicConsumables, PlayerSystem.Instance.Player.Level + 5);
            }
            SelectionView.SetupItemDataView(CurrentBuyableItems, ItemSelectionItemView.ActionType.Buy, false, 1.0);
            ShowItemSelectionView(true);
            BackButtonText.text = "Stop Buying";
        }

        public void SelectSellButton()
        {
            // Set up item view, filtered to the type of items the shop handles
            HashSet<ItemType> filterTypes = new HashSet<ItemType>();
            bool onlyMagicConsumables = false;
            bool onlyNonMagicConsumables = false;
            ShopSellPercentage = 0.5;

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
                    ShopSellPercentage = 0.4;
                    break;
                }
            }
            CurrentSellableItems = PlayerSystem.Instance.Player.GetCurrentItemsFiltered(filterTypes, onlyMagicConsumables, onlyNonMagicConsumables);
            SelectionView.SetupItemDataView(CurrentSellableItems, ItemSelectionItemView.ActionType.Sell, false, ShopSellPercentage);
            ShowItemSelectionView(true);
            BackButtonText.text = "Stop Selling";
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
            itemDataSlot.ItemData.IsStackable();
            int gold = PlayerSystem.Instance.Player.SellItem(itemDataSlot.ItemData.Name, itemDataSlot.Count, itemDataSlot.ItemID, ShopSellPercentage);
            if (itemDataSlot.ItemData.IsStackable())
            {
                LogView.AddMessage($"Sold {ItemSystem.Instance.BuildName(itemDataSlot)} [x{itemDataSlot.Count}] for <color=yellow>{gold}g</color>");
            }
            else
            {
                LogView.AddMessage($"Sold {ItemSystem.Instance.BuildName(itemDataSlot)} for <color=yellow>{gold}g</color>");
            }
            CurrentGold.text = $"Your Gold: {PlayerSystem.Instance.Player.Gold}";
        }

        public void BuyPlayerItem(ItemDataSlot itemDataSlot)
        {
            int goldCost = itemDataSlot.GetBuyCost() * itemDataSlot.Count;
            PlayerSystem.Instance.Player.Gold -= goldCost;
            PlayerSystem.Instance.Player.GainItem(itemDataSlot.ItemData.Name, itemDataSlot.Count, itemDataSlot.Rarity, itemDataSlot.Random);

            if (itemDataSlot.ItemData.IsStackable())
            {
                LogView.AddMessage($"Bought {ItemSystem.Instance.BuildName(itemDataSlot)} [x{itemDataSlot.Count}] for <color=yellow>{goldCost}g</color>");
            }
            else
            {
                LogView.AddMessage($"Bought {ItemSystem.Instance.BuildName(itemDataSlot)} for <color=yellow>{goldCost}g</color>");
            }

            CurrentGold.text = $"Your Gold: {PlayerSystem.Instance.Player.Gold}";
        }

    }
}