using UnityEngine;
using TMPro;
using Arena.Items;
using UnityEngine.UI;

namespace Arena
{
    public class ItemSelectionItemView : MonoBehaviour
    {
        public enum ActionType
        {
            Buy,
            Sell,
            Use,
            Equip,
            Study,
            None,
        }

        public TextMeshProUGUI ItemNameText;
        public TextMeshProUGUI ItemCostText;
        public TextMeshProUGUI ActionText;
        public TextMeshProUGUI ItemDescriptionText;
        public GameObject ActionButton;
        public Image Background;
        public ItemDataSlot ItemDataSlot;

        private ActionType SelectActionType;
        private SelectionView SelectionView;
        private int Id;
        private double ShopSellPercentage = 0.5;

        public void Setup(SelectionView selectionView, int id, ItemDataSlot itemDataSlot, ActionType selectActionType, double shopSellPercentage)
        {
            Id = id;
            SelectActionType = selectActionType;
            SelectionView = selectionView;
            ItemDataSlot = itemDataSlot;
            ShopSellPercentage = shopSellPercentage;

            if (itemDataSlot.ItemData.IsStackable())
            {
                ItemNameText.text = $"{ItemSystem.Instance.BuildName(itemDataSlot)} [x{itemDataSlot.Count}]";
            }
            else
            {
                ItemNameText.text = ItemSystem.Instance.BuildName(itemDataSlot);
            }
            ItemDescriptionText.text = ItemSystem.Instance.BuildDescription(itemDataSlot);

            switch (selectActionType)
            {
                case ActionType.Buy:
                {
                    ActionText.text = "Buy";
                    ActionButton.SafeSetActive(true);
                    ItemCostText.text = string.Format("{0:N0} g", itemDataSlot.GetBuyCost() * itemDataSlot.Count);
                    ItemCostText.gameObject.SafeSetActive(true);
                    break;
                }
                case ActionType.Sell:
                {
                    ActionText.text = "Sell";
                    ActionButton.SafeSetActive(true);
                    ItemCostText.text = string.Format("{0:N0} g", itemDataSlot.GetSellCost(shopSellPercentage) * itemDataSlot.Count);
                    ItemCostText.gameObject.SafeSetActive(true);
                    break;
                }
                case ActionType.Use:
                {
                    ActionText.text = "Use";
                    ActionButton.SafeSetActive(true);
                    ItemCostText.gameObject.SafeSetActive(false);
                    break;
                }
                case ActionType.Equip:
                {
                    ActionText.text = "Pick";
                    ActionButton.SafeSetActive(true);
                    ItemCostText.gameObject.SafeSetActive(false);
                    break;
                }
                case ActionType.Study:
                {
                    ActionText.text = "Study";
                    ActionButton.SafeSetActive(true);
                    ItemCostText.gameObject.SafeSetActive(false);
                    break;
                }
                case ActionType.None:
                {
                    ActionButton.SafeSetActive(false);
                    ItemCostText.gameObject.SafeSetActive(false);
                    break;
                }
            }
        }

        public void SelectAction()
        {
            switch (SelectActionType)
            {
                case ActionType.Buy:
                {
                    SelectionView.BuyItem(Id);
                    break;
                }
                case ActionType.Sell:
                {
                    SelectionView.SellItem(Id);
                    break;
                }
                case ActionType.Use:
                {
                    SelectionView.UseItem(Id);
                    break;
                }
                case ActionType.Equip:
                {
                    SelectionView.EquipItem(Id);
                    break;
                }
                case ActionType.Study:
                {
                    SelectionView.StudyItem(Id);
                    break;
                }
            }
        }
    }
}
