# 작업 현황 및 다음 단계

**최종 업데이트**: 2025-11-02
**현재 브랜치**: `006-save-load-system`
**작업 세션**: Phase 6-9 완료, PR 생성 대기

---

## 📊 현재 프로젝트 상태

### 완료된 Phase

#### ✅ Phase 1: Setup & Project Structure
- Core Enums (StatType, EquipmentSlot, EnemyType)
- Assembly Definition 문제 해결 (모두 제거, Assembly-CSharp로 통합)

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
- StatPanelUI.cs (190줄) - 실시간 UI
- StatPanelCreator.cs (242줄) - 에디터 도구
- 아이템 3개 에셋: FireSword, LeatherArmor, IronRing

#### ✅ Phase 4: Shop & Economy System (US2)
**완료 Task**: 7개
- CurrencySystem.cs (165줄) - 골드 관리 싱글톤
- InventorySystem.cs (230줄) - 인벤토리 싱글톤
- ShopSystem.cs (220줄) - 상점 로직
- ShopUI.cs (320줄) - 상점 UI
- ShopItemSlot.cs (71줄) - 독립 파일 (리팩토링)
- ShopUICreator.cs (480줄) - 에디터 도구

#### ✅ Phase 5: Enemy System (US3)
**완료 Task**: 6개
- EnemyData.cs (157줄) - 적 데이터 ScriptableObject
- Enemy.cs (301줄) - 적 MonoBehaviour (namespace: GASPT.Enemies)
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
- SaveSystem.cs (198줄) - JSON 기반 저장/로드 싱글톤
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

---

## 🎯 현재 작업 상태

### Git 상태
```bash
브랜치: 006-save-load-system
원격 푸시: 완료
최종 커밋: dd6e919 (기능: Level & EXP System 구현)
Phase 6 커밋: ba5de83
Phase 7 커밋: 6ab7663
Phase 8 커밋: 99f2876
Phase 9 커밋: dd6e919
```

