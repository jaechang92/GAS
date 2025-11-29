# 📁 생성된 파일 목록

**업데이트**: 2025-11-26
**총 파일 수**: 약 120개 이상

---

## 📊 개요

GASPT 프로젝트에서 생성된 모든 스크립트 파일 목록입니다.

---

## Scripts/Core/ (15개)

```
Core/
├── Enums/
│   ├── EnemyType.cs
│   ├── EquipmentSlot.cs
│   ├── StatType.cs
│   └── StatusEffectType.cs
├── Utilities/
│   ├── GameEvents.cs
│   ├── AwaitableHelper.cs
│   └── Interfaces/
│       └── IHealthEventProvider.cs
├── ObjectPool/
│   ├── IPoolable.cs
│   ├── ObjectPool.cs
│   ├── PooledObject.cs
│   ├── PoolManager.cs
│   └── PoolInitializer.cs
├── AwaitableExtensions.cs
└── SingletonPreloader.cs
```

---

## Scripts/Data/ (4개)

```
Data/
├── Item.cs
├── SkillItem.cs
├── StatusEffectData.cs
└── (FormData, RoomData는 다른 폴더에 위치)
```

---

## Scripts/Gameplay/ (25개 이상)

```
Gameplay/
├── Player/
│   └── PlayerController.cs
├── Enemy/
│   ├── BasicMeleeEnemy.cs
│   ├── RangedEnemy.cs
│   ├── FlyingEnemy.cs
│   └── EliteEnemy.cs
├── Form/
│   ├── Core/
│   │   ├── BaseForm.cs
│   │   ├── IFormController.cs
│   │   ├── FormData.cs
│   │   └── AbilityType.cs
│   ├── FormInputHandler.cs
│   ├── Implementations/
│   │   └── MageForm.cs
│   └── Abilities/
│       ├── BaseAbility.cs
│       ├── BaseProjectileAbility.cs
│       ├── JumpAbility.cs
│       ├── FireballAbility.cs
│       ├── MagicMissileAbility.cs
│       ├── LightningBoltAbility.cs
│       └── IceBlastAbility.cs
├── Projectiles/
│   ├── Projectile.cs
│   ├── MagicMissileProjectile.cs
│   └── FireballProjectile.cs
├── Level/Room/
│   ├── RoomData.cs
│   └── EnemySpawnPoint.cs
├── Item/
│   └── SkillItemManager.cs
├── Camera/
│   └── CameraFollow.cs
└── Effects/
    └── VisualEffect.cs
```

---

## Scripts/UI/ (25개)

```
UI/
├── BaseUI.cs (구버전)
├── InventoryUI.cs [Obsolete]
├── PortalUI.cs
├── DungeonCompleteUI.cs
├── EquipmentSlotUI.cs
├── PlayerHealthBar.cs
├── PlayerManaBar.cs
├── PlayerExpBar.cs
├── DamageNumber.cs
├── DamageNumberPool.cs
├── BuffIcon.cs
├── BuffIconPanel.cs
├── ItemPickupUI.cs
├── ItemPickupSlot.cs
├── EnemyNameTag.cs
├── BossHealthBar.cs
├── RoomInfoUI.cs
├── SkillSlotUI.cs
├── SkillUIPanel.cs
├── ShopUI.cs
├── ShopItemSlot.cs
├── StatPanelUI.cs
├── UIAnimationHelper.cs
│
└── MVP/ (NEW! - Phase 6-C)
    ├── IInventoryView.cs
    ├── ItemViewModel.cs
    ├── EquipmentViewModel.cs
    ├── InventoryPresenter.cs
    └── InventoryView.cs
```

---

## Scripts/Combat/ (2개)

```
Combat/
├── DamageCalculator.cs
└── CombatTest.cs
```

---

## Scripts/Loot/ (4개)

```
Loot/
├── LootSystem.cs
├── LootEntry.cs
├── LootTable.cs
└── DroppedItem.cs
```

---

## Scripts/Inventory/ (1개)

