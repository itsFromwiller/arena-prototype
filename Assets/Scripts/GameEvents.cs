using Arena.Combat;
using Arena.Dungeon;
using Arena.Enemies;
using Arena.Loot;
using System;
using System.Collections.Generic;

public class GameEvents
{
//    public static Action<string, int, bool> OnDealDamage;
//    public static Action<string, bool> OnMiss;
//    public static Action<string, int, bool> OnHeal;
//    public static Action<string, bool> OnKilled;
    public static Action<int> OnGetGold;
    public static Action<List<LootResult>> OnGetLoot;
    public static Action<int> OnGetXP;

    public static Action OnRestAtInn;
    public static Action OnEquipmentChanged; // not used or needed?
    public static Action OnPlayerHPChanged;
    public static Action OnPlayerMaxHPChanged;
    public static Action OnPlayerMPChanged;
    public static Action OnPlayerMaxMPChanged;
    public static Action OnPlayerGoldChanged; // not used or needed?
    public static Action OnPlayerXPChanged; // not used or needed?
    public static Action<int> OnPlayerLevelChanged;
    public static Action OnPlayerNameChanged; // not used or needed?
    public static Action OnPlayerClassChanged; 
    public static Action OnPlayerSpawned;
    public static Action<EnemyEntity> OnEnemyStateUpdated;
    public static Action<string> OnEnterDungeon;
    public static Action<DungeonRoomEntity> OnEnterDungeonRoom;
    public static Action OnFinishDungeon;
    public static Action OnEnterWorld;
    public static Action OnEnterTown;
    public static Action OnEnterInn;
    public static Action OnEnterTavern;
    public static Action<string> OnEnterShop;

    // Combat
    public static Action OnEnterCombat;
    public static Action<EnemyEntity> OnEnemySpawned;
    public static Action<EnemyEntity> OnEnemyGotInitiative;
    public static Action OnStartCombat;
    public static Action<CombatContext> OnPlayerDamaged;
    public static Action<CombatContext> OnPlayerHealed;
    public static Action<CombatContext> OnPlayerRestoreMP;
    public static Action<CombatContext> OnPlayerStealMP;
    public static Action<CombatContext> OnPlayerMissed;
    public static Action<CombatContext> OnPlayerSkillFailed;
    public static Action<CombatContext> OnPlayerKilled;
    public static Action<CombatContext> OnEnemyDamaged;
    public static Action<CombatContext> OnEnemyHealed;
    public static Action<CombatContext> OnEnemyRestoreMP;
    public static Action<CombatContext> OnEnemyStealMP;
    public static Action<CombatContext> OnEnemyMissed;
    public static Action<CombatContext> OnEnemySkillFailed;
    public static Action<CombatContext> OnEnemyKilled;
    public static Action<CombatContext> OnEnemyBuffStarted;
    public static Action OnEndCombat;

    public static void GetGold(int amount)
    {
        OnGetGold?.Invoke(amount);
    }

    public static void GetLoot(List<LootResult> loot)
    {
        OnGetLoot?.Invoke(loot);
    }

    public static void GetXP(int amount)
    {
        OnGetXP?.Invoke(amount);
    }

    public static void RestAtInn()
    {
        OnRestAtInn?.Invoke();
    }

    public static void EquipmentChanged()
    {
        OnEquipmentChanged?.Invoke();
    }

    public static void PlayerHPChanged()
    {
        OnPlayerHPChanged?.Invoke();
    }

    public static void PlayerMaxHPChanged()
    {
        OnPlayerMaxHPChanged?.Invoke();
    }

    public static void PlayerMPChanged()
    {
        OnPlayerMPChanged?.Invoke();
    }

    public static void PlayerMaxMPChanged()
    {
        OnPlayerMaxMPChanged?.Invoke();
    }

