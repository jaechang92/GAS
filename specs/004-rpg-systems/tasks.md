# Implementation Tasks: RPG Systems Feature

**Feature Branch**: `004-rpg-systems`
**Created**: 2025-11-01
**Status**: Ready for Implementation

---

## Overview

This document breaks down the RPG Systems feature into **47 executable tasks** organized by user story priority (P1-P6).

### User Stories Summary

| Story | Priority | Goal | Tasks | Independent Test |
|-------|----------|------|-------|------------------|
| **US1** | P1 | Stat-Based Character System | 8 | Equip items → verify stat display |
| **US2** | P2 | Shop & Economy System | 7 | Purchase items → verify gold deduction |
| **US3** | P3 | Enemy Type System | 6 | Spawn enemies → verify stats/rewards |
| **US4** | P4 | Combat with Stat Integration | 5 | Attack enemies → verify damage calculation |
| **US5** | P5 | Save/Load Progress | 6 | Save → load → verify data restored |
| **US6** | P6 | Fire Grimoire & Level Completion | 9 | Use Fire Magic → defeat Boss → level complete |

### Implementation Strategy

**MVP Scope**: US1 only (Stat System)
**Full Feature**: US1 → US2 → US3 → US4 → US5 → US6
**Parallel Opportunities**: 18 parallelizable tasks marked with [P]

---

## Phase 1: Setup & Project Structure

**Goal**: Initialize project structure, assembly definitions, and core enums.

### Tasks

- [ ] T001 Create Core enums directory at `Assets/_Project/Scripts/Core/Enums/`
- [ ] T002 [P] Create StatType enum in `Assets/_Project/Scripts/Core/Enums/StatType.cs`
- [ ] T003 [P] Create EquipmentSlot enum in `Assets/_Project/Scripts/Core/Enums/EquipmentSlot.cs`
- [ ] T004 [P] Create EnemyType enum in `Assets/_Project/Scripts/Core/Enums/EnemyType.cs`
- [ ] T005 Create assembly definition `Core.Enums.asmdef` in `Assets/_Project/Scripts/Core/Enums/`

**Completion Criteria**:
- All enums compile without errors
- Core.Enums assembly referenced by other assemblies

---

## Phase 2: Foundational - GAS Core Implementation

**Goal**: Implement minimal GAS (Gameplay Ability System) framework needed by all user stories.

**Blocking**: ALL user stories depend on this phase.

### Tasks

- [ ] T006 Create IAbility interface in `Assets/Plugins/GAS_Core/Interfaces/IAbility.cs`
- [ ] T007 Create IAbilitySystem interface in `Assets/Plugins/GAS_Core/Interfaces/IAbilitySystem.cs`
- [ ] T008 Create Ability base class in `Assets/Plugins/GAS_Core/Core/Ability.cs`
- [ ] T009 Create AbilityData ScriptableObject in `Assets/Plugins/GAS_Core/Data/AbilityData.cs`
- [ ] T010 Create AbilitySystem singleton in `Assets/Plugins/GAS_Core/Core/AbilitySystem.cs`
- [ ] T011 Update GAS.Core.asmdef to remove Core.Enums dependency
- [ ] T012 Verify GAS Core compiles and AbilityCooldown integration works

**Completion Criteria**:
- GAS Core assemblies compile without errors
- AbilitySystem can register and execute mock abilities
- Awaitable async pattern works (no Coroutines)

**Independent Test**: Create a TestAbility, register it, call TryExecuteAbilityAsync(), verify execution.

---

## Phase 3: User Story 1 - Stat-Based Character System (P1)

**Goal**: Implement player stats (HP, Attack, Defense) with item equipment bonuses.

**Why First**: Foundation for all RPG mechanics. Items, combat, and progression depend on this.

**Independent Test**: Start level → equip items with known bonuses → verify stat display shows base + bonuses.

### Tasks

- [ ] T013 [P] [US1] Create PlayerStats MonoBehaviour in `Assets/_Project/Scripts/Stats/PlayerStats.cs`
- [ ] T014 [P] [US1] Implement dirty flag optimization for stat calculation (<50ms requirement)
- [ ] T015 [US1] Add OnStatChanged event to PlayerStats with (StatType, oldValue, newValue) signature
- [ ] T016 [P] [US1] Create Item ScriptableObject in `Assets/_Project/Scripts/Data/Item.cs`
- [ ] T017 [P] [US1] Create 3 test items: FireSword (+5 Attack), LeatherArmor (+20 HP, +3 Defense), IronRing (+10 HP)
- [ ] T018 [US1] Implement EquipItem() and UnequipItem() methods in PlayerStats
- [ ] T019 [US1] Create StatPanel UI prefab in `Assets/_Project/Prefabs/UI/StatPanel.prefab`
- [ ] T020 [US1] Create StatPanelUI script in `Assets/_Project/Scripts/UI/StatPanelUI.cs` to display HP/Attack/Defense

