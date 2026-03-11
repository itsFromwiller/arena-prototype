using UnityEngine;
using TMPro;
using Arena.Player;
using UnityEngine.UI;

namespace Arena.Tavern
{
    public class GossipView : MonoBehaviour
    {
        public TextMeshProUGUI GossipText;
        public ScrollRect ScrollRect;

        public void Setup()
        {
            GossipText.text = "";
        }

        public void SelectListenButton()
        {
            if (!string.IsNullOrEmpty(GossipText.text))
            {
                GossipText.text += "\n\n";
            }
            var gossipData = TavernSystem.Instance.GetNextGossip();
            GossipText.text += gossipData.GossipText;

            if (gossipData.GossipEffect == GossipEffect.UnlockDungeon)
            {
                PlayerSystem.Instance.Player.UnlockDungeon(gossipData.EffectValue);
                GossipText.text += $"\n\n<color=yellow>You've discovered a new Dungeon: </color>{gossipData.EffectValue}";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);
            ScrollRect.verticalNormalizedPosition = 0;
        }

        public void SelectBackButton()
        {
            gameObject.SafeSetActive(false);
            GameEvents.EnterTavern();
        }

    }
}
