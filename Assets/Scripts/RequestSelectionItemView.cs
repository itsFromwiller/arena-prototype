using UnityEngine;
using TMPro;
using Arena.Items;
using UnityEngine.UI;
using Arena.Player;

namespace Arena.Requests
{
    public class RequestSelectionItemView : MonoBehaviour
    {
        public enum ActionType
        {
            Accept,
            Complete,
            Completed,
        }

        public TextMeshProUGUI RequestNameText;
        public TextMeshProUGUI RequestDescriptionText;
        public TextMeshProUGUI ActionText;
        public GameObject ActionButton;
        public Image Background;
        public RequestData RequestData;
        public TextMeshProUGUI RewardGoldText;
        public TextMeshProUGUI RewardXPText;
        public TextMeshProUGUI RewardItemText;

        private ActionType SelectActionType;
        private RequestsView RequestsView;
        private int Id;

        public void Setup(RequestsView requestsView, int id, RequestData requestData, ActionType selectActionType)
        {
            Id = id;
            SelectActionType = selectActionType;
            RequestsView = requestsView;
            RequestData = requestData;

            RequestNameText.text = RequestSystem.Instance.BuildName(requestData);
            RequestDescriptionText.text = RequestSystem.Instance.BuildDescription(requestData);
            
            RewardXPText.gameObject.SafeSetActive(requestData.RewardXP > 0);
            if (requestData.RewardXP > 0)
            {
                RewardXPText.text = $"<color=purple>{requestData.RewardXP} XP</color>";
            }
            RewardGoldText.gameObject.SafeSetActive(requestData.RewardGold > 0);
            if (requestData.RewardGold > 0)
            {
                RewardGoldText.text = $"<color=yellow>{requestData.RewardGold} g</color>";
            }
            RewardItemText.gameObject.SafeSetActive(!string.IsNullOrEmpty(requestData.RewardItem));
            if (!string.IsNullOrEmpty(requestData.RewardItem))
            {
                ItemData itemData = ItemSystem.Instance.GetItemData(requestData.RewardItem);
                // Stackable items are all common
                if (itemData.IsStackable())
                {
                    RewardItemText.text = $"{requestData.RewardItem}";
                }
                // Non stackable items are equipment, and requests give out Uncommon rewards
                else
                {
                    RewardItemText.text = $"<color=green>{requestData.RewardItem}</color>";
                }
            }

            Button button = ActionButton.GetComponent<Button>();
            switch (selectActionType)
            {
                case ActionType.Accept:
                {
                    ActionText.text = "Accept";
                    ActionButton.SafeSetActive(true);
                    button.interactable = true;
                    break;
                }
                case ActionType.Complete:
                {
                    ActionText.text = "Turn In";
                    ActionButton.SafeSetActive(true);
                    button.interactable = PlayerSystem.Instance.Player.CanCompleteRequest(requestData);
                    break;
                }
                case ActionType.Completed:
                {
                    ActionText.text = "Completed";
                    ActionButton.SafeSetActive(true);
                    button.interactable = false;
                    break;
                }
            }
        }

        public void SelectAction()
        {
            switch (SelectActionType)
            {
                case ActionType.Accept:
                {
                    RequestsView.AcceptRequest(Id);
                    break;
                }
                case ActionType.Complete:
                {
                    RequestsView.CompleteRequest(Id);
                    break;
                }
            }
        }
    }
}
