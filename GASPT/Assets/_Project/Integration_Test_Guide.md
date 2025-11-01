# Phase 3-5 통합 테스트 가이드

**작성일**: 2025-11-02
**대상**: Phase 3 (Stat System) + Phase 4 (Shop & Economy) + Phase 5 (Enemy System)
**목적**: 모든 RPG 시스템이 올바르게 통합되어 동작하는지 검증

---

## 📋 테스트 개요

### 완료된 Phase
- ✅ **Phase 3**: Stat System (PlayerStats, Item, StatPanelUI)
- ✅ **Phase 4**: Shop & Economy (CurrencySystem, InventorySystem, ShopSystem, ShopUI)
- ✅ **Phase 5**: Enemy System (EnemyData, Enemy, EnemyNameTag, BossHealthBar)

### 통합 시나리오
```
플레이어 시작 (100 HP, 10 Attack, 5 Defense, 100 Gold)
  ↓
상점에서 FireSword 구매 (80 Gold)
  ↓
FireSword 장착 (Attack: 10 → 15)
  ↓
Normal Enemy 생성 및 공격
  ↓
Enemy 처치 후 골드 획득
  ↓
Boss Enemy 생성 및 체력바 표시
  ↓
모든 시스템 정상 동작 확인
```

---

## 🔧 1단계: 준비 (Unity Editor 설정)

### 1-1. 에디터 도구로 UI 생성

1. **StatPanel 생성**
   - 메뉴: `Tools > GASPT > Create StatPanel UI`
   - 확인: `Assets/_Project/Prefabs/UI/StatPanel.prefab` 생성됨
   - Scene에 StatPanel 드래그

2. **ShopUI 생성**
   - 메뉴: `Tools > GASPT > Create ShopUI`
   - 확인: `ShopPanel.prefab`, `ItemSlotPrefab.prefab` 생성됨
   - Scene에 ShopPanel 드래그

3. **Enemy UI 생성**
   - 메뉴: `Tools > GASPT > Create Enemy UIs`
   - 확인: `EnemyNameTag.prefab`, `BossHealthBar.prefab` 생성됨
   - BossHealthBar는 Scene에 자동 생성됨

### 1-2. ScriptableObject 에셋 생성

#### A. 아이템 3개 생성

**경로**: `Assets/_Project/Data/Items/`

1. **FireSword**
   - 우클릭 → Create → GASPT → Items → Item
   - Item Name: "Fire Sword"
   - Slot: Weapon
   - Attack Bonus: 5

2. **LeatherArmor**
   - Item Name: "Leather Armor"
   - Slot: Armor
   - Hp Bonus: 20
   - Defense Bonus: 3

3. **IronRing**
   - Item Name: "Iron Ring"
   - Slot: Accessory
   - Hp Bonus: 10

#### B. 적 3종 생성

**경로**: `Assets/_Project/Data/Enemies/`

1. **NormalGoblin**
   - 우클릭 → Create → GASPT → Enemies → Enemy
   - Enemy Type: Normal
   - Enemy Name: "Normal Goblin"
   - Max Hp: 30
   - Attack: 5
   - Min Gold Drop: 15
   - Max Gold Drop: 25
   - Show Name Tag: false
   - Show Boss Health Bar: false

2. **EliteOrc**
   - Enemy Type: Named
   - Enemy Name: "Elite Orc"
   - Max Hp: 60
   - Attack: 10
   - Min Gold Drop: 40
   - Max Gold Drop: 60
   - Show Name Tag: **true** ✅
   - Show Boss Health Bar: false

3. **FireDragon**
   - Enemy Type: Boss
   - Enemy Name: "Fire Dragon"
   - Max Hp: 150
   - Attack: 15
   - Min Gold Drop: 100
   - Max Gold Drop: 150
   - Show Name Tag: false
   - Show Boss Health Bar: **true** ✅

### 1-3. Scene 설정

#### A. Player GameObject 생성

1. Hierarchy 우클릭 → Create Empty → 이름: `Player`
2. `Add Component` → `PlayerStats`
3. Inspector 설정:
   - Base Hp: 100
   - Base Attack: 10
   - Base Defense: 5

4. **StatPanelUI 연결**
   - StatPanel GameObject 선택
   - StatPanelUI 컴포넌트의 Player Stats 필드에 Player 드래그

#### B. ShopSystem GameObject 생성

1. Hierarchy 우클릭 → Create Empty → 이름: `ShopSystem`
2. `Add Component` → `ShopSystem`
3. Inspector 설정:
   - Shop Items Size: 3
   - Element 0:
     - Item: FireSword
     - Price: 80
   - Element 1:
     - Item: LeatherArmor
     - Price: 120
   - Element 2:
     - Item: IronRing
     - Price: 50

