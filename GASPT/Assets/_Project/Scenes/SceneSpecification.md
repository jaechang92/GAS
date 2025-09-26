# 플랫포머 게임 씬 명세서

## 🎮 게임 개요
- **장르**: 2D 플랫포머 액션 게임
- **타겟**: 중급 게이머 대상
- **세션**: 짧은 플레이 세션에 최적화
- **아키텍처**: GameFlow + GAS + FSM 시스템 기반

---

## 📋 씬 구조 및 역할

### 1. **Bootstrap Scene** (`00_Bootstrap.unity`)
#### 🎯 **역할**
- 게임 전체 시스템 초기화
- 매니저들의 생성 및 설정
- 메인 메뉴로 자동 전환

#### 🏗 **필수 오브젝트**
- **SystemBootstrap** (MonoBehaviour)
  - GameManager, AudioManager, UIManager 초기화
  - 게임 설정 로드
  - 메인 메뉴 씬으로 전환

#### 📦 **컴포넌트**
```csharp
- BootstrapManager : MonoBehaviour
  - InitializeManagers()
  - LoadGameSettings()
  - TransitionToMainMenu()
```

---

### 2. **Main Menu Scene** (`01_MainMenu.unity`)
#### 🎯 **역할**
- 게임 시작점
- 설정, 크레딧, 종료 메뉴
- 배경 음악 및 시각적 효과

#### 🏗 **필수 오브젝트**
- **UI Canvas**
  - 타이틀 로고
  - 메뉴 버튼들 (시작, 설정, 크레딧, 종료)
  - 배경 이미지/애니메이션

- **Audio Source**
  - 메인 메뉴 배경음악
  - UI 효과음

- **GameFlowManager**
  - 씬 전환 관리

#### 📦 **컴포넌트**
```csharp
- MainMenuUI : MonoBehaviour
  - OnStartGameClicked()
  - OnSettingsClicked()
  - OnCreditsClicked()
  - OnQuitClicked()

- BackgroundAnimator : MonoBehaviour
  - 배경 파티클/애니메이션 관리
```

---

### 3. **Level Select Scene** (`02_LevelSelect.unity`)
#### 🎯 **역할**
- 레벨 선택 인터페이스
- 진행도 표시
- 레벨별 별점/점수 표시

#### 🏗 **필수 오브젝트**
- **UI Canvas**
  - 레벨 선택 그리드
  - 뒤로가기 버튼
  - 진행도 표시바

- **Level Data Manager**
  - 레벨 잠금 상태 관리
  - 별점 데이터 관리

#### 📦 **컴포넌트**
```csharp
- LevelSelectUI : MonoBehaviour
  - DisplayLevels()
  - OnLevelSelected(int levelIndex)
  - UpdateProgressDisplay()

- LevelButton : MonoBehaviour
  - levelIndex : int
  - isUnlocked : bool
  - starCount : int
```

---

### 4. **Gameplay Scene** (`03_Gameplay.unity`)
#### 🎯 **역할**
- 실제 플랫포머 게임플레이
- 플레이어 조작 및 적 AI
- 아이템 수집 및 목표 달성

#### 🏗 **필수 오브젝트**

##### **A. 플레이어 시스템**
- **Player** (GameObject)
  - PlayerController
  - AbilitySystemComponent (GAS)
  - Rigidbody2D, Collider2D
  - Animator, SpriteRenderer

##### **B. 레벨 환경**
- **Level Manager**
  - 스폰 포인트 관리
  - 체크포인트 시스템
  - 레벨 완료 조건

- **Platform System**
  - Ground (정적 플랫폼)
  - MovingPlatform (이동 플랫폼)
  - BreakablePlatform (부서지는 플랫폼)
  - JumpPad (점프 패드)

##### **C. 적 시스템**
- **Enemy_Basic** (기본 적)
- **Enemy_Flying** (비행 적)
- **Enemy_Boss** (보스 적)

##### **D. 아이템 시스템**
- **Collectibles** (수집 아이템)
- **PowerUps** (능력 강화)
- **HealthPack** (체력 회복)

##### **E. 환경 요소**
- **Hazards** (위험 요소)
  - Spikes (가시)
  - Fire (불)
  - Water (물)

- **Interactive Objects**
  - Switch (스위치)
  - Door (문)
  - Elevator (엘리베이터)

#### 📦 **컴포넌트**
```csharp
// 플레이어
- PlayerController : MonoBehaviour
- PlayerMovement : AbilityBase (GAS)
- PlayerJump : AbilityBase (GAS)
- PlayerDash : AbilityBase (GAS)

// 레벨 관리
- LevelManager : MonoBehaviour
- CheckpointSystem : MonoBehaviour
- CameraController : MonoBehaviour

// 적 AI
- EnemyAI : MonoBehaviour
- EnemyPatrol : MonoBehaviour
- EnemyAttack : AbilityBase (GAS)

// 아이템
- Collectible : MonoBehaviour
- PowerUp : MonoBehaviour
- HealthPack : MonoBehaviour
```

