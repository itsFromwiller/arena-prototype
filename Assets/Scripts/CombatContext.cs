using Arena.Enemies;
using Arena.Items;
using Arena.Player;

namespace Arena.Combat
{
    public class CombatContext
    {
        // Setup at start
        public PlayerEntity Player;
        public EnemyEntity Enemy;
        // Reset each round
        public int DamageDealt;
        public int HealingAmount;
        public int RestoreAmount;
        public int StealAmount;
        public SkillData SkillUsed;
        public ItemData ItemUsed;
        public bool EnemyWasDead;
        public EnemyActionData EnemyActionUsed;
    }
}