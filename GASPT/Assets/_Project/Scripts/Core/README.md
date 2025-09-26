# 🎯 Core Scripts

이 폴더는 게임의 핵심 시스템들을 포함합니다.

## 📁 폴더 구조

```
Core/
├── Managers/                  # 게임 매니저들
│   ├── GameFlowManager.cs     # 게임 흐름 관리
│   ├── SceneManager.cs        # 씬 관리
│   └── AudioManager.cs        # 오디오 관리
└── Utilities/                 # 유틸리티 클래스들
    ├── Extensions/            # 확장 메서드들
    ├── Helpers/              # 헬퍼 클래스들
    └── Constants/            # 상수 정의
```

## 🎛️ Managers

### GameFlowManager
- 게임의 전체적인 흐름 제어
- 상태: Main, Loading, Ingame, Pause, Menu, Lobby

### SceneManager
- 씬 전환 및 로딩 관리
- 비동기 씬 로딩

### AudioManager
- BGM, SFX 관리
- 볼륨 조절, 믹싱

## 🛠️ Utilities

### Extensions
- C# 기본 타입 확장 메서드
- Unity 컴포넌트 확장 메서드

### Helpers
- 자주 사용되는 헬퍼 함수들
- 계산, 변환, 검증 유틸리티

### Constants
- 게임 전반에서 사용되는 상수들
- 태그, 레이어, 설정값

## 📋 사용 원칙

1. **재사용 가능**: 다른 프로젝트에서도 사용 가능한 범용적 코드
2. **종속성 최소화**: 외부 의존성을 최소화
3. **문서화**: 모든 공개 API는 문서화 필수