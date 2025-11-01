# GASPT 프로젝트 정리 및 경량화 계획

**Feature Branch**: `002-project-cleanup`
**Created**: 2025-11-01
**Updated**: 2025-11-01 (의존성 분석 완료)
**Status**: Ready to Execute
**목적**: 프로젝트 볼륨 감소, 중복 코드 제거, 미사용 코드 정리, 포트폴리오 문서화

**의존성 분석**: ✅ 완료 (dependency-analysis-report.md 참조)

---

## 📊 현재 프로젝트 현황 분석

### 코드베이스 규모
- **전체 C# 스크립트**: 153개
- **테스트 스크립트**: 18개 (11.8%)
- **총 코드 라인**: 37,672줄

### 주요 문제점
1. **테스트 코드 과다**: 전체 코드의 약 12% 차지, 실제 필요한 테스트만 유지 필요
2. **Demo/Test 스크립트 중복**: PlayerCombatDemo, EnemyCombatDemo, FullGamePlayDemo 등 유사 기능 중복
3. **미사용 시스템**: 구현되었으나 실제 게임에서 사용하지 않는 시스템들
4. **문서 산재**: 작업 히스토리가 여러 파일에 분산되어 있어 파악 어려움
5. **Assembly 중복**: 순환 참조 해결 과정에서 생성된 불필요한 어셈블리

---

## 🎯 프로젝트 목표

### 주요 목표
1. **코드 경량화**: 전체 코드 라인 19% 감소 (37K → 30.5K) - 의존성 분석 기반 현실적 목표
2. **핵심 기능 유지**: GAS, FSM, Combat, CharacterPhysics 시스템 보존
3. **테스트 최적화**: 필수 테스트만 유지 (18개 → 3개)
4. **문서 체계화**: 포트폴리오용 작업 히스토리 통합 문서 작성
5. **유지보수성 향상**: 명확한 구조, 최소한의 의존성

### 성공 지표 (의존성 분석 기반 수정)
- ✅ 코드 라인 30,500줄 이하 (-7,200 라인, -19%)
- ✅ 스크립트 수 137개 이하 (-16개, -10%)
- ✅ 테스트 스크립트 3개 유지 (PlayerCombatDemo, TestRunner, SkullManagerTests)
- ✅ 포트폴리오 문서 6개 작성
- ✅ 컴파일 에러 0개
- ✅ 핵심 기능 100% 동작

---

## 📋 Phase 0: 핵심 기능 식별 및 보존 목록

### 반드시 보존할 핵심 시스템

#### 1. GAS (Gameplay Ability System) - 완전 보존
**위치**: `Assets/Plugins/GAS_Core/`
**이유**: 프로젝트의 핵심 어빌리티 시스템, 완전히 구현되어 있으며 다른 시스템에서 의존

**보존 파일**:
- `AbilitySystem.cs`
- `BaseAbility.cs`
- `AbilityExecutor.cs`
- `AbilityData.cs`
- Related interfaces and base classes

#### 2. FSM (Finite State Machine) - 완전 보존
**위치**: `Assets/Plugins/FSM_Core/`
**이유**: Awaitable 기반 상태 관리 시스템, GAS와 통합되어 있으며 핵심 게임 로직 담당

**보존 파일**:
- `StateMachine.cs`
- `BaseState.cs`
- `IState.cs`
- All state-related core files

#### 3. Combat System - 보존 (일부 정리)
**위치**: `Assets/_Project/Scripts/Gameplay/Combat/`
**이유**: 콤보 체인, 데미지 처리, 히트박스 시스템 포함

**보존 파일**:
- `Core/DamageSystem.cs`
- `Core/HealthSystem.cs`
- `Core/ComboSystem.cs`
- `Hitbox/HitboxController.cs`
- `Attack/BasicAttack.cs`
- `Data/DamageData.cs`, `HitData.cs`, `ComboData.cs`

**삭제 대상**:
- 미사용 공격 타입 구현체
- 중복된 테스트 데이터

#### 4. CharacterPhysics - 보존 (경량화)
**위치**: `Assets/_Project/Scripts/Gameplay/Player/Physics/`
**이유**: Skul 스타일 플랫포머 물리, 3가지 점프 안전장치 구현