4. **ShopUI 연결**
   - ShopPanel GameObject 선택
   - ShopUI 컴포넌트의 Shop System 필드에 ShopSystem 드래그

#### C. Enemy GameObject 생성 (테스트용)

1. Hierarchy 우클릭 → 3D Object → Cube → 이름: `TestEnemy`
2. `Add Component` → `Enemy`
3. Inspector 설정:
   - Enemy Data: NormalGoblin (일단)

---

## 🧪 2단계: Phase 3 테스트 (Stat System)

### 테스트 2-1: 기본 스탯 표시

**목적**: StatPanelUI가 PlayerStats의 기본 스탯을 올바르게 표시하는지 확인

**실행**:
1. Play 모드 진입
2. StatPanel UI 확인

**예상 결과**:
```
HP: 100
Attack: 10
Defense: 5
```

**검증**:
- ✅ StatPanel에 "HP: 100" 표시
- ✅ StatPanel에 "Attack: 10" 표시
- ✅ StatPanel에 "Defense: 5" 표시

### 테스트 2-2: 아이템 장착 및 스탯 변경

**목적**: 아이템 장착 시 스탯이 올바르게 계산되고 UI가 업데이트되는지 확인

**실행**:
1. Play 모드에서 계속
2. Player GameObject 선택
3. Inspector → PlayerStats 컴포넌트
4. Context Menu (우클릭) → `Equip Test Item` 실행

**예상 결과** (FireSword 장착 시):
```
HP: 100
Attack: 15  (10 + 5)
Defense: 5
```

**검증**:
- ✅ Attack이 10 → 15로 증가
- ✅ StatPanel UI가 실시간으로 업데이트
- ✅ Console에 로그: "[PlayerStats] Fire Sword 장착 완료"

### 테스트 2-3: 성능 검증

**목적**: Dirty Flag 최적화가 작동하는지 확인 (<50ms)

**실행**:
1. PlayerStats Context Menu → `Print Stats Info` 실행
2. Console 로그 확인

**예상 결과**:
```
[PlayerStats] ========== 스탯 정보 ==========
[PlayerStats] 기본 HP: 100
[PlayerStats] 최종 HP: 100
[PlayerStats] 최종 Attack: 15
[PlayerStats] 최종 Defense: 5
```

**검증**:
- ✅ 스탯 재계산이 50ms 이내 완료 (프레임 드랍 없음)
- ✅ isDirty 플래그가 올바르게 작동

---

## 💰 3단계: Phase 4 테스트 (Shop & Economy System)

### 테스트 3-1: 골드 시스템

**목적**: CurrencySystem이 골드를 올바르게 관리하는지 확인

**실행**:
1. Play 모드에서 계속
2. ShopPanel 확인

**예상 결과**:
```
Gold: 100
```

**검증**:
- ✅ GoldText에 "Gold: 100" 표시
- ✅ Console: "[CurrencySystem] 초기화 완료 - 시작 골드: 100"

### 테스트 3-2: 상점 아이템 표시

**목적**: ShopUI가 ShopSystem의 아이템 목록을 올바르게 표시하는지 확인

**실행**:
1. ShopPanel Scroll View 확인

**예상 결과**:
```
Fire Sword       80 Gold    [Purchase]
Leather Armor   120 Gold    [Purchase]
Iron Ring        50 Gold    [Purchase]
```

**검증**:
- ✅ 아이템 3개 모두 표시
- ✅ 각 아이템의 이름과 가격이 정확함
- ✅ Purchase 버튼이 각 아이템에 있음
- ✅ Console: "[ShopUI] 상점 아이템 3개 표시 완료"

### 테스트 3-3: 아이템 구매 (성공)

**목적**: 골드가 충분할 때 구매가 성공하는지 확인

**실행**:
1. Fire Sword의 Purchase 버튼 클릭

**예상 결과**:
```
Gold: 20  (100 - 80)
메시지: "Fire Sword 구매 완료! (-80 골드)" (녹색)
```

**검증**:
- ✅ 골드가 100 → 20으로 감소
- ✅ GoldText 실시간 업데이트
- ✅ 녹색 메시지 표시 (2초 후 자동 숨김)
- ✅ Console 로그:
  ```
  [ShopSystem] 구매 성공: Fire Sword (80 골드)
  [CurrencySystem] 골드 소비: 100 → 20 (-80)
  [InventorySystem] 아이템 추가: Fire Sword (총 1개)
  ```

### 테스트 3-4: 아이템 구매 (실패 - 골드 부족)

