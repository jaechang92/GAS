# 작업 현황 및 다음 단계

**최종 업데이트**: 2025-01-15
**현재 브랜치**: `008-buff-debuff-system`
**작업 세션**: Phase 10-11 완료 + GameResourceManager 구현

---

## 📊 현재 프로젝트 상태

### 완료된 Phase

#### ✅ Phase 1: Setup & Project Structure
- Core Enums (StatType, EquipmentSlot, EnemyType, StatusEffectType)
- Assembly Definition 문제 해결 (모두 제거, Assembly-CSharp로 통합)
- SingletonManager<T> 패턴 확립

#### ✅ Phase 2: GAS Core Implementation
- IAbility, IAbilitySystem 인터페이스
- Ability 베이스 클래스
- AbilityData ScriptableObject
- AbilitySystem 싱글톤
- Awaitable 비동기 패턴 (Coroutine 미사용)

#### ✅ Phase 3: Stat System (US1)
**완료 Task**: 8개
- PlayerStats.cs (295줄) - Dirty Flag 최적화
- Item.cs (85줄) - ScriptableObject
- StatPanelUI.cs (270줄) - 실시간 UI + 버프/디버프 표시
- StatPanelCreator.cs (242줄) - 에디터 도구
- 아이템 3개 에셋: FireSword, LeatherArmor, IronRing

#### ✅ Phase 4: Shop & Economy System (US2)
**완료 Task**: 7개
- CurrencySystem.cs (165줄) - 골드 관리 싱글톤
- InventorySystem.cs (230줄) - 인벤토리 싱글톤
- ShopSystem.cs (220줄) - 상점 로직
- ShopUI.cs (320줄) - 상점 UI
- ShopItemSlot.cs (71줄) - 독립 파일
- ShopUICreator.cs (480줄) - 에디터 도구

#### ✅ Phase 5: Enemy System (US3)
**완료 Task**: 6개
- EnemyData.cs (157줄) - 적 데이터 ScriptableObject
- Enemy.cs (493줄) - 적 MonoBehaviour + StatusEffect 통합
- EnemyNameTag.cs (122줄) - World Space UI
- BossHealthBar.cs (201줄) - Screen Space UI
- EnemyUICreator.cs (400줄) - 에디터 도구
- 적 3종 에셋: NormalGoblin, EliteOrc, FireDragon

#### ✅ Phase 6: Combat Integration
**완료 Task**: 5개
- DamageCalculator.cs (84줄) - 데미지 계산 유틸리티
  - CalculateDamageDealt() - 공격력 → 데미지 (100% ~ 120% 랜덤)
  - CalculateDamageReceived() - 방어력 적용 (방어력 × 0.5 감소)
- PlayerStats.cs 수정 - Combat 메서드 추가
  - TakeDamage(), Heal(), DealDamageTo(), Revive()
- Enemy.cs 수정 - DealDamageTo() 추가
- CombatTest.cs (280줄) - 6가지 전투 시나리오 테스트

#### ✅ Phase 7: Save/Load System
**완료 Task**: 5개
- SaveData.cs (118줄) - 직렬화 데이터 구조
  - GameSaveData, PlayerStatsData, CurrencyData, InventoryData
- SaveSystem.cs (SingletonManager 사용, 198줄) - JSON 기반 저장/로드 싱글톤
  - Save(), Load(), HasSaveFile(), DeleteSave()
- PlayerStats.cs 수정 - GetSaveData(), LoadFromSaveData()
- CurrencySystem.cs 수정 - Save/Load 통합
- InventorySystem.cs 수정 - Save/Load 통합
- SaveTest.cs (220줄) - 6가지 저장/로드 시나리오 테스트

#### ✅ Phase 8: Player HP Bar UI
**완료 Task**: 3개
- PlayerHealthBar.cs (390줄) - HP Bar UI 스크립트
  - HP 슬라이더, 텍스트, 색상 효과
  - 데미지/회복 플래시 애니메이션
  - 저체력/위험 체력 색상 변화
- PlayerHealthBarCreator.cs (241줄) - UI 자동 생성 에디터 도구
- **버그 수정**:
  - Revive() 시 OnHealed 이벤트 미발생 → 수정
  - Fill Image 스프라이트 미할당 → 수정
  - 이벤트 구독 타이밍 이슈 → 수정

