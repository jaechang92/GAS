# PrefabMaker 사용 가이드

## 📋 개요

PrefabMaker 스크립트를 사용하여 4개의 Panel Prefab을 자동으로 생성할 수 있습니다.

---

## 🚀 사용 방법

### 1단계: PrefabTest 씬 생성

1. Unity 에디터 열기
2. `Assets/_Project/Scenes/` 폴더에서 우클릭
3. `Create` → `Scene`
4. 이름: `PrefabTest`

### 2단계: PrefabMaker 오브젝트 생성

1. `PrefabTest` 씬 열기
2. Hierarchy에서 우클릭 → `Create Empty`
3. 이름: `PrefabMaker`
4. Inspector에서 `Add Component` 클릭
5. `PrefabMaker` 검색 후 추가

### 3단계: 저장 경로 확인

PrefabMaker 컴포넌트의 Inspector에서:
- **Prefab Save Path**: `Assets/_Project/Resources/UI/Panels/`
- **Reference Resolution**: `1920 x 1080`

⚠️ **중요**: `Assets/_Project/Resources/UI/Panels/` 폴더가 미리 생성되어 있어야 합니다!

### 4단계: Prefab 생성

#### 방법 A: 모든 Prefab 한 번에 생성
1. PrefabMaker GameObject 선택
2. Inspector에서 PrefabMaker 컴포넌트 우클릭
3. `Create All Panel Prefabs` 선택

#### 방법 B: 개별 Prefab 생성
1. PrefabMaker GameObject 선택
2. Inspector에서 PrefabMaker 컴포넌트 우클릭
3. 원하는 Prefab 선택:
   - `Create MainMenuPanel Prefab`
   - `Create LoadingPanel Prefab`
   - `Create GameplayHUDPanel Prefab`
   - `Create PausePanel Prefab`

---

## ✅ 생성되는 Prefab 목록

### 1. MainMenuPanel.prefab
- **위치**: `Assets/_Project/Resources/UI/Panels/MainMenuPanel.prefab`
- **구성 요소**:
  - TitleText (제목: "GASPT")
  - StartButton (게임 시작)
  - SettingsButton (설정)
  - QuitButton (종료)

### 2. LoadingPanel.prefab
- **위치**: `Assets/_Project/Resources/UI/Panels/LoadingPanel.prefab`
- **구성 요소**:
  - BackgroundPanel (검은색 배경)
  - LoadingText ("Loading...")
  - ProgressBar (진행률 바)
  - ProgressText ("0%")
  - LoadingTipText (팁 텍스트)

### 3. GameplayHUDPanel.prefab
- **위치**: `Assets/_Project/Resources/UI/Panels/GameplayHUDPanel.prefab`
- **구성 요소**:
  - HealthBar (체력바 - 빈 GameObject)
  - ComboText (콤보 표시)
  - EnemyCountText (적 카운트)
  - ScoreText (점수)
  - PauseButton (일시정지 버튼)

### 4. PausePanel.prefab
- **위치**: `Assets/_Project/Resources/UI/Panels/PausePanel.prefab`
- **구성 요소**:
  - DimmedBackground (어두운 배경)
  - PopupPanel (팝업 패널)
  - TitleText ("일시정지")
  - ResumeButton (재개)
  - SettingsButton (설정)
  - MainMenuButton (메인 메뉴)

---

## 🔍 생성 확인

Prefab 생성 후 다음 사항을 확인하세요:

1. **Console 로그 확인**:
   ```
   [PrefabMaker] MainMenuPanel Prefab 생성 중...
   [PrefabMaker] MainMenuPanel Prefab 생성 완료!
   [PrefabMaker] Prefab 저장 완료: Assets/_Project/Resources/UI/Panels/MainMenuPanel.prefab
   ```

2. **Project 창에서 확인**:
   - `Assets/_Project/Resources/UI/Panels/` 폴더 열기
   - 4개의 Prefab 파일 확인

3. **Prefab 열어서 확인**:
   - Prefab 더블클릭하여 Prefab Mode 진입
   - UI 구조 확인
   - Panel 스크립트 필드 연결 확인

---

## 🧪 테스트 방법

### Bootstrap 씬에서 테스트
1. `Assets/_Project/Scenes/Bootstrap.unity` 씬 열기
2. Play 버튼 클릭
3. 게임 플로우 확인:
   - **Preload → Main**: MainMenuPanel 표시
   - **게임 시작 클릭**: LoadingPanel 표시
   - **로딩 완료**: GameplayHUDPanel 표시
   - **ESC 키**: PausePanel 표시

---

## 🛠️ 문제 해결

### 문제 1: "저장 경로가 존재하지 않습니다" 에러
**해결**:
```bash
Project 창에서 폴더 생성:
Assets/_Project/Resources/UI/Panels/
```

### 문제 2: Panel 스크립트를 찾을 수 없음
**해결**:
1. Unity 에디터에서 스크립트 컴파일 완료 대기
2. Console에 컴파일 에러가 없는지 확인
3. PrefabMaker 스크립트 재로딩 (Assets → Refresh)

### 문제 3: Prefab이 생성되지 않음
**해결**:
1. Console에서 에러 메시지 확인
2. PrefabMaker GameObject가 선택되어 있는지 확인
3. ContextMenu가 보이지 않으면 스크립트 재컴파일

### 문제 4: HealthBar에 HealthBarUI 컴포넌트 없음
**정상 동작입니다!**
- GameplayHUDPanel의 healthBar는 MonoBehaviour로 설정됨
- 나중에 Unity 에디터에서 HealthBarUI 컴포넌트를 추가하거나
- Reflection을 사용하여 동적으로 처리됩니다

---

## 📝 추가 커스터마이징

Prefab 생성 후 Unity 에디터에서 자유롭게 수정 가능:
- 폰트 변경
- 색상 조정
- 레이아웃 수정
- 추가 UI 요소 배치

---

**작성일**: 2025-01-XX
**스크립트 위치**: `Assets/_Project/Scripts/Tools/PrefabMaker.cs`
