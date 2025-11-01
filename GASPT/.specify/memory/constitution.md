<!--
╔════════════════════════════════════════════════════════════════════════╗
║                        SYNC IMPACT REPORT                              ║
╚════════════════════════════════════════════════════════════════════════╝

Version Change: 1.0.0 → 1.1.0

Rationale: MINOR version (1.1.0) - Material expansion of guidance by integrating
detailed coding standards, development workflows, and file structure guidelines
from legacy .spec folder into unified Constitution.

Modified Principles:
- EXPANDED: Detailed Coding Standards (new section)
  - Comprehensive naming conventions
  - Singleton pattern requirements
  - Code structure guidelines
  - Assembly definition rules
  - Forbidden/recommended practices

Added Sections:
- Detailed Coding Standards (comprehensive coding rules)
- Development Workflow (for AI Agents) - Complete workflow from .spec/workflows.yaml
  - Pre-Work Checklist
  - Code Writing Process
  - Code Writing Checkpoints
  - Post-Code Verification
  - Documentation Update Rules
  - Common Mistakes to Avoid
  - Work Scenarios
  - Quick Reference Commands
- File Structure Guidelines - Complete structure from .spec/file-structure.yaml
  - Scripts folder structure
  - Naming conventions
  - File placement rules
  - Best practices
  - Architecture reference

Removed Sections:
- None

Integration Notes:
- Migrated .spec/coding-rules.yaml → Detailed Coding Standards
- Migrated .spec/workflows.yaml → Development Workflow
- Migrated .spec/file-structure.yaml → File Structure Guidelines
- Referenced .spec/architecture.yaml → Architecture Reference section
- Legacy .spec folder can now be archived

Templates Requiring Updates:
✅ plan-template.md - No changes needed, compatible
✅ spec-template.md - No changes needed, compatible
✅ tasks-template.md - No changes needed, compatible

Follow-up TODOs:
- Update .claude/CODE_CONTEXT.md to reference Constitution
- Archive .spec folder to ../specs/.spec_backup/
- Update README.md to mention .specify system

Last Updated: 2025-10-25
-->

# GASPT Constitution

## Core Principles

### I. Completion-First Development

**완벽한 시스템보다 플레이 가능한 게임을 먼저 만든다.**

- Every feature MUST deliver a playable experience before architectural perfection
- Existing features MUST be completed before adding new features
- Features MUST be testable and demonstrable in isolation
- "Working game" takes precedence over "perfect system"

**Rationale**: Focuses development effort on delivering tangible value. Prevents
feature creep and ensures continuous playability throughout development.

---

### II. Incremental Development & Testing

**작은 단위로 나누어 개발하고 지속적으로 테스트한다.**

- Each development step MUST maintain a playable state
- Features MUST be broken down into independently testable units
- Foundation systems MUST be established before content expansion
- Continuous testing at each stage is MANDATORY

**Rationale**: Reduces risk, enables rapid iteration, and maintains project
stability. Small increments make debugging easier and progress visible.

---

### III. Productivity-First Architecture

**시스템 설계에 충분한 시간을 투자하여 장기 생산성을 확보한다.**

- Invest adequate time in system design BEFORE implementation
- Design for reusable components (Composition over Inheritance)
- Minimize code duplication through proper modularization
- ScriptableObject-based data management for flexibility

**Rationale**: Upfront design investment pays dividends in reduced maintenance
costs and faster feature development later in the project lifecycle.

---

### IV. Player Experience Priority

**복잡한 시스템보다 직관적이고 재미있는 게임플레이가 우선이다.**

- Intuitive and engaging gameplay over complex systems
- Target mid-level gamers with appropriate difficulty
- Optimize for short play sessions
- Player feedback systems MUST be clear and responsive

**Rationale**: Technical excellence means nothing if the game isn't fun. All
systems exist to serve player experience, not the other way around.

---

### V. Code Design Standards (SOLID, OOP)

**OOP 및 SOLID 원칙을 준수한다.**

- Single Responsibility Principle (SRP) MUST be followed
- Prefer composition over inheritance
- Clear separation of concerns (Core, Gameplay, UI layers)
- CamelCase naming convention REQUIRED
- No underscore prefix in variable names (except Unity serialized private fields)