**목적**: 골드가 부족할 때 구매가 실패하는지 확인

**실행**:
1. Leather Armor의 Purchase 버튼 클릭 (가격: 120 Gold, 보유: 20 Gold)

**예상 결과**:
```
Gold: 20  (변화 없음)
메시지: "구매 실패: 골드가 부족합니다. (현재: 20, 필요: 120)" (빨간색)
```

**검증**:
- ✅ 골드가 변하지 않음
- ✅ 빨간색 메시지 표시
- ✅ Console 로그: "[ShopSystem] 구매 실패: 골드가 부족합니다."

### 테스트 3-5: 인벤토리 통합

**목적**: 구매한 아이템이 인벤토리에 추가되고 장착 가능한지 확인

**실행**:
1. Hierarchy에서 찾기: Scene에 `InventorySystem` GameObject 자동 생성됨
2. InventorySystem GameObject 선택
3. Context Menu → `Print Inventory` 실행

**예상 결과**:
```
[InventorySystem] ========== 인벤토리 (1개) ==========
[InventorySystem] 1. Fire Sword (Weapon)
```

**검증**:
- ✅ Fire Sword가 인벤토리에 있음
- ✅ InventorySystem.ItemCount = 1

---

## ⚔️ 4단계: Phase 5 테스트 (Enemy System)

### 테스트 4-1: Normal Enemy 기본 동작

**목적**: Normal 적이 올바르게 초기화되고 데미지를 받는지 확인

**실행**:
1. TestEnemy GameObject 선택
2. Inspector → Enemy 컴포넌트
3. Enemy Data를 **NormalGoblin**으로 변경 (이미 설정됨)
4. Play 모드 재시작 (또는 GameObject 활성화)

**예상 결과**:
```
Console:
[Enemy] Normal Goblin 초기화 완료: HP=30/30, Attack=5
```

**검증**:
- ✅ Enemy가 정상 초기화
- ✅ CurrentHp = 30
- ✅ MaxHp = 30

### 테스트 4-2: Enemy TakeDamage

**목적**: 적이 데미지를 받고 HP가 감소하는지 확인

**실행**:
1. Play 모드에서 TestEnemy 선택
2. Context Menu → `Take 10 Damage (Test)` 실행

**예상 결과**:
```
Console:
[Enemy] Normal Goblin: 10 데미지 받음 (30 → 20)
```

**검증**:
- ✅ CurrentHp가 30 → 20으로 감소
- ✅ OnHpChanged 이벤트 발생

### 테스트 4-3: Enemy Die + 골드 드롭

**목적**: 적 사망 시 골드가 드롭되고 CurrencySystem에 추가되는지 확인

**실행**:
1. 현재 골드 확인: 20 Gold
2. TestEnemy Context Menu → `Instant Death (Test)` 실행

**예상 결과**:
```
Console:
[Enemy] Normal Goblin 사망!
[Enemy] Normal Goblin 골드 드롭: 18 골드 (예: 15-25 범위)
[CurrencySystem] 골드 추가: 20 → 38 (+18)
```

**ShopPanel UI**:
```
Gold: 38  (20 + 드롭된 골드)
```

**검증**:
- ✅ 적이 사망 (isDead = true)
- ✅ 골드가 15-25 범위에서 랜덤 드롭
- ✅ CurrencySystem에 골드 추가
- ✅ ShopPanel GoldText 실시간 업데이트
- ✅ 1초 후 GameObject 파괴

### 테스트 4-4: Named Enemy + EnemyNameTag

**목적**: Named 적 위에 이름표가 표시되는지 확인

**실행**:
1. Play 모드 정지
2. TestEnemy의 Enemy Data를 **EliteOrc**로 변경
3. Hierarchy 우클릭 → Create Empty → 이름: `NameTagManager`
4. NameTagManager에 스크립트 추가 (수동 테스트용):

```csharp
using UnityEngine;
using GASPT.Enemy;
using GASPT.UI;

public class NameTagTest : MonoBehaviour
{
    public GameObject enemyNameTagPrefab;
    public Enemy targetEnemy;

    private void Start()
    {
        if (targetEnemy.Data.showNameTag)
        {
            GameObject nameTagObj = Instantiate(enemyNameTagPrefab);
            EnemyNameTag nameTag = nameTagObj.GetComponent<EnemyNameTag>();
            nameTag.Initialize(targetEnemy);
        }
    }
}
```

5. Inspector 설정:
   - Enemy Name Tag Prefab: EnemyNameTag.prefab 드래그
   - Target Enemy: TestEnemy 드래그

6. Play 모드 진입

