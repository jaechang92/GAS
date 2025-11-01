# Feature Specification: First Level with RPG Systems

**Feature Branch**: `004-rpg-systems`
**Created**: 2025-11-01
**Status**: Draft
**Input**: User description: "First Level with RPG Systems - 플레이어는 직업별 기본 스탯(Fire Mage: HP, Attack, Defense)과 상점에서 구매한 아이템의 스탯 보너스로 최종 능력치가 결정됨. Fire Grimoire 장착 마법사로 일반몹(기본 적)/네임드몹(엘리트)/보스몹을 처치하며 레벨 완료. 상점에서 화폐로 아이템 구매 가능. 세이브/로드 시스템으로 진행도 저장."

## Overview

Transform the first playable level into a comprehensive RPG experience by integrating stat systems, item equipment, shop economy, diverse enemy types, and persistent save/load functionality. This feature elevates GASPT from a simple action platformer to an action-RPG with meaningful character progression and strategic depth.

**Target Audience**: Players who enjoy action-RPGs with character stat management, item collection, and strategic combat planning. Intermediate-level gamers who appreciate both skill-based action and numerical progression systems.

**Core Value**: Demonstrate that GASPT can deliver a satisfying RPG experience where player choices in stat management, item purchases, and equipment loadouts significantly impact combat effectiveness and level progression. The integration of three distinct enemy types (Normal/Named/Boss) provides escalating challenges that reward proper stat investment and strategic play.

**Business Goal**: Validate that RPG mechanics add meaningful depth to the action platformer core, creating a gameplay loop that encourages replaying, experimentation with different builds, and investment in character progression.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Stat-Based Character System (Priority: P1)

A player starts the level with a Fire Mage that has base stats (HP, Attack, Defense). When they equip items purchased from the shop, their total stats increase by the item bonuses. They can view their current stats in the UI at any time.

**Why this priority**: This is the foundation of all RPG mechanics. Without a functioning stat system, items, combat scaling, and progression have no meaning. All other features depend on this working correctly.

**Independent Test**: Can be tested by starting the level, viewing base stats, equipping items with known bonuses, and verifying the final stat values are correctly calculated (base + item bonuses). No shop or combat required.

**Acceptance Scenarios**:

1. **Given** the player starts the level as a Fire Mage, **When** they open the stat display, **Then** they see base stats: HP (e.g., 100), Attack (e.g., 15), Defense (e.g., 5)
2. **Given** the player equips an item with +10 HP and +3 Attack, **When** they check their stats, **Then** their displayed stats show HP: 110, Attack: 18, Defense: 5
3. **Given** the player equips multiple items with stat bonuses, **When** bonuses are applied, **Then** all bonuses stack additively to modify final stats
4. **Given** the player unequips an item, **When** the item is removed, **Then** the stat bonuses from that item are removed and stats recalculate correctly

---

### User Story 2 - Shop & Economy System (Priority: P2)

A player accesses a shop interface during the level. They browse available items (equipment pieces with stat bonuses), see the gold price for each item, and purchase items by spending their accumulated gold currency.

**Why this priority**: The shop provides the primary means for players to acquire items that enhance their stats. Without it, progression is limited and there's no gold sink. P2 because stat system must exist first.

**Independent Test**: Can be tested by opening the shop with a known gold amount, viewing items with prices, purchasing items, and verifying gold is deducted and items are added to inventory. Independent of combat or enemies.

**Acceptance Scenarios**:

1. **Given** the player has 150 gold, **When** they open the shop interface, **Then** they see a list of purchasable items with stat bonuses and gold prices
2. **Given** an item costs 80 gold and the player has 150 gold, **When** they purchase the item, **Then** their gold reduces to 70 and the item is added to their inventory
3. **Given** an item costs 100 gold and the player has 80 gold, **When** they attempt to purchase, **Then** the purchase fails with clear feedback (insufficient funds message)
4. **Given** the player has purchased an item, **When** they check their inventory, **Then** the purchased item appears and can be equipped

---

### User Story 3 - Enemy Type System (Priority: P3)

