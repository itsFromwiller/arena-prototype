using Newtonsoft.Json;

namespace Arena.Combat
{
    public class SkillDataSlot
    {
        public string Name;
        public int UseCount;
        public bool IsLearned;

        [JsonIgnore]
        public SkillData SkillData;

        public SkillDataSlot()
        { 
        }

        public SkillDataSlot(SkillData skillData, bool isLearned)
        {
            Name = skillData.Name;
            UseCount = 0;
            IsLearned = isLearned;
            SkillData = skillData;
        }
    }
}