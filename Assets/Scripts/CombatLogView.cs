using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using Arena.Enemies;
using Arena.Combat;
using UnityEngine.UI;
using Arena.Dungeon;
using Arena.Loot;
using Arena.Items;

public class CombatLogView : MonoBehaviour
{
    public TextMeshProUGUI LogText;
    public ScrollRect ScrollView;

    private List<string> logText = new List<string>();
    private StringBuilder sb = new StringBuilder();
    public string BadColor = "<color=red>";
    public string GoodColor = "<color=green>";
    public string GoldColor = "<color=yellow>";
    public string XPColor = "<color=#660066>";
    public string LevelUpColor = "<color=#0088FF>";
    public string SeparatorText = "===============";

    private void Awake()
    {
        GameEvents.OnGetGold += OnGetGold;
        GameEvents.OnGetLoot += OnGetLoot;
        GameEvents.OnGetXP += OnGetXP;
        GameEvents.OnPlayerLevelChanged += OnPlayerLevelChanged;
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
        GameEvents.OnPlayerHealed += OnPlayerHealed;
        GameEvents.OnPlayerHealedAtFountain += OnPlayerHealedAtFountain;
        GameEvents.OnPlayerRestoreMP += OnPlayerRestoreMP;
        GameEvents.OnPlayerKilled += OnPlayerKilled;
        GameEvents.OnPlayerMissed += OnPlayerMissed;
        GameEvents.OnPlayerSkillFailed += OnPlayerSkillFailed;
        GameEvents.OnEnemyDamaged += OnEnemyDamaged;
        GameEvents.OnEnemyHealed += OnEnemyHealed;
        GameEvents.OnEnemyRestoreMP += OnEnemyRestoreMP;
        GameEvents.OnEnemyKilled += OnEnemyKilled;
        GameEvents.OnEnemyMissed += OnEnemyMissed;
        GameEvents.OnEnemySkillFailed += OnEnemySkillFailed;
        GameEvents.OnEnemySpawned += OnEnemySpawned;
        GameEvents.OnEnemyGotInitiative += OnEnemyGotInitiative;
        GameEvents.OnEnemyBuffStarted += OnEnemyBuffStarted;
        GameEvents.OnEnterDungeonRoom += OnEnterDungeonRoom;
    }

    private void OnDisableDisabled()
    {
        GameEvents.OnGetGold -= OnGetGold;
        GameEvents.OnGetLoot -= OnGetLoot;
        GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
        GameEvents.OnPlayerHealed -= OnPlayerHealed;
        GameEvents.OnPlayerHealedAtFountain -= OnPlayerHealedAtFountain;
        GameEvents.OnPlayerRestoreMP -= OnPlayerRestoreMP;
        GameEvents.OnPlayerStealMP -= OnPlayerStealMP;
        GameEvents.OnPlayerKilled -= OnPlayerKilled;
        GameEvents.OnPlayerMissed -= OnPlayerMissed;
        GameEvents.OnPlayerSkillFailed -= OnPlayerSkillFailed;
        GameEvents.OnEnemyDamaged -= OnEnemyDamaged;
        GameEvents.OnEnemyHealed -= OnEnemyHealed;
        GameEvents.OnEnemyRestoreMP -= OnEnemyRestoreMP;
        GameEvents.OnEnemyStealMP -= OnEnemyStealMP;
        GameEvents.OnEnemyKilled -= OnEnemyKilled;
        GameEvents.OnEnemyMissed -= OnEnemyMissed;
        GameEvents.OnEnemySkillFailed -= OnEnemySkillFailed;
        GameEvents.OnEnemySpawned -= OnEnemySpawned;
        GameEvents.OnEnemyGotInitiative -= OnEnemyGotInitiative;
        GameEvents.OnEnemyBuffStarted -= OnEnemyBuffStarted;
        GameEvents.OnEnterDungeonRoom -= OnEnterDungeonRoom;
    }

