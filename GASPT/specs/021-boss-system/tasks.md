# GASPT 보스 시스템 태스크

**기능 번호**: 021
**생성일**: 2025-12-14
**총 태스크**: 32개
**상태**: 대기 중

---

## Phase 1: 기반 시스템 (US1 - Boss Foundation)

> 보스 데이터 구조와 핵심 클래스를 구현합니다.

### 사용자 스토리
*"플레이어로서 일반 적과 다른 강력한 보스를 만나고 싶다"*

### 완료 조건
- [ ] BossData ScriptableObject 생성 가능
- [ ] BaseBoss가 Enemy를 확장하여 동작
- [ ] 페이즈 전환이 체력 기반으로 작동

### 태스크 목록

- [ ] T001 [US1] BossGrade enum 생성 in `Assets/_Project/Scripts/Core/Enums/BossGrade.cs`
- [ ] T002 [US1] PatternType enum 생성 in `Assets/_Project/Scripts/Core/Enums/PatternType.cs`
- [ ] T003 [P] [US1] BossPhaseData 구조체 생성 in `Assets/_Project/Scripts/Data/BossPhaseData.cs`
- [ ] T004 [US1] BossData ScriptableObject 생성 in `Assets/_Project/Scripts/Data/BossData.cs`
- [ ] T005 [US1] BaseBoss 기본 클래스 생성 in `Assets/_Project/Scripts/Gameplay/Boss/BaseBoss.cs`
- [ ] T006 [US1] BossPhaseController 생성 in `Assets/_Project/Scripts/Gameplay/Boss/BossPhaseController.cs`

---

## Phase 2: 패턴 AI 시스템 (US2 - Pattern AI)

> 보스 공격 패턴과 텔레그래프 시스템을 구현합니다.

### 사용자 스토리
*"플레이어로서 보스 패턴을 학습하고 회피하는 재미를 느끼고 싶다"*

### 완료 조건
- [ ] 패턴이 텔레그래프 후 실행
- [ ] 가중치 기반 패턴 선택 작동
- [ ] 패턴별 쿨다운 관리

### 태스크 목록

- [ ] T007 [US2] BossPattern 추상 클래스 생성 in `Assets/_Project/Scripts/Gameplay/Boss/Patterns/BossPattern.cs`
- [ ] T008 [US2] TelegraphController 생성 in `Assets/_Project/Scripts/Gameplay/Boss/TelegraphController.cs`
- [ ] T009 [P] [US2] MeleePattern 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Patterns/MeleePattern.cs`
- [ ] T010 [P] [US2] RangedPattern 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Patterns/RangedPattern.cs`
- [ ] T011 [P] [US2] AreaPattern 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Patterns/AreaPattern.cs`
- [ ] T012 [P] [US2] SummonPattern 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Patterns/SummonPattern.cs`
- [ ] T013 [P] [US2] BuffPattern 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Patterns/BuffPattern.cs`
- [ ] T014 [US2] PatternSelector 생성 in `Assets/_Project/Scripts/Gameplay/Boss/PatternSelector.cs`

---

## Phase 3: 보스 전용 UI (US3 - Boss UI)

> 보스 체력바와 등장 연출을 구현합니다.

### 사용자 스토리
*"플레이어로서 보스의 체력과 페이즈를 명확하게 확인하고 싶다"*

### 완료 조건
- [ ] 보스 체력바가 화면 상단에 표시
- [ ] 페이즈 전환 시 UI 업데이트
- [ ] 보스 등장 연출 재생

### 태스크 목록

- [ ] T015 [US3] IBossHealthBarView 인터페이스 생성 in `Assets/_Project/Scripts/UI/MVP/Interfaces/IBossHealthBarView.cs`
- [ ] T016 [US3] BossHealthBarView 구현 in `Assets/_Project/Scripts/UI/MVP/Views/BossHealthBarView.cs`
- [ ] T017 [US3] BossHealthBarPresenter 구현 in `Assets/_Project/Scripts/UI/MVP/Presenters/BossHealthBarPresenter.cs`
- [ ] T018 [US3] BossIntroController 생성 in `Assets/_Project/Scripts/UI/Boss/BossIntroController.cs`

---

## Phase 4: 미니보스 구현 (US4 - Mini Bosses)

> 스테이지 1, 2의 미니보스를 구현합니다.

### 사용자 스토리
*"플레이어로서 초반 스테이지에서 미니보스를 처치하고 싶다"*

### 완료 조건
- [ ] 불꽃 골렘 전투 가능
- [ ] 얼음 정령 전투 가능
- [ ] 각 보스별 패턴 동작

### 태스크 목록

- [ ] T019 [US4] FlameGolemBoss 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Bosses/FlameGolemBoss.cs`
- [ ] T020 [P] [US4] FlameGolem용 BossData 에셋 생성 *(Unity 에디터 필요)*
- [ ] T021 [US4] FrostSpiritBoss 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Bosses/FrostSpiritBoss.cs`
- [ ] T022 [P] [US4] FrostSpirit용 BossData 에셋 생성 *(Unity 에디터 필요)*

---

## Phase 5: 중간/최종보스 구현 (US5 - Major Bosses)

> 스테이지 3~5의 중간보스와 최종보스를 구현합니다.

### 사용자 스토리
*"플레이어로서 복잡한 패턴의 강력한 보스와 전투하고 싶다"*

### 완료 조건
- [ ] 번개 드래곤 전투 가능 (3페이즈)
- [ ] 어둠의 군주 전투 가능 (4페이즈)
- [ ] 페이즈별 패턴 변화 동작

### 태스크 목록

- [ ] T023 [US5] ThunderDragonBoss 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Bosses/ThunderDragonBoss.cs`
- [ ] T024 [P] [US5] ThunderDragon용 BossData 에셋 생성 *(Unity 에디터 필요)*
- [ ] T025 [US5] DarkLordBoss 구현 in `Assets/_Project/Scripts/Gameplay/Boss/Bosses/DarkLordBoss.cs`
- [ ] T026 [P] [US5] DarkLord용 BossData 에셋 생성 *(Unity 에디터 필요)*

