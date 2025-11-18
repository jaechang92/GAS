# 오류 해결 사례 모음 (포트폴리오)

**프로젝트**: GASPT (Generic Ability System + FSM)
**문서 작성일**: 2025-11-04
**목적**: 개발 과정에서 발생한 주요 오류와 해결 방법을 기록하여 문제 해결 능력 입증

---

## 목차
1. [OnManaChanged 이벤트 매개변수 불일치 오류](#1-onmanachanged-이벤트-매개변수-불일치-오류)
2. [OperationCanceledException 발생 오류](#2-operationcanceledexception-발생-오류)
3. [Awaitable과 CancellationToken 개념 및 사용법](#3-awaitable과-cancellationtoken-개념-및-사용법)
4. [BuffIcon ContinueWith 컴파일 에러](#4-bufficon-continuewith-컴파일-에러)
5. [ScriptableObject Serialization과 기본값 문제](#섹션-5-scriptableobject-serialization과-기본값-문제)
6. [오브젝트 풀링 시스템 구축 및 최적화](#6-오브젝트-풀링-시스템-구축-및-최적화)
7. [Unity EditorWindow GUI 레이아웃 오류](#7-unity-editorwindow-gui-레이아웃-오류)
8. [virtual vs override: 메서드 하이딩과 오버라이딩의 차이](#8-virtual-vs-override-메서드-하이딩과-오버라이딩의-차이)

---

## 1. OnManaChanged 이벤트 매개변수 불일치 오류

### 📋 오류 개요
- **발생 날짜**: 2025-11-04
- **작업 컨텍스트**: PlayerManaBar UI 구현 (Phase 12 확장)
- **관련 브랜치**: `010-mana-bar-ui`
- **관련 PR**: [#4 - Mana Bar UI 구현](https://github.com/jaechang92/GAS/pull/4)

### 🔴 오류 내용

#### 오류 메시지
```
CS1061: 'Action<int, int>' does not contain a definition for 'Invoke'
with 3 parameters and no accessible extension method accepting
a first argument of type 'Action<int, int>' could be found
```

#### 발생 상황
`PlayerManaBar.cs`에서 `PlayerStats.OnManaChanged` 이벤트를 구독할 때, 이벤트 핸들러의 매개변수 개수가 실제 이벤트 시그니처와 일치하지 않았습니다.

#### 문제가 된 코드
```csharp
// PlayerStats.cs - 실제 이벤트 정의
public event Action<int, int> OnManaChanged; // (currentMana, maxMana)

// PlayerManaBar.cs - 잘못된 핸들러 구현 ❌
private void OnPlayerManaChanged(int oldMana, int newMana, int maxMana)
{
    // 3개 매개변수 사용 - 이벤트는 2개만 제공!
    UpdateManaBar(newMana, maxMana);

    if (newMana < oldMana)
        FlashColor(spendColor);  // 마나 소모
    else if (newMana > oldMana)
        FlashColor(regenColor);  // 마나 회복
}
```

### 🔍 문제 분석

#### 근본 원인
1. **이벤트 시그니처 오해**: `PlayerStats.OnManaChanged`는 `Action<int, int>`로 정의되어 있어 **2개의 매개변수**만 전달합니다.
   - 첫 번째 매개변수: `currentMana` (현재 마나)
   - 두 번째 매개변수: `maxMana` (최대 마나)

2. **oldMana 필요성**: 마나 소모/회복을 구분하기 위해 **이전 마나 값**이 필요했지만, 이벤트는 이전 값을 제공하지 않습니다.

3. **다른 UI와의 차이점**: `PlayerHealthBar`와 `PlayerExpBar`는 각각 별도의 이벤트를 사용:
   - `OnDamaged` (데미지 받음)
   - `OnHealed` (회복)
   - `OnExpGained` (경험치 획득)

   하지만 `OnManaChanged`는 **단일 이벤트**로 모든 마나 변화를 처리해야 했습니다.

### ✅ 해결 방법

#### 해결 전략
이벤트가 이전 값을 제공하지 않으므로, **내부 상태로 이전 값을 추적**하는 방식으로 해결했습니다.

#### 수정된 코드
```csharp
// PlayerManaBar.cs

// 내부 필드로 이전 마나 값 추적
private int lastMana; // 이전 마나 값 (플래시 효과 판단용)

// 초기화 시 lastMana 설정
private void InitializeUI()
{
    if (playerStats != null)
    {
        lastMana = playerStats.CurrentMana; // ✅ 초기값 설정
        UpdateManaBar(playerStats.CurrentMana, playerStats.MaxMana);
    }
    // ...
}

// 올바른 이벤트 핸들러 ✅
private void OnPlayerManaChanged(int currentMana, int maxMana)
{
    UpdateManaBar(currentMana, maxMana);

    // lastMana와 비교하여 소모/회복 판단
    if (currentMana < lastMana)
    {
        // 마나 소모
        FlashColor(spendColor);
    }
    else if (currentMana > lastMana)
    {
        // 마나 회복
        FlashColor(regenColor);
    }

    // 현재 마나를 lastMana에 저장 ✅
    lastMana = currentMana;
}

// PlayerStats 참조가 변경될 때도 lastMana 업데이트
public void SetPlayerStats(PlayerStats stats)
{
    UnsubscribeFromEvents();
    playerStats = stats;
    SubscribeToEvents();

    if (playerStats != null)
    {
        lastMana = playerStats.CurrentMana; // ✅ 재설정
        UpdateManaBar(playerStats.CurrentMana, playerStats.MaxMana);
    }
}
```

### 📊 해결 결과

#### 커밋 정보
- **커밋 해시**: `b017f13`
- **커밋 메시지**: "수정: OnManaChanged 이벤트 매개변수 수정"
- **변경 파일**: `PlayerManaBar.cs`

#### 테스트 결과
```
✅ 마나 소모 시 빨간색 플래시 정상 작동
✅ 마나 회복 시 파란색 플래시 정상 작동
✅ 마나바 텍스트 업데이트 정상 작동
✅ 저마나 경고 (20% 이하) 정상 작동
```

### 💡 배운 점 및 개선 사항

#### 1. 이벤트 매개변수 설계 원칙
- **이벤트 설계 시 고려사항**:
  - 이전 값(old value)이 필요한 경우 매개변수에 포함할지 결정
  - 단일 이벤트 vs 분리된 이벤트 (OnChanged vs OnIncreased/OnDecreased)

- **권장 패턴**:
  ```csharp
  // 옵션 1: 이전 값 포함
  public event Action<int, int, int> OnManaChanged; // (oldMana, newMana, maxMana)

  // 옵션 2: 분리된 이벤트
  public event Action<int, int> OnManaSpent;  // (spent, currentMana)
  public event Action<int, int> OnManaRegen;  // (regen, currentMana)

  // 옵션 3: 현재 값만 + 내부 추적 (현재 구현) ✅
  public event Action<int, int> OnManaChanged; // (currentMana, maxMana)
  // → UI에서 lastValue 필드로 이전 값 추적
  ```

#### 2. 상태 추적 패턴 (State Tracking Pattern)
- **적용 가능한 상황**: 이벤트가 delta 정보를 제공하지 않을 때
- **구현 방법**: private 필드로 이전 상태 저장
- **주의사항**: 초기화 시점과 참조 변경 시점에 필드 업데이트 필수

#### 3. 재사용 가능성
이 패턴은 다른 UI 컴포넌트에서도 활용 가능:
```csharp
// 다른 예시: 골드 UI
private int lastGold;

private void OnGoldChanged(int currentGold)
{
    if (currentGold > lastGold)
        PlayGainAnimation();
    else if (currentGold < lastGold)
        PlayLossAnimation();

    lastGold = currentGold;
}
```

---

## 2. OperationCanceledException 발생 오류

### 📋 오류 개요
- **발생 날짜**: 2025-11-04
- **작업 컨텍스트**: HealthBar/ExpBar Awaitable 리팩토링
- **관련 브랜치**: `011-awaitable-refactor`
- **관련 PR**: [#5 - HealthBar/ExpBar Awaitable 리팩토링](https://github.com/jaechang92/GAS/pull/5)

### 🔴 오류 내용

#### 오류 메시지
```
OperationCanceledException: The operation was canceled
  at System.Threading.CancellationToken.ThrowOperationCanceledException()
  at UnityEngine.Awaitable.NextFrameAsync(CancellationToken cancellationToken)
  at GASPT.UI.PlayerHealthBar.FlashColorAsync(Color flashColor, CancellationToken ct)
```

#### 발생 상황
Coroutine을 Awaitable로 리팩토링하는 과정에서, `CancellationToken`을 사용한 비동기 작업 취소 시 예외가 콘솔에 출력되었습니다.

#### 문제가 된 코드
```csharp
// PlayerHealthBar.cs - 리팩토링 후 (예외 처리 없음) ❌

private CancellationTokenSource flashCts;

private async void FlashColor(Color flashColor)
{
    if (fillImage == null) return;

    // 이전 플래시 중단
    flashCts?.Cancel();  // ← 여기서 취소!
    flashCts = new CancellationTokenSource();

    // 예외 처리 없이 호출 ❌
    await FlashColorAsync(flashColor, flashCts.Token);
}

private async Awaitable FlashColorAsync(Color flashColor, CancellationToken ct)
{
    float elapsed = 0f;
    fillImage.color = flashColor;

    while (elapsed < flashDuration)
    {
        if (ct.IsCancellationRequested) return;

        elapsed += Time.deltaTime;
        float t = elapsed / flashDuration;
        fillImage.color = Color.Lerp(flashColor, currentNormalColor, t);

        // 여기서 OperationCanceledException 발생! ❌
        await Awaitable.NextFrameAsync(ct);
    }

    fillImage.color = currentNormalColor;
}
```

### 🔍 문제 분석

#### 근본 원인
1. **Awaitable의 취소 동작**: Unity의 `Awaitable.NextFrameAsync(CancellationToken)`는 토큰이 취소되면 **예외를 던집니다**.
   ```csharp
   // Unity 내부 동작 (의사 코드)
   public static async Awaitable NextFrameAsync(CancellationToken ct)
   {
       if (ct.IsCancellationRequested)
           throw new OperationCanceledException(ct); // ← 예외 발생!

       await NextFrame();
   }
   ```

2. **연속적인 애니메이션 실행**:
   - 플레이어가 연속으로 데미지를 받으면 → 이전 플래시가 취소되고 새 플래시 시작
   - 이전 플래시의 `CancellationToken`이 취소됨
   - `Awaitable.NextFrameAsync(ct)`가 `OperationCanceledException` 던짐

3. **async void의 예외 처리**:
   - `async void` 메서드에서 예외가 발생하면 콘솔에 출력됨
   - 게임은 정상 동작하지만 불필요한 에러 로그가 남음

#### 왜 try-catch가 필요한가?
- **취소는 정상적인 동작**: 새 애니메이션 시작 시 이전 애니메이션을 중단하는 것은 **의도된 동작**입니다.
- **예외는 제어 흐름이 아님**: 하지만 Unity의 Awaitable은 취소 시 예외를 던지므로, 이를 조용히 처리해야 합니다.

### ✅ 해결 방법

#### 해결 전략
`try-catch` 블록으로 `OperationCanceledException`을 조용히 처리하여, 취소가 정상적인 동작임을 명시합니다.

#### 수정된 코드

##### PlayerHealthBar.cs
```csharp
private async void FlashColor(Color flashColor)
{
    if (fillImage == null) return;

    // 이전 플래시 중단
    flashCts?.Cancel();
    flashCts = new CancellationTokenSource();

    try
    {
        await FlashColorAsync(flashColor, flashCts.Token);
    }
    catch (System.OperationCanceledException)
    {
        // 취소됨 - 정상적인 동작 ✅
        // 새 플래시가 시작되면 이전 플래시가 취소되는 것은 의도된 동작
    }
}

private async Awaitable FlashColorAsync(Color flashColor, CancellationToken ct)
{
    float elapsed = 0f;
    fillImage.color = flashColor;

    while (elapsed < flashDuration)
    {
        if (ct.IsCancellationRequested) return;

        elapsed += Time.deltaTime;
        float t = elapsed / flashDuration;
        fillImage.color = Color.Lerp(flashColor, currentNormalColor, t);

        await Awaitable.NextFrameAsync(ct); // ✅ 예외는 상위에서 처리
    }

    fillImage.color = currentNormalColor;
}
```

##### PlayerExpBar.cs
```csharp
private async void FlashColor(Color flashColor)
{
    if (fillImage == null) return;

    flashCts?.Cancel();
    flashCts = new CancellationTokenSource();

    try
    {
        await FlashColorAsync(flashColor, flashCts.Token);
    }
    catch (System.OperationCanceledException)
    {
        // 취소됨 - 정상적인 동작 ✅
    }
}

private async void PlayLevelUpAnimation()
{
    levelUpCts?.Cancel();
    levelUpCts = new CancellationTokenSource();

    try
    {
        await LevelUpAnimationAsync(levelUpCts.Token);
    }
    catch (System.OperationCanceledException)
    {
        // 취소됨 - 정상적인 동작 ✅
    }
}
```

##### PlayerManaBar.cs (일관성 유지)
```csharp
// PlayerManaBar.cs에도 동일한 패턴 적용 ✅
private async void FlashColor(Color flashColor)
{
    if (fillImage == null) return;

    flashCts?.Cancel();
    flashCts = new CancellationTokenSource();

    try
    {
        await FlashColorAsync(flashColor, flashCts.Token);
    }
    catch (System.OperationCanceledException)
    {
        // 취소됨 - 정상적인 동작 ✅
    }
}
```

### 📊 해결 결과

#### 커밋 정보
- **커밋 해시**: `da1b389`
- **커밋 메시지**: "수정: OperationCanceledException 처리 추가"
- **변경 파일**:
  - `PlayerHealthBar.cs`
  - `PlayerExpBar.cs`
  - `PlayerManaBar.cs`

#### 테스트 결과
```
✅ 연속 데미지 시 플래시 정상 작동 (예외 없음)
✅ 연속 회복 시 플래시 정상 작동 (예외 없음)
✅ 레벨업 애니메이션 중단 및 재시작 정상 작동
✅ 콘솔에 OperationCanceledException 출력 없음
```

### 💡 배운 점 및 개선 사항

#### 1. Awaitable과 CancellationToken 패턴

##### 표준 패턴
```csharp
private CancellationTokenSource cts;

// async void 진입점 (Unity 이벤트 핸들러나 버튼 클릭 등)
private async void StartOperation()
{
    // 이전 작업 취소
    cts?.Cancel();
    cts = new CancellationTokenSource();

    try
    {
        // CancellationToken을 전달하여 비동기 작업 실행
        await OperationAsync(cts.Token);
    }
    catch (OperationCanceledException)
    {
        // 취소는 정상적인 동작
        // 로그 없이 조용히 처리
    }
}

// async Awaitable 작업 메서드
private async Awaitable OperationAsync(CancellationToken ct)
{
    while (true)
    {
        // 주기적으로 취소 확인 (선택사항 - 성능 최적화)
        if (ct.IsCancellationRequested)
            return;

        // 비동기 작업
        await Awaitable.NextFrameAsync(ct); // 예외 발생 가능
    }
}

// 정리
private void OnDestroy()
{
    cts?.Cancel(); // ✅ 컴포넌트 파괴 시 진행 중인 작업 취소
}
```

#### 2. Coroutine vs Awaitable 비교

| 항목 | Coroutine | Awaitable (Unity 6.0+) |
|------|-----------|------------------------|
| 취소 방법 | `StopCoroutine()` | `CancellationToken.Cancel()` |
| 취소 시 동작 | 조용히 중단 | `OperationCanceledException` 발생 |
| 예외 처리 | 불필요 | **try-catch 필수** |
| 성능 | GC Allocation 발생 | GC-Free (더 효율적) |
| 타입 안전성 | 약함 (IEnumerator) | 강함 (Awaitable<T>) |
| 프로젝트 규칙 | ❌ 사용 금지 | ✅ 사용 권장 |

#### 3. 프로젝트 전체 일관성 확보

이 수정으로 **모든 UI 컴포넌트**가 동일한 패턴을 따르게 되었습니다:

```
✅ PlayerHealthBar: Awaitable + try-catch
✅ PlayerExpBar: Awaitable + try-catch
✅ PlayerManaBar: Awaitable + try-catch
✅ SkillSlotUI: Awaitable (쿨다운)
```

#### 4. Unity 6.0 Best Practice

Unity 6.0부터는 Awaitable이 표준 비동기 패턴으로 권장됩니다:

```csharp
// ❌ 피해야 할 패턴 (Unity 6.0 이전)
StartCoroutine(MyCoroutine());

// ✅ 권장 패턴 (Unity 6.0+)
private async void MyMethod()
{
    CancellationTokenSource cts = new();
    try
    {
        await MyOperationAsync(cts.Token);
    }
    catch (OperationCanceledException)
    {
        // 취소 처리
    }
}
```

#### 5. 재사용 가능한 유틸리티 (향후 개선 아이디어)

중복 코드를 줄이기 위한 헬퍼 메서드:

```csharp
// 향후 고려: Extensions/AwaitableExtensions.cs
public static class AwaitableExtensions
{
    /// <summary>
    /// OperationCanceledException을 조용히 처리하는 Awaitable 실행
    /// </summary>
    public static async void RunSilent(
        this Func<CancellationToken, Awaitable> operation,
        ref CancellationTokenSource cts)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        try
        {
            await operation(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // 조용히 처리
        }
    }
}

// 사용 예시
private CancellationTokenSource flashCts;

private void FlashColor(Color flashColor)
{
    ((CancellationToken ct) => FlashColorAsync(flashColor, ct))
        .RunSilent(ref flashCts);
}
```

---

## 📚 추가 참고 자료

### 관련 문서
- [Unity 6.0 Awaitable Documentation](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Awaitable.html)
- [C# CancellationToken Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)

### 프로젝트 내부 문서
- `WORK_STATUS.md` - 전체 프로젝트 진행 상황
- `RESOURCES_GUIDE.md` - 리소스 관리 가이드

### 관련 PR
- [PR #3](https://github.com/jaechang92/GAS/pull/3) - Skill System 구현
- [PR #4](https://github.com/jaechang92/GAS/pull/4) - Mana Bar UI 구현
- [PR #5](https://github.com/jaechang92/GAS/pull/5) - HealthBar/ExpBar Awaitable 리팩토링

---

## 🎯 결론

### 핵심 교훈
1. **이벤트 설계의 중요성**: 이벤트 시그니처를 설계할 때 구독자가 필요로 하는 정보를 신중히 고려해야 합니다.
2. **상태 추적 패턴**: 이벤트가 충분한 정보를 제공하지 않을 때는 내부 상태로 보완할 수 있습니다.
3. **비동기 예외 처리**: Awaitable과 CancellationToken을 사용할 때는 항상 `OperationCanceledException`을 처리해야 합니다.
4. **프로젝트 일관성**: 유사한 컴포넌트는 동일한 패턴을 따라야 유지보수가 용이합니다.

### 향후 적용
- 새로운 UI 컴포넌트 작성 시 이 패턴들을 템플릿으로 활용
- Awaitable 사용 시 항상 try-catch 패턴 적용
- 이벤트 설계 시 delta 정보 제공 여부 사전 결정

---

## 3. Awaitable과 CancellationToken 개념 및 사용법

### 📘 개념 정리
- **작성 날짜**: 2025-11-09
- **작업 컨텍스트**: BuffIcon UI 구현 (Phase 11 확장)
- **관련 브랜치**: `012-buff-icon-ui`
- **목적**: Unity 6.0의 Awaitable 패턴과 CancellationToken 사용법을 체계적으로 정리

---

### 🎯 Awaitable이란?

#### 정의
**Awaitable**은 Unity 6.0부터 도입된 **공식 비동기 프로그래밍 패턴**입니다. 기존 Coroutine을 대체하는 현대적인 방식으로, C#의 `async/await` 문법을 Unity에 최적화하여 제공합니다.

#### Coroutine과의 비교

| 항목 | Coroutine (구식) | Awaitable (신식) |
|------|------------------|------------------|
| **도입 버전** | Unity 초기부터 | Unity 6.0+ |
| **문법** | `yield return` | `async/await` |
| **타입** | `IEnumerator` | `Awaitable`, `Awaitable<T>` |
| **성능** | GC Allocation 발생 | **GC-Free** (메모리 효율적) |
| **타입 안전성** | 약함 (런타임 체크) | **강함** (컴파일 타임 체크) |
| **취소** | `StopCoroutine()` | `CancellationToken` |
| **반환값** | 불가능 | `Awaitable<T>`로 가능 |
| **예외 처리** | 어려움 | `try-catch` 사용 가능 |
| **프로젝트 규칙** | ❌ 사용 금지 | ✅ **필수 사용** |

#### 왜 Awaitable을 사용하는가?

**1. 성능 개선**
```csharp
// ❌ Coroutine - GC Allocation 발생
IEnumerator FadeOut()
{
    for (float t = 0; t < 1f; t += Time.deltaTime)
    {
        yield return null; // ← 매 프레임마다 IEnumerator 객체 생성
    }
}

// ✅ Awaitable - GC-Free
async Awaitable FadeOut()
{
    for (float t = 0; t < 1f; t += Time.deltaTime)
    {
        await Awaitable.NextFrameAsync(); // ← 메모리 할당 없음
    }
}
```

**2. 타입 안전성**
```csharp
// ❌ Coroutine - 타입 검증 불가
IEnumerator GetValue()
{
    yield return 42; // int를 반환했지만...
}

void Use()
{
    StartCoroutine(GetValue());
    // 반환값을 받을 수 없음!
}

// ✅ Awaitable - 강력한 타입
async Awaitable<int> GetValue()
{
    return 42; // ← 명확한 int 반환
}

async void Use()
{
    int value = await GetValue(); // ← 타입 안전하게 받음
}
```

**3. 예외 처리**
```csharp
// ❌ Coroutine - 예외 처리 어려움
IEnumerator DoSomething()
{
    // try-catch를 사용할 수 없음
    yield return SomeOperation();
}

// ✅ Awaitable - 표준 try-catch 사용
async Awaitable DoSomething()
{
    try
    {
        await SomeOperation();
    }
    catch (Exception ex)
    {
        Debug.LogError($"오류 발생: {ex.Message}");
    }
}
```

---

### 🛑 CancellationToken이란?

#### 정의
**CancellationToken**은 비동기 작업을 **안전하게 취소**하기 위한 .NET 표준 패턴입니다. "이 작업을 중단해주세요"라는 신호를 보내는 메커니즘입니다.

#### 기본 구조

```csharp
using System.Threading; // ← 필수 using

// 1. CancellationTokenSource 생성 (취소 신호 발생 장치)
CancellationTokenSource cts = new CancellationTokenSource();

// 2. Token을 비동기 작업에 전달
await SomeOperationAsync(cts.Token);

// 3. 취소 신호 발송
cts.Cancel(); // ← 작업 중단 요청

// 4. 정리
cts.Dispose();
```

#### 왜 CancellationToken이 필요한가?

**문제 상황**: 애니메이션 도중 새로운 애니메이션 시작
```csharp
// 상황: 플레이어가 연속으로 데미지를 받음
// 1초: 빨간색 플래시 시작 (3초 애니메이션)
// 2초: 또 데미지! 이전 플래시를 중단하고 새로 시작해야 함
```

**해결책**: CancellationToken으로 안전하게 취소
```csharp
private CancellationTokenSource flashCts;

private async void FlashColor(Color flashColor)
{
    // 이전 플래시 중단
    flashCts?.Cancel();  // ← "이전 작업 중단해!"
    flashCts = new CancellationTokenSource();

    try
    {
        await FlashColorAsync(flashColor, flashCts.Token);
    }
    catch (OperationCanceledException)
    {
        // 취소됨 - 정상 동작
    }
}

private async Awaitable FlashColorAsync(Color color, CancellationToken ct)
{
    float t = 0f;
    while (t < 3f)
    {
        // 취소 요청 확인
        if (ct.IsCancellationRequested)
            return; // ← 즉시 종료

        t += Time.deltaTime;
        await Awaitable.NextFrameAsync(ct); // ← 여기서 예외 발생 가능
    }
}
```

---

### 📐 기본 사용 패턴

#### 패턴 1: 단순 반복 작업
```csharp
// 예시: 매 프레임마다 회전
async Awaitable RotateAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        transform.Rotate(0, 0, 90f * Time.deltaTime);
        await Awaitable.NextFrameAsync(ct);
    }
}

// 사용
private CancellationTokenSource rotateCts;

private void Start()
{
    rotateCts = new CancellationTokenSource();
    RotateAsync(rotateCts.Token);
}

private void OnDestroy()
{
    rotateCts?.Cancel(); // ← 컴포넌트 파괴 시 중단
    rotateCts?.Dispose();
}
```

#### 패턴 2: 타이머 대기
```csharp
// 예시: 3초 대기 후 실행
async Awaitable WaitAndExecute()
{
    await Awaitable.WaitForSecondsAsync(3f);
    Debug.Log("3초 경과!");
}

// 사용
private async void Start()
{
    await WaitAndExecute();
}
```

#### 패턴 3: 조건 대기
```csharp
// 예시: 특정 조건이 될 때까지 대기
async Awaitable WaitUntilAsync(System.Func<bool> condition, CancellationToken ct)
{
    while (!condition())
    {
        if (ct.IsCancellationRequested)
            return;

        await Awaitable.NextFrameAsync(ct);
    }
}

// 사용
private async void Example()
{
    CancellationTokenSource cts = new();
    await WaitUntilAsync(() => player.HP <= 0, cts.Token);
    Debug.Log("플레이어 사망!");
}
```

#### 패턴 4: 반환값이 있는 비동기
```csharp
// 예시: 웹에서 데이터 로드
async Awaitable<string> LoadDataAsync()
{
    await Awaitable.WaitForSecondsAsync(2f); // 로딩 시뮬레이션
    return "데이터 로드 완료!";
}

// 사용
private async void Start()
{
    string data = await LoadDataAsync();
    Debug.Log(data); // "데이터 로드 완료!"
}
```

---

### ⚠️ 주의사항 및 베스트 프랙티스

#### 1. async void는 진입점에만 사용
```csharp
// ✅ 좋은 예: Unity 이벤트 핸들러는 async void
private async void Start()
{
    await LoadDataAsync();
}

private async void OnButtonClick()
{
    await SaveDataAsync();
}

// ❌ 나쁜 예: 일반 메서드는 async Awaitable
private async void DoSomething() // ← 예외 추적 어려움
{
    await SomeOperation();
}

// ✅ 개선: async Awaitable 사용
private async Awaitable DoSomething()
{
    await SomeOperation();
}
```

#### 2. 항상 OperationCanceledException 처리
```csharp
// ❌ 나쁜 예: 예외 처리 없음
private async void FlashColor()
{
    flashCts?.Cancel();
    flashCts = new CancellationTokenSource();
    await FlashAsync(flashCts.Token); // ← 예외 발생 시 콘솔 에러!
}

// ✅ 좋은 예: try-catch로 처리
private async void FlashColor()
{
    flashCts?.Cancel();
    flashCts = new CancellationTokenSource();

    try
    {
        await FlashAsync(flashCts.Token);
    }
    catch (OperationCanceledException)
    {
        // 취소는 정상 동작
    }
}
```

#### 3. OnDestroy에서 항상 취소
```csharp
// ✅ 좋은 예: 컴포넌트 파괴 시 정리
private CancellationTokenSource cts;

private void OnDestroy()
{
    cts?.Cancel();  // ← 진행 중인 작업 취소
    cts?.Dispose(); // ← 리소스 해제
}
```

#### 4. 여러 CancellationTokenSource 관리
```csharp
// 예시: BuffIcon에서 사용한 패턴
public class BuffIcon : MonoBehaviour
{
    private CancellationTokenSource updateCts; // 타이머 업데이트용

    private void StartUpdating()
    {
        StopUpdating(); // ← 이전 작업 취소
        updateCts = new CancellationTokenSource();
        StartUpdateTimerAsync(updateCts.Token);
    }

    private void StopUpdating()
    {
        if (updateCts != null)
        {
            updateCts.Cancel();
            updateCts.Dispose();
            updateCts = null; // ← null로 초기화
        }
    }

    private void OnDestroy()
    {
        StopUpdating(); // ← 컴포넌트 파괴 시 정리
    }
}
```

---

### 💻 프로젝트 실제 사용 사례

#### 사례 1: PlayerHealthBar - 플래시 애니메이션
**파일**: `Assets/_Project/Scripts/UI/PlayerHealthBar.cs`

```csharp
public class PlayerHealthBar : MonoBehaviour
{
    private CancellationTokenSource flashCts;

    // 데미지 받으면 빨간색 플래시
    private void OnPlayerDamaged(int damage)
    {
        FlashColor(damageColor);
    }

    // 회복하면 초록색 플래시
    private void OnPlayerHealed(int amount)
    {
        FlashColor(healColor);
    }

    private async void FlashColor(Color flashColor)
    {
        if (fillImage == null) return;

        // 이전 플래시 취소 (연속 데미지 대응)
        flashCts?.Cancel();
        flashCts = new CancellationTokenSource();

        try
        {
            await FlashColorAsync(flashColor, flashCts.Token);
        }
        catch (OperationCanceledException)
        {
            // 취소됨 - 새 플래시가 시작됨
        }
    }

    private async Awaitable FlashColorAsync(Color flashColor, CancellationToken ct)
    {
        float elapsed = 0f;
        fillImage.color = flashColor;

        while (elapsed < flashDuration)
        {
            if (ct.IsCancellationRequested)
                return;

            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            fillImage.color = Color.Lerp(flashColor, normalColor, t);

            await Awaitable.NextFrameAsync(ct);
        }

        fillImage.color = normalColor;
    }

    private void OnDestroy()
    {
        flashCts?.Cancel();
        flashCts?.Dispose();
    }
}
```

#### 사례 2: BuffIcon - 타이머 업데이트
**파일**: `Assets/_Project/Scripts/UI/BuffIcon.cs`

```csharp
public class BuffIcon : MonoBehaviour
{
    private CancellationTokenSource updateCts;
    private StatusEffect currentEffect;

    // 버프 아이콘 표시 시작
    public void Show(StatusEffect effect, Sprite icon, bool isBuff)
    {
        currentEffect = effect;
        iconImage.sprite = icon;

        StartUpdating(); // ← 타이머 시작
    }

    // 비동기 타이머 시작
    private void StartUpdating()
    {
        StopUpdating(); // 이전 타이머 중단

        updateCts = new CancellationTokenSource();
        StartUpdateTimerAsync(updateCts.Token);
    }

    // fire-and-forget 패턴 (async void)
    private async void StartUpdateTimerAsync(CancellationToken ct)
    {
        await UpdateTimerAsync(ct);
    }

    // 실제 타이머 로직
    private async Awaitable UpdateTimerAsync(CancellationToken ct)
    {
        try
        {
            while (currentEffect != null && !ct.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(ct);

                if (currentEffect == null)
                    break;

                // 타이머 UI 업데이트
                float ratio = currentEffect.RemainingTime / currentEffect.Duration;
                timerFillImage.fillAmount = ratio;
                timeText.text = currentEffect.RemainingTime.ToString("F1") + "s";
            }
        }
        catch (OperationCanceledException)
        {
            // 취소됨 - 정상 동작
        }
    }

    // 타이머 중단
    private void StopUpdating()
    {
        if (updateCts != null)
        {
            updateCts.Cancel();
            updateCts.Dispose();
            updateCts = null;
        }
    }

    public void Hide()
    {
        currentEffect = null;
        StopUpdating(); // ← 타이머 중단
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        StopUpdating();
    }
}
```

#### 사례 3: SkillSlotUI - 쿨다운 애니메이션
**파일**: `Assets/_Project/Scripts/UI/SkillSlotUI.cs`

```csharp
public class SkillSlotUI : MonoBehaviour
{
    private CancellationTokenSource cooldownCts;

    // 스킬 사용 시 쿨다운 시작
    private void OnSkillUsed(int slotIndex)
    {
        if (slotIndex == this.slotIndex)
        {
            StartCooldownAnimation();
        }
    }

    private async void StartCooldownAnimation()
    {
        cooldownCts?.Cancel();
        cooldownCts = new CancellationTokenSource();

        try
        {
            await CooldownAnimationAsync(cooldownCts.Token);
        }
        catch (OperationCanceledException)
        {
            // 취소됨
        }
    }

    private async Awaitable CooldownAnimationAsync(CancellationToken ct)
    {
        while (true)
        {
            if (ct.IsCancellationRequested)
                return;

            // SkillSystem에서 쿨다운 비율 가져오기
            float ratio = SkillSystem.Instance.GetCooldownRatio(slotIndex);

            // UI 업데이트
            cooldownOverlay.fillAmount = ratio;
            cooldownText.text = (ratio * cooldown).ToString("F1");

            if (ratio >= 1f)
                break; // 쿨다운 완료

            await Awaitable.NextFrameAsync(ct);
        }

        // 쿨다운 완료
        cooldownOverlay.fillAmount = 0f;
        cooldownText.text = "";
    }

    private void OnDestroy()
    {
        cooldownCts?.Cancel();
        cooldownCts?.Dispose();
    }
}
```

---

### 🔧 자주 발생하는 에러와 해결법

#### 에러 1: CS1061 - ContinueWith가 없음
```csharp
// ❌ 에러 발생
UpdateTimerAsync(ct).ContinueWith(() => { });

// 원인: Awaitable에는 ContinueWith 메서드가 없음 (Task API임)

// ✅ 해결: async void 래퍼 사용
private async void StartTimer(CancellationToken ct)
{
    await UpdateTimerAsync(ct);
}
```

#### 에러 2: CS4014 - await 없이 호출
```csharp
// ❌ 경고 발생
private void DoSomething()
{
    SomeOperationAsync(); // ← await 없음!
}

// ✅ 해결 1: await 추가
private async void DoSomething()
{
    await SomeOperationAsync();
}

// ✅ 해결 2: fire-and-forget 명시
private void DoSomething()
{
    _ = SomeOperationAsync(); // ← 의도적으로 무시
}
```

#### 에러 3: InvalidOperationException - OnDestroy 후 Awaitable 실행
```csharp
// ❌ 문제 상황
private async void LongOperation()
{
    await Awaitable.WaitForSecondsAsync(10f);
    transform.position = Vector3.zero; // ← 컴포넌트가 이미 파괴됨!
}

// ✅ 해결: CancellationToken으로 조기 종료
private CancellationTokenSource cts;

private async void LongOperation()
{
    cts = new CancellationTokenSource();
    try
    {
        await Awaitable.WaitForSecondsAsync(10f, cts.Token);
        transform.position = Vector3.zero; // ← 안전
    }
    catch (OperationCanceledException)
    {
        // 취소됨
    }
}

private void OnDestroy()
{
    cts?.Cancel(); // ← 진행 중인 작업 중단
}
```

---

### 📊 프로젝트 전체 Awaitable 사용 현황

| 컴포넌트 | 파일 | 용도 | CancellationToken 사용 |
|----------|------|------|----------------------|
| PlayerHealthBar | UI/PlayerHealthBar.cs | 플래시 애니메이션 | ✅ flashCts |
| PlayerExpBar | UI/PlayerExpBar.cs | 플래시 + 레벨업 애니메이션 | ✅ flashCts, levelUpCts |
| PlayerManaBar | UI/PlayerManaBar.cs | 플래시 애니메이션 | ✅ flashCts |
| SkillSlotUI | UI/SkillSlotUI.cs | 쿨다운 애니메이션 | ✅ cooldownCts |
| BuffIcon | UI/BuffIcon.cs | 타이머 업데이트 | ✅ updateCts |
| Skill | Skills/Skill.cs | 쿨다운 타이머 | ✅ cooldownCts |

**총 6개 컴포넌트**에서 **Coroutine 0개, Awaitable 100%** 사용 ✅

---

### 🎓 학습 체크리스트

#### 기본 개념
- [ ] Awaitable이 무엇인지 설명할 수 있다
- [ ] Awaitable과 Coroutine의 차이를 3가지 이상 말할 수 있다
- [ ] CancellationToken의 역할을 설명할 수 있다

#### 코드 작성
- [ ] async void와 async Awaitable의 차이를 안다
- [ ] CancellationTokenSource를 생성하고 Cancel할 수 있다
- [ ] OperationCanceledException을 올바르게 처리할 수 있다

#### 프로젝트 적용
- [ ] 새로운 UI 애니메이션을 Awaitable로 작성할 수 있다
- [ ] 컴포넌트 파괴 시 CancellationToken으로 정리할 수 있다
- [ ] fire-and-forget 패턴을 적절히 사용할 수 있다

---

### 📚 추가 학습 자료

#### 공식 문서
- [Unity Awaitable API Reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Awaitable.html)
- [C# async/await 가이드](https://learn.microsoft.com/ko-kr/dotnet/csharp/asynchronous-programming/)
- [CancellationToken 베스트 프랙티스](https://learn.microsoft.com/ko-kr/dotnet/standard/threading/cancellation-in-managed-threads)

#### 관련 Unity 포럼
- [Unity 6.0 Awaitable 소개 (공식 블로그)](https://blog.unity.com/technology/unity-6-preview-awaitable)
- [Coroutine에서 Awaitable로 마이그레이션](https://discussions.unity.com/t/migrating-from-coroutines-to-awaitables)

---

## 4. BuffIcon ContinueWith 컴파일 에러

### 📋 오류 개요
- **발생 날짜**: 2025-11-09
- **작업 컨텍스트**: BuffIcon UI 구현
- **관련 브랜치**: `012-buff-icon-ui`
- **오류 코드**: CS1061

### 🔴 오류 내용

#### 오류 메시지
```
CS1061: 'Awaitable' does not contain a definition for 'ContinueWith'
and no accessible extension method 'ContinueWith' accepting a first
argument of type 'Awaitable' could be found
```

#### 발생 상황
BuffIcon.cs에서 비동기 타이머를 시작할 때, `Awaitable`에 `ContinueWith` 메서드가 없어서 발생한 컴파일 에러입니다.

#### 문제가 된 코드
```csharp
// BuffIcon.cs:121 - 잘못된 코드 ❌
private void StartUpdating()
{
    StopUpdating();
    updateCts = new CancellationTokenSource();
    UpdateTimerAsync(updateCts.Token).ContinueWith(() => { }); // ← 에러!
}
```

### 🔍 문제 분석

#### 근본 원인
1. **API 차이**: `ContinueWith`는 `Task` API의 메서드이며, `Awaitable`에는 존재하지 않습니다.

   ```csharp
   // System.Threading.Tasks.Task (C# 표준)
   Task.Run(() => { }).ContinueWith(t => { }); // ✅ 가능

   // UnityEngine.Awaitable (Unity 6.0)
   Awaitable.NextFrameAsync().ContinueWith(() => { }); // ❌ 불가능
   ```

2. **의도**: fire-and-forget 패턴으로 비동기 작업을 시작하고 결과를 기다리지 않으려 했습니다.

3. **올바른 방법**: Unity의 Awaitable은 `async void` 래퍼 메서드를 사용해야 합니다.

### ✅ 해결 방법

#### 해결 전략
`async void` 메서드로 래핑하여 fire-and-forget 패턴을 구현합니다.

#### 수정된 코드
```csharp
// BuffIcon.cs - 수정 후 ✅

private void StartUpdating()
{
    StopUpdating();
    updateCts = new CancellationTokenSource();
    StartUpdateTimerAsync(updateCts.Token); // ← async void 호출
}

/// <summary>
/// 비동기 타이머 시작 (fire-and-forget)
/// </summary>
private async void StartUpdateTimerAsync(CancellationToken ct)
{
    await UpdateTimerAsync(ct); // ← 실제 Awaitable 메서드 호출
}

private async Awaitable UpdateTimerAsync(CancellationToken ct)
{
    try
    {
        while (currentEffect != null && !ct.IsCancellationRequested)
        {
            await Awaitable.NextFrameAsync(ct);
            // 타이머 UI 업데이트...
        }
    }
    catch (OperationCanceledException)
    {
        // 취소됨 - 정상 동작
    }
}
```

### 📊 해결 결과

#### 커밋 정보
- **커밋 해시**: `ee20a27`
- **커밋 메시지**: "수정: BuffIcon ContinueWith 에러 수정 (CS1061)"
- **변경 파일**: `Assets/_Project/Scripts/UI/BuffIcon.cs`

#### 패턴 비교

| 패턴 | Task API | Awaitable API |
|------|----------|---------------|
| **Fire-and-forget** | `.ContinueWith()` | `async void` 래퍼 |
| **예외 처리** | `.ContinueWith(TaskContinuationOptions)` | `try-catch` in async void |
| **코드 복잡도** | 중간 | **낮음** (더 직관적) |

### 💡 배운 점

#### 1. Task vs Awaitable API 차이 인식
- **Task**: .NET 표준 비동기 API
- **Awaitable**: Unity 전용 경량 비동기 API
- 두 API는 메서드가 다르므로 호환되지 않음

#### 2. fire-and-forget 올바른 패턴
```csharp
// ✅ Unity Awaitable 권장 패턴
private void StartAsyncOperation()
{
    StartOperationAsync(); // async void 호출
}

private async void StartOperationAsync()
{
    try
    {
        await DoSomethingAsync();
    }
    catch (Exception ex)
    {
        Debug.LogError($"오류: {ex.Message}");
    }
}
```

#### 3. async void 사용 규칙
**사용해야 할 때**:
- Unity 이벤트 핸들러 (`Start`, `OnClick` 등)
- fire-and-forget 진입점 메서드

**사용하지 말아야 할 때**:
- 반환값이 필요한 경우 → `async Awaitable<T>` 사용
- 호출자가 완료를 기다려야 하는 경우 → `async Awaitable` 사용

---

## 섹션 5: ScriptableObject Serialization과 기본값 문제

### 📌 발생 상황

**날짜**: 2025-11-09
**브랜치**: 013-item-drop-loot
**파일**: LootEntry.cs, LootTable.cs

**증상**:
```csharp
// LootEntry.cs
[Serializable]
public class LootEntry
{
    public int minQuantity = 1;  // 기본값 1
    public int maxQuantity = 1;  // 기본값 1
}

// LootTable.cs - ValidateTable()
if (entry.minQuantity < 1 || entry.maxQuantity < minQuantity)
{
    // 여기서 경고 발생!
    Debug.LogWarning($"수량 범위가 올바르지 않습니다 (min: {minQuantity}, max: {maxQuantity})");
    // 출력: min: 0, max: 0  ← 기본값 1이 아닌 0!
}
```

**Inspector 표시**:
- Min Quantity: 슬라이더가 1로 보임
- Max Quantity: 슬라이더가 1로 보임

**실제 저장된 값**:
- minQuantity: 0
- maxQuantity: 0

**❓ 의문점**: 코드에서 기본값을 1로 설정했는데 왜 0인가?

---

### 🔍 근본 원인: Unity Serialization 시스템

#### 1. Unity Serialization이란?

Unity는 게임 오브젝트, 컴포넌트, ScriptableObject의 데이터를 **YAML 형식**으로 저장합니다.

**Serialization**: C# 객체 → YAML 파일
**Deserialization**: YAML 파일 → C# 객체

```yaml
# LootTable.asset 파일 내용
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_Script: {fileID: 11500000, guid: ...}
  m_Name: TEST_LootTable
  lootEntries:
  - item: {fileID: ...}
    dropChance: 0.3
    minQuantity: 0      # ← 여기에 0으로 저장됨!
    maxQuantity: 0      # ← 여기에 0으로 저장됨!
```

#### 2. 기본값이 적용되는 시점

```csharp
public class LootEntry
{
    public int minQuantity = 1;  // ← 이 값은 언제 적용될까?
}
```

**기본값이 적용되는 경우**:
1. ✅ **new LootEntry()** 생성자 호출 시
2. ✅ **C# 코드에서 직접 생성** 시
   ```csharp
   LootEntry entry = new LootEntry();
   Debug.Log(entry.minQuantity);  // 1 출력 ✅
   ```

**기본값이 적용되지 않는 경우**:
1. ❌ **Unity Serialization을 통한 Deserialization** 시
2. ❌ **Inspector에서 값 변경** 후
3. ❌ **이미 저장된 ScriptableObject** 로드 시

#### 3. Unity Serialization 프로세스

```
[Inspector에서 Element 추가]
         ↓
[Unity가 새 LootEntry 슬롯 생성]
         ↓
[YAML에 기본값(0) 저장]  ← C# 기본값 무시!
         ↓
[파일에 저장됨]
         ↓
[다음 로드 시]
         ↓
[YAML에서 값 읽어옴]
         ↓
minQuantity = 0 (YAML 값)
maxQuantity = 0 (YAML 값)
```

**핵심**: Unity는 **YAML에 저장된 값**을 우선시하며, **C# 기본값은 무시**합니다!

---

### 📖 Unity Serialization 상세 분석

#### 1. Serialization의 4가지 규칙

**규칙 1**: **이미 Serialize된 필드는 기본값을 무시**
```csharp
// 처음 생성 시
public int value = 10;  // YAML에 value: 5 저장됨

// 나중에 코드 수정
public int value = 100;  // ← 기존 에셋에는 적용 안됨! 여전히 5

// 기존 에셋: value = 5 (YAML 값)
// 새 에셋: value = 100 (기본값)
```

**규칙 2**: **Serialize되지 않은 필드는 항상 기본값**
```csharp
[NonSerialized] public int temp = 100;
// 항상 100 (저장 안됨)
```

**규칙 3**: **생성자는 Deserialization 시 호출되지 않음**
```csharp
[Serializable]
public class Data
{
    public int value;

    public Data()
    {
        value = 100;  // ← Deserialization 시 실행 안됨!
    }
}
```

**규칙 4**: **필드 기본값만 가능, 프로퍼티 기본값은 불가능**
```csharp
public int value = 10;  // ✅ 가능 (필드 초기화)
public int Value { get; set; } = 10;  // ❌ Serialize 안됨
```

#### 2. ScriptableObject의 Serialization 타이밍

```
[ScriptableObject 생성]
         ↓
CreateInstance<T>() 호출
         ↓
기본값 적용 (필드 초기화)
         ↓
OnEnable() 호출
         ↓
첫 저장 시점
         ↓
[YAML 파일 생성] ← 이 시점의 값이 저장됨!
         ↓
[이후 로드]
         ↓
YAML 값으로 필드 덮어씀 (기본값 무시)
         ↓
OnEnable() 호출
         ↓
OnValidate() 호출 (Editor only)
```

---

### 🐛 실제 프로젝트 사례 분석

#### 사례: LootEntry 수량 문제

**1단계: 초기 코드 작성**
```csharp
[Serializable]
public class LootEntry
{
    [Range(1, 99)] public int minQuantity = 1;
    [Range(1, 99)] public int maxQuantity = 1;
}
```

**2단계: LootTable 생성**
```csharp
// Unity Editor
// Create > GASPT > Loot > LootTable
```

**3단계: Inspector에서 Element 추가**
```
Loot Entries:
  Size: 1
  Element 0:
    ├─ Item: (드래그 & 드롭)
    ├─ Drop Chance: 0.3
    ├─ Min Quantity: [슬라이더 1]  ← 보기에는 1
    └─ Max Quantity: [슬라이더 1]  ← 보기에는 1
```

**4단계: YAML 파일 확인**
```yaml
# TEST_LootTable.asset
lootEntries:
- item: {fileID: ...}
  dropChance: 0.3
  minQuantity: 0  # ← 실제 저장값은 0!
  maxQuantity: 0  # ← 실제 저장값은 0!
```

**❓ 왜 0인가?**

Unity가 새 Element를 생성할 때:
1. C# 생성자를 호출하지 않음
2. 필드 초기화 구문을 실행하지 않음
3. **모든 int 필드를 0으로 초기화** (C# 기본 동작)
4. YAML에 0을 저장

**5단계: 검증 코드 실행**
```csharp
// LootTable.ValidateTable()
if (entry.minQuantity < 1 || entry.maxQuantity < minQuantity)
{
    Debug.LogWarning($"수량 범위가 올바르지 않습니다 (min: {entry.minQuantity}, max: {entry.maxQuantity})");
    // 출력: min: 0, max: 0 ← 경고 발생!
}
```

---

### ✅ 해결 방법 4가지

#### 방법 1: OnValidate()에서 자동 보정 (권장)

```csharp
// LootTable.cs
private void OnValidate()
{
    FixLootEntries();  // 자동 보정
    ValidateTable();   // 검증
}

private void FixLootEntries()
{
    foreach (var entry in lootEntries)
    {
        if (entry.minQuantity < 1)
            entry.minQuantity = 1;  // 0이면 1로 수정

        if (entry.maxQuantity < 1)
            entry.maxQuantity = 1;  // 0이면 1로 수정
    }
}
```

**장점**:
- ✅ Inspector에서 값 변경 시 자동 보정
- ✅ 기존 에셋도 자동 수정됨
- ✅ 사용자가 신경 쓸 필요 없음

**실행 시점**:
- Inspector에서 값 변경 시
- ScriptableObject Reimport 시
- Unity 재시작 시

#### 방법 2: 생성자 대신 팩토리 메서드

```csharp
[Serializable]
public class LootEntry
{
    public int minQuantity;
    public int maxQuantity;

    // 기본 생성자 (Serialization용)
    public LootEntry() { }

    // 팩토리 메서드
    public static LootEntry Create(Item item, float dropChance)
    {
        return new LootEntry
        {
            item = item,
            dropChance = dropChance,
            minQuantity = 1,  // 명시적 설정
            maxQuantity = 1   // 명시적 설정
        };
    }
}
```

**사용**:
```csharp
// ❌ 직접 생성 금지
var entry = new LootEntry();  // minQuantity = 0

// ✅ 팩토리 메서드 사용
var entry = LootEntry.Create(item, 0.3f);  // minQuantity = 1
```

#### 방법 3: ISerializationCallbackReceiver

```csharp
[Serializable]
public class LootEntry : ISerializationCallbackReceiver
{
    public int minQuantity = 1;
    public int maxQuantity = 1;

    // Deserialization 후 호출됨
    public void OnAfterDeserialize()
    {
        if (minQuantity < 1) minQuantity = 1;
        if (maxQuantity < 1) maxQuantity = 1;
    }

    public void OnBeforeSerialize() { }
}
```

**장점**:
- ✅ Deserialization 직후 자동 보정
- ✅ LootEntry 자체에서 해결

**단점**:
- ❌ 클래스가 복잡해짐
- ❌ Inspector에서 즉시 반영 안됨 (재로드 필요)

#### 방법 4: Custom PropertyDrawer (고급)

```csharp
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LootEntry))]
public class LootEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var minProp = property.FindPropertyRelative("minQuantity");
        var maxProp = property.FindPropertyRelative("maxQuantity");

        // 값이 0이면 자동으로 1로 수정
        if (minProp.intValue < 1)
            minProp.intValue = 1;
        if (maxProp.intValue < 1)
            maxProp.intValue = 1;

        // GUI 그리기
        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif
```

**장점**:
- ✅ Inspector 렌더링 시 자동 보정
- ✅ 실시간 반영

**단점**:
- ❌ 코드 복잡도 증가
- ❌ Editor 전용

---

### 📊 방법 비교

| 방법 | 난이도 | 효과 | 추천도 |
|------|--------|------|--------|
| OnValidate() | ⭐ 쉬움 | ⭐⭐⭐ 높음 | ✅ 권장 |
| 팩토리 메서드 | ⭐⭐ 보통 | ⭐⭐ 보통 | 🔶 경우에 따라 |
| ISerializationCallbackReceiver | ⭐⭐⭐ 어려움 | ⭐⭐ 보통 | ⚠️ 필요시만 |
| Custom PropertyDrawer | ⭐⭐⭐⭐ 매우 어려움 | ⭐⭐⭐ 높음 | ⚠️ 고급 사용자 |

---

### 🎯 베스트 프랙티스

#### 1. ScriptableObject 설계 시

**DO ✅**:
```csharp
// OnValidate()로 자동 보정
private void OnValidate()
{
    // 유효하지 않은 값 자동 수정
    if (health < 0) health = 0;
    if (maxHealth < 1) maxHealth = 100;
    if (health > maxHealth) health = maxHealth;
}
```

**DON'T ❌**:
```csharp
// 생성자에 의존 (작동 안함!)
public MyData()
{
    health = 100;  // ← Deserialization 시 무시됨!
}
```

#### 2. [Serializable] 클래스 설계 시

**DO ✅**:
```csharp
// 명시적 초기화 + OnValidate() 보정
[Serializable]
public class Entry
{
    public int value = 10;  // 기본값
}

// 상위 클래스에서
private void OnValidate()
{
    foreach (var entry in entries)
        if (entry.value < 1)
            entry.value = 10;  // 보정
}
```

**DON'T ❌**:
```csharp
// 기본값만 믿고 검증 안함
[Serializable]
public class Entry
{
    public int value = 10;  // 실제로는 0일 수 있음!
}
```

#### 3. Inspector Range 사용 시

**주의**: Range는 **표시 범위**일 뿐, **저장값을 제한하지 않음**!

```csharp
[Range(1, 99)]
public int quantity = 1;

// Inspector: 슬라이더가 1~99 범위로 보임
// 실제 저장값: 0일 수 있음! (기존 에셋)
// 새 값 입력 시: 1~99로 제한됨
```

**올바른 사용**:
```csharp
[Range(1, 99)]
public int quantity = 1;

private void OnValidate()
{
    // Range를 믿지 말고 검증!
    quantity = Mathf.Clamp(quantity, 1, 99);
}
```

---

### 🔬 디버깅 팁

#### 1. YAML 파일 직접 확인

```bash
# .asset 파일은 텍스트 에디터로 열기 가능
# Assets/Resources/Data/TEST_LootTable.asset

%YAML 1.1
lootEntries:
- item: {fileID: ...}
  minQuantity: 0  # ← 실제 저장값 확인!
```

#### 2. Serialization 로그 찍기

```csharp
[Serializable]
public class LootEntry : ISerializationCallbackReceiver
{
    public void OnAfterDeserialize()
    {
        Debug.Log($"Deserialized: min={minQuantity}, max={maxQuantity}");
    }

    public void OnBeforeSerialize()
    {
        Debug.Log($"Serializing: min={minQuantity}, max={maxQuantity}");
    }
}
```

#### 3. OnValidate() 로그

```csharp
private void OnValidate()
{
    Debug.Log("OnValidate() 호출됨");

    foreach (var entry in lootEntries)
    {
        Debug.Log($"Entry: min={entry.minQuantity}, max={entry.maxQuantity}");
    }
}
```

---

### 📚 학습 체크리스트

- [ ] Unity Serialization이 무엇인지 이해함
- [ ] 기본값이 적용되는 시점과 되지 않는 시점을 구분할 수 있음
- [ ] YAML 파일의 구조를 이해함
- [ ] OnValidate()의 역할과 실행 시점을 알고 있음
- [ ] ISerializationCallbackReceiver의 용도를 이해함
- [ ] ScriptableObject 설계 시 주의사항을 숙지함
- [ ] Inspector Range와 실제 저장값의 차이를 이해함

---

### 🔗 관련 Unity 문서

- [Script Serialization](https://docs.unity3d.com/Manual/script-Serialization.html)
- [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [ISerializationCallbackReceiver](https://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.html)
- [SerializeField](https://docs.unity3d.com/ScriptReference/SerializeField.html)

---

### 💡 핵심 요약

1. **Unity는 YAML에 저장된 값을 우선시함**
   - C# 기본값은 최초 생성 시에만 사용됨
   - 이미 저장된 값은 기본값 변경해도 적용 안됨

2. **생성자는 Deserialization 시 호출 안됨**
   - 필드 초기화 구문도 실행 안됨
   - 모든 값은 YAML에서 복원됨

3. **OnValidate()로 자동 보정이 최선**
   - Inspector 변경 시 자동 실행
   - 기존 에셋도 자동 수정 가능

4. **Range는 표시용일 뿐, 검증은 별도로 필요**
   - 슬라이더 범위 ≠ 저장값 범위
   - OnValidate()에서 Clamp 필수

---

## 6. 오브젝트 풀링 시스템 구축 및 최적화

### 📋 프로젝트 개요
- **작업 날짜**: 2025-11-10
- **작업 컨텍스트**: 게임 최적화 - 메모리 및 성능 개선
- **관련 브랜치**: `013-item-drop-loot`
- **목적**: Instantiate/Destroy 비용 절감 및 GC 압박 감소

---

### 🎯 오브젝트 풀링 시스템을 만든 이유

#### 1. 성능 문제 인식

게임플레이 중 다음과 같은 성능 이슈가 발생했습니다:

**문제 상황**:
```csharp
// 기존 코드 - 매번 새로 생성 및 파괴
public async Task LaunchFireball()
{
    GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    // ... 투사체 이동 ...
    Destroy(fireball); // ← GC 압박!
}

// 플레이어가 스킬 연타 시
// → 초당 5~10개 GameObject 생성/파괴
// → 프레임 드롭 및 GC 스파이크 발생
```

**성능 측정 결과** (예상):
- **메모리 할당**: 초당 ~500KB (투사체 + Trail + Collider)
- **GC 빈도**: 3~5초마다 50~100ms 멈춤
- **프레임 드롭**: 60 FPS → 40 FPS (전투 시)

#### 2. 재사용 가능한 오브젝트 식별

프로젝트에서 빈번하게 생성/파괴되는 오브젝트:

| 오브젝트 타입 | 생성 빈도 | 생존 시간 | 풀링 필요도 |
|--------------|----------|----------|------------|
| **FireBall** | 초당 1~2회 | 2~3초 | ⭐⭐⭐⭐⭐ 매우 높음 |
| **MagicMissile** | 초당 2~5회 | 1~2초 | ⭐⭐⭐⭐⭐ 매우 높음 |
| **Enemy** | 방당 5~20회 | 10~30초 | ⭐⭐⭐⭐ 높음 |
| **Visual Effect** | 초당 3~10회 | 0.5~1초 | ⭐⭐⭐⭐⭐ 매우 높음 |

**결론**: 모든 전투 관련 오브젝트에 풀링 필수!

#### 3. 최적화 목표

- ✅ **GC Allocation 90% 감소**
- ✅ **프레임 안정화** (일정한 60 FPS 유지)
- ✅ **메모리 사용량 예측 가능** (초기 풀 크기로 제한)
- ✅ **코드 재사용성 향상** (제네릭 풀 시스템)

---

### 🏗️ 오브젝트 풀링 시스템 구축 과정

#### Phase 1: 코어 시스템 설계

**1단계: IPoolable 인터페이스 설계**

```csharp
// Assets/_Project/Scripts/Core/ObjectPool/IPoolable.cs
namespace GASPT.Core.Pooling
{
    /// <summary>
    /// 풀링 가능한 오브젝트 인터페이스
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 풀에서 꺼낼 때 호출
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 풀로 반환할 때 호출
        /// </summary>
        void OnDespawn();
    }
}
```

**핵심 개념**:
- `OnSpawn()`: 오브젝트 초기화 (HP 복원, 상태 리셋)
- `OnDespawn()`: 정리 작업 (이벤트 구독 해제, 리소스 해제)

**2단계: ObjectPool<T> 제네릭 클래스**

```csharp
// Assets/_Project/Scripts/Core/ObjectPool/ObjectPool.cs
public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> availableObjects = new Queue<T>();
    private readonly HashSet<T> activeObjects = new HashSet<T>();
    private readonly T prefab;
    private readonly Transform poolParent;

    public T Get(Vector3 position, Quaternion rotation)
    {
        T obj;

        // 사용 가능한 오브젝트가 없으면 새로 생성
        if (availableObjects.Count == 0)
        {
            obj = CreateNewObject();
        }
        else
        {
            obj = availableObjects.Dequeue();
        }

        activeObjects.Add(obj);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);

        // IPoolable 인터페이스 호출
        if (obj is IPoolable poolable)
            poolable.OnSpawn();

        return obj;
    }

    public void Release(T obj)
    {
        if (!activeObjects.Contains(obj))
            return;

        // IPoolable 인터페이스 호출
        if (obj is IPoolable poolable)
            poolable.OnDespawn();

        activeObjects.Remove(obj);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolParent);
        availableObjects.Enqueue(obj);
    }
}
```

**설계 포인트**:
- `Queue<T>`: 사용 가능한 오브젝트 (FIFO)
- `HashSet<T>`: 활성 오브젝트 (중복 방지)
- 타입 안전성 (제네릭)

**3단계: PoolManager 싱글톤**

```csharp
// Assets/_Project/Scripts/Core/ObjectPool/PoolManager.cs
public class PoolManager : SingletonManager<PoolManager>
{
    private Dictionary<string, object> pools = new Dictionary<string, object>();

    public ObjectPool<T> CreatePool<T>(T prefab, int initialSize = 10, bool canGrow = true)
        where T : Component
    {
        string poolKey = typeof(T).Name;

        if (pools.ContainsKey(poolKey))
            return pools[poolKey] as ObjectPool<T>;

        var pool = new ObjectPool<T>(prefab, poolParent, initialSize, canGrow);
        pools[poolKey] = pool;

        return pool;
    }

    public T Spawn<T>(Vector3 position, Quaternion rotation) where T : Component
    {
        var pool = GetPool<T>();
        return pool.Get(position, rotation);
    }

    public void Despawn<T>(T obj) where T : Component
    {
        // 중요: 런타임 타입 사용!
        System.Type actualType = obj.GetType();
        string poolKey = actualType.Name;

        var pool = pools[poolKey];
        var releaseMethod = pool.GetType().GetMethod("Release");
        releaseMethod.Invoke(pool, new object[] { obj });
    }
}
```

**핵심 기능**:
- 모든 풀을 중앙에서 관리
- 타입별 풀 자동 생성
- Spawn/Despawn 편의 메서드

#### Phase 2: 투사체 풀링 적용

**1단계: Projectile 베이스 클래스**

```csharp
// Assets/_Project/Scripts/Gameplay/Projectiles/Projectile.cs
[RequireComponent(typeof(PooledObject))]
public class Projectile : MonoBehaviour, IPoolable
{
    protected float speed = 10f;
    protected float maxDistance = 20f;
    protected float damage = 10f;
    protected bool isActive;

    public virtual void OnSpawn()
    {
        startPosition = transform.position;
        travelDistance = 0f;
        isActive = true;
    }

    public virtual void OnDespawn()
    {
        isActive = false;
    }

    public virtual void Launch(Vector2 direction)
    {
        this.direction = direction.normalized;
        isActive = true;
    }

    protected virtual void ReturnToPool()
    {
        isActive = false;
        PoolManager.Instance.Despawn(this);
    }
}
```

**2단계: FireballProjectile 구현**

```csharp
public class FireballProjectile : Projectile
{
    [SerializeField] private float explosionRadius = 3f;

    protected override void OnHit(Collider2D hitCollider)
    {
        Vector3 explosionPos = transform.position;
        Explode(explosionPos);
    }

    private void Explode(Vector3 explosionPos)
    {
        // 범위 내 적 검색 및 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPos, explosionRadius);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage((int)damage);
            }
        }

        // 폭발 효과 재생 (풀 사용)
        PlayExplosionEffect(explosionPos);

        // 풀로 반환
        ReturnToPool();
    }

    private void PlayExplosionEffect(Vector3 explosionPos)
    {
        // VisualEffect 풀에서 가져오기
        var explosion = PoolManager.Instance.Spawn<VisualEffect>(
            explosionPos, Quaternion.identity
        );

        explosion.Play(
            duration: 0.5f,
            startScale: 0.5f,
            endScale: explosionRadius * 2f,
            startColor: new Color(1f, 0.8f, 0f, 0.7f),
            endColor: new Color(1f, 0.8f, 0f, 0f)
        );
    }
}
```

**3단계: Ability 클래스 수정**

```csharp
// Before - GameObject 직접 생성 ❌
public async Task ExecuteAsync(GameObject caster, CancellationToken token)
{
    GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    // ... 설정 ...
    Destroy(fireball);
}

// After - 풀 사용 ✅
public async Task ExecuteAsync(GameObject caster, CancellationToken token)
{
    var fireball = PoolManager.Instance.Spawn<FireballProjectile>(
        caster.transform.position,
        Quaternion.identity
    );

    fireball.Launch(direction);
    // 자동으로 풀 반환됨!
}
```

#### Phase 3: Enemy 및 Effect 풀링 적용

**Enemy 풀링**:
```csharp
public class Enemy : MonoBehaviour, IPoolable
{
    public void OnSpawn()
    {
        isDead = false;
        currentHp = enemyData.maxHp;
        OnHpChanged?.Invoke(currentHp, enemyData.maxHp);
    }

    public void OnDespawn()
    {
        UnsubscribeFromStatusEffectEvents();
        OnHpChanged = null;
        OnDeath = null;
    }

    private async void ReturnToPoolDelayed(float delay)
    {
        await Awaitable.WaitForSecondsAsync(delay);
        PoolManager.Instance.Despawn(this);
    }
}
```

**Effect 풀링**:
```csharp
public class VisualEffect : MonoBehaviour, IPoolable
{
    public void Play(float duration, float startScale, float endScale,
                     Color startColor, Color endColor)
    {
        // 애니메이션 실행
        // 완료 시 자동으로 ReturnToPool() 호출
    }
}
```

#### Phase 4: 초기화 시스템 통합

```csharp
// Assets/_Project/Scripts/Core/SingletonPreloader.cs
public void PreloadAllSingletons()
{
    // 0-1. Object Pooling (게임플레이 최적화)
    PreloadPoolManager();

    // ...

    // 8. Projectile Pools
    InitializeProjectilePools();

    // 9. Enemy Pools
    InitializeEnemyPools();

    // 10. Effect Pools
    InitializeEffectPools();
}
```

---

### 🐛 발견한 에러 및 해결 과정

#### 에러 1: Despawn이 호출되지 않음

**🔴 문제 상황**:
```csharp
// 증상: 오브젝트가 계속 생성만 되고 재사용되지 않음
// 콘솔 출력:
[PoolManager] FireballProjectile 풀 생성: 초기 5개
[FireballProjectile] Spawn (5번째 사용)
[PoolManager] FireballProjectile 풀 확장! 새로 생성 중...
// ← 풀로 반환되지 않아 계속 새로 생성!
```

**🔍 원인 분석**:

1. `PooledObject.ReturnToPool()`이 단순히 `gameObject.SetActive(false)`만 호출
2. `PoolManager.Despawn()`이 호출되지 않음
3. 풀의 `availableObjects` 큐에 반환되지 않음

**문제 코드**:
```csharp
// PooledObject.cs - 잘못된 구현 ❌
public void ReturnToPool()
{
    // 그냥 비활성화만 함!
    gameObject.SetActive(false);

    // PoolManager에 반환하지 않음! ← 문제!
}
```

**✅ 해결 방법**:

```csharp
// Projectile.cs - 수정 후
protected virtual void ReturnToPool()
{
    isActive = false;

    // PoolManager를 통해 풀로 반환
    if (PoolManager.Instance != null)
    {
        PoolManager.Instance.Despawn(this);
    }
    else
    {
        Debug.LogWarning("[Projectile] PoolManager 없음. GameObject 파괴.");
        Destroy(gameObject);
    }
}
```

**해결 결과**:
```
✅ Spawn: availableObjects.Dequeue() → activeObjects.Add()
✅ Despawn: activeObjects.Remove() → availableObjects.Enqueue()
✅ 재사용 정상 작동!
```

---

#### 에러 2: 런타임 타입 불일치로 풀을 찾지 못함

**🔴 문제 상황**:
```csharp
// 증상
[PoolManager] Despawn 호출: Projectile 타입
[PoolManager] Projectile 풀이 없습니다! GameObject 파괴합니다.

// 실제 풀 상태
pools["FireballProjectile"] = ObjectPool<FireballProjectile> ✅ 존재
pools["Projectile"] = null  ← 없음!
```

**🔍 원인 분석**:

```csharp
// Despawn<T> 메서드의 문제점
public void Despawn<T>(T obj) where T : Component
{
    // 컴파일 타임 타입 사용 ❌
    string poolKey = typeof(T).Name;  // "Projectile"

    // 실제 풀 키는 "FireballProjectile"!
    // → 풀을 찾을 수 없음!
}

// 호출 코드
Projectile projectile = GetComponent<Projectile>();
PoolManager.Instance.Despawn<Projectile>(projectile);
// → typeof(Projectile).Name = "Projectile" ❌
```

**타입 불일치 도식**:
```
풀 생성:
CreatePool<FireballProjectile>(...)
→ pools["FireballProjectile"] = ObjectPool<FireballProjectile>

Spawn:
Spawn<FireballProjectile>(...)
→ pools["FireballProjectile"].Get() ✅ 작동

Despawn (문제):
Projectile proj = ...;
Despawn<Projectile>(proj)
→ typeof(Projectile).Name = "Projectile"
→ pools["Projectile"] 찾기 시도 ❌ 없음!
```

**✅ 해결 방법**:

```csharp
// PoolManager.cs - 수정 후
public void Despawn<T>(T obj) where T : Component
{
    if (obj == null) return;

    // 런타임 타입 사용 ✅
    System.Type actualType = obj.GetType();  // FireballProjectile
    string poolKey = actualType.Name;  // "FireballProjectile"

    // 풀 찾기
    if (!pools.ContainsKey(poolKey))
    {
        Debug.LogWarning($"[PoolManager] {poolKey} 풀 없음.");
        Destroy(obj.gameObject);
        return;
    }

    // Reflection으로 Release 호출
    var pool = pools[poolKey];
    var releaseMethod = pool.GetType().GetMethod("Release");
    releaseMethod.Invoke(pool, new object[] { obj });
}
```

**동작 흐름**:
```
Despawn<Projectile>(fireballProjectile)
→ obj.GetType() = FireballProjectile (런타임)
→ poolKey = "FireballProjectile"
→ pools["FireballProjectile"] 찾기 ✅ 성공!
→ pool.Release(fireballProjectile) ✅ 반환 완료!
```

**해결 결과**:
```
[PoolManager] Despawn: FireballProjectile (런타임 타입)
[ObjectPool<FireballProjectile>] Release 호출
[FireballProjectile] OnDespawn() 호출
✅ 풀로 정상 반환!
```

---

#### 에러 3: Enemy 반환 시 타입 캐스팅 문제

**🔴 문제 상황**:
```csharp
// Enemy.cs
private async void ReturnToPoolDelayed(float delay)
{
    await Awaitable.WaitForSecondsAsync(delay);

    // 문제: Enemy는 추상 클래스, 실제 타입은 BasicMeleeEnemy
    PoolManager.Instance.Despawn(this);
    // → typeof(this) = BasicMeleeEnemy ✅
    // → pools["BasicMeleeEnemy"] 찾기 ✅
}
```

이 부분은 런타임 타입 사용으로 **자동 해결**되었습니다!

---

### 📊 성능 개선 결과

#### Before vs After 비교

| 항목 | Before (풀링 전) | After (풀링 후) | 개선율 |
|------|-----------------|----------------|--------|
| **메모리 할당** (전투 10초) | ~5 MB | ~200 KB | **96% 감소** |
| **GC 빈도** | 3초마다 | 30초마다 | **90% 감소** |
| **GC 시간** | 50~100ms | 5~10ms | **90% 감소** |
| **평균 FPS** (전투) | 45 FPS | 60 FPS | **33% 향상** |
| **프레임 드롭** | 빈번 (40~60) | 거의 없음 (58~60) | **안정화** |

#### 풀 사용 현황

```
[PoolManager] 풀 상태 출력
========== Pool Manager Info ==========
Total Pools: 4

[FireballProjectile]
  Total: 8, Active: 3, Available: 5
  Initial: 5, CanGrow: True

[MagicMissileProjectile]
  Total: 15, Active: 7, Available: 8
  Initial: 10, CanGrow: True

[BasicMeleeEnemy]
  Total: 10, Active: 5, Available: 5
  Initial: 5, CanGrow: True

[VisualEffect]
  Total: 20, Active: 8, Available: 12
  Initial: 10, CanGrow: True
=======================================
```

**인사이트**:
- FireballProjectile: 5개 초기 풀로 충분 (확장 3개만 발생)
- MagicMissileProjectile: 10개 초기 풀, 빈번한 사용으로 15개까지 확장
- VisualEffect: 가장 높은 사용 빈도 (폭발 + 타격)

---

### 💡 배운 점 및 베스트 프랙티스

#### 1. 오브젝트 풀링 설계 원칙

**DO ✅**:
```csharp
// 제네릭으로 타입 안전성 확보
public class ObjectPool<T> where T : Component { }

// IPoolable 인터페이스로 초기화/정리 표준화
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}

// 런타임 타입으로 풀 찾기
System.Type actualType = obj.GetType();
```

**DON'T ❌**:
```csharp
// 컴파일 타입으로 풀 찾기
typeof(T).Name  // ← 상속 계층에서 문제!

// 풀 반환 없이 SetActive(false)만
gameObject.SetActive(false);  // ← 풀에 반환 안됨!

// Destroy 직접 호출
Destroy(pooledObject);  // ← 풀링 의미 없음!
```

#### 2. 초기 풀 크기 결정

```csharp
// 공식: 초기 크기 = 동시 최대 사용량 + 여유분
public void InitializePool()
{
    int simultaneousUse = 5;     // 동시에 활성화될 최대 개수
    int buffer = 2;              // 여유분 (스파이크 대비)
    int initialSize = simultaneousUse + buffer;  // 7개

    PoolManager.Instance.CreatePool(prefab, initialSize, canGrow: true);
}
```

**프로파일링으로 최적값 찾기**:
1. 초기 크기를 작게 설정 (5개)
2. 게임 플레이하며 `PrintPoolInfo()` 확인
3. `Total > Initial`이면 확장 발생 → 초기 크기 증가
4. 반복하여 최적값 찾기

#### 3. 풀 반환 타이밍

```csharp
// 즉시 반환
protected override void OnHit(Collider2D hitCollider)
{
    // 충돌 처리
    enemy.TakeDamage(damage);

    // 즉시 반환
    ReturnToPool();
}

// 지연 반환 (애니메이션 후)
private void Die()
{
    // 사망 애니메이션 1초
    ReturnToPoolDelayed(1f);
}

// 자동 반환 (PooledObject)
[SerializeField] private bool autoReturn = true;
[SerializeField] private float autoReturnTime = 3f;
```

#### 4. 메모리 누수 방지

```csharp
// OnDespawn에서 완전 정리 필수!
public void OnDespawn()
{
    // 이벤트 구독 해제 ✅
    UnsubscribeFromStatusEffectEvents();

    // 이벤트 핸들러 null ✅
    OnHpChanged = null;
    OnDeath = null;

    // Trail 초기화 ✅
    if (trailRenderer != null)
        trailRenderer.Clear();

    // 상태 리셋 ✅
    currentEffect = null;
}
```

#### 5. 디버깅 팁

```csharp
// Context Menu로 풀 상태 확인
[ContextMenu("Print Pool Info")]
public void PrintPoolInfo()
{
    Debug.Log("========== Pool Manager Info ==========");
    // ... 풀 정보 출력 ...
}

// OnDrawGizmos로 활성 오브젝트 시각화
private void OnDrawGizmos()
{
    if (isActive)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
```

---

### 🎓 프로젝트 적용 체크리스트

#### 설계 단계
- [x] IPoolable 인터페이스 정의
- [x] ObjectPool<T> 제네릭 클래스 구현
- [x] PoolManager 싱글톤 구현
- [x] PooledObject 컴포넌트 작성

#### 적용 단계
- [x] Projectile 베이스 클래스 (IPoolable)
- [x] FireballProjectile 구현
- [x] MagicMissileProjectile 구현
- [x] Enemy IPoolable 적용
- [x] VisualEffect IPoolable 적용
- [x] Ability 클래스 풀 사용으로 수정

#### 초기화 단계
- [x] ProjectilePoolInitializer 작성
- [x] EnemyPoolInitializer 작성
- [x] EffectPoolInitializer 작성
- [x] SingletonPreloader 통합

#### 디버깅 단계
- [x] Despawn 호출 확인
- [x] 런타임 타입 문제 해결
- [x] 메모리 누수 확인
- [x] 성능 프로파일링

---

### 📚 참고 자료

#### Unity 공식 문서
- [Object Pooling in Unity](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity4-1.html)
- [Memory Management Best Practices](https://docs.unity3d.com/Manual/performance-garbage-collection-best-practices.html)

#### 학습 리소스
- Unity Object Pooling Tutorial (YouTube)
- C# Generic Collections (Microsoft Docs)
- Unity Profiler 사용법

---

### 🔗 관련 커밋

- `[PoolManager]` 코어 풀링 시스템 구현
- `[Projectile]` 투사체 풀링 적용
- `[Enemy]` Enemy 풀링 적용
- `[Effect]` VisualEffect 풀링 적용
- `[Fix]` Despawn 호출 누락 수정
- `[Fix]` 런타임 타입 불일치 문제 해결

---

### 💬 회고

#### 잘한 점
1. **제네릭 설계**: 타입 안전성과 재사용성 확보
2. **IPoolable 인터페이스**: 표준화된 초기화/정리 패턴
3. **싱글톤 매니저**: 중앙 집중식 풀 관리
4. **에러 해결**: 런타임 타입 문제를 빠르게 파악하고 해결

#### 개선할 점
1. **초기 풀 크기**: 프로파일링으로 최적값 찾기 필요
2. **풀 반환 로직**: 더 명확한 패턴 정립 필요
3. **문서화**: 사용법 가이드 작성 필요

#### 향후 계획
1. **자동 풀 크기 조정**: 런타임 통계 기반 동적 조정
2. **풀 워밍업**: 게임 시작 시 미리 생성
3. **풀 통계 UI**: Editor Window로 실시간 모니터링

---

## 7. Unity EditorWindow GUI 레이아웃 오류

### 📋 오류 개요
- **발생 날짜**: 2025-11-13
- **작업 컨텍스트**: Phase B-3 UI 시스템 통합 후 GameplaySceneCreator 실행
- **관련 브랜치**: `015-playable-prototype-phase-b1`
- **관련 커밋**: `e67dceb` - EditorWindow GUI 레이아웃 오류 해결

### 🔴 오류 내용

#### 오류 메시지
```
EndLayoutGroup: BeginLayoutGroup must be called first.
0x000002332c2416b3 (Mono JIT Code) GASPT.Editor.GameplaySceneCreator:OnGUI ()
(at D:/JaeChang/UintyDev/GASPT/GASPT/Assets/_Project/Scripts/Editor/GameplaySceneCreator.cs:129)
```

#### 발생 상황
`Tools > GASPT > 🎮 Gameplay Scene Creator` 메뉴에서 "🚀 GameplayScene 생성" 버튼을 클릭하면 Console에 빨간색 오류 메시지가 출력되었습니다. 씬은 정상적으로 생성되었지만, 에디터 윈도우가 오작동했습니다.

#### 문제가 된 코드
```csharp
// GameplaySceneCreator.cs - OnGUI() 메서드

private void OnGUI()
{
    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition); // ← BeginScrollView 시작

    // ... GUI 요소들 ...

    // 씬 생성 버튼
    if (GUILayout.Button("🚀 GameplayScene 생성", GUILayout.Height(50)))
    {
        CreateGameplayScene(); // ❌ 즉시 실행! (무거운 작업)
        // → 씬에 많은 오브젝트 생성
        // → SerializedObject 수정
        // → Unity가 GUI 재렌더링 시도
        // → 레이아웃 스택 충돌! 💥
    }

    // ... 더 많은 GUI 요소들 ...

    EditorGUILayout.EndScrollView(); // ← Line 129: 여기서 에러 발생!
    // EndScrollView()를 호출할 때 BeginScrollView()와 짝이 맞지 않음!
}
```

#### 재현 방법
1. `Tools > GASPT > 🎮 Gameplay Scene Creator` 실행
2. "🚀 GameplayScene 생성" 버튼 클릭
3. Console에 `EndLayoutGroup: BeginLayoutGroup must be called first.` 오류 출력

---

### 🔍 문제 분석

#### Unity IMGUI 시스템 이해

Unity의 EditorWindow는 **즉시 모드 GUI (IMGUI)** 시스템을 사용합니다:

```csharp
// IMGUI의 프레임 구조
Frame 1: OnGUI() 전체 실행 → GUI 렌더링
Frame 2: OnGUI() 전체 실행 → GUI 렌더링
Frame 3: OnGUI() 전체 실행 → GUI 렌더링
...
```

**IMGUI 레이아웃 규칙**:
- `Begin*()` 호출 → GUI 요소들 → `End*()` 호출 (순서 엄격)
- 하나의 `OnGUI()` 프레임 내에서 레이아웃 스택이 완전히 일치해야 함

#### 근본 원인

```
OnGUI() 실행 흐름:
┌─────────────────────────────────────────────┐
│ 1. BeginScrollView() 호출                    │ ← 레이아웃 스택 +1
├─────────────────────────────────────────────┤
│ 2. GUI 요소들 (버튼, 슬라이더 등)            │
├─────────────────────────────────────────────┤
│ 3. 버튼 클릭 → CreateAllUI() 즉시 실행 ❌    │
│    ├─ Canvas 생성                            │
│    ├─ 6개 UI 오브젝트 생성                   │ ← 씬 변경!
│    ├─ SerializedObject.ApplyModified()      │
│    └─ Unity가 씬 변경 감지                   │
│                                               │
│    Unity가 Editor를 재렌더링하려고 시도...   │ 💥
│    하지만 아직 OnGUI() 진행 중!              │
│    → GUI 레이아웃 스택 충돌!                 │
├─────────────────────────────────────────────┤
│ 4. EndScrollView() 호출                      │ ← 레이아웃 스택 -1 (예상)
│    → 하지만 스택이 이미 깨짐!                │ ← 에러 발생!
└─────────────────────────────────────────────┘
```

**문제점**:
1. **즉시 실행**: 버튼 클릭 → `CreateAllUI()` 즉시 실행
2. **무거운 작업**: 메서드 내부에서 많은 GameObject 생성 + SerializedObject 수정
3. **Unity 재렌더링**: Unity가 씬 변경을 감지하고 Editor GUI 재렌더링 시도
4. **레이아웃 충돌**: 아직 `OnGUI()`가 진행 중인데 GUI가 재렌더링되면서 레이아웃 스택 깨짐
5. **짝 불일치**: `EndScrollView()`를 호출할 때 `BeginScrollView()`와 짝이 맞지 않음

#### 영향 범위
- **GameplaySceneCreator.cs**: 5개 버튼 (씬 생성, 플레이어, 방 시스템, UI, 카메라)
- **PrefabCreator.cs**: 6개 버튼 (전체 생성, 개별 프리팹 생성들)

---

### ✅ 해결 방법

#### 핵심 아이디어: 작업 지연 실행

Unity가 제공하는 `EditorApplication.delayCall`을 사용하여 무거운 작업을 **현재 GUI 프레임 완료 후** 실행하도록 변경합니다.

#### 수정된 코드

```csharp
// BEFORE (문제 코드) ❌
if (GUILayout.Button("🚀 GameplayScene 생성", GUILayout.Height(50)))
{
    CreateGameplayScene(); // 즉시 실행 → 레이아웃 충돌!
}

// AFTER (수정된 코드) ✅
if (GUILayout.Button("🚀 GameplayScene 생성", GUILayout.Height(50)))
{
    EditorApplication.delayCall += CreateGameplayScene; // 지연 실행!
}
```

#### 동작 원리

```
수정 후 실행 흐름:
┌─────────────────────────────────────────────┐
│ Frame N: OnGUI() 실행                        │
│ 1. BeginScrollView()                         │ ← 레이아웃 스택 +1
│ 2. GUI 요소들                                │
│ 3. 버튼 클릭 → delayCall에 등록만 함 ✅      │ ← 즉시 실행 안함!
│ 4. EndScrollView()                           │ ← 레이아웃 스택 -1 ✅
│ → OnGUI() 정상 완료!                         │
└─────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────┐
│ Frame N+1: delayCall 실행                    │
│ → CreateGameplayScene() 실행                 │ ✅ 안전하게 실행!
│   ├─ Canvas 생성                             │
│   ├─ UI 오브젝트 생성                        │
│   └─ SerializedObject 수정                   │
│ → 레이아웃 충돌 없음!                        │
└─────────────────────────────────────────────┘
```

**장점**:
- ✅ **레이아웃 스택 보호**: OnGUI() 완전히 끝난 후 실행
- ✅ **Unity 재렌더링 안전**: 다음 프레임에서 실행되므로 충돌 없음
- ✅ **코드 변경 최소**: 한 줄만 수정 (`+=` 사용)

---

### 🛠️ 구체적인 수정 사항

#### 1. GameplaySceneCreator.cs (5개 버튼)

```csharp
// 1. 전체 씬 생성
if (GUILayout.Button("🚀 GameplayScene 생성", GUILayout.Height(50)))
{
    EditorApplication.delayCall += CreateGameplayScene; // ✅
}

// 2. 플레이어만 생성
if (GUILayout.Button("플레이어만 생성"))
{
    EditorApplication.delayCall += CreatePlayer; // ✅
}

// 3. 방 시스템만 생성
if (GUILayout.Button("방 시스템만 생성"))
{
    EditorApplication.delayCall += CreateRoomSystem; // ✅
}

// 4. UI만 생성
if (GUILayout.Button("UI만 생성"))
{
    EditorApplication.delayCall += CreateAllUI; // ✅ (주요 원인)
}

// 5. 카메라만 생성
if (GUILayout.Button("카메라만 생성"))
{
    EditorApplication.delayCall += CreateCameraSystem; // ✅
}
```

#### 2. PrefabCreator.cs (6개 버튼, 예방 차원)

```csharp
// 1. 전체 프리팹 생성
if (GUILayout.Button("🚀 모든 프리팹 생성", GUILayout.Height(40)))
{
    EditorApplication.delayCall += CreateAllPrefabs; // ✅
}

// 2. MageForm 프리팹
if (GUILayout.Button("MageForm 프리팹 생성"))
{
    EditorApplication.delayCall += CreateMageFormPrefab; // ✅
}

// 3. Projectile 프리팹
if (GUILayout.Button("Projectile 프리팹 생성"))
{
    EditorApplication.delayCall += CreateProjectilePrefabs; // ✅
}

// 4. VisualEffect 프리팹
if (GUILayout.Button("VisualEffect 프리팹 생성"))
{
    EditorApplication.delayCall += CreateVisualEffectPrefab; // ✅
}

// 5. BasicMeleeEnemy 프리팹
if (GUILayout.Button("BasicMeleeEnemy 프리팹 생성"))
{
    EditorApplication.delayCall += CreateBasicMeleeEnemyPrefab; // ✅
}

// 6. 폴더 생성
if (GUILayout.Button("프리팹 폴더 생성"))
{
    EditorApplication.delayCall += CreatePrefabFolders; // ✅
}
```

---

### 🧪 테스트 및 검증

#### 테스트 방법
1. Unity 에디터 재시작
2. `Tools > GASPT > 🎮 Gameplay Scene Creator` 실행
3. "🚀 GameplayScene 생성" 버튼 클릭
4. Console 확인

#### 검증 결과
- ✅ **오류 없음**: `EndLayoutGroup` 오류 미발생
- ✅ **씬 정상 생성**: Canvas + 6개 UI 요소 생성 확인
- ✅ **에디터 윈도우 정상 작동**: 버튼 클릭 후에도 GUI 정상 표시

---

### 📚 배운 점 (Best Practices)

#### Unity EditorWindow 개발 규칙

1. **무거운 작업은 지연 실행**
   ```csharp
   // ❌ 나쁜 예
   if (GUILayout.Button("Create"))
   {
       CreateManyObjects(); // 즉시 실행
   }

   // ✅ 좋은 예
   if (GUILayout.Button("Create"))
   {
       EditorApplication.delayCall += CreateManyObjects; // 지연 실행
   }
   ```

2. **OnGUI() 내에서 금지할 작업**
   - ❌ 씬에 많은 오브젝트 생성
   - ❌ SerializedObject 대량 수정
   - ❌ 에셋 생성/삭제
   - ❌ Resources.Load() 등 무거운 I/O

3. **지연 실행 방법 2가지**
   ```csharp
   // 방법 1: delayCall (단발성 작업)
   EditorApplication.delayCall += MyMethod;

   // 방법 2: update (반복 작업)
   EditorApplication.update += MyUpdateMethod;
   // ... 작업 후
   EditorApplication.update -= MyUpdateMethod;
   ```

4. **레이아웃 디버깅 팁**
   ```csharp
   // Begin/End 짝 확인
   try
   {
       EditorGUILayout.BeginScrollView(...);
       // GUI 요소들
   }
   finally
   {
       EditorGUILayout.EndScrollView(); // 반드시 호출!
   }
   ```

---

### 🔗 관련 커밋 및 PR

#### 커밋 정보
```
e67dceb - 수정: EditorWindow GUI 레이아웃 오류 해결
└─ GameplaySceneCreator.cs: 5개 버튼 delayCall 적용
└─ PrefabCreator.cs: 6개 버튼 delayCall 적용
```

#### 변경 파일
- `Assets/_Project/Scripts/Editor/GameplaySceneCreator.cs`
- `Assets/_Project/Scripts/Editor/PrefabCreator.cs`

---

### 💬 회고

#### 잘한 점
1. **신속한 문제 파악**: 오류 메시지에서 라인 번호 확인 → `EndScrollView()` 위치 파악
2. **근본 원인 분석**: IMGUI 레이아웃 스택 개념 이해
3. **최소 변경 원칙**: 기존 코드 구조 유지하면서 `+=` 연산자로 간단하게 해결
4. **예방 조치**: PrefabCreator도 함께 수정하여 동일 문제 예방

#### 개선할 점
1. **초기 설계**: EditorWindow 작성 시 무거운 작업은 처음부터 지연 실행 고려
2. **문서화**: Unity IMGUI 베스트 프랙티스 문서 작성 필요
3. **코드 리뷰**: 에디터 도구 코드에 대한 체크리스트 작성

#### 향후 적용
1. **모든 에디터 도구**: 무거운 작업은 `delayCall` 사용
2. **진행 표시**: 긴 작업은 `EditorUtility.DisplayProgressBar` 추가
3. **에러 핸들링**: try-catch로 레이아웃 스택 보호

---

### 📖 참고 자료

#### Unity 공식 문서
- [EditorApplication.delayCall](https://docs.unity3d.com/ScriptReference/EditorApplication-delayCall.html)
- [IMGUI Layout Modes](https://docs.unity3d.com/Manual/gui-Layout.html)
- [Editor Window Best Practices](https://docs.unity3d.com/Manual/editor-CustomEditors.html)

#### 관련 포럼
- Unity Forum: "EndLayoutGroup error in EditorWindow"
- Stack Overflow: "Unity IMGUI Layout Issues"

---

## 8. virtual vs override: 메서드 하이딩과 오버라이딩의 차이

### 📋 오류 개요
- **발생 날짜**: 2025-11-17
- **작업 컨텍스트**: BossEnemy 보스 HP 초기화 버그 수정 (Phase C-2)
- **관련 브랜치**: `master`
- **오류 코드**: CS0114 (메서드 하이딩 경고)

### 🔴 문제 상황

#### 발생한 버그
BossEnemy의 HP가 초기화되지 않아 0으로 남아있는 문제가 발생했습니다.

#### 원인 분석
상속 계층에서 `Start()` 메서드가 제대로 호출되지 않았습니다:

```
Enemy (베이스 클래스)
  ↓
PlatformerEnemy (중간 클래스)
  ↓
BossEnemy (최종 클래스)
```

**문제점:**
1. `Enemy.Start()`가 `private`으로 선언되어 상속되지 않음
2. `PlatformerEnemy.Start()`가 `base.Start()`를 호출하지 않음
3. `PlatformerEnemy.Start()`를 `virtual`로 선언하여 메서드 하이딩 발생

#### 컴파일러 경고
```
CS0114: 'PlatformerEnemy.Start()' hides inherited member 'Enemy.Start()'.
To make the current member override that implementation, add the override keyword.
Otherwise add the new keyword.
```

---

### 🔍 핵심 개념: virtual vs override

#### 1️⃣ virtual (새로운 가상 메서드 선언)

**의미:**
- 베이스 클래스에서 "이 메서드는 자식에서 재정의 가능하다"고 **선언**
- 새로운 가상 메서드 체인의 시작점

**사용 예시:**
```csharp
// Enemy.cs (베이스 클래스)
public class Enemy : MonoBehaviour
{
    protected virtual void Start()  // ✅ 가상 메서드 선언
    {
        Initialize();  // HP 초기화
    }
}
```

#### 2️⃣ override (가상 메서드 재정의)

**의미:**
- 부모 클래스의 가상 메서드를 **재정의**
- 상속 체인을 유지하며 기능 확장

**사용 예시:**
```csharp
// PlatformerEnemy.cs (중간 클래스)
public class PlatformerEnemy : Enemy
{
    protected override void Start()  // ✅ 부모 메서드 재정의
    {
        base.Start();  // Enemy.Start() 호출
        InitializeComponents();
    }
}

// BossEnemy.cs (최종 클래스)
public class BossEnemy : PlatformerEnemy
{
    protected override void Start()  // ✅ 계속 재정의 가능
    {
        base.Start();  // PlatformerEnemy.Start() 호출
        InitializePhaseController();
    }
}
```

---

### ⚠️ 잘못된 방법: virtual 재선언 (메서드 하이딩)

#### 문제가 된 코드

```csharp
// Enemy.cs
public class Enemy
{
    protected virtual void Start()
    {
        Debug.Log("Enemy.Start() - HP 초기화");
        Initialize();
    }
}

// PlatformerEnemy.cs
public class PlatformerEnemy : Enemy
{
    protected virtual void Start()  // ❌ 새로운 virtual (하이딩)
    {
        base.Start();
        Debug.Log("PlatformerEnemy.Start()");
    }
}

// BossEnemy.cs
public class BossEnemy : PlatformerEnemy
{
    protected override void Start()
    {
        base.Start();
        Debug.Log("BossEnemy.Start()");
    }
}
```

#### 실행 결과
```
✅ 직접 호출 시 (boss.Start()):
   Enemy.Start() - HP 초기화
   PlatformerEnemy.Start()
   BossEnemy.Start()
   → 정상 작동 (base.Start() 명시적 호출 때문)

❌ 다형성 사용 시 (Enemy타입으로 참조):
   Enemy boss = new BossEnemy();
   boss.Start();

   → Enemy.Start()만 호출됨!
   → PlatformerEnemy.Start(), BossEnemy.Start() 호출 안 됨!
```

---

### ✅ 올바른 방법: override 사용

#### 수정된 코드

```csharp
// Enemy.cs
public class Enemy : MonoBehaviour
{
    protected virtual void Start()  // ✅ virtual 선언
    {
        if (enemyData != null && currentHp == 0)
        {
            Initialize();  // HP 초기화
        }
    }
}

// PlatformerEnemy.cs
public class PlatformerEnemy : Enemy
{
    protected override void Start()  // ✅ override로 재정의
    {
        base.Start();  // Enemy.Start() 호출

        InitializeComponents();
        FindPlayer();
        startPosition = transform.position;
        ChangeState(EnemyState.Idle);
    }
}

// BossEnemy.cs
public class BossEnemy : PlatformerEnemy
{
    protected override void Start()  // ✅ override로 재정의
    {
        base.Start();  // PlatformerEnemy.Start() 호출

        InitializePhaseController();
        CreateBossHealthBar();
    }
}
```

#### 실행 결과
```
✅ 직접 호출 시 (boss.Start()):
   Enemy.Start() - HP 초기화
   PlatformerEnemy.Start()
   BossEnemy.Start()

✅ 다형성 사용 시 (Enemy타입으로 참조):
   Enemy boss = new BossEnemy();
   boss.Start();

   → BossEnemy.Start() 호출됨!
   → base.Start() 체인을 따라 모두 호출됨!
```

---

### 🧪 다형성 차이점 비교

#### 테스트 코드
```csharp
public void TestPolymorphism()
{
    // BossEnemy 인스턴스를 Enemy 타입으로 참조
    Enemy enemy = new BossEnemy();

    enemy.Start();  // 어떤 Start()가 호출될까?
}
```

#### virtual (메서드 하이딩) 방식
```csharp
public class PlatformerEnemy : Enemy
{
    protected virtual void Start()  // 새로운 virtual
    {
        base.Start();
        // ...
    }
}
```

**메서드 테이블:**
```
Enemy 타입으로 참조 → Enemy.Start() 호출
   └─ Enemy.Start()만 실행됨 ❌
```

#### override (메서드 오버라이딩) 방식 ✅
```csharp
public class PlatformerEnemy : Enemy
{
    protected override void Start()  // override
    {
        base.Start();
        // ...
    }
}
```

**메서드 테이블:**
```
Enemy 타입으로 참조 → 실제 타입(BossEnemy)의 Start() 호출
   └─ BossEnemy.Start()
      └─ base.Start() → PlatformerEnemy.Start()
         └─ base.Start() → Enemy.Start() ✅
```

---

### 📊 비교표: virtual vs override

| 항목 | virtual (하이딩) | override (오버라이딩) ✅ |
|------|-----------------|----------------------|
| **선언 위치** | 베이스 클래스 또는 새 체인 시작 | 자식 클래스 (부모에 virtual 있어야 함) |
| **의미** | 새로운 가상 메서드 선언 | 부모 메서드 재정의 |
| **컴파일** | 경고 발생 (CS0114) | 정상 |
| **실행 (직접 호출)** | 작동 (base.Start() 때문) | 작동 |
| **실행 (다형성)** | 부모 타입으로 참조 시 문제 ❌ | 올바르게 작동 ✅ |
| **상속 체인** | 끊어짐 (숨겨짐) | 유지됨 |
| **재정의 가능** | 가능 (새 체인 시작) | 가능 (override 자동 virtual) |
| **사용 사례** | 완전히 새로운 메서드 만들 때 | 부모 기능 확장할 때 |

---

### 🔄 Override 체인 (계속 재정의 가능)

C#에서 **override된 메서드는 자동으로 virtual 속성을 유지**하므로 계속 재정의 가능합니다:

```csharp
// 1단계: virtual
public class Enemy
{
    protected virtual void Start() { }
}

// 2단계: override (자동으로 virtual)
public class PlatformerEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
    }
}

// 3단계: override (자동으로 virtual)
public class BossEnemy : PlatformerEnemy
{
    protected override void Start()
    {
        base.Start();
    }
}

// 4단계: 계속 가능
public class SuperBoss : BossEnemy
{
    protected override void Start()
    {
        base.Start();
    }
}
```

#### Override 체인 중단 (sealed)
```csharp
public class FinalBoss : BossEnemy
{
    protected sealed override void Start()  // 더 이상 override 불가
    {
        base.Start();
    }
}

public class CannotOverride : FinalBoss
{
    // ❌ 컴파일 에러!
    // protected override void Start() { }
}
```

---

### 🛠️ 해결 과정

#### Step 1: Enemy.Start()를 virtual로 변경
```csharp
// BEFORE ❌
private void Start()
{
    if (enemyData != null && currentHp == 0)
    {
        Initialize();
    }
}

// AFTER ✅
protected virtual void Start()
{
    if (enemyData != null && currentHp == 0)
    {
        Initialize();
    }
}
```

#### Step 2: PlatformerEnemy.Start()를 override로 변경
```csharp
// BEFORE ❌
protected virtual void Start()
{
    InitializeComponents();
    FindPlayer();
    // ...
}

// AFTER ✅
protected override void Start()
{
    base.Start();  // Enemy.Start() 호출

    InitializeComponents();
    FindPlayer();
    // ...
}
```

#### Step 3: BossEnemy.Start()는 이미 override ✅
```csharp
protected override void Start()
{
    base.Start();  // PlatformerEnemy.Start() 호출

    InitializePhaseController();
    CreateBossHealthBar();
}
```

---

### ✅ 결과

#### 호출 순서 (수정 후)
```
BossEnemy.Start()
  ↓
base.Start() → PlatformerEnemy.Start()
  ↓
base.Start() → Enemy.Start()
  ↓
Initialize() → currentHp = maxHp ✅
```

#### 테스트 확인
- ✅ 보스 HP가 500으로 정상 설정됨
- ✅ 보스 체력바 정상 표시
- ✅ 미니언 HP도 정상 설정됨
- ✅ 다형성 사용 시에도 정상 작동
- ✅ 컴파일러 경고 사라짐

---

### 💡 핵심 교훈

#### 1. virtual은 선언, override는 재정의
- **virtual**: "재정의 가능한 메서드를 선언한다"
- **override**: "부모의 메서드를 재정의한다"

#### 2. 상속 계층에서는 override 사용
- 베이스 클래스: `virtual`
- 모든 자식 클래스: `override`
- 새로운 메서드 체인이 필요한 경우만 `virtual` 재선언

#### 3. base.Start() 호출은 필수
- 부모의 초기화 로직을 실행하기 위해 반드시 `base.Start()` 호출
- 상속 체인의 모든 클래스가 제대로 초기화되도록 보장

#### 4. 다형성을 고려한 설계
- Unity는 주로 직접 참조를 사용하지만, 올바른 OOP 설계가 중요
- `override`를 사용해야 다형성이 제대로 작동

#### 5. 컴파일러 경고 무시하지 말기
- CS0114 경고는 의도하지 않은 메서드 하이딩을 알려줌
- 경고가 나오면 `override` 또는 `new` 키워드로 의도를 명확히 해야 함

---

### 📖 참고 자료

#### Microsoft C# 공식 문서
- [virtual (C# Reference)](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/virtual)
- [override (C# Reference)](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/override)
- [Polymorphism](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/polymorphism)
- [sealed (C# Reference)](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/sealed)

#### Unity 관련
- [MonoBehaviour Messages Order](https://docs.unity3d.com/Manual/ExecutionOrder.html)
- [Inheritance in Unity](https://docs.unity3d.com/Manual/class-MonoBehaviour.html)

---

**문서 작성자**: Jae Chang
**프로젝트 GitHub**: https://github.com/jaechang92/GAS
**마지막 업데이트**: 2025-11-17
