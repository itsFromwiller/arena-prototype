using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arena.Enemies;
using Arena.Player;
using Arena.Combat;

namespace Arena
{
    public class CombatStatsView : MonoBehaviour
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI LevelText;
        public GameObject XPBar;
        public Image XPBarFill;
        public Image HPBar;
        public TextMeshProUGUI HPText;
        public Image MPBar;
        public TextMeshProUGUI MPText;
        public bool IsPlayer;

        private int HP;
        private int MaxHP;
        private int MP;
        private int MaxMP;
        private int XP;
        private int MaxXP;
        private bool isDirty;

        private void Awake()
        {
            // We don't destroy these, so they always want to
            // listen to these changes
            GameEvents.OnEnemySpawned += HandleEnemySpawned;
            GameEvents.OnEnemyStateUpdated += HandleEnemyStateUpdated;
            GameEvents.OnPlayerSpawned += HandlePlayerSpawned;
            GameEvents.OnPlayerHPChanged += HandlePlayerUpdated;
            GameEvents.OnPlayerMaxHPChanged += HandlePlayerUpdated;
            GameEvents.OnPlayerMPChanged += HandlePlayerUpdated;
            GameEvents.OnPlayerMaxMPChanged += HandlePlayerUpdated;
            GameEvents.OnPlayerLevelChanged += HandlePlayerXPOrLevelUpdated;
            GameEvents.OnGetXP += HandlePlayerXPOrLevelUpdated;
            GameEvents.OnPlayerDamaged += OnPlayerCombatStateUpdated;
            GameEvents.OnPlayerHealed += OnPlayerCombatStateUpdated;
            GameEvents.OnPlayerRestoreMP += OnPlayerCombatStateUpdated;
            GameEvents.OnPlayerStealMP += OnPlayerCombatStateUpdated;
            GameEvents.OnEnemyDamaged += OnEnemyCombatStateUpdated;
            GameEvents.OnEnemyHealed += OnEnemyCombatStateUpdated;
            GameEvents.OnEnemyRestoreMP += OnEnemyCombatStateUpdated;
            GameEvents.OnEnemyStealMP += OnEnemyCombatStateUpdated;
        }

        private void OnEnable()
        {
            if (PlayerSystem.Instance != null)
            {
                HandlePlayerSpawned();
            }
        }

        private void LateUpdate()
        {
            if (!isDirty)
            {
                return;
            }
            isDirty = false;

            float fillAmount = (float)HP / MaxHP;
            HPBar.fillAmount = fillAmount;
            HPText.text = $"{HP} / {MaxHP}";
            MPBar.fillAmount = (float) MP / MaxMP;
            MPText.text = $"{MP} / {MaxMP}";
            if (IsPlayer && MaxXP > 0)
            {
                XPBarFill.fillAmount = (float)XP / MaxXP;
            }
        }

        private void HandleEnemySpawned(EnemyEntity enemyEntity)
        {
            if (IsPlayer)
            {
                return;
            }
            NameText.text = enemyEntity.Data.Name;
            LevelText.text = $"Level {enemyEntity.Level}";
            XPBar.SafeSetActive(false);
            HandleEnemyStateUpdated(enemyEntity);
        }

        private void HandleEnemyStateUpdated(EnemyEntity enemyEntity)
        {
            if (IsPlayer)
            {
                return;
            }
            UpdateData(enemyEntity);
        }

        private void HandlePlayerSpawned()
        {
            if (!IsPlayer)
            {
                return;
            }
            NameText.text = PlayerSystem.Instance.Player.Name;
            LevelText.text = $"Level {PlayerSystem.Instance.Player.Level}";
            XPBar.SafeSetActive(true);
            UpdateXPData();
            HandlePlayerUpdated();
        }

        private void HandlePlayerXPOrLevelUpdated(int ignoredAmount)
        {
            if (!IsPlayer)
            {
                return;
            }
            UpdateXPData();
            UpdateData(PlayerSystem.Instance.Player);
        }

        private void UpdateXPData()
        {
            // XP will be calculated from XP start and end for current level,
            // normalized.
            var player = PlayerSystem.Instance.Player;
            int playerLevel = player.Level;
            int playerXP = player.XP;
            int startXPForLevel = playerLevel == 1 ? 0 : player.GetMaxXPForLevel(playerLevel - 1);
            int endXPForLevel = player.GetMaxXPForLevel(playerLevel);
            // start = 0, end = 50, xp = 25
            // xp = 25, maxXP = 50
            // start = 50, end = 150, xp = 149
            // xp = 99, maxXP = 100
            XP = playerXP - startXPForLevel;
            MaxXP = endXPForLevel - startXPForLevel;
        }

        private void HandlePlayerUpdated()
        {
            if (!IsPlayer)
            {
                return;
            }
            UpdateData(PlayerSystem.Instance.Player);
        }

        private void OnPlayerCombatStateUpdated(CombatContext combatContext)
        {
            if (!IsPlayer)
            {
                return;
            }
            UpdateData(combatContext.Player);
        }

        private void OnEnemyCombatStateUpdated(CombatContext combatContext)
        {
            if (IsPlayer)
            {
                return;
            }
            UpdateData(combatContext.Enemy);
        }

        private void UpdateData(CombatEntity combatEntity) //int hp, int maxHP, int mp, int maxMP)
        {
            isDirty = true;
            HP = combatEntity.HP;
            MaxHP = combatEntity.MaxHP;
            MP = combatEntity.MP;
            MaxMP = combatEntity.MaxMP;
        }

    }
}