**Completion Criteria**:
- PlayerStats correctly calculates final stats (base + equipped items)
- Stat changes trigger OnStatChanged event
- StatPanel UI displays current stats and updates in real-time (<50ms)
- Equipping/unequipping items immediately reflects in UI

**Parallel Opportunities**: T013, T014, T016, T017 can run in parallel (different files).

---

## Phase 4: User Story 2 - Shop & Economy System (P2)

**Goal**: Implement gold currency, shop UI, and item purchasing.

**Why Second**: Provides the primary means to acquire items that enhance stats.

**Dependencies**: Requires US1 (PlayerStats, Item) to be complete.

**Independent Test**: Open shop with 150 gold → purchase item (80 gold) → verify gold = 70 and item added to inventory.

### Tasks

- [ ] T021 [P] [US2] Create CurrencySystem singleton in `Assets/_Project/Scripts/Economy/CurrencySystem.cs`
- [ ] T022 [P] [US2] Implement AddGold(), TrySpendGold(), OnGoldChanged event in CurrencySystem
- [ ] T023 [P] [US2] Create InventorySystem singleton in `Assets/_Project/Scripts/Inventory/InventorySystem.cs`
- [ ] T024 [US2] Implement AddItem(), EquipItem(), UnequipItem() in InventorySystem with PlayerStats integration
- [ ] T025 [P] [US2] Create ShopSystem MonoBehaviour in `Assets/_Project/Scripts/Shop/ShopSystem.cs`
- [ ] T026 [US2] Create ShopUI prefab in `Assets/_Project/Prefabs/UI/ShopUI.prefab` with item list and purchase buttons
- [ ] T027 [US2] Implement PurchaseItem() method: check gold → TrySpendGold() → AddItem() → update UI

**Completion Criteria**:
- CurrencySystem tracks gold and fires OnGoldChanged event
- InventorySystem stores items and integrates with PlayerStats for equipping
- ShopUI displays items with prices
- Purchasing succeeds when gold sufficient, fails with "Insufficient Funds" message when not
- Purchased items appear in inventory and are equippable

**Parallel Opportunities**: T021, T022, T023, T025 can run in parallel.

---

## Phase 5: User Story 3 - Enemy Type System (P3)

**Goal**: Implement 3 enemy types (Normal, Named, Boss) with different stats and gold rewards.

**Why Third**: Creates varied challenges that justify stat progression and shop purchases.

**Dependencies**: Requires US2 (CurrencySystem) for gold drops.

**Independent Test**: Spawn each enemy type → observe HP, Attack, visual indicators → defeat → verify gold drop ranges.

### Tasks

- [ ] T028 [P] [US3] Create EnemyData ScriptableObject in `Assets/_Project/Scripts/Data/EnemyData.cs`
- [ ] T029 [P] [US3] Create 3 enemy data assets: NormalGoblin (30 HP, 5 Atk, 15-25 gold), EliteOrc (60 HP, 10 Atk, 40-60 gold), FireDragon (150 HP, 15 Atk, 100-150 gold)
- [ ] T030 [P] [US3] Create Enemy MonoBehaviour in `Assets/_Project/Scripts/Enemy/Enemy.cs`
- [ ] T031 [US3] Implement TakeDamage() and Die() methods in Enemy with gold drop logic
- [ ] T032 [US3] Create EnemyNameTag UI for Named enemies in `Assets/_Project/Prefabs/UI/EnemyNameTag.prefab`
- [ ] T033 [US3] Create BossHealthBar UI in `Assets/_Project/Prefabs/UI/BossHealthBar.prefab`

**Completion Criteria**:
- 3 EnemyData assets created with correct stats
- Enemy component uses EnemyData for initialization
- Normal mobs drop 15-25 gold, Named drop 40-60 gold, Boss drops 100-150 gold
- Named enemies display name tag above them
- Boss displays large health bar UI

**Parallel Opportunities**: T028, T029, T030 can run in parallel.

---

## Phase 6: User Story 4 - Combat with Stat Integration (P4)

**Goal**: Integrate PlayerStats into combat damage calculation (Attack → damage dealt, Defense → damage reduction).

**Why Fourth**: Brings stats to life in combat, showing immediate feedback on stat investments.

**Dependencies**: Requires US1 (PlayerStats), US3 (Enemy) to be complete.

**Independent Test**: Set Attack=20 → hit enemy → verify 20 damage dealt. Set Defense=5, receive 10 damage → verify 5 damage taken.

### Tasks