**보존 파일**:
- `CharacterPhysics.cs` - 핵심 물리 시스템
- `WallDirection.cs`
- Related data classes

**정리 대상**:
- 주석 정리 및 디버그 코드 제거
- 미사용 실험적 기능 제거

#### 5. Core Managers - 보존
**위치**: `Assets/_Project/Scripts/Core/Managers/`

**보존 파일**:
- `GameFlowManager.cs` - 게임 흐름 관리
- `SceneLoader.cs` - 씬 전환
- `SceneTransitionManager.cs` - 전환 효과
- `GameResourceManager.cs` - 리소스 관리
- `SingletonManager.cs` - 싱글톤 패턴 기반

#### 6. Skull System - 보존 (정리)
**위치**: `Assets/_Project/Scripts/Gameplay/Skull/`
**이유**: 클래스 변신 시스템, 게임의 핵심 차별화 요소

**보존 파일**:
- `SkullManager.cs`
- `SkullSystem.cs`
- `BaseSkull.cs`
- `ISkullController.cs`
- 기본 Skull 구현체 (Default, Mage, Warrior)

**삭제 대상**:
- 미구현 Skull 타입 (Assassin, Tank, Rider)
- 과도한 테스트 Skull

#### 7. Player Controller - 보존
**위치**: `Assets/_Project/Scripts/Gameplay/Player/`

**보존 파일**:
- `PlayerController.cs`
- `InputHandler.cs`
- `AnimationController.cs`
- 핵심 State 클래스 (Idle, Move, Jump, Attack)

#### 8. UI System - 보존 (간소화)
**위치**: `Assets/_Project/Scripts/UI/`

**보존 파일**:
- `HUD/HUDManager.cs`
- `HUD/HealthBarUI.cs`
- 기본 메뉴 UI

**삭제 대상**:
- 미사용 UI 컴포넌트
- 실험적 UI 프로토타입

---

## 🗑️ Phase 1: 삭제할 코드 목록

### 1. 테스트 코드 정리 (우선순위: 높음)

#### 삭제할 테스트 스크립트
**위치**: `Assets/_Project/Scripts/Tests/`

**삭제 파일** (11개):
1. `FullGamePlayDemo.cs` - 중복, PlayerCombatDemo로 통합 가능
2. `FullGameFlowTest.cs` - 게임 플로우 테스트, 필요시 수동 테스트로 대체
3. `ComprehensiveTestRunner.cs` - 과도한 자동화, TestRunner.cs로 충분
4. `Tests/Demo/EnemyCombatDemo.cs` - PlayerCombatDemo에 적 생성 기능 있음
5. `Tests/Demo/CombatDemoScene.cs` - 씬 기반 데모, 불필요
6. `Tests/Demo/CombatTestUI.cs` - 테스트 UI, 디버그 모드로 대체 가능
7. `Tests/Unit/SkullThrowAbilityTests.cs` - 미구현 기능 테스트
8. `Tests/Unit/Combat/HitboxSystemTests.cs` - 중복 테스트
9. `Tests/Integration/SkullSystemIntegrationTests.cs` - 과도한 통합 테스트
10. `Tests/Performance/SkullSystemPerformanceTests.cs` - 최적화 단계에서 필요시 재작성
11. `SceneBootstrap.cs` - 불필요한 부트스트랩, GameFlowManager로 충분

**보존할 테스트** (3개):
- `TestRunner.cs` - 통합 테스트 러너 (필수)
- `Tests/Unit/SkullManagerTests.cs` - 핵심 시스템 테스트
- `Tests/Demo/PlayerCombatDemo.cs` - 메인 데모 (개선)

**예상 절감**: ~4,000 라인

### 2. Mocks 및 Test Utilities 정리

**삭제 파일**:
- `Tests/Mocks/MockSkullController.cs` - SkullManagerTests.cs 내부로 이동
- `Tests/TestConfiguration.cs` - 설정 하드코딩으로 대체

**예상 절감**: ~800 라인

### 3. 미구현 Skull 타입 제거

**위치**: `Assets/_Project/Scripts/Gameplay/Skull/Implementation/`

**삭제할 Skull 구현체** (스텁만 있고 실제 기능 없음):
- `AssassinSkull.cs`
- `TankSkull.cs`
- `RiderSkull.cs`