**예상 결과**:
- Scene에 EnemyNameTag GameObject 생성됨
- TestEnemy 위에 노란색 이름표 표시: "Elite Orc"
- 이름표가 카메라를 향해 회전 (빌보드)

**검증**:
- ✅ EnemyNameTag가 적 위 1.5m에 표시
- ✅ 텍스트: "Elite Orc" (노란색)
- ✅ 카메라를 향해 자동 회전
- ✅ 적 사망 시 이름표도 함께 제거

### 테스트 4-5: Boss Enemy + BossHealthBar

**목적**: Boss 적 생성 시 화면 상단에 체력바가 표시되는지 확인

**실행**:
1. Play 모드 정지
2. TestEnemy의 Enemy Data를 **FireDragon**으로 변경
3. BossHealthBar GameObject가 Scene에 있는지 확인 (Create Enemy UIs로 자동 생성됨)
4. BossHealthBar에 초기화 스크립트 추가 (수동 테스트용):

```csharp
using UnityEngine;
using GASPT.Enemy;
using GASPT.UI;

public class BossHealthBarTest : MonoBehaviour
{
    public BossHealthBar bossHealthBar;
    public Enemy targetBoss;

    private void Start()
    {
        if (targetBoss.Data.showBossHealthBar)
        {
            bossHealthBar.Initialize(targetBoss);
        }
    }
}
```

5. Play 모드 진입

**예상 결과**:
- 화면 상단에 BossHealthBar 표시
- Boss 이름: "Fire Dragon" (빨간색, Bold)
- 체력바: 초록색, 100% 채워짐
- 체력 텍스트: "150 / 150"

**검증**:
- ✅ BossHealthBar가 화면 상단 중앙에 표시
- ✅ 이름과 체력이 정확함

### 테스트 4-6: Boss 체력바 애니메이션

**목적**: Boss가 데미지를 받을 때 체력바가 부드럽게 감소하는지 확인

**실행**:
1. Play 모드에서 TestEnemy (Boss) 선택
2. Context Menu → `Take 10 Damage (Test)` 여러 번 실행

**예상 결과**:
- 체력바가 부드럽게 감소 (Lerp 애니메이션)
- 체력 텍스트 실시간 업데이트: "140 / 150", "130 / 150", ...
- HP < 30% 되면 체력바 색상이 초록색 → 빨간색으로 변경

**검증**:
- ✅ 체력바 Fill Amount가 부드럽게 감소
- ✅ HP < 45 (30%)일 때 빨간색으로 변경
- ✅ Boss 사망 시 체력바 자동 숨김

---

## 🎯 5단계: 통합 테스트 (전체 시나리오)

### 시나리오: 상점 → 장비 → 전투 → 보상

**목적**: 모든 시스템이 함께 동작하는 완전한 게임플레이 루프 검증

**실행 순서**:

#### Step 1: 초기 상태 확인
```
Player: HP=100, Attack=10, Defense=5
Gold: 100
Inventory: 비어있음
```

#### Step 2: 상점에서 아이템 구매
1. ShopPanel에서 Fire Sword 구매 (80 Gold)
2. 결과:
   - Gold: 100 → 20
   - Inventory: Fire Sword 추가

#### Step 3: 아이템 장착
1. PlayerStats Context Menu → `Equip Test Item` (Fire Sword)
2. 결과:
   - Attack: 10 → 15
   - StatPanel 업데이트

#### Step 4: Normal Enemy 처치
1. TestEnemy를 NormalGoblin으로 설정
2. Play 모드 진입
3. Enemy Context Menu → `Instant Death`
4. 결과:
   - Enemy 사망
   - Gold: 20 → 35~45 (15-25 골드 드롭)

#### Step 5: 추가 아이템 구매
1. Iron Ring 구매 (50 Gold)
2. 결과:
   - Gold: 35~45 → -15~-5 (부족하면 실패)
   - 성공 시 Inventory: Fire Sword, Iron Ring

#### Step 6: Boss Enemy 생성 및 처치
1. TestEnemy를 FireDragon으로 변경
2. BossHealthBar 초기화
3. Play 모드 진입
4. 여러 번 데미지 → 최종 사망
5. 결과:
   - Boss 사망
   - Gold 증가: +100~150
   - BossHealthBar 숨김

**최종 검증**:
- ✅ 전체 게임플레이 루프가 끊김 없이 동작
- ✅ 모든 이벤트가 올바르게 발생
- ✅ UI가 모든 변경사항을 실시간 반영
- ✅ Console에 에러 없음

---

## ✅ 통합 테스트 체크리스트