#### ✅ Phase 9: Level & EXP System
**완료 Task**: 6개
- PlayerLevel.cs (279줄) - 레벨/EXP 관리 싱글톤
  - EXP 공식: RequiredEXP = Level × 100
  - 레벨업 보상: HP +10, Attack +2, Defense +1
  - 레벨업 시 HP 완전 회복 (Revive 호출)
  - Reflection으로 PlayerStats 기본 스탯 수정
- PlayerExpBar.cs (390줄) - EXP Bar UI 스크립트
  - 레벨 텍스트 (Lv.X)
  - EXP 슬라이더 및 텍스트 (X/Y)
  - 레벨업 애니메이션 (텍스트 스케일 + 색상 효과)
- PlayerExpBarCreator.cs (241줄) - UI 자동 생성 에디터 도구
  - Hierarchy 렌더링 순서 최적화 (LevelText 마지막 배치)
- LevelTest.cs (301줄) - 6가지 테스트 시나리오
- EnemyData.cs 수정 - expReward 필드 추가
- Enemy.cs 수정 - GiveExp() 메서드 추가
- **네임스페이스 수정**: GASPT.Enemy → GASPT.Enemies (CS0118 에러 해결)

#### ✅ Phase 10: Combat UI & Damage Numbers
**완료 Task**: 5개
- DamageNumber.cs (180줄) - 데미지 텍스트 애니메이션
  - 일반 데미지 (빨간색), 크리티컬 (노란색), 회복 (초록색), EXP (파란색)
  - 위로 떠오르는 애니메이션 + 페이드 아웃
  - 자동 풀링 복귀
- DamageNumberPool.cs (350줄) - 오브젝트 풀링 시스템
  - 공용 Canvas 사용 (성능 최적화)
  - 카메라 빌보드 효과
  - 자동 리소스 로딩 (GameResourceManager 사용)
- DamageNumberCreator.cs (150줄) - 프리팹 자동 생성 에디터 도구
- PlayerStats.cs 수정 - DamageNumber 표시 통합
- Enemy.cs 수정 - DamageNumber 표시 통합
- SingletonPreloader.cs 수정 - DamageNumberPool 사전 로딩

#### ✅ Phase 11: Buff/Debuff System (상태이상 시스템)
**완료 Task**: 10개

**핵심 시스템** (5개 파일):
- StatusEffectType.cs (46줄) - 16가지 효과 타입 Enum
  - 버프: AttackUp, DefenseUp, SpeedUp, CriticalRateUp
  - 디버프: AttackDown, DefenseDown, SpeedDown, Stun, Slow
  - DoT: Poison, Burn, Bleed
  - 특수: Invincible, Regeneration, Shield, Root
- StatusEffect.cs (259줄) - 효과 인스턴스 클래스
  - 효과 생명주기 관리 (Apply → Update → Remove)
  - 틱 기반 DoT 시스템
  - 스택 시스템 (중첩 효과)
  - 이벤트 시스템 (OnApplied, OnRemoved, OnTick)
- StatusEffectData.cs (112줄) - ScriptableObject 데이터
  - 디자이너 친화적 효과 정의
  - CreateInstance() 팩토리 메서드
- StatusEffectManager.cs (300줄) - 싱글톤 관리자
  - Dictionary<GameObject, List<StatusEffect>> 구조
  - Update 루프에서 모든 활성 효과 업데이트
  - 효과 적용/제거/조회 API
  - 이벤트 브로드캐스트
- StatusEffectTest.cs (520줄) - 18개 테스트 케이스
  - Context Menu 기반 테스트
  - 버프/디버프/DoT/회복/중첩/제거 테스트

**기존 시스템 통합** (3개 파일):
- PlayerStats.cs 수정
  - Attack/Defense 프로퍼티에 버프/디버프 적용
  - BaseAttack/BaseDefense 프로퍼티 추가
  - DoT 틱 처리 (Poison, Burn, Bleed) - 방어력 무시
  - Regeneration 틱 처리 (회복)
  - OnEnable에서 StatusEffectManager 이벤트 구독
- Enemy.cs 수정
  - Attack 프로퍼티에 버프/디버프 적용
  - DoT/Regeneration 틱 처리
  - OnEnable에서 StatusEffectManager 이벤트 구독
- SingletonPreloader.cs 수정
  - StatusEffectManager 사전 로딩 추가 (총 7개 싱글톤)

