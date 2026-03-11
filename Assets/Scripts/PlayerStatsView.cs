using Arena.Player;
using TMPro;
using UnityEngine;

namespace Arena
{
    public class PlayerStatsView : MonoBehaviour
    {
        public TextMeshProUGUI StrValue;
        public TextMeshProUGUI IntValue;
        public TextMeshProUGUI EndValue;
        public TextMeshProUGUI AgiValue;
        public TextMeshProUGUI AttackValue;
        public TextMeshProUGUI MAttackValue;
        public TextMeshProUGUI DefenseValue;
        public TextMeshProUGUI MDefenseValue;
        public TextMeshProUGUI SpeedValue;

        public void Setup()
        {
            var player = PlayerSystem.Instance.Player;
            StrValue.text = player.Strength.ToString();
            IntValue.text = player.Intelligence.ToString();
            EndValue.text = player.Endurance.ToString();
            AgiValue.text = player.Agility.ToString();
            AttackValue.text = player.CalculatedAttack().ToString();
            MAttackValue.text = player.CalculatedMAttack().ToString();
            DefenseValue.text = player.CalculatedDefense().ToString();
            MDefenseValue.text = player.CalculatedMDefense().ToString();
            SpeedValue.text = player.CalculatedSpeed().ToString();
        }
    }
}
