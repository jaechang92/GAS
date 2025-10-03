# 🎮 GASPT 프로젝트 개요

> **GASPT**: Generic Ability System + FSM Platform Game
> Unity 2023+ 기반 2D 플랫포머 액션 게임 프레임워크

---

## 📖 프로젝트 소개

GASPT는 **Skul: The Hero Slayer** 스타일의 2D 플랫포머 액션 게임을 위한 **모듈형 게임 개발 프레임워크**입니다.

### 핵심 컨셉
- **스컬 교체 시스템**: 다양한 능력을 가진 캐릭터로 즉시 전환
- **정밀한 플랫포머 물리**: Skul 스타일의 부드러운 이동감
- **확장 가능한 전투**: 콤보 기반 액션 전투 시스템
- **모듈형 아키텍처**: 재사용 가능한 시스템 설계

---

## 🏗️ 핵심 시스템

### 1. **GAS (Gameplay Ability System)** 🎯
> 게임플레이 어빌리티를 관리하는 범용 시스템

**특징**:
- ScriptableObject 기반 데이터 관리
- 쿨다운 및 리소스 관리
- 이펙트 및 태그 시스템
- 확장 가능한 어빌리티 구조

**사용 예**:
```csharp
// 어빌리티 실행
abilitySystem.TryExecuteAbility("BasicAttack");

// 어빌리티 추가
abilitySystem.GiveAbility(new SkullThrowAbility());
```

**위치**: `Assets/Plugins/GAS_Core/`

---

### 2. **FSM (Finite State Machine)** 🔄
> 게임 객체의 상태를 관리하는 유한상태머신

**특징**:
- Unity 2023+ Awaitable 기반 비동기 처리
- 이벤트 기반 상태 전환
- Inspector 디버깅 도구 제공
- 성능 최적화된 구조

**사용 예**:
```csharp
// 상태 추가
stateMachine.AddState("Attack", new PlayerAttackState());

// 상태 전환
stateMachine.RequestTransition("Attack");
```

**위치**: `Assets/Plugins/FSM_Core/`

---

### 3. **Combat System** ⚔️
> 콤보 기반 전투 시스템

**구성 요소**:
- **DamageSystem**: 데미지 계산 및 적용
- **HealthSystem**: 체력 관리
- **ComboSystem**: 콤보 체인 관리
- **HitboxController**: 히트박스/허트박스 관리
- **AttackAnimationHandler**: 공격 애니메이션 연동

**주요 기능**:
- ✅ 3단 콤보 체인 (1→2→3)
- ✅ 콤보 윈도우 시스템
- ✅ 데미지 타입별 처리 (물리, 마법, 속성 등)
- ✅ 이벤트 기반 피드백 시스템

**위치**: `Assets/_Project/Scripts/Gameplay/Combat/`

---

### 4. **CharacterPhysics** 🏃
> Skul 스타일의 정밀한 플랫포머 물리

**특징**:
- Transform 기반 이동 (물리 엔진 미사용)
- 3가지 점프 안전장치
- 자기 자신 충돌 제외
- 코요테 타임 및 점프 버퍼
- Layer 기반 지면 감지

**구현 완료** (2025-10-04):
- ✅ 좁은 공간 점프 안정성
- ✅ 천장 충돌 시 즉시 하강
- ✅ 자동 Layer 설정 + 경고 시스템

**위치**: `Assets/_Project/Scripts/Gameplay/Player/CharacterPhysics.cs`

---

### 5. **HUD System** 🖥️
> 인게임 UI 시스템

**구성 요소**:
- **HealthBarUI**: 체력바 (보라색 게이지, lerp 애니메이션)
- **ItemSlotUI**: 아이템/스킬 슬롯 (쿨다운, 개수 표시)
- **ResourcePanel**: 골드/다이아 표시
- **PlayerInfoPanel**: 플레이어 정보 통합
- **HUDManager**: HUD 전체 관리

**자동 생성 도구**:
```
Unity 메뉴 > GASPT > UI > Create HUD Prefab
```

**위치**: `Assets/_Project/Scripts/UI/HUD/`

---

## 🎯 프로젝트 목표

### 단기 목표 (Phase 2 - 진행 중)
- ✅ Combat 시스템 완성 (70%)
- ✅ CharacterPhysics 개선 (85%)
- 🔄 스컬 교체 시스템 통합 (준비 중)
- 🔄 적 AI 시스템 구현 (계획 중)

### 중기 목표 (Phase 3)
- 🎯 절차적 레벨 생성
- 🎯 아이템 시스템
- 🎯 스컬 각성 시스템
- 🎯 다양한 스컬 타입 (10개 이상)

### 장기 목표 (Phase 4-5)
- 🎯 완성도 높은 UI/UX
- 🎯 사운드 및 VFX
- 🎯 성능 최적화
- 🎯 배포 준비

---

## 📊 현재 완성도