    private void AddToLog(string logLine)
    {
        if (logText.Count == 999 )
        {
            logText.RemoveAt(0);
        }
        logText.Add(logLine);
        UpdateLog();
    }

    public void ClearLog()
    {
        logText.Clear();
        UpdateLog();
    }

    private void UpdateLog()
    {
        sb.Clear();
        foreach(string line in logText)
        {
            sb.AppendLine(line);
        }
        LogText.text = sb.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollView.content);
        ScrollView.verticalNormalizedPosition = 0;
    }

    private void OnPlayerDamaged(CombatContext combatContext)
    {
        if (combatContext.SkillUsed != null)
        {
            if (combatContext.IsRepeatedAction)
            {
                AddToLog($"{BadColor}You are {combatContext.SkillUsed.RepeatText} for {combatContext.DamageDealt}</color>");
            }
            else
            {
                AddToLog($"{BadColor}{combatContext.Enemy.Data.Name} cast {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} you for {combatContext.DamageDealt}</color>");
            }
        }
        else if (combatContext.EnemyActionUsed != null)
        {
            AddToLog($"{BadColor}{combatContext.Enemy.Data.Name} {combatContext.EnemyActionUsed.SuccessText} for {combatContext.DamageDealt}</color>");
        }
        else
        {
            AddToLog($"{BadColor}{combatContext.Enemy.Data.Name} hits you for {combatContext.DamageDealt}</color>");
        }
    }

    private void OnPlayerHealedAtFountain()
    {
        AddToLog($"{GoodColor}You drank from the Magic Fountain and restored your HP/MP!</color>");
    }

    private void OnPlayerHealed(CombatContext combatContext)
    {
        if (combatContext.SkillUsed != null && combatContext.ItemUsed != null)
        {
            AddToLog($"{GoodColor}You use {combatContext.ItemUsed.Name}, {combatContext.SkillUsed.CombatText} you for {combatContext.HealingAmount}</color>");
        }
        else if (combatContext.SkillUsed != null)
        {
            if (combatContext.IsRepeatedAction)
            {
                AddToLog($"{GoodColor}You are {combatContext.SkillUsed.RepeatText} for {combatContext.HealingAmount}</color>");
            }
            else
            {
                AddToLog($"{GoodColor}You cast {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} you for {combatContext.HealingAmount}</color>");
            }
        }
        else if (combatContext.ItemUsed != null)
        {
            AddToLog($"{GoodColor}You use {combatContext.ItemUsed.Name}, healing yourself for for {combatContext.HealingAmount}</color>");
        }
        else
        {
            AddToLog($"{GoodColor}You heal yourself for {combatContext.HealingAmount}</color>");
        }
    }

    private void OnPlayerRestoreMP(CombatContext combatContext)
    {
        if (combatContext.SkillUsed != null && combatContext.ItemUsed != null)
        {
            AddToLog($"{GoodColor}You use {combatContext.ItemUsed.Name}, {combatContext.SkillUsed.CombatText} by {combatContext.RestoreAmount}</color>");
        }
        else if (combatContext.SkillUsed != null)
        {
            AddToLog($"{GoodColor}You cast {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} by {combatContext.RestoreAmount}</color>");
        }
        else if (combatContext.ItemUsed != null)
        {
            AddToLog($"{GoodColor}You use {combatContext.ItemUsed.Name}, restoring your magic points by {combatContext.RestoreAmount}</color>");
        }
        else
        {
            AddToLog($"{GoodColor}You restore magic points by {combatContext.RestoreAmount}</color>");
        }
    }

    private void OnPlayerStealMP(CombatContext combatContext)
    {
        if (combatContext.EnemyWasDead)
        {
            AddToLog($"{combatContext.Enemy.Data.Name} was already dead, that did nothing...");
        }
        else if (combatContext.SkillUsed != null && combatContext.ItemUsed != null)
        {
            AddToLog($"{GoodColor}You use {combatContext.ItemUsed.Name}, {combatContext.SkillUsed.CombatText} {combatContext.Enemy.Data.Name}, restoring your magic points by {combatContext.StealAmount}</color>");
        }
        else if (combatContext.SkillUsed != null)
        {
            AddToLog($"{GoodColor}You cast {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} {combatContext.Enemy.Data.Name}, restoring your magic points by {combatContext.StealAmount}</color>");
        }
        else if (combatContext.ItemUsed != null)
        {
            AddToLog($"{GoodColor}You use {combatContext.ItemUsed.Name}, stealing {combatContext.StealAmount} magic points from {combatContext.Enemy.Data.Name}</color>");
        }
        else
        {
            AddToLog($"{GoodColor}You restore magic points by {combatContext.RestoreAmount}</color>");
        }
    }

    private void OnPlayerMissed(CombatContext combatContext)
    {
        AddToLog($"{BadColor}You missed!</color>");
    }

    private void OnPlayerSkillFailed(CombatContext combatContext)
    {
        AddToLog($"{BadColor}You tried to use {combatContext.SkillUsed.Name}, but failed!</color>");
    }

    private void OnPlayerKilled(CombatContext combatContext)
    {
        AddToLog(SeparatorText);
        AddToLog($"{BadColor}You were defeated by {combatContext.Enemy.Data.Name}</color>");
    }

    private void OnEnemyDamaged(CombatContext combatContext)
    {
        if (combatContext.EnemyWasDead)
        {
            AddToLog($"{combatContext.Enemy.Data.Name} was already dead, that did nothing...");
        }
        else if (combatContext.SkillUsed != null && combatContext.ItemUsed != null)
        {
            AddToLog($"You use {combatContext.ItemUsed.Name}, {combatContext.SkillUsed.CombatText} {combatContext.Enemy.Data.Name} for {combatContext.DamageDealt}");
        }
        else if (combatContext.SkillUsed != null)
        {
            if (combatContext.IsRepeatedAction)
            {
                AddToLog($"{combatContext.Enemy.Data.Name} is {combatContext.SkillUsed.RepeatText} for {combatContext.DamageDealt}");
            }
            else
            {
                AddToLog($"You cast {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} {combatContext.Enemy.Data.Name} for {combatContext.DamageDealt}");
            }
        }
        else if (combatContext.ItemUsed != null)
        {
            AddToLog($"You use {combatContext.ItemUsed.Name}, dealing {combatContext.DamageDealt} damage to {combatContext.Enemy.Data.Name}");
        }
        else
        {
            AddToLog($"You hit {combatContext.Enemy.Data.Name} for {combatContext.DamageDealt}");
        }
    }

    private void OnEnemyHealed(CombatContext combatContext)
    {
        if (combatContext.SkillUsed != null)
        {
            if (combatContext.IsRepeatedAction)
            {
                AddToLog($"{combatContext.Enemy.Data.Name} is {combatContext.SkillUsed.RepeatText} for {combatContext.HealingAmount}");
            }
            else
            {
                AddToLog($"{combatContext.Enemy.Data.Name} casts {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} it for {combatContext.HealingAmount}");
            }
        }
        else if (combatContext.EnemyActionUsed != null)
        {
            AddToLog($"{combatContext.Enemy.Data.Name} {combatContext.EnemyActionUsed.HealText}, restoring {combatContext.HealingAmount} HP");
        }
        else
        {
            AddToLog($"{combatContext.Enemy.Data.Name} heals itself for {combatContext.HealingAmount}");
        }
    }

    private void OnEnemyRestoreMP(CombatContext combatContext)
    {
        if (combatContext.SkillUsed != null)
        {
            AddToLog($"{combatContext.Enemy.Data.Name} casts {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} by {combatContext.RestoreAmount}");
        }
        else
        {
            AddToLog($"{combatContext.Enemy.Data.Name} restores its magic points by  {combatContext.RestoreAmount}");
        }
    }

    private void OnEnemyStealMP(CombatContext combatContext)
    {
        if (combatContext.SkillUsed != null)
        {
            AddToLog($"{BadColor}{combatContext.Enemy.Data.Name} casts {combatContext.SkillUsed.Name}, {combatContext.SkillUsed.CombatText} you, restoring its magic points by {combatContext.StealAmount}</color>");
        }
        else
        {
            AddToLog($"{BadColor}{combatContext.Enemy.Data.Name} steals {combatContext.StealAmount} magic points from you</color>");
        }
    }

    private void OnEnemyMissed(CombatContext combatContext)
    {
        AddToLog($"{combatContext.Enemy.Data.Name} tried to {combatContext.EnemyActionUsed.FailedText}, but missed!");
    }

    private void OnEnemySkillFailed(CombatContext combatContext)
    {
        AddToLog($"{combatContext.Enemy.Data.Name} tried to cast {combatContext.SkillUsed.Name}, but failed!");
    }

    private void OnEnemyKilled(CombatContext combatContext)
    {
        AddToLog(SeparatorText);
        AddToLog($"{GoodColor}You killed {combatContext.Enemy.Data.Name}!</color>");
    }

    private void OnGetGold(int amount)
    {
        AddToLog($"{GoldColor}You found {amount} gold coins!</color>");
    }

    private void OnGetXP(int amount)
    {
        AddToLog($"{XPColor}You earned {amount} XP!</color>");
    }

    private void OnPlayerLevelChanged(int newLevel)
    {
        AddToLog($"{LevelUpColor}You have reached Level {newLevel}! HP/MP restored!</color>");
    }

    private void OnGetLoot(List<LootResult> loot)
    {
        foreach (var lootResult in loot)
        {
            if (lootResult.ItemDataSlot == null)
            {
                continue;
            }
            if (lootResult.Count > 1)
            {
                AddToLog($"{GoldColor}You found {ItemSystem.Instance.BuildName(lootResult.ItemDataSlot)} x{lootResult.Count}!</color>");
            }
            else
            {
                AddToLog($"{GoldColor}You found {ItemSystem.Instance.BuildName(lootResult.ItemDataSlot)}!</color>");
            }
        }
    }

    private void OnEnterDungeonRoom(DungeonRoomEntity dungeonRoomEntity)
    {
        AddToLog(SeparatorText);
        AddToLog($"<color=#0099AA>You entered {dungeonRoomEntity.DungeonEntity.DungeonInfoData.RoomName} {dungeonRoomEntity.DungeonEntity.CurrentRoom} of Floor {dungeonRoomEntity.DungeonEntity.CurrentFloor}</color>");
        switch (dungeonRoomEntity.RoomTypeName)
        {
            case "Treasure":
            {
                AddToLog($"You discovered a Treasure Chest!");
                break;
            }
            case "Fountain":
            {
                AddToLog($"You discovered a Magic Fountain!");
                break;
            }
        }
    }

    private void OnEnemySpawned(EnemyEntity enemy)
    {
        string prefix = "A";
        if (enemy.Data.Name.StartsWith("a", System.StringComparison.OrdinalIgnoreCase) ||
            enemy.Data.Name.StartsWith("e", System.StringComparison.OrdinalIgnoreCase) ||
            enemy.Data.Name.StartsWith("i", System.StringComparison.OrdinalIgnoreCase) ||
            enemy.Data.Name.StartsWith("o", System.StringComparison.OrdinalIgnoreCase) ||
            enemy.Data.Name.StartsWith("u", System.StringComparison.OrdinalIgnoreCase))
        {
            prefix = "An";
        }
        AddToLog(SeparatorText);
        AddToLog($"{prefix} {enemy.Data.Name} approaches!");
    }

    private void OnEnemyGotInitiative(EnemyEntity enemy)
    {
        AddToLog($"{BadColor}{enemy.Data.Name} got the initiative and goes first!</color>");
    }

    private void OnEnemyBuffStarted(CombatContext combatContext)
    {
        AddToLog($"{BadColor}{combatContext.Enemy.Data.Name} {combatContext.EnemyActionUsed.SuccessText}</color>");
    }
}