**보존할 Skull 구현체**:
- `DefaultSkull.cs`
- `MageSkull.cs`
- `WarriorSkull.cs`

**이유**: 실제 구현되지 않은 클래스들로 혼란만 가중, 필요시 나중에 추가

**예상 절감**: ~600 라인

### 4. 중복/미사용 Data 클래스 정리

**위치**: `Assets/_Project/Scripts/Gameplay/Combat/Data/`

**검토 후 삭제 대상**:
- 중복된 Damage 타입 정의
- 미사용 Combo 데이터
- 실험적 데이터 구조

**예상 절감**: ~500 라인

### 5. Enemy 시스템 간소화

**위치**: `Assets/_Project/Scripts/Gameplay/Enemy/`

**현재 상태**: 기본 Enemy 구현만 있고 실제 AI나 다양한 적 타입 없음

**조치**:
- 기본 EnemyController만 유지
- 미구현 Enemy State 클래스 제거
- EnemyData를 간단한 구조로 변경

**예상 절감**: ~1,200 라인

### 6. 디버그 및 개발 도구 정리

**위치**: Various locations

**삭제 대상**:
- 과도한 Debug.Log 문구
- 사용하지 않는 Gizmo 그리기 코드
- 실험적 에디터 툴

**예상 절감**: ~800 라인

---

## 📝 Phase 2: 코드 경량화 및 통합

### 1. PlayerCombatDemo 개선 및 통합

**목표**: 하나의 Demo 스크립트로 모든 핵심 기능 시연

**통합할 기능**:
- 기존 PlayerCombatDemo 기능
- EnemyCombatDemo의 적 생성 기능
- FullGamePlayDemo의 게임 플로우 기능

**개선 사항**:
- 키보드 단축키로 시연 모드 전환
- F1: 기본 전투 데모
- F2: 스컬 변경 데모
- F3: 물리 시스템 데모 (벽 점프, 플랫폼)
- F4: 콤보 시스템 데모

**예상 결과**: 5개의 Demo 스크립트 → 1개로 통합

### 2. 주석 및 문서화 정리

**조치**:
- 과도한 한글 주석 정리 (명확한 코드 작성으로 대체)
- XML 문서 주석은 public API만 유지
- 각 클래스 상단에 간단한 설명만 유지

**기준**:
- Public API: XML 주석 필수
- Private 메서드: 복잡한 로직만 주석
- 변수: 이름이 명확하면 주석 불필요

**예상 절감**: ~1,500 라인

### 3. CharacterPhysics 경량화

**현재 문제**: 과도한 디버그 코드와 실험적 기능 포함

**조치**:
- 디버그 Gizmo 그리기 조건부 컴파일로 변경 (`#if UNITY_EDITOR`)
- 미사용 실험적 기능 제거
- 주석 정리

**예상 절감**: ~400 라인

### 4. Assembly Definition 정리

**현재 상태**:
- 많은 Assembly 파일이 순환 참조 해결 과정에서 생성됨
- 일부는 불필요하게 세분화되어 있음

**조치**:
- 꼭 필요한 Assembly만 유지
- 작은 Assembly는 통합 검토
- 의존성 그래프 단순화

**목표 Assembly 구조**:
```
GAS.Core (독립)
FSM.Core (독립)
Core.Utilities (독립)
Core.Managers (→ FSM.Core, Core.Utilities)
Gameplay.Common (공통 인터페이스/데이터)
Player (→ FSM.Core, GAS.Core, Gameplay.Common)
Combat (→ GAS.Core, Gameplay.Common)
Skull (→ FSM.Core, GAS.Core, Gameplay.Common)
```

---

## 📚 Phase 3: 포트폴리오 문서화

### 목표
1. **통합 작업 히스토리**: 모든 개발 과정을 시간순으로 정리
2. **기술 스택 명세**: 사용한 기술과 선택 이유
3. **핵심 성과물**: 구현한 시스템별 상세 설명
4. **문제 해결 사례**: 주요 기술적 도전과 해결 방법
5. **프로젝트 구조**: 최종 아키텍처 다이어그램

### 문서 구조

