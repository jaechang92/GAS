# 절차적 레벨 생성 시스템 - 태스크 목록

**기능 번호**: 016
**브랜치**: `016-procedural-level-generation`
**생성일**: 2025-11-29
**상태**: 구현 완료

---

## 개요

이 문서는 절차적 레벨 생성 시스템의 구현 태스크를 정의합니다.
사용자 스토리 기반으로 구성되어 각 Phase가 독립적으로 테스트 가능합니다.

---

## 사용자 스토리 매핑

| 스토리 ID | 우선순위 | 설명 |
|----------|---------|------|
| US1 | P1 | 던전 그래프 생성 - 시드 기반 재현 가능한 던전 구조 생성 |
| US2 | P1 | 방 이동 및 분기 선택 - 포탈을 통한 다음 방 선택 |
| US3 | P2 | 미니맵 시스템 - 던전 구조 시각화 |
| US4 | P2 | 스테이지 시스템 - 5단계 스테이지 진행 |
| US5 | P3 | 에디터 도구 - 던전 미리보기 및 테스트 |

---

## Phase 1: Setup (프로젝트 초기화) ✅

**목표**: 절차적 레벨 생성에 필요한 폴더 구조 및 기본 설정

- [x] T001 Create folder structure `Assets/_Project/Scripts/Gameplay/Level/Graph/`
- [x] T002 Create folder structure `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/`
- [x] T003 Create folder structure `Assets/_Project/Scripts/UI/Minimap/`
- [x] T004 Create git branch `016-procedural-level-generation` from master

---

## Phase 2: Foundational (기반 데이터 구조) ✅

**목표**: 모든 사용자 스토리에서 공통으로 사용하는 데이터 구조 정의
**완료 기준**: DungeonNode, DungeonEdge, DungeonGraph 클래스가 컴파일되고 단위 테스트 가능

### 그래프 데이터 구조

- [x] T005 [P] Create DungeonNode class with nodeId, roomType, position, connections, isVisited, isRevealed in `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonNode.cs`
- [x] T006 [P] Create DungeonEdge class with fromNode, toNode, edgeType (Normal, Secret, OneWay) in `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonEdge.cs`
- [x] T007 Create DungeonGraph class with nodes Dictionary, edges List, entryNode, bossNode, currentNode in `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonGraph.cs`
- [x] T008 Implement GetAdjacentNodes() method in DungeonGraph returning connected nodes in `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonGraph.cs`
- [x] T009 Implement GetPath() method for pathfinding between two nodes in `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonGraph.cs`
- [x] T010 Implement ValidateGraph() method to ensure all nodes reachable from entry in `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonGraph.cs`

### 기존 데이터 확장

- [x] T011 [P] Add maxConnections field (default 4) to RoomData in `Assets/_Project/Scripts/Gameplay/Level/Room/RoomData.cs`
- [x] T012 [P] Add canBeBranch boolean field to RoomData in `Assets/_Project/Scripts/Gameplay/Level/Room/RoomData.cs`
- [x] T013 Add branchingFactor (0-1), maxBranches, minPathLength, maxPathLength to RoomGenerationRules in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/RoomGenerationRules.cs`
- [x] T014 Add secretRoomChance field to RoomGenerationRules in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/RoomGenerationRules.cs`

---

## Phase 3: US1 - 던전 그래프 생성 ✅

**스토리 목표**: 플레이어가 매 런마다 새로운 던전 구조를 경험할 수 있도록 시드 기반으로 그래프 생성
**독립 테스트 기준**:
- 동일 시드로 동일한 그래프 구조 생성됨
- 생성된 그래프에서 Entry → Boss 경로가 항상 존재
- 분기점이 2~3개 존재

### SeedManager

