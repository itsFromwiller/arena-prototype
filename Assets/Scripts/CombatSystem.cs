using Arena.Dungeon;
using Arena.Enemies;
using Arena.Items;
using Arena.Loot;
using Arena.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

namespace Arena.Combat
{
    public class CombatSystem : MonoBehaviour
    {
        public static CombatSystem Instance;

        public GameObject CombatView;
        public CombatLogView CombatLogView;
        public SelectionView CombatSelectionView;
        public GameObject CombatActionsView;
        public GameObject AfterCombatActionsView;
        public GameObject SummaryView;
        public GameObject LevelUpView;
        public TextMeshProUGUI ResultText;
        public TextMeshProUGUI SummaryText;
        public TextMeshProUGUI StatPointsRemainingText;
        public GameObject AfterCombatSkillButton;
        public GameObject AfterCombatItemButton;
        public GameObject AfterCombatContinueButton;
        public CombatStatsView EnemyCombatStatsView;
        public CombatStatsView PlayerCombatStatsView;
        public PlayerStatsView PlayerStatsView;
        public GameObject TreasureRoomView;
        public GameObject TreasureRoomChestButton;
        public TextMeshProUGUI TreasureRoomResultText;
        public GameObject FountainRoomView;
        public GameObject FountainRoomDrinkButton;
        public GameObject FountainRoomSearchButton;
        public TextMeshProUGUI FountainRoomResultText;

        private CombatContext CombatContext = new CombatContext();

        private List<SkillDataSlot> CurrentSkills = new List<SkillDataSlot>();
        private List<ItemDataSlot> CurrentUsableItems = new List<ItemDataSlot>();
        private bool hasLeftBattle = false;
        private int totalEarnedXP = 0;
        private int totalEarnedGold = 0;
        private List<LootResult> lootEarned = new List<LootResult>();

        public enum CombatSummary
        {
            Victory,
            Defeat,
            Escaped
        }

        private void Awake()
        {
            Instance = this;
            CombatView.SafeSetActive(false);

            CombatLogView.gameObject.SafeSetActive(false);
            CombatSelectionView.gameObject.SafeSetActive(false);
            CombatActionsView.SafeSetActive(false);
            AfterCombatActionsView.SafeSetActive(false);
            SummaryView.SafeSetActive(false);
            LevelUpView.SafeSetActive(false);
            EnemyCombatStatsView.gameObject.SafeSetActive(false);
            PlayerCombatStatsView.gameObject.SafeSetActive(false);
            TreasureRoomView.SafeSetActive(false);
            FountainRoomView.SafeSetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnEnemySpawned += HandleEnemySpawned;
            GameEvents.OnEnterCombat += HandleEnterCombat;
            GameEvents.OnStartCombat += HandleStartCombat;
            GameEvents.OnEnterDungeon += HandleEnterDungeon;
            GameEvents.OnFinishDungeon += HandleFinishDungeon;
            GameEvents.OnEnterDungeonRoom += HandleEnterDungeonRoom;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemySpawned -= HandleEnemySpawned;
            GameEvents.OnEnterCombat -= HandleEnterCombat;
            GameEvents.OnStartCombat -= HandleStartCombat;
            GameEvents.OnEnterDungeon -= HandleEnterDungeon;
            GameEvents.OnFinishDungeon -= HandleFinishDungeon;
            GameEvents.OnEnterDungeonRoom -= HandleEnterDungeonRoom;
        }

        public void HandleEnterDungeon(string dungeonName)
        {
            totalEarnedXP = 0;
            totalEarnedGold = 0;
            lootEarned.Clear();

            CombatLogView.gameObject.SafeSetActive(false);
            CombatLogView.ClearLog();
            CombatSelectionView.gameObject.SafeSetActive(false);
            CombatActionsView.SafeSetActive(false);
            AfterCombatActionsView.SafeSetActive(false);
            SummaryView.SafeSetActive(false);
            LevelUpView.SafeSetActive(false);
            EnemyCombatStatsView.gameObject.SafeSetActive(false);
            PlayerCombatStatsView.gameObject.SafeSetActive(false);
            TreasureRoomView.SafeSetActive(false);
            FountainRoomView.SafeSetActive(false);
        }