```
Inventory/
└── InventorySystem.cs
```

---

## Scripts/Stats/ (1개)

```
Stats/
└── PlayerStats.cs
```

---

## Scripts/StatusEffects/ (3개)

```
StatusEffects/
├── StatusEffect.cs
├── StatusEffectManager.cs
└── StatusEffectTest.cs
```

---

## Scripts/Skills/ (5개)

```
Skills/
├── SkillSystem.cs
├── Skill.cs
├── SkillData.cs
└── SkillEnums.cs
```

---

## Scripts/Shop/ (1개)

```
Shop/
└── ShopSystem.cs
```

---

## Scripts/Economy/ (1개)

```
Economy/
└── CurrencySystem.cs
```

---

## Scripts/Save/ (3개)

```
Save/
├── SaveSystem.cs
├── SaveData.cs
└── SaveTest.cs
```

---

## Scripts/Resources/ (2개)

```
Resources/
├── GameResourceManager.cs
└── ResourcePaths.cs
```

---

## Scripts/Editor/ (13개)

```
Editor/
├── PrefabCreator.cs
├── GameplaySceneCreator.cs
├── LootTableCreator.cs
├── InventoryUICreator.cs
├── PhaseC3SetupCreator.cs
├── DamageNumberCreator.cs
├── EnemyUICreator.cs
├── ShopUICreator.cs
├── SkillUICreator.cs
├── SkillSystemTestSetup.cs
├── CombatTestSceneSetup.cs
├── IntegrationTestSceneSetup.cs
└── EditorUtilities.cs
```

---

## Scripts/Testing/ (10개 이상)

```
Testing/
├── BaseTestManager.cs
├── CombatTestManager.cs
├── LootSystemTest.cs
├── SkillSystemTest.cs
├── SkillItemTest.cs
├── StatusEffectTest.cs
├── SaveTest.cs
├── CombatTest.cs
├── CombatUITest.cs
└── LevelTest.cs
```

---

## 📊 Phase별 파일 생성 내역

| Phase | 생성 파일 수 | 주요 시스템 |
|-------|-------------|-------------|
| **Phase A** | 34개 | Form, Enemy AI, Room, Skill |
| **Phase B** | 25개 | Editor Tools, UI, ObjectPool |
| **Phase C** | 35개 | Loot, Inventory, Shop, Save |
| **Phase D** | 5개 | MVP Pattern (UI 리팩토링) |
| **기타** | 20개+ | Core, Utilities, Managers |
| **총계** | **120개+** | - |

---

## 🗂️ 시스템별 분류

### Core 시스템 (완성도: 100%)
- Enums, Utilities, ObjectPool, Extensions

### Gameplay 시스템 (완성도: 80%)
- Player, Enemy, Form, Projectiles, Room

### UI 시스템 (완성도: 90%)
- HUD, Inventory (MVP), Shop, Status

### 전투 시스템 (완성도: 80%)
- Combat, Damage, StatusEffects

### 아이템 시스템 (완성도: 100%)
- Loot, Inventory, Equipment, Shop

### 저장 시스템 (완성도: 100%)
- Save/Load, CurrencySystem

### 에디터 도구 (완성도: 100%)
- Prefab Creator, Scene Creator, UI Creator

---

## 📝 최근 추가 파일 (2025-11)

### MVP 패턴 (Phase 6-C)
- `UI/MVP/IInventoryView.cs` (70줄)
- `UI/MVP/ItemViewModel.cs` (75줄)
- `UI/MVP/EquipmentViewModel.cs` (60줄)
- `UI/MVP/InventoryPresenter.cs` (340줄)
- `UI/MVP/InventoryView.cs` (330줄)

---

## 🔗 관련 문서

- [Phase 히스토리](phase-history/) - Phase별 상세 작업 내역
- [최신 작업](LATEST.md) - 최근 생성/수정된 파일

---

*이 문서는 Phase 완료 시 `/update-worklog --phase` 명령으로 자동 업데이트됩니다.*
