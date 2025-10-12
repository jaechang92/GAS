# 씬 설정 가이드 (Scene Setup Guide)

## 📋 목차
1. [개요](#개요)
2. [자동 설정 도구 사용](#자동-설정-도구-사용)
3. [수동 설정 방법](#수동-설정-방법)
4. [테스트 방법](#테스트-방법)
5. [문제 해결](#문제-해결)

---

## 개요

GASPT 프로젝트는 씬 기반 아키텍처를 사용합니다. 이 가이드는 필요한 모든 씬을 설정하는 방법을 설명합니다.

### 필요한 씬 목록

| 씬 이름 | Build Index | 역할 |
|---------|-------------|------|
| Bootstrap | 0 | 게임 진입점, 매니저 초기화 |
| Preload | 1 | 필수 리소스 로딩 |
| Main | 2 | 메인 메뉴 |
| Loading | 3 | 로딩 화면 (Additive) |
| Gameplay | 4 | 실제 게임플레이 |
| Pause | 5 | 일시정지 메뉴 (Additive) |

---

## 자동 설정 도구 사용 (권장)

### 방법 1: 메뉴를 통한 빠른 생성

#### 1단계: 메뉴 접근
Unity 에디터 상단 메뉴에서:
```
GASPT → Scene Setup → Create All Scenes
```

이 메뉴를 클릭하면 모든 씬이 자동으로 생성되고 Build Settings에 추가됩니다.

#### 2단계: 완료 확인
- `Assets/_Project/Scenes/` 폴더에 6개의 씬 파일이 생성됨
- `File → Build Settings`에서 씬 목록 확인

---

### 방법 2: Scene Setup Tool 윈도우 사용

#### 1단계: Scene Setup Tool 열기
Unity 에디터 상단 메뉴에서:
```
GASPT → Scene Setup → Open Scene Setup Tool
```

#### 2단계: 옵션 선택
Scene Setup Tool 윈도우가 열립니다:

```
┌─────────────────────────────────────┐
│   씬 자동 생성 및 설정 도구         │
├─────────────────────────────────────┤
│                                     │
│ 생성할 씬 선택                      │
│ ☑ Bootstrap (진입점)                │
│ ☑ Preload (초기 로딩)               │
│ ☑ Main (메인 메뉴)                  │
│ ☑ Loading (로딩 화면)               │
│ ☑ Gameplay (게임플레이)             │
│ ☑ Pause (일시정지)                  │
│                                     │
│ 옵션                                │
│ ☑ Build Settings에 추가             │
│ ☑ 씬 오브젝트 자동 설정             │
│                                     │
│ [  모든 씬 생성  ] [Build Settings] │
│                                     │
│ [    씬 폴더 열기     ]             │
└─────────────────────────────────────┘
```

**옵션 설명:**
- **생성할 씬 선택**: 생성하고 싶은 씬만 체크
- **Build Settings에 추가**: 체크하면 자동으로 Build Settings에 추가
- **씬 오브젝트 자동 설정**: 체크하면 각 씬에 필요한 기본 오브젝트 자동 생성

#### 3단계: 씬 생성
1. 원하는 옵션 선택
2. **"모든 씬 생성"** 버튼 클릭
3. 확인 다이얼로그에서 **"생성"** 클릭

#### 4단계: 결과 확인
완료 다이얼로그가 표시됩니다:
```
씬 생성 완료
6개의 씬이 생성되었습니다.
위치: Assets/_Project/Scenes
```

---

## 자동 생성되는 씬 내용

### Bootstrap.unity
```
Bootstrap (Scene)
├─ BootstrapManager (GameObject)
│  └─ BootstrapManager (Component)
│     ├─ Auto Start Game: ✓
│     └─ Initial Scene: Preload
└─ Directional Light
```

**역할:**
- 게임 시작 시 첫 번째로 로드되는 씬
- SceneLoader, SceneTransitionManager 등 핵심 매니저 초기화
- 자동으로 Preload 씬으로 전환

---

### Preload.unity
```
Preload (Scene)
├─ Main Camera
└─ Directional Light
```

**역할:**
- Essential + MainMenu 리소스 로딩
- 최소 2초 로딩 화면 표시
- 완료 후 Main 씬으로 전환

---

### Main.unity
```
Main (Scene)
├─ Main Camera
├─ Directional Light
├─ EventSystem
│  ├─ EventSystem (Component)
│  └─ StandaloneInputModule (Component)
└─ MainMenuUI (Canvas)
   ├─ Canvas
   ├─ CanvasScaler
   └─ GraphicRaycaster
```

**역할:**
- 메인 메뉴 UI 표시
- 게임 시작, 설정, 종료 버튼

**TODO:**
- MainMenuUI 하위에 UI 요소 추가 필요

---

### Loading.unity
```
Loading (Scene)
└─ LoadingScreenUI (Canvas)
   ├─ Canvas (sortingOrder: 100)
   ├─ CanvasScaler
   └─ GraphicRaycaster
```

**역할:**
- Additive로 로드되는 로딩 화면
- Gameplay 리소스 로딩 중 표시

**TODO:**
- 로딩바, 진행률 텍스트, 팁 텍스트 추가 필요

---

### Gameplay.unity
```
Gameplay (Scene)
├─ Main Camera
├─ Directional Light
├─ EventSystem
├─ Ground (Platform)
│  ├─ BoxCollider2D (30 x 2)
│  └─ SpriteRenderer
├─ SpawnPoints
│  ├─ PlayerSpawn (위치: -8, 2, 0)
│  ├─ Enemy1Spawn (위치: 5, 2, 0)
│  └─ Enemy2Spawn (위치: 10, 2, 0)
└─ IngameUI (Canvas)
```

**역할:**
- 실제 게임플레이
- Player, Enemy 캐릭터 생성 및 전투

**TODO:**
- IngameUI에 체력바, 점수 등 UI 요소 추가

---

### Pause.unity
```
Pause (Scene)
└─ PauseMenuUI (Canvas)
   ├─ Canvas (sortingOrder: 50)
   ├─ CanvasScaler
   └─ GraphicRaycaster
```

**역할:**
- Additive로 로드되는 일시정지 메뉴
- ESC 키로 활성화/비활성화

**TODO:**
- Resume, Settings, Main Menu 버튼 추가

---

## 수동 설정 방법

자동 도구를 사용하지 않고 수동으로 설정하려면:

### 1단계: 씬 생성

**File → New Scene**으로 각 씬을 생성하고 저장:

```
Assets/_Project/Scenes/
├─ Bootstrap.unity
├─ Preload.unity
├─ Main.unity
├─ Loading.unity
├─ Gameplay.unity
└─ Pause.unity
```

### 2단계: Bootstrap 씬 설정

1. Bootstrap.unity 열기
2. 빈 GameObject 생성 → 이름: "BootstrapManager"
3. `Core.Bootstrap.BootstrapManager` 컴포넌트 추가
4. Inspector 설정:
   - Auto Start Game: ✓
   - Initial Scene: Preload

### 3단계: Build Settings 설정

**File → Build Settings** 열기

**"Add Open Scenes"** 버튼으로 씬 추가 (순서 중요!):
```
0. Bootstrap
1. Preload
2. Main
3. Loading
4. Gameplay
5. Pause
```

⚠️ **Build Index 순서가 SceneType Enum과 일치해야 함!**

---

## Build Settings 수동 업데이트

씬이 이미 생성되어 있고 Build Settings만 업데이트하려면:

### 메뉴 사용
```
GASPT → Scene Setup → Update Build Settings
```

이 메뉴를 클릭하면 자동으로 모든 씬을 찾아서 Build Settings에 올바른 순서로 추가합니다.

---

## 테스트 방법

### 1단계: Bootstrap 씬 열기
Project 창에서 `Assets/_Project/Scenes/Bootstrap.unity` 더블클릭

### 2단계: Play 모드 실행
Unity 에디터 상단의 **Play** 버튼 클릭

### 3단계: 예상 동작 확인

#### Console 로그 확인:
```
========================================
=== Bootstrap: 게임 시작 ===
========================================
[Bootstrap] 매니저 초기화 시작...
[SceneLoader] 초기화 완료
[SceneTransition] 초기화 완료
[Bootstrap] 매니저 초기화 완료
[Bootstrap] Preload 씬으로 전환 중...
[SceneLoader] 씬 로드 시작: Preload
[SceneLoader] 씬 로드 완료: Preload
[PreloadState] 초기 리소스 로딩 시작...
[PreloadState] 최소 시간 보장을 위해 2.0초 대기 중...
[PreloadState] Main 씬으로 전환합니다.
[SceneTransition] 페이드 아웃 시작
[SceneLoader] 씬 로드 시작: Main
[SceneLoader] 씬 로드 완료: Main
[SceneTransition] 페이드 인 시작
```

#### 씬 전환 확인:
1. **Bootstrap** (0.1초) → 즉시 전환
2. **Preload** (2초) → 로딩 화면 표시
3. **Main** → 메인 메뉴 표시

### 4단계: 게임 흐름 테스트

Main 씬에서 (나중에 UI 추가 후):
1. "게임 시작" 버튼 클릭
2. Loading 씬 → Gameplay 씬 전환 확인
3. ESC 키 → Pause 씬 활성화 확인

---

## 문제 해결

### Q: "씬을 찾을 수 없음" 에러
**A:** Build Settings에 씬이 추가되지 않았습니다.
- **해결:** `GASPT → Scene Setup → Update Build Settings` 메뉴 실행

### Q: Build Index 순서가 맞지 않음
**A:** SceneType Enum과 Build Settings 순서가 다릅니다.
- **해결:** Build Settings에서 씬을 드래그하여 순서 변경
  ```
  0. Bootstrap
  1. Preload
  2. Main
  3. Loading
  4. Gameplay
  5. Pause
  ```

### Q: BootstrapManager가 없다는 에러
**A:** Bootstrap.unity에 BootstrapManager 컴포넌트가 없습니다.
- **해결:** Bootstrap.unity 열기 → GameObject 생성 → BootstrapManager 컴포넌트 추가

### Q: SceneLoader.Instance is null
**A:** Bootstrap이 실행되지 않았습니다.
- **해결:**
  1. Bootstrap.unity를 첫 번째 씬으로 설정
  2. Play 모드 시작 전 Bootstrap.unity를 열어둠

### Q: 씬이 즉시 전환되지 않음
**A:** 비동기 로딩이므로 약간의 지연이 있습니다.
- **정상 동작:** 페이드 효과로 부드럽게 전환됨
- **비정상:** 검은 화면에서 멈춤 → Console 에러 확인

---

## 추가 커스터마이징

### Bootstrap 설정 변경

Bootstrap.unity → BootstrapManager Inspector:

- **Auto Start Game**: 자동 시작 여부
  - ✓ 체크: Play 시 자동으로 Preload로 이동
  - ☐ 해제: 수동으로 씬 로드 필요

- **Initial Scene**: 최초 로드할 씬
  - Preload (기본값, 권장)
  - Main (리소스 로딩 건너뜀)
  - Gameplay (테스트용)

### 페이드 효과 커스터마이징

SceneTransitionManager (DontDestroyOnLoad):

- **Default Fade Duration**: 페이드 시간 (기본: 0.5초)
- **Fade Color**: 페이드 색상 (기본: Black)

---

## 씬 폴더 구조

```
Assets/_Project/Scenes/
├─ Bootstrap.unity          (빌드 필수)
├─ Preload.unity           (빌드 필수)
├─ Main.unity              (빌드 필수)
├─ Loading.unity           (빌드 필수)
├─ Gameplay.unity          (빌드 필수)
├─ Pause.unity             (빌드 필수)
└─ TestScene.unity         (빌드 제외 가능 - 테스트용)
```

---

## 다음 단계

씬 설정이 완료되면:

1. ✅ **Main.unity**: MainMenu UI 디자인 및 버튼 기능 구현
2. ✅ **Loading.unity**: 로딩바 및 진행률 표시 UI 추가
3. ✅ **Gameplay.unity**: 캐릭터 및 레벨 디자인
4. ✅ **Pause.unity**: 일시정지 메뉴 UI 구현
5. ✅ **리소스 매니페스트**: 필요한 리소스 등록

---

## 참고 문서

- [씬 관리 시스템 아키텍처](../infrastructure/SceneManagementSystem.md)
- [전체 게임 플레이 데모 가이드](../../FULL_GAME_DEMO_GUIDE.md)
- [GameFlow FSM 문서](../development/GameFlowSystem.md)

---

**문서 버전**: 1.0
**작성일**: 2025-10-12
**작성자**: GASPT Development Team
