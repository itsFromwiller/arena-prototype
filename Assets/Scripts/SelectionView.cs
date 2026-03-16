using Arena.Combat;
using Arena.Inn;
using Arena.Items;
using Arena.Player;
using Arena.Shop;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Arena.Core;

namespace Arena
{

    public class SelectionView : MonoBehaviour
    {
        public ScrollRect ScrollView;
        public GameObjectPoolManager ItemViewPoolManager;
        public GameObjectPoolManager SkillViewPoolManager;
        public Dictionary<int, ItemSelectionItemView> ItemsInView = new Dictionary<int, ItemSelectionItemView>();
        public Dictionary<int, SkillSelectionItemView> SkillsInView = new Dictionary<int, SkillSelectionItemView>();

        public enum ViewType
        {
            None,
            Items,
            Skills,
            Equipment,
            StudyBooks,
        }
        private ViewType CurrentViewType = ViewType.None;
        private SlotType EquipmentSlotType = SlotType.None;
        private float LastItemViewPosition = 1;
        private float LastSkillViewPosition = 1;

        private void HideViews()
        {
            if (CurrentViewType == ViewType.Items)
            {
                LastItemViewPosition = ScrollView.verticalNormalizedPosition;
            }
            if (CurrentViewType == ViewType.Skills)
            {
                LastSkillViewPosition = ScrollView.verticalNormalizedPosition;
            }

            foreach (var view in SkillsInView.Values)
            {
                SkillViewPoolManager.ReturnToPool(view.gameObject);
            }
            SkillsInView.Clear();
            foreach (var view in ItemsInView.Values)
            {
                ItemViewPoolManager.ReturnToPool(view.gameObject);
            }
            ItemsInView.Clear();
        }

        public void SetupItemDataViewForEquipment(List<ItemDataSlot> itemDataSlots, SlotType equipmentSlotType, ItemSelectionItemView.ActionType selectionActionType, bool keepScrollPosition)
        {
            EquipmentSlotType = equipmentSlotType;
            SetupItemDataView(itemDataSlots, selectionActionType, keepScrollPosition, 1.0);
        }

        public void SetupItemDataView(List<ItemDataSlot> itemDataSlots, ItemSelectionItemView.ActionType selectionActionType, bool keepScrollPosition, double shopSellPercentage)
        {
            float cachedPosition = ScrollView.verticalNormalizedPosition;
            HideViews();
            CurrentViewType = ViewType.Items;

            for (int i = 0; i < itemDataSlots.Count; ++i)
            {
                var itemDataSlot = itemDataSlots[i];

                ItemSelectionItemView itemView;
                GameObject pooledObject = ItemViewPoolManager.GetPooledObject();
                pooledObject.transform.SetParent(ScrollView.content);
                if (!pooledObject.TryGetComponent(out itemView))
                {
                    Debug.LogError("SelectionView not set up correctly, missing ItemSelectionItemView in its template");
                    break;
                }

                ItemsInView.Add(i, itemView);
                itemView.Setup(this, i, itemDataSlot, selectionActionType, shopSellPercentage);
                itemView.gameObject.SafeSetActive(true);
                itemView.Background.color = i % 2 == 0 ? new Color(0, 0, 0, 0) : new Color(0, 0, 0, 0.1f);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollView.content);
        }

        public void SetupSkillDataView(List<SkillDataSlot> skillDataSlots, int currentMP, SkillSelectionItemView.ActionType selectionActionType)
        {
            HideViews();
            CurrentViewType = ViewType.Skills;

            for (int i = 0; i < skillDataSlots.Count; ++i)
            {
                var skillDataSlot = skillDataSlots[i];
                
                var skillView = SkillViewPoolManager.GetPooledObject<SkillSelectionItemView>();
                if (skillView == null)
                {
                    // We have a bad template, can't do much now.
                    break;
                }
                SkillsInView.Add(i, skillView);
                skillView.gameObject.transform.SetParent(ScrollView.content);
                skillView.Setup(this, i, currentMP, skillDataSlot, selectionActionType);
                skillView.gameObject.SafeSetActive(true);
                skillView.Background.color = i % 2 == 0 ? new Color(0,0,0,0) : new Color(0,0,0,0.1f);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollView.content);
        }
        
        public void UseItem(int id)
        {
            CombatSystem.Instance.ProcessPlayerItem(ItemsInView[id].ItemDataSlot);
        }

        public void SellItem(int id)
        {
            if (ItemsInView.Remove(id, out var view))
            {
                ShopSystem.Instance.SellPlayerItem(view.ItemDataSlot);
                ItemViewPoolManager.ReturnToPool(view.gameObject);
            }
        }

        public void BuyItem(int id)
        {
            ShopSystem.Instance.BuyPlayerItem(ItemsInView[id].ItemDataSlot);
        }

        public void CastSkill(int id)
        {
            CombatSystem.Instance.ProcessPlayerSkill(SkillsInView[id].SkillDataSlot);
        }

        public void EquipItem(int id)
        {
            InnSystem.Instance.EquipItem(EquipmentSlotType, ItemsInView[id].ItemDataSlot);
        }

        public void StudyItem(int id)
        {
            InnSystem.Instance.StudyItem(ItemsInView[id].ItemDataSlot);
        }
    }
}