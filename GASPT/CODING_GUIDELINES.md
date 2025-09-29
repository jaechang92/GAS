# GASPT 프로젝트 코딩 가이드라인

## 🚫 절대 금지 사항

### IEnumerator 사용 금지
- **모든 비동기 작업은 `async/await` + `Awaitable` 사용**
- **Unity 코루틴 사용 금지**
- **`yield return` 구문 사용 금지**

```csharp
// ❌ 절대 금지
public IEnumerator SomeMethod()
{
    yield return null;
    yield return new WaitForSeconds(1f);
}

// ✅ 반드시 사용
public async Awaitable SomeMethod()
{
    await Awaitable.NextFrameAsync();
    await Awaitable.WaitForSecondsAsync(1f);
}
```

### 테스트 코드 패턴
```csharp
// ❌ 절대 금지
[UnityTest]
public IEnumerator TestMethod()
{
    yield return null;
}

// ✅ 반드시 사용
[Test]
public async void TestMethod()
{
    await Awaitable.NextFrameAsync();
}
```

## ✅ 권장 패턴

### 1. 비동기 메서드
- `async Awaitable` 사용
- `async void` (테스트에서만)
- `async Task` (필요시)

### 2. 대기 패턴
- `await Awaitable.NextFrameAsync()` - 프레임 대기
- `await Awaitable.WaitForSecondsAsync(time)` - 시간 대기
- `await Awaitable.FromCancellationToken(token)` - 취소 토큰 대기

### 3. Unity 통합
- MonoBehaviour에서 직접 async 메서드 호출
- `_ = SomeAsyncMethod();` - fire-and-forget 패턴
- CancellationToken 적절히 활용

## 🔧 Migration 체크리스트
- [ ] 모든 IEnumerator → async Awaitable 변경
- [ ] 모든 yield return → await 변경
- [ ] 모든 [UnityTest] → [Test] 변경
- [ ] using System.Collections; 제거
- [ ] using UnityEngine.TestTools; 제거