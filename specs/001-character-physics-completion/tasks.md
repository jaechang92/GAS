# Tasks: CharacterPhysics 시스템 완성

**Input**: Design documents from `/specs/001-character-physics-completion/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: 테스트는 명세에 명시되지 않아 포함하지 않음. Demo Scene으로 수동 검증 수행.

**Organization**: User Story별로 그룹화하여 각 스토리를 독립적으로 구현 및 테스트 가능하도록 구성.

## Format: `- [ ] [ID] [P?] [Story?] Description with file path`

- **[P]**: 병렬 실행 가능 (다른 파일, 의존성 없음)
- **[Story]**: 해당 User Story (US1, US2, US3)
- 파일 경로를 명확히 기재

## Path Conventions

Unity 프로젝트 구조:
- **Player 시스템**: `Assets/_Project/Scripts/Gameplay/Player/`
- **Physics**: `Assets/_Project/Scripts/Gameplay/Player/Physics/`
- **Data**: `Assets/_Project/Scripts/Data/Physics/` (신규 생성)
- **Environment**: `Assets/_Project/Scripts/Gameplay/Environment/`
- **Demo Scene**: `Assets/_Project/Scenes/`

---

## Phase 1: Setup (공유 인프라)

**Purpose**: 프로젝트 초기화 및 기본 구조

- [x] T001 Unity Layer 설정 확인 - Ground Layer 존재 여부 확인 (Project Settings > Tags and Layers)
- [x] T002 [P] OneWayPlatform Tag 생성 (Project Settings > Tags and Layers에 "OneWayPlatform" 추가)
- [x] T003 [P] Data/Physics 폴더 생성 - `Assets/_Project/Scripts/Data/Physics/` 폴더 생성

---

## Phase 2: Foundational (차단 전제조건)

**Purpose**: 모든 User Story 구현 전에 완료되어야 하는 핵심 인프라

**⚠️ CRITICAL**: 이 단계가 완료되기 전까지 User Story 작업 시작 불가

- [x] T004 WallDirection Enum 생성 - `Assets/_Project/Scripts/Gameplay/Player/Physics/WallDirection.cs` 작성 (None=0, Left=-1, Right=1)
- [x] T005 [P] PlatformType Enum 생성 - `Assets/_Project/Scripts/Gameplay/Environment/PlatformType.cs` 작성 (Solid, OneWay, Moving, Crumbling)
- [x] T006 [P] SkullMovementProfile ScriptableObject 생성 - `Assets/_Project/Scripts/Data/Physics/SkullMovementProfile.cs` 작성
- [x] T007 CharacterPhysics.cs 현재 상태 분석 - 기존 API 확인 및 확장 지점 파악 (`Assets/_Project/Scripts/Gameplay/Player/Physics/CharacterPhysics.cs`)
- [x] T008 InputHandler.cs 확장 - DownPressed 프로퍼티 추가 (`Assets/_Project/Scripts/Gameplay/Player/InputHandler.cs`)

**Checkpoint**: 기반 준비 완료 - User Story 구현 병렬 시작 가능

---

## Phase 3: User Story 1 - 벽 점프 및 슬라이딩 (Priority: P1) 🎯 MVP

**Goal**: 플레이어가 양쪽 벽을 번갈아 점프하며 수직 공간을 탐험할 수 있도록 벽 슬라이딩과 벽 점프 메커닉 구현

**Independent Test**: 좌우 벽이 배치된 수직 통로(높이 15 유닛)에서 플레이어가 양쪽 벽을 번갈아 점프하며 최상단까지 도달 가능. 벽 슬라이딩 속도가 일반 낙하 속도의 30% 이하인지 확인.

### Data Structures for US1

- [x] T009 [P] [US1] WallDetectionData Struct 정의 - CharacterPhysics.cs 내부에 struct 정의 (isOnWall, wallDirection, wallNormal, distanceToWall, wallHit)

### Core Physics Implementation for US1

- [x] T010 [US1] CheckWallCollision 메서드 구현 - CharacterPhysics.cs에 BoxCast 기반 벽 감지 로직 추가 (좌우 0.1유닛 거리, Ground Layer)
- [x] T011 [US1] StartWallSlide 메서드 구현 - CharacterPhysics.cs에 벽 슬라이딩 시작 로직 추가 (isWallSliding = true, currentWallDirection 설정, OnWallSlideStart 이벤트 발생)
- [x] T012 [US1] StopWallSlide 메서드 구현 - CharacterPhysics.cs에 벽 슬라이딩 종료 로직 추가 (isWallSliding = false, OnWallSlideEnd 이벤트 발생)
- [x] T013 [US1] PerformWallJump 메서드 구현 - CharacterPhysics.cs에 벽 점프 로직 추가 (수평 120%, 수직 85% 속도, OnWallJump 이벤트 발생)

### State Management for US1

- [x] T014 [US1] CharacterPhysics 상태 필드 추가 - isWallSliding, currentWallDirection, CanWallJump 프로퍼티 추가
- [x] T015 [US1] FixedUpdate 벽 감지 로직 통합 - FixedUpdate에서 매 프레임 CheckWallCollision 호출 및 상태 업데이트
- [x] T016 [US1] 벽 슬라이딩 속도 제어 구현 - FixedUpdate에서 isWallSliding == true일 때 linearVelocity.y를 일반 낙하 속도의 30%로 제한

### Events for US1

- [x] T017 [P] [US1] 벽 상호작용 이벤트 정의 - CharacterPhysics.cs에 OnWallSlideStart, OnWallSlideEnd, OnWallJump 이벤트 추가

### FSM Integration (Optional) for US1

- [ ] T018 [US1] PlayerWallSlideState 생성 (옵션) - FSM 통합이 필요한 경우 `Assets/_Project/Scripts/Gameplay/Player/States/PlayerWallSlideState.cs` 작성

**Checkpoint**: US1 완료 - 벽 점프와 슬라이딩이 독립적으로 동작하며 수직 통로 테스트 시나리오 통과

---

## Phase 4: User Story 2 - 낙하 플랫폼 상호작용 (Priority: P2)

**Goal**: 플레이어가 일방향 플랫폼을 아래 방향+점프 입력으로 통과하고, 위에서만 착지할 수 있도록 구현

**Independent Test**: 3층 구조의 낙하 플랫폼 테스트 스테이지에서 최상단에서 아래 방향+점프 입력으로 2초 이내에 모든 플랫폼을 통과하여 바닥 도달. 아래에서 위로 점프 시 각 플랫폼에 정상 착지.

### Component Implementation for US2

- [x] T019 [P] [US2] OneWayPlatform Component 생성 - `Assets/_Project/Scripts/Gameplay/Environment/OneWayPlatform.cs` 작성
- [x] T020 [US2] OneWayPlatform.RequestPassthrough 구현 - Physics2D.IgnoreCollision 호출, ignoredColliders HashSet 관리, cooldownTimers Dictionary 시작
- [x] T021 [US2] OneWayPlatform.ResetPassthrough 구현 - IgnoreCollision 해제, ignoredColliders 및 cooldownTimers 정리
- [x] T022 [US2] OneWayPlatform.CanLandOn 구현 - playerVelocity.y <= 0 && !IsIgnoringCollider 체크
- [x] T023 [US2] OneWayPlatform.IsIgnoringCollider 구현 - HashSet 조회
- [x] T024 [US2] OneWayPlatform.FixedUpdate 쿨다운 관리 - Dictionary 타이머 감소 및 만료 시 ResetPassthrough 호출

### CharacterPhysics Integration for US2

- [x] T025 [US2] CharacterPhysics.RequestPlatformPassthrough 구현 - OneWayPlatform과 상호작용하는 메서드 추가
- [x] T026 [US2] CharacterPhysics.UpdatePlatformCooldowns 구현 (private) - FixedUpdate에서 호출되는 쿨다운 업데이트 로직
- [x] T027 [US2] CharacterPhysics 입력 처리 확장 - FixedUpdate에서 InputHandler.DownPressed && JumpPressed 감지 시 RequestPlatformPassthrough 호출
- [x] T028 [US2] activePlatformCooldowns 필드 추가 - CharacterPhysics에 Dictionary<Collider2D, float> 추가

### Events for US2

- [x] T029 [P] [US2] OneWayPlatform 이벤트 정의 - OnPassthroughRequested, OnPassthroughReset 이벤트 추가

**Checkpoint**: US2 완료 - 낙하 플랫폼이 독립적으로 동작하며 3층 테스트 시나리오 통과

---

## Phase 5: User Story 3 - 스컬별 이동 특성 (Priority: P3)

**Goal**: 스컬(클래스) 변경 시 이동 특성(속도, 점프, 공중 제어력)이 즉시 변경되어 각 스컬마다 고유한 플레이 느낌 제공

**Independent Test**: 3가지 스컬(기본/전사/마법사)을 각각 장착하고 동일한 장애물 코스를 통과하며 이동 속도, 점프 높이, 공중 제어력 차이를 체감. 동일 거리 이동 시 시간 차이가 15% 이상 확인.

### ScriptableObject Asset Creation for US3

- [x] T030 [P] [US3] DefaultSkullProfile Asset 생성 - `Assets/_Project/Scripts/Data/Physics/DefaultSkullProfile.asset` (모든 배율 1.0)
- [x] T031 [P] [US3] WarriorSkullProfile Asset 생성 - `Assets/_Project/Scripts/Data/Physics/WarriorSkullProfile.asset` (이동 0.9, 점프 0.85, 공중 0.8, 벽점프H 0.9, 벽점프V 0.85)
- [x] T032 [P] [US3] MageSkullProfile Asset 생성 - `Assets/_Project/Scripts/Data/Physics/MageSkullProfile.asset` (이동 1.15, 점프 1.1, 공중 1.25, 벽점프H 1.15, 벽점프V 1.1)

### CharacterPhysics Integration for US3

- [x] T033 [US3] CharacterPhysics.ApplySkullProfile 구현 - currentSkullProfile 교체 및 공중 상태 시 속도 재조정
- [x] T034 [US3] CharacterPhysics.GetModifiedSpeed 구현 - baseSpeed * currentSkullProfile.moveSpeedMultiplier
- [x] T035 [US3] CharacterPhysics.GetModifiedJumpForce 구현 - baseForce * currentSkullProfile.jumpHeightMultiplier
- [x] T036 [US3] currentSkullProfile 필드 추가 - CharacterPhysics에 SkullMovementProfile 참조 추가
- [x] T037 [US3] Start 메서드에서 기본 프로필 로드 - defaultProfile SerializeField 추가 및 Start에서 ApplySkullProfile(defaultProfile) 호출

### Skull System Integration for US3

- [x] T038 [US3] OnSkullChanged 이벤트 구독 - CharacterPhysics에서 PlayerController (또는 SkullManager)의 OnSkullChanged 이벤트 구독 및 ApplySkullProfile 호출
- [x] T039 [US3] 기존 이동/점프 로직에 배율 적용 - CalculateVelocity, HandleJump 등 기존 메서드에서 GetModifiedSpeed/GetModifiedJumpForce 호출

### Edge Case Handling for US3

- [x] T040 [US3] 공중/벽 슬라이딩 중 스컬 변경 처리 - ApplySkullProfile에서 현재 속도를 새 배율로 재조정하는 로직 추가
- [x] T041 [US3] Null 체크 및 기본값 처리 - ApplySkullProfile에서 null 프로필 입력 시 defaultProfile 사용

**Checkpoint**: US3 완료 - 스컬별 이동 특성이 독립적으로 동작하며 3가지 스컬 테스트 시나리오 통과

---

## Phase 6: Polish & Demo

**Purpose**: 통합 테스트 및 데모 씬 구성

### Demo Scene Setup

- [ ] T042 [P] PhysicsCompletionDemo Scene 생성 - `Assets/_Project/Scenes/PhysicsCompletionDemo.unity` 생성
- [ ] T043 수직 벽 통로 구조 생성 - Demo Scene에 15 유닛 높이 수직 통로 배치 (US1 테스트용)
- [ ] T044 3층 낙하 플랫폼 구조 생성 - Demo Scene에 3층 OneWayPlatform 배치 (US2 테스트용)
- [ ] T045 스컬 변경 UI 추가 - Demo Scene에 1/2/3 키 입력으로 스컬 변경하는 테스트 UI 추가 (US3 테스트용)
- [ ] T046 [P] PhysicsCompletionDemo.cs 스크립트 작성 - `Assets/_Project/Scripts/Gameplay/Demo/PhysicsCompletionDemo.cs` (자동 씬 설정 및 디버그 정보 표시)

### Integration & Validation

- [ ] T047 모든 User Story 통합 테스트 - Demo Scene에서 US1, US2, US3가 함께 동작하는지 확인
- [ ] T048 성능 프로파일링 - Unity Profiler로 CPU/Memory 사용량 확인 (CharacterPhysics.FixedUpdate < 0.5ms, 60 FPS 유지)
- [ ] T049 엣지 케이스 검증 - 벽 점프 연속 실행, 벽 슬라이딩 중 상태 전환, 플랫폼 통과 타이밍 등 8개 엣지 케이스 테스트

### Documentation & Polish

- [ ] T050 [P] Inspector 툴팁 추가 - SkullMovementProfile, OneWayPlatform의 모든 SerializeField에 [Tooltip] 추가
- [ ] T051 [P] Gizmos 구현 - CharacterPhysics 및 OneWayPlatform에 OnDrawGizmos 추가 (벽 감지 범위, 플랫폼 쿨다운 시각화)
- [ ] T052 Success Criteria 검증 - quickstart.md의 SC-001~SC-008 모든 성공 기준 달성 확인

**Final Checkpoint**: 전체 기능 완성 - 3개 User Story 모두 독립적으로 동작하며 통합 시나리오 통과

---

## Dependencies & Execution Order

### Story Dependencies
- **US1 (벽 점프)**: 독립 실행 가능 (Phase 2 완료 후)
- **US2 (낙하 플랫폼)**: 독립 실행 가능 (Phase 2 완료 후)
- **US3 (스컬 특성)**: 독립 실행 가능 (Phase 2 완료 후, US1 완료 시 벽 점프 배율 적용 가능)

### Recommended Implementation Order
1. **Phase 1-2**: Setup + Foundational (T001-T008)
2. **Phase 3**: US1 구현 (T009-T018) - MVP 기능
3. **Phase 4**: US2 구현 (T019-T029) - US1과 병렬 가능
4. **Phase 5**: US3 구현 (T030-T041) - US1과 병렬 가능
5. **Phase 6**: Demo & Polish (T042-T052)

### Parallel Execution Opportunities
- **Phase 2**: T004, T005, T006 병렬 실행 가능
- **Phase 3**: T009, T017 병렬 실행 가능
- **Phase 4**: T019, T029 병렬 실행 가능
- **Phase 5**: T030, T031, T032 병렬 실행 가능
- **Phase 6**: T042, T046, T050, T051 병렬 실행 가능

---

## Implementation Strategy

**MVP Scope**: User Story 1 (벽 점프 및 슬라이딩)만 구현하면 기본적인 수직 탐험 메커닉 제공 가능

**Incremental Delivery**:
1. US1 완료 → 수직 탐험 가능
2. US2 추가 → 레벨 디자인 유연성 확보
3. US3 추가 → 스컬별 차별화 완성

**Constitution Compliance**: 모든 작업이 9가지 원칙 준수
- Principle VI: Coroutine 사용 금지 (FixedUpdate + Dictionary 타이머)
- Principle IX: linearVelocity 사용, FindAnyObjectByType 사용
- CamelCase 네이밍, SOLID 원칙, 완성 우선

---

**Total Tasks**: 52
- Setup: 3
- Foundational: 5
- US1: 10
- US2: 11
- US3: 12
- Polish & Demo: 11

**Estimated Parallel Tasks**: 13 tasks can run in parallel (marked with [P])

**Independent Test Criteria**:
- US1: 15유닛 수직 통로 5초 이내 등반
- US2: 3층 플랫폼 2초 이내 통과
- US3: 스컬 간 15% 이상 이동 시간 차이
