# 🎮 GASPT - Generic Ability System + FSM Platform Game

> Unity 2023+ 기반 2D 플랫포머 액션 게임 프레임워크
> Skul: The Hero Slayer 영감의 모듈형 게임 개발 시스템

[![Unity Version](https://img.shields.io/badge/Unity-2023.3%2B-blue)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Project Progress](https://img.shields.io/badge/Progress-84%25-brightgreen)](docs/development/CurrentStatus.md)

---

## ✨ 주요 기능

### 🎯 **GAS (Gameplay Ability System)**
확장 가능한 어빌리티 시스템으로 스킬, 버프, 디버프를 체계적으로 관리

### 🔄 **FSM (Finite State Machine)**
Unity 2023+ Awaitable 기반 고성능 상태 관리 시스템

### ⚔️ **Combat System**
콤보 체인, 히트박스, 데미지 타입을 지원하는 완성도 높은 전투 시스템

### 🏃 **CharacterPhysics**
Skul 스타일의 정밀한 플랫포머 물리 (Transform 기반, 3가지 점프 안전장치)

### 🖥️ **HUD System**
자동 생성 가능한 인게임 UI (체력바, 리소스, 아이템 슬롯)

---

## 🚀 빠른 시작

### 5분 만에 실행하기

1. **Unity에서 프로젝트 열기** (Unity 2023.3 이상)
2. **새 씬 생성** → 빈 GameObject에 `PlayerCombatDemo` 추가
3. **Play 버튼** 클릭 ▶️
4. **자동 설정 완료** (플레이어 + 적 3개 + 콤보 시스템)

### 조작 방법
- **WASD**: 이동
- **Space**: 점프
- **LShift**: 대시
- **마우스 좌클릭**: 공격 (1→2→3 콤보)

**상세 가이드**: [빠른 시작 문서](docs/getting-started/QuickStart.md)

---

## 📚 문서

### 시작하기 🎓
- [5분 빠른 시작](docs/getting-started/QuickStart.md) - 바로 시작하기
- [프로젝트 개요](docs/getting-started/ProjectOverview.md) - GASPT가 무엇인지
- [폴더 구조](docs/getting-started/FolderStructure.md) - 파일이 어디에 있는지
- [플레이어 설정](docs/getting-started/PlayerSetup.md) - 캐릭터 만들기

### 개발 문서 💻
- [현재 진행 상황](docs/development/CurrentStatus.md) - 최신 작업 내용
- [개발 로드맵](docs/development/Roadmap.md) - 전체 개발 계획
- [코딩 가이드라인](docs/development/CodingGuidelines.md) - 코딩 규칙
- [Skul 시스템 설계](docs/development/SkulSystemDesign.md) - 시스템 상세 설계

### 테스트 & 기타 🧪
- [테스트 가이드](docs/testing/TestingGuide.md) - 테스트 방법
- [인코딩 가이드](docs/infrastructure/EncodingGuide.md) - 한글 인코딩
- [작업 일지](docs/archive/Worklog.md) - 일별 작업 기록

**전체 문서 보기**: [docs/README.md](docs/README.md)

---

## 🎯 현재 상태

### 전체 진행률: **84%**

| Phase | 시스템 | 진행률 | 상태 |
|-------|-------|--------|------|
| **Phase 1** | Core 시스템 (GAS + FSM) | 100% | ✅ 완료 |
| **Phase 2.1** | CharacterPhysics | 85% | 🔄 진행 중 |
| **Phase 2.2** | Combat System | 70% | 🔄 진행 중 |
| **Phase 3** | 콘텐츠 확장 | 0% | ⏳ 대기 |
| **Phase 4** | UI/UX | 40% | 🔄 진행 중 |
| **Phase 5** | 최적화 및 배포 | 0% | ⏳ 대기 |

### 최근 업데이트 (2025-10-04) 🆕
- ✅ 콤보 체인 시스템 구현 (1→2→3 연계)
- ✅ CharacterPhysics 점프 안정성 강화 (3가지 안전장치)
- ✅ ResourceManager → GameResourceManager 리팩토링
- ✅ DictionaryInspectorHelper 확장
- ✅ 8개 커밋으로 작업 정리

**상세 정보**: [현재 진행 상황](docs/development/CurrentStatus.md)

---

## 📁 프로젝트 구조

```
GASPT/
├── Assets/
│   ├── _Project/                    # 프로젝트 전용 에셋
│   │   ├── Scripts/                 # 게임 로직
│   │   │   ├── Core/               # 매니저, 유틸리티
│   │   │   ├── Gameplay/           # 플레이어, Combat, 엔티티
│   │   │   └── UI/                 # UI 스크립트
│   │   ├── Art/                    # 아트 에셋
│   │   ├── Prefabs/                # 프리팹
│   │   └── Scenes/                 # 씬 파일
│   └── Plugins/                    # 독립 시스템
│       ├── FSM_Core/               # 유한상태머신
│       └── GAS_Core/               # 어빌리티 시스템
└── docs/                           # 프로젝트 문서
    ├── getting-started/            # 시작 가이드
    ├── development/                # 개발 문서
    ├── testing/                    # 테스트 가이드
    ├── infrastructure/             # 인프라 문서
    └── archive/                    # 작업 히스토리
```

**상세 정보**: [폴더 구조 가이드](docs/getting-started/FolderStructure.md)

---

## 🔧 기술 스택

### 개발 환경
- **Unity**: 2023.3 이상
- **언어**: C# 11 with async/await (Awaitable)
- **IDE**: Visual Studio 2022 / Rider
- **버전 관리**: Git

### 핵심 패턴
- **아키텍처**: 컴포넌트 조합 패턴 (Composition over Inheritance)
- **비동기**: `async Awaitable` (⚠️ Coroutine 사용 금지)
- **설계 원칙**: SOLID, SRP (단일 책임 원칙)
- **데이터 관리**: ScriptableObject 기반

### Unity 기능
- Transform 기반 커스텀 물리 (Physics2D 미사용)
- 2D Sprite + Sorting Layers
- Animator + Animation Events
- Legacy Input (New Input System 전환 예정)

---

## 🎮 주요 시스템

### GAS (Gameplay Ability System)
```csharp
// 어빌리티 실행
abilitySystem.TryExecuteAbility("BasicAttack");

// 어빌리티 추가
abilitySystem.GiveAbility(new SkullThrowAbility());
```

### FSM (Finite State Machine)
```csharp
// 상태 추가
stateMachine.AddState("Attack", new PlayerAttackState());

// 상태 전환
stateMachine.RequestTransition("Attack");
```

### Combat System
- ✅ 3단 콤보 체인 (1→2→3)
- ✅ 콤보 윈도우 시스템
- ✅ 데미지 타입별 처리 (물리, 마법, 속성 등)
- ✅ 이벤트 기반 피드백 시스템

### CharacterPhysics
- ✅ Skul 스타일 정밀한 플랫포머 물리
- ✅ 3가지 점프 안전장치 (접지 강제 리셋, 하강 감지, 키 릴리즈)
- ✅ 자기 자신 충돌 제외
- ✅ 코요테 타임 및 점프 버퍼

---

## 👥 개발 원칙

### 1. **완성 우선 원칙**
> "완벽한 시스템보다 플레이 가능한 게임을 먼저"

### 2. **단계적 개발 원칙**
> "작은 단위로 나누어 개발하고 지속적으로 테스트"

### 3. **생산성 우선 원칙**
> "시스템 설계에 충분한 시간 투자"

### 4. **코드 품질 원칙**
> "OOP, SOLID 준수"

**상세 정보**: [코딩 가이드라인](docs/development/CodingGuidelines.md)

---

## 🤝 기여하기

### 개발자라면
1. [코딩 가이드라인](docs/development/CodingGuidelines.md) 숙지
2. [현재 진행 상황](docs/development/CurrentStatus.md) 확인
3. [개발 로드맵](docs/development/Roadmap.md)에서 작업 선택

### 테스터라면
1. [테스트 가이드](docs/testing/TestingGuide.md) 확인
2. PlayerCombatDemo 실행
3. 발견된 버그 리포트

### 기여 규칙
- **Commit 메시지**: `feat:`, `fix:`, `docs:`, `refactor:` 등 사용
- **코드 스타일**: 카멜케이스, `_` 미사용
- **비동기 패턴**: `async Awaitable` 필수 (Coroutine 금지)
- **한글 주석**: UTF-8 인코딩 준수

---

## 📞 지원 및 문의

- **개발자**: JaeChang
- **프로젝트**: GASPT (Generic Ability System + FSM Platform)
- **Unity 버전**: 2023.3+
- **라이선스**: MIT License
- **시작일**: 2025-09
- **최근 업데이트**: 2025-10-04

---

## 📝 라이선스

이 프로젝트는 MIT 라이선스를 따릅니다. 상세 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

---

## 🎯 다음 단계

### 개발자
→ [코딩 가이드라인](docs/development/CodingGuidelines.md) 숙지
→ [현재 진행 상황](docs/development/CurrentStatus.md) 확인
→ [개발 로드맵](docs/development/Roadmap.md)에서 작업 선택

### 신규 사용자
→ [5분 빠른 시작](docs/getting-started/QuickStart.md)
→ [프로젝트 개요](docs/getting-started/ProjectOverview.md)
→ [플레이어 설정](docs/getting-started/PlayerSetup.md)

### 기여자
→ [Skul 시스템 설계](docs/development/SkulSystemDesign.md) 검토
→ [테스트 가이드](docs/testing/TestingGuide.md) 확인
→ 기여 가능한 영역 선택

---

**🎮 Happy Game Development! 🎮**

*GASPT 프로젝트 - 2025*