**Rationale**: SOLID principles create maintainable, extensible, and testable
code. Consistent naming conventions improve readability across the team.

---

### VI. Async Pattern Enforcement (NON-NEGOTIABLE)

**모든 비동기 작업은 async/await + Awaitable을 사용하며 Coroutine 사용을 금지한다.**

- ALL async operations MUST use `async Awaitable` pattern
- Unity Coroutines (`IEnumerator`, `yield return`) are STRICTLY FORBIDDEN
- Use `Awaitable.NextFrameAsync()` instead of `yield return null`
- Use `Awaitable.WaitForSecondsAsync(time)` instead of `WaitForSeconds`
- Test methods MUST use `[Test]` with `async void`, NOT `[UnityTest]` with `IEnumerator`

**Rationale**: Unity 2023+ Awaitable provides better performance, type safety,
cancellation support, and async/await compatibility. This is non-negotiable to
maintain modern async patterns throughout the codebase.

**Examples**:
```csharp
// ✅ CORRECT
public async Awaitable SomeMethod()
{
    await Awaitable.NextFrameAsync();
    await Awaitable.WaitForSecondsAsync(1f);
}

// ❌ FORBIDDEN
public IEnumerator SomeMethod()
{
    yield return null;
    yield return new WaitForSeconds(1f);
}
```

---

### VII. Localization & Encoding

**한글 주석과 변수명을 허용하며 UTF-8 인코딩을 준수한다.**

- Korean comments and variable names are ALLOWED and ENCOURAGED
- UTF-8 encoding MUST be maintained across all text files
- When Korean text breaks during file edits, use MultiEdit for targeted recovery
- .gitattributes and .editorconfig MUST enforce UTF-8 encoding

**Rationale**: Enables native language development for Korean team while
maintaining international collaboration capability. Explicit encoding rules
prevent common encoding issues.

---

### VIII. Token Efficiency

**파일 수정 시 토큰 효율성을 고려한다.**

- Small changes MUST use Edit/MultiEdit tools (token-efficient)
- Large structural changes MAY warrant full file rewrite
- Korean comment recovery MUST use MultiEdit for individual lines only
- Avoid full file rewrites when Edit tools suffice

**Rationale**: Optimizes AI assistant token usage, enabling more complex
operations within token budgets. Targeted edits are faster and less error-prone.

---

### IX. Unity 6.0+ Compatibility

**Unity 6.0 이상 버전의 최신 API를 사용하며 deprecated 코드를 피한다.**

- Use `linearVelocity` instead of deprecated `velocity`
- Use `FindAnyObjectByType` instead of deprecated `FindObjectOfType`
- Watch for CS0618 warnings and address immediately
- Stay current with Unity's API evolution for Unity 2023.3+

**Rationale**: Prevents technical debt from deprecated APIs. Ensures codebase
remains compatible with future Unity versions and benefits from performance
improvements.

---

## Detailed Coding Standards

### Naming Conventions

**Variables (Private Fields)**:
- Style: `camelCase`
- NO underscores: `moveSpeed` ✅ | `_moveSpeed` ❌ | `move_speed` ❌
- Examples: `moveSpeed`, `jumpForce`, `maxHealth`, `isGrounded`

**Properties**:
- Style: `PascalCase`
- Examples: `MaxHealth`, `IsAlive`, `CurrentState`

**Methods**:
- Style: `PascalCase`
- Examples: `CalculateVelocity()`, `OnStateEnter()`, `UpdateMovement()`

**Classes**:
- Style: `PascalCase` with suffix
- Suffixes: `Manager`, `State`, `Data`, `System`, `Controller`
- Examples: `GameFlowManager`, `PlayerAttackState`, `EnemyData`, `DamageSystem`

**Constants**:
- Style: `PascalCase`
- Examples: `MaxComboCount`, `DefaultFadeDuration`

---

### Singleton Pattern (Manager Classes)

**REQUIRED**: All Manager classes MUST use `SingletonManager<T>`

