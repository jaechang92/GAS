# 구현 계획: CharacterPhysics 시스템 완성

**브랜치**: `001-character-physics-completion` | **날짜**: 2025-10-28 | **명세**: [spec.md](./spec.md)
**입력**: 명세 문서에서 기능 요구사항 추출

## 요약

CharacterPhysics 시스템에 3가지 핵심 플랫포머 메커닉을 추가합니다:
1. **벽 점프 및 슬라이딩** (P1): 수직 공간 탐험 가능
2. **낙하 플랫폼** (P2): 일방향 통과 시스템
3. **스컬별 이동 특성** (P3): 클래스별 차별화된 물리 특성

기존 CharacterPhysics.cs를 확장하며 ScriptableObject 기반 데이터 관리를 유지합니다.

---

## Phase 0: 연구 (Research)

**목표**: 기술적 실현 가능성 조사 및 구현 패턴 결정

**산출물**: [research.md](./research.md)

### 연구 작업

#### RT-001: 쿨다운 타이머 구현
**질문**: 낙하 플랫폼 통과 후 0.3초 쿨다운을 어떻게 구현할 것인가?

**결정**: FixedUpdate + Dictionary<Collider2D, float> 타이머

**근거**:
- Constitution Principle VI 준수: Coroutine 금지
- 물리는 FixedUpdate에서 처리되므로 동기 타이머가 자연스러움
- 여러 플랫폼의 쿨다운을 Dictionary로 효율적으로 관리
- async는 I/O나 긴 작업에 적합, 0.3초 타이머는 동기가 더 단순

---

#### RT-002: 벽 감지 Layer 아키텍처
**질문**: 벽 감지를 위한 별도 Layer가 필요한가?

**결정**: Ground Layer 재사용 (Wall Layer 불필요)

**근거**:
- 기존 Layer 시스템 단순 유지
- 벽도 플랫폼의 일종 (Ground로 간주 가능)
- BoxCast 방향으로 벽 vs 바닥 구분 가능
  - 좌우 BoxCast → 벽
  - 하단 BoxCast → 바닥
- Layer 추가는 프로젝트 복잡도 증가

---

#### RT-003: Skull 이벤트 통합
**질문**: 스컬 변경 이벤트를 어떻게 구독하고 물리 특성을 적용할 것인가?

**결정**: 이벤트 구독 + 즉시 적용 패턴

**근거**:
- Unity의 C# 이벤트 패턴 사용
- 스컬 변경 시 물리 특성을 즉시 재계산
- ScriptableObject 참조를 교체하는 방식

**엣지 케이스 처리**:
- 공중/벽 슬라이딩 중 스컬 변경: 현재 속도를 새 배율로 재조정
- 초기화: Start에서 기본 프로필 로드

---

#### RT-004: Unity Physics2D 일방향 플랫폼 패턴
**질문**: PlatformEffector2D vs 수동 Physics2D.IgnoreCollision?

**결정**: Physics2D.IgnoreCollision + 타이머 관리

**근거**:
- PlatformEffector2D는 제어가 제한적 (쿨다운 커스터마이징 어려움)
- IgnoreCollision은 완전한 제어 가능
- 쿨다운 타이머와 통합 용이 (RT-001 패턴 재사용)

**충돌 조건**:
- 아래 방향 + 점프 입력: IgnoreCollision 활성화
- 위에서 떨어질 때 (velocity.y <= 0): 충돌 허용
- 아래에서 위로 점프: IgnoreCollision 유지

---

### 연구 요약

| 연구 작업 | 결정 사항 | 핵심 기술 |
|-----------|-----------|-----------|
| RT-001 | FixedUpdate + Dictionary 타이머 | Time.fixedDeltaTime |
| RT-002 | Ground Layer 재사용 | Physics2D.BoxCast |
| RT-003 | C# 이벤트 구독 + 즉시 적용 | OnSkullChanged event |
| RT-004 | Physics2D.IgnoreCollision | 수동 충돌 관리 |

**Constitution 준수**: ✅ 모든 결정이 Principle VI (Async Pattern) 준수

---

## Phase 1: 설계 (Design)

**목표**: 데이터 모델, API 계약, 테스트 가이드 작성

**산출물**:
- [data-model.md](./data-model.md)
- [contracts/CharacterPhysicsAPI.md](./contracts/CharacterPhysicsAPI.md)
- [contracts/OneWayPlatformAPI.md](./contracts/OneWayPlatformAPI.md)
- [quickstart.md](./quickstart.md)

### 데이터 모델

**6개 Entity 정의**:

1. **SkullMovementProfile** (ScriptableObject)
   - 목적: 스컬별 이동 특성 데이터 저장
   - 필드: skullName, moveSpeedMultiplier, jumpHeightMultiplier, airControlMultiplier, wallJumpHorizontal/VerticalMultiplier

2. **WallDetectionData** (Struct)
   - 목적: 벽 감지 결과 데이터
   - 필드: isOnWall, wallDirection, wallNormal, distanceToWall, wallHit

3. **OneWayPlatformData** (Component Data)
   - 목적: 낙하 플랫폼 충돌 관리
   - 필드: platformType, passthroughCooldown, ignoredColliders, cooldownTimers

4. **CharacterPhysicsState** (기존 확장)
   - 신규 필드: isWallSliding, currentWallDirection, canWallJump, activePlatformCooldowns, currentSkullProfile

5. **WallDirection** (Enum)
   - 값: None = 0, Left = -1, Right = 1

6. **PlatformType** (Enum)
   - 값: Solid, OneWay, Moving, Crumbling

**세부 내용**: [data-model.md](./data-model.md) 참조

