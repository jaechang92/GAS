# 몬스터 시스템 구현 태스크

**기능 번호**: 023
**생성일**: 2026-01-07
**총 태스크**: 45개
**상태**: ✅ 코드 구현 완료 (70%)

---

## 기존 구현 현황

### 이미 구현된 항목 ✅
| 항목 | 파일 | 상태 |
|------|------|------|
| EnemyType enum | `Core/Enums/EnemyType.cs` | ✅ Normal, Named, Boss |
| EnemyClass enum | `Core/Enums/EnemyClass.cs` | ✅ BasicMelee, Ranged, Flying, Elite, Boss |
| ElementType enum | `Core/Enums/ElementType.cs` | ✅ None, Fire, Ice, Thunder, Dark, Light, Poison, Earth |
| BossGrade enum | `Core/Enums/BossGrade.cs` | ✅ MiniBoss, MidBoss, FinalBoss |
| EnemyData SO | `Data/EnemyData.cs` | ✅ 기본 구조 (ElementType 필드 없음) |
| Enemy 기본 클래스 | `Gameplay/Enemy/Base/Enemy.cs` | ✅ HP, 데미지, 드롭, 풀링 |
| BasicMeleeEnemy | `Gameplay/Enemy/BasicMeleeEnemy.cs` | ✅ FSM 기반 |
| RangedEnemy | `Gameplay/Enemy/RangedEnemy.cs` | ✅ 원거리 AI |
| FlyingEnemy | `Gameplay/Enemy/FlyingEnemy.cs` | ✅ 비행 AI |
| EliteEnemy | `Gameplay/Enemy/EliteEnemy.cs` | ✅ 특수 패턴 |
| BossEnemy | `Gameplay/Enemy/BossEnemy.cs` | ✅ 페이즈 시스템 |

### 구현 필요 항목 ❌
- EnemyData에 ElementType 필드 추가
- 속성 상성 데미지 계산 시스템
- MonsterSpawnTable (스테이지별 스폰 테이블)
- 몬스터 데이터 에셋 37종 (Normal 20, Named 11, Boss 6)
- 에디터 도구: MonsterAssetGenerator
- 스테이지별 스폰 테이블 에셋 5종

---

## Phase 1: 기반 시스템 (Setup) ✅ 완료

> 몬스터 시스템의 데이터 구조를 확장하고 속성 시스템을 구현합니다.

### 완료 조건
- [x] EnemyData에 속성 관련 필드 추가
- [x] 속성 상성 계산 시스템 작동
- [x] 스테이지 스케일링 공식 적용

### 태스크 목록

- [x] T001 EnemyData에 ElementType 필드 추가 in `Assets/_Project/Scripts/Data/EnemyData.cs`
- [x] T002 EnemyData에 BossGrade 필드 추가 (Boss 전용) in `Assets/_Project/Scripts/Data/EnemyData.cs`
- [x] T003 EnemyData에 stageAppearance (출현 스테이지) 필드 추가 in `Assets/_Project/Scripts/Data/EnemyData.cs`
- [x] T004 [P] ElementDamageCalculator 정적 클래스 생성 in `Assets/_Project/Scripts/Combat/ElementDamageCalculator.cs`
- [x] T005 [P] StageScalingCalculator 정적 클래스 생성 in `Assets/_Project/Scripts/Combat/StageScalingCalculator.cs`
- [ ] T006 Enemy.TakeDamage()에 속성 데미지 계산 연동 in `Assets/_Project/Scripts/Gameplay/Enemy/Base/Enemy.cs`

---

## Phase 2: 스폰 테이블 시스템 (US1 - Spawn Tables) ✅ 완료

> 스테이지별 몬스터 스폰 테이블을 구현합니다.

### 사용자 스토리
*"레벨 디자이너로서 스테이지별로 다른 몬스터 조합을 설정하고 싶다"*

### 완료 조건
- [x] 스폰 테이블 ScriptableObject 정의
- [x] 가중치 기반 랜덤 선택 구현
- [x] 방 타입별 몬스터 배치 로직

### 태스크 목록