**UI 시각화** (1개 파일):
- StatPanelUI.cs 수정
  - 버프/디버프 색상 표시 (초록/빨강)
  - "기본값 → 현재값" 형식 표시
  - StatusEffectManager 이벤트 구독

**버그 수정** (3개 커밋):
- StatusEffectManager 중첩 시 이벤트 발생
- PlayerStats OnStatChanged 이벤트 트리거
- 이벤트 구독 타이밍 문제 해결 (Awake → OnEnable)

#### ✅ 추가 구현: GameResourceManager (리소스 관리 시스템)
**완료 Task**: 6개

**핵심 시스템** (2개 파일):
- GameResourceManager.cs (251줄) - 싱글톤 리소스 관리자
  - Resources.Load() 래핑 및 캐싱 시스템
  - 타입별 로딩 메서드:
    - LoadPrefab() - GameObject
    - LoadScriptableObject<T>() - ScriptableObject
    - LoadAudioClip() - AudioClip
    - LoadSprite() - Sprite
    - LoadTextAsset() - TextAsset
  - 인스턴스화 메서드:
    - Instantiate(path, parent)
    - Instantiate(path, position, rotation, parent)
  - 캐싱 관리:
    - UnloadResource(path)
    - UnloadAllResources()
    - PrintCacheInfo() (디버그용)
- ResourcePaths.cs (195줄) - 리소스 경로 상수 관리
  - 카테고리별 구분 (Prefabs, Data, Audio, Sprites)
  - IDE 자동완성 지원
  - 타입 안전성 보장

**리팩토링** (2개 파일):
- DamageNumberPool.cs 수정
  - damageNumberPrefab SerializeField 제거
  - GameResourceManager를 통한 자동 로딩
  - LoadDamageNumberPrefab() 메서드 추가
- SingletonPreloader.cs 수정
  - GameResourceManager 최우선 순위 사전 로딩
  - 총 7개 싱글톤 관리

**문서화**:
- RESOURCES_GUIDE.md (220줄) - Resources 폴더 구조 가이드
  - 폴더 구조 정의
  - 사용 방법 및 예제
  - 네이밍 규칙
  - 주의사항 및 베스트 프랙티스

---

## 🎯 현재 작업 상태

### Git 상태
```bash
브랜치: 008-buff-debuff-system
원격 푸시: 완료
최종 커밋: 786baeb (기능: GameResourceManager 리소스 관리 시스템 구현)
Phase 10 커밋: e90f14b
Phase 11 커밋: 456d199 + 4개 버그 수정 커밋
GameResourceManager 커밋: 786baeb
```

### 주요 커밋 목록
```
786baeb 기능: GameResourceManager 리소스 관리 시스템 구현
fdf66d5 수정: StatusEffectManager 이벤트 구독 타이밍 문제 해결
6217aa8 수정: StatusEffectManager 중첩 시 OnEffectApplied 이벤트 발생 추가
39feee9 수정: PlayerStats에서 버프/디버프 적용 시 OnStatChanged 이벤트 발생
51fddad 개선: StatPanelUI에 버프/디버프 시각적 표시 기능 추가
456d199 기능: Buff/Debuff 상태이상 시스템 구현 (Phase 11)
be3af16 리팩토링: SaveSystem을 SingletonManager 사용하도록 변경
e90f14b 기능: Combat UI & Damage Numbers 구현 (Phase 10)
```

### 싱글톤 시스템 현황 (7개)
1. **GameResourceManager** - 리소스 자동 로딩 및 캐싱
2. **DamageNumberPool** - 데미지 텍스트 풀링
3. **CurrencySystem** - 골드 관리
4. **InventorySystem** - 인벤토리 관리
5. **PlayerLevel** - 레벨/EXP 관리
6. **SaveSystem** - 저장/로드
7. **StatusEffectManager** - 상태이상 효과 관리

### PR 생성 대기
- **Phase 10-11 + GameResourceManager PR**: 생성 필요
  - **Base 브랜치**: master
  - **Compare 브랜치**: 008-buff-debuff-system
  - **포함 내용**:
    - Combat UI & Damage Numbers
    - Buff/Debuff System
    - GameResourceManager
    - 버그 수정 4건

---

## 📂 중요 파일 위치