```csharp
// ✅ CORRECT
using Core;

public class GameFlowManager : SingletonManager<GameFlowManager>
{
    protected override void OnSingletonAwake()
    {
        // Initialization logic
    }
}

// ❌ FORBIDDEN - Manual singleton implementation
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

**Location**: `Assets/_Project/Scripts/Core/Utilities/SingletonManager.cs`

---

### Code Structure

**Using Order**:
1. System namespaces
2. UnityEngine namespaces
3. Third-party namespaces
4. Custom namespaces

**Member Order**:
1. Serialized fields (`[SerializeField]`)
2. Public properties
3. Private fields
4. Unity callbacks (Awake, Start, Update, etc.)
5. Public methods
6. Protected methods
7. Private methods

**Method Size**: Max 50 lines per method (prefer smaller methods)

**File Size**: 500 lines warning threshold → consider splitting file

**Splitting Strategies**:
- State-based: `GameState.cs` → `MainState.cs`, `LoadingState.cs`
- Feature-based: `PlayerController.cs` → `PlayerMovement.cs`, `PlayerCombat.cs`
- Partial classes: For large UI managers

---

### Comments

**Language**: Korean allowed and encouraged

**Style**:
```csharp
/// <summary>
/// 한글로 메서드 설명
/// </summary>
/// <param name="direction">이동 방향</param>
/// <returns>계산된 속도 벡터</returns>
public Vector2 CalculateVelocity(Vector2 direction)
{
    // 속도 계산 로직 (inline Korean comments allowed)
    return direction * moveSpeed;
}
```

---

### Assembly Definitions

**Naming**: `[FolderName].asmdef`

**Required For**:
- New independent systems
- Major folders (Player, Combat, Enemy)
- Plugins (GAS_Core, FSM_Core)

**Existing Assemblies**:
- `FSM.Core.asmdef` (no dependencies)
- `GAS.Core.asmdef` (no dependencies)
- `Core.Utilities.asmdef` (no dependencies)
- `Core.Managers.asmdef` (depends on FSM.Core, Core.Utilities)
- `Combat.asmdef` (depends on GAS.Core, FSM.Core, Core.Utilities)
- `Player.asmdef` (depends on FSM.Core, Combat)
- `Enemy.asmdef` (depends on FSM.Core, Combat)
- `Combat.Demo.asmdef` (depends on Combat, Player, Enemy, Core.Utilities)

**CRITICAL**: Always check for circular references before adding assembly references

---

### Forbidden Practices

**Strictly Forbidden**:
- Coroutines (`IEnumerator`, `yield return`)
- Manual Singleton implementation
- Underscore prefix in private fields (except Unity serialized)
- Deprecated Unity APIs (FindObjectOfType, velocity, etc.)
- Files exceeding 500 lines without justification
- Methods exceeding 50 lines
- Code duplication
- Magic numbers (use named constants)

**Discouraged**:
- Deep folder nesting (5+ levels)
- Ambiguous naming (Stuff, Misc, Other)
- Assets in project root
- Temporary folders (Temp, Test)

---

### Recommended Practices

**Strongly Recommended**:
- ScriptableObject-based data management
- Event-based communication (minimize direct references)
- Component composition pattern (Composition over Inheritance)
- Transform-based physics (minimize Rigidbody2D usage)
- Unit test coverage for core systems

---

## Development Standards

### Technology Stack

**Required:**
- Unity 2023.3 or higher
- C# 11 with async/await (Awaitable)
- Transform-based custom physics (Physics2D NOT used for player movement)
- 2D Sprite + Sorting Layers
- Animator + Animation Events
- ScriptableObject for data management

**Planned Migration:**
- Legacy Input → New Input System (future roadmap)

**Architecture Patterns:**
- Component composition pattern (Composition over Inheritance)
- `async Awaitable` for ALL async operations (Coroutines FORBIDDEN)
- SOLID principles, particularly SRP (Single Responsibility Principle)

---

### Project Structure

```
GASPT/
├── Assets/
│   ├── _Project/              # Project-specific assets
│   │   ├── Scripts/           # Game logic (Core, Gameplay, UI)
│   │   ├── Art/               # Art assets
│   │   ├── Prefabs/           # Prefabs
│   │   └── Scenes/            # Scene files
│   └── Plugins/               # Independent systems
│       ├── FSM_Core/          # Finite State Machine (Awaitable-based)
│       └── GAS_Core/          # Gameplay Ability System
└── docs/                      # Project documentation
    ├── getting-started/
    ├── development/
    ├── architecture/
    ├── guides/
    └── testing/