---

## Phase 6: 보상 및 연동 (US6 - Rewards & Integration)

> 보상 시스템과 던전 연동을 구현합니다.

### 사용자 스토리
*"플레이어로서 보스를 처치하고 가치있는 보상을 받고 싶다"*

### 완료 조건
- [ ] 보스 처치 시 보상 지급
- [ ] 첫 클리어 보상 지급
- [ ] 던전 5번째 방에서 보스 전투 시작

### 태스크 목록

- [ ] T027 [US6] BossRewardSystem 생성 in `Assets/_Project/Scripts/Gameplay/Boss/BossRewardSystem.cs`
- [ ] T028 [US6] BossRoomController 생성 in `Assets/_Project/Scripts/Gameplay/Level/Room/BossRoomController.cs`
- [ ] T029 [US6] RoomType.Boss enum 추가 in `Assets/_Project/Scripts/Core/Enums/RoomType.cs`
- [ ] T030 [US6] RoomGenerator 보스 방 생성 로직 추가 in `Assets/_Project/Scripts/Gameplay/Level/Room/RoomGenerator.cs`

---

## Phase 7: 폴리싱 (Polish)

> 밸런싱 및 최종 테스트를 수행합니다.

### 완료 조건
- [ ] 보스별 전투 시간 적정 (미니: 45초, 중간: 90초, 최종: 180초)
- [ ] 패턴 학습 가능
- [ ] 60FPS 유지

### 태스크 목록

- [ ] T031 보스별 스탯 밸런싱 조정 in `Assets/Resources/Data/Bosses/*.asset` *(Unity 에디터 필요)*
- [ ] T032 통합 테스트 및 버그 수정

---

## 의존성 그래프

```
Phase 1 (기반) ─────────────────────┐
├── T001 BossGrade                  │
├── T002 PatternType                │
├── T003 BossPhaseData              │
├── T004 BossData                   │
├── T005 BaseBoss ←─────────────────┤
└── T006 BossPhaseController        │
            ↓                       │
Phase 2 (패턴) ─────────────────────┤
├── T007 BossPattern ←──────────────┤
├── T008 TelegraphController        │
├── T009~T013 패턴 구현 [병렬]      │
└── T014 PatternSelector            │
            ↓                       │
Phase 3 (UI) ──────────────────────┐│
├── T015 IBossHealthBarView        ││
├── T016 BossHealthBarView         ││
├── T017 BossHealthBarPresenter    ││
└── T018 BossIntroController       ││
            ↓                      ││
Phase 4 (미니보스) ────────────────┤│
├── T019~T020 FlameGolem [병렬]    ││
└── T021~T022 FrostSpirit [병렬]   ││
            ↓                      ││
Phase 5 (메이저보스) ──────────────┤│
├── T023~T024 ThunderDragon [병렬] ││
└── T025~T026 DarkLord [병렬]      ││
            ↓                      ││
Phase 6 (연동) ────────────────────┘│
├── T027 BossRewardSystem           │
├── T028 BossRoomController         │
├── T029 RoomType.Boss              │
└── T030 RoomGenerator 수정         │
            ↓                       │
Phase 7 (폴리싱) ───────────────────┘
├── T031 밸런싱
└── T032 통합 테스트
```

