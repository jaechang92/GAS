# 절차적 레벨 생성 시스템 - 구현 계획서

**기능 번호**: 016
**브랜치**: `016-procedural-level-generation`
**작성일**: 2025-11-29
**상태**: 계획 완료

---

## 1. 현재 시스템 분석

### 1.1 기존 구조 (활용 가능)

| 파일 | 역할 | 활용 방안 |
|------|------|----------|
| `RoomManager.cs` | 방 관리 싱글톤 | 그래프 기반 던전 관리로 확장 |
| `DungeonConfig.cs` | 던전 설정 SO | Procedural 모드 확장 |
| `RoomGenerationRules.cs` | 생성 룰 SO | 그래프 생성 룰로 확장 |
| `RoomData.cs` | 방 데이터 SO | 연결 정보 추가 |
| `Portal.cs` | 포탈 시스템 | 분기 선택 포탈로 확장 |
| `Room.cs` | 방 컴포넌트 | 연결된 방 정보 추가 |

### 1.2 기존 DungeonGenerationType

```
- Prefab: 고정 Room Prefab 로드
- Data: RoomData로 동적 생성
- Procedural: 룰 기반 랜덤 생성 (선형 구조 - 확장 필요)
```

### 1.3 확장 필요 사항

1. **선형 → 그래프 구조**: 현재 `rooms` List를 그래프(DungeonGraph)로 변환
2. **분기 선택**: Portal에서 다음 방 선택 UI 추가
3. **미니맵**: 현재 위치 및 진행 경로 표시
4. **시드 기반 생성**: 재현 가능한 던전 생성

---

## 2. 아키텍처 설계

### 2.1 핵심 컴포넌트

```
DungeonGenerator (신규)
├── GraphBuilder: 그래프 구조 생성
├── RoomPlacer: 방 배치 알고리즘
├── ConnectionManager: 방 연결 관리
└── SeedManager: 시드 기반 난수 생성

DungeonGraph (신규)
├── DungeonNode: 그래프 노드 (방 정보)
├── DungeonEdge: 그래프 엣지 (연결 정보)
└── GraphTraversal: 그래프 탐색 유틸리티

MinimapSystem (신규)
├── MinimapGenerator: 미니맵 생성
├── MinimapUI: UI 표시
└── MinimapNodeIcon: 노드 아이콘

Portal (확장)
├── BranchSelectionUI: 분기 선택 UI
└── ConnectedRooms: 연결된 방 목록
```

### 2.2 데이터 흐름

```
1. DungeonConfig 로드
2. SeedManager에서 시드 설정
3. GraphBuilder로 DungeonGraph 생성
4. RoomPlacer로 실제 Room 인스턴스화
5. ConnectionManager로 Portal 연결 설정
6. MinimapGenerator로 미니맵 생성
7. RoomManager에 DungeonGraph 등록
```

### 2.3 폴더 구조

```
Assets/_Project/Scripts/
├── Gameplay/
│   └── Level/
│       ├── Dungeon/
│       │   ├── DungeonConfig.cs (수정)
│       │   ├── RoomGenerationRules.cs (수정)
│       │   └── Generation/           (신규)
│       │       ├── DungeonGenerator.cs
│       │       ├── GraphBuilder.cs
│       │       ├── RoomPlacer.cs
│       │       └── SeedManager.cs
│       ├── Graph/                    (신규)
│       │   ├── DungeonGraph.cs
│       │   ├── DungeonNode.cs
│       │   └── DungeonEdge.cs
│       ├── Room/
│       │   ├── Room.cs (수정)
│       │   ├── RoomData.cs (수정)
│       │   └── Portal.cs (수정)
│       └── Manager/
│           └── RoomManager.cs (수정)
└── UI/
    └── Minimap/                      (신규)
        ├── MinimapUI.cs
        ├── MinimapNodeUI.cs
        └── MinimapConnectionUI.cs
```

---

## 3. 구현 단계

### Phase 1: 데이터 구조 (Foundation)

