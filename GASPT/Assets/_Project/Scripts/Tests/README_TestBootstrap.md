# TestBootstrap 사용 가이드

## 📖 개요

`TestBootstrap`은 테스트 씬에서 전체 게임 흐름(GameFlow) 없이 필요한 매니저와 리소스만 선택적으로 초기화하는 경량 시스템입니다.

## 🎯 목적

- ✅ 테스트에 필요한 것만 초기화
- ✅ GameFlowManager 없이 동작
- ✅ 빠른 테스트 환경 구축
- ✅ 리소스 로딩 최소화

## 🚀 사용 방법

### 1. TestBootstrap 추가

테스트 씬에 빈 GameObject를 만들고 `TestBootstrap` 컴포넌트 추가:

```
1. Hierarchy에서 우클릭 → Create Empty
2. 이름을 "TestBootstrap"으로 변경
3. Add Component → TestBootstrap
```

### 2. Inspector 설정

#### 초기화 옵션
- **Initialize Resource Manager**: 리소스 관리 시스템 (대부분 필요)
- **Initialize Audio Manager**: 오디오 시스템
- **Initialize UI Manager**: UI 관리 시스템
- **Initialize Game Manager**: 게임 상태 관리

#### 리소스 로딩 옵션
- **Categories To Load**: 자동으로 로드할 카테고리 배열
  - `Essential`: 필수 리소스
  - `MainMenu`: 메인 메뉴 리소스
  - `Gameplay`: 게임플레이 리소스
  - `Common`: 공통 리소스

- **Individual Resources**: 개별 리소스 경로 배열
  - 예: `Data/SkulPhysicsConfig`

#### 디버그
- **Show Debug Logs**: 초기화 과정 로그 출력

### 3. 예시 설정

#### PlayerCombatDemo 테스트
```
Initialize Resource Manager: ✓
Initialize Audio Manager: ✗
Initialize UI Manager: ✗
Initialize Game Manager: ✗

Categories To Load: [Essential]
Individual Resources: []
Show Debug Logs: ✓
```

이 설정은:
- GameResourceManager만 초기화
- Essential 카테고리 리소스만 로드 (SkulPhysicsConfig 포함)
- 다른 매니저는 초기화하지 않음

#### UI 테스트 씬
```
Initialize Resource Manager: ✓
Initialize Audio Manager: ✓
Initialize UI Manager: ✓
Initialize Game Manager: ✗

Categories To Load: [Essential, MainMenu]
Individual Resources: []
Show Debug Logs: ✓
```

#### 풀 게임플레이 테스트
```
Initialize Resource Manager: ✓
Initialize Audio Manager: ✓
Initialize UI Manager: ✓
Initialize Game Manager: ✓

Categories To Load: [Essential, Gameplay]
Individual Resources: []
Show Debug Logs: ✓
```

## 💡 코드에서 사용하기

### 초기화 완료 대기

```csharp
using Tests;

public class MyTest : MonoBehaviour
{
    private async void Start()
    {
        var bootstrap = FindFirstObjectByType<TestBootstrap>();

        if (bootstrap != null)
        {
            // 초기화 완료까지 대기
            while (!bootstrap.IsInitialized)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }

            Debug.Log("TestBootstrap 초기화 완료!");
        }

        // 테스트 시작
        RunTest();
    }
}
```

### 런타임에 리소스 로드

```csharp
var bootstrap = FindFirstObjectByType<TestBootstrap>();

// 개별 리소스 로드
var config = bootstrap.LoadResource<SkulPhysicsConfig>("Data/SkulPhysicsConfig");

// 카테고리 로드
await bootstrap.LoadCategory(ResourceCategory.Gameplay, destroyCancellationToken);
```

### 초기화 완료 이벤트

```csharp
var bootstrap = FindFirstObjectByType<TestBootstrap>();

bootstrap.OnInitializationComplete += () =>
{
    Debug.Log("초기화 완료!");
    RunTest();
};
```

## 📋 체크리스트

테스트 씬 설정 시:

- [ ] TestBootstrap GameObject 추가
- [ ] 필요한 매니저만 체크
- [ ] 필요한 리소스 카테고리만 선택
- [ ] 테스트 스크립트에서 `waitForTestBootstrap` 옵션 활성화
- [ ] Play 모드로 테스트

## 🔍 디버깅

### Console 로그 확인

```
[TestBootstrap] 테스트 환경 초기화 시작...
[TestBootstrap] 매니저 초기화 중...
[TestBootstrap] GameResourceManager 초기화 완료
[TestBootstrap] Essential 카테고리 로딩 중...
[GameResourceManager] Essential 카테고리 로딩 시작... (1개 리소스)
[GameResourceManager] 로드 성공: Data/SkulPhysicsConfig
[TestBootstrap] Essential 카테고리 로딩 완료
[TestBootstrap] 테스트 환경 초기화 완료!
```

### 상태 확인

Inspector에서 TestBootstrap 우클릭 → Print Status

## ⚠️ 주의사항

1. **GameFlowManager와 함께 사용하지 마세요**
   - TestBootstrap은 GameFlow 없이 동작하도록 설계됨
   - 둘 다 있으면 충돌 가능

2. **씬마다 설정이 다를 수 있습니다**
   - 각 테스트 씬의 요구사항에 맞게 설정

3. **리소스 경로 확인**
   - Individual Resources는 Resources 폴더 기준 경로 (확장자 제외)
   - 예: `Assets/_Project/Resources/Data/Config.asset` → `Data/Config`

## 🎮 실전 예제

### PlayerCombatDemo.cs

```csharp
[Header("초기화")]
[SerializeField] private bool waitForTestBootstrap = true;

private async void Start()
{
    if (waitForTestBootstrap)
    {
        var bootstrap = FindFirstObjectByType<TestBootstrap>();
        if (bootstrap != null)
        {
            while (!bootstrap.IsInitialized)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }
        }
    }

    SetupDemoScene();
}
```

이렇게 하면 TestBootstrap이 있을 때만 대기하고, 없으면 바로 시작합니다.

## 📚 관련 문서

- GameResourceManager 가이드
- ResourceManifest 설정 가이드
- GameFlow 시스템 가이드

---

**작성일**: 2025-10-03
**버전**: 1.0
**작성자**: GASPT 프로젝트팀