```

---

### Assembly Architecture

- **GAS_Core.asmdef**: Gameplay Ability System (no external dependencies)
- **FSM_Core.asmdef**: Finite State Machine (no external dependencies)
- **GASPT_Scripts.asmdef**: Main game scripts (depends on GAS_Core, FSM_Core)

**Dependency Rules:**
- Core systems (GAS, FSM) MUST remain independent
- Game scripts MAY depend on core systems
- No circular dependencies allowed

---

## Code Review Requirements

### Before Committing

All code MUST pass these gates before commit:

1. **Async Pattern Check**: Verify NO Coroutines (`IEnumerator`, `yield return`)
2. **Unity 6.0 Check**: Verify NO deprecated API usage (CS0618 warnings)
3. **Naming Convention**: CamelCase enforced, no underscores (except Unity serialized private)
4. **SOLID Check**: Single Responsibility Principle verification
5. **Encoding Check**: Korean comments preserved, UTF-8 encoding intact

### During Code Review

- Verify feature delivers playable experience (Principle I)
- Ensure incremental testability (Principle II)
- Check for code reuse opportunities (Principle III)
- Validate player experience impact (Principle IV)
- Confirm async pattern compliance (Principle VI - NON-NEGOTIABLE)

---

## Development Workflow (for AI Agents)

### Pre-Work Checklist (CRITICAL)

**Step 1: Read Constitution**
- Read `.specify/memory/constitution.md` (this file) FIRST
- Review all Core Principles and Detailed Coding Standards

**Step 2: Check Current Status**
- Read `docs/development/CurrentStatus.md` (MUST READ)
- Review recent work history
- Check current Phase and completed tasks
- Review "다음 작업 예정" (next work planned)
- Check "수정된 버그" (fixed bugs)

**Step 3: Search Related Documentation**
- Use: `find docs -name '*.md' | grep -i [keyword]`
- Use: `Grep: '[keyword]' pattern: 'docs/**/*.md'`
- WARNING: Do NOT duplicate existing documentation

**Step 4: Find Similar Code Patterns**
- For Manager classes → Read existing Manager classes
- For State classes → Read similar State classes
- For UI classes → Read existing UI classes
- For Data classes → Read existing Data ScriptableObjects
- ALWAYS follow existing patterns

---

### Code Writing Process

**Step 1: Verify Naming**
- Check: `camelCase` for private fields (NO underscores)
- Check: `PascalCase` for methods/properties
- Check: `PascalCase` for classes with suffix (Manager, State, Data)

**Step 2: Verify Async Pattern**
- ✅ Use: `async Awaitable`
- ❌ NEVER: `IEnumerator`, `yield return`

**Step 3: Verify Singleton (for Managers)**
- ✅ Use: `SingletonManager<T>`
- ✅ Implement: `OnSingletonAwake()`
- ❌ NEVER: Manual Singleton implementation

**Step 4: Verify Unity API**
- ✅ Use: `FindAnyObjectByType`, `linearVelocity`
- ❌ Avoid: `FindObjectOfType` (deprecated), `velocity` (deprecated)

**Step 5: Check Assembly Definitions**
- Is this a new folder/system?
- Does it need a new `.asmdef`?
- Check for circular references

---

### Code Writing Checkpoints

**Consistency Check (every class)**:
- Variable naming style matches existing code
- Comment style matches existing code
- Method order matches existing code
- Using directive order matches existing code

**Pattern Following**:
- For Manager classes: Follow other Manager class structure
- For State classes: Follow other State class structure
- For UI classes: Follow other UI class structure
- For Data classes: Follow other Data class structure

**File Size Check**:
- Max 500 lines → consider splitting if exceeded

---

### Post-Code Verification

**Step 1: Impact Analysis**
- Use: `Grep: 'ChangedClassName' pattern: '*.cs' output_mode: 'files_with_matches'`
- Check all referencing files still work
- Check for compilation errors
- Check existing features aren't broken

**Step 2: Final Consistency Check**
- Naming conventions match
- Async patterns match
- Comment style matches
- Code structure matches

**Step 3: Test Plan**
- Can it be tested with Demo scripts?
- Does it need Unit Tests?
- Did you add test instructions to documentation?

---

### Documentation Update Rules

**When to Update CurrentStatus.md**:
- New system completed
- Phase progress changed
- Bug fixed
- Major feature added
- Test tool added

**Sections to Update**:
- `## 최근 작업` - Add today's work
- `## 수정된 버그` - Add fixed bugs
- `## 다음 작업 예정` - Add new TODOs

