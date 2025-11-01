# OneWayPlatform API 계약서

**기능**: 001-character-physics-completion
**날짜**: 2025-10-28

## 개요

OneWayPlatform 컴포넌트의 public API를 정의합니다.

## 클래스 정의

```csharp
public class OneWayPlatform : MonoBehaviour
{
    // Public API
    public void RequestPassthrough(Collider2D playerCollider);
    public void ResetPassthrough(Collider2D playerCollider);
    public bool CanLandOn(Vector2 playerVelocity);
    public bool IsIgnoringCollider(Collider2D collider);

    // 설정
    [SerializeField] private PlatformType platformType = PlatformType.OneWay;
    [SerializeField] private float passthroughCooldown = 0.3f;
    [SerializeField] private LayerMask playerLayer;
}
```

---

## 메서드

### RequestPassthrough
```csharp
/// <summary>
/// 플레이어가 플랫폼을 통과하도록 충돌을 무시합니다.
/// </summary>
/// <param name="playerCollider">플레이어 Collider2D</param>
public void RequestPassthrough(Collider2D playerCollider)
```

**동작**:
1. `Physics2D.IgnoreCollision(platformCollider, playerCollider, true)` 호출
2. `ignoredColliders` HashSet에 추가
3. `cooldownTimers` Dictionary에 쿨다운 시작
4. `OnPassthroughRequested` 이벤트 발생

**요구사항**:
- playerCollider != null
- platformType == OneWay

---

### ResetPassthrough
```csharp
/// <summary>
/// 플레이어와의 충돌을 복구합니다.
/// </summary>
/// <param name="playerCollider">플레이어 Collider2D</param>
public void ResetPassthrough(Collider2D playerCollider)
```

**동작**:
1. `Physics2D.IgnoreCollision(platformCollider, playerCollider, false)` 호출
2. `ignoredColliders`에서 제거
3. `cooldownTimers`에서 제거
4. `OnPassthroughReset` 이벤트 발생

---

### CanLandOn
```csharp
/// <summary>
/// 현재 플레이어가 플랫폼에 착지할 수 있는지 확인합니다.
/// </summary>
/// <param name="playerVelocity">플레이어의 현재 속도</param>
/// <returns>착지 가능 여부</returns>
public bool CanLandOn(Vector2 playerVelocity)
```

**로직**:
```csharp
return playerVelocity.y <= 0 && !IsIgnoringCollider(playerCollider);
```

**조건**:
- 아래로 이동 중 (velocity.y <= 0)
- 현재 충돌 무시 상태가 아님

---

### IsIgnoringCollider
```csharp
/// <summary>
/// 특정 콜라이더를 현재 무시하고 있는지 확인합니다.
/// </summary>
/// <param name="collider">확인할 Collider2D</param>
/// <returns>무시 중이면 true</returns>
public bool IsIgnoringCollider(Collider2D collider)
```

---

## 내부 메서드 (Private)

### UpdateCooldowns
```csharp
/// <summary>
/// FixedUpdate에서 호출되어 모든 쿨다운 타이머를 업데이트합니다.
/// </summary>
private void UpdateCooldowns()
```

**구현**:
```csharp
private void FixedUpdate()
{
    foreach (var collider in cooldownTimers.Keys.ToList())
    {
        cooldownTimers[collider] -= Time.fixedDeltaTime;

        if (cooldownTimers[collider] <= 0)
        {
            ResetPassthrough(collider);
        }
    }
}
```

---

## 이벤트

### OnPassthroughRequested
```csharp
/// <summary>
/// 플레이어가 통과를 요청할 때 발생
/// </summary>
public event Action<Collider2D> OnPassthroughRequested;
```

### OnPassthroughReset
```csharp
/// <summary>
/// 충돌이 복구될 때 발생
/// </summary>
public event Action<Collider2D> OnPassthroughReset;
```

---

## 사용 예시