        void HandleEnemySpawned(EnemyEntity enemyEntity)
        {
            CombatContext.Enemy = enemyEntity;
            CombatContext.Player = PlayerSystem.Instance.Player;
            CombatContext.DamageDealt = 0;
            CombatContext.HealingAmount = 0;
            CombatContext.TurnCount = 0;
        }

        void HandleEnterCombat()
        {
            CombatView.SafeSetActive(true);
            EnemyCombatStatsView.gameObject.SafeSetActive(true);
            PlayerCombatStatsView.gameObject.SafeSetActive(true);
            CombatLogView.gameObject.SafeSetActive(true);
            CombatActionsView.SafeSetActive(true);
            hasLeftBattle = false;
        }

        void HandleEnterDungeonRoom(DungeonRoomEntity dungeonRoomEntity)
        {
            switch (dungeonRoomEntity.RoomTypeName)
            {
                case "Treasure":
                {
                    ShowTreasureRoom();
                    break;
                }
                case "Fountain":
                {
                    ShowFountainRoom();
                    break;
                }
            }
        }

        void HandleStartCombat()
        {
            ProcessStartCombatConditionalEnemyActions();

            if (CombatContext.Enemy.GetInitiative() > CombatContext.Player.GetInitiative())
            {
                GameEvents.EnemyGotInitiative(CombatContext.Enemy);
                ProcessEnemyTurn();
            }
        }

        public void HandleFinishDungeon()
        {
            CombatView.SafeSetActive(false);
        }

        private void ProcessStartOfPlayerTurn()
        {
            CombatLogView.gameObject.SafeSetActive(true);
            CombatSelectionView.gameObject.SafeSetActive(false);
            ResetContextForNewTurn();
            CombatContext.Player.ProcessActiveSkillLifetimes(SkillLifetime.Turn);
            // Have active skills do their effects when the player's
            // turn starts.
            ProcessActiveSkillEffects(CombatContext.Player);
            ProcessActiveSkillEffects(CombatContext.Enemy);
        }

        public void ProcessPlayerAttack()
        {
            ProcessStartOfPlayerTurn();

            if (CombatContext.Player.DidAttackSuccessfully())
            {
                CombatContext.Player.AttackTarget(CombatContext.Enemy, CombatContext);
            }
            else
            {
                GameEvents.PlayerMissed(CombatContext);
            }

            ProcessEndOfPlayerTurn();
        }

        public void ShowSkillList()
        {
            if (CurrentSkills.Count == 0)
            {
                CurrentSkills = CombatContext.Player.GetCurrentSkills();
            }
            CombatSelectionView.SetupSkillDataView(CurrentSkills, CombatContext.Player.MP, SkillSelectionItemView.ActionType.Cast);
            
            CombatSelectionView.gameObject.SafeSetActive(true);
            CombatLogView.gameObject.SafeSetActive(false);
            EnemyCombatStatsView.gameObject.SafeSetActive(true);
            TreasureRoomView.SafeSetActive(false);
            FountainRoomView.SafeSetActive(false);
        }

        public void ProcessPlayerSkill(SkillDataSlot skillDataSlot)
        {
            ProcessStartOfPlayerTurn();

            skillDataSlot.UseCount++;
            ProcessSkillForSource(CombatContext.Player, CombatContext.Enemy, skillDataSlot.SkillData);

            if (hasLeftBattle)
            {
                return;
            }

            ProcessEndOfPlayerTurn();
        }

        public void ProcessSkillForSource(CombatEntity source, CombatEntity enemy, SkillData skillData)
        {
            CombatContext.SkillUsed = skillData;
            if (source.DidUseSkillSuccessfully(CombatContext))
            {
                source.UseSkill(CombatContext);
                ProcessSkill(source, enemy, skillData);
            }
        }

