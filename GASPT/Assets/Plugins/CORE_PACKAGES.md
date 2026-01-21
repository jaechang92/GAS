# GASPT Core Packages

GASPT 프로젝트에서 추출한 재사용 가능한 핵심 패키지 목록

## 패키지 개요

| 패키지 | 설명 | 의존성 |
|--------|------|--------|
| GAS_Core | 게임플레이 어빌리티 시스템 | 없음 |
| FSM_Core | 유한 상태 머신 | 없음 |
| MVP_Core | UI MVP 패턴 프레임워크 | TextMeshPro |
| Singleton_Core | 싱글톤 매니저 패턴 | 없음 |
| ObjectPool_Core | 오브젝트 풀링 시스템 | 없음 |
| SaveSystem_Core | JSON 저장 시스템 | 없음 |

---

## 1. GAS_Core

**경로**: `Assets/Plugins/GAS_Core/`

**설명**: Unity용 게임플레이 어빌리티 시스템 (Unreal GAS 패턴 기반)

**주요 기능**:
- AbilityBase: 어빌리티 기본 클래스
- AbilitySpec: 어빌리티 인스턴스 관리
- AbilitySystem: 중앙 어빌리티 관리자
- GameplayEffect: 버프/디버프 효과 시스템
- GameplayTag: 태그 기반 상태 관리

**사용 예시**:
```csharp
// 어빌리티 시스템 초기화
var abilitySystem = GetComponent<AbilitySystem>();

// 어빌리티 부여
abilitySystem.GiveAbility(jumpAbility);

// 어빌리티 활성화
abilitySystem.TryActivateAbility<JumpAbility>();
```

---

## 2. FSM_Core

**경로**: `Assets/Plugins/FSM_Core/`

**설명**: 범용 유한 상태 머신 프레임워크

**주요 기능**:
- StateMachine: 상태 머신 기본 클래스
- IState: 상태 인터페이스
- 자동 상태 전환
- Enter/Exit/Update 생명주기

**사용 예시**:
```csharp
public class PlayerFSM : StateMachine
{
    protected override void SetupStates()
    {
        AddState(new IdleState());
        AddState(new MoveState());
        SetInitialState<IdleState>();
    }
}
```

---

## 3. MVP_Core

**경로**: `Assets/MVP_Core/`

**설명**: Unity UI를 위한 MVP 패턴 프레임워크

**주요 기능**:
- ViewBase/ToggleableViewBase: 뷰 기본 클래스
- PresenterBase: 프레젠터 기본 클래스
- ViewModelBase: 불변 ViewModel 패턴
- UIAnimationHelper: Awaitable 기반 UI 애니메이션
- UIExtensions: 확장 메서드 (Flash, SetFillSmooth 등)

**사용 예시**:
```csharp
// ViewModel
public record HealthBarViewModel(
    float CurrentValue,
    float MaxValue,
    float FillAmount
) : ViewModelBase;

// Presenter
public class HealthBarPresenter : PresenterBase<HealthView, HealthBarViewModel>
{
    protected override void OnModelChanged(HealthBarViewModel model)
    {
        view.UpdateHealth(model.FillAmount);
    }
}
```

---

## 4. Singleton_Core

**경로**: `Assets/Singleton_Core/`

**설명**: Unity용 싱글톤 매니저 패턴

**주요 기능**:
- SingletonManager<T>: 씬 전환 유지 싱글톤
- SceneSingletonManager<T>: 씬 전용 싱글톤
- 자동 인스턴스 생성
- 중복 생성 방지

**사용 예시**:
```csharp
public class GameManager : SingletonManager<GameManager>
{
    protected override void OnSingletonAwake()
    {
        // 초기화 로직
    }
}

// 사용
GameManager.Instance.StartGame();
```

---

## 5. ObjectPool_Core

**경로**: `Assets/ObjectPool_Core/`

**설명**: 제네릭 오브젝트 풀링 시스템

**주요 기능**:
- ObjectPool<T>: 제네릭 오브젝트 풀
- PoolManager: 중앙 풀 관리자
- IPoolable: 풀링 콜백 인터페이스
- 자동 풀 확장

**사용 예시**:
```csharp
// 풀 생성
var bulletPool = new ObjectPool<Bullet>(prefab, transform, 20);

// 오브젝트 가져오기
var bullet = bulletPool.Get(spawnPos, rotation);

// 반환
bulletPool.Release(bullet);
```

---

## 6. SaveSystem_Core

**경로**: `Assets/SaveSystem_Core/`

**설명**: JSON 기반 저장 시스템

**주요 기능**:
- JsonUtility 기반 직렬화
- 저장/로드 이벤트
- 파일 관리 (존재 확인, 삭제, 정보)
- SaveDataBase: 저장 데이터 기본 클래스

**사용 예시**:
```csharp
// 저장 데이터 정의
[Serializable]
public class GameData : SaveDataBase
{
    public int level;
    public int score;
}

// 저장
var data = new GameData { level = 5, score = 1000 };
SaveSystem.Instance.Save(data, "save.json");

// 로드
var loaded = SaveSystem.Instance.LoadOrCreate<GameData>("save.json");
```

---

## 다른 프로젝트에서 사용하기

### 방법 1: 폴더 복사
1. 원하는 `*_Core` 폴더를 새 프로젝트의 `Assets/` 폴더에 복사
2. 필요한 의존성 설치 (MVP_Core의 경우 TextMeshPro)
3. 네임스페이스 import 후 사용

### 방법 2: Unity Package Manager
1. 각 패키지를 별도 Git 저장소로 분리
2. Package Manager에서 Git URL로 추가

---

## 패키지 의존성 다이어그램

```
┌─────────────────────────────────────────────────────────┐
│                    GASPT Project                         │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐ │
│  │  GAS_Core   │    │  FSM_Core   │    │  MVP_Core   │ │
│  │  (독립)     │    │  (독립)     │    │ (TMP 필요) │ │
│  └─────────────┘    └─────────────┘    └─────────────┘ │
│                                                         │
│  ┌─────────────┐    ┌───────────────┐  ┌─────────────┐ │
│  │Singleton_   │    │ ObjectPool_   │  │ SaveSystem_ │ │
│  │   Core      │    │     Core      │  │    Core     │ │
│  │  (독립)     │    │   (독립)      │  │   (독립)    │ │
│  └─────────────┘    └───────────────┘  └─────────────┘ │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 버전 정보

| 패키지 | 버전 | 최종 수정 |
|--------|------|----------|
| GAS_Core | 1.0 | 2025-01 |
| FSM_Core | 1.0 | 2025-01 |
| MVP_Core | 1.0 | 2025-01 |
| Singleton_Core | 1.0 | 2025-01 |
| ObjectPool_Core | 1.0 | 2025-01 |
| SaveSystem_Core | 1.0 | 2025-01 |

---

## 라이선스

MIT License
