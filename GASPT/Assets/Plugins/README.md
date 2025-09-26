# 🔌 Plugins

이 폴더는 독립적인 시스템들과 외부 라이브러리를 포함합니다.

## 📁 폴더 구조

```
Plugins/
├── FSM_Core/                  # 유한상태머신 시스템
│   ├── Core/                  # FSM 핵심 로직
│   ├── Interfaces/            # FSM 인터페이스
│   ├── Utils/                # FSM 유틸리티
│   ├── Examples/             # FSM 사용 예시
│   └── Integration/          # 다른 시스템과의 통합
├── GAS_Core/                 # 게임플레이 어빌리티 시스템
│   ├── Core/                 # GAS 핵심 로직
│   ├── Abilities/            # 어빌리티 구현
│   ├── Attributes/           # 속성 시스템
│   ├── Effects/              # 이펙트 시스템
│   ├── Data/                 # 데이터 구조
│   └── Examples/             # GAS 사용 예시
└── ThirdParty/               # 서드파티 라이브러리
    ├── DOTween/              # 트위닝 라이브러리
    ├── InputSystem/          # Unity 인풋 시스템
    └── TextMeshPro/          # 텍스트 렌더링
```

## 🎯 시스템 설명

### FSM_Core - 유한상태머신 시스템
- **목적**: 게임 객체의 상태 관리
- **특징**:
  - Unity 2023+ Awaitable 지원
  - 이벤트 기반 상태 전환
  - 디버깅 도구 제공
- **사용처**: 플레이어, AI, 게임 흐름 등

### GAS_Core - 게임플레이 어빌리티 시스템
- **목적**: 복잡한 게임플레이 로직 관리
- **특징**:
  - 어빌리티 기반 설계
  - 속성 및 이펙트 시스템
  - 모듈화된 구조
- **사용처**: 스킬, 버프/디버프, 전투 시스템

## 📋 사용 원칙

### ✅ 이 폴더에 적합한 것
- **독립적 시스템**: 다른 프로젝트에서도 사용 가능
- **재사용 가능한 라이브러리**: 범용적 기능
- **서드파티 플러그인**: 외부에서 가져온 라이브러리

### ❌ 이 폴더에 부적합한 것
- **프로젝트 전용 로직**: 게임 고유의 로직
- **임시 코드**: 실험적이거나 불안정한 코드
- **UI 관련 코드**: 게임 UI 로직

## 🔧 통합 가이드

### FSM_Core 사용법
```csharp
// StateMachine 컴포넌트 추가
var stateMachine = gameObject.GetComponent<StateMachine>();
stateMachine.AddState(new MyCustomState());
stateMachine.StartStateMachine("InitialState");
```

### GAS_Core 사용법
```csharp
// AbilitySystem 컴포넌트 추가
var abilitySystem = gameObject.GetComponent<AbilitySystem>();
abilitySystem.GiveAbility(new MyCustomAbility());
```

## 📚 추가 정보
- 각 시스템의 자세한 사용법은 해당 폴더의 README와 Examples 참조
- 통합 관련 문제는 Integration 폴더의 가이드 참조