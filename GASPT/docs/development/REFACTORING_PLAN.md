# GASPT 리팩토링 계획서

> 프로젝트 확장 전 코드 품질 개선 및 기술 부채 해결

## 📋 목차
1. [리팩토링 목표](#리팩토링-목표)
2. [우선순위별 작업 목록](#우선순위별-작업-목록)
3. [단계별 실행 계획](#단계별-실행-계획)
4. [예상 효과](#예상-효과)
5. [위험 요소 및 대응](#위험-요소-및-대응)

---

## 리팩토링 목표

### 주요 목표
1. **유지보수성 향상**: 파일 분리, 책임 명확화
2. **성능 최적화**: 불필요한 의존성 제거, 메모리 관리 개선
3. **확장성 확보**: 레이어 구조 정립, 느슨한 결합
4. **코드 품질**: 중복 제거, 일관된 패턴 적용

### 비기능적 목표
- 컴파일 시간 단축 (Assembly Definition 최적화)
- 신입 프로그래머 온보딩 시간 단축
- 버그 발생률 감소

---

## 우선순위별 작업 목록

### 🔴 Phase 1: 긴급 (1-2일)

#### 1-1. GameState.cs 파일 분리
**현재 문제**:
- 모든 GameState 클래스가 하나의 파일에 500+ 줄로 존재
- 수정 시 충돌 위험 높음
- 가독성 저하

**작업 내용**:
```
1. Assets/_Project/Scripts/Core/Managers/GameStates/ 디렉토리 생성
2. 각 State를 개별 파일로 분리
   - GameStateBase.cs (추상 클래스)
   - PreloadState.cs
   - MainState.cs
   - LoadingState.cs
   - IngameState.cs
   - PauseState.cs
3. GameFlowManager.cs에서 using 추가
4. 테스트 실행
```

**예상 소요 시간**: 1시간
**위험도**: 낮음 (단순 파일 분리)

**체크리스트**:
- [ ] GameStates 디렉토리 생성
- [ ] GameStateBase.cs 분리
- [ ] PreloadState.cs 분리
- [ ] MainState.cs 분리
- [ ] LoadingState.cs 분리
- [ ] IngameState.cs 분리
- [ ] PauseState.cs 분리
- [ ] GameFlowManager.cs 업데이트
- [ ] 컴파일 테스트
- [ ] 씬 전환 테스트 (Bootstrap → Main → Gameplay)

---

#### 1-2. UI.Panels의 Combat.Core 의존성 제거
**현재 문제**:
- UI 레이어가 Gameplay 레이어에 의존 (레이어 위반)
- GameplayHUDPanel.cs가 HealthSystem 직접 참조

**작업 내용**:
```
1. Combat.Core에 이벤트 인터페이스 추가
   public interface IHealthEventProvider
   {
       event Action<float, float> OnHealthChanged;
   }

2. HealthSystem이 인터페이스 구현
   public class HealthSystem : MonoBehaviour, IHealthEventProvider

3. GameplayHUDPanel에서 Combat.Core 제거
   - Reflection 대신 인터페이스 캐스팅 사용
   - 또는 GameplayManager를 통한 중개

4. UI.Panels.asmdef에서 Combat.Core 의존성 제거
```

**예상 소요 시간**: 2시간
**위험도**: 중간 (기존 기능 유지 필요)

**체크리스트**:
- [ ] IHealthEventProvider 인터페이스 작성
- [ ] HealthSystem 인터페이스 구현
- [ ] GameplayHUDPanel 수정
- [ ] UI.Panels.asmdef 의존성 제거
- [ ] 컴파일 테스트
- [ ] 체력 UI 업데이트 테스트

---

#### 1-3. DontDestroyOnLoad 객체 관리 통합
**현재 문제**:
- BootstrapManager에서 개별적으로 DontDestroyOnLoad 호출
- 중복 체크 로직이 각 CreateManager마다 반복

**작업 내용**:
```
1. PersistentObjectManager 싱글톤 생성
   - RegisterPersistent(GameObject) 메서드
   - 중복 체크 및 로깅
   - Scene 전환 시 자동 검증

2. BootstrapManager에서 통합 사용
   - CreateManager()에서 PersistentObjectManager 사용
   - CreateEventSystem()에서 PersistentObjectManager 사용

3. 디버그 모드 추가
   - Hierarchy에서 DontDestroyOnLoad 객체 표시
   - 중복 생성 경고 로그
```

**예상 소요 시간**: 1.5시간
**위험도**: 낮음

**체크리스트**:
- [ ] PersistentObjectManager.cs 작성
- [ ] BootstrapManager 수정
- [ ] 중복 생성 테스트
- [ ] 씬 전환 시 객체 유지 확인
- [ ] 디버그 로그 확인

---

### 🟡 Phase 2: 중요 (3-5일)

#### 2-1. Panel Preloading 전략 구현
**현재 문제**:
- 모든 Panel이 Lazy Load되어 첫 Open시 지연 발생
- Loading, Pause Panel은 빠른 반응 필요

**작업 내용**:
```
1. UIManager에 Preload 설정 추가
   [SerializeField] private PanelType[] preloadPanels;

2. Bootstrap 완료 후 Preload 실행
   - Loading Panel 사전 로딩
   - Pause Panel 사전 로딩

3. 메모리 관리 정책 수립
   - 사용하지 않는 Panel 자동 Unload
   - LRU 캐시 전략 고려
```

**예상 소요 시간**: 3시간
**위험도**: 낮음

**체크리스트**:
- [ ] UIManager에 preloadPanels 필드 추가
- [ ] PreloadPanels 메서드 개선
- [ ] Bootstrap에서 Preload 호출
- [ ] 메모리 프로파일링
- [ ] Panel Open 속도 측정

---

#### 2-2. 이벤트 시스템 중앙화
**현재 문제**:
- 각 Panel마다 개별 이벤트 정의
- 이벤트 구독/해제 로직 중복

**작업 내용**:
```
1. GameEvents 정적 클래스 생성
   public static class GameEvents
   {
       // Player 이벤트
       public static event Action<float, float> OnPlayerHealthChanged;
       public static event Action<Vector3> OnPlayerMoved;

       // Combat 이벤트
       public static event Action<int> OnComboChanged;
       public static event Action<int, int> OnDamageDealt;

       // Game 이벤트
       public static event Action<int> OnScoreChanged;
       public static event Action<int> OnEnemyCountChanged;
   }

2. 각 시스템에서 GameEvents 사용
   - HealthSystem → GameEvents.OnPlayerHealthChanged?.Invoke()
   - ComboSystem → GameEvents.OnComboChanged?.Invoke()

3. UI에서 GameEvents 구독
   - GameplayHUDPanel → GameEvents 구독
```

**예상 소요 시간**: 4시간
**위험도**: 중간

**체크리스트**:
- [ ] Core.Utilities에 GameEvents.cs 작성
- [ ] 모든 게임 이벤트 정의
- [ ] HealthSystem에서 이벤트 발생
- [ ] ComboSystem에서 이벤트 발생
- [ ] GameplayHUDPanel에서 이벤트 구독
- [ ] 기존 이벤트 코드 제거
- [ ] 메모리 누수 테스트

---

#### 2-3. SceneType과 GameStateType 분리
**현재 문제**:
- SceneType과 GameStateType이 혼용
- Loading은 State이지만 씬은 아님

**작업 내용**:
```
1. SceneType 재정의 (물리적 씬)
   public enum SceneType
   {
       Bootstrap,    // 진입점
       Preload,      // 리소스 로딩
       MainMenu,     // 메인 메뉴
       Game          // 게임플레이
   }

2. GameStateType 유지 (논리적 상태)
   public enum GameStateType
   {
       Preload,      // 리소스 로딩 중
       Main,         // 메인 메뉴
       Loading,      // 게임 로딩 중 (씬은 Game)
       Ingame,       // 게임플레이 중
       Pause         // 일시정지
   }

3. 매핑 테이블 작성
   MainState → Main 씬
   LoadingState → Game 씬 로딩
   IngameState → Game 씬
   PauseState → Game 씬 (유지)
```

**예상 소요 시간**: 2시간
**위험도**: 낮음

**체크리스트**:
- [ ] SceneType 재정의
- [ ] 모든 SceneType 참조 수정
- [ ] Build Settings 씬 순서 확인
- [ ] 씬 전환 테스트

---

### 🟢 Phase 3: 개선 (5-7일)

#### 3-1. Assembly Definition 정리
**현재 문제**:
- UI.Menu, UI.HUD 어셈블리가 비어있음
- 불필요한 어셈블리로 컴파일 시간 증가

**작업 내용**:
```
1. 사용하지 않는 어셈블리 제거
   - UI.Menu.asmdef (내용 없음)
   - UI.HUD.asmdef (내용 없음)

2. 어셈블리 통합 검토
   - Combat.Attack → Combat.Core 통합 고려
   - Combat.Hitbox → Combat.Core 통합 고려

3. 의존성 그래프 최적화
   - 순환 참조 검사
   - 불필요한 의존성 제거
```

**예상 소요 시간**: 3시간
**위험도**: 낮음

**체크리스트**:
- [ ] 빈 어셈블리 파일 확인
- [ ] UI.Menu, UI.HUD 제거
- [ ] Combat 어셈블리 통합 검토
- [ ] 의존성 그래프 그리기
- [ ] 컴파일 시간 측정 (Before/After)

---

#### 3-2. Test 어셈블리 구조화
**현재 문제**:
- Unit, Demo, PlayMode 테스트가 혼재
- 테스트 실행 시간 증가

**작업 내용**:
```
1. 테스트 디렉토리 재구성
   Assets/_Project/Scripts/Tests/
   ├─ Unit/                 ← 단위 테스트 (빠름)
   │  └─ Combat/
   ├─ Integration/          ← 통합 테스트 (중간)
   │  └─ GameFlow/
   └─ PlayMode/             ← PlayMode 테스트 (느림)
      └─ Demo/

2. 각 테스트 어셈블리 분리
   - Tests.Unit.asmdef (빠른 피드백)
   - Tests.Integration.asmdef
   - Tests.PlayMode.asmdef (CI/CD에서만 실행)

3. Test Runner 설정
   - 개발 중: Unit 테스트만
   - PR 전: Integration 테스트
   - 배포 전: 모든 테스트
```

**예상 소요 시간**: 2시간
**위험도**: 낮음

**체크리스트**:
- [ ] 테스트 디렉토리 재구성
- [ ] 어셈블리 분리
- [ ] Test Runner 설정
- [ ] 각 테스트 실행 시간 측정

---

#### 3-3. 성능 프로파일링 및 최적화
**현재 문제**:
- 성능 측정 없이 개발 진행
- 병목 지점 파악 필요

**작업 내용**:
```
1. 프로파일링 포인트 설정
   - Bootstrap 시간
   - 씬 전환 시간
   - Panel Open 시간
   - FSM Update 오버헤드

2. Profiler 마커 추가
   using Unity.Profiling;
   static readonly ProfilerMarker marker = new ProfilerMarker("MyMethod");

3. 최적화 타겟 설정
   - Bootstrap < 1초
   - 씬 전환 < 2초
   - Panel Open < 0.1초

4. 메모리 프로파일링
   - Panel 메모리 사용량
   - 불필요한 참조 제거
```

**예상 소요 시간**: 4시간
**위험도**: 낮음

**체크리스트**:
- [ ] Profiler 마커 추가
- [ ] 각 구간 시간 측정
- [ ] 병목 지점 파악
- [ ] 최적화 적용
- [ ] Before/After 비교

---

## 단계별 실행 계획

### Week 1: Phase 1 (긴급)
**목표**: 레이어 구조 정립 및 기술 부채 해결

| 일차 | 작업 | 담당 | 상태 |
|------|------|------|------|
| Day 1 | 1-1. GameState.cs 파일 분리 | - | ⬜ 대기 |
| Day 2 | 1-2. UI.Panels 의존성 제거 | - | ⬜ 대기 |
| Day 2 | 1-3. DontDestroyOnLoad 통합 | - | ⬜ 대기 |

### Week 2: Phase 2 (중요)
**목표**: 성능 개선 및 확장성 확보

| 일차 | 작업 | 담당 | 상태 |
|------|------|------|------|
| Day 3 | 2-1. Panel Preloading 전략 | - | ⬜ 대기 |
| Day 4-5 | 2-2. 이벤트 시스템 중앙화 | - | ⬜ 대기 |
| Day 5 | 2-3. SceneType 분리 | - | ⬜ 대기 |

### Week 3: Phase 3 (개선)
**목표**: 코드 품질 향상 및 개발 환경 개선

| 일차 | 작업 | 담당 | 상태 |
|------|------|------|------|
| Day 6 | 3-1. Assembly Definition 정리 | - | ⬜ 대기 |
| Day 7 | 3-2. Test 어셈블리 구조화 | - | ⬜ 대기 |
| Day 7 | 3-3. 성능 프로파일링 | - | ⬜ 대기 |

---

## 예상 효과

### 코드 품질
- ✅ **가독성 향상**: GameState 파일 분리로 500+ 줄 → 100줄 이하
- ✅ **유지보수성 향상**: 레이어 구조 정립으로 수정 범위 명확화
- ✅ **확장성 확보**: 이벤트 시스템으로 느슨한 결합

### 성능
- ✅ **컴파일 시간 단축**: 불필요한 어셈블리 제거로 10-20% 개선 예상
- ✅ **런타임 성능**: Panel Preloading으로 반응 속도 향상
- ✅ **메모리 효율**: 사용하지 않는 Panel Unload

### 개발 생산성
- ✅ **신입 온보딩**: 명확한 레이어 구조로 학습 시간 단축
- ✅ **버그 감소**: 중앙화된 이벤트 시스템으로 누수 방지
- ✅ **테스트 용이성**: 구조화된 테스트로 빠른 피드백

---

## 위험 요소 및 대응

### 위험 1: 기존 기능 손상
**확률**: 중간 | **영향도**: 높음

**대응책**:
- 각 Phase 완료 후 전체 테스트 실행
- 주요 시나리오 수동 테스트 (Bootstrap → Main → Gameplay)
- Git 브랜치 전략 사용 (feature/refactoring-phase-1)

### 위험 2: 시간 초과
**확률**: 낮음 | **영향도**: 중간

**대응책**:
- Phase별 우선순위 명확화
- Phase 1만 완료해도 충분한 가치 제공
- Phase 2, 3는 점진적 적용 가능

### 위험 3: 팀원 이해 부족
**확률**: 중간 | **영향도**: 중간

**대응책**:
- 리팩토링 전 아키텍처 문서 공유 (PROJECT_ARCHITECTURE.md)
- 각 Phase 완료 후 코드 리뷰
- 변경 사항 Wiki 문서화

---

## 추가 고려사항

### 성공 기준
- [ ] 모든 Phase 1 작업 완료
- [ ] 기존 기능 100% 동작
- [ ] 컴파일 시간 10% 이상 단축
- [ ] 씬 전환 시간 2초 이하
- [ ] 메모리 누수 없음

### 후속 작업
- **Phase 4**: 게임플레이 기능 확장 준비
- **Phase 5**: CI/CD 파이프라인 구축
- **Phase 6**: 디버그 도구 개발

---

**작성일**: 2025-10-15
**버전**: 1.0
**리뷰 필요**: ⬜