        public void ProcessSkill(CombatEntity source, CombatEntity enemy, SkillData skillData)
        {
            switch (skillData.SkillType)
            {
                case SkillType.EscapeDungeon:
                {
                    PlayerEscaped();
                    return;
                }
                case SkillType.DealDamage:
                case SkillType.DealMDamage:
                {
                    source.AttackTargetWithSkill(enemy, skillData, CombatContext);
                    break;
                }
                case SkillType.Heal:
                {
                    if (skillData.SkillValue > 0)
                    {
                        CombatContext.HealingAmount = skillData.SkillValue;
                    }
                    else
                    {
                        CombatContext.HealingAmount = (int)(skillData.SkillPercentage * CombatContext.Player.MaxHP);
                    }
                    if (skillData.RepeatTurns > 0)
                    {
                        source.AddActiveSkillEntity(new SkillEntity(skillData, SkillLifetime.Turn, skillData.RepeatTurns, true));
                    }
                    source.Heal(CombatContext);
                    break;
                }
            }
        }

        public void ShowItemList()
        {
            CurrentUsableItems = CombatContext.Player.GetCurrentItems(true, false);
            CombatSelectionView.SetupItemDataView(CurrentUsableItems, ItemSelectionItemView.ActionType.Use, true, 1.0);

            CombatSelectionView.gameObject.SafeSetActive(true);
            CombatLogView.gameObject.SafeSetActive(false);
            EnemyCombatStatsView.gameObject.SafeSetActive(true);
            TreasureRoomView.SafeSetActive(false);
            FountainRoomView.SafeSetActive(false);
        }

        public void ProcessPlayerItem(ItemDataSlot itemDataSlot)
        {
            ProcessStartOfPlayerTurn();

            CombatContext.ItemUsed = itemDataSlot.ItemData;
            CombatContext.Player.UseItem(itemDataSlot.ItemData.Name, 1);
            switch (CombatContext.ItemUsed.ActionType)
            {
                case ActionType.Escape:
                {
                    PlayerEscaped();
                    return;
                }
                case ActionType.DamageHP:
                {
                    CombatContext.Player.AttackTargetWithItem(CombatContext.Enemy, itemDataSlot.ItemData, CombatContext);
                    break;
                }
                case ActionType.HealHP:
                {
                    int.TryParse(CombatContext.ItemUsed.ActionValue, out CombatContext.HealingAmount);
                    CombatContext.Player.Heal(CombatContext);
                    break;
                }
                case ActionType.RestoreMP:
                {
                    int.TryParse(CombatContext.ItemUsed.ActionValue, out CombatContext.RestoreAmount);
                    CombatContext.Player.RestoreMP(CombatContext);
                    break;
                }
                case ActionType.StealMP:
                {
                    int.TryParse(CombatContext.ItemUsed.ActionValue, out CombatContext.StealAmount);
                    CombatContext.Player.StealMP(CombatContext.Enemy, CombatContext);
                    break;
                }
                case ActionType.UseSkill:
                {
                    SkillData skillData = SkillSystem.Instance.GetSkillData(itemDataSlot.ItemData.ActionValue);
                    if (skillData != null)
                    {
                        CombatContext.SkillUsed = skillData;
                        ProcessSkill(CombatContext.Player, CombatContext.Enemy, skillData);
                    }
                    break;
                }
            }

            if (hasLeftBattle)
            {
                return;
            }

            ProcessEndOfPlayerTurn();
        }

        private void ProcessEndOfPlayerTurn()
        {
            // Enemies can get a last hit in with
            // damaged conditional actions
            ProcessDamagedConditionalEnemyActions();

            // Enemies will die first for loot
            if (CombatContext.Enemy.IsDead())
            {
                KillEnemy();
                return;
            }

            // Then we can process player death
            if (CombatContext.Player.IsDead())
            {
                KillPlayer();
                return;
            }

            if (CombatContext.Enemy.CanAttack())
            {
                ProcessEnemyTurn();
            }

            StartCoroutine(DisableThenReenableButtons());
        }

        private void ResetContextForNewTurn()
        {
            ResetContext();
            CombatContext.EnemyWasDead = CombatContext.Enemy.IsDead();
            CombatContext.TurnCount++;
        }

        public void ResetContext()
        {
            CombatContext.DamageDealt = 0;
            CombatContext.HealingAmount = 0;
            CombatContext.RestoreAmount = 0;
            CombatContext.StealAmount = 0;
            CombatContext.SkillUsed = null;
            CombatContext.ItemUsed = null;
            CombatContext.EnemyActionUsed = null;
            CombatContext.IsRepeatedAction = false;
        }