A player encounters three distinct enemy types in the level: Normal mobs (basic enemies with standard stats and gold rewards), Named mobs (elite enemies with higher stats and better rewards), and Boss mobs (powerful unique enemy with high stats and large reward).

**Why this priority**: Multiple enemy types create varied challenges and reward escalation. This justifies the stat system and shop purchases - players need stronger stats to defeat stronger enemies. P3 because combat and stats must work first.

**Independent Test**: Can be tested by spawning each enemy type and observing their different health values, damage output, visual indicators (Name tags, boss indicators), and gold rewards upon defeat. Validates the enemy taxonomy is meaningful.

**Acceptance Scenarios**:

1. **Given** the player encounters a Normal mob, **When** they engage in combat, **Then** the enemy has baseline stats (e.g., 30 HP, 5 Attack) and drops 15-25 gold when defeated
2. **Given** the player encounters a Named mob, **When** they engage in combat, **Then** the enemy has higher stats (e.g., 60 HP, 10 Attack), displays a distinctive name tag, and drops 40-60 gold when defeated
3. **Given** the player encounters the Boss mob, **When** they engage in combat, **Then** the boss has high stats (e.g., 150 HP, 15 Attack), displays a boss indicator (health bar, special UI), and drops 100-150 gold when defeated
4. **Given** all three enemy types are present in the level, **When** the player progresses, **Then** they experience a clear difficulty curve (Normal → Named → Boss)

---
### User Story 4 - Combat with Stat Integration (Priority: P4)

A player with Attack stat of 20 deals 20 damage per hit to enemies. When attacked by an enemy with 10 Attack, if the player has 5 Defense, they take 5 reduced damage (10 - 5 = 5 damage received). All combat calculations use current stat values.

**Why this priority**: This brings stats to life in combat. Players see immediate feedback on how their stat investments affect gameplay. P4 because it requires stat system, items, and basic combat to be functional first.

**Independent Test**: Can be tested by setting specific Attack and Defense values, attacking enemies, and verifying damage dealt matches Attack value. Then test receiving damage and verify Defense reduces incoming damage correctly.

**Acceptance Scenarios**:

1. **Given** the player has 20 Attack, **When** they hit an enemy, **Then** the enemy loses 20 HP with damage number displayed
2. **Given** the player has 5 Defense and is attacked for 10 damage, **When** the attack lands, **Then** the player loses 5 HP (10 - 5 Defense)
3. **Given** the player purchases an item with +5 Attack, **When** they attack after equipping it, **Then** their damage increases by 5 (new Attack value applies immediately)
4. **Given** the player has 10 Defense and receives 8 damage, **When** the attack lands, **Then** damage is reduced to 0 (Defense exceeds incoming damage, minimum 0 damage taken)

---

### User Story 5 - Save/Load Progress (Priority: P5)

A player can save their current game state (stats, inventory, equipped items, gold, level checkpoint position). When they load the save file, all their progress is restored exactly as it was saved.

**Why this priority**: Save/load enables longer play sessions and progression investment. Players won't lose progress, encouraging experimentation and exploration. P5 because all other systems must be stable before persisting their state.

**Independent Test**: Can be tested by progressing to a certain point (acquiring items, gold, reaching checkpoint), saving, closing/restarting the game, loading, and verifying all data is restored. Independent gameplay loop test.

**Acceptance Scenarios**:

1. **Given** the player has 200 gold, 3 equipped items, and is at a checkpoint, **When** they save the game, **Then** a save file is created with all current state
2. **Given** a save file exists, **When** the player selects "Load Game", **Then** all saved data (gold, inventory, equipped items, checkpoint position) is restored accurately
3. **Given** the player has made progress since last save, **When** they load without saving, **Then** the game warns about unsaved progress (or auto-saves before load)
4. **Given** no save file exists, **When** the player attempts to load, **Then** the system displays "No save data found" and starts a new game

---

### User Story 6 - Fire Grimoire & Level Completion (Priority: P6)

A player equipped with the Fire Grimoire uses fire magic abilities (scaled by their Attack stat) to defeat enemies throughout the level. After defeating the Boss mob and reaching the level goal, the level is completed successfully.