#### 1. 프로젝트 개요 문서
**파일**: `docs/portfolio/PROJECT_OVERVIEW.md`

**내용**:
- 프로젝트명: GASPT (Generic Ability System + FSM Platform)
- 개발 기간: 2025-09 ~ 2025-11 (3개월)
- 개발 인원: 1인 (JaeChang)
- 게임 장르: 2D 플랫포머 액션 (Skul: The Hero Slayer 영감)
- 개발 환경: Unity 2023.3+, C# 11
- 프로젝트 규모: 최종 ~26,000 라인, 130+ 스크립트

#### 2. 기술 스택 문서
**파일**: `docs/portfolio/TECHNICAL_STACK.md`

**내용**:
- **핵심 시스템**:
  - GAS (Gameplay Ability System): 범용 어빌리티 시스템
  - FSM (Finite State Machine): Awaitable 기반 상태 관리
  - Combat System: 콤보 체인 및 데미지 처리
  - CharacterPhysics: Transform 기반 커스텀 물리
  - Skull System: 클래스 변신 시스템

- **기술적 특징**:
  - Unity 2023+ Awaitable 패턴 (Coroutine 대체)
  - SOLID 원칙 준수
  - ScriptableObject 기반 데이터 관리
  - Assembly Definition으로 모듈화
  - 한글 주석 및 변수명 지원 (UTF-8)

- **아키텍처 패턴**:
  - Composition over Inheritance
  - Singleton Pattern (Manager 클래스)
  - State Pattern (FSM)
  - Command Pattern (Ability System)
  - Observer Pattern (Event System)

#### 3. 개발 히스토리 문서
**파일**: `docs/portfolio/DEVELOPMENT_HISTORY.md`

**구조**:
```markdown
# GASPT 개발 히스토리

## Phase 1: Core 시스템 구축 (2025-09-01 ~ 2025-09-15)

### Week 1: GAS (Gameplay Ability System) 개발
- **작업 내용**: 범용 어빌리티 시스템 설계 및 구현
- **구현 기능**:
  - BaseAbility 추상 클래스
  - AbilitySystem 실행 엔진
  - AbilityData ScriptableObject
  - 어빌리티 쿨다운 및 코스트 시스템
- **기술적 도전**:
  - 어빌리티 체인 시스템 구현
  - 동시 실행 어빌리티 관리
- **해결 방법**:
  - async/await 패턴으로 비동기 어빌리티 처리
  - AbilityQueue를 통한 우선순위 관리
- **커밋 히스토리**:
  - `c28c63c`: feat: GAS Core 시스템 기본 구조 구축
  - `5f05344`: feat: 어빌리티 실행 엔진 구현

### Week 2: FSM (Finite State Machine) 개발
[...]

## Phase 2: Gameplay 시스템 구현 (2025-09-16 ~ 2025-10-15)

### Week 3: CharacterPhysics 시스템
[...]

### Week 4: Combat System
[...]

## Phase 3: 통합 및 최적화 (2025-10-16 ~ 2025-11-01)

### Week 9: 순환 참조 해결 및 Assembly 정리
- **문제**: Player ↔ Skull 순환 참조
- **해결**: Gameplay.Common 공통 Assembly 생성
[...]

### Week 11: 프로젝트 정리 및 경량화
[...]
```

#### 4. 핵심 기능 명세서
**파일**: `docs/portfolio/CORE_FEATURES.md`

**각 시스템별 상세 설명**:
```markdown
# GASPT 핵심 기능 명세

## 1. GAS (Gameplay Ability System)

### 개요
범용 게임플레이 어빌리티 시스템으로, 스킬, 버프, 디버프를 체계적으로 관리

### 핵심 클래스
- `AbilitySystem`: 어빌리티 실행 엔진
- `BaseAbility`: 모든 어빌리티의 기본 클래스
- `AbilityData`: 어빌리티 데이터 정의

### 사용 예제
\`\`\`csharp
// 어빌리티 실행
abilitySystem.TryExecuteAbility("BasicAttack");

// 어빌리티 추가
abilitySystem.GiveAbility(new SkullThrowAbility());
\`\`\`

### 구현 세부사항
- async/await 기반 비동기 실행
- 쿨다운 및 코스트 관리
- 어빌리티 체인 시스템

[각 시스템마다 동일한 형식으로 작성]
```

