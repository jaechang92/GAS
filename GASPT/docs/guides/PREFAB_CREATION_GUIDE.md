# UI Panel Prefab 제작 가이드

## 📋 개요

이 가이드는 새로운 Panel 기반 UI 시스템의 Prefab을 Unity 에디터에서 제작하는 방법을 설명합니다.

---

## 🎯 제작해야 할 Prefab 목록

1. **MainMenuPanel.prefab** - 메인 메뉴
2. **LoadingPanel.prefab** - 로딩 화면
3. **GameplayHUDPanel.prefab** - 게임플레이 HUD
4. **PausePanel.prefab** - 일시정지 메뉴

---

## 📁 Prefab 저장 위치

```
Assets/_Project/Resources/UI/Panels/
├── MainMenuPanel.prefab
├── LoadingPanel.prefab
├── GameplayHUDPanel.prefab
└── PausePanel.prefab
```

⚠️ **중요**: `Resources/UI/Panels/` 폴더에 저장해야 UIManager가 자동으로 로드할 수 있습니다.

---

## 🛠️ 1. MainMenuPanel Prefab 제작

### Step 1: Canvas 생성
1. Hierarchy에서 우클릭 → `UI` → `Canvas`
2. Canvas 이름을 `MainMenuPanel`로 변경
3. Inspector에서 설정:
   - Canvas → Render Mode: `Screen Space - Overlay`
   - Canvas Scaler → UI Scale Mode: `Scale With Screen Size`
   - Canvas Scaler → Reference Resolution: `1920 x 1080`

### Step 2: UI 요소 생성

#### 제목 텍스트
1. MainMenuPanel 하위에 우클릭 → `UI` → `Text`
2. 이름: `TitleText`
3. Inspector 설정:
   - Text: `GASPT`
   - Font Size: `72`
   - Alignment: `Center`
   - Color: `White`
4. RectTransform:
   - Anchors: `Center Top` (0.5, 1)
   - Pos Y: `-150`
   - Width: `400`, Height: `100`

#### 시작 버튼
1. MainMenuPanel 하위에 우클릭 → `UI` → `Button`
2. 이름: `StartButton`
3. Text 자식 오브젝트 수정:
   - Text: `게임 시작`
   - Font Size: `24`
4. RectTransform:
   - Anchors: `Center` (0.5, 0.5)
   - Pos Y: `0`
   - Width: `200`, Height: `60`

#### 설정 버튼
1. StartButton 복사 (Ctrl+D)
2. 이름: `SettingsButton`
3. Text: `설정`
4. Pos Y: `-80`

#### 종료 버튼
1. SettingsButton 복사 (Ctrl+D)
2. 이름: `QuitButton`
3. Text: `종료`
4. Pos Y: `-160`

### Step 3: 스크립트 추가
1. MainMenuPanel (Root) 선택
2. Inspector → `Add Component`
3. `MainMenuPanel` 스크립트 추가
4. 스크립트 필드 연결:
   - Start Button → `StartButton` 드래그
   - Settings Button → `SettingsButton` 드래그
   - Quit Button → `QuitButton` 드래그
   - Title Text → `TitleText` 드래그

### Step 4: Prefab 저장
1. `Assets/_Project/Resources/UI/Panels/` 폴더 생성 (없다면)
2. Hierarchy의 `MainMenuPanel`을 `Resources/UI/Panels/` 폴더로 드래그
3. Hierarchy에서 MainMenuPanel 삭제 (Prefab은 유지)

---

## 🛠️ 2. LoadingPanel Prefab 제작

### Step 1: Canvas 생성
1. Hierarchy에서 우클릭 → `UI` → `Canvas`
2. Canvas 이름을 `LoadingPanel`로 변경

### Step 2: UI 요소 생성

#### 배경 패널
1. LoadingPanel 하위에 우클릭 → `UI` → `Image`
2. 이름: `BackgroundPanel`
3. RectTransform:
   - Anchors: `Stretch` (전체 화면)
   - Left/Top/Right/Bottom: `0`
4. Image:
   - Color: `검은색` (R:0.1, G:0.1, B:0.1, A:1)

#### 로딩 텍스트
1. LoadingPanel 하위에 우클릭 → `UI` → `Text`
2. 이름: `LoadingText`
3. Text: `Loading...`
4. Font Size: `48`
5. Alignment: `Center`
6. RectTransform:
   - Anchors: `Center` (0.5, 0.6)
   - Width: `400`, Height: `80`

#### 진행률 바 (Slider)
1. LoadingPanel 하위에 우클릭 → `UI` → `Slider`
2. 이름: `ProgressBar`
3. RectTransform:
   - Anchors: `Center` (0.5, 0.5)
   - Width: `600`, Height: `30`