- [x] T007 [US1] MonsterSpawnEntry 데이터 클래스 생성 in `Assets/_Project/Scripts/Level/Spawn/MonsterSpawnEntry.cs`
- [x] T008 [US1] MonsterSpawnTable ScriptableObject 생성 in `Assets/_Project/Scripts/Level/Spawn/MonsterSpawnTable.cs`
- [x] T009 [US1] MonsterSpawnManager 클래스 생성 in `Assets/_Project/Scripts/Level/Spawn/MonsterSpawnManager.cs`
- [ ] T010 [US1] RoomController에 MonsterSpawnManager 연동 in `Assets/_Project/Scripts/Level/RoomController.cs`

---

## Phase 3: Normal 몬스터 에셋 (US2 - Normal Monsters) ⏳ 진행예정

> 일반 몬스터 20종의 데이터 에셋을 생성합니다.

### 사용자 스토리
*"플레이어로서 스테이지마다 다양한 일반 몬스터와 전투하고 싶다"*

### 완료 조건
- [ ] BasicMelee 몬스터 5종 에셋 생성
- [ ] Ranged 몬스터 5종 에셋 생성
- [ ] Flying 몬스터 5종 에셋 생성

### 태스크 목록

#### BasicMelee (5종)
- [ ] T011 [P] [US2] M001_ForestGolem 에셋 생성 (Earth, Stage 1) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T012 [P] [US2] M002_SkeletonWarrior 에셋 생성 (None, Stage 1-2) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T013 [P] [US2] M003_FlameImp 에셋 생성 (Fire, Stage 2) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T014 [P] [US2] M004_FrozenCorpse 에셋 생성 (Ice, Stage 3) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T015 [P] [US2] M005_VenomousSpider 에셋 생성 (Poison, Stage 4) in `Assets/Resources/Data/Enemies/Normal/`

#### Ranged (5종)
- [ ] T016 [P] [US2] M010_SkeletonArcher 에셋 생성 (None, Stage 1-2) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T017 [P] [US2] M011_FlameMage 에셋 생성 (Fire, Stage 2) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T018 [P] [US2] M012_IceMage 에셋 생성 (Ice, Stage 3) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T019 [P] [US2] M013_PoisonDartTribesman 에셋 생성 (Poison, Stage 4) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T020 [P] [US2] M014_DarkCaster 에셋 생성 (Dark, Stage 5) in `Assets/Resources/Data/Enemies/Normal/`

#### Flying (5종)
- [ ] T021 [P] [US2] M020_ForestBat 에셋 생성 (None, Stage 1) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T022 [P] [US2] M021_FireElemental 에셋 생성 (Fire, Stage 2) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T023 [P] [US2] M022_IceFairy 에셋 생성 (Ice, Stage 3) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T024 [P] [US2] M023_MiasmaSpirit 에셋 생성 (Poison, Stage 4) in `Assets/Resources/Data/Enemies/Normal/`
- [ ] T025 [P] [US2] M024_DarkRaven 에셋 생성 (Dark, Stage 5) in `Assets/Resources/Data/Enemies/Normal/`

---

## Phase 4: Named 몬스터 에셋 (US3 - Named Monsters) ⏳ 진행예정

> 네임드 몬스터 11종의 데이터 에셋을 생성합니다.

### 사용자 스토리
*"플레이어로서 강력한 네임드 몬스터를 처치하고 특별한 보상을 얻고 싶다"*

### 완료 조건
- [ ] Elite 클래스 Named 8종 에셋 생성
- [ ] 강화형 Named 3종 에셋 생성
- [ ] 특수 능력 데이터 정의

### 태스크 목록

#### Elite Named (8종)
- [ ] T026 [P] [US3] N001_AncientForestGuardian 에셋 생성 (Earth, Stage 1) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T027 [P] [US3] N002_BoneKnightCaptain 에셋 생성 (None, Stage 1-2) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T028 [P] [US3] N003_FlameWitch 에셋 생성 (Fire, Stage 2) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T029 [P] [US3] N004_FrostKnight 에셋 생성 (Ice, Stage 3) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T030 [P] [US3] N005_ThunderSpiritKing 에셋 생성 (Thunder, Stage 3) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T031 [P] [US3] N006_PoisonQueen 에셋 생성 (Poison, Stage 4) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T032 [P] [US3] N007_ShadowAssassin 에셋 생성 (Dark, Stage 4-5) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T033 [P] [US3] N008_LightInquisitor 에셋 생성 (Light, Stage 5) in `Assets/Resources/Data/Enemies/Named/`