#### 5. 기술적 도전 및 해결 사례
**파일**: `docs/portfolio/TECHNICAL_CHALLENGES.md`

**주요 사례**:
1. **Coroutine → Awaitable 전환**
   - 문제: Unity 2023+ 최신 패턴 적용 필요
   - 해결: 전체 프로젝트에서 Coroutine 제거, async/await로 전환
   - 성과: 코드 가독성 향상, 예외 처리 개선

2. **순환 참조 해결**
   - 문제: Player ↔ Skull 어셈블리 순환 참조
   - 해결: Gameplay.Common 공통 어셈블리 생성, 인터페이스 분리
   - 성과: 깔끔한 의존성 구조

3. **Transform 기반 물리 시스템**
   - 문제: Skul 스타일의 정밀한 플랫포머 물리 구현
   - 해결: Rigidbody2D 대신 Transform 기반 커스텀 물리
   - 성과: 더 정밀한 제어, 3가지 점프 안전장치 구현

[더 많은 사례들...]

#### 6. 최종 아키텍처 문서
**파일**: `docs/portfolio/FINAL_ARCHITECTURE.md`

**내용**:
- 전체 시스템 다이어그램
- Assembly 의존성 그래프
- 핵심 클래스 관계도
- 데이터 흐름 다이어그램

---

## 🚀 Phase 4: 실행 계획

### Step 1: 백업 및 브랜치 생성 (1일차)
```bash
# 현재 상태 백업
git commit -am "backup: Project state before cleanup"
git tag backup-before-cleanup

# 새 작업 브랜치 생성
git checkout -b 002-project-cleanup
```

### Step 2: 테스트 코드 삭제 (2일차)
**우선순위**: 높음
**예상 시간**: 2시간

**작업 순서**:
1. 보존할 테스트 확인 (PlayerCombatDemo, TestRunner, SkullManagerTests)
2. 삭제 대상 11개 파일 삭제
3. 컴파일 확인
4. PlayerCombatDemo 개선 (F1-F4 단축키 추가)

### Step 3: 미구현 Skull 및 Enemy 정리 (2일차)
**예상 시간**: 1시간

**작업 순서**:
1. AssassinSkull, TankSkull, RiderSkull 삭제
2. Enemy State 클래스 정리
3. 관련 Data 파일 정리
4. 컴파일 확인

### Step 4: 코드 경량화 (3일차)
**예상 시간**: 3시간

**작업 순서**:
1. CharacterPhysics 디버그 코드 정리
2. 과도한 주석 제거
3. Debug.Log 정리
4. Assembly Definition 검토 및 통합

### Step 5: 컴파일 및 테스트 (3일차)
**예상 시간**: 1시간

**확인 사항**:
- 컴파일 에러 0개
- PlayerCombatDemo 동작 확인
- 핵심 기능 테스트
- 씬 로드 테스트

### Step 6: 포트폴리오 문서 작성 (4-5일차)
**예상 시간**: 8시간

**작업 순서**:
1. PROJECT_OVERVIEW.md 작성
2. TECHNICAL_STACK.md 작성
3. DEVELOPMENT_HISTORY.md 작성 (시간 투자 필요)
4. CORE_FEATURES.md 작성
5. TECHNICAL_CHALLENGES.md 작성
6. FINAL_ARCHITECTURE.md 작성

### Step 7: 최종 검토 및 커밋 (5일차)
**예상 시간**: 2시간

**확인 사항**:
- 모든 문서 검토
- 코드 정리 확인
- 최종 테스트
- Git 커밋 히스토리 정리

```bash
# 작업 커밋
git add -A
git commit -m "refactor: Project cleanup and portfolio documentation

- Removed 11 unnecessary test scripts (~4,000 lines)
- Removed unimplemented Skull types (Assassin, Tank, Rider)
- Simplified Enemy system
- Cleaned up debug code and comments
- Optimized Assembly Definitions
- Created comprehensive portfolio documentation

Total reduction: ~11,000 lines (30% decrease)
Final size: ~26,000 lines, 130+ scripts"
```

---

## 📊 예상 성과