**When to Create New Documentation**:
- New system design document → `docs/development/`
- New user guide → `docs/getting-started/`
- Demo test checklist → `docs/testing/`
- System architecture → `docs/infrastructure/`
- Work log → `docs/archive/`

---

### Common Mistakes to Avoid

**Mistake 1: Not Checking Existing Documentation**
- Solution: `find docs -name '*.md' | grep -i [keyword]`
- Example: Creating coding convention doc when CodingGuidelines.md exists

**Mistake 2: Ignoring Existing Patterns**
- Solution: Read similar class types first
- Example: Manual Singleton when `SingletonManager<T>` exists

**Mistake 3: Inconsistent Code Style**
- Solution: Check existing code variable/method naming
- Example: Using `snake_case` when project uses `camelCase`

**Mistake 4: Using Deprecated APIs**
- Solution: Reference this Constitution (Principle IX)
- Example: `FindObjectOfType` → `FindAnyObjectByType`

**Mistake 5: Not Updating CurrentStatus.md**
- Solution: Update immediately after work completion
- Impact: Duplicate work in next session

**Mistake 6: Duplicate Work**
- Solution: Grep for existing implementations
- Example: Creating new Singleton when `SingletonManager` exists

**Mistake 7: Not Checking Impact Scope**
- Solution: Grep all references and update them
- Example: `ResourceManager` → `GameResourceManager` requires updating 6 files

---

### Work Scenarios

**New Manager Class**:
1. Read other Manager classes (`GameFlowManager`, `SceneLoader`)
2. Inherit from `SingletonManager<T>`
3. Implement `OnSingletonAwake()`
4. Check `.asmdef` (`Core.Managers.asmdef`)
5. Update `CurrentStatus.md`

**New State Class**:
1. Read similar State classes from same Entity
2. Inherit from `BaseState` or `GameState`
3. Implement `OnEnter`/`OnExit`/`OnUpdate`
4. Use `Awaitable` for async operations
5. Register state in FSM

**New System**:
1. Write design document first (`docs/development/`)
2. Create `.asmdef` (if needed)
3. Check reference relationships (avoid circular dependencies)
4. Write Demo/Test scripts
5. Update `CurrentStatus.md`

**Refactoring**:
1. Identify impact scope (Grep)
2. Update all related files
3. Check compilation errors
4. Run tests
5. Update documentation

**Bug Fix**:
1. Verify bug reproduction
2. Identify root cause
3. Apply fix
4. Verify with tests
5. Update CurrentStatus.md "수정된 버그" section

---

### Quick Reference Commands

**Find Documentation**:
```bash
find docs -name '*.md' | grep -i [keyword]
```

**Find Code**:
```bash
Grep: '[ClassName]' pattern: '*.cs' output_mode: 'files_with_matches'
```

**Find Patterns**:
```bash
Glob: '**/*Manager.cs'  # Manager classes
Glob: '**/*State.cs'    # State classes
Glob: '**/*Data.cs'     # Data classes
```

---

## File Structure Guidelines

### Scripts Folder Structure

