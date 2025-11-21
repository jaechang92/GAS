# 🎮 GASPT 프로젝트 마스터 로드맵

**프로젝트명**: GASPT (Generic Ability System + FSM Platform Game)
**장르**: 로그라이크 플랫포머 (Skul: The Hero Slayer 스타일)
**현재 버전**: Phase C-4 완료
**전체 진행률**: 약 70%
**최종 업데이트**: 2025-11-19

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [전체 Phase 구조](#전체-phase-구조)
3. [Phase별 상세 로드맵](#phase별-상세-로드맵)
4. [현재 진행 상황](#현재-진행-상황)
5. [남은 작업 우선순위](#남은-작업-우선순위)
6. [마일스톤 및 예상 일정](#마일스톤-및-예상-일정)
7. [서버 오류 대응 방안](#서버-오류-대응-방안)

---

## 🎯 프로젝트 개요

### 게임 컨셉
- **장르**: 2D 로그라이크 플랫포머
- **핵심 메커니즘**:
  - 스컬(Form) 교체를 통한 능력 변화
  - 절차적 던전 생성
  - 아이템 드롭 및 장착 시스템
  - 로그라이크 런 기반 플레이
- **타겟**: 중급 게이머, 짧은 세션 플레이 (30분~1시간)
- **참고 게임**: Skul: The Hero Slayer

### 기술 스택
- **엔진**: Unity 2023.x (Unity 6.0+)
- **언어**: C# with async/await 패턴 (Awaitable 사용, Coroutine 금지)
- **아키텍처**:
  - GAS (Gameplay Ability System)
  - FSM (Finite State Machine)
  - Panel 기반 UI 시스템
  - ScriptableObject 기반 데이터 관리
- **디자인 패턴**:
  - Singleton (Manager 클래스)
  - Object Pooling (적, 투사체, 이펙트)
  - Component 기반 설계
  - SOLID 원칙 준수

### 개발 원칙
1. **완성 우선**: 완벽한 시스템보다 플레이 가능한 게임 먼저
2. **단계적 개발**: 작은 단위로 나누어 개발하고 지속적으로 테스트
3. **생산성 우선**: 시스템 설계에 충분한 시간 투자
4. **코드 품질**: OOP, SOLID 준수, 한글 주석 허용

---

## 🗺️ 전체 Phase 구조

### Phase 분류 체계
프로젝트는 크게 **Phase A → Phase B → Phase C → Phase D → Phase E**로 구성됩니다.

```
Phase A: Form 시스템 기초 (완료)
    └─ Form 교체, MageForm, Room 시스템, Enemy AI

Phase B: 플레이어블 프로토타입 (완료)
    ├─ Phase B-1: 에디터 도구 및 프리팹 자동 생성
    ├─ Phase B-2: 적 스폰 및 전투 시스템
    └─ Phase B-3: UI 시스템 통합

Phase C: 던전 진행 및 아이템 시스템 (완료)
    ├─ Phase C-1: 적 타입별 동적 스폰 시스템
    ├─ Phase C-2: 보스 전투 시스템
    ├─ Phase C-3: 던전 진행 시스템 (Portal, DungeonCompleteUI)
    └─ Phase C-4: 아이템 드롭 및 인벤토리 시스템

Phase D: UI 시스템 재설계 및 베이스 개선 (진행 중)
    ├─ BaseUI 패턴 도입
    ├─ InventoryUI, PortalUI, DungeonCompleteUI 리팩토링
    └─ Panel 자동 찾기 기능

Phase E: 로그라이크 콘텐츠 확장 (예정)
    ├─ Phase E-1: 절차적 레벨 생성 시스템
    ├─ Phase E-2: 스컬 교체 시스템 확장
    ├─ Phase E-3: 메타 진행 시스템
    └─ Phase E-4: 밸런싱 및 콘텐츠 추가

Phase F: 최적화 및 배포 (예정)
    ├─ 성능 최적화
    ├─ 버그 수정 및 안정화
    └─ 빌드 및 배포
```

---

## 📊 Phase별 상세 로드맵

### ✅ Phase A: Form 시스템 기초 (100% 완료)

**목표**: 스컬(Form) 교체 메커니즘의 기초 구축

#### 완료된 작업
- ✅ **Phase A-1**: MageForm 시스템 구현
  - BaseForm, IFormController 인터페이스
  - MageForm 구현 (마법 미사일, 파이어볼 등)
  - FormInputHandler

- ✅ **Phase A-2**: Enemy AI + Combat 통합
  - BasicMeleeEnemy, RangedEnemy, FlyingEnemy
  - Enemy AI FSM
  - Combat System 통합

- ✅ **Phase A-3**: Room System (절차적 던전)
  - RoomData ScriptableObject
  - EnemySpawnPoint 시스템
  - Room 기반 전투

- ✅ **Phase A-4**: Item-Skill System
  - SkillItem, SkillData
  - SkillSystem, SkillItemManager
  - Skill UI (SkillSlotUI, SkillUIPanel)

**주요 산출물**:
- Scripts: Form/, Gameplay/Enemy/, Gameplay/Level/Room/, Skills/
- 커밋: 131f4e9 ~ 86dbf45

---

### ✅ Phase B: 플레이어블 프로토타입 (100% 완료)

**목표**: 실제 플레이 가능한 게임 데모 구축

#### Phase B-1: 에디터 도구 및 프리팹 자동 생성 (완료)
- ✅ PrefabCreator 에디터 도구
- ✅ GameplaySceneCreator 씬 자동 생성
- ✅ 플레이어, 적, 플랫폼 프리팹 생성
- ✅ Sprite 에셋 관리 (SerializeField)

**주요 커밋**: e5557a1 (Phase B-1 에디터 도구)

#### Phase B-2: 적 스폰 및 전투 시스템 (완료)
- ✅ 적 타입별 구현 (BasicMelee, Ranged, Flying)
- ✅ 적 스폰 시스템
- ✅ 전투 테스트 씬
- ✅ Enemy 체력바, 네임태그 UI

**주요 커밋**: 447d184 (Phase B-2 완료), ea44f20 (문서)

#### Phase B-3: UI 시스템 통합 (완료)
- ✅ HUD 시스템 (PlayerHealthBar, PlayerManaBar, PlayerExpBar)
- ✅ Damage Number 시스템
- ✅ Buff/Debuff Icon 시스템
- ✅ Item Pickup UI
- ✅ Enemy UI (BossHealthBar, EnemyNameTag)
- ✅ Ground Layer 설정

**주요 커밋**: 475291f (Phase B-3 완료), 20045f6 (최종 상태)

---

### ✅ Phase C: 던전 진행 및 아이템 시스템 (100% 완료)

**목표**: 던전 클리어 루프 및 아이템 드롭 시스템 구축

#### Phase C-1: 적 타입별 동적 스폰 시스템 (완료)
- ✅ EnemyType Enum (Normal, Elite, Boss, Flying)
- ✅ 적 타입별 동적 스폰
- ✅ 난이도별 적 조정
- ✅ 테스트 시스템

**주요 커밋**: a8b2433 (Phase C-1 완료)

#### Phase C-2: 보스 전투 시스템 (완료)
- ✅ 보스 AI 구현
- ✅ 보스 체력바 UI
- ✅ 보스 전용 스킬 패턴
- ✅ 에디터 자동화 도구

**주요 커밋**: d2681cc (Phase C-2 완료)

#### Phase C-3: 던전 진행 시스템 (완료)
- ✅ Portal 시스템 (다음 방 이동)
- ✅ PortalUI (상호작용 안내)
- ✅ DungeonCompleteUI (클리어 보상 표시)
- ✅ PhaseC3SetupCreator 에디터 도구
- ✅ Time.timeScale 제어 (일시정지)

**주요 커밋**: b4610b4 (Phase C-3 완료)

#### Phase C-4: 아이템 드롭 및 인벤토리 시스템 (완료)
- ✅ Item ScriptableObject (무기, 방어구, 악세서리)
- ✅ LootSystem (드롭 확률, LootTable)
- ✅ DroppedItem (바닥 아이템)
- ✅ InventorySystem (아이템 추가/제거/장착)
- ✅ InventoryUI (아이템 목록, 장비 슬롯)
- ✅ EquipmentSlotUI (장비 슬롯 UI)
- ✅ PlayerStats 장비 스탯 적용
- ✅ LootTableCreator, InventoryUICreator 에디터 도구

**주요 커밋**: bb5a148 (Phase C-4 완료), 179fce9 ~ f8b40f5 (리팩토링)

---

### 🔄 Phase D: UI 시스템 재설계 및 베이스 개선 (90% 완료)

**목표**: 코드 중복 제거, 유지보수성 향상, BaseUI 패턴 도입

#### 완료된 작업
- ✅ **BaseUI 추상 클래스 생성**
  - Panel GameObject 자동 관리
  - Show(), Hide(), Toggle() 공통 메서드
  - IsVisible 프로퍼티
  - hideOnAwake 설정

- ✅ **기존 UI 리팩토링**
  - InventoryUI, PortalUI, DungeonCompleteUI → BaseUI 상속
  - 중복 코드 제거 (~70줄 감소)
  - 일관된 인터페이스 제공

- ✅ **에디터 도구 개선**
  - InventoryUICreator, PhaseC3SetupCreator 수정
  - Canvas 구조 개선 ("=== UI CANVAS ===" 하위 생성)
  - SetActive 문제 해결 (Parent-Child 구조)

- ✅ **Panel 자동 찾기 기능**
  - InitializePanel() 메서드: "Panel" 자식 GameObject 자동 탐색
  - Initialize() 가상 메서드: 자식 클래스 추가 초기화 지원

#### 남은 작업
- ⏳ Unity 에디터에서 테스트
- ⏳ 기존 Prefab 재생성 확인

**관련 파일**:
- Assets/_Project/Scripts/UI/BaseUI.cs
- Assets/_Project/Scripts/UI/InventoryUI.cs
- Assets/_Project/Scripts/UI/PortalUI.cs
- Assets/_Project/Scripts/UI/DungeonCompleteUI.cs

---

### 🎯 Phase E: 로그라이크 콘텐츠 확장 (0% → 100%, 예정)

**목표**: 로그라이크 핵심 메커니즘 완성

#### Phase E-1: 절차적 레벨 생성 시스템 (예정)
**예상 기간**: 2-3주

- [ ] **Room Generator**
  - Tilemap 기반 방 생성
  - 랜덤 플랫폼 배치
  - 장애물 배치 (가시, 함정)

- [ ] **Dungeon Generator**
  - Graph 기반 방 연결 (BSP, Cellular Automata)
  - 시작 방 → 보스 방 경로 보장
  - 보물, 상점, 휴식 방 배치

- [ ] **Room Types**
  - NormalRoom, EliteRoom, TreasureRoom
  - ShopRoom, BossRoom, RestRoom

- [ ] **Minimap System**
  - 방문한 방 표시
  - 현재 위치 표시
  - 방 타입별 아이콘

#### Phase E-2: 스컬 교체 시스템 확장 (예정)
**예상 기간**: 3-4주

- [ ] **SkullData ScriptableObject**
  - 기본 스탯
  - 고유 스킬 (GAS Ability)
  - 애니메이션, 스프라이트 세트

- [ ] **Skull Types**
  - Basic, Warrior, Mage, Assassin, Tank

- [ ] **Skull Manager**
  - 현재 스컬 관리
  - 소유 스컬 목록
  - 교체 로직 (Q키)
  - 쿨다운 관리

- [ ] **Transform System**
  - 스프라이트/애니메이션 교체
  - 스탯 적용
  - GAS Ability 교체
  - VFX 효과

- [ ] **Awakening System**
  - 각성 조건 (골드, 아이템)
  - 각성 스컬 (강화 버전)
  - 추가 스킬, 스탯 증가

#### Phase E-3: 메타 진행 시스템 (예정)
**예상 기간**: 1-2주

- [ ] **Meta Currency**
  - 뼈 (Bone): 플레이 중 획득
  - 영혼 (Soul): 보스 처치 시 획득

- [ ] **Upgrade Tree**
  - 체력 증가
  - 공격력 증가
  - 새로운 스컬 해금
  - 시작 아이템 해금

- [ ] **Persistence System**
  - SaveSystem 활용
  - 업그레이드 상태 저장
  - 해금 스컬 저장

- [ ] **Achievement System**
  - 조건 체크
  - 보상 지급
  - UI 표시

#### Phase E-4: 밸런싱 및 콘텐츠 추가 (예정)
**예상 기간**: 2-3주

- [ ] 아이템 추가 (100개 이상)
- [ ] 스컬 추가 (20개 이상)
- [ ] 적 타입 추가 (30개 이상)
- [ ] 난이도 곡선 조절
- [ ] 플레이 테스트 및 피드백 반영

---

### ⚡ Phase F: 최적화 및 배포 (0% → 100%, 예정)

**목표**: 상업적 출시 가능한 품질 확보

**예상 기간**: 4-6주

#### 최적화 (2-3주)
- [ ] Object Pooling 확대 적용
- [ ] Level Streaming (방 단위 로드/언로드)
- [ ] 메모리 최적화 (텍스처, 오디오 압축)
- [ ] 성능 프로파일링 (CPU, GPU, 메모리)

#### 밸런싱 (2주)
- [ ] 난이도 곡선 조절
- [ ] 아이템/스컬 밸런스
- [ ] 보상 밸런스
- [ ] 플레이 테스트

#### 배포 준비 (1주)
- [ ] 빌드 최적화
- [ ] 플랫폼별 테스트 (Windows, Mac, Linux)
- [ ] 세이브/로드 시스템 안정화
- [ ] 크래시 리포팅

---

## 📈 현재 진행 상황

### 전체 진행률: **약 70%**

| Phase | 시스템 | 진행률 | 상태 |
|-------|-------|--------|------|
| **Phase A** | Form 시스템 기초 | 100% | ✅ 완료 |
| **Phase B** | 플레이어블 프로토타입 | 100% | ✅ 완료 |
| **Phase C** | 던전 진행 및 아이템 시스템 | 100% | ✅ 완료 |
| **Phase D** | UI 시스템 재설계 | 90% | 🔄 진행 중 |
| **Phase E** | 로그라이크 콘텐츠 확장 | 0% | ⏳ 예정 |
| **Phase F** | 최적화 및 배포 | 0% | ⏳ 예정 |

### 최근 완료 작업 (2025-11-19)
- ✅ BaseUI 추상 클래스 생성
- ✅ InventoryUI, PortalUI, DungeonCompleteUI 리팩토링
- ✅ Panel 자동 찾기 기능 추가 (InitializePanel)
- ✅ EquipmentSlot 리팩토링 (템플릿 프리팹 패턴)

### 현재 작업 중
- 🔄 Unity 에디터 테스트 대기
- 🔄 UI Prefab 재생성 확인

---

## 🎯 남은 작업 우선순위

### 🔥 최우선 (즉시 시작)
1. **Phase D 완료**: Unity 에디터 테스트 및 Prefab 검증
2. **Phase E-1 착수**: 절차적 레벨 생성 시스템 설계
3. **Phase E-2 기획**: 스컬 교체 시스템 상세 설계

### ⭐ 높은 우선순위 (1-2개월 내)
1. **Phase E-1 완료**: Room Generator, Dungeon Generator
2. **Phase E-2 시작**: SkullData, Skull Manager 구현
3. **인벤토리 확장**: 드래그 앤 드롭, 아이템 정렬

### 💎 중간 우선순위 (2-3개월 내)
1. **Phase E-3**: 메타 진행 시스템
2. **Shop System**: 상점 UI 및 구매 시스템
3. **VFX/사운드**: 기본 이펙트 및 사운드 추가

### 📦 낮은 우선순위 (3개월 이후)
1. **Phase F**: 최적화
2. **콘텐츠 추가**: 추가 스컬, 아이템, 적
3. **애니메이션 폴리싱**

---

## 📅 마일스톤 및 예상 일정

### Milestone 1: Phase D 완료 (현재)
**예상 기간**: 1-2일
**목표**:
- UI 시스템 리팩토링 완료
- BaseUI 패턴 안정화
- Unity 에디터 테스트 통과

**완료 조건**:
- ✅ BaseUI 클래스 생성 완료
- ⏳ 모든 UI가 BaseUI 상속
- ⏳ Panel 자동 찾기 동작 확인
- ⏳ 에디터 도구로 생성한 Prefab 정상 작동

---

### Milestone 2: Phase E-1 완료 (Playable Prototype)
**예상 기간**: 2-3주
**목표**:
- 절차적 던전 생성 완료
- 기본 Room Types 구현
- Minimap 시스템

**완료 조건**:
- [ ] 랜덤 던전 생성 동작
- [ ] 5개 Room Types 구현
- [ ] 던전 클리어 가능
- [ ] Minimap 표시

---

### Milestone 3: Phase E-2 완료 (Core Loop Complete)
**예상 기간**: 4-5주
**목표**:
- 스컬 교체 시스템 완성
- 5개 스컬 구현
- 각성 시스템

**완료 조건**:
- [ ] 스컬 교체 동작 (Q키)
- [ ] 5개 스컬 플레이 가능
- [ ] 각성 시스템 동작
- [ ] 스컬별 고유 스킬

---

### Milestone 4: Phase E-3~E-4 완료 (Content Complete)
**예상 기간**: 3-4주
**목표**:
- 메타 진행 완료
- 밸런싱 완료
- 충분한 콘텐츠 (20 스컬, 100 아이템)

**완료 조건**:
- [ ] 메타 업그레이드 동작
- [ ] 도전 과제 시스템
- [ ] 20개 스컬, 100개 아이템
- [ ] 밸런싱 완료

---

### Milestone 5: Phase F 완료 (Release Candidate)
**예상 기간**: 4-6주
**목표**:
- 최적화 완료
- 버그 수정
- 배포 준비

**완료 조건**:
- [ ] 60fps 안정 유지
- [ ] 주요 버그 0개
- [ ] 플랫폼별 빌드 테스트
- [ ] 배포 준비 완료

---

### 전체 일정 요약

```
현재 (2025-11-19):  Phase D 완료 직전
+1-2일:              Milestone 1 완료 (Phase D)
+2-3주:              Milestone 2 완료 (Phase E-1)
+4-5주:              Milestone 3 완료 (Phase E-2)
+3-4주:              Milestone 4 완료 (Phase E-3~E-4)
+4-6주:              Milestone 5 완료 (Phase F)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
총 예상 기간:        약 4-5개월
```

---

## 🛡️ 서버 오류 대응 방안

### 1. 문서 기반 복구 시스템

#### 핵심 문서 3종
이 로드맵과 함께 다음 문서를 참조하여 수동 작업 진행:

1. **PROJECT_MASTER_ROADMAP.md** (본 문서)
   - 전체 Phase 구조
   - 남은 작업 우선순위
   - 마일스톤 체크리스트

2. **WORK_HISTORY.md**
   - Phase별 완료 내역
   - 생성된 파일 목록
   - 주요 커밋 정보

3. **IMPLEMENTATION_GUIDE.md**
   - 시스템별 구현 방법
   - 코드 스니펫
   - 단계별 체크리스트

#### 사용 방법
```
서버 오류 발생 시:
1. PROJECT_MASTER_ROADMAP.md에서 현재 Phase 확인
2. WORK_HISTORY.md에서 완료된 작업 확인
3. IMPLEMENTATION_GUIDE.md에서 다음 작업 구현 방법 확인
4. 단계별 체크리스트를 따라 수동 구현
```

---

### 2. Git 커밋 기반 복구

#### 주요 Phase별 커밋 태그

**Phase A 완료**:
- `86dbf45`: Phase A-1 완료 (MageForm)
- `02d36c0`: Phase A-2 완료 (Enemy AI + Combat)
- `439cf08`: Phase A-3 완료 (Room System)
- `c9171e3`: Phase A-4 완료 (Item-Skill System)

**Phase B 완료**:
- `e5557a1`: Phase B-1 완료 (에디터 도구)
- `447d184`: Phase B-2 완료 (적 스폰 및 전투)
- `475291f`: Phase B-3 완료 (UI 시스템 통합)

**Phase C 완료**:
- `a8b2433`: Phase C-1 완료 (적 타입별 스폰)
- `d2681cc`: Phase C-2 완료 (보스 전투)
- `b4610b4`: Phase C-3 완료 (던전 진행)
- `bb5a148`: Phase C-4 완료 (아이템/인벤토리)

**Phase D 진행 중**:
- `f8b40f5`: EquipmentSlot 리팩토링
- 현재: BaseUI 패턴 도입 (커밋 대기)

#### 복구 방법
```bash
# 특정 Phase로 돌아가기
git checkout <커밋 해시>

# 차이점 확인
git diff <커밋1> <커밋2>

# 파일 단위 복구
git checkout <커밋 해시> -- <파일 경로>
```

---

### 3. 파일 목록 기반 복구

#### 핵심 시스템 파일 위치

**Core 시스템**:
```
Assets/_Project/Scripts/
├── Core/
│   ├── Enums/             # EnemyType, EquipmentSlot, StatType 등
│   ├── Utilities/         # GameEvents, AwaitableHelper
│   └── ObjectPool/        # ObjectPool, PoolManager
├── Data/                  # Item, StatusEffectData
├── Inventory/             # InventorySystem
└── Stats/                 # PlayerStats
```

**UI 시스템**:
```
Assets/_Project/Scripts/UI/
├── BaseUI.cs              # 새로 추가된 베이스 클래스
├── InventoryUI.cs         # BaseUI 상속
├── PortalUI.cs            # BaseUI 상속
├── DungeonCompleteUI.cs   # BaseUI 상속
└── EquipmentSlotUI.cs     # 장비 슬롯
```

**에디터 도구**:
```
Assets/_Project/Scripts/Editor/
├── InventoryUICreator.cs  # 인벤토리 UI 생성
├── PhaseC3SetupCreator.cs # Portal, DungeonComplete UI 생성
└── LootTableCreator.cs    # 드롭 테이블 생성
```

**상세 파일 목록**: WORK_HISTORY.md 참조

---

### 4. 단계별 복구 체크리스트

서버 오류 발생 시 다음 순서로 작업:

#### Step 1: 현재 상태 파악 (5분)
- [ ] 마지막 커밋 확인: `git log -1`
- [ ] 현재 Phase 확인: PROJECT_MASTER_ROADMAP.md
- [ ] 변경된 파일 확인: `git status`

#### Step 2: 문서 기반 작업 재개 (10분)
- [ ] WORK_HISTORY.md에서 완료 작업 확인
- [ ] IMPLEMENTATION_GUIDE.md에서 다음 작업 확인
- [ ] 단계별 체크리스트 준비

#### Step 3: 수동 구현 (작업 시간 소요)
- [ ] IMPLEMENTATION_GUIDE.md의 코드 스니펫 참고
- [ ] 단계별 체크리스트 진행
- [ ] 각 단계 완료 시 커밋

#### Step 4: 테스트 및 검증 (10-20분)
- [ ] Unity에서 플레이 모드 실행
- [ ] 기능 동작 확인
- [ ] 버그 발견 시 즉시 수정

---

### 5. 긴급 연락망 및 백업

#### 프로젝트 백업 전략
1. **Git Repository**: GitHub (Primary)
2. **로컬 백업**: 외장 하드 (주 1회)
3. **클라우드 백업**: Google Drive (주 1회)

#### 문서 위치
- **로컬**: `D:\JaeChang\UintyDev\GASPT\GASPT\docs\development\`
- **Git**: `docs/development/`
- **README.md**: 문서 인덱스 및 빠른 참조

---

## 📝 참고 문서

### 개발 문서
- [WORK_HISTORY.md](WORK_HISTORY.md) - 완료된 작업 상세 기록
- [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md) - 수동 구현 가이드
- [Roadmap.md](Roadmap.md) - 기존 Phase 1~5 로드맵
- [ROGUELIKE_PLATFORMER_ROADMAP.md](ROGUELIKE_PLATFORMER_ROADMAP.md) - 로그라이크 세부 계획
- [CURRENT_WORK.md](CURRENT_WORK.md) - 최근 작업 내용

### 설계 문서
- [UI_SYSTEM_DESIGN.md](../guides/UI_SYSTEM_DESIGN.md) - UI 시스템 설계
- [PREFAB_CREATION_GUIDE.md](../guides/PREFAB_CREATION_GUIDE.md) - Prefab 제작 가이드
- [CodingGuidelines.md](CodingGuidelines.md) - 코딩 규칙

### 참고 자료
- [README.md](../../README.md) - 프로젝트 개요
- [QuickStart.md](../getting-started/QuickStart.md) - 빠른 시작

---

## 🎯 성공 지표

### 기술적 지표
- **테스트 커버리지**: 80% 이상 (현재: 60%)
- **빌드 성공률**: 95% 이상 (현재: 100%)
- **버그 발생률**: 주요 버그 0개 (현재: 0개)
- **성능 목표**: 60fps 안정 유지 (현재: 달성)

### 게임 품질 지표
- **플레이어 세션**: 평균 30분 이상 (목표)
- **재플레이율**: 70% 이상 (목표)
- **스컬 밸런스**: 모든 스컬 사용률 15% 이상 (목표)
- **학습 곡선**: 5분 내 기본 조작 습득 (현재: 달성)

### 개발 생산성 지표
- **개발 속도**: 주당 기능 완성도 5% 이상 (현재: 유지 중)
- **코드 품질**: 리뷰 통과율 90% 이상 (현재: 95%)
- **일정 준수**: 마일스톤 지연 1주 이내 (현재: 준수 중)

---

## 🔮 향후 확장 계획

### Phase G: 라이브 서비스 (선택사항)
- 온라인 기능 (리더보드, 도전 과제)
- 정기 업데이트 (새로운 스컬/레벨)
- 커뮤니티 (모드 지원, 레벨 에디터)

### Phase H: 차기작 준비
- 엔진 개선 (차기작용 프레임워크)
- 새로운 장르 적용
- IP 확장 콘텐츠

---

**최종 업데이트**: 2025-11-19
**다음 업데이트**: Phase D 완료 후
**작성자**: GASPT 개발팀

---

*이 로드맵은 서버 오류 시에도 프로젝트를 지속할 수 있도록 설계되었습니다.*
*WORK_HISTORY.md와 IMPLEMENTATION_GUIDE.md를 함께 참조하세요.*
