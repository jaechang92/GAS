# 데이터 모델: CharacterPhysics 시스템 완성

**기능**: 001-character-physics-completion
**날짜**: 2025-10-28
**상태**: Phase 1 설계

## 개요

이 문서는 CharacterPhysics 시스템 완성을 위한 데이터 모델을 정의합니다.

## Entity 1: SkullMovementProfile (ScriptableObject)

**목적**: 스컬별 이동 특성 데이터 저장

**필드**:
| 필드명 | 타입 | 기본값 | 범위 | 설명 |
|--------|------|--------|------|------|
| skullName | string | "" | - | 스컬 식별 이름 |
| moveSpeedMultiplier | float | 1.0f | 0.1 ~ 3.0 | 이동 속도 배율 |
| jumpHeightMultiplier | float | 1.0f | 0.1 ~ 3.0 | 점프 높이 배율 |
| airControlMultiplier | float | 1.0f | 0.1 ~ 3.0 | 공중 제어력 배율 |
| wallJumpHorizontalMultiplier | float | 1.0f | 0.1 ~ 3.0 | 벽 점프 수평 속도 배율 |
| wallJumpVerticalMultiplier | float | 1.0f | 0.1 ~ 3.0 | 벽 점프 수직 속도 배율 |

**검증 규칙**:
- 모든 multiplier는 0.1 ~ 3.0 범위
- skullName은 빈 문자열 불가
- 기본 프로필 (Default Skull): 모든 배율 1.0

**샘플 인스턴스**:
- Default: (1.0, 1.0, 1.0, 1.0, 1.0)
- Warrior: (0.9, 0.85, 0.8, 0.9, 0.85)
- Mage: (1.15, 1.1, 1.25, 1.15, 1.1)

---

## Entity 2: WallDetectionData (Struct)

**목적**: 벽 감지 결과를 담는 데이터 구조

**필드**:
| 필드명 | 타입 | 설명 |
|--------|------|------|
| isOnWall | bool | 현재 벽에 닿아있는지 여부 |
| wallDirection | WallDirection | 벽의 방향 (Left, Right, None) |
| wallNormal | Vector2 | 벽 표면의 법선 벡터 |
| distanceToWall | float | 벽까지의 거리 |
| wallHit | RaycastHit2D | BoxCast 충돌 정보 |

**상태 전환**:
- None → Left/Right: 벽 감지 시
- Left/Right → None: 벽에서 떨어질 때
- Left ↔ Right: 반대쪽 벽으로 전환 시 (벽 점프)

---

## Entity 3: OneWayPlatformData (Component Data)

**목적**: 낙하 플랫폼의 충돌 관리 데이터

**필드**:
| 필드명 | 타입 | 설명 |
|--------|------|------|
| platformType | PlatformType | 플랫폼 타입 (OneWay, Solid 등) |
| passthroughCooldown | float | 통과 후 쿨다운 시간 (기본: 0.3초) |
| ignoredColliders | HashSet<Collider2D> | 현재 충돌 무시 중인 콜라이더 목록 |
| cooldownTimers | Dictionary<Collider2D, float> | 각 콜라이더별 남은 쿨다운 시간 |

---

## Entity 4: CharacterPhysicsState (기존 - 확장)

**목적**: 캐릭터의 현재 물리 상태 추적

**기존 필드**:
- isGrounded: bool
- currentVelocity: Vector2
- isFalling: bool
- isJumping: bool

**신규 필드**:
| 필드명 | 타입 | 설명 |
|--------|------|------|
| isWallSliding | bool | 벽 슬라이딩 중인지 여부 |
| currentWallDirection | WallDirection | 현재 벽 방향 |
| canWallJump | bool | 벽 점프 가능 여부 |
| activePlatformCooldowns | Dictionary<Collider2D, float> | 플랫폼별 쿨다운 타이머 |
| currentSkullProfile | SkullMovementProfile | 현재 적용 중인 스컬 프로필 |

---

## Entity 5: WallDirection (Enum)

**목적**: 벽 방향 정의

**값**:
- None = 0
- Left = -1
- Right = 1

---

## Entity 6: PlatformType (Enum)

**목적**: 플랫폼 타입 정의

**값**:
- Solid: 일반 플랫폼 (모든 방향 충돌)
- OneWay: 낙하 플랫폼 (위에서만 착지)
- Moving: 이동 플랫폼 (향후 확장용)
- Crumbling: 붕괴 플랫폼 (향후 확장용)

---

## 데이터 흐름

### 1. 벽 감지 Flow
```
FixedUpdate()
  → CheckWallCollision()
  → WallDetectionData 생성
  → CharacterPhysicsState 업데이트
  → FSM 상태 전환 (옵션)
```

### 2. 스컬 변경 Flow
```
OnSkullChanged Event
  → ApplySkullProfile()
  → currentSkullProfile 업데이트
  → 공중 상태 시 속도 재조정
```

### 3. 낙하 플랫폼 Flow
```
아래 방향 + 점프 입력
  → OneWayPlatform.RequestPassthrough()
  → Physics2D.IgnoreCollision(true)
  → Cooldown 타이머 시작
  → FixedUpdate에서 타이머 감소
  → 타이머 만료 시 충돌 복구
```

---

## 저장소 및 영속성

**ScriptableObjects** (Assets/Data/Physics/):
- DefaultSkullProfile.asset
- WarriorSkullProfile.asset
- MageSkullProfile.asset

**런타임 데이터**:
- CharacterPhysicsState (MonoBehaviour 인스턴스)
- WallDetectionData (struct, 매 프레임 계산)
- Platform Cooldowns (Dictionary, 런타임 관리)

**데이터베이스 없음**: 모든 데이터는 메모리 및 ScriptableObject로 관리

---

## 검증 및 제약사항

**SkullMovementProfile**:
- Multiplier 범위: [0.1, 3.0]
- 빈 skullName 불허

**WallDetectionData**:
- distanceToWall >= 0
- wallDirection와 wallNormal 일관성 유지

**OneWayPlatformData**:
- passthroughCooldown >= 0.1s (최소값)
- ignoredColliders의 null 참조 방지

**CharacterPhysicsState**:
- isWallSliding && isGrounded == false (상호 배타)
- currentSkullProfile != null (항상 기본 프로필 로드)

---

## 성능 고려사항

**메모리**:
- ScriptableObject: 공유 데이터, 인스턴스당 ~100 bytes
- Dictionary (cooldowns): 활성 플랫폼당 ~50 bytes
- HashSet (ignoredColliders): 충돌 중인 플레이어당 ~30 bytes

**CPU**:
- BoxCast (벽 감지): ~0.1ms per cast (좌우 2회)
- Dictionary 업데이트: O(n), n = 활성 쿨다운 수 (보통 < 5)
- IgnoreCollision: ~0.01ms per call

**최적화**:
- BoxCast를 매 프레임이 아닌 입력 있을 때만 수행 (옵션)
- Cooldown 만료 시 즉시 Dictionary에서 제거
- ScriptableObject 공유로 메모리 절약

---

## 테스트 전략

**단위 테스트**:
- SkullMovementProfile: 배율 범위 검증
- WallDetectionData: 벽 방향 계산 검증
- Cooldown Timer: 타이머 감소 로직 검증

**통합 테스트**:
- 스컬 변경 시 물리 특성 즉시 적용 확인
- 벽 점프 연속 실행 시 방향 전환 확인
- 플랫폼 통과 후 쿨다운 동작 확인

**Demo Scene**:
- 3가지 스컬 전환 테스트
- 벽 점프 수직 통로 테스트
- 낙하 플랫폼 3층 구조 테스트