#### 강화형 Named (3종)
- [ ] T034 [P] [US3] N010_MagmaGolem 에셋 생성 (Fire+Earth, Stage 2) in `Assets/Resources/Data/Enemies/Named/`
- [ ] T035 [P] [US3] N011_GlacierGiant 에셋 생성 (Ice, Stage 3) in `Assets/Resources/Data/Enemies/Named/`

---

## Phase 5: 보스 몬스터 에셋 (US4 - Boss Monsters) ⏳ 진행예정

> 보스 몬스터 6종의 데이터 에셋을 생성합니다.

### 사용자 스토리
*"플레이어로서 스테이지 보스를 처치하고 다음 스테이지로 진행하고 싶다"*

### 완료 조건
- [ ] MiniBoss 3종 에셋 생성
- [ ] MidBoss 2종 에셋 생성
- [ ] FinalBoss 1종 에셋 생성
- [ ] 보스 페이즈 데이터 정의

### 태스크 목록

#### MiniBoss (3종)
- [ ] T036 [P] [US4] B001_FlameGolem 에셋 생성 (Fire, Stage 1) in `Assets/Resources/Data/Enemies/Boss/`
- [ ] T037 [P] [US4] B002_FrostSpirit 에셋 생성 (Ice, Stage 2) in `Assets/Resources/Data/Enemies/Boss/`
- [ ] T038 [P] [US4] B003_SwampQueenSpider 에셋 생성 (Poison, Stage 4) in `Assets/Resources/Data/Enemies/Boss/`

#### MidBoss (2종)
- [ ] T039 [P] [US4] B004_ThunderDragon 에셋 생성 (Thunder, Stage 3) in `Assets/Resources/Data/Enemies/Boss/`
- [ ] T040 [P] [US4] B005_ShadowLord 에셋 생성 (Dark, Stage 5) in `Assets/Resources/Data/Enemies/Boss/`

#### FinalBoss (1종)
- [ ] T041 [US4] B006_DarkArchmage 에셋 생성 (Dark, Stage 5 Final) in `Assets/Resources/Data/Enemies/Boss/`

---

## Phase 6: 스테이지 스폰 테이블 에셋 (US5 - Stage Tables) ⏳ 진행예정

> 5개 스테이지별 스폰 테이블 에셋을 생성합니다.

### 사용자 스토리
*"레벨 디자이너로서 각 스테이지에 테마에 맞는 몬스터를 배치하고 싶다"*

### 완료 조건
- [ ] 5개 스테이지 스폰 테이블 에셋 생성
- [ ] 가중치 설정 완료
- [ ] 레벨 생성 시스템 연동

### 태스크 목록

- [ ] T042 [P] [US5] Stage1_ForestRuins 스폰 테이블 에셋 생성 in `Assets/Resources/Data/SpawnTables/`
- [ ] T043 [P] [US5] Stage2_FlameCavern 스폰 테이블 에셋 생성 in `Assets/Resources/Data/SpawnTables/`
- [ ] T044 [P] [US5] Stage3_FrozenCitadel 스폰 테이블 에셋 생성 in `Assets/Resources/Data/SpawnTables/`
- [ ] T045 [P] [US5] Stage4_PoisonSwamp 스폰 테이블 에셋 생성 in `Assets/Resources/Data/SpawnTables/`
- [ ] T046 [P] [US5] Stage5_DarkTower 스폰 테이블 에셋 생성 in `Assets/Resources/Data/SpawnTables/`

---

## Phase 7: 에디터 도구 (US6 - Editor Tools) ✅ 완료

> 몬스터 에셋 생성을 자동화하는 에디터 도구를 구현합니다.

### 사용자 스토리
*"개발자로서 몬스터 데이터를 쉽게 생성하고 관리하고 싶다"*