### 코드
```
Assets/_Project/Scripts/
├── Core/
│   ├── SingletonManager.cs
│   └── SingletonPreloader.cs (7개 싱글톤 관리)
├── Core/Enums/
│   ├── StatType.cs
│   ├── EquipmentSlot.cs
│   ├── EnemyType.cs
│   └── StatusEffectType.cs (NEW)
├── Stats/
│   └── PlayerStats.cs (Combat, Save/Load, StatusEffect 통합)
├── Data/
│   ├── Item.cs
│   ├── EnemyData.cs
│   └── StatusEffectData.cs (NEW)
├── Economy/
│   └── CurrencySystem.cs
├── Inventory/
│   └── InventorySystem.cs
├── Shop/
│   └── ShopSystem.cs
├── Enemies/
│   └── Enemy.cs (StatusEffect 통합)
├── Combat/
│   └── DamageCalculator.cs
├── Save/
│   ├── SaveData.cs
│   └── SaveSystem.cs (SingletonManager 사용)
├── Level/
│   └── PlayerLevel.cs
├── StatusEffects/ (NEW)
│   ├── StatusEffect.cs
│   ├── StatusEffectManager.cs
│   └── StatusEffectTest.cs
├── Resources/ (NEW)
│   ├── GameResourceManager.cs
│   └── ResourcePaths.cs
├── UI/
│   ├── StatPanelUI.cs (버프/디버프 표시)
│   ├── ShopUI.cs
│   ├── ShopItemSlot.cs
│   ├── EnemyNameTag.cs
│   ├── BossHealthBar.cs
│   ├── PlayerHealthBar.cs
│   ├── PlayerExpBar.cs
│   ├── DamageNumber.cs (NEW)
│   └── DamageNumberPool.cs (NEW, 자동 로딩)
├── Editor/
│   ├── StatPanelCreator.cs
│   ├── ShopUICreator.cs
│   ├── EnemyUICreator.cs
│   ├── PlayerHealthBarCreator.cs
│   ├── PlayerExpBarCreator.cs
│   └── DamageNumberCreator.cs (NEW)
└── Tests/
    ├── CombatTest.cs
    ├── SaveTest.cs
    ├── LevelTest.cs
    └── StatusEffectTest.cs (NEW)
```

### 문서
```
GASPT/
├── WORK_STATUS.md (현재 파일)
├── RESOURCES_GUIDE.md (NEW)
├── specs/
└── docs/
```

---

## 📊 Phase별 완료 통계

| Phase | 설명 | 파일 수 | 코드 라인 | 상태 |
|-------|------|---------|-----------|------|
| Phase 1 | Setup & Project Structure | 3 | ~100 | ✅ 완료 |
| Phase 2 | GAS Core | 5 | ~500 | ✅ 완료 |
| Phase 3 | Stat System | 4 | ~812 | ✅ 완료 |
| Phase 4 | Shop & Economy | 5 | ~1,486 | ✅ 완료 |
| Phase 5 | Enemy System | 5 | ~1,118 | ✅ 완료 |
| Phase 6 | Combat Integration | 2 | ~364 | ✅ 완료 |
| Phase 7 | Save/Load System | 4 | ~536 | ✅ 완료 |
| Phase 8 | Player HP Bar UI | 2 | ~631 | ✅ 완료 |
| Phase 9 | Level & EXP System | 4 | ~1,211 | ✅ 완료 |
| Phase 10 | Combat UI & Damage Numbers | 3 | ~680 | ✅ 완료 |
| Phase 11 | Buff/Debuff System | 9 | ~1,691 | ✅ 완료 |
| 추가 | GameResourceManager | 3 | ~666 | ✅ 완료 |
| **합계** | **11개 Phase + 추가** | **49개** | **~9,795줄** | **✅ 완료** |

---

## 🚀 다음 작업 옵션

### 옵션 1: PR 생성 및 머지 (Phase 10-11)

**수행 단계**:
1. PR 제목: "Combat UI, Buff/Debuff System, GameResourceManager 구현 (Phase 10-11)"
2. PR 본문:
```markdown
## Phase 10: Combat UI & Damage Numbers
- DamageNumber 애니메이션 (일반/크리티컬/회복/EXP)
- DamageNumberPool 오브젝트 풀링 시스템
- 공용 Canvas 성능 최적화
- 카메라 빌보드 효과

## Phase 11: Buff/Debuff System
- 16가지 상태 이상 효과 타입
- StatusEffect 생명주기 관리
- DoT/HoT 시스템 (Poison, Burn, Bleed, Regeneration)
- 효과 중첩 시스템
- PlayerStats/Enemy 통합
- StatPanelUI 시각화 (버프: 초록, 디버프: 빨강)

## GameResourceManager
- Resources.Load() 래핑 및 캐싱
- 타입별 로딩 메서드
- 자동 리소스 로딩
- DamageNumberPool 리팩토링

## 버그 수정
- StatusEffectManager 이벤트 구독 타이밍 (Awake → OnEnable)
- 중첩 시 이벤트 미발생 문제
- PlayerStats OnStatChanged 트리거

## 테스트
- StatusEffectTest: 18개 시나리오
```