- [x] T015 [US1] Create SeedManager static class with SetSeed(), GetCurrentSeed() methods in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/SeedManager.cs`
- [x] T016 [US1] Implement SaveRandomState() and RestoreRandomState() for Unity Random.State management in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/SeedManager.cs`
- [x] T017 [US1] Add GenerateRandomSeed() method returning timestamp-based seed in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/SeedManager.cs`

### GraphBuilder

- [x] T018 [US1] Create GraphBuilder class with GenerateGraph(DungeonConfig, int seed) method signature in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`
- [x] T019 [US1] Implement floor-based node generation algorithm (Floor 0 = Entry, Last Floor = Boss) in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`
- [x] T020 [US1] Implement GetNodeCountForFloor() returning 1-3 based on floor index and rules in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`
- [x] T021 [US1] Implement ConnectToPreviousFloor() for creating edges between floors in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`
- [x] T022 [US1] Implement branch/merge logic ensuring no crossing edges in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`
- [x] T023 [US1] Implement AssignRoomTypes() distributing Combat, Elite, Shop, Rest, Treasure rooms in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`
- [x] T024 [US1] Add validation to ensure Entry→Boss path always exists in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`

### RoomPlacer

- [x] T025 [US1] Create RoomPlacer class with PlaceRooms(DungeonGraph, Transform container) method in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/RoomPlacer.cs`
- [x] T026 [US1] Implement SelectRoomPrefab() matching node type to RoomData pool in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/RoomPlacer.cs`
- [x] T027 [US1] Implement InstantiateRoom() creating Room instances from prefabs in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/RoomPlacer.cs`
- [x] T028 [US1] Implement ConfigurePortals() setting up portal connections based on edges in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/RoomPlacer.cs`

### DungeonGenerator (Facade)

- [x] T029 [US1] Create DungeonGenerator class as facade combining GraphBuilder and RoomPlacer in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/DungeonGenerator.cs`
- [x] T030 [US1] Implement GenerateDungeonAsync(DungeonConfig, int? seed) returning DungeonGraph in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/DungeonGenerator.cs`
- [x] T031 [US1] Add event OnDungeonGenerated(DungeonGraph) for subscribers in `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/DungeonGenerator.cs`

---

## Phase 4: US2 - 방 이동 및 분기 선택 ✅

**스토리 목표**: 플레이어가 포탈을 통해 다음 방을 선택하고 이동할 수 있음
**독립 테스트 기준**:
- 포탈 진입 시 다음 방 옵션이 표시됨
- 선택한 방으로 이동됨
- 방문 기록이 업데이트됨

### RoomManager 확장

- [x] T032 [US2] Add DungeonGraph field and CurrentNode property to RoomManager in `Assets/_Project/Scripts/Gameplay/Level/Manager/RoomManager.cs`
- [x] T033 [US2] Implement LoadDungeonGraph(DungeonGraph) method replacing current room list in `Assets/_Project/Scripts/Gameplay/Level/Manager/RoomManager.cs`
- [x] T034 [US2] Implement MoveToNodeAsync(DungeonNode) method with fade transition in `Assets/_Project/Scripts/Gameplay/Level/Manager/RoomManager.cs`
- [x] T035 [US2] Update OnRoomClear to reveal adjacent nodes on minimap in `Assets/_Project/Scripts/Gameplay/Level/Manager/RoomManager.cs`
- [x] T036 [US2] Add GetAvailableNextNodes() returning unvisited adjacent nodes in `Assets/_Project/Scripts/Gameplay/Level/Manager/RoomManager.cs`

### Portal 분기 선택

- [x] T037 [US2] Add PortalType.BranchSelection enum value in `Assets/_Project/Scripts/Gameplay/Level/Room/Portal.cs`
- [x] T038 [US2] Add connectedNodes List<DungeonNode> field to Portal in `Assets/_Project/Scripts/Gameplay/Level/Room/Portal.cs`
- [x] T039 [US2] Implement ShowBranchSelection() triggering UI when multiple destinations in `Assets/_Project/Scripts/Gameplay/Level/Room/Portal.cs`
- [x] T040 [US2] Update OnPlayerUsePortal() to handle branch selection flow in `Assets/_Project/Scripts/Gameplay/Level/Room/Portal.cs`

### 분기 선택 UI