### Phase 3 (Stat System)
- [ ] StatPanel이 기본 스탯 표시
- [ ] 아이템 장착 시 스탯 증가
- [ ] StatPanel UI 실시간 업데이트
- [ ] Dirty Flag 최적화 작동 (<50ms)
- [ ] OnStatChanged 이벤트 발생

### Phase 4 (Shop & Economy)
- [ ] CurrencySystem 초기 골드 100
- [ ] ShopUI에 아이템 3개 표시
- [ ] 골드 충분 시 구매 성공
- [ ] 골드 부족 시 구매 실패
- [ ] 구매한 아이템이 인벤토리에 추가
- [ ] ShopUI GoldText 실시간 업데이트
- [ ] 성공/실패 메시지 표시 (녹색/빨간색)

### Phase 5 (Enemy System)
- [ ] EnemyData로 Enemy 초기화
- [ ] TakeDamage()로 HP 감소
- [ ] Die()로 사망 처리
- [ ] 골드 드롭 (랜덤 범위)
- [ ] CurrencySystem에 골드 추가
- [ ] Named 적에 EnemyNameTag 표시
- [ ] Boss 적에 BossHealthBar 표시
- [ ] 체력바 부드러운 애니메이션
- [ ] HP < 30% 시 체력바 빨간색

### 통합 검증
- [ ] 상점 구매 → 인벤토리 추가 → 스탯 증가 흐름
- [ ] 적 처치 → 골드 획득 → 상점 구매 순환
- [ ] 모든 UI가 실시간 동기화
- [ ] Console에 에러 없음
- [ ] 성능 문제 없음 (프레임 드랍 없음)

---

## 🐛 알려진 이슈 및 해결 방법

### 이슈 1: ShopUI에 아이템이 표시되지 않음

**원인**: ItemSlotPrefab 참조가 누락됨

**해결**:
1. ShopPanel GameObject 선택
2. ShopUI 컴포넌트 확인
3. Item Slot Prefab 필드에 ItemSlotPrefab.prefab 드래그

### 이슈 2: StatPanel이 업데이트되지 않음

**원인**: PlayerStats 참조가 누락됨

**해결**:
1. StatPanel GameObject 선택
2. StatPanelUI 컴포넌트 확인
3. Player Stats 필드에 Player GameObject 드래그

### 이슈 3: BossHealthBar가 표시되지 않음

**원인**: BossHealthBar.Initialize()가 호출되지 않음

**해결**:
1. BossHealthBar는 수동으로 Initialize() 호출 필요
2. 위의 BossHealthBarTest 스크립트 사용
3. 또는 EnemySpawner에서 Boss 생성 시 자동 호출

### 이슈 4: 골드가 증가하지 않음

**원인**: CurrencySystem.Instance가 null

**해결**:
1. Scene에 CurrencySystem GameObject 자동 생성됨 (Play 모드 시)
2. SingletonManager가 올바르게 작동하는지 확인

---

## 📊 성능 검증

### 측정 항목

1. **Stat Calculation Performance**
   - 목표: <50ms
   - 측정: PlayerStats.RecalculateStats() 실행 시간
   - 확인: Profiler 또는 Debug.Log with Stopwatch

2. **UI Update Performance**
   - 목표: 60 FPS 유지
   - 측정: StatPanel, ShopUI, BossHealthBar 업데이트
   - 확인: Stats 창 (FPS 표시)

3. **Memory Allocation**
   - 목표: GC 최소화
   - 측정: 프레임당 GC Alloc
   - 확인: Profiler Memory 섹션

---

## 🎓 다음 단계

### 통합 테스트 완료 후

1. **발견된 버그 수정**
2. **Phase 6 (Combat Integration) 진행**
   - DamageCalculator 구현
   - PlayerStats.GetStat(Attack) 통합
   - PlayerStats.GetStat(Defense) 통합

3. **Phase 7 (Save/Load System) 진행**
   - SaveData 클래스
   - SaveLoadManager 싱글톤
   - 모든 시스템 상태 저장/로드

---

## 📝 테스트 결과 기록

### 테스트 수행일: ___________

### 테스트 결과
- [ ] Phase 3: 통과 / 실패 (메모: _____________)
- [ ] Phase 4: 통과 / 실패 (메모: _____________)
- [ ] Phase 5: 통과 / 실패 (메모: _____________)
- [ ] 통합: 통과 / 실패 (메모: _____________)

### 발견된 이슈
1. _________________________________________
2. _________________________________________
3. _________________________________________

### 개선 사항
1. _________________________________________
2. _________________________________________
3. _________________________________________

---

**작성자**: Claude Code
**버전**: 1.0
**브랜치**: 004-rpg-systems