#### Task 1.1: DungeonGraph 데이터 구조
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Graph/`

- [ ] `DungeonNode.cs` 생성
  - nodeId: 노드 고유 ID
  - roomType: 방 타입
  - roomData: RoomData 참조
  - position: 그래프 상 위치 (층/열)
  - connections: 연결된 노드 목록
  - isVisited: 방문 여부
  - isRevealed: 미니맵 공개 여부

- [ ] `DungeonEdge.cs` 생성
  - fromNode: 출발 노드
  - toNode: 도착 노드
  - edgeType: 연결 타입 (일반, 비밀, 일방통행)

- [ ] `DungeonGraph.cs` 생성
  - nodes: Dictionary<nodeId, DungeonNode>
  - edges: List<DungeonEdge>
  - entryNode: 시작 노드
  - bossNode: 보스 노드
  - currentNode: 현재 위치
  - GetAdjacentNodes(): 인접 노드 조회
  - GetPath(): 두 노드 간 경로 계산

#### Task 1.2: 기존 데이터 확장
**파일**: 기존 파일 수정

- [ ] `RoomData.cs` 수정
  - roomConnections: 방 연결 제한 (최대 연결 수)
  - canBeBranch: 분기점 가능 여부

- [ ] `RoomGenerationRules.cs` 수정
  - branchingFactor: 분기 확률 (0~1)
  - maxBranches: 최대 분기 수
  - minPathLength: 최소 경로 길이 (시작→보스)
  - maxPathLength: 최대 경로 길이
  - secretRoomChance: 비밀 방 확률

---

### Phase 2: 그래프 생성 알고리즘

#### Task 2.1: SeedManager
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/SeedManager.cs`

- [ ] 시드 설정/획득
- [ ] Unity Random.State 관리
- [ ] 재현 가능한 랜덤 생성

#### Task 2.2: GraphBuilder
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/GraphBuilder.cs`

- [ ] `GenerateGraph(DungeonConfig, seed)` 메서드
- [ ] 층(Floor) 기반 생성 알고리즘:
  ```
  1. 시작 노드 생성 (Floor 0)
  2. 각 Floor마다 1~3개 노드 생성
  3. 이전 Floor 노드와 연결 (분기/합류)
  4. 마지막 Floor에 보스 노드
  5. 특수 방(상점, 휴식 등) 배치
  6. 비밀 방 추가 (선택적)
  ```
- [ ] 연결 규칙 검증
- [ ] 도달 가능성 검증 (모든 노드 도달 가능)

#### Task 2.3: RoomPlacer
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/Generation/RoomPlacer.cs`

- [ ] DungeonGraph → 실제 Room 인스턴스화
- [ ] RoomData 선택 로직 (타입, 난이도 매칭)
- [ ] Room 위치 계산 (Additive Scene or Container 배치)
- [ ] Portal 연결 설정

---

### Phase 3: RoomManager 통합

#### Task 3.1: RoomManager 확장
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Manager/RoomManager.cs`

- [ ] `DungeonGraph` 필드 추가
- [ ] `LoadDungeonGraph(DungeonGraph)` 메서드
- [ ] `MoveToNode(DungeonNode)` 메서드 (기존 MoveToRoomAsync 대체)
- [ ] 현재 노드 상태 관리
- [ ] 방문 기록 관리

#### Task 3.2: Portal 분기 선택
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Room/Portal.cs`

- [ ] `PortalType.BranchSelection` 추가
- [ ] `connectedNodes: List<DungeonNode>` 필드
- [ ] 분기 선택 시 UI 표시
- [ ] 선택된 노드로 이동

---

### Phase 4: 분기 선택 UI

#### Task 4.1: BranchSelectionUI
**파일**: `Assets/_Project/Scripts/UI/BranchSelectionUI.cs`

- [ ] 선택 가능한 방 목록 표시
- [ ] 방 타입 아이콘 표시
- [ ] 난이도/보상 미리보기
- [ ] 선택 확인/취소
- [ ] 키보드/게임패드 지원

#### Task 4.2: BranchSelectionPresenter (MVP 패턴)
**파일**: `Assets/_Project/Scripts/UI/MVP/Presenters/BranchSelectionPresenter.cs`

- [ ] 방 정보 포맷팅
- [ ] 선택 이벤트 처리
- [ ] RoomManager와 연동

---

### Phase 5: 미니맵 시스템

#### Task 5.1: MinimapUI
**파일**: `Assets/_Project/Scripts/UI/Minimap/MinimapUI.cs`

- [ ] Canvas 기반 UI
- [ ] 노드 아이콘 동적 생성
- [ ] 연결선 표시
- [ ] 현재 위치 강조
- [ ] 방문/미방문/미공개 상태 표시
- [ ] 확대/축소 지원

#### Task 5.2: MinimapNodeUI
**파일**: `Assets/_Project/Scripts/UI/Minimap/MinimapNodeUI.cs`

- [ ] 노드 타입별 아이콘
- [ ] 상태별 색상
- [ ] 클릭 이벤트 (방 정보 표시)
- [ ] 애니메이션 (현재 위치)