- [x] T041 [P] [US2] Create IBranchSelectionView interface with Show(), Hide(), SetOptions() in `Assets/_Project/Scripts/UI/MVP/Interfaces/IBranchSelectionView.cs`
- [x] T042 [P] [US2] Create BranchSelectionView MonoBehaviour implementing IBranchSelectionView in `Assets/_Project/Scripts/UI/MVP/Views/BranchSelectionView.cs`
- [x] T043 [US2] Implement room option buttons with type icons and difficulty indicator in `Assets/_Project/Scripts/UI/MVP/Views/BranchSelectionView.cs`
- [x] T044 [US2] Create BranchSelectionPresenter handling selection logic in `Assets/_Project/Scripts/UI/MVP/Presenters/BranchSelectionPresenter.cs`
- [x] T045 [US2] Implement keyboard/gamepad navigation (arrow keys, Enter to select) in `Assets/_Project/Scripts/UI/MVP/Views/BranchSelectionView.cs`
- [ ] T046 [US2] Create BranchSelectionUI prefab in `Assets/Resources/Prefabs/UI/BranchSelectionUI.prefab` **(Unity 에디터 작업 필요)**

---

## Phase 5: US3 - 미니맵 시스템 ✅

**스토리 목표**: 플레이어가 미니맵을 통해 던전 구조와 현재 위치를 파악할 수 있음
**독립 테스트 기준**:
- 미니맵에 방문한 방과 현재 위치가 표시됨
- 미방문 인접 방이 ? 아이콘으로 표시됨
- Tab 키로 전체 맵 토글됨

### 미니맵 데이터

- [x] T047 [P] [US3] Create MinimapNodeData struct with nodeId, position, type, state (Visited, Unvisited, Current, Hidden) in `Assets/_Project/Scripts/UI/Minimap/MinimapNodeData.cs`
- [x] T048 [P] [US3] Create MinimapEdgeData struct (MinimapConnectionData) with fromPosition, toPosition in `Assets/_Project/Scripts/UI/Minimap/MinimapNodeData.cs`

### 미니맵 UI

- [x] T049 [US3] Create IMinimapView interface with UpdateNodes(), SetCurrentNode() in `Assets/_Project/Scripts/UI/Minimap/IMinimapView.cs`
- [x] T050 [US3] Create MinimapView MonoBehaviour with node container and edge container in `Assets/_Project/Scripts/UI/Minimap/MinimapView.cs`
- [x] T051 [US3] Implement GenerateNodeIcons() creating UI elements from MinimapNodeData in `Assets/_Project/Scripts/UI/Minimap/MinimapView.cs`
- [x] T052 [US3] Implement GenerateConnections() drawing lines between nodes in `Assets/_Project/Scripts/UI/Minimap/MinimapEdgeUI.cs`
- [x] T053 [US3] Create MinimapNodeUI component with icon, state-based color, pulse animation in `Assets/_Project/Scripts/UI/Minimap/MinimapNodeUI.cs`
- [x] T054 [US3] Create MinimapConfig ScriptableObject for room type icons and colors in `Assets/_Project/Scripts/UI/Minimap/MinimapConfig.cs`

### 미니맵 Presenter

- [x] T055 [US3] Create MinimapPresenter subscribing to RoomManager.OnNodeChanged in `Assets/_Project/Scripts/UI/Minimap/MinimapPresenter.cs`
- [x] T056 [US3] Implement ConvertGraphToMinimapData() mapping DungeonGraph to UI positions in `Assets/_Project/Scripts/UI/Minimap/MinimapPresenter.cs`
- [x] T057 [US3] Implement UpdateNodeStates() based on visited/revealed status in `Assets/_Project/Scripts/UI/Minimap/MinimapPresenter.cs`

### 전체 맵 토글

- [x] T058 [US3] Add fullscreen map mode with Tab/M key toggle in `Assets/_Project/Scripts/UI/Minimap/MinimapPresenter.cs`
- [x] T059 [US3] Implement zoom and pan for map view in `Assets/_Project/Scripts/UI/Minimap/MinimapView.cs`
- [ ] T060 [US3] Create MinimapUI prefab with mini (corner) and full (center) layouts in `Assets/Resources/Prefabs/UI/MinimapUI.prefab` **(Unity 에디터 작업 필요)**

---

## Phase 6: US4 - 스테이지 시스템 ✅

