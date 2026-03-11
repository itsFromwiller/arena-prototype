using UnityEngine;
using TMPro;
using Arena.Items;
using Arena.Player;
using Arena.Inn;

namespace Arena
{
    public class EquipmentSlotItem : MonoBehaviour
    {
        public TextMeshProUGUI ItemName;
        public TextMeshProUGUI ItemDescription;
        private SlotType SlotType;
        bool hasEquippedItem;

        public void Setup(SlotType slotType)
        {
            SlotType = slotType;
            var player = PlayerSystem.Instance.Player;
            ItemDataSlot itemDataSlot = null;

            switch (slotType)
            {
                case SlotType.OneHand:
                {
                    if (!player.TryGetEquipmentInSlot(SlotType.OneHand, out itemDataSlot))
                    {
                        if (!player.TryGetEquipmentInSlot(SlotType.TwoHand, out itemDataSlot))
                        {
                            ItemName.text = "Main-hand (empty)";
                        }
                    }
                    break;
                }
                case SlotType.OffHand:
                {
                    if (!player.TryGetEquipmentInSlot(SlotType.OffHand, out itemDataSlot))
                    {
                        if (!player.TryGetEquipmentInSlot(SlotType.TwoHand, out itemDataSlot))
                        {
                            ItemName.text = "Off-hand (empty)";
                        }
                    }
                    break;
                }
                case SlotType.Head:
                {
                    if (!player.TryGetEquipmentInSlot(SlotType.Head, out itemDataSlot))
                    {
                        ItemName.text = "Helmet (empty)";
                    }
                    break;
                }
                case SlotType.Body:
                {
                    if (!player.TryGetEquipmentInSlot(SlotType.Body, out itemDataSlot))
                    {
                        ItemName.text = "Armor (empty)";
                    }
                    break;
                }
                case SlotType.Cape:
                {
                    if (!player.TryGetEquipmentInSlot(SlotType.Cape, out itemDataSlot))
                    {
                        ItemName.text = "Cape (empty)";
                    }
                    break;
                }
            }
            hasEquippedItem = itemDataSlot != null;
            if (itemDataSlot != null)
            {

                ItemName.text = ItemSystem.Instance.BuildName(itemDataSlot);
                if (slotType == SlotType.OffHand && itemDataSlot.ItemData.SlotType == SlotType.TwoHand)
                {
                    ItemDescription.text = "Both hands hold this";
                }
                else
                {
                    ItemDescription.text = ItemSystem.Instance.BuildDescription(itemDataSlot);
                }
            }
            else
            {
                ItemDescription.text = "";
            }
        }

        public void SelectAction()
        {
            InnSystem.Instance.ShowEquipmentList(SlotType, hasEquippedItem);
        }
    }
}