### 코드 감소량
| 항목 | Before | After | 감소량 |
|------|--------|-------|--------|
| 총 라인 수 | 37,672 | ~26,000 | -11,672 (-31%) |
| 스크립트 수 | 153 | ~130 | -23 (-15%) |
| 테스트 스크립트 | 18 | 3 | -15 (-83%) |
| Assembly 파일 | 12 | 8 | -4 (-33%) |

### 유지보수성 개선
- ✅ 핵심 시스템만 남아 구조 파악 용이
- ✅ 테스트 코드 최소화로 빠른 반복 개발 가능
- ✅ 명확한 Assembly 의존성
- ✅ 통합 Demo로 빠른 기능 시연

### 포트폴리오 가치
- ✅ 완성도 높은 문서화
- ✅ 기술적 도전과 해결 사례 정리
- ✅ 명확한 개발 히스토리
- ✅ 코드 품질 및 아키텍처 명확히 전달

---

## ⚠️ 주의사항

### 삭제 전 반드시 확인
1. **의존성 확인**: 삭제할 파일이 다른 곳에서 참조되는지 확인
2. **기능 동작 확인**: 삭제 후 핵심 기능이 정상 동작하는지 테스트
3. **Git 커밋**: 단계별로 커밋하여 롤백 가능하도록 유지

### 백업 전략
- 현재 상태를 Git tag로 백업
- 각 Phase마다 별도 커밋
- 롤백 가능한 상태 유지

### Constitution 준수
이 작업은 GASPT Constitution의 다음 원칙을 따릅니다:
- **Principle I (Completion-First)**: 핵심 기능 유지하며 정리
- **Principle III (Productivity-First)**: 장기 유지보수성 향상
- **Principle VIII (Token Efficiency)**: 코드 경량화로 AI 도구 효율성 향상

---

## 📅 일정

| Phase | 작업 | 예상 시간 | 일정 |
|-------|------|-----------|------|
| Phase 0 | 현황 분석 및 계획 | 2시간 | Day 1 |
| Phase 1 | 테스트 코드 삭제 | 2시간 | Day 2 |
| Phase 1 | Skull/Enemy 정리 | 1시간 | Day 2 |
| Phase 2 | 코드 경량화 | 3시간 | Day 3 |
| Phase 2 | 컴파일 및 테스트 | 1시간 | Day 3 |
| Phase 3 | 포트폴리오 문서 | 8시간 | Day 4-5 |
| Phase 4 | 최종 검토 및 커밋 | 2시간 | Day 5 |
| **합계** | | **19시간** | **5일** |

---

## ✅ 체크리스트

### Phase 0: 준비
- [ ] 현재 프로젝트 상태 백업 (Git tag)
- [ ] 브랜치 생성 (002-project-cleanup)
- [ ] 핵심 기능 목록 확인
- [ ] Constitution 검토

### Phase 1: 코드 삭제
- [ ] 테스트 스크립트 11개 삭제
- [ ] Mock 파일 정리
- [ ] 미구현 Skull 타입 3개 삭제
- [ ] Enemy 시스템 간소화
- [ ] 컴파일 에러 해결
- [ ] 기능 동작 확인

### Phase 2: 경량화
- [ ] CharacterPhysics 디버그 코드 정리
- [ ] 주석 정리
- [ ] Debug.Log 정리
- [ ] Assembly Definition 통합
- [ ] PlayerCombatDemo 개선 (F1-F4)
- [ ] 최종 컴파일 및 테스트

### Phase 3: 문서화
- [ ] PROJECT_OVERVIEW.md 작성
- [ ] TECHNICAL_STACK.md 작성
- [ ] DEVELOPMENT_HISTORY.md 작성
- [ ] CORE_FEATURES.md 작성
- [ ] TECHNICAL_CHALLENGES.md 작성
- [ ] FINAL_ARCHITECTURE.md 작성
- [ ] README.md 업데이트

### Phase 4: 완료
- [ ] 모든 문서 검토
- [ ] 최종 테스트 (모든 핵심 기능)
- [ ] Git 커밋 (의미 있는 메시지)
- [ ] 작업 완료 확인

---

**작성일**: 2025-11-01
**작성자**: AI Assistant (Claude Code)
**승인 대기**: JaeChang