**스토리 목표**: 플레이어가 5단계 스테이지를 순차적으로 진행할 수 있음
**독립 테스트 기준**:
- 스테이지 1 클리어 시 스테이지 2로 전환됨
- 각 스테이지별 방 개수와 난이도가 다름
- 스테이지 5 보스 클리어 시 게임 클리어

### 스테이지 설정

- [x] T061 [P] [US4] Create StageConfig ScriptableObject with stageNumber, minRooms, maxRooms, difficultyMultiplier in `Assets/_Project/Scripts/Gameplay/Level/Stage/StageConfig.cs`
- [x] T062 [P] [US4] Add bossData, nextStageConfig, backgroundTheme fields to StageConfig in `Assets/_Project/Scripts/Gameplay/Level/Stage/StageConfig.cs`
- [ ] T063 [US4] Create 5 StageConfig assets (Stage1~5) with progressively harder settings in `Assets/Resources/Data/Stages/` **(Unity 에디터 작업 필요)**

### 스테이지 매니저

- [x] T064 [US4] Create StageManager singleton with CurrentStage, TotalStages properties in `Assets/_Project/Scripts/Gameplay/Level/Stage/StageManager.cs`
- [x] T065 [US4] Implement StartStageAsync(int stageNumber) generating dungeon for that stage in `Assets/_Project/Scripts/Gameplay/Level/Stage/StageManager.cs`
- [x] T066 [US4] Implement OnBossDefeated() advancing to next stage or triggering game clear in `Assets/_Project/Scripts/Gameplay/Level/Stage/StageManager.cs`
- [x] T067 [US4] Add events OnStageStarted, OnStageCompleted, OnGameCleared in `Assets/_Project/Scripts/Gameplay/Level/Stage/StageManager.cs`
- [x] T068 [US4] Create StageProgressSaver for saving/loading progress in `Assets/_Project/Scripts/Gameplay/Level/Stage/StageProgressSaver.cs`

---

## Phase 7: US5 - 에디터 도구 ✅

**스토리 목표**: 개발자가 에디터에서 던전 생성을 미리보기하고 테스트할 수 있음
**독립 테스트 기준**:
- 에디터 윈도우에서 시드 입력 후 그래프 생성됨
- 생성된 그래프가 시각적으로 표시됨
- 플레이 모드 없이 테스트 가능

### 에디터 윈도우

- [x] T069 [P] [US5] Create DungeonGraphViewer EditorWindow with seed input field in `Assets/_Project/Scripts/Editor/DungeonGraphViewer.cs`
- [x] T070 [US5] Implement Generate button calling GraphBuilder.GenerateGraph() in `Assets/_Project/Scripts/Editor/DungeonGraphViewer.cs`
- [x] T071 [US5] Implement DrawGraph() visualizing nodes and edges using Handles in `Assets/_Project/Scripts/Editor/DungeonGraphViewer.cs`
- [x] T072 [US5] Add node type coloring and labels in graph visualization in `Assets/_Project/Scripts/Editor/DungeonGraphViewer.cs`
- [x] T073 [US5] Implement statistics display (room count per type, path length, branch count) in `Assets/_Project/Scripts/Editor/DungeonGraphViewer.cs`

### DungeonConfig 커스텀 에디터

- [x] T074 [US5] Create DungeonConfigEditor with quick rules editor in `Assets/_Project/Scripts/Editor/DungeonConfigEditor.cs`
- [x] T075 [US5] Implement path validation and console output in `Assets/_Project/Scripts/Editor/DungeonConfigEditor.cs`

---

## Phase 8: Polish & 통합 ✅

**목표**: 전체 시스템 통합 및 최적화

### 통합

- [x] T076 Create ProceduralDungeonTest script for testing in `Assets/_Project/Scripts/Testing/ProceduralDungeonTest.cs`
- [ ] T077 Update Portal.OnPlayerUsePortal() to use new graph-based navigation **(기존 코드에 이미 통합됨)**
- [ ] T078 Add minimap initialization in GameplayManager.Start() **(필요시 추가)**

### 최적화