#### Task 5.3: MinimapPresenter
**파일**: `Assets/_Project/Scripts/UI/MVP/Presenters/MinimapPresenter.cs`

- [ ] DungeonGraph → UI 매핑
- [ ] 실시간 업데이트
- [ ] 토글 표시/숨김

---

### Phase 6: 스테이지 시스템

#### Task 6.1: StageConfig
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/StageConfig.cs`

- [ ] 스테이지별 설정 SO
- [ ] 방 개수 범위
- [ ] 적 난이도 배율
- [ ] 보스 데이터
- [ ] 다음 스테이지 정보

#### Task 6.2: StageManager
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Manager/StageManager.cs`

- [ ] 현재 스테이지 관리
- [ ] 스테이지 전환
- [ ] 스테이지 진행도 저장

---

### Phase 7: 테스트 및 밸런싱

#### Task 7.1: 에디터 테스트 도구
**파일**: `Assets/_Project/Scripts/Editor/DungeonGeneratorEditor.cs`

- [ ] 던전 미리보기 생성
- [ ] 시드 입력 및 테스트
- [ ] 그래프 시각화 (에디터 윈도우)

#### Task 7.2: 런타임 테스트
- [ ] 다양한 시드로 생성 테스트
- [ ] 방 타입 분포 검증
- [ ] 경로 길이 검증
- [ ] 성능 테스트 (생성 시간)

---

## 4. 기술 명세

### 4.1 그래프 생성 알고리즘 (Slay the Spire 스타일)

```csharp
// 층(Floor) 기반 생성
for (int floor = 0; floor < totalFloors; floor++)
{
    int nodeCount = GetNodeCountForFloor(floor);

    for (int i = 0; i < nodeCount; i++)
    {
        // 노드 생성
        var node = CreateNode(floor, i);

        // 이전 층 노드와 연결
        ConnectToPreviousFloor(node, floor);
    }
}

// 연결 규칙
- 첫 층: 시작 노드 1개
- 중간 층: 1~3개 노드, 이전 층에서 분기/합류
- 마지막 층 전: 휴식/상점 방
- 마지막 층: 보스 노드 1개 (모든 경로가 합류)
```

### 4.2 방 타입 분포 (스테이지당)

| 스테이지 | 전투 방 | 엘리트 방 | 휴식 방 | 상점 | 보물 방 | 보스 |
|---------|---------|----------|---------|------|--------|------|
| 1 | 3-4 | 1 | 1 | 1 | 0-1 | 1 |
| 2 | 4-5 | 1-2 | 1 | 1 | 0-1 | 1 |
| 3 | 5-6 | 2 | 1 | 1 | 1 | 1 |
| 4 | 6-7 | 2-3 | 1 | 1 | 1 | 1 |
| 5 | 7-8 | 3 | 1 | 1 | 1 | 1 (최종) |

### 4.3 연결 규칙

- 모든 노드는 최소 1개 입력 연결
- 모든 노드는 최소 1개 출력 연결 (보스 제외)
- 연결선은 교차하지 않음 (가능한 범위 내)
- 시작 → 보스 경로 항상 존재

---

## 5. 의존성 및 제약사항

### 5.1 의존성

- Unity UI System (Canvas, Image, Button)
- 기존 RoomManager, Room, Portal 시스템
- GameFlowStateMachine (씬 전환)
- MVP UI 패턴 (기존 구조)

### 5.2 제약사항

- Coroutine 대신 Awaitable 사용
- 500줄 이상 파일 분할
- 한글 주석 허용
- Unity 6.0+ (linearVelocity 등)

---

## 6. 예상 작업량

| Phase | 작업 내용 | 예상 규모 |
|-------|----------|----------|
| Phase 1 | 데이터 구조 | 소형 |
| Phase 2 | 그래프 생성 알고리즘 | 중형 |
| Phase 3 | RoomManager 통합 | 중형 |
| Phase 4 | 분기 선택 UI | 소형 |
| Phase 5 | 미니맵 시스템 | 중형 |
| Phase 6 | 스테이지 시스템 | 소형 |
| Phase 7 | 테스트 및 밸런싱 | 소형 |

**총 예상 규모**: 대형 (여러 핵심 시스템 신규 개발)

---

## 7. 다음 단계

이 계획서가 승인되면 `/speckit.tasks`를 실행하여 구체적인 태스크 목록을 생성합니다.

```bash
/speckit.tasks
```

---

*이 문서는 `/speckit.plan` 명령어로 생성되었습니다.*