```csharp
// CharacterPhysics.cs
void FixedUpdate()
{
    if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.Space))
    {
        // 현재 서있는 플랫폼 찾기
        var platform = GetCurrentPlatform();

        if (platform != null && platform is OneWayPlatform oneWay)
        {
            oneWay.RequestPassthrough(GetComponent<Collider2D>());
        }
    }
}

OneWayPlatform GetCurrentPlatform()
{
    RaycastHit2D hit = Physics2D.BoxCast(
        transform.position,
        boxSize,
        0f,
        Vector2.down,
        0.1f,
        groundLayer
    );

    return hit.collider?.GetComponent<OneWayPlatform>();
}
```

---

## Editor 통합

### Custom Inspector (옵션)

```csharp
[CustomEditor(typeof(OneWayPlatform))]
public class OneWayPlatformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        OneWayPlatform platform = (OneWayPlatform)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("런타임 정보", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"무시 중인 콜라이더 수: {platform.IgnoredCount}");
        EditorGUILayout.LabelField($"활성 쿨다운 수: {platform.CooldownCount}");
    }
}
```

---

## 테스트

### 단위 테스트 예시

```csharp
[Test]
public void RequestPassthrough_충돌무시됨()
{
    // Arrange
    var platform = CreatePlatform();
    var playerCollider = CreatePlayerCollider();

    // Act
    platform.RequestPassthrough(playerCollider);

    // Assert
    Assert.IsTrue(platform.IsIgnoringCollider(playerCollider));
}

[Test]
public void 쿨다운만료_충돌복구됨()
{
    // Arrange
    var platform = CreatePlatform();
    var playerCollider = CreatePlayerCollider();
    platform.RequestPassthrough(playerCollider);

    // Act
    WaitForSeconds(0.4f); // > 0.3s 쿨다운

    // Assert
    Assert.IsFalse(platform.IsIgnoringCollider(playerCollider));
}
```

---

## 설정

### Inspector 필드

```csharp
[Header("플랫폼 설정")]
[Tooltip("플랫폼 타입")]
[SerializeField] private PlatformType platformType = PlatformType.OneWay;

[Header("통과 설정")]
[Tooltip("통과 후 충돌 복구까지 대기 시간 (초)")]
[SerializeField] private float passthroughCooldown = 0.3f;

[Tooltip("플레이어 레이어 (충돌 감지용)")]
[SerializeField] private LayerMask playerLayer;

[Header("디버그")]
[Tooltip("디버그 정보 표시")]
[SerializeField] private bool showDebugInfo = false;
```

---

## Gizmos

```csharp
private void OnDrawGizmos()
{
    if (!showDebugInfo) return;

    Gizmos.color = Color.yellow;
    var bounds = GetComponent<Collider2D>().bounds;
    Gizmos.DrawWireCube(bounds.center, bounds.size);

    // 무시 중인 콜라이더 표시
    Gizmos.color = Color.red;
    foreach (var collider in ignoredColliders)
    {
        if (collider != null)
        {
            Gizmos.DrawLine(bounds.center, collider.bounds.center);
        }
    }
}
```

---

## 성능

**메모리**:
- HashSet: 무시된 콜라이더당 ~30 bytes
- Dictionary: 활성 쿨다운당 ~50 bytes
- 총합: 플랫폼당 < 500 bytes (동시 플레이어 < 5명 가정)

**CPU**:
- UpdateCooldowns: O(n), n = 활성 쿨다운 (보통 < 3)
- IsIgnoringCollider: O(1) HashSet 조회
- RequestPassthrough: O(1) 연산

---

## 오류 처리

```csharp
public void RequestPassthrough(Collider2D playerCollider)
{
    if (playerCollider == null)
    {
        Debug.LogError("통과 요청 불가: null 콜라이더");
        return;
    }

    if (platformType != PlatformType.OneWay)
    {
        Debug.LogWarning("OneWay가 아닌 플랫폼에서 RequestPassthrough 호출됨");
        return;
    }

    // 구현
}
```

---

## 스레드 안전성

**스레드 안전하지 않음**. 모든 메서드는 Unity 메인 스레드에서 호출되어야 합니다.

---

## 생명 주기

- `Awake()`: 컴포넌트 초기화, Collider2D 참조 획득
- `FixedUpdate()`: 쿨다운 타이머 업데이트
- `OnDestroy()`: 모든 충돌 무시 상태 정리
