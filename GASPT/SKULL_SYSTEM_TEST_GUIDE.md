# 🎮 스컬 시스템 테스트 가이드

GASPT 프로젝트의 스컬 시스템을 테스트하는 방법에 대한 완전한 가이드입니다.

---

## 📋 목차

1. [테스트 시스템 개요](#테스트-시스템-개요)
2. [단위 테스트 (Unit Tests)](#단위-테스트-unit-tests)
3. [통합 테스트 (Integration Tests)](#통합-테스트-integration-tests)
4. [인게임 테스트 도구](#인게임-테스트-도구)
5. [성능 테스트](#성능-테스트)
6. [자동화된 테스트](#자동화된-테스트)
7. [문제 해결 가이드](#문제-해결-가이드)

---

## 🎯 테스트 시스템 개요

### 테스트 구조
```
Tests/
├── Unit/                    # 단위 테스트
│   ├── SkullManagerTests.cs
│   └── SkullThrowAbilityTests.cs
├── Integration/            # 통합 테스트
│   ├── SkullSystemIntegrationTests.cs
│   └── SkullSystemPerformanceTests.cs
├── TestRunner.cs          # 통합 테스트 러너
└── Gameplay/
    ├── SkullSystemTester.cs      # 실시간 테스트 도구
    └── SkulPhysicsTestRunner.cs  # 물리 시스템 테스트
```

### 테스트 레벨
- **Level 1**: 단위 테스트 (개별 컴포넌트)
- **Level 2**: 통합 테스트 (시스템 간 연동)
- **Level 3**: 인게임 테스트 (실제 게임플레이)
- **Level 4**: 성능 테스트 (최적화 검증)

---

## 🧪 단위 테스트 (Unit Tests)

### 1. Unity Test Runner 사용

#### 설정 방법
1. **Window** → **General** → **Test Runner** 열기
2. **PlayMode** 탭 선택
3. **Run All** 또는 개별 테스트 실행

#### 주요 테스트 클래스

##### SkullManagerTests.cs
```csharp
[Test]
public void SkullManager_초기화_시_기본값_설정()
{
    // 스컬 매니저 초기 상태 검증
    Assert.AreEqual(0, skullManager.SkullCount);
    Assert.AreEqual(2, skullManager.MaxSlots);
    Assert.IsNull(skullManager.CurrentSkull);
}

[Test]
public async void SwitchToSlot_유효한_슬롯_교체_성공()
{
    // 비동기 스컬 교체 테스트
    var skull1 = CreateMockSkull(SkullType.Default);
    skullManager.AddSkullToSlot(0, skull1);

    skullManager.SetCurrentSlot(0);
    await Awaitable.NextFrameAsync();

    Assert.AreEqual(skull1, skullManager.CurrentSkull);
}
```

##### SkullThrowAbilityTests.cs
```csharp
[Test]
public void CanExecute_WhenNoActiveProjectile_ReturnsTrue()
{
    // 어빌리티 실행 가능 상태 테스트
    bool result = testAbility.CanExecute();
    Assert.IsTrue(result);
}
```

### 2. 테스트 실행 방법

#### NUnit Framework 사용
```bash
# Unity Test Runner에서
1. Test Runner 창 열기
2. PlayMode 선택
3. 개별/전체 테스트 실행
```

#### 콘솔에서 확인
```csharp
Debug.Log("[TEST] 테스트 결과 확인");
```

---

## 🔗 통합 테스트 (Integration Tests)

### 1. TestRunner.cs 사용

#### 자동 실행 방법
```csharp
// 씬에 TestRunner 프리팹 배치
// Inspector에서 설정:
runOnStart = true;              // 시작 시 자동 실행
runUnitTests = true;           // 단위 테스트 포함
runIntegrationTests = true;    // 통합 테스트 포함
runPerformanceTests = true;    // 성능 테스트 포함
```

#### 키보드 단축키
- **F1**: 단위 테스트 실행
- **F2**: 통합 테스트 실행
- **F3**: 성능 테스트 실행
- **F9**: 전체 테스트 실행
- **F10**: 결과 저장
- **F11**: 결과 초기화
- **F12**: 상세 결과 토글

### 2. 통합 테스트 시나리오

#### GAS 연동 테스트
```csharp
await SimulateTestExecution("스컬시스템_GAS_연동_테스트", "Integration", results);
await SimulateTestExecution("스컬교체_이벤트_순서_테스트", "Integration", results);
```

#### 시스템 안정성 테스트
```csharp
await SimulateTestExecution("어빌리티_실행_통합_테스트", "Integration", results);
await SimulateTestExecution("동시성_안정성_테스트", "Integration", results);
```

---

## 🎮 인게임 테스트 도구

### 1. SkullSystemTester.cs

#### 설정 방법
1. 씬에 SkullSystemTester 프리팹 배치
2. Inspector에서 테스트 대상 설정:
   ```csharp
   skullSystem = [SkullSystem 참조]
   skullManager = [SkullManager 참조]
   gasBridge = [SkullGASBridge 참조]
   ```

#### 실시간 테스트 키
- **F1**: 스컬 교체 테스트
- **F2**: 어빌리티 테스트
- **F3**: GAS 통합 테스트
- **F4**: 스컬 던지기 테스트
- **F5**: 시스템 안정성 테스트
- **F10**: 전체 테스트 실행
- **F11**: 결과 출력
- **F12**: 결과 초기화

#### 자동 테스트 시퀀스
```csharp
[ContextMenu("자동 테스트 실행")]
public void RunAutomatedTest()
{
    _ = AutomatedTestSequence();
}

// 30초마다 자동 실행
enableAutomaticTesting = true;
```

### 2. GameFlowTestScript.cs

#### GameFlow 시스템 테스트
- **UI 버튼**: 각 상태 전환 테스트
- **키보드 단축키**: F1-F5로 빠른 테스트
- **자동 시퀀스**: 전체 플로우 자동 테스트

#### 테스트 시나리오
```csharp
// F1: StartGame
gameFlowManager.StartGame();

// F2: PauseGame
gameFlowManager.PauseGame();

// F3: ResumeGame
gameFlowManager.ResumeGame();

// F4: GoToMain
gameFlowManager.GoToMainMenu();

// F5: GoToLobby
gameFlowManager.GoToLobby();
```

### 3. SkulPhysicsTestRunner.cs

#### 물리 시스템 테스트
- **1**: 기본 이동 테스트
- **2**: 점프 메커니즘 테스트
- **3**: 대시 시스템 테스트
- **4**: 벽 상호작용 테스트
- **5**: 중력과 낙하 테스트
- **6**: 코요테 타임과 점프 버퍼 테스트
- **0**: 전체 테스트 실행

---

## ⚡ 성능 테스트

### 1. SkullSystemPerformanceTests.cs

#### 측정 지표
```csharp
// 실행 시간 측정
[Test]
public async void 스컬교체_실행시간_성능_테스트()
{
    using var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "SkullManager.SwitchToSlot");

    await skullManager.SwitchToNextSlot();

    Assert.Less(recorder.LastValue, 16_000_000); // 16ms 이하
}

// 메모리 사용량 측정
[Test]
public void 스컬시스템_메모리_사용량_테스트()
{
    long beforeMemory = GC.GetTotalMemory(false);

    // 테스트 실행
    for (int i = 0; i < 100; i++)
    {
        skullManager.SwitchToNextSlotSync();
    }

    GC.Collect();
    long afterMemory = GC.GetTotalMemory(true);

    Assert.Less(afterMemory - beforeMemory, 1_000_000); // 1MB 이하
}
```

### 2. 성능 기준값

| 항목 | 기준값 | 측정 방법 |
|------|--------|----------|
| 스컬 교체 시간 | < 16ms | ProfilerRecorder |
| 어빌리티 실행 시간 | < 8ms | ProfilerRecorder |
| 메모리 사용량 | < 1MB/100회 | GC.GetTotalMemory |
| FPS 영향도 | < 5% | Application.targetFrameRate |

---

## 🤖 자동화된 테스트

### 1. CI/CD 통합

#### GitHub Actions 설정
```yaml
name: Unity Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - uses: game-ci/unity-test-runner@v2
      with:
        testMode: playmode
        artifactsPath: test-results
```

### 2. 자동 테스트 시나리오

#### 빌드 전 테스트
```csharp
// Pre-build Hook
[InitializeOnLoadMethod]
static void RunPreBuildTests()
{
    if (BuildPipeline.isBuildingPlayer)
    {
        EditorApplication.delayCall += () => {
            var testRunner = FindObjectOfType<TestRunner>();
            testRunner?.RunTests(TestSuite.All);
        };
    }
}
```

### 3. 결과 리포팅

#### JSON 형태로 저장
```csharp
var reportData = new {
    timestamp = DateTime.Now,
    totalTests = allTestResults.Count,
    passedTests = allTestResults.Count(r => r.passed),
    results = allTestResults.Select(r => new {
        testName = r.testName,
        category = r.category,
        passed = r.passed,
        executionTime = r.executionTime,
        errorMessage = r.errorMessage
    }).ToArray()
};

string json = JsonUtility.ToJson(reportData, true);
File.WriteAllText(filePath, json);
```

---

## 🔧 문제 해결 가이드

### 1. 자주 발생하는 문제들

#### Mock 객체 오류
```csharp
// 문제: MockSkullController가 없음
// 해결: MockSkullController 클래스 생성 확인

public class MockSkullController : ISkullController
{
    // 모든 인터페이스 메서드 구현 필요
}
```

#### LINQ 오류
```csharp
// 문제: Count(), ToArray() 메서드 없음
// 해결: using System.Linq; 추가

using System.Linq;
```

#### 비동기 테스트 오류
```csharp
// 문제: IEnumerator 사용
// 해결: async/await 패턴 사용

[Test]
public async void TestMethod()
{
    await Awaitable.NextFrameAsync();
    // 테스트 로직
}
```

### 2. 디버깅 팁

#### 로그 활성화
```csharp
// SkullSystemTester에서
enableDebugLogs = true;

// GameFlowTestScript에서
Debug.Log($"[GameFlowTest] {message}");
```

#### 브레이크포인트 설정
```csharp
// 중요한 지점에 브레이크포인트
await skullManager.SwitchToSlot(targetSlot); // ← 여기
Assert.AreEqual(expectedSkull, skullManager.CurrentSkull);
```

#### 상태 확인
```csharp
// 테스트 중 상태 출력
Debug.Log($"Current Skull: {skullManager.CurrentSkull?.SkullData?.SkullName}");
Debug.Log($"Slot Count: {skullManager.SkullCount}");
Debug.Log($"Can Switch: {skullManager.CanSwitch}");
```

### 3. 성능 이슈 해결

#### 프로파일러 사용
1. **Window** → **Analysis** → **Profiler** 열기
2. **Scripts** 섹션에서 스컬 시스템 메서드 확인
3. 병목 지점 식별 및 최적화

#### 메모리 누수 확인
```csharp
// 테스트 전후 메모리 비교
long beforeMemory = GC.GetTotalMemory(false);
// 테스트 실행
GC.Collect();
long afterMemory = GC.GetTotalMemory(true);
Debug.Log($"Memory diff: {afterMemory - beforeMemory} bytes");
```

---

## 📊 테스트 실행 체크리스트

### 개발 중 테스트
- [ ] 새 기능 추가 시 단위 테스트 작성
- [ ] 코드 변경 후 관련 테스트 실행
- [ ] 커밋 전 전체 테스트 실행

### 릴리즈 전 테스트
- [ ] 모든 단위 테스트 통과
- [ ] 통합 테스트 통과
- [ ] 성능 테스트 기준값 만족
- [ ] 인게임 테스트 시나리오 확인

### 정기 테스트
- [ ] 주간 자동화 테스트 실행
- [ ] 성능 지표 추적
- [ ] 테스트 커버리지 확인

---

## 🎯 결론

이 가이드를 따라 체계적으로 테스트를 수행하면:

1. **품질 보장**: 버그 조기 발견 및 수정
2. **안정성 확보**: 리팩토링 시 기능 보장
3. **성능 최적화**: 지속적인 성능 모니터링
4. **개발 효율성**: 자동화된 검증 프로세스

정기적인 테스트 실행으로 높은 품질의 스컬 시스템을 유지하세요! 🚀

---

*최종 업데이트: 2025-09-29*
*GASPT 프로젝트 - 스컬 시스템 v1.0*