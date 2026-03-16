using Arena.Combat;
using Arena.Inn;
using Arena.Items;
using Arena.Player;
using Arena.Shop;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Arena.Core;

namespace Arena.Requests
{

    public class RequestsView : MonoBehaviour
    {
        public ScrollRect ScrollView;
        public GameObjectPoolManager RequestViewPoolManager;
        public Dictionary<int, RequestSelectionItemView> RequestsInView = new Dictionary<int, RequestSelectionItemView>();

        private void HideViews()
        {
            foreach (var view in RequestsInView.Values)
            {
                RequestViewPoolManager.ReturnToPool(view.gameObject);
            }
            RequestsInView.Clear();
        }

        public void SetupRequestDataView(List<RequestData> requestDataList)
        {
            HideViews();

            for (int i = 0; i < requestDataList.Count; ++i)
            {
                var requestData = requestDataList[i];

                RequestSelectionItemView requestView;
                GameObject pooledObject = RequestViewPoolManager.GetPooledObject();
                pooledObject.transform.SetParent(ScrollView.content);
                if (!pooledObject.TryGetComponent(out requestView))
                {
                    Debug.LogError("RequestsView not set up correctly, missing RequestSelectionItemView in its template");
                    break;
                }

                RequestsInView.Add(i, requestView);
                bool isActive = PlayerSystem.Instance.Player.HasRequest(requestData.Id);
                requestView.Setup(this, i, requestData, isActive ? RequestSelectionItemView.ActionType.Complete : RequestSelectionItemView.ActionType.Accept);
                requestView.gameObject.SafeSetActive(true);
                requestView.Background.color = i % 2 == 0 ? new Color(0, 0, 0, 0) : new Color(0, 0, 0, 0.1f);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollView.content);
        }

        public void CompleteRequest(int id)
        {
            PlayerSystem.Instance.Player.CompleteRequest(RequestsInView[id].RequestData);
            RequestsInView[id].Setup(this, id, RequestsInView[id].RequestData, RequestSelectionItemView.ActionType.Completed);
        }

        public void AcceptRequest(int id)
        {
            PlayerSystem.Instance.Player.AcceptRequest(RequestsInView[id].RequestData);
            RequestsInView[id].Setup(this, id, RequestsInView[id].RequestData, RequestSelectionItemView.ActionType.Complete);
        }

        public void SelectBackButton()
        {
            // We are a child of the main dialog, should fix this as some point...
            transform.parent.gameObject.SafeSetActive(false);
            GameEvents.EnterTavern();
        }
    }
}