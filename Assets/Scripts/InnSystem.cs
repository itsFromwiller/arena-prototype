using UnityEngine;
using TMPro;
using Arena.Player;
using Arena.Items;
using System.Collections.Generic;

namespace Arena.Inn
{
    public class InnSystem : MonoBehaviour
    {
        public static InnSystem Instance;

        public GameObject InnView;

        public GameObject MainOptionView;
        public GameObject EquipmentListView;
        public GameObject StudyView;
        public SelectionView EquipmentSelectionListView;
        public SelectionView StudySelectionListView;
        public WornEquipmentView WornEquipmentView;

        private void Awake()
        {
            Instance = this;
            InnView.SafeSetActive(false);
        }
        private void OnEnable()
        {
            GameEvents.OnEnterInn += OnEnterInn;
        }

        private void OnDisable()
        {
            GameEvents.OnEnterInn -= OnEnterInn;
        }

        void OnEnterInn()
        {
            // Populate view
            InnView.SafeSetActive(true);
            MainOptionView.SafeSetActive(true);
            WornEquipmentView.gameObject.SafeSetActive(false);
            EquipmentListView.SafeSetActive(false);
            StudyView.SafeSetActive(false);
        }

        public void SelectLeaveInnButton()
        {
            InnView.SafeSetActive(false);
            GameEvents.EnterTown();
        }

        public void ClickRest()
        {
            GameEvents.RestAtInn();
        }

        public void EquipItem(SlotType slotType, ItemDataSlot itemDataSlot)
        {
            if (itemDataSlot.ItemData.SlotType == SlotType.None)
            {
                PlayerSystem.Instance.Player.UnequipSlot(slotType);
                // If our slot type is the OneHand or OffHand, we might
                // have tried to unequip a TwoHand weapon, so do that
                // as well
                if (slotType == SlotType.OffHand || slotType == SlotType.OneHand)
                {
                    PlayerSystem.Instance.Player.UnequipSlot(SlotType.TwoHand);
                }
            }
            else
            {
                PlayerSystem.Instance.Player.EquipItem(itemDataSlot.ItemData.Name, itemDataSlot.ItemID);
            }
            SelectExitEquipmentList();
            SelectChangeEquipment();
        }

        public void StudyItem(ItemDataSlot itemDataSlot)
        {
            if (itemDataSlot.ItemData.ActionType == ActionType.LearnSkill)
            {
                PlayerSystem.Instance.Player.LearnSkill(itemDataSlot.ItemData.ActionValue);
                PlayerSystem.Instance.Player.UseItem(itemDataSlot.ItemData.Name, 1);
            }
            SelectStudy();
        }

        public void SelectExitStudyList()
        {
            MainOptionView.SafeSetActive(true);
            StudyView.SafeSetActive(false);
        }

        public void SelectChangeEquipment()
        {
            MainOptionView.SafeSetActive(false);
            WornEquipmentView.gameObject.SafeSetActive(true);
            WornEquipmentView.Setup();
        }

        public void SelectExitChangeEquipment()
        {
            MainOptionView.SafeSetActive(true);
            WornEquipmentView.gameObject.SafeSetActive(false);
        }

        public void SelectExitEquipmentList()
        {
            WornEquipmentView.gameObject.SafeSetActive(true);
            EquipmentListView.SafeSetActive(false);
        }

        public void SelectStudy()
        {
            // Build list of books to study
            var itemDataSlots = PlayerSystem.Instance.Player.GetStudyItems();
            StudySelectionListView.SetupItemDataView(itemDataSlots, ItemSelectionItemView.ActionType.Study, false, 1.0);

            MainOptionView.SafeSetActive(false);
            StudyView.SafeSetActive(true);
        }

        public void ShowEquipmentList(SlotType slotType, bool hasEquippedItem)
        {
            var filter = new HashSet<SlotType>();
            filter.Add(slotType);
            if (slotType == SlotType.OneHand)
            {
                filter.Add(SlotType.TwoHand);
            }
            var itemDataSlots = PlayerSystem.Instance.Player.GetEquipmentFiltered(filter);
            if (hasEquippedItem)
            {
                // Add an special item to the start of the list, of SlotType None and named "Remove equipment",
                // which will be used to remove equipment if selected
                itemDataSlots.Insert(0, new ItemDataSlot(new ItemData() { SlotType = SlotType.None, Name = "Remove equipment" }, 1, null, null, 0));
            }
            EquipmentSelectionListView.SetupItemDataViewForEquipment(itemDataSlots, slotType, ItemSelectionItemView.ActionType.Equip, false);

            WornEquipmentView.gameObject.SafeSetActive(false);
            EquipmentListView.SafeSetActive(true);
        }

    }
}