4. Slider:
   - Min Value: `0`
   - Max Value: `1`
   - Value: `0`
5. Fill Area의 Fill 이미지:
   - Color: `초록색` (R:0.2, G:0.8, B:0.2)

#### 진행률 텍스트
1. LoadingPanel 하위에 우클릭 → `UI` → `Text`
2. 이름: `ProgressText`
3. Text: `0%`
4. Font Size: `24`
5. RectTransform:
   - Anchors: `Center` (0.5, 0.45)
   - Width: `200`, Height: `40`

#### 팁 텍스트
1. LoadingPanel 하위에 우클릭 → `UI` → `Text`
2. 이름: `LoadingTipText`
3. Text: `TIP: 게임을 시작합니다...`
4. Font Size: `18`
5. Color: `회색` (R:0.8, G:0.8, B:0.8)
6. RectTransform:
   - Anchors: `Center` (0.5, 0.3)
   - Width: `600`, Height: `60`

### Step 3: 스크립트 추가
1. LoadingPanel (Root) 선택
2. `LoadingPanel` 스크립트 추가
3. 필드 연결:
   - Progress Bar → `ProgressBar` 드래그
   - Progress Text → `ProgressText` 드래그
   - Loading Tip Text → `LoadingTipText` 드래그
   - Loading Text → `LoadingText` 드래그

### Step 4: Prefab 저장
1. `LoadingPanel`을 `Resources/UI/Panels/` 폴더로 드래그
2. Hierarchy에서 삭제

---

## 🛠️ 3. GameplayHUDPanel Prefab 제작

### Step 1: Canvas 생성
1. Canvas 이름: `GameplayHUDPanel`

### Step 2: UI 요소 생성

#### 체력바 (좌측 상단)
1. GameplayHUDPanel 하위에 GameObject 생성
2. 이름: `HealthBar`
3. `HealthBarUI` 스크립트 추가
4. 체력바 구성:
   - Background (Image): 검은색 배경
   - Fill (Image): 보라색 (R:0.6, G:0.2, B:0.8)
   - HPText (Text): "100/100"
5. RectTransform:
   - Anchors: `Top Left` (0, 1)
   - Pivot: (0, 1)
   - Pos: (20, -20)
   - Size: (300, 40)

#### 콤보 텍스트 (중앙 상단)
1. `UI` → `Text`
2. 이름: `ComboText`
3. Text: `5 COMBO!` (예시)
4. Font Size: `48`, Bold
5. Color: `노란색` (R:1, G:0.8, B:0)
6. RectTransform:
   - Anchors: `Top Center` (0.5, 1)
   - Pos Y: `-80`
   - Size: (400, 80)

#### 적 카운트 (우측 상단)
1. `UI` → `Text`
2. 이름: `EnemyCountText`
3. Text: `적: 0`
4. Font Size: `24`
5. RectTransform:
   - Anchors: `Top Right` (1, 1)
   - Pivot: (1, 1)
   - Pos: (-20, -80)
   - Size: (200, 40)

#### 점수 (좌측 하단)
1. `UI` → `Text`
2. 이름: `ScoreText`
3. Text: `점수: 0`
4. Font Size: `24`
5. RectTransform:
   - Anchors: `Bottom Left` (0, 0)
   - Pivot: (0, 0)
   - Pos: (20, 20)
   - Size: (200, 40)

#### 일시정지 버튼 (우측 상단)
1. `UI` → `Button`
2. 이름: `PauseButton`
3. Text: `II` (일시정지 아이콘)
4. RectTransform:
   - Anchors: `Top Right` (1, 1)
   - Pivot: (1, 1)
   - Pos: (-20, -20)
   - Size: (80, 40)

### Step 3: 스크립트 추가
1. GameplayHUDPanel (Root) 선택
2. `GameplayHUDPanel` 스크립트 추가
3. 필드 연결:
   - Health Bar → `HealthBar` 드래그
   - Combo Text → `ComboText` 드래그
   - Enemy Count Text → `EnemyCountText` 드래그
   - Score Text → `ScoreText` 드래그
   - Pause Button → `PauseButton` 드래그

### Step 4: Prefab 저장
1. `GameplayHUDPanel`을 `Resources/UI/Panels/` 폴더로 드래그
2. Hierarchy에서 삭제

---

## 🛠️ 4. PausePanel Prefab 제작

### Step 1: Canvas 생성
1. Canvas 이름: `PausePanel`

### Step 2: UI 요소 생성

#### 어두운 배경 (Dimmed Background)
1. PausePanel 하위에 `UI` → `Image`
2. 이름: `DimmedBackground`
3. RectTransform: `Stretch` (전체 화면)
4. Image Color: `반투명 검은색` (R:0, G:0, B:0, A:0.7)