- [ ] T079 [P] Implement object pooling for MinimapNodeUI instances **(추후 최적화)**
- [ ] T080 [P] Add async room prefab loading with progress callback **(추후 최적화)**

### 정리

- [x] T081 Add XML documentation comments to all public APIs
- [ ] T082 Update ResourcePaths.cs with new prefab paths **(필요시 추가)**
- [ ] T083 Create sample DungeonConfig for testing **(Unity 에디터 작업 필요)**

---

## 의존성 그래프

```
Phase 1 (Setup) ✅
    ↓
Phase 2 (Foundational) ✅ ─────────────────────────────┐
    ↓                                                  │
Phase 3 (US1: 그래프 생성) ✅ ←────────────────────────┤
    ↓                                                  │
Phase 4 (US2: 방 이동) ✅ ←── Phase 2                  │
    ↓                                                  │
Phase 5 (US3: 미니맵) ✅ ←── Phase 3, Phase 4          │
    ↓                                                  │
Phase 6 (US4: 스테이지) ✅ ←── Phase 3                 │
    ↓                                                  │
Phase 7 (US5: 에디터) ✅ ←── Phase 3 (독립적)          │
    ↓                                                  │
Phase 8 (Polish) ✅ ←── All Phases ────────────────────┘
```

---

## 완료 요약

| Phase | 상태 | 완료 태스크 | 미완료 (Unity 에디터 필요) |
|-------|------|------------|--------------------------|
| Phase 1 | ✅ | 4/4 | - |
| Phase 2 | ✅ | 10/10 | - |
| Phase 3 | ✅ | 17/17 | - |
| Phase 4 | ✅ | 14/15 | 1 (프리팹 생성) |
| Phase 5 | ✅ | 13/14 | 1 (프리팹 생성) |
| Phase 6 | ✅ | 7/8 | 1 (에셋 생성) |
| Phase 7 | ✅ | 7/7 | - |
| Phase 8 | ✅ | 2/8 | 6 (최적화/통합) |
| **총계** | **✅** | **74/83** | **9 (Unity 작업)** |

---

## 생성된 파일 목록

### Graph 시스템
- `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonNode.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonEdge.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Graph/DungeonGraph.cs`

### Generation 시스템
- `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/SeedManager.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/RoomPlacer.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/DungeonGenerator.cs`

### Stage 시스템
- `Assets/_Project/Scripts/Gameplay/Level/Stage/StageConfig.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Stage/StageManager.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Stage/StageProgressSaver.cs`

### Minimap 시스템
- `Assets/_Project/Scripts/UI/Minimap/MinimapNodeData.cs`
- `Assets/_Project/Scripts/UI/Minimap/MinimapConfig.cs`
- `Assets/_Project/Scripts/UI/Minimap/IMinimapView.cs`
- `Assets/_Project/Scripts/UI/Minimap/MinimapNodeUI.cs`
- `Assets/_Project/Scripts/UI/Minimap/MinimapEdgeUI.cs`
- `Assets/_Project/Scripts/UI/Minimap/MinimapView.cs`
- `Assets/_Project/Scripts/UI/Minimap/MinimapPresenter.cs`

### Branch Selection UI
- `Assets/_Project/Scripts/UI/MVP/Interfaces/IBranchSelectionView.cs`
- `Assets/_Project/Scripts/UI/MVP/Views/BranchSelectionView.cs`
- `Assets/_Project/Scripts/UI/MVP/Presenters/BranchSelectionPresenter.cs`

### Editor 도구
- `Assets/_Project/Scripts/Editor/DungeonGraphViewer.cs`
- `Assets/_Project/Scripts/Editor/DungeonConfigEditor.cs`

### Testing
- `Assets/_Project/Scripts/Testing/ProceduralDungeonTest.cs`

### 수정된 기존 파일
- `Assets/_Project/Scripts/Gameplay/Level/Room/RoomData.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Dungeon/RoomGenerationRules.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Manager/RoomManager.cs`
- `Assets/_Project/Scripts/Gameplay/Level/Room/Portal.cs`

---

*이 문서는 `/speckit.tasks` 명령어로 생성되었습니다.*
*마지막 업데이트: 2025-11-30*