### 완료 조건
- [x] 몬스터 에셋 일괄 생성 도구
- [ ] 스폰 테이블 에디터 윈도우
- [ ] 밸런스 검증 도구

### 태스크 목록

- [x] T047 [US6] MonsterAssetGenerator 에디터 도구 생성 in `Assets/_Project/Editor/MonsterAssetGenerator.cs`
- [ ] T048 [US6] SpawnTableEditorWindow 에디터 윈도우 생성 in `Assets/_Project/Editor/SpawnTableEditorWindow.cs`
- [ ] T049 [US6] MonsterBalanceValidator 밸런스 검증 도구 생성 in `Assets/_Project/Editor/MonsterBalanceValidator.cs`

---

## Phase 8: 폴리싱 (Polish) ⏳ 진행예정

> 최종 테스트 및 밸런스 조정을 수행합니다.

### 완료 조건
- [ ] 전체 몬스터 스폰 테스트
- [ ] 속성 데미지 검증
- [ ] 밸런스 수치 조정

### 태스크 목록

- [ ] T050 Unity 에디터에서 MonsterAssetGenerator 실행하여 전체 에셋 생성
- [ ] T051 스테이지별 플레이테스트 및 밸런스 조정
- [ ] T052 난이도별 스케일링 테스트 (Easy/Normal/Hard)

---

## 의존성 그래프

```
Phase 1 (기반 시스템) ⏳
    ↓
Phase 2 (스폰 테이블)
    ↓
┌───┴───┬───────┬───────┐
↓       ↓       ↓       ↓
Phase 3 Phase 4 Phase 5 Phase 6
(Normal)(Named) (Boss)  (Stage Tables)
    ↓       ↓       ↓       ↓
    └───────┴───────┴───────┘
                ↓
            Phase 7 (에디터 도구)
                ↓
            Phase 8 (폴리싱)
```

---

## 병렬 실행 가이드

### Phase 3-6 병렬 실행 가능
Phase 1, 2 완료 후 다음 태스크들은 병렬로 실행 가능:
- T011~T025 (Normal 몬스터 15종)
- T026~T035 (Named 몬스터 10종)
- T036~T041 (Boss 몬스터 6종)
- T042~T046 (스폰 테이블 5종)

### 추천 실행 순서
1. **MVP (최소 기능)**: Phase 1 → Phase 2 → T011~T015 (BasicMelee 5종) → T042 (Stage 1)
2. **확장**: 나머지 Phase 3~6 병렬 진행
3. **완성**: Phase 7, 8

---

## 몬스터 스탯 참조표

### EnemyType별 기본 스탯
| Type | Base HP | Base Attack | Gold | EXP | Bone |
|------|:-------:|:-----------:|:----:|:---:|:----:|
| Normal | 30 | 5 | 15-25 | 10 | 0-1 |
| Named | 60 | 10 | 40-60 | 30 | 1-3 |
| MiniBoss | 300 | 15 | 80-120 | 80 | 3-5 |
| MidBoss | 600 | 20 | 150-200 | 150 | 5-8 |
| FinalBoss | 1200 | 30 | 300-400 | 300 | 10-15 |

### 스테이지 스케일링 공식
```
실제 HP = Base HP * (1 + Stage * 0.15)
실제 Attack = Base Attack * (1 + Stage * 0.1)
실제 Gold = Base Gold * (1 + Stage * 0.2)
실제 EXP = Base EXP * (1 + Stage * 0.15)
```

### 속성 상성표
| 공격 속성 | 강함 (x1.5) | 약함 (x0.5) |
|----------|------------|------------|
| Fire | Ice, Poison | Fire, Water |
| Ice | Thunder, Earth | Ice, Fire |
| Thunder | Ice, Flying | Thunder, Earth |
| Dark | Light | Dark |
| Light | Dark | Light |
| Poison | Earth, None | Poison, Fire |
| Earth | Thunder, Fire | Earth, Ice |

---

*생성: Claude Code (speckit.tasks)*
*최종 수정: 2026-01-07*
*현재 상태: 구현 시작 전*
