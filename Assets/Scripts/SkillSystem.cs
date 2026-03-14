using Arena.Core;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arena.Combat
{
    public class SkillSystem : MonoBehaviour
    {
        public static SkillSystem Instance;

        private Dictionary<string, SkillData> skillDatabase = new Dictionary<string, SkillData>();

        private void Awake()
        {
            Instance = this;
        }

        public void SetData(Dictionary<string, string> data)
        {
            var skillData = JsonConvert.DeserializeObject<List<SkillData>>(data["Skills"]);
            foreach (var dataItem in skillData)
            {
                if (!skillDatabase.TryAdd(dataItem.Name, dataItem))
                {
                    Debug.LogError($"Skill data couldn't be added, something already exists with its name: {dataItem.Name}");
                    continue;
                }
                dataItem.SkillType = EnumMap<SkillType>.GetValue(dataItem.SkillTypeName);
                dataItem.ActivationType = EnumMap<ActivationType>.GetValue(dataItem.ActivationTypeName);
                dataItem.TargetType = EnumMap<TargetType>.GetValue(dataItem.TargetTypeName);
                dataItem.ElementType = EnumMap<ElementType>.GetValue(dataItem.ElementTypeName);
            }
        }

        public void Init()
        {
        }

        public SkillData GetSkillData(string name)
        {
            if (skillDatabase.TryGetValue(name, out SkillData skillData))
            {
                return skillData;
            }
            return null;
        }

        public string BuildDescription(SkillData skillData)
        {
            string description;
            string value = "";
            if (skillData.SkillValue > 0)
            {
                value = skillData.SkillValue.ToString("N0");
            }
            else if (skillData.SkillPercentage > 0.0)
            {
                value = (skillData.SkillPercentage * 100.00).ToString("N0");
            }
            else if (skillData.SuccessRate > 0.0)
            {
                value = (skillData.SuccessRate * 100.00).ToString("N0");
            }

            if (skillData.RepeatTurns > 0)
            {
                description = string.Format(skillData.Description, value, skillData.RepeatTurns);
            }
            else
            {
                description = string.Format(skillData.Description, value);
            }
            return description;
        }
    }
}