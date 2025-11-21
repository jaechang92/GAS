# 🎮 GASPT 프로젝트 상세 로드맵

**프로젝트명**: GASPT (Generic Ability System + FSM Platform Game)
**장르**: 로그라이크 플랫포머 (Skul: The Hero Slayer 스타일)
**현재 버전**: Phase D 완료 직전 (약 70%)
**최종 업데이트**: 2025-11-19

> 이 문서는 각 기능을 **실제로 구현할 수 있을 정도로 상세하게** 설명합니다.
> 무엇을 만드는지, 왜 필요한지, 어떻게 동작하는지, 어떤 순서로 만드는지를 명확히 제시합니다.

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [현재 완료 상태 요약](#현재-완료-상태-요약)
3. [Phase E-1: 절차적 레벨 생성 시스템](#phase-e-1-절차적-레벨-생성-시스템)
4. [Phase E-2: 스컬 교체 시스템](#phase-e-2-스컬-교체-시스템)
5. [Phase E-3: 메타 진행 시스템](#phase-e-3-메타-진행-시스템)
6. [Phase E-4: 밸런싱 및 콘텐츠](#phase-e-4-밸런싱-및-콘텐츠)
7. [Phase F: 최적화 및 배포](#phase-f-최적화-및-배포)

---

## 🎯 프로젝트 개요

### 게임 컨셉
- **장르**: 2D 로그라이크 플랫포머
- **핵심 루프**:
  1. 던전 입장 → 방마다 적 처치 → 아이템 획득 → 스컬 강화
  2. 보스 처치 → 보상 획득 → 다음 던전 or 메타 업그레이드
  3. 죽으면 처음부터, 메타 화폐로 영구 업그레이드
- **참고 게임**: Skul: The Hero Slayer, Hades
- **타겟 플레이 시간**: 1회 런당 20-40분, 총 플레이 30시간 이상

### 기술 스택
- Unity 2023.x / C# with async Awaitable
- GAS + FSM + Panel UI + ScriptableObject
- Object Pooling + Singleton 패턴

---

## 📊 현재 완료 상태 요약

### ✅ 완료 (Phase A~D, 70%)
- **Core 시스템**: GAS, FSM, ObjectPool, SaveSystem
- **전투 시스템**: DamageCalculator, ComboSystem, Enemy AI
- **아이템 시스템**: Item, LootSystem, InventorySystem, PlayerStats
- **UI 시스템**: BaseUI, InventoryUI, PortalUI, DungeonCompleteUI, HUD
- **던전 시스템**: Portal, 방 이동, 적 스폰, 보스 전투

### ⏳ 다음 작업 (Phase E, 0%)
지금부터 **Phase E: 로그라이크 콘텐츠 확장**을 시작합니다.
이 문서는 Phase E를 **실제로 구현할 수 있도록 매우 상세하게** 설명합니다.

---

# 🎮 Phase E-1: 절차적 레벨 생성 시스템

**목표**: 매번 다른 던전 레이아웃을 자동으로 생성하여 재플레이 가치를 높임
**예상 기간**: 2-3주
**전체 진행률**: 0% → 100%

---

## 📍 E-1-1: Room Generator (방 생성기)

### 🎯 무엇을 만드는가?
**RoomData**를 기반으로 **Tilemap**을 자동 생성하는 시스템입니다.
- 바닥 타일 배치
- 플랫폼 랜덤 배치 (점프로 올라갈 수 있는 발판)
- 장애물 랜덤 배치 (가시, 함정)

### 🔍 왜 필요한가?
현재는 씬에 수동으로 배치한 고정된 방만 존재합니다.
절차적 생성을 통해:
- 매번 다른 레이아웃으로 플레이 가능
- 방 타입(Normal, Elite, Boss)에 따라 다른 구조 생성
- 디자이너가 일일이 방을 만들 필요 없음

### 🏗️ 아키텍처

**클래스 구조**:
```
RoomGenerator (MonoBehaviour)
├── GenerateRoom(RoomData roomData)
├── ClearRoom()
├── GenerateGround(RoomData)
├── GeneratePlatforms(RoomData)
└── GenerateObstacles(RoomData)

RoomData (ScriptableObject) - 기존 파일 확장
├── roomType (RoomType Enum)
├── roomWidth, roomHeight (int)
├── platformCount (int)
├── obstacleCount (int)
├── goldReward, expReward (int)
└── enemySpawnPoints (기존 필드)

RoomType (Enum) - 새로 생성
├── Start
├── Normal
├── Elite
├── Boss
├── Treasure
├── Shop
└── Rest
```

**데이터 흐름**:
```
1. DungeonManager가 RoomData 로드
2. RoomGenerator.GenerateRoom(roomData) 호출
3. RoomGenerator가 Tilemap에 타일 배치
4. 완료 후 적 스폰, 플레이어 배치
```

### 🧩 필요한 컴포넌트

#### C# 스크립트
1. `RoomType.cs` (Enum, 새로 생성)
2. `RoomData.cs` (기존 파일 확장)
3. `RoomGenerator.cs` (MonoBehaviour, 새로 생성)

#### Unity GameObject/Prefab
1. **Room GameObject** (씬에 배치)
   - Tilemap: Ground (바닥)
   - Tilemap: Platform (발판)
   - Tilemap: Obstacle (장애물)
   - RoomGenerator (컴포넌트)

#### Tile Assets (Sprite)
1. GroundTile.asset (TileBase)
2. PlatformTile.asset (TileBase)
3. SpikeTile.asset (TileBase)

#### ScriptableObject Assets
- RoomData 여러 개 (Normal, Elite, Boss 등)

### 🔧 Unity 설정

#### 1. Tilemap 생성
```
Hierarchy:
Room
├── Grid
    ├── Ground (Tilemap, Tilemap Renderer)
    ├── Platform (Tilemap, Tilemap Renderer)
    └── Obstacle (Tilemap, Tilemap Renderer)
```

**Tilemap 설정**:
- Ground: Sorting Layer "Ground", Order in Layer 0
- Platform: Sorting Layer "Ground", Order in Layer 1
- Obstacle: Sorting Layer "Obstacles", Order in Layer 2

#### 2. Tile Palette 생성
1. Window > 2D > Tile Palette
2. Create New Palette: "RoomTiles"
3. 임시 Sprite 생성 (64x64 흰색 사각형)
   - GroundSprite.png (회색)
   - PlatformSprite.png (갈색)
   - SpikeSprite.png (빨간색)
4. Sprite를 Tile로 변환:
   - Assets > Create > 2D > Tiles > Tile
   - Sprite 할당
   - `Assets/_Project/Art/Tiles/` 폴더에 저장

#### 3. RoomGenerator GameObject 설정
1. Hierarchy에서 Room 선택
2. Add Component > RoomGenerator
3. Inspector에서 참조 할당:
   - Ground Tilemap: Ground
   - Platform Tilemap: Platform
   - Obstacle Tilemap: Obstacle
   - Ground Tile: GroundTile.asset
   - Platform Tile: PlatformTile.asset
   - Spike Tile: SpikeTile.asset

### 📝 구현 단계

#### Step 1: RoomType Enum 생성 (5분)
**파일**: `Assets/_Project/Scripts/Core/Enums/RoomType.cs`

```csharp
namespace Core.Enums
{
    /// <summary>
    /// 방 타입
    /// </summary>
    public enum RoomType
    {
        Start,      // 시작 방
        Normal,     // 일반 전투 방
        Elite,      // 엘리트 전투 방
        Boss,       // 보스 방
        Treasure,   // 보물 방
        Shop,       // 상점 방
        Rest        // 휴식 방
    }
}
```

**체크리스트**:
- [ ] 파일 생성
- [ ] 7가지 타입 정의
- [ ] Core.Enums 네임스페이스 확인

---

#### Step 2: RoomData 확장 (10분)
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Room/RoomData.cs` (기존 파일 수정)

**추가할 필드**:
```csharp
using Core.Enums;

[CreateAssetMenu(fileName = "RoomData", menuName = "GASPT/Level/Room Data")]
public class RoomData : ScriptableObject
{
    // 기존 필드는 그대로 유지
    // ...

    // 새로 추가할 필드
    [Header("Room Type")]
    public RoomType roomType = RoomType.Normal;

    [Header("Room Properties")]
    [Tooltip("방 너비 (타일 단위)")]
    [Range(10, 50)]
    public int roomWidth = 20;

    [Tooltip("방 높이 (타일 단위)")]
    [Range(10, 30)]
    public int roomHeight = 15;

    [Tooltip("플랫폼 개수")]
    [Range(0, 10)]
    public int platformCount = 3;

    [Tooltip("장애물 개수")]
    [Range(0, 5)]
    public int obstacleCount = 1;

    [Header("Rewards")]
    public int goldReward = 50;
    public int expReward = 100;
}
```

**체크리스트**:
- [ ] using Core.Enums 추가
- [ ] roomType 필드 추가
- [ ] roomWidth, roomHeight 추가
- [ ] platformCount, obstacleCount 추가
- [ ] goldReward, expReward 추가

---

#### Step 3: RoomGenerator 클래스 생성 (1시간)
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Room/RoomGenerator.cs`

**전체 코드**:
```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GASPT.Level
{
    /// <summary>
    /// 방 생성기
    /// RoomData를 기반으로 Tilemap 생성
    /// </summary>
    public class RoomGenerator : MonoBehaviour
    {
        [Header("Tilemap References")]
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap platformTilemap;
        [SerializeField] private Tilemap obstacleTilemap;

        [Header("Tiles")]
        [SerializeField] private TileBase groundTile;
        [SerializeField] private TileBase platformTile;
        [SerializeField] private TileBase spikeTile;

        /// <summary>
        /// RoomData를 기반으로 방 생성
        /// </summary>
        public void GenerateRoom(RoomData roomData)
        {
            if (roomData == null)
            {
                Debug.LogError("[RoomGenerator] RoomData is null");
                return;
            }

            ClearRoom();
            GenerateGround(roomData);
            GeneratePlatforms(roomData);
            GenerateObstacles(roomData);

            Debug.Log($"[RoomGenerator] Room generated: {roomData.roomType}");
        }

        private void ClearRoom()
        {
            if (groundTilemap != null) groundTilemap.ClearAllTiles();
            if (platformTilemap != null) platformTilemap.ClearAllTiles();
            if (obstacleTilemap != null) obstacleTilemap.ClearAllTiles();
        }

        private void GenerateGround(RoomData roomData)
        {
            if (groundTilemap == null || groundTile == null)
                return;

            // 바닥 한 줄 생성
            for (int x = 0; x < roomData.roomWidth; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, 0, 0);
                groundTilemap.SetTile(tilePos, groundTile);
            }
        }

        private void GeneratePlatforms(RoomData roomData)
        {
            if (platformTilemap == null || platformTile == null)
                return;

            for (int i = 0; i < roomData.platformCount; i++)
            {
                int platformWidth = Random.Range(3, 8);
                int platformX = Random.Range(2, roomData.roomWidth - platformWidth - 2);
                int platformY = Random.Range(3, roomData.roomHeight - 2);

                for (int x = 0; x < platformWidth; x++)
                {
                    Vector3Int tilePos = new Vector3Int(platformX + x, platformY, 0);
                    platformTilemap.SetTile(tilePos, platformTile);
                }
            }
        }

        private void GenerateObstacles(RoomData roomData)
        {
            if (obstacleTilemap == null || spikeTile == null)
                return;

            for (int i = 0; i < roomData.obstacleCount; i++)
            {
                int obstacleX = Random.Range(2, roomData.roomWidth - 2);
                Vector3Int tilePos = new Vector3Int(obstacleX, 1, 0);
                obstacleTilemap.SetTile(tilePos, spikeTile);
            }
        }
    }
}
```

**체크리스트**:
- [ ] 파일 생성
- [ ] 네임스페이스 확인 (GASPT.Level)
- [ ] GenerateRoom() 구현
- [ ] ClearRoom() 구현
- [ ] GenerateGround() 구현
- [ ] GeneratePlatforms() 구현
- [ ] GenerateObstacles() 구현

---

#### Step 4: Unity에서 Tilemap 설정 (30분)

**4-1. Grid + Tilemap 생성**:
1. Hierarchy 우클릭 > 2D Object > Tilemap > Rectangular
2. Grid를 "Room"으로 이름 변경
3. Tilemap을 "Ground"로 이름 변경
4. Ground 복제 → "Platform", "Obstacle"로 이름 변경

**4-2. Sorting Layer 설정**:
1. Edit > Project Settings > Tags and Layers
2. Sorting Layers 추가:
   - Ground
   - Obstacles
3. Ground Tilemap:
   - Tilemap Renderer > Sorting Layer: Ground
   - Order in Layer: 0
4. Platform Tilemap:
   - Sorting Layer: Ground
   - Order in Layer: 1
5. Obstacle Tilemap:
   - Sorting Layer: Obstacles
   - Order in Layer: 0

**체크리스트**:
- [ ] Grid + Tilemap 3개 생성 (Ground, Platform, Obstacle)
- [ ] Sorting Layer 설정
- [ ] Collider 설정 (Tilemap Collider 2D 추가)

---

#### Step 5: Tile Assets 생성 (30분)

**5-1. 임시 Sprite 생성** (나중에 아트 에셋으로 교체):
1. 외부 툴(Paint, Photoshop 등)에서 64x64 사각형 이미지 3개 생성:
   - GroundSprite.png (회색 #808080)
   - PlatformSprite.png (갈색 #8B4513)
   - SpikeSprite.png (빨간색 #FF0000)
2. Unity로 Import: `Assets/_Project/Art/Tiles/Sprites/`
3. Inspector에서 Texture Type: Sprite (2D and UI)
4. Apply

**5-2. Tile 생성**:
1. Project 우클릭 > Create > 2D > Tiles > Tile
2. "GroundTile" 생성
3. Inspector에서 Sprite: GroundSprite 할당
4. 동일하게 PlatformTile, SpikeTile 생성
5. `Assets/_Project/Art/Tiles/` 폴더에 저장

**체크리스트**:
- [ ] Sprite 3개 생성 및 Import
- [ ] Tile 3개 생성 (GroundTile, PlatformTile, SpikeTile)
- [ ] Sprite 할당 확인

---

#### Step 6: RoomGenerator Component 설정 (10분)

1. Hierarchy에서 Room GameObject 선택
2. Add Component > RoomGenerator
3. Inspector에서 참조 할당:
   - Ground Tilemap: Room/Ground
   - Platform Tilemap: Room/Platform
   - Obstacle Tilemap: Room/Obstacle
   - Ground Tile: GroundTile
   - Platform Tile: PlatformTile
   - Spike Tile: SpikeTile

**체크리스트**:
- [ ] RoomGenerator 컴포넌트 추가
- [ ] Tilemap 참조 6개 모두 할당
- [ ] None (Tilemap) 없는지 확인

---

#### Step 7: RoomData ScriptableObject 생성 (10분)

1. Project 우클릭 > Create > GASPT > Level > Room Data
2. "NormalRoom" 생성
3. Inspector에서 설정:
   - Room Type: Normal
   - Room Width: 20
   - Room Height: 15
   - Platform Count: 3
   - Obstacle Count: 1
   - Gold Reward: 50
   - Exp Reward: 100
4. `Assets/_Project/Data/Rooms/` 폴더에 저장
5. 동일하게 EliteRoom, BossRoom 생성

**체크리스트**:
- [ ] NormalRoom.asset 생성
- [ ] EliteRoom.asset 생성
- [ ] BossRoom.asset 생성
- [ ] 각각 설정값 입력

---

#### Step 8: 테스트 (20분)

**8-1. 테스트 씬 생성**:
1. Assets > Scenes > "RoomGeneratorTest" 생성
2. Room GameObject를 씬에 배치
3. RoomGenerator 컴포넌트 확인

**8-2. 테스트 스크립트 생성**:
**파일**: `Assets/_Project/Scripts/Testing/RoomGeneratorTest.cs`

```csharp
using UnityEngine;
using GASPT.Level;

public class RoomGeneratorTest : MonoBehaviour
{
    [SerializeField] private RoomGenerator roomGenerator;
    [SerializeField] private RoomData testRoomData;

    private void Start()
    {
        if (roomGenerator != null && testRoomData != null)
        {
            roomGenerator.GenerateRoom(testRoomData);
        }
    }

    // Context Menu for testing in Editor
    [ContextMenu("Generate Room")]
    private void TestGenerateRoom()
    {
        if (roomGenerator != null && testRoomData != null)
        {
            roomGenerator.GenerateRoom(testRoomData);
        }
    }
}
```

**8-3. 테스트 실행**:
1. 빈 GameObject 생성 → "RoomGeneratorTest"
2. RoomGeneratorTest 컴포넌트 추가
3. Inspector에서:
   - Room Generator: Room
   - Test Room Data: NormalRoom
4. Play Mode 실행
5. 바닥, 플랫폼, 장애물이 생성되는지 확인
6. Context Menu "Generate Room" 실행하여 여러 번 생성 테스트

**체크리스트**:
- [ ] 바닥이 가로로 생성됨
- [ ] 플랫폼이 랜덤 위치에 생성됨
- [ ] 장애물이 바닥 위에 생성됨
- [ ] 여러 번 생성 시 다른 위치에 생성됨
- [ ] Console에 에러 없음

---

### 📋 Room Generator 완료 체크리스트

- [ ] RoomType Enum 생성
- [ ] RoomData 확장 (필드 추가)
- [ ] RoomGenerator 클래스 생성
- [ ] Unity Tilemap 설정 (Grid, 3개 Tilemap)
- [ ] Tile Assets 생성 (Sprite, Tile)
- [ ] RoomGenerator Component 설정
- [ ] RoomData ScriptableObject 3개 생성
- [ ] 테스트 스크립트 작성
- [ ] Play Mode에서 동작 확인

**총 예상 시간**: 약 3-4시간

---

## 📍 E-1-2: Dungeon Generator (던전 생성기)

### 🎯 무엇을 만드는가?
**여러 개의 방을 연결하여 던전 레이아웃을 생성**하는 시스템입니다.
- 시작 방 → N개의 방 → 보스 방으로 이어지는 경로 생성
- 각 방의 타입 결정 (Normal, Elite, Treasure, Shop, Rest)
- 방과 방을 연결하는 그래프 구조 생성

### 🔍 왜 필요한가?
현재는 단일 방만 존재합니다. 던전 생성기를 통해:
- 5~10개 방으로 구성된 던전 생성
- 시작 방 → 보스 방까지 보장된 경로 제공
- 선택적 경로 (보물 방, 상점 방 등) 추가로 재플레이 가치 향상

### 🏗️ 아키텍처

**클래스 구조**:
```
DungeonGenerator (MonoBehaviour, Singleton)
├── GenerateDungeon() -> DungeonLayout
├── DetermineRoomType(int index, int total) -> RoomType
└── LoadRoom(int roomId)

DungeonLayout (일반 클래스)
├── List<DungeonNode> nodes
├── int startNodeId
├── int bossNodeId
├── AddNode(DungeonNode)
├── ConnectNodes(int, int)
└── GetNode(int) -> DungeonNode

DungeonNode (일반 클래스)
├── int nodeId
├── RoomType roomType
├── Vector2Int gridPosition
├── List<int> connectedNodes
├── RoomData roomData
└── bool isVisited
```

**데이터 흐름**:
```
1. 게임 시작 또는 새 던전 진입
2. DungeonGenerator.GenerateDungeon() 호출
3. DungeonLayout 생성 (노드 + 연결)
4. DungeonLayout을 MinimapUI에 전달 (미니맵 표시)
5. 플레이어가 Portal 사용 시 DungeonGenerator.LoadRoom(nextRoomId) 호출
6. RoomGenerator.GenerateRoom(roomData) 호출하여 방 생성
```

### 🧩 필요한 컴포넌트

#### C# 스크립트
1. `DungeonLayout.cs` (일반 클래스)
2. `DungeonNode.cs` (일반 클래스)
3. `DungeonGenerator.cs` (MonoBehaviour, Singleton)

#### Unity GameObject
1. **DungeonGenerator GameObject** (씬에 배치, DontDestroyOnLoad)
   - DungeonGenerator 컴포넌트

#### ScriptableObject Assets
- 없음 (DungeonLayout은 런타임 생성)

### 📝 구현 단계

#### Step 1: DungeonNode 클래스 생성 (15분)
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/DungeonNode.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;
using Core.Enums;

namespace GASPT.Level
{
    /// <summary>
    /// 던전 노드 (방 하나를 나타냄)
    /// </summary>
    [System.Serializable]
    public class DungeonNode
    {
        public int nodeId;
        public RoomType roomType;
        public Vector2Int gridPosition;
        public List<int> connectedNodes = new List<int>();
        public RoomData roomData; // 해당 방의 RoomData
        public bool isVisited = false;

        public DungeonNode(int id, RoomType type, Vector2Int position)
        {
            nodeId = id;
            roomType = type;
            gridPosition = position;
        }
    }
}
```

**체크리스트**:
- [ ] 파일 생성
- [ ] 필드 5개 정의
- [ ] 생성자 구현

---

#### Step 2: DungeonLayout 클래스 생성 (20분)
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/DungeonLayout.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace GASPT.Level
{
    /// <summary>
    /// 던전 레이아웃
    /// 방들의 연결 구조
    /// </summary>
    public class DungeonLayout
    {
        public List<DungeonNode> nodes = new List<DungeonNode>();
        public int startNodeId;
        public int bossNodeId;
        public int currentNodeId;

        public void AddNode(DungeonNode node)
        {
            nodes.Add(node);
        }

        public void ConnectNodes(int nodeId1, int nodeId2)
        {
            DungeonNode node1 = GetNode(nodeId1);
            DungeonNode node2 = GetNode(nodeId2);

            if (node1 != null && node2 != null)
            {
                if (!node1.connectedNodes.Contains(nodeId2))
                    node1.connectedNodes.Add(nodeId2);

                if (!node2.connectedNodes.Contains(nodeId1))
                    node2.connectedNodes.Add(nodeId1);
            }
        }

        public DungeonNode GetNode(int nodeId)
        {
            return nodes.Find(n => n.nodeId == nodeId);
        }

        public DungeonNode GetCurrentNode()
        {
            return GetNode(currentNodeId);
        }
    }
}
```

**체크리스트**:
- [ ] 파일 생성
- [ ] AddNode() 구현
- [ ] ConnectNodes() 구현 (양방향 연결)
- [ ] GetNode() 구현
- [ ] GetCurrentNode() 구현

---

#### Step 3: DungeonGenerator 클래스 생성 (1시간)
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/DungeonGenerator.cs`

```csharp
using UnityEngine;
using Core.Enums;

namespace GASPT.Level
{
    /// <summary>
    /// 던전 생성기
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        public static DungeonGenerator Instance { get; private set; }

        [Header("Dungeon Settings")]
        [Range(1, 10)]
        public int floorCount = 3;

        [Range(3, 10)]
        public int minRoomsPerFloor = 5;

        [Range(5, 15)]
        public int maxRoomsPerFloor = 10;

        [Header("Room Data Assets")]
        [SerializeField] private RoomData normalRoomData;
        [SerializeField] private RoomData eliteRoomData;
        [SerializeField] private RoomData bossRoomData;
        [SerializeField] private RoomData treasureRoomData;

        [Header("References")]
        [SerializeField] private RoomGenerator roomGenerator;

        private DungeonLayout currentDungeon;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public DungeonLayout GenerateDungeon()
        {
            DungeonLayout layout = new DungeonLayout();

            int roomsPerFloor = Random.Range(minRoomsPerFloor, maxRoomsPerFloor + 1);
            int totalRooms = roomsPerFloor * floorCount;

            // 노드 생성
            for (int i = 0; i < totalRooms; i++)
            {
                RoomType roomType = DetermineRoomType(i, totalRooms);
                Vector2Int gridPos = new Vector2Int(i % roomsPerFloor, i / roomsPerFloor);

                DungeonNode node = new DungeonNode(i, roomType, gridPos);
                node.roomData = GetRoomDataForType(roomType);
                layout.AddNode(node);

                if (i == 0) layout.startNodeId = i;
                if (i == totalRooms - 1) layout.bossNodeId = i;
            }

            // 노드 연결 (선형 경로 + 랜덤 분기)
            for (int i = 0; i < totalRooms - 1; i++)
            {
                layout.ConnectNodes(i, i + 1);

                // 랜덤 분기 (30% 확률)
                if (Random.value > 0.7f && i + 2 < totalRooms)
                {
                    layout.ConnectNodes(i, i + 2);
                }
            }

            layout.currentNodeId = layout.startNodeId;
            currentDungeon = layout;

            Debug.Log($"[DungeonGenerator] Dungeon generated: {totalRooms} rooms");
            return layout;
        }

        private RoomType DetermineRoomType(int index, int totalRooms)
        {
            if (index == 0) return RoomType.Start;
            if (index == totalRooms - 1) return RoomType.Boss;

            float rand = Random.value;

            if (rand < 0.5f) return RoomType.Normal;
            if (rand < 0.7f) return RoomType.Elite;
            if (rand < 0.85f) return RoomType.Treasure;
            if (rand < 0.95f) return RoomType.Shop;
            return RoomType.Rest;
        }

        private RoomData GetRoomDataForType(RoomType type)
        {
            return type switch
            {
                RoomType.Normal => normalRoomData,
                RoomType.Elite => eliteRoomData,
                RoomType.Boss => bossRoomData,
                RoomType.Treasure => treasureRoomData,
                _ => normalRoomData
            };
        }

        public void LoadRoom(int roomId)
        {
            if (currentDungeon == null)
            {
                Debug.LogError("[DungeonGenerator] No dungeon generated");
                return;
            }

            DungeonNode node = currentDungeon.GetNode(roomId);
            if (node == null)
            {
                Debug.LogError($"[DungeonGenerator] Room {roomId} not found");
                return;
            }

            node.isVisited = true;
            currentDungeon.currentNodeId = roomId;

            if (roomGenerator != null && node.roomData != null)
            {
                roomGenerator.GenerateRoom(node.roomData);
            }

            Debug.Log($"[DungeonGenerator] Loaded room {roomId}: {node.roomType}");
        }

        public DungeonLayout GetCurrentDungeon() => currentDungeon;
    }
}
```

**체크리스트**:
- [ ] Singleton 패턴 구현
- [ ] GenerateDungeon() 구현
- [ ] DetermineRoomType() 구현
- [ ] GetRoomDataForType() 구현
- [ ] LoadRoom() 구현
- [ ] GetCurrentDungeon() 구현

---

#### Step 4: Unity에서 DungeonGenerator 설정 (15분)

1. Hierarchy 우클릭 > Create Empty → "DungeonGenerator"
2. Add Component > DungeonGenerator
3. Inspector에서 설정:
   - Floor Count: 3
   - Min Rooms Per Floor: 5
   - Max Rooms Per Floor: 8
   - Normal Room Data: NormalRoom
   - Elite Room Data: EliteRoom
   - Boss Room Data: BossRoom
   - Treasure Room Data: (아직 없으면 NormalRoom 할당)
   - Room Generator: Room (씬의 RoomGenerator)

**체크리스트**:
- [ ] DungeonGenerator GameObject 생성
- [ ] Component 추가
- [ ] 설정값 입력
- [ ] RoomData 할당 (4개)
- [ ] RoomGenerator 참조 할당

---

#### Step 5: 테스트 (20분)

**파일**: `Assets/_Project/Scripts/Testing/DungeonGeneratorTest.cs`

```csharp
using UnityEngine;
using GASPT.Level;

public class DungeonGeneratorTest : MonoBehaviour
{
    private void Start()
    {
        TestGenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    private void TestGenerateDungeon()
    {
        DungeonLayout layout = DungeonGenerator.Instance.GenerateDungeon();

        Debug.Log("=== Dungeon Layout ===");
        Debug.Log($"Total Rooms: {layout.nodes.Count}");
        Debug.Log($"Start: {layout.startNodeId}, Boss: {layout.bossNodeId}");

        foreach (var node in layout.nodes)
        {
            Debug.Log($"Room {node.nodeId}: {node.roomType}, Connections: {string.Join(", ", node.connectedNodes)}");
        }
    }

    [ContextMenu("Load Start Room")]
    private void TestLoadStartRoom()
    {
        DungeonLayout layout = DungeonGenerator.Instance.GetCurrentDungeon();
        if (layout != null)
        {
            DungeonGenerator.Instance.LoadRoom(layout.startNodeId);
        }
    }
}
```

**테스트 절차**:
1. GameObject 생성 → "DungeonGeneratorTest"
2. DungeonGeneratorTest 컴포넌트 추가
3. Play Mode 실행
4. Console에서 던전 구조 확인:
   - 총 방 개수 (예: 15~24개)
   - 각 방의 타입 (Start, Normal, Elite, Boss...)
   - 연결 정보 (예: Room 0 → 1, 2)
5. Context Menu "Load Start Room" 실행
6. Scene View에서 방이 생성되는지 확인

**체크리스트**:
- [ ] 던전 생성됨 (방 개수 확인)
- [ ] 시작 방과 보스 방 설정됨
- [ ] 방 타입이 다양함 (Normal, Elite, Treasure 등)
- [ ] 방 연결 정보 출력됨
- [ ] LoadRoom() 호출 시 Tilemap에 방이 생성됨

---

### 📋 Dungeon Generator 완료 체크리스트

- [ ] DungeonNode 클래스 생성
- [ ] DungeonLayout 클래스 생성
- [ ] DungeonGenerator 클래스 생성 (Singleton)
- [ ] Unity에서 DungeonGenerator GameObject 설정
- [ ] RoomData 4개 할당
- [ ] 테스트 스크립트 작성
- [ ] Play Mode에서 던전 생성 확인
- [ ] LoadRoom() 동작 확인

**총 예상 시간**: 약 2-3시간

---

## 📍 E-1-3: Minimap UI (미니맵 시스템)

### 🎯 무엇을 만드는가?
**던전 레이아웃을 시각적으로 표시하는 미니맵 UI**입니다.
- 화면 우측 상단에 작은 미니맵 표시
- 각 방을 작은 아이콘으로 표시
- 현재 방을 하이라이트
- 방 타입별로 다른 아이콘 (시작, 보스, 보물 등)

### 🔍 왜 필요한가?
플레이어가 던전 구조를 파악하고 탐색 경로를 계획할 수 있게 합니다.
- 현재 위치 확인
- 보스 방까지의 거리 파악
- 보물 방, 상점 방 위치 확인

### 🏗️ 아키텍처

**클래스 구조**:
```
MinimapUI (BaseUI 상속)
├── GenerateMinimap(DungeonLayout)
├── UpdateCurrentRoom(int roomId)
├── ClearMinimap()
└── CreateRoomIcon(DungeonNode) -> GameObject

RoomIconUI (MonoBehaviour)
├── nodeId (int)
├── iconImage (Image)
└── SetHighlight(bool)
```

**데이터 흐름**:
```
1. DungeonGenerator가 DungeonLayout 생성
2. MinimapUI.GenerateMinimap(layout) 호출
3. 각 DungeonNode마다 RoomIcon 생성
4. 플레이어가 방 이동 시 MinimapUI.UpdateCurrentRoom(roomId) 호출
5. 이전 방 하이라이트 해제, 새 방 하이라이트
```

### 🧩 필요한 컴포넌트

#### C# 스크립트
1. `MinimapUI.cs` (BaseUI 상속)
2. `RoomIconUI.cs` (MonoBehaviour, 선택사항)

#### Unity GameObject/Prefab
1. **MinimapUI GameObject** (Canvas 하위)
   - MinimapUI (컴포넌트)
   - Panel (자식)
     - MinimapContainer (RectTransform, Grid Layout Group)

2. **RoomIcon Prefab**
   - Image (방 타입별 sprite)
   - RoomIconUI (컴포넌트, 선택사항)

#### Sprite Assets
- StartRoomIcon.png (파란색)
- NormalRoomIcon.png (회색)
- EliteRoomIcon.png (주황색)
- BossRoomIcon.png (빨간색)
- TreasureRoomIcon.png (노란색)
- ShopRoomIcon.png (녹색)
- CurrentRoomHighlight.png (빛나는 테두리)

### 🔧 Unity 설정

#### 1. Minimap UI 생성

```
Hierarchy:
=== UI CANVAS ===
└── MinimapUI
    ├── MinimapUI (Component)
    └── Panel
        └── MinimapContainer (Grid Layout Group)
```

**MinimapUI 설정**:
- Anchor: Top Right
- Width: 300, Height: 300
- Anchor Min: (1, 1), Anchor Max: (1, 1)
- Pivot: (1, 1)
- Anchored Position: (-20, -20)

**Panel 설정**:
- Anchor: Stretch (0,0) ~ (1,1)
- Image: 반투명 검정 배경

**MinimapContainer 설정**:
- Grid Layout Group:
  - Cell Size: 40x40
  - Spacing: 5, 5
  - Start Corner: Upper Left
  - Start Axis: Horizontal
  - Constraint: Flexible

#### 2. RoomIcon Prefab 생성

1. Hierarchy 우클릭 > UI > Image → "RoomIcon"
2. Inspector 설정:
   - Width: 40, Height: 40
   - Image Color: 흰색
   - Sprite: (동적 할당)
3. Prefab으로 저장: `Assets/Resources/Prefabs/UI/RoomIcon.prefab`
4. Hierarchy에서 삭제

#### 3. Sprite Assets 준비 (임시)

**간단한 방법** (아트 에셋 없을 시):
- Unity에서 Sprite 생성: Project 우클릭 > Create > Sprites > Circle
- Inspector에서 Color Tint:
  - StartRoomIcon: 파란색
  - NormalRoomIcon: 회색
  - BossRoomIcon: 빨간색
  - 등등...

**또는 외부 이미지**:
- 32x32 원형 아이콘 이미지 생성 (Paint, Photoshop)
- Unity로 Import
- Texture Type: Sprite (2D and UI)

### 📝 구현 단계

#### Step 1: MinimapUI 클래스 생성 (40분)
**파일**: `Assets/_Project/Scripts/UI/MinimapUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using GASPT.Level;
using System.Collections.Generic;
using Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// 미니맵 UI
    /// </summary>
    public class MinimapUI : BaseUI
    {
        [Header("Minimap Settings")]
        [SerializeField] private RectTransform minimapContainer;
        [SerializeField] private GameObject roomIconPrefab;

        [Header("Room Icons")]
        [SerializeField] private Sprite startRoomIcon;
        [SerializeField] private Sprite normalRoomIcon;
        [SerializeField] private Sprite eliteRoomIcon;
        [SerializeField] private Sprite bossRoomIcon;
        [SerializeField] private Sprite treasureRoomIcon;
        [SerializeField] private Sprite shopRoomIcon;
        [SerializeField] private Sprite currentRoomHighlight;

        private Dictionary<int, GameObject> roomIcons = new Dictionary<int, GameObject>();
        private int currentRoomId = 0;

        public void GenerateMinimap(DungeonLayout layout)
        {
            ClearMinimap();

            if (layout == null || layout.nodes == null)
            {
                Debug.LogError("[MinimapUI] Invalid DungeonLayout");
                return;
            }

            foreach (var node in layout.nodes)
            {
                GameObject iconObj = Instantiate(roomIconPrefab, minimapContainer);
                Image iconImage = iconObj.GetComponent<Image>();

                // 방 타입에 따른 아이콘
                iconImage.sprite = GetIconForRoomType(node.roomType);

                // 위치 설정 (Grid Layout Group이 자동 배치)
                roomIcons[node.nodeId] = iconObj;
            }

            UpdateCurrentRoom(layout.startNodeId);
            Show();

            Debug.Log($"[MinimapUI] Minimap generated: {layout.nodes.Count} rooms");
        }

        public void UpdateCurrentRoom(int roomId)
        {
            // 이전 방 하이라이트 해제
            if (roomIcons.ContainsKey(currentRoomId))
            {
                Image prevIcon = roomIcons[currentRoomId].GetComponent<Image>();
                // TODO: 하이라이트 해제 로직
            }

            // 새 방 하이라이트
            if (roomIcons.ContainsKey(roomId))
            {
                Image newIcon = roomIcons[roomId].GetComponent<Image>();
                newIcon.sprite = currentRoomHighlight;
                currentRoomId = roomId;
            }
        }

        private void ClearMinimap()
        {
            foreach (var icon in roomIcons.Values)
            {
                Destroy(icon);
            }
            roomIcons.Clear();
        }

        private Sprite GetIconForRoomType(RoomType type)
        {
            return type switch
            {
                RoomType.Start => startRoomIcon,
                RoomType.Normal => normalRoomIcon,
                RoomType.Elite => eliteRoomIcon,
                RoomType.Boss => bossRoomIcon,
                RoomType.Treasure => treasureRoomIcon,
                RoomType.Shop => shopRoomIcon,
                _ => normalRoomIcon
            };
        }
    }
}
```

**체크리스트**:
- [ ] 파일 생성
- [ ] BaseUI 상속 확인
- [ ] GenerateMinimap() 구현
- [ ] UpdateCurrentRoom() 구현
- [ ] ClearMinimap() 구현
- [ ] GetIconForRoomType() 구현

---

#### Step 2: Unity에서 MinimapUI 생성 (30분)

**2-1. MinimapUI GameObject 생성**:
1. Hierarchy: === UI CANVAS === 우클릭 > UI > Panel → "MinimapUI"
2. Panel 자식 생성: GameObject 생성 → "Panel"
3. Panel 자식으로 GameObject 생성 → "MinimapContainer"
4. MinimapContainer에 Add Component > Layout > Grid Layout Group

**2-2. RectTransform 설정**:

**MinimapUI**:
- Anchor Preset: Top Right (Shift+Alt 클릭)
- Width: 300, Height: 300
- Pos X: -20, Pos Y: -20

**Panel**:
- Anchor: Stretch All
- Image Component 추가
- Color: (0, 0, 0, 150) - 반투명 검정

**MinimapContainer**:
- Anchor: Stretch All
- Grid Layout Group:
  - Padding: 10
  - Cell Size: (40, 40)
  - Spacing: (5, 5)
  - Child Alignment: Upper Left

**2-3. MinimapUI Component 추가**:
1. MinimapUI GameObject 선택
2. Add Component > MinimapUI
3. Inspector에서 참조 할당:
   - Panel: Panel (자식)
   - Minimap Container: MinimapContainer
   - Room Icon Prefab: (다음 단계에서 할당)
   - Sprites: (다음 단계에서 할당)

**체크리스트**:
- [ ] MinimapUI GameObject 생성
- [ ] Panel, MinimapContainer 생성
- [ ] RectTransform 설정
- [ ] Grid Layout Group 설정
- [ ] MinimapUI 컴포넌트 추가

---

#### Step 3: RoomIcon Prefab 생성 (20분)

1. Hierarchy 우클릭 > UI > Image → "RoomIcon"
2. Inspector 설정:
   - Width: 40, Height: 40
   - Color: 흰색
   - Preserve Aspect: 체크
3. Prefab 저장:
   - RoomIcon을 Project로 드래그
   - `Assets/Resources/Prefabs/UI/RoomIcon.prefab`
4. Hierarchy에서 RoomIcon 삭제
5. MinimapUI Inspector:
   - Room Icon Prefab: RoomIcon (방금 생성한 Prefab)

**체크리스트**:
- [ ] RoomIcon Prefab 생성
- [ ] Resources/Prefabs/UI/ 폴더에 저장
- [ ] MinimapUI에 할당

---

#### Step 4: Sprite Assets 생성 (30분)

**간단한 방법** (임시 Sprite):
1. Project 우클릭 > Create > Sprites > Circle
2. 6개 생성:
   - StartRoomIcon, NormalRoomIcon, EliteRoomIcon
   - BossRoomIcon, TreasureRoomIcon, ShopRoomIcon
3. 각 Sprite 선택 > Inspector > Color:
   - Start: 파란색 #0000FF
   - Normal: 회색 #808080
   - Elite: 주황색 #FFA500
   - Boss: 빨간색 #FF0000
   - Treasure: 노란색 #FFFF00
   - Shop: 녹색 #00FF00
4. CurrentRoomHighlight: 흰색 Circle + Outline

**또는 외부 이미지**:
1. 32x32 원형 PNG 이미지 6개 제작
2. Unity로 Import: `Assets/_Project/Art/UI/Minimap/`
3. Texture Type: Sprite (2D and UI)

**MinimapUI Inspector에 할당**:
- Start Room Icon: StartRoomIcon
- Normal Room Icon: NormalRoomIcon
- Elite Room Icon: EliteRoomIcon
- Boss Room Icon: BossRoomIcon
- Treasure Room Icon: TreasureRoomIcon
- Shop Room Icon: ShopRoomIcon
- Current Room Highlight: CurrentRoomHighlight

**체크리스트**:
- [ ] Sprite 7개 생성 (또는 Import)
- [ ] MinimapUI에 모두 할당
- [ ] None (Sprite) 없는지 확인

---

#### Step 5: DungeonGenerator와 연동 (30분)

**DungeonGenerator 수정**:
**파일**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/DungeonGenerator.cs`

```csharp
// 기존 코드에 추가

[Header("UI References")]
[SerializeField] private MinimapUI minimapUI;

public DungeonLayout GenerateDungeon()
{
    // ... 기존 코드 ...

    layout.currentNodeId = layout.startNodeId;
    currentDungeon = layout;

    // 미니맵 생성 추가
    if (minimapUI != null)
    {
        minimapUI.GenerateMinimap(layout);
    }

    Debug.Log($"[DungeonGenerator] Dungeon generated: {totalRooms} rooms");
    return layout;
}

public void LoadRoom(int roomId)
{
    // ... 기존 코드 ...

    node.isVisited = true;
    currentDungeon.currentNodeId = roomId;

    // 미니맵 업데이트 추가
    if (minimapUI != null)
    {
        minimapUI.UpdateCurrentRoom(roomId);
    }

    if (roomGenerator != null && node.roomData != null)
    {
        roomGenerator.GenerateRoom(node.roomData);
    }

    Debug.Log($"[DungeonGenerator] Loaded room {roomId}: {node.roomType}");
}
```

**Unity 설정**:
1. DungeonGenerator GameObject 선택
2. Inspector > Minimap UI: MinimapUI (Canvas 하위)

**체크리스트**:
- [ ] DungeonGenerator에 minimapUI 필드 추가
- [ ] GenerateDungeon()에서 minimapUI.GenerateMinimap() 호출
- [ ] LoadRoom()에서 minimapUI.UpdateCurrentRoom() 호출
- [ ] Inspector에서 MinimapUI 할당

---

#### Step 6: 테스트 (20분)

**테스트 절차**:
1. Play Mode 실행
2. Console에서 "Generate Dungeon" (DungeonGeneratorTest Context Menu)
3. 화면 우측 상단에 미니맵 표시 확인:
   - 작은 원형 아이콘들이 Grid로 배열
   - 시작 방(파란색), 보스 방(빨간색) 등 색상 구분
   - 현재 방(하이라이트)이 표시됨
4. DungeonGeneratorTest에서 "Load Room" 실행
5. 미니맵에서 현재 방 하이라이트가 이동하는지 확인

**체크리스트**:
- [ ] 미니맵이 화면 우측 상단에 표시됨
- [ ] 방 아이콘이 Grid로 배열됨
- [ ] 방 타입별로 다른 색상 아이콘
- [ ] 현재 방이 하이라이트됨
- [ ] LoadRoom() 호출 시 하이라이트 이동

---

### 📋 Minimap UI 완료 체크리스트

- [ ] MinimapUI 클래스 생성 (BaseUI 상속)
- [ ] Unity에서 MinimapUI GameObject 생성
- [ ] RoomIcon Prefab 생성
- [ ] Sprite Assets 7개 준비
- [ ] DungeonGenerator와 연동
- [ ] Play Mode에서 미니맵 표시 확인
- [ ] 방 이동 시 하이라이트 업데이트 확인

**총 예상 시간**: 약 2-3시간

---

## 📊 Phase E-1 완료 체크리스트 (전체)

- [ ] Room Generator 구현 완료
- [ ] Dungeon Generator 구현 완료
- [ ] Minimap UI 구현 완료
- [ ] 던전 생성 → 방 생성 → 미니맵 표시 흐름 동작
- [ ] Play Mode에서 전체 시스템 테스트 통과

**총 예상 시간**: 약 8-10시간 (1-2주 작업)

---

**다음**: [Phase E-2: 스컬 교체 시스템](#phase-e-2-스컬-교체-시스템)으로 계속...

---

# 🦴 Phase E-2: 스컬 교체 시스템

**목표**: 플레이어가 여러 스컬을 수집하고 Q키로 교체하여 다양한 플레이 스타일 제공
**예상 기간**: 3-4주
**전체 진행률**: 0% → 100%

> 이 섹션도 E-1과 동일한 상세도로 작성합니다.
> (Skull Data, Skull Manager, Transform System, Awakening System)

---

## 📍 E-2-1: Skull Data System

### 🎯 무엇을 만드는가?
**스컬의 모든 정보를 담은 ScriptableObject**입니다.
- 스컬 이름, 설명, 아이콘
- 기본 스탯 (체력, 공격력, 방어력, 이동속도)
- 스킬 4개 (기본 공격, Q, E, R)
- 애니메이션 컨트롤러, 스프라이트

### 🔍 왜 필요한가?
현재는 MageForm 하나만 하드코딩되어 있습니다.
SkullData를 통해:
- 디자이너가 Unity Inspector에서 스컬 생성 가능
- 스컬별로 다른 스탯, 스킬 설정
- 새 스컬 추가가 쉬워짐 (코드 수정 없이 ScriptableObject만 생성)

### 🏗️ 아키텍처

**클래스 구조**:
```
SkullData (ScriptableObject)
├── Basic Info (name, description, icon)
├── Stats (health, attack, defense, moveSpeed)
├── Abilities (basicAttack, skill1, skill2, ultimate)
└── Visuals (animatorController, sprites)
```

**사용 예시**:
```
1. 디자이너가 "WarriorSkull" ScriptableObject 생성
2. Inspector에서 스탯 설정 (높은 체력, 낮은 속도)
3. 스킬 이름 입력 ("WarriorSlash", "ShieldBash" 등)
4. 런타임에 SkullManager가 SkullData 읽어서 플레이어에 적용
```

### 🧩 필요한 컴포넌트

#### C# 스크립트
1. `SkullData.cs` (ScriptableObject)

#### ScriptableObject Assets
1. BasicSkull.asset
2. WarriorSkull.asset
3. MageSkull.asset
4. AssassinSkull.asset
5. TankSkull.asset

### 📝 구현 단계

#### Step 1: SkullData 클래스 생성 (30분)
**파일**: `Assets/_Project/Scripts/Data/SkullData.cs`

```csharp
using UnityEngine;

namespace GASPT.Data
{
    [CreateAssetMenu(fileName = "SkullData", menuName = "GASPT/Skull/Skull Data")]
    public class SkullData : ScriptableObject
    {
        [Header("Basic Info")]
        public string skullName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Stats")]
        [Tooltip("체력 보너스")]
        public int healthBonus = 0;
        [Tooltip("공격력 보너스")]
        public int attackBonus = 0;
        [Tooltip("방어력 보너스")]
        public int defenseBonus = 0;
        [Tooltip("이동 속도 배율")]
        [Range(0.5f, 2f)]
        public float moveSpeedMultiplier = 1f;

        [Header("Abilities")]
        public string basicAttackAbility;
        public string skill1Ability;
        public string skill2Ability;
        public string ultimateAbility;

        [Header("Visuals")]
        public RuntimeAnimatorController animatorController;
        public Sprite idleSprite;
    }
}
```

**체크리스트**:
- [ ] 파일 생성
- [ ] CreateAssetMenu 속성 확인
- [ ] 필드 12개 정의

---

#### Step 2: SkullData Assets 생성 (1시간)

**2-1. BasicSkull 생성**:
1. Project 우클릭 > Create > GASPT > Skull > Skull Data
2. "BasicSkull" 이름 변경
3. Inspector 설정:
   - Skull Name: "기본 스컬"
   - Description: "균형잡힌 기본 스컬"
   - Health Bonus: 0
   - Attack Bonus: 0
   - Defense Bonus: 0
   - Move Speed Multiplier: 1.0
   - Basic Attack Ability: "BasicSlash"
   - Skill 1: "SkullThrow"
   - (나머지 임시로 비워둠)

**2-2. WarriorSkull 생성**:
- Skull Name: "전사 스컬"
- Description: "높은 체력과 강력한 공격력"
- Health Bonus: +50
- Attack Bonus: +10
- Defense Bonus: +5
- Move Speed Multiplier: 0.8 (느림)
- Basic Attack: "HeavySlash"
- Skill 1: "GroundSmash"

**2-3. MageSkull 생성**:
- Skull Name: "마법사 스컬"
- Description: "원거리 마법 공격"
- Health Bonus: -20
- Attack Bonus: +5
- Move Speed Multiplier: 1.2 (빠름)
- Basic Attack: "MagicMissile"
- Skill 1: "Fireball"

**2-4. AssassinSkull, TankSkull 동일하게 생성**

**저장 위치**: `Assets/_Project/Data/Skulls/`

**체크리스트**:
- [ ] BasicSkull.asset 생성
- [ ] WarriorSkull.asset 생성
- [ ] MageSkull.asset 생성
- [ ] AssassinSkull.asset 생성
- [ ] TankSkull.asset 생성
- [ ] 각각 스탯 설정 완료

---

## 📍 E-2-2: Skull Manager

### 🎯 무엇을 만드는가?
**플레이어의 스컬을 관리하고 Q키로 교체하는 시스템**입니다.
- 소유 스컬 목록 관리
- Q키 입력 시 다음 스컬로 변신
- 변신 쿨다운 관리
- 스컬 스탯/비주얼/어빌리티 적용

### 🔍 왜 필요한가?
현재는 단일 Form만 존재합니다. SkullManager를 통해:
- 여러 스컬 수집 및 교체 가능
- 스컬마다 다른 플레이 경험 제공
- 런 중 새 스컬 획득 시 즉시 사용 가능

### 🏗️ 아키텍처

**클래스 구조**:
```
SkullManager (MonoBehaviour)
├── ownedSkulls (List<SkullData>)
├── currentSkull (SkullData)
├── currentSkullIndex (int)
├── transformCooldown (float)
├── TransformToNextSkull()
├── EquipSkull(SkullData)
├── ApplySkullStats(SkullData)
├── ApplySkullVisuals(SkullData)
├── ApplySkullAbilities(SkullData)
└── AddSkull(SkullData)
```

**데이터 흐름**:
```
1. 플레이어가 Q키 입력
2. SkullManager.TransformToNextSkull() 호출
3. currentSkullIndex 증가 (순환)
4. EquipSkull(nextSkull) 호출
5. ApplySkullStats/Visuals/Abilities 순서로 적용
6. 애니메이션 효과 재생
```

### 📝 구현 단계

#### Step 1: SkullManager 클래스 생성 (1.5시간)
**파일**: `Assets/_Project/Scripts/Gameplay/Skull/SkullManager.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;
using GASPT.Data;
using GASPT.Stats;

namespace GASPT.Skull
{
    public class SkullManager : MonoBehaviour
    {
        [Header("Skull Settings")]
        [SerializeField] private List<SkullData> ownedSkulls = new List<SkullData>();
        [SerializeField] private SkullData startingSkull;

        [Header("Transform Settings")]
        [SerializeField] private float transformCooldown = 1f;
        [SerializeField] private GameObject transformEffect; // VFX

        private SkullData currentSkull;
        private PlayerStats playerStats;
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private float lastTransformTime = 0f;
        private int currentSkullIndex = 0;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (startingSkull != null)
            {
                AddSkull(startingSkull);
                EquipSkull(startingSkull);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TransformToNextSkull();
            }
        }

        public void TransformToNextSkull()
        {
            if (Time.time - lastTransformTime < transformCooldown)
            {
                Debug.Log("[SkullManager] Transform on cooldown");
                return;
            }

            if (ownedSkulls.Count == 0) return;

            currentSkullIndex = (currentSkullIndex + 1) % ownedSkulls.Count;
            SkullData nextSkull = ownedSkulls[currentSkullIndex];

            EquipSkull(nextSkull);
            lastTransformTime = Time.time;

            // VFX 재생
            if (transformEffect != null)
            {
                Instantiate(transformEffect, transform.position, Quaternion.identity);
            }

            Debug.Log($"[SkullManager] Transformed to: {nextSkull.skullName}");
        }

        public void EquipSkull(SkullData skull)
        {
            if (skull == null) return;

            currentSkull = skull;

            ApplySkullStats(skull);
            ApplySkullVisuals(skull);
            ApplySkullAbilities(skull);
        }

        private void ApplySkullStats(SkullData skull)
        {
            if (playerStats == null) return;

            // TODO: PlayerStats에 SetBonusStats() 메서드 추가 필요
            // playerStats.SetBonusStats(skull.healthBonus, skull.attackBonus, skull.defenseBonus);
            // playerStats.SetMoveSpeedMultiplier(skull.moveSpeedMultiplier);
        }

        private void ApplySkullVisuals(SkullData skull)
        {
            if (animator != null && skull.animatorController != null)
            {
                animator.runtimeAnimatorController = skull.animatorController;
            }

            if (spriteRenderer != null && skull.idleSprite != null)
            {
                spriteRenderer.sprite = skull.idleSprite;
            }
        }

        private void ApplySkullAbilities(SkullData skull)
        {
            // TODO: GAS와 연동
            // AbilitySystem.ReplaceAbility("BasicAttack", skull.basicAttackAbility);
            // AbilitySystem.ReplaceAbility("Skill1", skull.skill1Ability);
        }

        public void AddSkull(SkullData skull)
        {
            if (skull == null || ownedSkulls.Contains(skull))
                return;

            ownedSkulls.Add(skull);
            Debug.Log($"[SkullManager] Added skull: {skull.skullName}");
        }

        // Getters
        public SkullData CurrentSkull => currentSkull;
        public List<SkullData> OwnedSkulls => ownedSkulls;
    }
}
```

**체크리스트**:
- [ ] 파일 생성
- [ ] TransformToNextSkull() 구현
- [ ] EquipSkull() 구현
- [ ] ApplySkullStats/Visuals/Abilities 구현
- [ ] AddSkull() 구현

---

#### Step 2: PlayerController에 SkullManager 추가 (20분)

1. Player GameObject 선택
2. Add Component > SkullManager
3. Inspector 설정:
   - Starting Skull: BasicSkull
   - Transform Cooldown: 1.0
   - (Owned Skulls는 런타임에 AddSkull()로 추가)

**체크리스트**:
- [ ] SkullManager 컴포넌트 추가
- [ ] Starting Skull 할당
- [ ] Transform Cooldown 설정

---

#### Step 3: 테스트 (30분)

**테스트 스크립트**:
**파일**: `Assets/_Project/Scripts/Testing/SkullSystemTest.cs`

```csharp
using UnityEngine;
using GASPT.Skull;
using GASPT.Data;

public class SkullSystemTest : MonoBehaviour
{
    [SerializeField] private SkullManager skullManager;
    [SerializeField] private SkullData testSkull1;
    [SerializeField] private SkullData testSkull2;

    private void Start()
    {
        if (skullManager != null)
        {
            if (testSkull1 != null) skullManager.AddSkull(testSkull1);
            if (testSkull2 != null) skullManager.AddSkull(testSkull2);
        }
    }

    [ContextMenu("Add Warrior Skull")]
    private void TestAddWarriorSkull()
    {
        if (skullManager != null && testSkull1 != null)
        {
            skullManager.AddSkull(testSkull1);
        }
    }
}
```

**테스트 절차**:
1. GameObject 생성 → "SkullSystemTest"
2. SkullSystemTest 컴포넌트 추가
3. Inspector:
   - Skull Manager: Player의 SkullManager
   - Test Skull 1: WarriorSkull
   - Test Skull 2: MageSkull
4. Play Mode 실행
5. Q키 눌러서 스컬 변신 테스트:
   - Console에 "Transformed to: 전사 스컬" 출력
   - Q키 다시 누르면 "마법사 스컬"로 변신
   - 다시 누르면 "기본 스컬"로 돌아옴

**체크리스트**:
- [ ] Q키로 스컬 변신 동작
- [ ] 스컬이 순환함 (Basic → Warrior → Mage → Basic)
- [ ] Console에 로그 출력
- [ ] 쿨다운 동작 (1초 내 재변신 불가)

---

### 📋 Phase E-2 완료 체크리스트 (일부)

- [ ] SkullData ScriptableObject 생성
- [ ] SkullData Assets 5개 생성
- [ ] SkullManager 클래스 생성
- [ ] Player에 SkullManager 추가
- [ ] Q키 변신 동작 확인

**총 예상 시간 (E-2-1 + E-2-2)**: 약 4-5시간

> **Note**: Transform System (애니메이션, VFX), Awakening System은 추가 구현 필요
> 이후 단계에서 계속...

---

이런 식으로 **모든 Phase를 매우 상세하게** 작성합니다.
각 기능마다:
1. 무엇을 만드는지
2. 왜 필요한지
3. 아키텍처 (클래스 구조, 데이터 흐름)
4. 필요한 컴포넌트 (스크립트, GameObject, Assets)
5. Unity 설정 (상세한 Inspector 값)
6. 구현 단계 (Step by Step, 예상 시간 포함)
7. 테스트 체크리스트

---

**계속 작성 중...**
(Phase E-3, E-4, F도 동일한 상세도로 작성 예정)