        public void ProcessEnemyTurn()
        {
            ResetContextForNewTurn();

            CombatContext.Enemy.ProcessActiveSkillLifetimes(SkillLifetime.Turn);
            CombatContext.Enemy.ProcessActionCooldowns();
            CombatContext.Enemy.ProcessBuffs();

            ProcessStartTurnConditionalEnemyActions();

            CombatContext.Enemy.PrepareForAttack();
            CombatContext.EnemyActionUsed = CombatContext.Enemy.ActionToPerform;
            CombatContext.SkillUsed = CombatContext.EnemyActionUsed != null ? SkillSystem.Instance.GetSkillData(CombatContext.EnemyActionUsed.SkillToUseName) : null;

            if (CombatContext.Enemy.DidAttackSuccessfully())
            {
                if (CombatContext.SkillUsed != null)
                {
                    CombatContext.Enemy.AttackTargetWithSkill(CombatContext.Player, CombatContext.SkillUsed, CombatContext);
                }
                else
                {
                    HandleEnemyActionUsed();
                }

                if (CombatContext.Player.IsDead())
                {
                    KillPlayer();
                }
            }
            else
            {
                if (CombatContext.SkillUsed != null)
                {
                    GameEvents.EnemySkillFailed(CombatContext);
                }
                else
                {
                    GameEvents.EnemyMissed(CombatContext);
                }
            }
        }

        public void ProcessActiveSkillEffects(CombatEntity target)
        {
            if (target.IsDead())
            {
                return;
            }

            foreach (var activeSkill in target.ActiveSkills)
            {
                if (activeSkill.IsExpired)
                {
                    continue;
                }
                if (activeSkill.SkillData.RepeatTurns == 0)
                {
                    continue;
                }
                CombatContext.SkillUsed = activeSkill.SkillData;
                CombatContext.IsRepeatedAction = true;
                CombatEntity source = activeSkill.SourceIsPlayer ? CombatContext.Player : CombatContext.Enemy;
                if (activeSkill.SkillData.SkillType == SkillType.DealDamage || activeSkill.SkillData.SkillType == SkillType.DealMDamage)
                {
                    source.AttackTargetWithSkill(target, activeSkill.SkillData, CombatContext);
                }
                else if (activeSkill.SkillData.SkillType == SkillType.Heal)
                {
                    CombatContext.HealingAmount = activeSkill.SkillData.SkillValue;
                    source.Heal(CombatContext);
                }
                ResetContext();
            }
        }

        public void ProcessStartTurnConditionalEnemyActions()
        {
            foreach (var action in CombatContext.Enemy.Data.ActionDataList)
            {
                if (action.ConditionType != ConditionType.None)
                {
                    if (CombatContext.Enemy.IsActionOnCooldown(action.Name))
                    {
                        continue;
                    }

                    bool actionTriggered = false;

                    switch (action.ConditionType)
                    {
                        case ConditionType.TurnCount:
                        {
                            if (action.ConditionValue % CombatContext.TurnCount == 0)
                            {
                                actionTriggered = true;
                            }
                            break;
                        }
                    }

                    if (actionTriggered)
                    {
                        ProcessConditionalEnemyAction(action);
                    }
                }
            }
        }

        public void ProcessDamagedConditionalEnemyActions()
        {
            foreach (var action in CombatContext.Enemy.Data.ActionDataList)
            {
                if (action.ConditionType != ConditionType.None)
                {
                    if (CombatContext.Enemy.IsActionOnCooldown(action.Name))
                    {
                        continue;
                    }

                    bool actionTriggered = false;
                    switch (action.ConditionType)
                    {
                        case ConditionType.TakeDamage:
                        {
                            actionTriggered = !CombatContext.Enemy.IsDead() && CombatContext.DamageDealt > 0;
                            break;
                        }
                        case ConditionType.Death:
                        {
                            actionTriggered = CombatContext.Enemy.IsDead();
                            break;
                        }
                        case ConditionType.HPUnderPercent:
                        {
                            actionTriggered = !CombatContext.Enemy.IsDead() && ((double)CombatContext.Enemy.HP / CombatContext.Enemy.MaxHP) < ((double)action.ConditionValue / 100);
                            break;
                        }
                    }

                    if (actionTriggered)
                    {
                        ProcessConditionalEnemyAction(action);
                    }
                }
            }
        }