**Why this priority**: This validates the complete gameplay loop - stat building, shop purchases, combat through enemy progression, and level victory. P6 because it's the integration test of all prior systems.

**Independent Test**: Can be tested by starting with Fire Grimoire equipped, using fire magic on all enemy types, defeating the Boss, reaching the goal, and verifying level completion. Tests the full end-to-end experience.

**Acceptance Scenarios**:

1. **Given** the player has Fire Grimoire equipped with 20 Attack, **When** they use fire magic, **Then** fire damage is calculated (e.g., 2.5x Attack = 50 damage) with cooldown timer
2. **Given** the player has defeated all required enemies including the Boss, **When** they reach the level goal, **Then** the level completes with victory screen showing stats (enemies defeated, gold collected, time taken)
3. **Given** the player reaches the goal without defeating the Boss, **When** they try to complete the level, **Then** completion is blocked with feedback ("Defeat the Boss first")
4. **Given** the level is completed, **When** victory screen appears, **Then** the player's final stats, inventory, and gold are saved automatically

---

### Edge Cases

- What happens when inventory is full and player tries to purchase an item? (Assumption: Inventory has unlimited slots for MVP, or purchase blocked with "Inventory Full" message)
- What happens if player tries to save during combat? (Assumption: Save is permitted at any time, or combat prevents saving with message)
- What happens when player has high Defense and takes 0 damage from weak enemies? (Assumption: Minimum 1 damage per hit, or 0 damage is valid)
- How does the system handle defeating multiple enemies rapidly and gold accumulation? (Assumption: Gold pickup is instant and stacks automatically)
- What happens if player equips item in invalid slot? (Assumption: Each item has designated slot type, invalid equips are blocked)
- How does the system handle fractional stat values from percentage bonuses? (Assumption: All stats are integers, round down any fractional results)

## Requirements *(mandatory)*

### Functional Requirements

**Stat System**:

- **FR-001**: Fire Mage character MUST have three base stats: HP (health points), Attack (damage output), Defense (damage reduction)
- **FR-002**: System MUST calculate final stat values by adding base stats and all equipped item bonuses
- **FR-003**: System MUST display current stat values to the player in a stat panel UI
- **FR-004**: Stat recalculation MUST occur immediately when items are equipped or unequipped
- **FR-005**: All stat values MUST persist in save data

**Item System**:

- **FR-006**: Items MUST be defined with item name, stat bonuses, gold price, and equipment slot type
- **FR-007**: Each item MUST provide at least one stat bonus (HP, Attack, or Defense)
- **FR-008**: Items MUST be purchasable from the shop
- **FR-009**: Items MUST be equippable in designated equipment slots (e.g., Weapon, Armor, Accessory)
- **FR-010**: System MUST support item data definition through data files (allowing designers to add new items without code changes)

**Inventory System**:

- **FR-011**: Player MUST have an inventory that stores purchased items
- **FR-012**: Inventory MUST support equipping items to apply their stat bonuses
- **FR-013**: Inventory MUST support unequipping items to remove their stat bonuses
- **FR-014**: Inventory UI MUST display all owned items with their stat bonuses
- **FR-015**: Inventory data MUST persist in save files

**Shop System**:

- **FR-016**: Shop UI MUST display available items with names, stat bonuses, and gold prices
- **FR-017**: Player MUST be able to purchase items by spending gold currency
- **FR-018**: System MUST deduct gold when purchase is successful
- **FR-019**: System MUST block purchase and display feedback when player has insufficient gold
- **FR-020**: Shop MUST be accessible at designated shop locations in the level

**Currency System**:

- **FR-021**: Gold currency MUST have an integer value tracked per player
- **FR-022**: Enemies MUST drop gold rewards when defeated (amount varies by enemy type)
- **FR-023**: System MUST display current gold amount to the player at all times
- **FR-024**: Gold value MUST persist in save data

**Enemy Type System**:

- **FR-025**: Level MUST contain three enemy types: Normal mobs, Named mobs, and Boss mob
- **FR-026**: Normal mobs MUST have baseline stats (e.g., 30 HP, 5 Attack) and drop 15-25 gold
- **FR-027**: Named mobs MUST have elevated stats (e.g., 60 HP, 10 Attack), display name tag, and drop 40-60 gold
- **FR-028**: Boss mob MUST have high stats (e.g., 150 HP, 15 Attack), display boss UI indicator, and drop 100-150 gold
- **FR-029**: System MUST spawn appropriate quantities of each type (e.g., 4-5 Normal, 2-3 Named, 1 Boss)

**Combat with Stats**:

- **FR-030**: Player damage dealt MUST equal player's current Attack stat value
- **FR-031**: Enemy damage received MUST be reduced by player's current Defense stat
- **FR-032**: Damage reduction calculation MUST be: Damage Received = Max(Incoming Damage - Defense, 0)
- **FR-033**: System MUST display damage numbers when attacks land
- **FR-034**: Enemy defeat MUST trigger gold reward distribution to player

**Grimoire System Integration**:

- **FR-035**: Fire Grimoire MUST be equipped on level start
- **FR-036**: Fire magic ability damage MUST scale with player's Attack stat (e.g., 2.5x Attack)
- **FR-037**: Fire magic ability MUST have cooldown period (e.g., 7 seconds)
- **FR-038**: UI MUST display fire magic cooldown status

**Level Structure**:

- **FR-039**: Level MUST have designated shop access point
- **FR-040**: Level MUST have clear starting point and goal/exit point
- **FR-041**: Level completion MUST require defeating the Boss mob
- **FR-042**: System MUST provide victory feedback when level is completed

**Save/Load System**:

- **FR-043**: System MUST save player data including: current stats, inventory contents, equipped items, gold amount, level checkpoint position
- **FR-044**: Player MUST be able to manually trigger save action
- **FR-045**: System MUST auto-save at key moments (level completion, boss defeat)
- **FR-046**: Player MUST be able to load saved game data from the main menu or in-game
- **FR-047**: System MUST handle missing save files gracefully with clear messaging

**UI Requirements**:

- **FR-048**: UI MUST display a stat panel showing HP, Attack, Defense values
- **FR-049**: UI MUST display inventory interface with item icons, names, and stat bonuses
- **FR-050**: UI MUST display shop interface with purchasable items and player's gold
- **FR-051**: UI MUST display fire magic cooldown indicator

### Key Entities

- **Fire Mage**: The player character with three core stats (HP, Attack, Defense), inventory, equipment slots, and equipped Fire Grimoire
- **Stat System**: The numerical foundation determining character effectiveness (HP for survival, Attack for damage output, Defense for damage mitigation)
- **Item**: An equippable piece of equipment with stat bonuses, gold cost, and slot assignment
- **Inventory**: The player's collection of purchased items, supporting equip/unequip actions
- **Equipment Slots**: Designated positions where items can be equipped (e.g., Weapon, Armor, Accessory slots)
- **Shop**: An in-level interface where players spend gold to purchase items
- **Currency (Gold)**: The economic resource earned from defeating enemies and spent in the shop
- **Enemy Types**: Three distinct categories - Normal mob (baseline challenge), Named mob (elite challenge), Boss mob (final challenge)
- **Normal Mob**: Standard enemies with basic stats and standard gold rewards
- **Named Mob**: Elite enemies with enhanced stats, name display, and improved rewards
- **Boss Mob**: Single powerful enemy with high stats, unique UI, and large rewards
- **Save System**: Persistent storage of player progress (stats, inventory, equipment, gold, checkpoint)
- **Level**: The playable environment containing shop access, enemy spawns, and completion goal

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Stat display updates within 50 milliseconds of equipping/unequipping items
- **SC-002**: 90% of players use the shop at least once per playthrough
- **SC-003**: Players can correctly identify how Attack and Defense stats affect combat within 2 combat encounters
- **SC-004**: 95% of save/load operations complete successfully without data corruption
- **SC-005**: Players can distinguish between Normal, Named, and Boss enemies within 5 seconds of encounter based on visual indicators
- **SC-006**: Level completion rate increases by 20% when players purchase 2+ items from shop (compared to no purchases)
- **SC-007**: Average playtime for level completion is 8-15 minutes (indicating appropriate content depth)
- **SC-008**: 75% of players understand stat bonuses from items without external explanation
- **SC-009**: Boss defeat rate is 40-50% on first attempt (indicating appropriate challenge scaling)
- **SC-010**: Save/load operations complete within 2 seconds