---

### API 계약

#### CharacterPhysics API 확장

**벽 상호작용 API**:
- `CheckWallCollision(int direction, float distance = 0.1f)` → WallDetectionData
- `StartWallSlide(WallDirection direction)`
- `StopWallSlide()`
- `PerformWallJump()`

**일방향 플랫폼 API**:
- `RequestPlatformPassthrough(OneWayPlatform platform)`
- `UpdatePlatformCooldowns()` (private)

**스컬 이동 API**:
- `ApplySkullProfile(SkullMovementProfile profile)`
- `GetModifiedSpeed(float baseSpeed)` → float
- `GetModifiedJumpForce(float baseForce)` → float

**프로퍼티**:
- `IsWallSliding { get; private set; }`
- `CurrentWallDirection { get; private set; }`
- `CanWallJump => IsWallSliding && !IsGrounded`

**이벤트**:
- `OnWallSlideStart` (WallDirection)
- `OnWallSlideEnd` ()
- `OnWallJump` (WallDirection)

**세부 내용**: [contracts/CharacterPhysicsAPI.md](./contracts/CharacterPhysicsAPI.md) 참조

---

#### OneWayPlatform API

**Public API**:
- `RequestPassthrough(Collider2D playerCollider)`
- `ResetPassthrough(Collider2D playerCollider)`
- `CanLandOn(Vector2 playerVelocity)` → bool
- `IsIgnoringCollider(Collider2D collider)` → bool

**설정**:
- platformType: PlatformType (OneWay/Solid)
- passthroughCooldown: float (기본 0.3초)
- playerLayer: LayerMask

**세부 내용**: [contracts/OneWayPlatformAPI.md](./contracts/OneWayPlatformAPI.md) 참조

---

### Quickstart 가이드

**목적**: 새로운 기능을 빠르게 테스트할 수 있는 Demo Scene 가이드

**내용**:
- Demo Scene 설정 방법
- 조작 가이드 (기본/벽 점프/낙하 플랫폼/스컬 변경)
- 3가지 테스트 시나리오
- 엣지 케이스 테스트
- 디버깅 및 성능 모니터링
- 문제 해결 가이드

**세부 내용**: [quickstart.md](./quickstart.md) 참조

---

## Phase 2: 작업 생성 (Tasks)

**상태**: ⏳ PENDING

**다음 단계**: `/speckit.tasks` 명령 실행

**산출물**:
- tasks.md (구현 작업 목록)
- 각 작업은 Acceptance Criteria와 연결

---

## 구현 범위

### 수정할 파일
- `CharacterPhysics.cs`: 벽 점프, 플랫폼 로직 추가
- `InputHandler.cs`: 아래 방향 키 입력 추가 (DownPressed)

### 생성할 파일
- `SkullMovementProfile.cs`: ScriptableObject 정의
- `OneWayPlatform.cs`: 일방향 플랫폼 Component
- `WallDirection.cs`: Enum 정의
- `PlatformType.cs`: Enum 정의
- `PlayerWallSlideState.cs`: FSM State (옵션)
- `PhysicsCompletionDemo.cs`: Demo Scene 스크립트
- `Assets/Data/Physics/`: 스컬 프로필 ScriptableObject 인스턴스 (Default, Warrior, Mage)

---

## Constitution 준수 확인

**모든 9가지 원칙 준수**: ✅ PASS

**주요 체크**:
- ✅ Principle VI: NO Coroutines (FixedUpdate 타이머 사용)
- ✅ Principle IX: linearVelocity 사용, FindAnyObjectByType 사용
- ✅ CamelCase 네이밍 규칙
- ✅ SOLID 원칙
- ✅ 완성 우선 원칙: 기존 시스템 확장
- ✅ 단계적 개발: Phase별 진행

---

## 성공 기준 (Success Criteria)

### 측정 가능한 결과

- **SC-001**: 15 유닛 수직 벽 통로에서 벽 점프만으로 5초 이내 최상단 도달
- **SC-002**: 벽 슬라이딩 속도 <= 일반 낙하 속도의 30%
- **SC-003**: 3층 낙하 플랫폼 테스트에서 2초 이내 바닥 도달
- **SC-004**: 낙하 플랫폼 아래→위 착지 성공률 100%
- **SC-005**: 스컬 간 이동 시간 차이 >= 15%
- **SC-006**: 스컬 변경 후 0.1초 이내 특성 적용
- **SC-007**: 벽 점프 연속 실행 시 60 FPS 이상 유지
- **SC-008**: 플레이 테스터 90% 이상이 5분 이내 메커닉 이해

---

## 다음 단계

1. ✅ Phase 0: 연구 완료 (research.md)
2. ✅ Phase 1: 설계 완료 (data-model.md, contracts/, quickstart.md)
3. ⏳ **Phase 2: 작업 생성** - `/speckit.tasks` 실행
4. ⏳ Phase 3: 구현 시작
5. ⏳ Phase 4: Demo Scene 테스트
6. ⏳ Phase 5: Success Criteria 검증

---

## 빠른 링크

- **명세**: [spec.md](./spec.md) - 기능 명세서
- **연구**: [research.md](./research.md) - 기술 결정사항
- **데이터 모델**: [data-model.md](./data-model.md) - Entity 정의
- **API 계약**: [contracts/](./contracts/) - API 문서
- **빠른 시작**: [quickstart.md](./quickstart.md) - 테스트 가이드
- **체크리스트**: [checklists/requirements.md](./checklists/requirements.md) - 품질 검증

---

**상태**: ✅ 계획 완료, `/speckit.tasks` 실행 준비 완료
