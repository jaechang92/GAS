# GASPT 에디터 메뉴 가이드

GASPT 프로젝트의 모든 에디터 도구를 한눈에 볼 수 있는 가이드입니다.

## 📌 빠른 시작

Unity 에디터 상단 메뉴에서 **`GASPT`** 메뉴를 클릭하면 모든 도구를 사용할 수 있습니다.

처음 사용하신다면:
1. Unity 에디터에서 `GASPT → Help → Open Menu Guide` 실행
2. 가이드 윈도우에서 필요한 기능을 버튼으로 실행

---

## 🎨 메뉴 구조

```
GASPT/
├─ Prefabs/
│  ├─ UI Panels/          # UI Panel Prefab 자동 생성
│  ├─ NPC/                # NPC Prefab 자동 생성
│  └─ Dialogue/           # 대화 시스템 Prefab 자동 생성
├─ Scene Setup/           # 씬 자동 생성 및 설정
├─ Resources/             # Resource Manifest 관리
├─ Character/             # 캐릭터 생성 도구
└─ Help/                  # 메뉴 가이드
```

---

## 📁 Prefabs - UI Panels

**위치**: `GASPT → Prefabs → UI Panels`

### 기능
- **Create All Panels**: 모든 UI Panel Prefab 생성 (MainMenu, Loading, GameplayHUD, Pause)
- **Create MainMenu Panel**: 메인 메뉴 Panel Prefab 생성
- **Create Loading Panel**: 로딩 화면 Panel Prefab 생성
- **Create GameplayHUD Panel**: 게임플레이 HUD Panel Prefab 생성
- **Create Pause Panel**: 일시정지 Panel Prefab 생성
- **Open Prefabs Folder**: Prefab 저장 폴더 열기

### 저장 위치
`Assets/_Project/Resources/UI/Panels/`

### 사용 방법
1. `GASPT → Prefabs → UI Panels → Create All Panels` 실행
2. 생성 확인 다이얼로그에서 "생성" 클릭
3. `Assets/_Project/Resources/UI/Panels/` 폴더에서 Prefab 확인

### 주의사항
- 기존 Prefab이 있으면 덮어씁니다 (경고 메시지 출력)
- BasePanel 스크립트가 자동으로 Canvas를 추가하므로 Prefab에는 RectTransform만 포함

---

## 👥 Prefabs - NPC

**위치**: `GASPT → NPC Creator`

### 기능
- **Open Creator Window**: NPC 생성 윈도우 열기 (커스텀 NPC 생성)
- **Create StoryNPC**: 스토리 NPC 빠른 생성 (마을사람)
- **Create ShopNPC**: 상점 NPC 빠른 생성 (상인)
- **Create All NPCs**: 모든 기본 NPC 생성
- **Open NPC Folder**: NPC Prefab 폴더 열기

### 저장 위치
- **Data**: `Assets/_Project/Data/NPC/`
- **Prefab**: `Assets/_Project/Prefabs/NPC/`

### 사용 방법

#### 빠른 생성 (기본 NPC)
1. `GASPT → NPC Creator → Create StoryNPC` 실행
2. 자동으로 NPCData와 Prefab 생성

#### 커스텀 NPC 생성
1. `GASPT → NPC Creator → Open Creator Window` 실행
2. NPC 타입 선택 (Story / Shop)
3. NPC 이름 입력
4. (선택) 스프라이트 할당
5. (선택) Episode ID 입력
6. "NPC 생성" 버튼 클릭

---

## 💬 Prefabs - Dialogue

**위치**: `GASPT → Prefabs → Dialogue`

### 기능
- **Create DialoguePanel**: 대화 패널 Prefab 생성 (NPC 대화 시스템용)
- **Create ChoiceButton**: 선택지 버튼 Prefab 생성 (DialoguePanel용)

### 저장 위치
- **DialoguePanel**: `Assets/_Project/Resources/UI/Panels/DialogPanel.prefab`
- **ChoiceButton**: `Assets/_Project/Resources/UI/Prefabs/ChoiceButton.prefab`

### 사용 방법

#### DialoguePanel 생성
1. `GASPT → Prefabs → Dialogue → Create DialoguePanel` 실행
2. 자동으로 DialogPanel Prefab 생성
3. NPC 대화 시스템에서 사용

#### ChoiceButton 생성
1. `GASPT → Prefabs → Dialogue → Create ChoiceButton` 실행
2. 자동으로 ChoiceButton Prefab 생성
3. DialoguePanel이 선택지 표시 시 사용

### 주의사항
- DialoguePanel을 생성하기 전에 ChoiceButton을 먼저 생성하는 것을 권장
- 두 Prefab은 NPC 대화 시스템에서 함께 사용됨

### 포함 컴포넌트

#### DialoguePanel
- 반투명 배경
- 대화 창 (DialogueBox)
- 화자 이름 패널
- 대화 텍스트 영역
- 계속 버튼
- 선택지 버튼 컨테이너
- 타이핑 효과 설정

#### ChoiceButton
- 버튼 배경 (Image)
- 텍스트 (TextMeshProUGUI)
- 호버/클릭 효과 (ColorBlock)

---

## 🎬 Scene Setup

**위치**: `GASPT → Scene Setup`

### 기능
- **Open Scene Setup Tool**: 씬 설정 도구 윈도우 열기
- **Create All Scenes**: 모든 기본 씬 생성 (Bootstrap, Preload, Main, Gameplay, Lobby)
- **Update Build Settings**: Build Settings에 씬 추가
- **Open Scene Folder**: 씬 폴더 열기