### Technical Constraints

- Level loading with save data must complete within 5 seconds
- Maximum 8 enemies active simultaneously (performance optimization)
- Stat recalculation must complete within 50 milliseconds
- Save file size must not exceed 100KB
- Shop UI must load within 1 second of access
- All UI stat displays must update in real-time (no manual refresh required)

## Out of Scope

- Multiple levels or level selection menu
- Multiple Grimoire types (only Fire Grimoire for this feature)
- Additional enemy types beyond Normal/Named/Boss
- Crafting or item upgrading systems
- Quest or achievement systems
- Skill trees or talent point allocation
- Consumable items (potions, scrolls)
- Item rarity tiers (all items are common quality)
- Multiplayer or co-op features
- Procedurally generated levels or enemies
- Achievement tracking
- Leaderboards or online features
- Localization beyond English and Korean

## Assumptions

- Players are familiar with basic RPG concepts (stats, equipment, shops)
- Three stats (HP, Attack, Defense) are sufficient to demonstrate meaningful progression
- Fire Mage base stats are: HP 100, Attack 15, Defense 5 (balanced for level difficulty)
- Item stat bonuses range from +5 to +20 per stat
- Gold rewards from enemies provide meaningful purchasing power (Normal: 15-25, Named: 40-60, Boss: 100-150)
- Shop items cost between 50-200 gold (requires defeating 2-10 enemies to afford items)
- Defense reduces damage linearly (Damage = Incoming - Defense, minimum 0)
- Unlimited inventory slots are acceptable (no inventory management complexity)
- Fire magic damage scaling is 2.5x Attack stat
- Fire magic cooldown is 7 seconds (balanced for tactical use)
- Boss must be defeated to complete level (enforces challenge completion)
- Save data uses JSON text format (human-readable for debugging)
- Auto-save occurs at level completion and boss defeat
- Players start with 0 gold and no items equipped (progression from scratch)

## Dependencies

**Existing Systems**:
- CharacterPhysics system for player movement and collision
- GAS (Gameplay Ability System) for magic ability execution
- FSM (Finite State Machine) for character and enemy state management
- Grimoire System for Fire Grimoire functionality
- Health System for HP tracking and damage processing
- Enemy Controller for enemy behavior and AI

**New Systems Required**:
- Stat System for managing HP, Attack, Defense values and calculations
- Item System for defining equipment with stat bonuses
- Inventory System for storing, equipping, and unequipping items
- Shop System for displaying and purchasing items with gold
- Economy System for gold currency tracking and transactions
- Enemy Type System for differentiating Normal/Named/Boss enemies
- Save/Load System for persisting player progress

**Supporting Systems**:
- UI System for stat display, inventory interface, shop interface
- Visual Effects for enhanced combat feedback based on stats
- Scene Management for level loading with saved checkpoint data

## Clarifications

*(This section will be populated with Q&A from clarification sessions during implementation)*

## Success Metrics Post-Launch

After implementing this feature, we will measure:

- Average time to complete the level (target: 8-15 minutes indicating appropriate content length)
- Average number of deaths before completion (target: fewer than 8, indicating balanced difficulty)
- Shop engagement rate (target: 90%+ players use shop at least once)
- Average number of items purchased per playthrough (target: 2-3 items)
- Gold accumulation at level end (target: 150-250 gold indicating proper reward tuning)
- Fire magic ability usage frequency (target: 5-8 uses per playthrough)
- Player satisfaction with stat progression (target: 7.5/10 or higher)
- Percentage of players who understand stat impact on combat (target: 75%+)
- Boss mob defeat rate on first attempt (target: 40-50% indicating appropriate final challenge)
- Save/load reliability (target: 100% successful operations with zero data corruption)
- Inventory management ease (target: 80%+ players find equipping items intuitive)
- Shop pricing perception (target: 70%+ players feel items are fairly priced)