**Core/**:
- `Managers/` - Game managers (`[Name]Manager.cs`, inherit `SingletonManager<T>`)
  - `GameFlowManager.cs`, `SceneLoader.cs`, `SceneTransitionManager.cs`, `GameResourceManager.cs`
- `Utilities/` - Helper classes, extensions
  - `SingletonManager.cs`, `Extensions.cs`, `DebugHelper.cs`
- `Enums/` - Project-wide enums
  - `SceneType.cs`, `DamageType.cs`, `ResourceCategory.cs`
- `Bootstrap/` - Game initialization
  - `BootstrapManager.cs`

**Gameplay/**:
- `Player/` (Assembly: `Player.asmdef`)
  - `PlayerController.cs`, `CharacterPhysics.cs`, `InputHandler.cs`
  - `States/` - `PlayerIdleState.cs`, `PlayerMoveState.cs`, `PlayerJumpState.cs`, etc.
- `Combat/` (Assembly: `Combat.asmdef`)
  - `Core/` - `DamageSystem.cs`, `HealthSystem.cs`, `ComboSystem.cs`
  - `Hitbox/` - `HitboxController.cs`
  - `Attack/` - `BasicAttack.cs`, `AttackAnimationHandler.cs`
  - `Data/` - `DamageData.cs`, `HitData.cs`, `ComboData.cs`
- `Enemy/` (Assembly: `Enemy.asmdef`)
  - `EnemyController.cs`, `EnemyBaseState.cs`
  - `States/` - `EnemyIdleState.cs`, `EnemyPatrolState.cs`, etc.
  - `Data/` - `EnemyData.cs`
- `Entities/` - Other game entities (NPC, Projectile, etc.)
- `Systems/` - Gameplay systems (Inventory, Quest, etc.)

**UI/**:
- `HUD/` - In-game HUD (`HUDManager.cs`, `HealthBarUI.cs`, `ItemSlotUI.cs`, etc.)
- `Menus/` - Menu UIs (`MainMenu.cs`, `PauseMenu.cs`, `SettingsMenu.cs`)

**Data/**:
- ScriptableObject data (`EnemyData.cs`, `ComboData.cs`, `SkullData.cs`)

**Tests/**:
- `Demo/` (Assembly: `Combat.Demo.asmdef`) - Demos and prototypes
  - `PlayerCombatDemo.cs`, `EnemyCombatDemo.cs`
- `Unit/` - Unit tests
  - `DamageSystemTests.cs`, `ComboSystemTests.cs`

**Editor/**:
- Editor extension tools (`SceneSetupTool.cs`, `HUDPrefabCreator.cs`)

---

### Naming Conventions

**Folders**: `PascalCase`, use plural (`Scripts`, `Prefabs`, `Animations`, `Materials`)

**Special Prefix**: `_Project` (underscore for top-level project folder)

**Files**:
- Scripts: `PascalCase` with suffix
  - Managers: `[Name]Manager.cs`
  - States: `[Entity][Action]State.cs`
  - Data: `[Type]Data.cs`
  - Systems: `[Name]System.cs`
  - Controllers: `[Name]Controller.cs`
  - UI: `[Name]UI.cs` or `[Name]Panel.cs`
- Prefabs: `PascalCase` (e.g., `PlayerCharacter.prefab`, `FireballEffect.prefab`)
- Scenes: `PascalCase` (e.g., `Bootstrap.unity`, `Gameplay.unity`)
- Sprites: `PascalCase` descriptive (e.g., `PlayerIdle_01.png`, `EnemyWalk_02.png`)

---

### File Placement Rules

**Manager Classes**:
- Location: `Assets/_Project/Scripts/Core/Managers/`
- Naming: `[Name]Manager.cs`
- Base Class: `SingletonManager<T>`

**State Classes**:
- Location: `[Entity]/States/`
- Naming: `[Entity][Action]State.cs`
- Base Class: `BaseState` or `GameState`
- Examples:
  - `Assets/_Project/Scripts/Gameplay/Player/States/PlayerAttackState.cs`
  - `Assets/_Project/Scripts/Gameplay/Enemy/States/EnemyChaseState.cs`

**Data Classes**:
- Location: `[System]/Data/`
- Naming: `[Type]Data.cs`
- Base Class: `ScriptableObject`
- Examples:
  - `Assets/_Project/Scripts/Gameplay/Enemy/Data/EnemyData.cs`
  - `Assets/_Project/Scripts/Gameplay/Combat/Data/ComboData.cs`

**System Classes**:
- Location: `Assets/_Project/Scripts/Gameplay/[System]/Core/`
- Naming: `[Name]System.cs`
- Examples:
  - `Assets/_Project/Scripts/Gameplay/Combat/Core/DamageSystem.cs`
  - `Assets/_Project/Scripts/Gameplay/Combat/Core/ComboSystem.cs`

**UI Classes**:
- Location: `Assets/_Project/Scripts/UI/[Category]/`
- Naming: `[Name]UI.cs` or `[Name]Panel.cs`
- Examples:
  - `Assets/_Project/Scripts/UI/HUD/HealthBarUI.cs`
  - `Assets/_Project/Scripts/UI/HUD/ResourcePanel.cs`

**Demo Scripts**:
- Location: `Assets/_Project/Scripts/Tests/Demo/`
- Naming: `[System]Demo.cs`
- Examples: `PlayerCombatDemo.cs`, `EnemyCombatDemo.cs`

**Editor Tools**:
- Location: `Assets/_Project/Scripts/Editor/`
- Naming: `[Tool]Tool.cs` or `[Tool]Editor.cs`
- Examples: `SceneSetupTool.cs`, `HUDPrefabCreator.cs`

---

### Best Practices

**DO**:
- Create new assets in appropriate folders immediately
- Group related files in same folder
- Maintain consistent naming
- Clean up unnecessary files regularly
- Use Assembly Definitions to reduce compilation time

**DON'T**:
- Create files in Assets root
- Abuse temporary folders (Temp, Test)
- Create deeply nested folders (5+ levels)
- Use ambiguous naming (Stuff, Misc, Other)
- Create duplicate files

---

### Architecture Reference

For detailed architecture information, refer to:
- `docs/architecture/PROJECT_ARCHITECTURE.md` - Overall system architecture
- `docs/architecture/ASSEMBLY_ARCHITECTURE.md` - Assembly dependency structure
- `docs/architecture/ARCHITECTURE_DIAGRAMS.md` - Visual diagrams

**Core Systems**:
- **GAS (Gameplay Ability System)**: `Assets/Plugins/GAS_Core/`
- **FSM (Finite State Machine)**: `Assets/Plugins/FSM_Core/`
- **GameFlow System**: `Assets/_Project/Scripts/Core/Managers/`

**Key Architecture Patterns**:
- Manager Pattern: `SingletonManager<T>` for all managers
- State Pattern: FSM-based state management (Awaitable-based)
- Data Pattern: ScriptableObject-based data management
- Event Pattern: C# events for decoupled communication

---

## Governance

**Constitution Authority**: This constitution supersedes all other development
practices and guidelines.

**Amendment Process**:
1. Amendments MUST be documented with rationale
2. Version MUST be incremented according to semantic versioning:
   - MAJOR: Backward incompatible governance changes or principle removal
   - MINOR: New principle added or material expansion of guidance
   - PATCH: Clarifications, wording fixes, non-semantic refinements
3. All dependent templates MUST be synchronized after amendments
4. Migration plan REQUIRED for breaking changes

**Compliance Verification**:
- All PRs/reviews MUST verify constitution compliance
- Complexity introduced MUST be justified against Principle III
- Violations of NON-NEGOTIABLE principles (e.g., Principle VI) result in
  automatic rejection

**Runtime Development Guidance**: For AI agent-specific development guidance,
refer to global configuration at `C:\Users\JaeChang\.claude\CLAUDE.md` and
project-specific README.md.

**Principle Conflicts**: When principles appear to conflict, prioritize in this
order:
1. Player Experience (IV) - The game must be fun
2. Completion-First (I) - Deliver working features
3. Async Pattern (VI) - Non-negotiable technical standard
4. All other principles equally weighted

---

**Version**: 1.1.0 | **Ratified**: 2025-10-25 | **Last Amended**: 2025-10-25