#### 팝업 패널
1. PausePanel 하위에 `UI` → `Image`
2. 이름: `PopupPanel`
3. RectTransform:
   - Anchors: `Center`
   - Size: (400, 500)
4. Image Color: `어두운 회색` (R:0.2, G:0.2, B:0.2, A:0.95)

#### 제목 텍스트
1. PopupPanel 하위에 `UI` → `Text`
2. 이름: `TitleText`
3. Text: `일시정지`
4. Font Size: `48`
5. Alignment: `Center`
6. RectTransform:
   - Anchors: `Top Center`
   - Pos Y: `-50`
   - Size: (300, 80)

#### 재개 버튼
1. PopupPanel 하위에 `UI` → `Button`
2. 이름: `ResumeButton`
3. Text: `재개`
4. RectTransform:
   - Anchors: `Center`
   - Pos Y: `50`
   - Size: (250, 60)

#### 설정 버튼
1. ResumeButton 복사
2. 이름: `SettingsButton`
3. Text: `설정`
4. Pos Y: `-30`

#### 메인 메뉴 버튼
1. SettingsButton 복사
2. 이름: `MainMenuButton`
3. Text: `메인 메뉴`
4. Pos Y: `-110`

### Step 3: 스크립트 추가
1. PausePanel (Root) 선택
2. `PausePanel` 스크립트 추가
3. 필드 연결:
   - Resume Button → `ResumeButton` 드래그
   - Settings Button → `SettingsButton` 드래그
   - Main Menu Button → `MainMenuButton` 드래그
   - Title Text → `TitleText` 드래그

### Step 4: Prefab 저장
1. `PausePanel`을 `Resources/UI/Panels/` 폴더로 드래그
2. Hierarchy에서 삭제

---

## ✅ 완료 체크리스트

제작 완료 후 다음 사항을 확인하세요:

- [ ] 모든 Prefab이 `Assets/_Project/Resources/UI/Panels/` 폴더에 저장됨
- [ ] Prefab 파일명이 정확함 (MainMenuPanel.prefab 등)
- [ ] 각 Prefab의 Root에 해당 Panel 스크립트가 추가됨
- [ ] 모든 UI 요소가 스크립트 필드에 연결됨
- [ ] Canvas Scaler 설정 확인 (1920x1080 기준)
- [ ] 버튼 이벤트가 정상 작동하는지 테스트

---

## 🧪 테스트 방법

### Unity 에디터에서 테스트
1. Bootstrap 씬 실행
2. 게임 플로우 확인:
   - Preload → Main (MainMenuPanel 표시)
   - 게임 시작 → Loading (LoadingPanel 표시)
   - Ingame (GameplayHUDPanel 표시)
   - ESC 키 → Pause (PausePanel 표시)

### 디버그 로그 확인
Console에서 다음 로그 확인:
```
[UIManager] Panel Prefab 로드 완료: MainMenu
[MainMenuPanel] 메인 메뉴 열림
[LoadingPanel] 로딩 화면 표시
[GameplayHUDPanel] 게임플레이 HUD 표시
[PausePanel] 일시정지
```

---

## 🎨 디자인 팁

### 색상 팔레트
- **배경**: (0.1, 0.1, 0.1) - 검은색
- **버튼**: (0.2, 0.2, 0.2) - 어두운 회색
- **텍스트**: (1, 1, 1) - 흰색
- **강조**: (0.6, 0.2, 0.8) - 보라색 (게임 테마)
- **성공**: (0.2, 0.8, 0.2) - 초록색
- **경고**: (1, 0.8, 0) - 노란색

### 폰트 크기 가이드
- **제목**: 48-72
- **버튼**: 24-32
- **일반 텍스트**: 18-24
- **작은 텍스트**: 14-18

---

## 🔧 문제 해결

### Prefab이 로드되지 않음
**증상**: `Panel Prefab을 찾을 수 없습니다` 에러
**해결**:
1. Prefab이 정확히 `Resources/UI/Panels/` 폴더에 있는지 확인
2. 파일명이 정확한지 확인 (예: `MainMenuPanel.prefab`)
3. Unity 에디터 재시작

### 스크립트가 연결되지 않음
**증상**: `BasePanel 컴포넌트가 없습니다` 에러
**해결**:
1. Prefab의 Root GameObject에 Panel 스크립트 추가
2. 스크립트 컴파일 완료 후 재시도

### UI 요소가 보이지 않음
**해결**:
1. Canvas Render Mode 확인
2. Canvas의 Sorting Order 확인
3. UI 요소의 RectTransform 위치 확인

---

**작성일**: 2025-01-XX
**최종 수정**: 2025-01-XX