        public void ProcessStartCombatConditionalEnemyActions()
        {
            foreach (var action in CombatContext.Enemy.Data.ActionDataList)
            {
                if (action.ConditionType != ConditionType.None)
                {
                    if (CombatContext.Enemy.IsActionOnCooldown(action.Name))
                    {
                        continue;
                    }

                    bool actionTriggered = false;

                    switch (action.ConditionType)
                    {
                        case ConditionType.CombatStart:
                        {
                            actionTriggered = true;
                            break;
                        }
                    }

                    if (actionTriggered)
                    {
                        ProcessConditionalEnemyAction(action);
                    }
                }
            }
        }

        public void ProcessConditionalEnemyAction(EnemyActionData action)
        {
            CombatContext.EnemyActionUsed = action;
            CombatContext.Enemy.StartActionCooldown(action);
            HandleEnemyActionUsed();

            // Reset the context for our next attacks
            ResetContext();
        }

        public void HandleEnemyActionUsed()
        {
            if (CombatContext.EnemyActionUsed.BuffTurns > 0)
            {
                CombatContext.Enemy.HandleBuffsForEnemyAction(CombatContext.EnemyActionUsed);
                GameEvents.EnemyBuffStarted(CombatContext);
            }
            else if (CombatContext.EnemyActionUsed.DamageMultiplier > 0.0)
            {
                CombatContext.Enemy.AttackTargetWithEnemyAction(CombatContext.Player, CombatContext);
                if (CombatContext.EnemyActionUsed.HealAmount > 0.0 && CombatContext.DamageDealt > 0)
                {
                    CombatContext.HealingAmount = Math.Max(1, (int)(CombatContext.DamageDealt * CombatContext.EnemyActionUsed.HealAmount));
                    CombatContext.Enemy.Heal(CombatContext);
                }
            }
            else if (CombatContext.EnemyActionUsed.HealAmount > 0.0)
            {
                CombatContext.HealingAmount = (int)(CombatContext.Enemy.MaxHP * CombatContext.EnemyActionUsed.HealAmount);
                CombatContext.Enemy.Heal(CombatContext);
            }
        }

        public void KillEnemy()
        {
            GameEvents.EnemyKilled(CombatContext);

            List<LootTableData> combinedLootTables = new List<LootTableData>(CombatContext.Enemy.Data.LootTableDataList);
            foreach (var requestData in CombatContext.Player.ActiveRequestData)
            {
                if (string.IsNullOrEmpty(requestData.SpawnLoot))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(requestData.RequiresDungeon) || requestData.RequiresDungeon == DungeonSystem.Instance.GetCurrentDungeonName())
                {
                    combinedLootTables.AddRange(LootSystem.Instance.GetLootTables(requestData.SpawnLoot));
                }
            }
            var lootFound = LootSystem.Instance.RollLoot(combinedLootTables, CombatContext.Player.Level + 5, false);
            ProcessLoot(lootFound);

            int lootGold = 0;
            foreach (var lootResult in lootFound)
            {
                if (lootResult.Gold > 0)
                {
                    lootGold += lootResult.Gold;
                }
            }

            int enemyGold = CombatContext.Enemy.Data.Loot + UnityEngine.Random.Range(0, 3);
            totalEarnedGold += (enemyGold + lootGold);
            GameEvents.GetGold(enemyGold);

            hasLeftBattle = true;

            CombatContext.Player.ProcessActiveSkillLifetimes(SkillLifetime.Battle);

            PlayerSystem.Instance.Player.EarnXP(CombatContext.Enemy.Data.XP);
            totalEarnedXP += CombatContext.Enemy.Data.XP;
            if (PlayerSystem.Instance.Player.StatPointsRemaining > 0)
            {
                PlayerLeveledUp();
            }
            else
            {
                PlayerFinishedCombat();
            }
        }