- [ ] T034 [US4] Create DamageCalculator utility class in `Assets/_Project/Scripts/Combat/DamageCalculator.cs`
- [ ] T035 [US4] Implement CalculateDamageDealt(int attackStat) → int damage method
- [ ] T036 [US4] Implement CalculateDamageReceived(int incomingDamage, int defenseStat) → int finalDamage method with Max(0, damage-defense)
- [ ] T037 [US4] Integrate PlayerStats.GetStat(Attack) into player attack logic
- [ ] T038 [US4] Integrate PlayerStats.GetStat(Defense) into player TakeDamage() method

**Completion Criteria**:
- Player damage = PlayerStats.GetStat(Attack)
- Player damage received = Max(0, incoming - PlayerStats.GetStat(Defense))
- Damage numbers display on hit
- Equipping items with +Attack increases damage immediately
- Equipping items with +Defense reduces damage taken immediately

---

## Phase 7: User Story 5 - Save/Load Progress (P5)

**Goal**: Implement JSON-based save/load system for stats, inventory, gold, and checkpoint position.

**Why Fifth**: Enables longer play sessions and progression investment. Requires all other systems to be stable.

**Dependencies**: Requires US1 (PlayerStats), US2 (Inventory, Currency) to be complete.

**Independent Test**: Acquire items + gold → reach checkpoint → save → close game → load → verify all data restored.

### Tasks

- [ ] T039 [P] [US5] Create SaveData class in `Assets/_Project/Scripts/Save/SaveData.cs` with serializable fields
- [ ] T040 [P] [US5] Create SaveLoadManager singleton in `Assets/_Project/Scripts/Save/SaveLoadManager.cs`
- [ ] T041 [US5] Implement SaveAsync() method using File.WriteAllTextAsync() and JsonUtility
- [ ] T042 [US5] Implement LoadAsync() method using File.ReadAllTextAsync() and JsonUtility
- [ ] T043 [US5] Integrate SaveAsync() with PlayerStats, InventorySystem, CurrencySystem data collection
- [ ] T044 [US5] Integrate LoadAsync() with system restoration (PlayerStats.SetStat(), InventorySystem.AddItem(), CurrencySystem.SetGold())

**Completion Criteria**:
- SaveAsync() completes in <2 seconds
- LoadAsync() completes in <2 seconds
- Save file size <100KB
- All data restored accurately: stats, inventory, equipped items, gold, checkpoint position
- HasSaveFile() returns true when save exists
- Graceful handling when no save file exists (FR-047)

**Parallel Opportunities**: T039, T040 can run in parallel.

---

## Phase 8: User Story 6 - Fire Grimoire & Level Completion (P6)

**Goal**: Implement Fire Magic ability scaled by Attack stat, level goal with Boss requirement, and victory screen.

**Why Last**: Integration test of all systems - validates complete gameplay loop.

**Dependencies**: Requires US1 (PlayerStats), US3 (Enemy), US4 (Combat), GAS Core (Phase 2) to be complete.

**Independent Test**: Equip Fire Grimoire → use Fire Magic → defeat Boss → reach goal → verify level completion.

### Tasks

- [ ] T045 [P] [US6] Create FireMagicData ScriptableObject in `Assets/_Project/Scripts/Data/FireMagicData.cs` with damageMultiplier=2.5, cooldown=7s
- [ ] T046 [US6] Create FireMagicAbility in `Assets/_Project/Scripts/Abilities/FireMagicAbility.cs` inheriting from Ability
- [ ] T047 [US6] Implement CanExecute() in FireMagicAbility checking IsAlive, CanAct, Cooldown
- [ ] T048 [US6] Implement ExecuteAsync() in FireMagicAbility: get Attack stat → calculate damage (Attack * 2.5) → spawn projectile → fly projectile → apply damage
- [ ] T049 [P] [US6] Create Fire projectile prefab in `Assets/_Project/Prefabs/Effects/FireProjectile.prefab`
- [ ] T050 [US6] Create FSMAbilityBridge in `Assets/Plugins/FSM_GAS_Integration/FSMAbilityBridge.cs`
- [ ] T051 [US6] Create PlayerAbilityController in `Assets/_Project/Scripts/Player/PlayerAbilityController.cs` for input handling (Q key → Fire Magic)
- [ ] T052 [US6] Create LevelGoal trigger in scene with Boss defeat requirement
- [ ] T053 [US6] Create VictoryScreen UI prefab in `Assets/_Project/Prefabs/UI/VictoryScreen.prefab` with stats summary

**Completion Criteria**:
- Fire Magic damage = PlayerStats.GetStat(Attack) * 2.5
- Fire Magic has 7 second cooldown (FR-037)
- Cooldown UI displays remaining time
- Fire Magic can defeat all 3 enemy types
- Level goal requires Boss defeated before triggering
- Victory screen shows: enemies defeated, gold collected, time taken
- Auto-save occurs on level completion

