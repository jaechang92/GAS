# Unity LayerMask 레퍼런스

> **작성일**: 2025-11-15
> **목적**: LayerMask 관련 주요 API와 사용 패턴 정리

---

## 📋 목차

1. [LayerMask.NameToLayer vs LayerMask.GetMask](#1-layermasknamelayer-vs-layermaskgetmask)
2. [Layer와 LayerMask의 차이](#2-layer와-layermask의-차이)
3. [비트마스크 연산](#3-비트마스크-연산)
4. [실전 사용 예제](#4-실전-사용-예제)
5. [성능 고려사항](#5-성능-고려사항)
6. [자주하는 실수](#6-자주하는-실수)

---

## 1. LayerMask.NameToLayer vs LayerMask.GetMask

### 1.1 개념 차이

| 구분 | LayerMask.NameToLayer | LayerMask.GetMask |
|---|---|---|
| **반환 타입** | `int` (Layer Index) | `int` (LayerMask Bitmask) |
| **반환 값** | 0~31 (레이어 번호) | 비트마스크 (2의 거듭제곱) |
| **용도** | GameObject.layer 설정 | Physics Raycast/Overlap 필터링 |
| **매개변수** | 레이어 이름 1개 | 레이어 이름 여러 개 가능 |

### 1.2 LayerMask.NameToLayer

**함수 시그니처:**
```csharp
public static int NameToLayer(string layerName);
```

**설명:**
- 레이어 **이름**을 레이어 **번호**(인덱스)로 변환
- 0~31 범위의 정수 반환
- GameObject의 layer 속성 설정에 사용

**예제:**
```csharp
// "Player" 레이어의 번호를 가져옴 (예: 6)
int playerLayerIndex = LayerMask.NameToLayer("Player");
Debug.Log(playerLayerIndex); // 출력: 6

// GameObject의 레이어 설정
gameObject.layer = playerLayerIndex;
```

**반환 값 예시:**
```
"Default"  → 0
"Player"   → 6
"Enemy"    → 7
"Ground"   → 8
```

### 1.3 LayerMask.GetMask

**함수 시그니처:**
```csharp
public static int GetMask(params string[] layerNames);
```

**설명:**
- 레이어 **이름**들을 **비트마스크**로 변환
- 여러 레이어를 동시에 지정 가능
- Physics 연산(Raycast, OverlapSphere 등)의 필터링에 사용

**예제:**
```csharp
// "Player" 레이어만 포함 (비트마스크: 1 << 6 = 64)
int playerMask = LayerMask.GetMask("Player");
Debug.Log(playerMask); // 출력: 64

// "Player"와 "Enemy" 레이어 모두 포함
int combatMask = LayerMask.GetMask("Player", "Enemy");
Debug.Log(combatMask); // 출력: 192 (64 + 128)
```

**반환 값 예시:**
```
"Player" (Layer 6)  → 64      (2^6 = 0b00000000_01000000)
"Enemy"  (Layer 7)  → 128     (2^7 = 0b00000000_10000000)
"Player", "Enemy"   → 192     (64 + 128 = 0b00000000_11000000)
```

---

## 2. Layer와 LayerMask의 차이

### 2.1 Layer (레이어 인덱스)

**타입:** `int` (0~31)
**용도:** GameObject가 속한 레이어 지정

```csharp
// GameObject의 레이어 설정
gameObject.layer = 6; // Player 레이어
gameObject.layer = LayerMask.NameToLayer("Enemy"); // Enemy 레이어
```

**특징:**
- 하나의 GameObject는 **1개의 레이어만** 가질 수 있음
- 0~31 범위의 정수 (Unity는 32개 레이어 지원)

### 2.2 LayerMask (레이어 비트마스크)

**타입:** `int` (비트플래그)
**용도:** 여러 레이어를 동시에 표현 (필터링)

```csharp
// 여러 레이어를 동시에 지정
LayerMask mask = LayerMask.GetMask("Player", "Enemy", "Ground");

// Raycast에서 특정 레이어만 감지
Physics.Raycast(origin, direction, out hit, maxDistance, mask);
```

**특징:**
- 비트마스크로 **여러 레이어를 동시에** 표현
- 각 비트가 하나의 레이어를 나타냄

### 2.3 시각적 비교

```
Layer Index (0~31):
Player  = 6
Enemy   = 7
Ground  = 8

LayerMask (32-bit Bitmask):
비트 위치:  31 ... 8  7  6  5 ... 0
                     ↑  ↑  ↑
                     Ground Enemy Player

GetMask("Player"):         0b00000000_01000000 = 64
GetMask("Enemy"):          0b00000000_10000000 = 128
GetMask("Player","Enemy"): 0b00000000_11000000 = 192
```

---

## 3. 비트마스크 연산

### 3.1 기본 연산

```csharp
// Layer Index → LayerMask 변환
int layerIndex = 6; // Player
int layerMask = 1 << layerIndex; // 64 (비트 시프트)

// LayerMask 합치기 (OR 연산)
int playerMask = LayerMask.GetMask("Player");
int enemyMask = LayerMask.GetMask("Enemy");
int combinedMask = playerMask | enemyMask; // 192

// LayerMask에서 특정 레이어 제외 (NOT 연산)
int allExceptPlayer = ~LayerMask.GetMask("Player");

// LayerMask에 레이어 포함 여부 확인 (AND 연산)
int mask = LayerMask.GetMask("Player", "Enemy");
bool hasPlayer = (mask & (1 << LayerMask.NameToLayer("Player"))) != 0;
```

### 3.2 실전 예제

```csharp
public class RaycastExample : MonoBehaviour
{
    [Header("Layer Settings")]
    [SerializeField] private LayerMask targetLayers; // Inspector에서 설정

    void Update()
    {
        // 방법 1: Inspector에서 설정한 LayerMask 사용 (권장)
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, targetLayers))
        {
            Debug.Log($"Hit: {hit.collider.name}");
        }

        // 방법 2: 코드로 LayerMask 생성
        int mask = LayerMask.GetMask("Player", "Enemy");
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, mask))
        {
            Debug.Log($"Hit: {hit.collider.name}");
        }

        // 방법 3: 특정 레이어만 제외
        int allExceptGround = ~LayerMask.GetMask("Ground");
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, allExceptGround))
        {
            Debug.Log($"Not ground: {hit.collider.name}");
        }
    }
}
```

---

## 4. 실전 사용 예제

### 4.1 GameObject 레이어 설정

```csharp
// ❌ 잘못된 사용 (LayerMask를 layer에 할당)
gameObject.layer = LayerMask.GetMask("Player"); // 64 → 잘못됨!

// ✅ 올바른 사용 (Layer Index 사용)
gameObject.layer = LayerMask.NameToLayer("Player"); // 6 → 올바름!
```

### 4.2 Physics Raycast 필터링

```csharp
public class PlayerAttack : MonoBehaviour
{
    private int enemyLayerMask;

    private void Awake()
    {
        // 초기화 시 LayerMask 캐싱 (성능 최적화)
        enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    private void Attack()
    {
        // Enemy 레이어만 감지하는 Raycast
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, enemyLayerMask))
        {
            if (hit.collider.TryGetComponent<EnemyStats>(out var enemy))
            {
                enemy.TakeDamage(10);
            }
        }
    }
}
```

### 4.3 OverlapSphere로 범위 내 적 감지

```csharp
public class EliteEnemy : MonoBehaviour
{
    [Header("Area Attack Settings")]
    [SerializeField] private float attackRadius = 3.5f;

    private int playerLayerMask;

    private void Awake()
    {
        playerLayerMask = LayerMask.GetMask("Player");
    }

    private void ExecuteAreaAttack()
    {
        // Player 레이어만 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRadius,
            playerLayerMask // ✅ LayerMask 사용
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerStats>(out var player))
            {
                player.TakeDamage(attack * 2);
            }
        }
    }
}
```

### 4.4 충돌 매트릭스와 함께 사용

```csharp
public class Projectile : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private LayerMask targetLayers; // Inspector: Player, Enemy 선택

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 targetLayers에 포함되는지 확인
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            Debug.Log($"Hit target: {other.name}");
            // 데미지 처리...
        }
    }
}
```

---

## 5. 성능 고려사항

### 5.1 LayerMask 캐싱

```csharp
// ❌ 나쁜 예 - 매 프레임 GetMask 호출
void Update()
{
    int mask = LayerMask.GetMask("Enemy"); // 매번 문자열 검색!
    Physics.Raycast(transform.position, transform.forward, out hit, 10f, mask);
}

// ✅ 좋은 예 - 초기화 시 캐싱
private int enemyLayerMask;

void Awake()
{
    enemyLayerMask = LayerMask.GetMask("Enemy"); // 한 번만 호출
}

void Update()
{
    Physics.Raycast(transform.position, transform.forward, out hit, 10f, enemyLayerMask);
}
```

**이유:**
- `GetMask()`는 내부적으로 **문자열 비교** 수행
- 매 프레임 호출 시 불필요한 오버헤드
- 초기화 시 캐싱하면 **비트마스크 정수** 사용으로 빠름

### 5.2 Inspector 설정 활용

```csharp
public class EnemyDetector : MonoBehaviour
{
    // ✅ 가장 좋은 방법 - Inspector에서 LayerMask 직접 선택
    [SerializeField] private LayerMask targetLayers;

    void Update()
    {
        // 별도 변환 없이 바로 사용
        if (Physics2D.OverlapCircle(transform.position, 5f, targetLayers))
        {
            // ...
        }
    }
}
```

**장점:**
- 코드에서 문자열 사용 안 함 (오타 방지)
- 디자이너가 Unity Editor에서 직접 조정 가능
- 런타임 변환 오버헤드 없음

---

## 6. 자주하는 실수

### 6.1 Layer와 LayerMask 혼동

```csharp
// ❌ 잘못된 사용
int playerLayer = LayerMask.GetMask("Player"); // 64
gameObject.layer = playerLayer; // Layer 64는 존재하지 않음!

// ✅ 올바른 사용
int playerLayerIndex = LayerMask.NameToLayer("Player"); // 6
gameObject.layer = playerLayerIndex; // 올바름

int playerLayerMask = LayerMask.GetMask("Player"); // 64
Physics.Raycast(origin, direction, out hit, 10f, playerLayerMask); // 올바름
```

### 6.2 비트마스크 직접 계산 실수

```csharp
// ❌ 잘못된 계산
int playerLayer = 6;
int wrongMask = playerLayer; // 6 → 잘못됨!

// ✅ 올바른 계산
int playerLayer = 6;
int correctMask = 1 << playerLayer; // 64 → 올바름!

// ✅ 더 좋은 방법 - GetMask 사용
int bestMask = LayerMask.GetMask("Player"); // 64 → 가장 안전함!
```

### 6.3 문자열 오타

```csharp
// ❌ 오타 - 런타임 에러 발생
int mask = LayerMask.GetMask("Plaeyr"); // 존재하지 않는 레이어

// ✅ 올바른 사용
int mask = LayerMask.GetMask("Player");

// ✅ 더 안전한 방법 - 상수로 관리
public static class Layers
{
    public const string Player = "Player";
    public const string Enemy = "Enemy";
    public const string Ground = "Ground";
}

int mask = LayerMask.GetMask(Layers.Player); // 오타 방지
```

### 6.4 LayerMask 반전 실수

```csharp
// ❌ 잘못된 반전 (Layer Index 사용)
int playerLayer = LayerMask.NameToLayer("Player"); // 6
int wrongMask = ~playerLayer; // -7 → 의미 없음!

// ✅ 올바른 반전 (LayerMask 사용)
int playerMask = LayerMask.GetMask("Player"); // 64
int correctMask = ~playerMask; // Player 제외한 모든 레이어
```

---

## 7. GASPT 프로젝트에서의 사용 예시

### 7.1 EnemyProjectile (Phase C-1)

```csharp
// Assets/_Project/Scripts/Gameplay/Projectiles/EnemyProjectile.cs
public class EnemyProjectile : Projectile
{
    protected override void Awake()
    {
        base.Awake();

        // ✅ NameToLayer로 Player 레이어 번호 가져옴 (6)
        targetLayer = LayerMask.NameToLayer("Player");
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ GameObject.layer와 비교 (둘 다 int)
        if (other.gameObject.layer != targetLayer)
            return;

        // Player에게 데미지
        // ...
    }
}
```

### 7.2 EliteEnemy Area Attack (Phase C-1)

```csharp
// Assets/_Project/Scripts/Gameplay/Enemy/EliteEnemy.cs
public class EliteEnemy : PlatformerEnemy
{
    private int playerLayerMask;

    protected override void Awake()
    {
        base.Awake();

        // ✅ GetMask로 LayerMask 생성 (64)
        playerLayerMask = LayerMask.GetMask("Player");
    }

    private void ExecuteAreaAttack()
    {
        // ✅ OverlapCircleAll의 layerMask 매개변수에 사용
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            Data.areaAttackRadius,
            playerLayerMask // LayerMask 전달
        );

        foreach (var hit in hits)
        {
            // Player에게 2배 데미지
            // ...
        }
    }
}
```

---

## 8. 요약 테이블

### 8.1 언제 무엇을 사용할까?

| 상황 | 사용할 함수 | 반환 타입 | 용도 |
|---|---|---|---|
| GameObject 레이어 설정 | `NameToLayer()` | `int` (0~31) | `gameObject.layer = ...` |
| Raycast 필터링 | `GetMask()` | `int` (Bitmask) | `Physics.Raycast(..., mask)` |
| OverlapSphere 필터링 | `GetMask()` | `int` (Bitmask) | `Physics.OverlapSphere(..., mask)` |
| 여러 레이어 동시 지정 | `GetMask()` | `int` (Bitmask) | `GetMask("A", "B", "C")` |
| Inspector에서 설정 | N/A | `LayerMask` | `[SerializeField] LayerMask` |

### 8.2 빠른 참조

```csharp
// GameObject 레이어 설정
gameObject.layer = LayerMask.NameToLayer("Player"); // ✅

// Raycast 필터링
int mask = LayerMask.GetMask("Player", "Enemy"); // ✅
Physics.Raycast(origin, direction, out hit, 10f, mask);

// 레이어 제외
int allExceptGround = ~LayerMask.GetMask("Ground"); // ✅

// Inspector 설정 (가장 권장)
[SerializeField] private LayerMask targetLayers; // ✅
```

---

## 📚 참고 자료

- Unity Documentation: [LayerMask](https://docs.unity3d.com/ScriptReference/LayerMask.html)
- Unity Manual: [Layers](https://docs.unity3d.com/Manual/Layers.html)
- Unity Manual: [Physics.Raycast](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html)

---

## 📝 변경 이력

### 2025-11-15
- 초안 작성
- GASPT 프로젝트 예시 추가 (Phase C-1)

---

**작성자**: Claude Code & JaeChang
**버전**: 1.0
**마지막 수정**: 2025-11-15
