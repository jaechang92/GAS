# 오류 해결 사례 모음 (포트폴리오)

**프로젝트**: GASPT (Generic Ability System + FSM)
**문서 작성일**: 2025-11-04
**목적**: 개발 과정에서 발생한 주요 오류와 해결 방법을 기록하여 문제 해결 능력 입증

---

## 목차
1. [OnManaChanged 이벤트 매개변수 불일치 오류](#1-onmanachanged-이벤트-매개변수-불일치-오류)
2. [OperationCanceledException 발생 오류](#2-operationcanceledexception-발생-오류)

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

**문서 작성자**: Jae Chang
**프로젝트 GitHub**: https://github.com/jaechang92/GAS
**마지막 업데이트**: 2025-11-04