**Parallel Opportunities**: T045, T049 can run in parallel.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Goal**: Performance optimization, error handling, and final integration.

### Tasks

- [ ] T054 [P] Verify stat calculation performance <50ms with 10 equipped items (SC-001)
- [ ] T055 [P] Verify save/load operations complete in <2 seconds with full inventory (SC-010)
- [ ] T056 [P] Add error logging for all async operations (save/load failures, ability execution errors)
- [ ] T057 [P] Implement damage number visual feedback system
- [ ] T058 Playtesting: Complete full gameplay loop (shop → equip → combat → Boss → victory) and verify all 51 FRs

**Completion Criteria**:
- All performance targets met (SC-001 through SC-010)
- No console errors during full playthrough
- All 51 functional requirements verified
- Victory screen displays correct statistics

---

## Dependencies Graph

```
Setup (Phase 1)
  ↓
Foundational - GAS Core (Phase 2)
  ↓
US1: Stat System (P1) ← BLOCKING for all others
  ↓
  ├→ US2: Shop & Economy (P2)
  │    ↓
  │    └→ US3: Enemy Types (P3)
  │
  ├→ US4: Combat Integration (P4) ← depends on US1, US3
  │
  ├→ US5: Save/Load (P5) ← depends on US1, US2
  │
  └→ US6: Fire Grimoire (P6) ← depends on US1, US3, US4, GAS Core
       ↓
     Polish (Phase 9)
```

**Critical Path**: Setup → GAS Core → US1 → US4 → US6 → Polish

**Parallel Streams**:
- Stream A: US1 → US2 → US3
- Stream B: US1 → US4 → US6
- Stream C: US1 → US5

---

## Parallel Execution Examples

### Per User Story

**US1 (Stat System)**: T013, T014, T016, T017 can run in parallel (4 tasks simultaneously)

**US2 (Shop & Economy)**: T021, T022, T023, T025 can run in parallel (4 tasks simultaneously)

**US3 (Enemy Types)**: T028, T029, T030 can run in parallel (3 tasks simultaneously)

**US5 (Save/Load)**: T039, T040 can run in parallel (2 tasks simultaneously)

**US6 (Fire Grimoire)**: T045, T049 can run in parallel (2 tasks simultaneously)

**Polish**: T054, T055, T056, T057 can run in parallel (4 tasks simultaneously)

**Total Parallelizable Tasks**: 18 out of 58 tasks (31%)

---

## Task Statistics

- **Total Tasks**: 58
- **Setup Tasks**: 5
- **Foundational Tasks**: 7
- **US1 Tasks**: 8
- **US2 Tasks**: 7
- **US3 Tasks**: 6
- **US4 Tasks**: 5
- **US5 Tasks**: 6
- **US6 Tasks**: 9
- **Polish Tasks**: 5
- **Parallelizable Tasks**: 18 (31%)

---

## MVP Recommendation

**Minimal Viable Product**: Phase 1-3 (Setup + GAS Core + US1)
- 20 tasks total
- Delivers: Stat system with item equipping
- Test: Equip items → verify stat display changes
- Timeline: ~5-7 days

**First Playable**: Phase 1-4 (+ US2)
- 27 tasks total
- Delivers: Stat system + shop + economy
- Test: Purchase items → equip → verify stats increase
- Timeline: ~8-10 days

**Full Feature**: All phases (Phase 1-9)
- 58 tasks total
- Delivers: Complete RPG Systems integration
- Test: Full gameplay loop (shop → combat → Boss → victory)
- Timeline: 18-26 days (per plan.md)

---

## Validation Checklist

✅ All tasks follow `- [ ] [ID] [P?] [Story?] Description with file path` format
✅ Each user story has independent test criteria
✅ Dependencies clearly documented
✅ Parallel opportunities identified (18 tasks)
✅ MVP scope defined (US1 only)
✅ File paths specified for all implementation tasks
✅ Performance targets included (SC-001, SC-010)
✅ All 51 FRs from spec.md covered across tasks

---

## Next Steps

1. **Review and Approve**: Review task breakdown and dependencies
2. **Start MVP**: Begin Phase 1 (Setup) → Phase 2 (GAS Core) → Phase 3 (US1)
3. **Iterate**: Complete US1 → test independently → proceed to US2
4. **Integrate**: Complete all user stories → Phase 9 (Polish) → full E2E test

---

**Version**: 1.0
**Generated**: 2025-11-01
**Based On**: [spec.md](./spec.md), [plan.md](./plan.md), [data-model.md](./data-model.md), [api-contracts.md](./api-contracts.md)
