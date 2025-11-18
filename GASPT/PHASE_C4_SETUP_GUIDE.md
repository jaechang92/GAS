# Phase C-4 자동 설정 가이드

**작성일**: 2025-11-18
**Phase**: C-4 - 아이템 드롭 및 장착 시스템
**목적**: LootTable 및 Inventory UI 자동 생성 가이드

---

## 📋 목차

1. [개요](#개요)
2. [사전 준비](#사전 준비)
3. [자동 설정 단계](#자동-설정-단계)
4. [수동 설정 (선택)](#수동-설정-선택)
5. [테스트](#테스트)
6. [문제 해결](#문제-해결)

---

## 개요

Phase C-4는 **아이템 드롭 및 인벤토리 시스템**을 구현합니다.

### 주요 기능

1. **아이템 드롭**: 적 처치 시 확률적으로 아이템 드롭
2. **아이템 획득**: 드롭된 아이템을 플레이어가 자동 획득
3. **인벤토리 UI**: I키로 인벤토리 열기/닫기, 아이템 목록 표시
4. **장비 시스템**: 아이템 장착/해제, 장비 슬롯 UI

### 이미 구현된 시스템 (재사용)

- **LootSystem.cs**: 아이템 드롭 관리
- **LootTable.cs**: 확률 기반 드롭 테이블
- **DroppedItem.cs**: 월드 아이템 오브젝트
- **InventorySystem.cs**: 인벤토리 관리
- **PlayerStats.cs**: 장비 장착/해제
- **Enemy.cs**: `DropLoot()` 메서드 (이미 구현됨!)

### 신규 작성 파일

**UI 스크립트** (2개):
- `InventoryUI.cs` (~400줄)
- `EquipmentSlotUI.cs` (~200줄)

**에디터 도구** (2개):
- `LootTableCreator.cs` (~300줄)
- `InventoryUICreator.cs` (~700줄)

**총 코드량**: ~1,600줄

---

## 사전 준비

### 필수 확인 사항

1. **Unity Scene**:
   - GameplayScene 열기
   - Canvas가 Scene에 존재해야 함
   - Player 오브젝트 존재 확인

2. **기존 에셋**:
   - `Assets/_Project/Data/Items/Equipment/FireSword.asset` ✅
   - `Assets/_Project/Data/Items/Equipment/LeatherArmor.asset` ✅
   - `Assets/_Project/Data/Items/Equipment/IronRing.asset` ✅

3. **기존 프리팹**:
   - `Assets/Resources/Prefabs/UI/DroppedItem.prefab` ✅

---

## 자동 설정 단계

### Step 1: LootTable 생성

**목적**: Normal/Elite/Boss용 LootTable 3개 자동 생성

1. Unity 에디터 메뉴: `Tools > GASPT > Loot > LootTable Creator`

2. 창이 열리면 **"🎲 모든 LootTable 생성"** 클릭

3. 생성된 에셋 확인:
   ```
   Assets/_Project/Data/Loot/
   ├── NormalLootTable.asset  (30% 드롭 확률)
   ├── EliteLootTable.asset   (60% 드롭 확률)
   └── BossLootTable.asset    (100% 드롭 확률)
   ```

4. **Console 로그 확인**:
   ```
   [LootTableCreator] NormalLootTable 생성 완료 (30% 드롭 확률)
   [LootTableCreator] EliteLootTable 생성 완료 (60% 드롭 확률)
   [LootTableCreator] BossLootTable 생성 완료 (100% 드롭)
   [LootTableCreator] 모든 LootTable 생성 완료!
   ```

---

### Step 2: EnemyData에 LootTable 연결

**목적**: 각 적 타입에 LootTable 할당

1. **PlatformerEnemy_Goblin** (일반 적):
   - Project 창에서 `Assets/_Project/Data/Enemies/PlatformerEnemy_Goblin.asset` 선택
   - Inspector에서 **Loot Table** 필드에 `NormalLootTable` 드래그

2. **PlatformerEnemy_EliteOrc** (정예 적):
   - `Assets/_Project/Data/Enemies/PlatformerEnemy_EliteOrc.asset` 선택
   - **Loot Table** 필드에 `EliteLootTable` 드래그

3. **BossEnemy_FireDragon** (보스):
   - `Assets/_Project/Data/Enemies/BossEnemy_FireDragon.asset` 선택
   - **Loot Table** 필드에 `BossLootTable` 드래그

4. **저장**: `Ctrl + S`

---

### Step 3: Inventory UI 생성

**목적**: 인벤토리 UI 자동 생성

1. Unity 에디터 메뉴: `Tools > GASPT > UI > Create Inventory UI`

2. 창이 열리면 **"🎨 모든 UI 자동 생성"** 클릭

3. 생성된 UI 확인:
   - **Scene Hierarchy**:
     ```
     Canvas
     └── InventoryPanel (InventoryUI 컴포넌트)
         ├── Background
         ├── TitleText ("인벤토리")
         ├── ItemListPanel (ScrollView)
         │   └── Viewport
         │       └── Content
         ├── EquipmentPanel
         │   ├── WeaponSlot (EquipmentSlotUI)
         │   ├── ArmorSlot (EquipmentSlotUI)
         │   └── RingSlot (EquipmentSlotUI)
         └── CloseButton
     ```

   - **Prefabs**:
     ```
     Assets/Resources/Prefabs/UI/
     └── ItemSlot.prefab
     ```

4. **Console 로그 확인**:
   ```
   [InventoryUICreator] ItemSlot 프리팹 생성 완료
   [InventoryUICreator] InventoryPanel 생성 완료
   [InventoryUICreator] 모든 UI 생성 완료!
   ```

---

### Step 4: 테스트

1. **Play 버튼** 클릭

2. **I키** 눌러서 인벤토리 열기 확인

3. **적 처치** 후 아이템 드롭 확인

4. **아이템 획득** (플레이어 충돌) 확인

5. **인벤토리**에서 아이템 장착/해제 테스트

---

## 수동 설정 (선택)

자동 생성 도구를 사용하지 않고 수동으로 설정하려면:

### LootTable 수동 생성

1. Project 창에서 우클릭: `Create > GASPT > Loot > LootTable`

2. 이름: `NormalLootTable`

3. Inspector에서 설정:
   - **Loot Entries** 크기: 3
   - Entry 0:
     - Item: `FireSword`
     - Drop Chance: `0.1` (10%)
   - Entry 1:
     - Item: `LeatherArmor`
     - Drop Chance: `0.1` (10%)
   - Entry 2:
     - Item: `IronRing`
     - Drop Chance: `0.1` (10%)
   - **Allow No Drop**: ✅ (체크)

4. 같은 방식으로 `EliteLootTable` (각 20%), `BossLootTable` (각 40%, 30%, 30%) 생성

### Inventory UI 수동 생성

1. Canvas 자식으로 빈 GameObject 생성, 이름: `InventoryPanel`

2. `InventoryUI` 컴포넌트 추가

3. 하위 UI 요소들 수동 생성 (Background, TitleText, ItemListPanel, EquipmentPanel, CloseButton)

4. Inspector에서 모든 참조 연결

---

## 테스트

자세한 테스트 항목은 **PHASE_C4_TEST_CHECKLIST.md** 참고

### 빠른 테스트

1. **드롭 테스트**:
   - Play 모드 진입
   - 적 처치
   - 아이템이 드롭되는지 확인 (30% 확률)

2. **획득 테스트**:
   - 드롭된 아이템에 플레이어 접근
   - 자동 획득 확인

3. **인벤토리 테스트**:
   - I키 눌러서 인벤토리 열기
   - 획득한 아이템 목록 확인
   - "장착" 버튼 클릭
   - StatPanel에서 스탯 변경 확인

4. **장비 슬롯 테스트**:
   - 인벤토리 오른쪽의 장비 슬롯 확인
   - 장착된 아이템 아이콘 표시 확인
   - 슬롯 클릭 시 장착 해제 확인

---

## 문제 해결

### 문제 1: LootTable 생성 실패

**증상**: LootTableCreator에서 "아이템을 찾을 수 없습니다" 에러

**원인**: Item 에셋 경로가 잘못됨

**해결**:
1. Project 창에서 Item 에셋 위치 확인
2. `LootTableCreator.cs` 의 `ItemPath` 상수 수정
3. 경로: `Assets/_Project/Data/Items/Equipment/`

---

### 문제 2: 인벤토리 UI가 열리지 않음

**증상**: I키를 눌러도 인벤토리가 열리지 않음

**원인**: InventoryUI 컴포넌트가 비활성화되어 있거나 참조 누락

**해결**:
1. Hierarchy에서 `InventoryPanel` 선택
2. Inspector에서 `InventoryUI` 컴포넌트 확인
3. `inventoryPanel` 참조가 자기 자신을 가리키는지 확인
4. `itemListContent` 참조가 `Content` 오브젝트를 가리키는지 확인

---

### 문제 3: 아이템 드롭이 안 됨

**증상**: 적을 처치해도 아이템이 드롭되지 않음

**원인**: EnemyData에 LootTable이 연결되지 않음

**해결**:
1. Project 창에서 EnemyData 에셋 선택
2. Inspector에서 **Loot Table** 필드 확인
3. LootTable 에셋 드래그 앤 드롭

---

### 문제 4: DroppedItem 프리팹이 없음

**증상**: "DroppedItem 프리팹을 찾을 수 없습니다" 에러

**원인**: DroppedItem.prefab이 없거나 경로가 잘못됨

**해결**:
1. `Resources/Prefabs/UI/DroppedItem.prefab` 경로 확인
2. 프리팹이 없으면 DroppedItemCreator 에디터 도구 사용 (이미 존재함)

---

### 문제 5: 장착해도 스탯이 변하지 않음

**증상**: 아이템 장착 시 스탯이 변경되지 않음

**원인**: PlayerStats의 Dirty Flag가 업데이트되지 않음

**해결**:
1. Console에서 `[PlayerStats] FireSword 장착 완료` 로그 확인
2. StatPanelUI가 Scene에 있는지 확인
3. StatPanelUI의 이벤트 구독 확인

---

## 다음 단계

Phase C-4 완료 후:

1. **통합 테스트**: 던전 시작 → 적 처치 → 아이템 획득 → 장착 → 스탯 확인
2. **커밋**: `git add .` && `git commit -m "기능: Phase C-4 아이템 드롭 및 인벤토리 시스템 완성"`
3. **Phase D**: 추가 Form 구현 또는 기능 확장

---

**작성자**: Claude Code
**버전**: 1.0
**최종 수정일**: 2025-11-18