3. GitHub에서 PR 생성
4. 리뷰 후 머지

**머지 후**:
```bash
git checkout master
git pull origin master
git branch -d 008-buff-debuff-system  # 로컬 브랜치 삭제
```

---

### 옵션 2: Phase 12 시작 (Skill System)

**새 브랜치 생성**:
```bash
git checkout -b 009-skill-system
```

**Phase 12 예상 Task 목록**:
- [ ] SkillData ScriptableObject
- [ ] SkillSystem 싱글톤
- [ ] Skill UI (버튼, 쿨다운)
- [ ] 기본 스킬 4-5개 구현
- [ ] 마나 시스템 (선택)

---

### 옵션 3: BuffIconUI 구현 (Phase 11 확장)

**Phase 11 완성도 향상**:
- [ ] BuffIconUI 프리팹
- [ ] BuffIconPool 오브젝트 풀링
- [ ] 활성 버프/디버프 아이콘 표시
- [ ] 지속시간 표시 (원형 타이머)
- [ ] 스택 수 표시

---

### 옵션 4: Item Drop & Loot System

**Phase 12 새 기능**:
- [ ] LootTable ScriptableObject
- [ ] DropSystem 싱글톤
- [ ] 아이템 드롭 로직 (확률 기반)
- [ ] 드롭 아이템 UI
- [ ] Enemy에 LootTable 연동

---

## 🧪 빠른 테스트 방법 (Unity에서)

### 에디터 도구로 UI 생성
```
Tools > GASPT > Create StatPanel UI
Tools > GASPT > Create ShopUI
Tools > GASPT > Create Enemy UIs
Tools > GASPT > Create Player HealthBar UI
Tools > GASPT > Create Player ExpBar UI
Tools > GASPT > Create DamageNumber Prefab
```

### Context Menu로 빠른 테스트

**StatusEffectTest** (NEW):
- 우클릭 → `Test/Player/Apply AttackUp` (버프 적용)
- 우클릭 → `Test/Player/Apply AttackDown` (디버프 적용)
- 우클릭 → `Test/Player/Apply Poison` (DoT 적용)
- 우클릭 → `Test/Player/Apply Regeneration` (회복)
- 우클릭 → `Test/Player/Remove All Effects` (효과 제거)
- 우클릭 → `Test/Player/Print Active Effects` (활성 효과 확인)

**DamageNumberPool** (NEW):
- DamageNumber는 자동으로 표시됨 (데미지/회복/EXP 시)

**GameResourceManager** (NEW):
- 우클릭 → `Print Cache Info` (캐시 상태 확인)

**PlayerStats**:
- 우클릭 → `Take 10 Damage (Test)` → DamageNumber 표시됨
- 우클릭 → `Heal 20 HP (Test)` → 회복 텍스트 표시됨

**Enemy**:
- 우클릭 → `Instant Death (Test)` → EXP Number 표시됨

**PlayerLevel**:
- 우클릭 → `Add 50 EXP (Test)` → EXP Number 표시됨

---

## 📝 작업 재개 시 체크리스트

### 1. Git 상태 확인
```bash
cd D:/JaeChang/UintyDev/GASPT/GASPT
git status
git log --oneline -10
git branch
```

### 2. 현재 브랜치 확인
```bash
# 현재 브랜치가 008-buff-debuff-system인지 확인
git branch --show-current
```

### 3. Resources 폴더 설정 (Unity)
- `Assets/Resources/Prefabs/UI/` 폴더 생성
- DamageNumber.prefab을 해당 위치로 이동
- GameResourceManager가 자동으로 로드함

### 4. 다음 작업 선택
- PR 생성 및 머지 → 옵션 1
- Phase 12 (Skill System) → 옵션 2
- BuffIconUI 구현 → 옵션 3
- Item Drop System → 옵션 4

---

## 🔍 주요 명령어 요약