---

### 5. **Pause Menu Scene** (`04_PauseMenu.unity` - 오버레이)
#### 🎯 **역할**
- 게임 중 일시정지 메뉴
- 설정 변경
- 메인 메뉴로 돌아가기

#### 🏗 **필수 오브젝트**
- **Pause UI Canvas**
  - 반투명 배경
  - 계속하기, 설정, 메인메뉴 버튼
  - 현재 점수/시간 표시

#### 📦 **컴포넌트**
```csharp
- PauseMenuUI : MonoBehaviour
  - OnResumeClicked()
  - OnSettingsClicked()
  - OnMainMenuClicked()
```

---

### 6. **Settings Scene** (`05_Settings.unity`)
#### 🎯 **역할**
- 게임 설정 변경
- 오디오, 그래픽, 컨트롤 설정
- 설정 저장/로드

#### 🏗 **필수 오브젝트**
- **Settings UI Canvas**
  - 오디오 슬라이더
  - 그래픽 품질 드롭다운
  - 키 바인딩 설정
  - 저장/취소 버튼

#### 📦 **컴포넌트**
```csharp
- SettingsUI : MonoBehaviour
- AudioSettings : MonoBehaviour
- GraphicsSettings : MonoBehaviour
- ControlSettings : MonoBehaviour
```

---

### 7. **Game Over Scene** (`06_GameOver.unity`)
#### 🎯 **역할**
- 게임 오버 시 결과 표시
- 재시작 또는 메뉴 선택
- 점수 및 통계 표시

#### 🏗 **필수 오브젝트**
- **Game Over UI Canvas**
  - 게임 오버 텍스트
  - 최종 점수
  - 재시작/메뉴 버튼

#### 📦 **컴포넌트**
```csharp
- GameOverUI : MonoBehaviour
  - DisplayFinalScore(int score)
  - OnRestartClicked()
  - OnMainMenuClicked()
```

---

### 8. **Level Complete Scene** (`07_LevelComplete.unity`)
#### 🎯 **역할**
- 레벨 완료 시 결과 표시
- 별점 평가
- 다음 레벨 또는 레벨 선택으로 이동

#### 🏗 **필수 오브젝트**
- **Level Complete UI Canvas**
  - 완료 축하 텍스트
  - 별점 표시 (1-3개)
  - 점수, 시간, 수집품 통계
  - 다음 레벨/레벨 선택 버튼

#### 📦 **컴포넌트**
```csharp
- LevelCompleteUI : MonoBehaviour
  - DisplayResults(LevelResult result)
  - ShowStarRating(int stars)
  - OnNextLevelClicked()
  - OnLevelSelectClicked()

- LevelResult : ScriptableObject
  - score : int
  - timeSpent : float
  - collectiblesFound : int
  - starRating : int
```

---

## 🔄 씬 전환 흐름

```
Bootstrap → MainMenu → LevelSelect → Gameplay
                ↓           ↓         ↓
              Settings   Settings   PauseMenu
                                     ↓
                               GameOver/LevelComplete
                                     ↓
                               MainMenu/LevelSelect
```

---

## 📁 폴더 구조

```
Assets/
├── Scenes/
│   ├── 00_Bootstrap.unity
│   ├── 01_MainMenu.unity
│   ├── 02_LevelSelect.unity
│   ├── 03_Gameplay.unity
│   ├── 04_PauseMenu.unity
│   ├── 05_Settings.unity
│   ├── 06_GameOver.unity
│   └── 07_LevelComplete.unity
├── Scripts/
│   ├── Scenes/
│   │   ├── Bootstrap/
│   │   ├── MainMenu/
│   │   ├── LevelSelect/
│   │   ├── Gameplay/
│   │   └── UI/
├── Prefabs/
│   ├── UI/
│   ├── Player/
│   ├── Enemies/
│   ├── Environment/
│   └── Effects/
└── Resources/
    ├── Audio/
    ├── Sprites/
    └── Data/
```

---

## 🎯 개발 우선순위

### Phase 1 (기반 구축)
1. Bootstrap + MainMenu 씬
2. 기본 Gameplay 씬 (플레이어 이동만)
3. 씬 전환 시스템

### Phase 2 (핵심 기능)
1. 플랫포머 메커닉 (점프, 충돌)
2. 기본 UI 시스템
3. PauseMenu 구현

### Phase 3 (콘텐츠 확장)
1. 적 AI 시스템
2. 아이템 시스템
3. LevelSelect, GameOver, LevelComplete

### Phase 4 (완성도)
1. Settings 시스템
2. 사운드 및 이펙트
3. 최적화 및 폴리싱

이 명세서를 바탕으로 단계적으로 씬을 구현하면 체계적인 플랫포머 게임을 만들 수 있습니다!