### PR 생성 대기
- **Phase 3-5 PR**: 이미 머지 완료 (#2)
- **Phase 6 PR**: 이미 머지 완료 (#1)
- **Phase 7-9 PR**: 생성 필요
  - **Base 브랜치**: master
  - **Compare 브랜치**: 006-save-load-system
  - **포함 내용**: Save/Load System + HP Bar UI + Level & EXP System

### 파일 통계 (Phase 7-9)
- 총 변경 파일: 약 25개
- 신규 C# 코드: 10개 파일
- 수정 C# 코드: 6개 파일
- Unity 에셋: 3개 EnemyData에 expReward 추가
- 총 추가 코드: 약 2,500줄

---

## 📂 중요 파일 위치

### 코드
```
Assets/_Project/Scripts/
├── Stats/
│   └── PlayerStats.cs (Combat, Save/Load 통합)
├── Data/
│   ├── Item.cs
│   └── EnemyData.cs (expReward 추가)
├── Economy/
│   └── CurrencySystem.cs (Save/Load 통합)
├── Inventory/
│   └── InventorySystem.cs (Save/Load 통합)
├── Shop/
│   └── ShopSystem.cs
├── Enemies/
│   └── Enemy.cs (EXP 지급, namespace 변경)
├── Combat/
│   └── DamageCalculator.cs
├── Save/
│   ├── SaveData.cs
│   └── SaveSystem.cs
├── Level/
│   └── PlayerLevel.cs
├── UI/
│   ├── StatPanelUI.cs
│   ├── ShopUI.cs
│   ├── ShopItemSlot.cs
│   ├── EnemyNameTag.cs
│   ├── BossHealthBar.cs
│   ├── PlayerHealthBar.cs (NEW)
│   └── PlayerExpBar.cs (NEW)
├── Editor/
│   ├── StatPanelCreator.cs
│   ├── ShopUICreator.cs
│   ├── EnemyUICreator.cs
│   ├── PlayerHealthBarCreator.cs (NEW)
│   └── PlayerExpBarCreator.cs (NEW)
└── Tests/
    ├── CombatTest.cs
    ├── SaveTest.cs
    └── LevelTest.cs (NEW)
```

### 에셋
```
Assets/_Project/Data/
├── Items/
│   ├── FireSword.asset
│   ├── LeatherArmor.asset
│   └── IronRing.asset
└── Enemies/
    ├── Normal Goblin.asset (expReward: 10)
    ├── EliteOrc.asset (expReward: 50)
    └── FireDragon.asset (expReward: 200)
```

### 프리팹
```
Assets/_Project/Prefabs/UI/
├── StatPanel.prefab
├── ShopPanel.prefab
├── ItemSlotPrefab.prefab
├── EnemyNameTag.prefab
├── BossHealthBar.prefab
├── PlayerHealthBar.prefab (NEW)
└── PlayerExpBar.prefab (NEW)
```

---

## 🚀 다음 작업 옵션

### 옵션 1: PR 생성 및 머지 (Phase 7-9)

**수행 단계**:
1. PR 제목: "Save/Load, HP Bar UI, Level & EXP System 구현 (Phase 7-9)"
2. PR 본문 예시:
```markdown
## Phase 7: Save/Load System
- JSON 기반 저장/로드 시스템
- PlayerStats, Currency, Inventory 통합

## Phase 8: Player HP Bar UI
- 실시간 HP 바 표시
- 데미지/회복 시각 효과
- 저체력 색상 변화

## Phase 9: Level & EXP System
- 레벨/경험치 관리
- Enemy 처치 시 EXP 획득
- 레벨업 시 스탯 증가 및 HP 회복
- EXP Bar UI 및 레벨업 애니메이션

## 버그 수정
- PlayerHealthBar 이벤트 구독 타이밍 이슈 해결
- Fill Image 스프라이트 미할당 문제 해결
- namespace 충돌 (GASPT.Enemy → GASPT.Enemies) 해결

## 테스트
- CombatTest: 6개 시나리오 통과
- SaveTest: 6개 시나리오 통과
- LevelTest: 6개 시나리오 통과
```

3. GitHub에서 PR 생성
4. 리뷰 후 머지

**머지 후**:
```bash
git checkout master
git pull origin master
git branch -d 006-save-load-system  # 로컬 브랜치 삭제 (선택)
```

---

### 옵션 2: 통합 테스트 수행

**수행 단계**:
1. Unity Editor 열기
2. Bootstrap 씬 로드
3. 다음 UI 생성 (에디터 도구 사용):
   - `Tools > GASPT > Create Player HealthBar UI`
   - `Tools > GASPT > Create Player ExpBar UI`
   - `Tools > GASPT > Create StatPanel UI`
   - `Tools > GASPT > Create ShopUI`
   - `Tools > GASPT > Create Enemy UIs`

4. Scene에 PlayerLevel 싱글톤 배치 (DontDestroyOnLoad)

5. 통합 테스트 시나리오:
   - **전투 테스트**: Player vs Enemy 데미지 계산
   - **레벨업 테스트**: Enemy 처치 → EXP 획득 → 레벨업
   - **HP 회복 테스트**: 레벨업 시 HP 완전 회복
   - **저장/로드 테스트**: 게임 진행 → 저장 → 로드 → 상태 확인
   - **UI 테스트**: HP 바, EXP 바 실시간 업데이트

6. Context Menu 테스트:
   - **LevelTest**: 6개 시나리오 실행
   - **CombatTest**: 6개 시나리오 실행
   - **SaveTest**: 6개 시나리오 실행

**테스트 소요 시간**: 약 1-1.5시간

---

### 옵션 3: Phase 10 시작 (Combat UI & Damage Numbers)

**새 브랜치 생성** (master 머지 후 권장):
```bash
git checkout master
git pull origin master
git checkout -b 007-combat-ui
```

**Phase 10 예상 Task 목록**:
- [ ] Damage Number UI (World Space)
- [ ] Floating Text Animation
- [ ] Combat Log UI (Screen Space)
- [ ] Attack Button UI
- [ ] Target Selection UI
- [ ] Combat State Machine

---

## 🧪 빠른 테스트 방법 (Unity에서)

### 에디터 도구로 UI 생성
```
Tools > GASPT > Create StatPanel UI
Tools > GASPT > Create ShopUI
Tools > GASPT > Create Enemy UIs
Tools > GASPT > Create Player HealthBar UI  (NEW)
Tools > GASPT > Create Player ExpBar UI     (NEW)
```
→ 모든 UI 프리팹이 자동 생성됨

### Context Menu로 빠른 테스트

**PlayerStats**:
- 우클릭 → `Equip Test Item` (아이템 장착)
- 우클릭 → `Print Stats Info` (스탯 확인)
- 우클릭 → `Take 10 Damage (Test)` (데미지 받기)
- 우클릭 → `Heal 20 HP (Test)` (회복)
- 우클릭 → `Revive (Test)` (부활)

**Enemy**:
- 우클릭 → `Print Enemy Info` (적 정보)
- 우클릭 → `Take 10 Damage (Test)` (데미지 받기)
- 우클릭 → `Instant Death (Test)` (즉사 - EXP 지급됨)

**PlayerLevel** (NEW):
- 우클릭 → `Print Level Info` (레벨 정보)
- 우클릭 → `Add 50 EXP (Test)` (EXP 추가)
- 우클릭 → `Level Up (Test)` (강제 레벨업)

**SaveSystem** (NEW):
- 우클릭 → `Save Game (Test)` (게임 저장)
- 우클릭 → `Load Game (Test)` (게임 로드)
- 우클릭 → `Delete Save (Test)` (저장 파일 삭제)

**ShopSystem**:
- 우클릭 → `Print Shop Items` (상점 아이템 목록)

**InventorySystem**:
- 우클릭 → `Print Inventory` (인벤토리 확인)

**CurrencySystem**:
- 우클릭 → `Print Gold Info` (골드 확인)
- 우클릭 → `Add 100 Gold (Test)` (골드 추가)
- 우클릭 → `Spend 50 Gold (Test)` (골드 소비)

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
# 현재 브랜치가 006-save-load-system인지 확인
git branch --show-current
```

### 3. 최신 코드 확인
```bash
# 원격 저장소와 동기화
git fetch origin
git status
```

### 4. PR 상태 확인
- GitHub에서 Phase 7-9 PR이 생성되었는지 확인
- 머지되었는지 확인
- 리뷰 코멘트가 있는지 확인

### 5. 다음 작업 선택
- PR 생성 → 옵션 1
- 통합 테스트 → 옵션 2
- Phase 10 시작 → 옵션 3

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
git checkout -b 007-combat-ui

# 변경사항 커밋
git add .
git commit -m "커밋 메시지"
git push origin <브랜치명>
```

### Unity 명령어
```
# 에디터 도구
Tools > GASPT > Create StatPanel UI
Tools > GASPT > Create ShopUI
Tools > GASPT > Create Enemy UIs
Tools > GASPT > Create Player HealthBar UI
Tools > GASPT > Create Player ExpBar UI

# ScriptableObject 생성
Create > GASPT > Items > Item
Create > GASPT > Enemies > Enemy
```

---

## 📚 참고 문서

### 프로젝트 문서
1. **specs/004-rpg-systems/spec.md** - 기능 명세
2. **specs/004-rpg-systems/tasks.md** - Task 목록
3. **specs/004-rpg-systems/plan.md** - 구현 계획
4. **docs/portfolio/unity-assembly-definition-troubleshooting.md** - Assembly 문제 해결

---

## ⚠️ 알아두면 좋은 정보

### 네임스페이스 변경
- **변경 전**: `GASPT.Enemy` (CS0118 에러 발생)
- **변경 후**: `GASPT.Enemies` (복수형)
- **영향 받는 파일**: Enemy.cs, CombatTest.cs, LevelTest.cs, EnemyNameTag.cs, BossHealthBar.cs

### 주요 버그 수정 (Phase 8)
1. **Revive 시 HP Text 미업데이트**: OnHealed 이벤트 추가
2. **HP 바 슬라이더 미표시**: Fill Image 스프라이트 할당
3. **이벤트 구독 타이밍**: OnEnable → Start로 변경

### UI 렌더링 순서 (Phase 9)
- Unity UI는 Hierarchy 순서로 렌더링
- 나중에 배치된 자식이 위에 그려짐
- PlayerExpBar의 LevelText를 마지막에 생성하여 해결

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

### 커밋 메시지
- 한글로 작성
- 형식: "타입: 간단한 설명"
- 예: "기능: Level & EXP System 구현 (Phase 9)"

---

## 🎯 추천 작업 순서

**다음에 작업 재개 시 추천 순서**:

1. **이 파일(WORK_STATUS.md) 먼저 읽기** ✅
2. **Git 상태 확인** (`git status`, `git log`)
3. **Phase 7-9 PR 생성** (아직 안 했으면)
4. **통합 테스트 수행** (선택사항)
5. **PR 머지**
6. **Phase 10 시작** 또는 **다음 기능 기획**

---

## 💡 빠른 재개를 위한 팁

### Claude Code와 다시 대화 시작할 때
1. 이 파일(`WORK_STATUS.md`) 내용 공유
2. 현재 브랜치 알려주기: `006-save-load-system`
3. 하고 싶은 작업 명시:
   - "Phase 7-9 PR 생성하고 싶어"
   - "통합 테스트 진행하고 싶어"
   - "Phase 10 시작하고 싶어"

### 통합 테스트 수행할 때
1. Unity Editor 열기
2. Bootstrap 씬 로드
3. UI 생성 (에디터 도구 사용)
4. Context Menu로 각 시스템 테스트
5. 실제 게임플레이 시나리오 테스트

### Phase 10 시작할 때
1. master 브랜치로 이동 및 최신화
2. 새 브랜치 생성: `007-combat-ui`
3. "Phase 10 구현 시작해줘" 요청
4. TodoWrite로 Task 목록 생성
5. 순서대로 구현

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
| **합계** | **9개 Phase** | **34개** | **~6,758줄** | **✅ 완료** |

---

**작성일**: 2025-11-02
**다음 예정 작업**: Phase 7-9 PR 생성 또는 통합 테스트
**브랜치**: 006-save-load-system
**상태**: Phase 9까지 커밋 완료, 푸시 완료, PR 생성 대기

🚀 **수고하셨습니다! Phase 6-9 완료!**