### 전체 진행률: **84%**

| Phase | 시스템 | 진행률 | 상태 |
|-------|-------|--------|------|
| Phase 1 | Core 시스템 (GAS + FSM) | 100% | ✅ 완료 |
| Phase 2.1 | CharacterPhysics | 85% | 🔄 진행 중 |
| Phase 2.2 | Combat System | 70% | 🔄 진행 중 |
| Phase 3 | 콘텐츠 확장 | 0% | ⏳ 대기 |
| Phase 4 | UI/UX | 40% | 🔄 진행 중 |
| Phase 5 | 최적화 및 배포 | 0% | ⏳ 대기 |

**최근 업데이트** (2025-10-04):
- ✅ 콤보 체인 시스템 구현
- ✅ CharacterPhysics 점프 안정성 강화
- ✅ ResourceManager → GameResourceManager 리팩토링
- ✅ DictionaryInspectorHelper 확장
- ✅ 8개 커밋으로 작업 정리

---

## 🔧 기술 스택

### 개발 환경
- **Unity**: 2023.3 이상
- **언어**: C# 11 with async/await
- **IDE**: Visual Studio 2022 / Rider
- **버전 관리**: Git

### 핵심 패턴
- **아키텍처**: 컴포넌트 조합 패턴 (Composition over Inheritance)
- **비동기**: `async Awaitable` (Coroutine 사용 금지)
- **설계 원칙**: SOLID, SRP (단일 책임 원칙)
- **데이터 관리**: ScriptableObject 기반

### Unity 기능
- **Physics**: Transform 기반 커스텀 물리
- **Rendering**: 2D Sprite + Sorting Layers
- **Animation**: Animator + Animation Events
- **Input**: Legacy Input (New Input System 전환 예정)

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
    └── archive/                    # 작업 히스토리
```

**상세 정보**: [폴더 구조 가이드](FolderStructure.md)

---

## 🎮 플레이 방법

### PlayerCombatDemo 실행
1. 새 씬에 `PlayerCombatDemo` 컴포넌트 추가
2. Play 버튼 클릭
3. 자동 설정 완료 (플레이어 + 적 3개)

### 조작
- **WASD**: 이동
- **Space**: 점프
- **LShift**: 대시
- **마우스 좌클릭**: 공격 (콤보)

**상세 정보**: [빠른 시작 가이드](QuickStart.md)

---

## 👥 개발 원칙

### 1. **완성 우선 원칙**
> "완벽한 시스템보다 플레이 가능한 게임을 먼저"

- 기능 추가보다 기존 기능의 완성도에 집중
- 우선 완성 후 점진적 확장

### 2. **단계적 개발 원칙**
> "작은 단위로 나누어 개발하고 지속적으로 테스트"

- 각 단계마다 플레이 가능한 상태 유지
- 기반 시스템 먼저, 콘텐츠는 나중에

### 3. **생산성 우선 원칙**
> "시스템 설계에 충분한 시간 투자"

- 재사용 가능한 컴포넌트 중심 설계
- 코드 중복 최소화 및 모듈화

### 4. **코드 품질 원칙**
> "OOP, SOLID 준수"

- 단일 책임 원칙 (SRP)
- 의존성 역전 원칙 (DIP)
- 인터페이스 분리 원칙 (ISP)

---

## 📚 참고 문서

### 시작하기
- [빠른 시작 가이드](QuickStart.md) - 5분 만에 시작
- [폴더 구조](FolderStructure.md) - 파일 위치 안내
- [플레이어 설정](PlayerSetup.md) - 캐릭터 생성 방법

### 개발 가이드
- [코딩 가이드라인](../development/CodingGuidelines.md) - 코딩 규칙
- [개발 로드맵](../development/Roadmap.md) - 전체 계획
- [Skul 시스템 설계](../development/SkulSystemDesign.md) - 시스템 상세 설계
- [현재 진행 상황](../development/CurrentStatus.md) - 최신 작업 내용

### 테스트 및 기타
- [테스트 가이드](../testing/TestingGuide.md) - 테스트 방법
- [인코딩 가이드](../infrastructure/EncodingGuide.md) - 한글 인코딩
- [작업 일지](../archive/Worklog.md) - 일별 작업 기록

---

## 🚀 다음 단계

1. **개발자**: [코딩 가이드라인](../development/CodingGuidelines.md) 숙지 → [현재 상황](../development/CurrentStatus.md) 확인
2. **테스터**: [테스트 가이드](../testing/TestingGuide.md) 확인 → PlayerCombatDemo 실행
3. **기여자**: [개발 로드맵](../development/Roadmap.md) 검토 → 기여 영역 선택

---

**프로젝트 정보**
- **개발자**: JaeChang
- **라이선스**: MIT License
- **Unity 버전**: 2023.3+
- **시작일**: 2025-09
- **최근 업데이트**: 2025-10-04