        private void ProcessLoot(List<LootResult> lootFound)
        {
            if (lootFound != null && lootFound.Count > 0)
            {
                foreach (var loot in lootFound)
                {
                    // Gold loot doesn't have items
                    if (loot.ItemDataSlot == null)
                    {
                        continue;
                    }
                    if (!loot.ItemDataSlot.ItemData.IsStackable())
                    {
                        lootEarned.Add(loot);
                        continue;
                    }
                    bool alreadyFound = false;
                    foreach (var earnedLoot in lootEarned)
                    {
                        if (loot.ItemDataSlot.ItemData == earnedLoot.ItemDataSlot.ItemData)
                        {
                            earnedLoot.Count += loot.Count;
                            alreadyFound = true;
                            break;
                        }
                    }
                    if (!alreadyFound)
                    {
                        lootEarned.Add(loot);
                    }
                }
                GameEvents.GetLoot(lootFound);
            }
        }

        private void DisableButtons()
        {
            var afterCombatButtons = AfterCombatActionsView.GetComponentsInChildren<Button>();
            var combatButtons = CombatActionsView.GetComponentsInChildren<Button>();
            foreach (var button in afterCombatButtons)
            {
                button.interactable = false;
            }
            foreach (var button in combatButtons)
            {
                button.interactable = false;
            }
        }

        private void EnableButtons()
        {
            var afterCombatButtons = AfterCombatActionsView.GetComponentsInChildren<Button>();
            var combatButtons = CombatActionsView.GetComponentsInChildren<Button>();
            foreach (var button in afterCombatButtons)
            {
                button.interactable = true;
            }
            foreach (var button in combatButtons)
            {
                button.interactable = true;
            }
        }

        IEnumerator DisableThenReenableButtons()
        {
            DisableButtons();

            yield return new WaitForSeconds(0.25f);

            EnableButtons();
        }

        public void KillPlayer()
        {
            GameEvents.PlayerKilled(CombatContext);
            ShowSummaryView(CombatSummary.Defeat);
        }

        public void PlayerFinishedCombat()
        {
            if (CombatContext.Player.IsDead())
            {
                KillPlayer();
                return;
            }

            if (DungeonSystem.Instance.IsLastRoomOfDungeon())
            {
                ShowSummaryView(CombatSummary.Victory);
            }
            else
            {
                if (DungeonSystem.Instance.IsLastRoomOfFloor())
                {
                    CombatContext.Player.ProcessActiveSkillLifetimes(SkillLifetime.Floors);
                }
                CombatActionsView.SafeSetActive(false);
                AfterCombatContinueButton.SafeSetActive(true);
                AfterCombatItemButton.SafeSetActive(true);
                AfterCombatSkillButton.SafeSetActive(true);
                AfterCombatActionsView.SafeSetActive(true);

                StartCoroutine(DisableThenReenableButtons());
            }
        }

        public void PlayerEscaped()
        {
            hasLeftBattle = true;
            ShowSummaryView(CombatSummary.Escaped);
        }

        public void ShowSummaryView(CombatSummary combatResult)
        {
            CombatLogView.gameObject.SafeSetActive(false);
            CombatActionsView.SafeSetActive(false);
            SummaryView.SafeSetActive(true);
            AfterCombatActionsView.SafeSetActive(true);
            AfterCombatContinueButton.SafeSetActive(false);
            AfterCombatItemButton.SafeSetActive(false);
            AfterCombatSkillButton.SafeSetActive(false);

            if (combatResult == CombatSummary.Victory)
            {
                ResultText.text = "<color=#00AAFF>You've Won!</color>";
            }
            else if (combatResult == CombatSummary.Defeat)
            {
                ResultText.text = "<color=red>You've Been Defeated!</color>";
            }
            else
            {
                ResultText.text = "<color=#00FFFF>You've Escaped!</color>";
            }

            StartCoroutine(DisableThenReenableButtons());

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Combat Summary:");
            sb.AppendLine();
            sb.AppendLine("Earned:");
            sb.AppendLine();
            sb.AppendLine($"<color=purple>{totalEarnedXP} XP</color>");
            sb.AppendLine($"<color=yellow>{totalEarnedGold} Gold</color>");
            sb.AppendLine();
            if (lootEarned.Count > 0)
            {
                sb.AppendLine("Found:");
                sb.AppendLine();
                foreach (var item in lootEarned)
                {
                    if (item.Count > 0)
                    {
                        sb.AppendLine($"{ItemSystem.Instance.BuildName(item.ItemDataSlot)} x{item.Count}");
                    }
                    else
                    {
                        sb.AppendLine(ItemSystem.Instance.BuildName(item.ItemDataSlot));
                    }
                }
            }
            SummaryText.text = sb.ToString();

            CombatContext.Player.ProcessActiveSkillLifetimes(SkillLifetime.Dungeon);
        }

