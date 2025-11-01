# CharacterPhysics API 확장

**기능**: 001-character-physics-completion
**날짜**: 2025-10-28

## 개요

CharacterPhysics 클래스에 추가될 새로운 public API를 정의합니다.

## 벽 상호작용 API

### CheckWallCollision
```csharp
/// <summary>
/// 지정된 방향으로 벽 충돌을 감지합니다.
/// </summary>
/// <param name="direction">감지 방향 (-1: 좌, 1: 우)</param>
/// <param name="distance">감지 거리 (기본: 0.1f)</param>
/// <returns>벽 감지 데이터</returns>
public WallDetectionData CheckWallCollision(int direction, float distance = 0.1f)
```

**사용 예시**:
```csharp
var leftWall = physics.CheckWallCollision(-1);
if (leftWall.isOnWall)
{
    Debug.Log("왼쪽 벽 감지됨");
}
```

### StartWallSlide
```csharp
/// <summary>
/// 벽 슬라이딩 상태를 시작합니다.
/// </summary>
/// <param name="direction">벽 방향</param>
public void StartWallSlide(WallDirection direction)
```

### StopWallSlide
```csharp
/// <summary>
/// 벽 슬라이딩 상태를 종료합니다.
/// </summary>
public void StopWallSlide()
```

### PerformWallJump
```csharp
/// <summary>
/// 벽 점프를 수행합니다.
/// </summary>
public void PerformWallJump()
```

**요구사항**:
- isWallSliding == true
- currentWallDirection != None
- jumpKey 입력 감지됨

---

## 일방향 플랫폼 API

### RequestPlatformPassthrough
```csharp
/// <summary>
/// 낙하 플랫폼 통과를 요청합니다.
/// </summary>
/// <param name="platform">통과할 플랫폼</param>
public void RequestPlatformPassthrough(OneWayPlatform platform)
```

### UpdatePlatformCooldowns
```csharp
/// <summary>
/// 플랫폼 쿨다운 타이머를 업데이트합니다.
/// FixedUpdate에서 호출됩니다.
/// </summary>
private void UpdatePlatformCooldowns()
```

**내부 사용 전용** (private 메서드)

---

## 스컬 이동 API

### ApplySkullProfile
```csharp
/// <summary>
/// 스컬 이동 프로필을 적용합니다.
/// </summary>
/// <param name="profile">적용할 프로필</param>
public void ApplySkullProfile(SkullMovementProfile profile)
```

**동작**:
- currentSkullProfile 교체
- 공중 상태 시 현재 속도 재조정
- 즉시 적용 (다음 프레임 대기 없음)

### GetModifiedSpeed
```csharp
/// <summary>
/// 스컬 프로필이 적용된 이동 속도를 반환합니다.
/// </summary>
/// <param name="baseSpeed">기본 이동 속도</param>
/// <returns>배율이 적용된 속도</returns>
public float GetModifiedSpeed(float baseSpeed)
```

**공식**: `baseSpeed * currentSkullProfile.moveSpeedMultiplier`

### GetModifiedJumpForce
```csharp
/// <summary>
/// 스컬 프로필이 적용된 점프 힘을 반환합니다.
/// </summary>
/// <param name="baseForce">기본 점프 힘</param>
/// <returns>배율이 적용된 점프 힘</returns>
public float GetModifiedJumpForce(float baseForce)
```

---

## 프로퍼티

### IsWallSliding
```csharp
/// <summary>
/// 현재 벽 슬라이딩 중인지 여부
/// </summary>
public bool IsWallSliding { get; private set; }
```

### CurrentWallDirection
```csharp
/// <summary>
/// 현재 벽 방향
/// </summary>
public WallDirection CurrentWallDirection { get; private set; }
```

### CanWallJump
```csharp
/// <summary>
/// 벽 점프 가능 여부
/// </summary>
public bool CanWallJump => IsWallSliding && !IsGrounded;
```

---

## 이벤트

### OnWallSlideStart
```csharp
/// <summary>
/// 벽 슬라이딩 시작 시 발생
/// </summary>
public event Action<WallDirection> OnWallSlideStart;
```

### OnWallSlideEnd
```csharp
/// <summary>
/// 벽 슬라이딩 종료 시 발생
/// </summary>
public event Action OnWallSlideEnd;
```

### OnWallJump
```csharp
/// <summary>
/// 벽 점프 수행 시 발생
/// </summary>
public event Action<WallDirection> OnWallJump;
```

**사용 예시**:
```csharp
physics.OnWallJump += (direction) =>
{
    Debug.Log($"벽 점프 수행: {direction}");
};
```

---

## InputHandler와의 통합

### 새 입력 추가 필요

```csharp
public class InputHandler : MonoBehaviour
{
    // 기존
    public bool JumpPressed { get; private set; }
    public float MoveInput { get; private set; }

    // 신규 추가
    public bool DownPressed { get; private set; }  // 아래 방향 키
    public int WallDirection => MoveInput > 0 ? 1 : (MoveInput < 0 ? -1 : 0);
}
```

---

## 설정

### Inspector 노출 필드

```csharp
[Header("벽 상호작용")]
[SerializeField] private float wallSlideSpeed = 2f;
[SerializeField] private float wallJumpHorizontalForce = 10f;
[SerializeField] private float wallJumpVerticalForce = 15f;
[SerializeField] private float wallDetectionDistance = 0.1f;

[Header("일방향 플랫폼")]
[SerializeField] private float platformPassthroughCooldown = 0.3f;

[Header("스컬 이동")]
[SerializeField] private SkullMovementProfile defaultProfile;
```

---

## 오류 처리

### Assertions
```csharp
void PerformWallJump()
{
    Debug.Assert(IsWallSliding, "벽 점프 불가: 벽 슬라이딩 중이 아님");
    Debug.Assert(CurrentWallDirection != WallDirection.None, "잘못된 벽 방향");

    // 구현
}
```

### Null 체크
```csharp
void ApplySkullProfile(SkullMovementProfile profile)
{
    if (profile == null)
    {
        Debug.LogWarning("Null 프로필 제공됨, 기본값 사용");
        profile = defaultProfile;
    }

    // 구현
}
```

---

## 성능 노트

- 벽 충돌 체크: 매 FixedUpdate (~50Hz)
- 플랫폼 쿨다운 업데이트: 매 FixedUpdate
- 이벤트 호출: 상태 변경 시에만 (프레임당 0-2회)

**최적화**:
- BoxCast 결과 캐싱 (동일 프레임 내 재사용)
- Dictionary 용량 예약 (platformCooldowns.EnsureCapacity(5))

---

## 하위 호환성

기존 CharacterPhysics API는 변경 없이 유지:
- CalculateVelocity()
- ApplyMovement()
- CheckGroundCollision()
- HandleJump()

새 API는 별도 메서드로 추가되어 기존 코드와 충돌 없음.