---

## 병렬 실행 가능 태스크

### Phase 1 내 병렬
- T003 (BossPhaseData)는 T001, T002와 독립적

### Phase 2 내 병렬
- T009~T013 (패턴 구현 5개)는 T007, T008 완료 후 병렬 가능

### Phase 4 내 병렬
- T019~T020 (FlameGolem)과 T021~T022 (FrostSpirit) 병렬 가능

### Phase 5 내 병렬
- T023~T024 (ThunderDragon)과 T025~T026 (DarkLord) 병렬 가능

---

## MVP 범위

**MVP = Phase 1 + Phase 2 + Phase 3 + Phase 4 (미니보스 1종)**

| 항목 | MVP 포함 |
|------|:--------:|
| 기반 시스템 | ✅ |
| 패턴 AI | ✅ |
| 보스 UI | ✅ |
| 불꽃 골렘 (미니보스) | ✅ |
| 얼음 정령 (미니보스) | ❌ |
| 번개 드래곤 (중간보스) | ❌ |
| 어둠의 군주 (최종보스) | ❌ |
| 보상 시스템 | ❌ |
| 던전 연동 | ❌ |

---

## 구현 전략

1. **점진적 전달**: Phase 1~3 완료 시 보스 전투 기본 동작 확인 가능
2. **수직 슬라이스**: FlameGolem 먼저 완성 후 다른 보스 추가
3. **UI 병행**: Phase 2와 Phase 3은 독립적으로 진행 가능
4. **데이터 기반**: BossData ScriptableObject로 밸런스 조정 용이

---

## 파일 생성 순서

### 신규 디렉토리 생성 필요
```
Assets/_Project/Scripts/Gameplay/Boss/
Assets/_Project/Scripts/Gameplay/Boss/Patterns/
Assets/_Project/Scripts/Gameplay/Boss/Bosses/
Assets/_Project/Scripts/UI/Boss/
Assets/Resources/Data/Bosses/
```

### 코드 파일 (22개)
1. `Core/Enums/BossGrade.cs`
2. `Core/Enums/PatternType.cs`
3. `Data/BossPhaseData.cs`
4. `Data/BossData.cs`
5. `Gameplay/Boss/BaseBoss.cs`
6. `Gameplay/Boss/BossPhaseController.cs`
7. `Gameplay/Boss/Patterns/BossPattern.cs`
8. `Gameplay/Boss/TelegraphController.cs`
9. `Gameplay/Boss/Patterns/MeleePattern.cs`
10. `Gameplay/Boss/Patterns/RangedPattern.cs`
11. `Gameplay/Boss/Patterns/AreaPattern.cs`
12. `Gameplay/Boss/Patterns/SummonPattern.cs`
13. `Gameplay/Boss/Patterns/BuffPattern.cs`
14. `Gameplay/Boss/PatternSelector.cs`
15. `UI/MVP/Interfaces/IBossHealthBarView.cs`
16. `UI/MVP/Views/BossHealthBarView.cs`
17. `UI/MVP/Presenters/BossHealthBarPresenter.cs`
18. `UI/Boss/BossIntroController.cs`
19. `Gameplay/Boss/Bosses/FlameGolemBoss.cs`
20. `Gameplay/Boss/Bosses/FrostSpiritBoss.cs`
21. `Gameplay/Boss/Bosses/ThunderDragonBoss.cs`
22. `Gameplay/Boss/Bosses/DarkLordBoss.cs`
23. `Gameplay/Boss/BossRewardSystem.cs`
24. `Gameplay/Level/Room/BossRoomController.cs`

### 수정 파일 (1개)
- `Core/Enums/RoomType.cs` - Boss 값 추가
- `Gameplay/Level/Room/RoomGenerator.cs` - 보스 방 생성 로직

### 에셋 파일 (Unity 에디터 필요)
- `Resources/Data/Bosses/FlameGolem.asset`
- `Resources/Data/Bosses/FrostSpirit.asset`
- `Resources/Data/Bosses/ThunderDragon.asset`
- `Resources/Data/Bosses/DarkLord.asset`

---

*생성: Claude Code*
*최종 수정: 2025-12-14*