        public void ShowTreasureRoom()
        {
            EnemyCombatStatsView.gameObject.SafeSetActive(false);
            CombatLogView.gameObject.SafeSetActive(false);
            CombatActionsView.SafeSetActive(false);
            AfterCombatActionsView.SafeSetActive(true);
            AfterCombatContinueButton.SafeSetActive(true);
            AfterCombatItemButton.SafeSetActive(true);
            AfterCombatSkillButton.SafeSetActive(true);
            
            DisableButtons();

            TreasureRoomView.SafeSetActive(true);
            TreasureRoomChestButton.SafeSetActive(true);
            TreasureRoomResultText.gameObject.SafeSetActive(false);
        }

        public void SelectTreasureChest()
        {
            TreasureRoomChestButton.SafeSetActive(false);
            TreasureRoomResultText.gameObject.SafeSetActive(true);
            TreasureRoomResultText.text = "";
            
            var roomLootTables = DungeonSystem.Instance.GetTreasureRoomLootTableForCurrentDungeon();
            var lootResultList = LootSystem.Instance.RollLoot(roomLootTables, CombatContext.Player.Level + 5, false);

            ProcessRoomLoot(lootResultList, TreasureRoomResultText);

            EnableButtons();
        }

        public void ProcessRoomLoot(List<LootResult> lootResultList, TextMeshProUGUI resultText)
        {
            ProcessLoot(lootResultList);

            int lootGold = 0;
            foreach (var lootResult in lootResultList)
            {
                if (lootResult.Gold > 0)
                {
                    lootGold += lootResult.Gold;
                }
            }

            totalEarnedGold += lootGold;
            GameEvents.GetGold(lootGold);

            if (lootResultList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Found:");
                sb.AppendLine();
                foreach (var item in lootResultList)
                {
                    if (item.Gold > 0)
                    {
                        sb.AppendLine($"<color=yellow>{item.Gold} Gold</color>");
                    }
                    else if (item.Count > 0)
                    {
                        sb.AppendLine($"{ItemSystem.Instance.BuildName(item.ItemDataSlot)} x{item.Count}");
                    }
                    else
                    {
                        sb.AppendLine(ItemSystem.Instance.BuildName(item.ItemDataSlot));
                    }
                }
                resultText.text += sb.ToString();
            }
        }

        public void ShowFountainRoom()
        {
            EnemyCombatStatsView.gameObject.SafeSetActive(false);
            CombatLogView.gameObject.SafeSetActive(false);
            CombatActionsView.SafeSetActive(false);
            AfterCombatActionsView.SafeSetActive(true);
            AfterCombatContinueButton.SafeSetActive(true);
            AfterCombatItemButton.SafeSetActive(true);
            AfterCombatSkillButton.SafeSetActive(true);

            DisableButtons();

            FountainRoomView.SafeSetActive(true);
            FountainRoomDrinkButton.SafeSetActive(true);
            FountainRoomSearchButton.SafeSetActive(true);
            FountainRoomResultText.gameObject.SafeSetActive(false);
        }

        public void SelectFountainDrink()
        {
            FountainRoomDrinkButton.SafeSetActive(false);
            FountainRoomSearchButton.SafeSetActive(false);
            FountainRoomResultText.text = "You drank some water, which restored your HP/MP.\n\nThe fountain has dried up and there is nothing left.";
            FountainRoomResultText.gameObject.SafeSetActive(true);

            CombatContext.Player.HP = CombatContext.Player.MaxHP;
            CombatContext.Player.MP = CombatContext.Player.MaxMP;
            GameEvents.PlayerHPChanged();
            GameEvents.PlayerMPChanged();
            GameEvents.PlayerHealedAtFountain();

            EnableButtons();
        }