### Git 명령어
```bash
# 현재 상태 확인
git status
git log --oneline -10
git branch

# PR 생성 후 머지됐으면
git checkout master
git pull origin master

# 새 Phase 시작
git checkout -b 009-skill-system

# 변경사항 커밋
git add .
git commit -m "커밋 메시지"
git push origin <브랜치명>
```

### Unity 에디터 도구
```
Tools > GASPT > Create StatPanel UI
Tools > GASPT > Create ShopUI
Tools > GASPT > Create Enemy UIs
Tools > GASPT > Create Player HealthBar UI
Tools > GASPT > Create Player ExpBar UI
Tools > GASPT > Create DamageNumber Prefab
```

### ScriptableObject 생성
```
Create > GASPT > Items > Item
Create > GASPT > Enemies > Enemy
Create > GASPT > StatusEffects > StatusEffect
```

---

## ⚠️ 알아두면 좋은 정보

### GameResourceManager 사용
```csharp
// BEFORE (수동 할당)
[SerializeField] private GameObject prefab;

// AFTER (자동 로딩)
GameObject prefab = GameResourceManager.Instance.LoadPrefab(ResourcePaths.Prefabs.UI.DamageNumber);
```

### StatusEffect 사용 예시
```csharp
// 버프 적용
StatusEffectData attackUp = GameResourceManager.Instance.LoadScriptableObject<StatusEffectData>(
    ResourcePaths.Data.StatusEffects.AttackUp
);
StatusEffectManager.Instance.ApplyEffect(player.gameObject, attackUp);

// 효과 확인
bool hasBuff = StatusEffectManager.Instance.HasEffect(player.gameObject, StatusEffectType.AttackUp);

// 효과 제거
StatusEffectManager.Instance.RemoveEffect(player.gameObject, StatusEffectType.AttackUp);
```

### 이벤트 구독 패턴 (중요!)
```csharp
// Awake가 아닌 OnEnable에서 구독
private void OnEnable()
{
    StatusEffectManager manager = StatusEffectManager.Instance;  // HasInstance 사용 금지
    if (manager != null)
    {
        manager.OnEffectApplied += OnEffectApplied;
    }
}

private void OnDisable()
{
    if (StatusEffectManager.HasInstance)  // OnDisable에서만 HasInstance 사용
    {
        StatusEffectManager.Instance.OnEffectApplied -= OnEffectApplied;
    }
}
```

### Unity 버전
- Unity 6.0 이상
- CS0618 경고 주의 (deprecated API)
  - velocity → linearVelocity
  - FindObjectOfType → FindAnyObjectByType

### 코딩 규칙
- 카멜케이스 사용 (변수명에 '_' 붙이지 않음)
- 한글 주석 허용
- 500줄 넘으면 파일 분할
- Coroutine 사용 금지 (Awaitable 사용)

---

## 🎯 추천 작업 순서

**다음에 작업 재개 시 추천 순서**:

1. **이 파일(WORK_STATUS.md) 먼저 읽기** ✅
2. **Git 상태 확인** (`git status`, `git log`)
3. **Phase 10-11 PR 생성** (옵션 1)
4. **PR 머지**
5. **Phase 12 기획 및 시작** (Skill System 또는 다른 옵션)

---

## 💡 빠른 재개를 위한 팁

### Claude Code와 다시 대화 시작할 때
1. 이 파일(`WORK_STATUS.md`) 내용 공유
2. 현재 브랜치 알려주기: `008-buff-debuff-system`
3. 하고 싶은 작업 명시:
   - "Phase 10-11 PR 생성하고 싶어"
   - "Phase 12 (Skill System) 시작하고 싶어"
   - "BuffIconUI 구현하고 싶어"

---

## 📚 참고 문서

### 프로젝트 문서
1. **WORK_STATUS.md** (현재 파일) - 전체 작업 현황
2. **RESOURCES_GUIDE.md** - Resources 폴더 구조 및 사용법
3. **specs/004-rpg-systems/** - 기능 명세 및 Task 목록

---

**작성일**: 2025-01-15
**다음 예정 작업**: Phase 10-11 PR 생성 또는 Phase 12 시작
**브랜치**: 008-buff-debuff-system
**상태**: Phase 11 + GameResourceManager 완료, 푸시 완료, PR 생성 대기

🚀 **수고하셨습니다! Phase 10-11 + GameResourceManager 완료!**
