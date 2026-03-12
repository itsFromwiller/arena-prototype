namespace Arena.Enemies
{

//    public enum ActionType
//    {
//        Random,
//        Conditional
//    }

    public enum ConditionType
    {
        None,
        HPUnderPercent,
        TurnCount,
        CombatStart,
        Death,
        TakeDamage
    }

    public class EnemyActionData
    {
        public string Name;
//        public string ActionTypeName;
//        public ActionType ActionType; // Parsed at runtime, not from data
        public double ChanceToHit = 0;
        public int MPCost = 0;
        public string FailedText;
        public string SuccessText;
        public string HealText;
        public string SkillToUseName;
        public double DamageMultiplier = 0.0;
        public double HealAmount = 0.0;
        public int BuffTurns = 0;
        public double DefenseMultiplier = 0.0;
        public int CooldownTurns = 0;
        public string ConditionName = "None";
        public ConditionType ConditionType; // Parsed at runtime, not from data
        public int ConditionValue = 0;
    }
}