### 저장 위치
`Assets/_Project/Scenes/`

### 생성되는 씬
1. **Bootstrap.unity**: 게임 진입점, BootstrapManager 포함
2. **Preload.unity**: 초기 로딩 씬
3. **Main.unity**: 메인 메뉴 씬
4. **Gameplay.unity**: 게임플레이 씬 (Ground, SpawnPoints 포함)
5. **Lobby.unity**: 로비 씬 (NPC SpawnPoints 포함)

### 사용 방법
1. `GASPT → Scene Setup → Create All Scenes` 실행
2. 생성할 씬 선택
3. "모든 씬 생성" 버튼 클릭
4. (자동) Build Settings에 씬 추가됨

---

## 📦 Resources

**위치**: `GASPT → Resources`

### 기능
- **Create All Manifests**: 모든 Resource Manifest 생성
- **Create Essential Manifest**: 필수 리소스 Manifest 생성
- **Create MainMenu Manifest**: 메인 메뉴 리소스 Manifest 생성
- **Create Gameplay Manifest**: 게임플레이 리소스 Manifest 생성
- **Create Common Manifest**: 공통 리소스 Manifest 생성
- **Delete All Manifests**: 모든 Manifest 삭제

### 저장 위치
`Assets/_Project/Resources/Manifests/`

### Manifest 종류
1. **EssentialManifest**: 게임 시작 시 필수 리소스 (SkulPhysicsConfig 등)
2. **MainMenuManifest**: 메인 메뉴 리소스
3. **GameplayManifest**: 게임플레이 리소스
4. **CommonManifest**: 공통 리소스 (VFX, 사운드 등)

### 사용 방법
1. `GASPT → Resources → Create All Manifests` 실행
2. `Assets/_Project/Resources/Manifests/` 폴더에서 Manifest 확인
3. Inspector에서 리소스 추가/수정

---

## 🎮 Character

**위치**: `GASPT → Character`

### 기능
- **Create Player (Skul Physics)**: Skul 스타일 물리 시스템 플레이어 캐릭터 생성 도구

### 사용 방법
1. `GASPT → Character → Create Player (Skul Physics)` 실행
2. 캐릭터 생성 윈도우에서 설정
3. "플레이어 캐릭터 생성" 버튼 클릭

### 포함 컴포넌트
- PlayerController (FSM 상태 관리)
- CharacterPhysics (통합 물리 시스템)
- Rigidbody2D
- BoxCollider2D
- SkulPhysicsConfig
- InputHandler (선택)
- AnimationController (선택)
- AbilitySystem (선택)

---

## ❓ Help

**위치**: `GASPT → Help`

### 기능
- **Open Menu Guide**: 메뉴 가이드 윈도우 열기

### 사용 방법
1. `GASPT → Help → Open Menu Guide` 실행
2. 가이드 윈도우에서 버튼을 클릭하여 기능 실행
3. 빠른 작업 섹션에서 자주 사용하는 기능 실행

---

## ⚡ 빠른 작업

자주 사용하는 기능들을 빠르게 실행하세요:

### 새 프로젝트 설정
```
1. GASPT → Scene Setup → Create All Scenes
2. GASPT → Prefabs → UI Panels → Create All Panels
3. GASPT → Resources → Create All Manifests
4. GASPT → NPC Creator → Create All NPCs
```

### UI 작업
```
GASPT → Prefabs → UI Panels → Create All Panels
```

### 씬 작업
```
GASPT → Scene Setup → Create All Scenes
GASPT → Scene Setup → Update Build Settings
```

---

## 💡 팁

1. **폴더 자동 생성**: 모든 도구는 필요한 폴더를 자동으로 생성합니다.
2. **덮어쓰기 확인**: 기존 파일이 있으면 덮어쓰기 여부를 물어봅니다.
3. **독립 실행**: 각 도구는 독립적으로 실행 가능합니다.
4. **메뉴 가이드**: `GASPT → Help → Open Menu Guide`에서 모든 기능을 한눈에 볼 수 있습니다.
5. **버전 관리**: 생성된 Prefab과 Data는 Git으로 관리됩니다.

---

## 🔧 문제 해결

### Prefab 생성 실패
- `Assets/_Project/Resources/UI/Panels/` 폴더가 존재하는지 확인
- Unity 에디터를 재시작 후 다시 시도

### 씬 생성 실패
- `Assets/_Project/Scenes/` 폴더가 존재하는지 확인
- 기존 씬이 열려있으면 저장 후 다시 시도

### NPC 생성 실패
- NPCData, StoryNPC, ShopNPC 스크립트가 컴파일되었는지 확인
- Unity 에디터를 재시작 후 다시 시도

---

## 📝 변경 이력

### 2025-10-19: GASPT 메뉴 통합 완료
- 모든 GASPT 도구를 단일 메뉴 아래 통합
- 레거시 도구 제거 (LegacyFolderCleanup, FolderStructureOrganizer, HUDPrefabCreator, PlayerHUDPanelPrefabGenerator)
- PrefabMaker를 PanelPrefabCreator로 교체 (MenuItem 기반)
- GASPTMenuGuide 윈도우 추가
- PlayerCharacterCreator 메뉴 경로 변경 (Tools/Project → GASPT/Character)
- Dialogue 도구 통합 (Tools/Dialogue → GASPT/Prefabs/Dialogue)

---

## 🎯 다음 작업 예정

- [ ] GAS-Combat 통합 완료 (Unity 에디터 작업)
- [ ] UI Prefab 재생성 및 테스트
- [ ] Phase 3: 콘텐츠 확장 시작
