# 전체 게임 흐름 테스트 가이드

## 📋 개요

Main 씬에서 Gameplay 씬까지 전체 게임 흐름을 확인할 수 있는 통합 테스트 시스템입니다.

**게임 흐름**: `Main 씬` → `Loading 상태` → `Gameplay 씬` → `게임플레이`

---

## 🚀 빠른 시작 (3단계)

### 방법 1: 빈 씬에서 테스트

1. **새 씬 생성**
   - `File > New Scene`
   - 빈 씬으로 시작

2. **테스트 스크립트 추가**
   - Hierarchy > 우클릭 > Create Empty
   - 이름: "GameFlowTest"
   - `FullGameFlowTest` 컴포넌트 추가

3. **Play 모드 진입**
   - Play 버튼 클릭
   - 메인 메뉴 UI 자동 생성됨
   - "게임 시작" 버튼 클릭 → Gameplay 씬으로 이동

---

### 방법 2: Main 씬에서 직접 테스트

1. **Main 씬 열기**
   - `Assets/_Project/Scenes/Main.unity` 열기

2. **필수 오브젝트 추가 (없는 경우)**
   - `GameFlowManager` (DontDestroyOnLoad)
   - `SceneLoader` (DontDestroyOnLoad)
   - `MainMenuUI` (씬 UI)

3. **Play 모드**
   - "게임 시작" 버튼 클릭 → Gameplay 씬 로드

---

## 📝 씬별 구성

### Main 씬
- **GameFlowManager**: 게임 상태 관리 (FSM)
- **SceneLoader**: 씬 로드/언로드 관리
- **SceneTransitionManager**: 씬 전환 효과 (페이드)
- **MainMenuUI**: 메인 메뉴 (게임 시작, 설정, 종료 버튼)

### Loading 씬 (자동 생성)
- **LoadingUI**: 로딩 바, 진행률, 팁 표시
- 리소스 로딩 중 표시
- Gameplay 씬 로드

### Gameplay 씬
- **GameplayManager**: 게임 콘텐츠 초기화
  - 플레이어 자동 생성 (위치: 0, 1, 0)
  - 적 3개 자동 생성
  - Ground 생성
  - DamageSystem 초기화

---

## 🎮 조작 방법

### Main 씬 (메인 메뉴)
- **게임 시작 버튼**: Gameplay 씬으로 전환
- **설정 버튼**: (미구현)
- **종료 버튼**: 게임 종료

### Gameplay 씬
- **WASD**: 플레이어 이동
- **Space**: 점프
- **LShift**: 대시
- **마우스 좌클릭**: 공격 (콤보)
- **ESC**: 일시정지 (미구현)

---

## 🔍 전체 게임 흐름 상세

### 1단계: Main 씬 시작
```
GameFlowManager 초기화
  ↓
Main State 진입
  ↓
MainMenuUI 표시
```

### 2단계: 게임 시작 버튼 클릭
```
GameFlowManager.StartGame() 호출
  ↓
Loading State로 전환
  ↓
LoadingState.EnterState() 실행
```

### 3단계: 로딩 및 씬 전환
```
리소스 로딩 시뮬레이션 (1.5초)
  ↓
Gameplay 씬 로드 (SceneLoader)
  ↓
Ingame State로 전환
```

### 4단계: Gameplay 씬 진입
```
GameplayManager.Setup() 실행
  ↓
플레이어 생성
  ↓
Enemy 3개 생성
  ↓
Ground 생성
  ↓
게임플레이 시작
```

---

## 📂 주요 파일 위치

### UI 스크립트
```
Assets/_Project/Scripts/UI/Menu/
├── MainMenuUI.cs        # 메인 메뉴 UI
└── LoadingUI.cs         # 로딩 화면 UI
```

### Core 시스템
```
Assets/_Project/Scripts/Core/Managers/
├── GameFlowManager.cs      # 게임 흐름 FSM
├── SceneLoader.cs          # 씬 로딩
├── SceneTransitionManager.cs  # 씬 전환 효과
├── GameplayManager.cs      # Gameplay 씬 관리
└── GameState.cs            # 게임 상태 정의
```

### 테스트 스크립트
```
Assets/_Project/Scripts/Tests/
└── FullGameFlowTest.cs     # 통합 테스트
```

---

## ✅ 테스트 체크리스트

### Main 씬
- [ ] MainMenuUI가 표시되는가?
- [ ] "게임 시작" 버튼이 보이는가?
- [ ] 버튼 클릭이 가능한가?

