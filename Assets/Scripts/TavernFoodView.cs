using Arena.Combat;
using Arena.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Arena.Tavern
{
    public class TavernFoodView: MonoBehaviour
    {
        public Button HPFoodButton;
        public Button MPFoodButton;
        public Button AttackFoodButton;
        public Button DefenseFoodButton;
        public GameObject AlreadyAteText;
        private bool HasActiveFood = false;

        public void Setup()
        {
            var player = PlayerSystem.Instance.Player;

            // Enable buttons if we can pick a meal.
            string[] foodSkillNames = { "FoodMaxHP", "FoodMaxMP", "FoodAttack", "FoodDefense" };
            SkillEntity activeSkillEntity = null;
            foreach (string foodSkillName in foodSkillNames)
            {
                var foodSkillData = SkillSystem.Instance.GetSkillData(foodSkillName);
                if (player.TryGetActiveSkillEntity(foodSkillData, out activeSkillEntity))
                {
                    break;
                }
            }
            HasActiveFood = activeSkillEntity != null;
            HPFoodButton.interactable = !HasActiveFood || activeSkillEntity.SkillData.SkillType == SkillType.ModifyMaxHP;
            MPFoodButton.interactable = !HasActiveFood || activeSkillEntity.SkillData.SkillType == SkillType.ModifyMaxMP;
            AttackFoodButton.interactable = !HasActiveFood || activeSkillEntity.SkillData.SkillType == SkillType.ModifyAllAttack;
            DefenseFoodButton.interactable = !HasActiveFood || activeSkillEntity.SkillData.SkillType == SkillType.ModifyAllDefense;
            AlreadyAteText.SafeSetActive(HasActiveFood);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void SelectFood(string foodSkillName)
        {
            if (HasActiveFood)
            {
                return;
            }

            SkillData skillData = SkillSystem.Instance.GetSkillData(foodSkillName);
            if (skillData == null)
            {
                Debug.LogError($"Tavern: Food skill does not exist: {foodSkillName}");
                return;
            }
            
            PlayerSystem.Instance.Player.AddActiveSkillEntity(new SkillEntity(skillData, SkillLifetime.Dungeon, 1, false));
            if (skillData.SkillType == SkillType.ModifyMaxHP)
            {
                PlayerSystem.Instance.Player.HP = PlayerSystem.Instance.Player.MaxHP;
                GameEvents.PlayerMaxHPChanged();
            }
            else if (skillData.SkillType == SkillType.ModifyMaxMP)
            {
                PlayerSystem.Instance.Player.MP = PlayerSystem.Instance.Player.MaxMP;
                GameEvents.PlayerMaxMPChanged();
            }
            Setup();
        }

        public void SelectBackButton()
        {
            gameObject.SafeSetActive(false);
            GameEvents.EnterTavern();
        }
    }
}