    public static void PlayerNameChanged()
    {
        OnPlayerNameChanged?.Invoke();
    }
    public static void PlayerClassChanged()
    {
        OnPlayerClassChanged?.Invoke();
    }
    public static void PlayerLevelChanged(int newLevel)
    {
        OnPlayerLevelChanged?.Invoke(newLevel);
    }
    public static void PlayerXPChanged()
    {
        OnPlayerXPChanged?.Invoke();
    }
    public static void PlayerGoldChanged()
    {
        OnPlayerGoldChanged?.Invoke();
    }

    public static void EnemySpawned(EnemyEntity enemyEntity)
    {
        OnEnemySpawned?.Invoke(enemyEntity);
    }

    public static void PlayerSpawned()
    {
        OnPlayerSpawned?.Invoke();
    }

    public static void EnemyStateUpdated(EnemyEntity enemyEntity)
    {
        OnEnemyStateUpdated?.Invoke(enemyEntity);
    }

    public static void EnterDungeon(string dungeonName)
    {
        OnEnterDungeon?.Invoke(dungeonName);
    }

    public static void EnterDungeonRoom(DungeonRoomEntity entity)
    {
        OnEnterDungeonRoom?.Invoke(entity);
    }

    public static void FinishDungeon()
    {
        OnFinishDungeon?.Invoke();
    }

    public static void EnterWorld()
    {
        OnEnterWorld?.Invoke();
    }

    public static void EnterTown()
    {
        OnEnterTown?.Invoke();
    }

    public static void EnterInn()
    {
        OnEnterInn?.Invoke();
    }

    public static void EnterTavern()
    {
        OnEnterTavern?.Invoke();
    }

    public static void EnterShop(string shopType)
    {
        OnEnterShop?.Invoke(shopType);
    }

    public static void EnterCombat()
    {
        OnEnterCombat?.Invoke();
    }

    public static void StartCombat()
    {
        OnStartCombat?.Invoke();
    }

    public static void PlayerDamaged(CombatContext combatContext)
    {
        OnPlayerDamaged?.Invoke(combatContext);
    }

    public static void PlayerHealed(CombatContext combatContext)
    {
        OnPlayerHealed?.Invoke(combatContext);
    }

    public static void PlayerRestoreMP(CombatContext combatContext)
    {
        OnPlayerRestoreMP?.Invoke(combatContext);
    }

    public static void PlayerStealMP(CombatContext combatContext)
    {
        OnPlayerStealMP?.Invoke(combatContext);
    }

    public static void PlayerMissed(CombatContext combatContext)
    {
        OnPlayerMissed?.Invoke(combatContext);
    }

    public static void PlayerSkillFailed(CombatContext combatContext)
    {
        OnPlayerSkillFailed?.Invoke(combatContext);
    }

    public static void PlayerKilled(CombatContext combatContext)
    {
        OnPlayerKilled?.Invoke(combatContext);
    }

    public static void EnemyDamaged(CombatContext combatContext)
    {
        OnEnemyDamaged?.Invoke(combatContext);
    }

    public static void EnemyHealed(CombatContext combatContext)
    {
        OnEnemyHealed?.Invoke(combatContext);
    }

    public static void EnemyRestoreMP(CombatContext combatContext)
    {
        OnEnemyRestoreMP?.Invoke(combatContext);
    }

    public static void EnemyStealMP(CombatContext combatContext)
    {
        OnEnemyStealMP?.Invoke(combatContext);
    }

    public static void EnemyMissed(CombatContext combatContext)
    {
        OnEnemyMissed?.Invoke(combatContext);
    }

    public static void EnemySkillFailed(CombatContext combatContext)
    {
        OnEnemySkillFailed?.Invoke(combatContext);
    }

    public static void EnemyKilled(CombatContext combatContext)
    {
        OnEnemyKilled?.Invoke(combatContext);
    }

    public static void EnemyGotInitiative(EnemyEntity enemyEntity)
    {
        OnEnemyGotInitiative?.Invoke(enemyEntity);
    }

    public static void EnemyBuffStarted(CombatContext combatContext)
    {
        OnEnemyBuffStarted?.Invoke(combatContext);
    }

    public static void EndCombat()
    {
        OnEndCombat?.Invoke();
    }
}