### Loading 단계
- [ ] 로딩 화면이 표시되는가?
- [ ] 로딩 바가 0% → 100% 진행되는가?
- [ ] 로딩 팁이 표시되는가?
- [ ] 로딩 완료 후 자동으로 Gameplay로 전환되는가?

### Gameplay 씬
- [ ] 플레이어가 생성되는가? (파란색)
- [ ] Enemy 3개가 생성되는가? (빨간색)
- [ ] Ground가 생성되는가? (회색)
- [ ] 플레이어 조작이 가능한가? (WASD, Space, 공격)
- [ ] Enemy AI가 동작하는가? (Idle → Patrol → Chase → Attack)
- [ ] 전투 시스템이 동작하는가? (데미지, 체력)

---

## 🐛 문제 해결

### 1. "게임 시작" 버튼 클릭 시 아무 일도 일어나지 않음
**원인**: GameFlowManager가 없음
**해결**: FullGameFlowTest 스크립트 추가 → Play 모드

### 2. Gameplay 씬에서 플레이어가 생성되지 않음
**원인**: GameplayManager가 없음
**해결**: Gameplay 씬에 빈 GameObject 생성 → GameplayManager 컴포넌트 추가

### 3. 로딩 화면이 표시되지 않음
**원인**: LoadingUI가 없음
**해결**: Loading 씬에 빈 GameObject 생성 → LoadingUI 컴포넌트 추가 (또는 자동 생성 활성화)

### 4. 씬 전환 시 에러 발생
**원인**: Build Settings에 씬이 등록되지 않음
**해결**:
1. `File > Build Settings` 열기
2. 씬 추가:
   - Main.unity (Build Index: 2)
   - Loading.unity (Build Index: 3)
   - Gameplay.unity (Build Index: 4)

### 5. Layer 관련 경고
**원인**: Player/Enemy/Ground Layer가 없음
**해결**:
1. `Edit > Project Settings > Tags and Layers`
2. Layer 추가:
   - Layer 6: Player
   - Layer 7: Enemy
   - Layer 8: Ground

---

## 💡 개발 팁

### 1. 빠른 테스트
- Main 씬에서 바로 Play 모드 시작
- 콘솔 로그로 흐름 확인 가능

### 2. 디버깅
- Console 창에서 `[GameFlow]`, `[LoadingState]`, `[GameplayManager]` 로그 확인
- GameFlowManager Inspector에서 현재 상태 확인

### 3. 씬 전환 커스터마이징
- `GameState.cs`의 `LoadingState` 수정
- 로딩 시간, 페이드 효과 등 조정 가능

### 4. UI 커스터마이징
- `MainMenuUI.cs`의 `CreateUI()` 메서드 수정
- 버튼 위치, 색상, 크기 조정

---

## 📊 성능 최적화

### 로딩 시간 최소화
- `LoadingState.cs`의 시뮬레이션 시간 조정 (현재 1.5초)
- 실제 리소스 매니저 연동 시 비동기 로드 활용

### 메모리 관리
- DontDestroyOnLoad 오브젝트 최소화
- Gameplay 씬 종료 시 생성된 오브젝트 정리

---

## 🎯 다음 단계

### Phase 1: UI 개선
- [ ] 메인 메뉴 그래픽 디자인
- [ ] 설정 메뉴 구현
- [ ] 일시정지 메뉴 구현
- [ ] HUD 통합 (체력바, 아이템 슬롯)

### Phase 2: 게임플레이 확장
- [ ] 레벨 디자인
- [ ] 웨이브 시스템
- [ ] 보상 시스템
- [ ] 사운드/음악 추가

### Phase 3: 애니메이션
- [ ] 캐릭터 애니메이션
- [ ] 전투 이펙트
- [ ] UI 애니메이션

---

## 📞 참고 문서

- **프로젝트 현황**: `docs/development/CurrentStatus.md`
- **코딩 규칙**: `.spec/coding-rules.yaml`
- **시스템 아키텍처**: `.spec/architecture.yaml`
- **워크플로우**: `.spec/workflows.yaml`

---

**작성일**: 2025-10-13
**최종 업데이트**: 2025-10-13
**작성자**: GASPT 개발팀 + Claude Code

---

## 🎉 완료!

이제 Main 씬에서 Gameplay 씬까지 전체 게임 흐름을 테스트할 수 있습니다!

**시작 방법**: 빈 씬 → FullGameFlowTest 컴포넌트 추가 → Play → 게임 시작 버튼 클릭