        public void SelectFountainSearch()
        {
            FountainRoomDrinkButton.SafeSetActive(false);
            FountainRoomSearchButton.SafeSetActive(false);

            var roomLootTables = DungeonSystem.Instance.GetFountainRoomLootTableForCurrentDungeon();
            var lootResultList = LootSystem.Instance.RollLoot(roomLootTables, CombatContext.Player.Level + 5, false);
            if (lootResultList.Count == 0)
            {
                FountainRoomResultText.text = "You searched the fountain's waters and came up with nothing.\n";
            }
            else
            {
                FountainRoomResultText.text = "You searched the fountain's waters and found something!\n\n";
                ProcessRoomLoot(lootResultList, FountainRoomResultText);
            }

            FountainRoomResultText.text += "\nThe fountain has dried up and there is nothing left.";

            FountainRoomResultText.gameObject.SafeSetActive(true);

            EnableButtons();
        }

        public void SelectContinue()
        {
            EnemyCombatStatsView.gameObject.SafeSetActive(true);
            CombatLogView.gameObject.SafeSetActive(true);
            CombatActionsView.SafeSetActive(true);
            AfterCombatActionsView.SafeSetActive(false);
            CombatSelectionView.gameObject.SafeSetActive(false);
            CombatLogView.gameObject.SafeSetActive(true);
            TreasureRoomView.SafeSetActive(false);
            FountainRoomView.SafeSetActive(false);

            DungeonSystem.Instance.AdvanceRoom();
        }

        public void SelectReturnToTown()
        {
            if (!SummaryView.gameObject.activeSelf)
            {
                PlayerEscaped();
                return;
            }
            if (CombatContext.Player.IsDead())
            {
                GameEvents.RestAtInn();
            }
            CombatView.SafeSetActive(false);
            GameEvents.OnEnterTown();
        }

        public void PlayerLeveledUp()
        {
            CombatLogView.gameObject.SafeSetActive(false);
            CombatActionsView.SafeSetActive(false);
            PlayerCombatStatsView.gameObject.SafeSetActive(false);
            EnemyCombatStatsView.gameObject.SafeSetActive(false);
            PlayerStatsView.Setup();
            LevelUpView.SafeSetActive(true);
            UpdateStatPointsRemaining();
        }

        public void UpdateStatPointsRemaining()
        {
            StatPointsRemainingText.text = $"Stat Points Remaining: {PlayerSystem.Instance.Player.StatPointsRemaining}";
        }

        public void AddStatFromLevelingUp(string stat)
        {
            switch (stat)
            {
                case "Strength":
                {
                    PlayerSystem.Instance.Player.Strength++;
                    break;
                }
                case "Intelligence":
                {
                    PlayerSystem.Instance.Player.Intelligence++;
                    PlayerSystem.Instance.Player.MP = PlayerSystem.Instance.Player.MaxMP;
                    GameEvents.PlayerMaxMPChanged();
                    break;
                }
                case "Endurance":
                {
                    PlayerSystem.Instance.Player.Endurance++;
                    PlayerSystem.Instance.Player.HP = PlayerSystem.Instance.Player.MaxHP;
                    GameEvents.PlayerMaxHPChanged();
                    break;
                }
                case "Agility":
                {
                    PlayerSystem.Instance.Player.Agility++;
                    break;
                }
            }
            PlayerSystem.Instance.Player.StatPointsRemaining--;
            PlayerStatsView.Setup();
            UpdateStatPointsRemaining();
            if (PlayerSystem.Instance.Player.StatPointsRemaining <= 0)
            {
                PlayerFinishedLevelingUp();
            }
        }

        public void PlayerFinishedLevelingUp()
        {
            CombatLogView.gameObject.SafeSetActive(true);
            CombatActionsView.SafeSetActive(true);
            PlayerCombatStatsView.gameObject.SafeSetActive(true);
            EnemyCombatStatsView.gameObject.SafeSetActive(true);
            LevelUpView.SafeSetActive(false);

            PlayerFinishedCombat();
        }

    }
}