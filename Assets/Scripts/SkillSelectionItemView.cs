using Arena.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arena
{
    public class SkillSelectionItemView : MonoBehaviour
    {
        public enum ActionType
        {
            Cast,
            None,
        }

        public TextMeshProUGUI SkillNameText;
        public TextMeshProUGUI SkillCostText;
        public TextMeshProUGUI ActionText;
        public TextMeshProUGUI SkillDescriptionText;
        public Button ActionButton;
        public Image Background;
        public SkillDataSlot SkillDataSlot;

        private ActionType SelectActionType;
        private SelectionView SelectionView;
        private int Id;

        public void Setup(SelectionView selectionView, int id, int currentMP, SkillDataSlot skillDataSlot, ActionType selectActionType)
        {
            Id = id;
            SelectActionType = selectActionType;
            SelectionView = selectionView;
            SkillDataSlot = skillDataSlot;

            SkillNameText.text = skillDataSlot.SkillData.Name;
            SkillCostText.text = string.Format("{0:N0} mp", skillDataSlot.SkillData.MPCost);
            SkillDescriptionText.text = SkillSystem.Instance.BuildDescription(skillDataSlot.SkillData);
            ActionButton.interactable = currentMP >= skillDataSlot.SkillData.MPCost;

            switch (selectActionType)
            {
                case ActionType.Cast:
                {
                    ActionText.text = "Cast";
                    ActionButton.gameObject.SafeSetActive(true);
                    SkillCostText.gameObject.SafeSetActive(skillDataSlot.SkillData.MPCost > 0);
                    break;
                }
                case ActionType.None:
                {
                    ActionButton.gameObject.SafeSetActive(false);
                    SkillCostText.gameObject.SafeSetActive(false);
                    break;
                }
            }
        }

        public void SelectAction()
        {
            switch (SelectActionType)
            {
                case ActionType.Cast:
                {
                    SelectionView.CastSkill(Id);
                    break;
                }
            }
        }
    }
}
