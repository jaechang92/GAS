# 🛠️ GASPT 수동 구현 가이드

**프로젝트명**: GASPT (Generic Ability System + FSM Platform Game)
**작성일**: 2025-11-19
**목적**: 서버 오류 시 수동으로 작업할 수 있는 단계별 구현 가이드

---

## 📋 목차

1. [가이드 사용 방법](#가이드-사용-방법)
2. [Phase E-1: 절차적 레벨 생성 시스템](#phase-e-1-절차적-레벨-생성-시스템)
3. [Phase E-2: 스컬 교체 시스템](#phase-e-2-스컬-교체-시스템)
4. [Phase E-3: 메타 진행 시스템](#phase-e-3-메타-진행-시스템)
5. [시스템별 구현 체크리스트](#시스템별-구현-체크리스트)
6. [트러블슈팅 가이드](#트러블슈팅-가이드)
7. [코드 스니펫 라이브러리](#코드-스니펫-라이브러리)

---

## 📖 가이드 사용 방법

### 서버 오류 발생 시 복구 절차

1. **현재 상태 파악** (5분)
   ```bash
   # 마지막 커밋 확인
   git log -1

   # 현재 변경 사항 확인
   git status

   # 현재 Phase 확인
   # PROJECT_MASTER_ROADMAP.md 참조
   ```

2. **문서 확인** (10분)
   - `PROJECT_MASTER_ROADMAP.md`: 전체 로드맵 및 현재 Phase 확인
   - `WORK_HISTORY.md`: 완료된 작업 확인
   - `IMPLEMENTATION_GUIDE.md` (본 문서): 다음 작업 구현 방법 확인

3. **단계별 구현** (작업 시간 소요)
   - 본 문서의 체크리스트를 따라 단계별로 구현
   - 각 단계 완료 시 커밋 생성
   - 코드 스니펫 참조하여 핵심 로직 작성

4. **테스트 및 검증** (10-20분)
   - Unity에서 플레이 모드 실행
   - 기능 동작 확인
   - 버그 발견 시 즉시 수정

---

## 🎮 Phase E-1: 절차적 레벨 생성 시스템

**예상 기간**: 2-3주
**현재 상태**: 미착수 (0%)
**선행 요구사항**: Phase C-3 (던전 진행 시스템) 완료

---

### Step 1: RoomGenerator 구현 (1주)

#### 1.1 RoomType Enum 추가

**파일 위치**: `Assets/_Project/Scripts/Core/Enums/RoomType.cs`

**작업 내용**:
```csharp
namespace Core.Enums
{
    /// <summary>
    /// 방 타입
    /// </summary>
    public enum RoomType
    {
        Start,        // 시작 방
        Normal,       // 일반 전투 방
        Elite,        // 엘리트 전투 방
        Boss,         // 보스 방
        Treasure,     // 보물 방
        Shop,         // 상점 방
        Rest,         // 휴식 방
        Event         // 이벤트 방
    }
}
```

**체크리스트**:
- [ ] RoomType.cs 파일 생성
- [ ] 8가지 RoomType 정의
- [ ] XML 주석 추가
- [ ] Core.Enums 네임스페이스 확인

---

#### 1.2 RoomData 확장

**파일 위치**: `Assets/_Project/Scripts/Gameplay/Level/Room/RoomData.cs` (기존 파일 수정)

**추가 필드**:
```csharp
using Core.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "GASPT/Level/Room Data")]
public class RoomData : ScriptableObject
{
    // 기존 필드
    // ...

    // 새로 추가할 필드
    [Header("Room Type")]
    [Tooltip("방 타입")]
    public RoomType roomType = RoomType.Normal;

    [Header("Room Properties")]
    [Tooltip("방 너비 (타일 단위)")]
    [Range(10, 50)]
    public int roomWidth = 20;

    [Tooltip("방 높이 (타일 단위)")]
    [Range(10, 30)]
    public int roomHeight = 15;

    [Tooltip("플랫폼 개수 (랜덤 배치)")]
    [Range(0, 10)]
    public int platformCount = 3;

    [Tooltip("장애물 개수 (가시, 함정)")]
    [Range(0, 5)]
    public int obstacleCount = 1;

    [Header("Rewards")]
    [Tooltip("클리어 시 골드 보상")]
    public int goldReward = 50;

    [Tooltip("클리어 시 경험치 보상")]
    public int expReward = 100;
}
```

**체크리스트**:
- [ ] RoomType 필드 추가
- [ ] 방 크기 필드 추가 (roomWidth, roomHeight)
- [ ] 플랫폼/장애물 개수 필드 추가
- [ ] 보상 필드 추가 (goldReward, expReward)
- [ ] Range 속성 추가하여 Inspector에서 조절 가능하게 설정

---

#### 1.3 RoomGenerator 클래스 생성

**파일 위치**: `Assets/_Project/Scripts/Gameplay/Level/Room/RoomGenerator.cs`

**핵심 로직**:
```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

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
                Debug.LogError("[RoomGenerator] RoomData가 null입니다.");
                return;
            }

            // 기존 타일 제거
            ClearRoom();

            // 바닥 생성
            GenerateGround(roomData);

            // 플랫폼 생성
            GeneratePlatforms(roomData);

            // 장애물 생성
            GenerateObstacles(roomData);

            Debug.Log($"[RoomGenerator] 방 생성 완료: {roomData.roomType}");
        }

        /// <summary>
        /// 기존 타일 제거
        /// </summary>
        private void ClearRoom()
        {
            if (groundTilemap != null) groundTilemap.ClearAllTiles();
            if (platformTilemap != null) platformTilemap.ClearAllTiles();
            if (obstacleTilemap != null) obstacleTilemap.ClearAllTiles();
        }

        /// <summary>
        /// 바닥 생성
        /// </summary>
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

        /// <summary>
        /// 플랫폼 랜덤 생성
        /// </summary>
        private void GeneratePlatforms(RoomData roomData)
        {
            if (platformTilemap == null || platformTile == null)
                return;

            for (int i = 0; i < roomData.platformCount; i++)
            {
                // 랜덤 위치 및 크기
                int platformWidth = Random.Range(3, 8);
                int platformX = Random.Range(2, roomData.roomWidth - platformWidth - 2);
                int platformY = Random.Range(3, roomData.roomHeight - 2);

                // 플랫폼 생성
                for (int x = 0; x < platformWidth; x++)
                {
                    Vector3Int tilePos = new Vector3Int(platformX + x, platformY, 0);
                    platformTilemap.SetTile(tilePos, platformTile);
                }
            }
        }

        /// <summary>
        /// 장애물 랜덤 생성
        /// </summary>
        private void GenerateObstacles(RoomData roomData)
        {
            if (obstacleTilemap == null || spikeTile == null)
                return;

            for (int i = 0; i < roomData.obstacleCount; i++)
            {
                // 랜덤 위치 (바닥 위)
                int obstacleX = Random.Range(2, roomData.roomWidth - 2);
                int obstacleY = 1; // 바닥 바로 위

                Vector3Int tilePos = new Vector3Int(obstacleX, obstacleY, 0);
                obstacleTilemap.SetTile(tilePos, spikeTile);
            }
        }
    }
}
```

**체크리스트**:
- [ ] RoomGenerator.cs 파일 생성
- [ ] Tilemap 참조 필드 추가
- [ ] Tile 참조 필드 추가
- [ ] GenerateRoom() 메서드 구현
- [ ] ClearRoom() 메서드 구현
- [ ] GenerateGround() 메서드 구현
- [ ] GeneratePlatforms() 메서드 구현
- [ ] GenerateObstacles() 메서드 구현
- [ ] XML 주석 추가

---

### Step 2: DungeonGenerator 구현 (1주)

#### 2.1 DungeonLayout 데이터 구조

**파일 위치**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/DungeonLayout.cs`

**핵심 로직**:
```csharp
using UnityEngine;
using System.Collections.Generic;
using Core.Enums;

namespace GASPT.Level
{
    /// <summary>
    /// 던전 레이아웃
    /// 방들의 연결 구조를 나타냄
    /// </summary>
    [System.Serializable]
    public class DungeonNode
    {
        public int nodeId;
        public RoomType roomType;
        public Vector2Int gridPosition;
        public List<int> connectedNodes = new List<int>();

        public DungeonNode(int id, RoomType type, Vector2Int position)
        {
            nodeId = id;
            roomType = type;
            gridPosition = position;
        }
    }

    /// <summary>
    /// 던전 레이아웃
    /// </summary>
    public class DungeonLayout
    {
        public List<DungeonNode> nodes = new List<DungeonNode>();
        public int startNodeId;
        public int bossNodeId;

        /// <summary>
        /// 노드 추가
        /// </summary>
        public void AddNode(DungeonNode node)
        {
            nodes.Add(node);
        }

        /// <summary>
        /// 노드 연결
        /// </summary>
        public void ConnectNodes(int nodeId1, int nodeId2)
        {
            DungeonNode node1 = nodes.Find(n => n.nodeId == nodeId1);
            DungeonNode node2 = nodes.Find(n => n.nodeId == nodeId2);

            if (node1 != null && node2 != null)
            {
                if (!node1.connectedNodes.Contains(nodeId2))
                    node1.connectedNodes.Add(nodeId2);

                if (!node2.connectedNodes.Contains(nodeId1))
                    node2.connectedNodes.Add(nodeId1);
            }
        }

        /// <summary>
        /// ID로 노드 찾기
        /// </summary>
        public DungeonNode GetNode(int nodeId)
        {
            return nodes.Find(n => n.nodeId == nodeId);
        }
    }
}
```

**체크리스트**:
- [ ] DungeonLayout.cs 파일 생성
- [ ] DungeonNode 클래스 정의
- [ ] DungeonLayout 클래스 정의
- [ ] AddNode() 메서드 구현
- [ ] ConnectNodes() 메서드 구현
- [ ] GetNode() 메서드 구현

---

#### 2.2 DungeonGenerator 클래스

**파일 위치**: `Assets/_Project/Scripts/Gameplay/Level/Dungeon/DungeonGenerator.cs`

**핵심 로직**:
```csharp
using UnityEngine;
using System.Collections.Generic;
using Core.Enums;

namespace GASPT.Level
{
    /// <summary>
    /// 던전 생성기
    /// Graph 기반으로 던전 레이아웃 생성
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        [Tooltip("던전 층수")]
        [Range(1, 10)]
        public int floorCount = 3;

        [Tooltip("층당 방 개수 (최소)")]
        [Range(3, 10)]
        public int minRoomsPerFloor = 5;

        [Tooltip("층당 방 개수 (최대)")]
        [Range(5, 15)]
        public int maxRoomsPerFloor = 10;

        /// <summary>
        /// 던전 레이아웃 생성
        /// </summary>
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
                layout.AddNode(node);

                // 시작/보스 방 설정
                if (i == 0) layout.startNodeId = i;
                if (i == totalRooms - 1) layout.bossNodeId = i;
            }

            // 노드 연결 (간단한 선형 경로)
            for (int i = 0; i < totalRooms - 1; i++)
            {
                layout.ConnectNodes(i, i + 1);

                // 랜덤 추가 연결 (선택적 경로)
                if (Random.value > 0.7f && i + 2 < totalRooms)
                {
                    layout.ConnectNodes(i, i + 2);
                }
            }

            Debug.Log($"[DungeonGenerator] 던전 생성 완료: {totalRooms}개 방");
            return layout;
        }

        /// <summary>
        /// 방 타입 결정
        /// </summary>
        private RoomType DetermineRoomType(int index, int totalRooms)
        {
            if (index == 0) return RoomType.Start;
            if (index == totalRooms - 1) return RoomType.Boss;

            // 랜덤 방 타입
            float rand = Random.value;

            if (rand < 0.5f) return RoomType.Normal;
            if (rand < 0.7f) return RoomType.Elite;
            if (rand < 0.85f) return RoomType.Treasure;
            if (rand < 0.95f) return RoomType.Shop;
            return RoomType.Rest;
        }
    }
}
```

**체크리스트**:
- [ ] DungeonGenerator.cs 파일 생성
- [ ] 던전 설정 필드 추가
- [ ] GenerateDungeon() 메서드 구현
- [ ] DetermineRoomType() 메서드 구현
- [ ] 시작 방 → 보스 방 경로 보장
- [ ] 선택적 경로 생성 (랜덤 연결)

---

### Step 3: Minimap 시스템 (3-5일)

#### 3.1 MinimapUI 클래스

**파일 위치**: `Assets/_Project/Scripts/UI/MinimapUI.cs`

**핵심 로직**:
```csharp
using UnityEngine;
using UnityEngine.UI;
using GASPT.Level;
using System.Collections.Generic;

namespace GASPT.UI
{
    /// <summary>
    /// 미니맵 UI
    /// 던전 레이아웃을 시각화
    /// </summary>
    public class MinimapUI : BaseUI
    {
        [Header("Minimap Settings")]
        [SerializeField] private RectTransform minimapContainer;
        [SerializeField] private GameObject roomIconPrefab;

        [Header("Room Icons")]
        [SerializeField] private Sprite startRoomIcon;
        [SerializeField] private Sprite normalRoomIcon;
        [SerializeField] private Sprite bossRoomIcon;
        [SerializeField] private Sprite currentRoomIcon;

        private Dictionary<int, GameObject> roomIcons = new Dictionary<int, GameObject>();
        private int currentRoomId = 0;

        /// <summary>
        /// 던전 레이아웃으로 미니맵 생성
        /// </summary>
        public void GenerateMinimap(DungeonLayout layout)
        {
            // 기존 아이콘 제거
            ClearMinimap();

            // 방 아이콘 생성
            foreach (var node in layout.nodes)
            {
                GameObject iconObj = Instantiate(roomIconPrefab, minimapContainer);
                Image iconImage = iconObj.GetComponent<Image>();

                // 방 타입에 따른 아이콘 설정
                if (node.nodeId == layout.startNodeId)
                    iconImage.sprite = startRoomIcon;
                else if (node.nodeId == layout.bossNodeId)
                    iconImage.sprite = bossRoomIcon;
                else
                    iconImage.sprite = normalRoomIcon;

                // 위치 설정 (그리드 기반)
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchoredPosition = new Vector2(
                    node.gridPosition.x * 100f,
                    node.gridPosition.y * 100f
                );

                roomIcons[node.nodeId] = iconObj;
            }

            // 시작 방을 현재 방으로 설정
            UpdateCurrentRoom(layout.startNodeId);
        }

        /// <summary>
        /// 현재 방 업데이트
        /// </summary>
        public void UpdateCurrentRoom(int roomId)
        {
            // 이전 현재 방 아이콘 복구
            if (roomIcons.ContainsKey(currentRoomId))
            {
                Image prevIcon = roomIcons[currentRoomId].GetComponent<Image>();
                // 원래 아이콘으로 복구 (로직 추가 필요)
            }

            // 새 현재 방 표시
            if (roomIcons.ContainsKey(roomId))
            {
                Image newIcon = roomIcons[roomId].GetComponent<Image>();
                newIcon.sprite = currentRoomIcon;
                currentRoomId = roomId;
            }
        }

        /// <summary>
        /// 미니맵 초기화
        /// </summary>
        private void ClearMinimap()
        {
            foreach (var icon in roomIcons.Values)
            {
                Destroy(icon);
            }
            roomIcons.Clear();
        }
    }
}
```

**체크리스트**:
- [ ] MinimapUI.cs 파일 생성
- [ ] BaseUI 상속
- [ ] 방 아이콘 프리팹 참조 추가
- [ ] GenerateMinimap() 메서드 구현
- [ ] UpdateCurrentRoom() 메서드 구현
- [ ] ClearMinimap() 메서드 구현

---

## 🦴 Phase E-2: 스컬 교체 시스템

**예상 기간**: 3-4주
**현재 상태**: 미착수 (0%)
**선행 요구사항**: Phase A-1 (MageForm 시스템) 완료

---

### Step 1: SkullData ScriptableObject (1주)

#### 1.1 SkullData 클래스

**파일 위치**: `Assets/_Project/Scripts/Data/SkullData.cs`

**핵심 로직**:
```csharp
using UnityEngine;
using GASPT.Stats;

namespace GASPT.Data
{
    /// <summary>
    /// 스컬 데이터
    /// 스컬별 스탯, 스킬, 애니메이션 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SkullData", menuName = "GASPT/Skull/Skull Data")]
    public class SkullData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("스컬 이름")]
        public string skullName;

        [Tooltip("스컬 설명")]
        [TextArea(3, 5)]
        public string description;

        [Tooltip("스컬 아이콘")]
        public Sprite icon;

        [Header("Stats")]
        [Tooltip("체력 증가량")]
        public int healthBonus = 0;

        [Tooltip("공격력 증가량")]
        public int attackBonus = 0;

        [Tooltip("방어력 증가량")]
        public int defenseBonus = 0;

        [Tooltip("이동 속도 배율")]
        [Range(0.5f, 2f)]
        public float moveSpeedMultiplier = 1f;

        [Header("Abilities")]
        [Tooltip("기본 공격 (좌클릭)")]
        public string basicAttackAbilityName;

        [Tooltip("스킬 1 (Q키)")]
        public string skill1AbilityName;

        [Tooltip("스킬 2 (E키)")]
        public string skill2AbilityName;

        [Tooltip("궁극기 (R키)")]
        public string ultimateAbilityName;

        [Header("Visuals")]
        [Tooltip("스프라이트 (Idle, Run, Attack 등)")]
        public RuntimeAnimatorController animatorController;

        [Tooltip("기본 스프라이트 (Idle)")]
        public Sprite idleSprite;
    }
}
```

**체크리스트**:
- [ ] SkullData.cs 파일 생성
- [ ] 기본 정보 필드 추가
- [ ] 스탯 필드 추가
- [ ] 어빌리티 필드 추가
- [ ] 비주얼 필드 추가
- [ ] CreateAssetMenu 속성 추가

---

### Step 2: SkullManager 구현 (1-2주)

#### 2.1 SkullManager 클래스

**파일 위치**: `Assets/_Project/Scripts/Gameplay/Skull/SkullManager.cs`

**핵심 로직**:
```csharp
using UnityEngine;
using System.Collections.Generic;
using GASPT.Data;
using GASPT.Stats;

namespace GASPT.Skull
{
    /// <summary>
    /// 스컬 매니저
    /// 스컬 교체 및 관리
    /// </summary>
    public class SkullManager : MonoBehaviour
    {
        [Header("Skull Settings")]
        [Tooltip("소유 스컬 목록")]
        [SerializeField] private List<SkullData> ownedSkulls = new List<SkullData>();

        [Tooltip("시작 스컬")]
        [SerializeField] private SkullData startingSkull;

        [Header("Transform Settings")]
        [Tooltip("변신 쿨다운 (초)")]
        [SerializeField] private float transformCooldown = 1f;

        private SkullData currentSkull;
        private PlayerStats playerStats;
        private Animator playerAnimator;
        private SpriteRenderer playerRenderer;

        private float lastTransformTime = 0f;
        private int currentSkullIndex = 0;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            playerAnimator = GetComponent<Animator>();
            playerRenderer = GetComponent<SpriteRenderer>();

            // 시작 스컬 장착
            if (startingSkull != null)
            {
                EquipSkull(startingSkull);
            }
        }

        private void Update()
        {
            // Q키로 스컬 교체
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TransformToNextSkull();
            }
        }

        /// <summary>
        /// 다음 스컬로 변신
        /// </summary>
        public void TransformToNextSkull()
        {
            // 쿨다운 체크
            if (Time.time - lastTransformTime < transformCooldown)
            {
                Debug.Log("[SkullManager] 변신 쿨다운 중");
                return;
            }

            if (ownedSkulls.Count == 0)
            {
                Debug.LogWarning("[SkullManager] 소유 스컬이 없습니다.");
                return;
            }

            // 다음 스컬 인덱스
            currentSkullIndex = (currentSkullIndex + 1) % ownedSkulls.Count;
            SkullData nextSkull = ownedSkulls[currentSkullIndex];

            // 스컬 장착
            EquipSkull(nextSkull);

            lastTransformTime = Time.time;
            Debug.Log($"[SkullManager] {nextSkull.skullName}(으)로 변신");
        }

        /// <summary>
        /// 스컬 장착
        /// </summary>
        public void EquipSkull(SkullData skull)
        {
            if (skull == null)
                return;

            currentSkull = skull;

            // 스탯 적용
            ApplySkullStats(skull);

            // 비주얼 적용
            ApplySkullVisuals(skull);

            // 어빌리티 적용 (GAS 연동 필요)
            ApplySkullAbilities(skull);
        }

        /// <summary>
        /// 스컬 스탯 적용
        /// </summary>
        private void ApplySkullStats(SkullData skull)
        {
            if (playerStats == null)
                return;

            // 기본 스탯에 보너스 추가
            // (playerStats.SetBonusHealth(skull.healthBonus) 등 구현 필요)
        }

        /// <summary>
        /// 스컬 비주얼 적용
        /// </summary>
        private void ApplySkullVisuals(SkullData skull)
        {
            // 애니메이터 컨트롤러 교체
            if (playerAnimator != null && skull.animatorController != null)
            {
                playerAnimator.runtimeAnimatorController = skull.animatorController;
            }

            // 기본 스프라이트 설정
            if (playerRenderer != null && skull.idleSprite != null)
            {
                playerRenderer.sprite = skull.idleSprite;
            }
        }

        /// <summary>
        /// 스컬 어빌리티 적용
        /// </summary>
        private void ApplySkullAbilities(SkullData skull)
        {
            // GAS와 연동하여 어빌리티 교체
            // (AbilitySystem.ReplaceAbility() 등 구현 필요)
        }

        /// <summary>
        /// 스컬 추가
        /// </summary>
        public void AddSkull(SkullData skull)
        {
            if (!ownedSkulls.Contains(skull))
            {
                ownedSkulls.Add(skull);
                Debug.Log($"[SkullManager] 새 스컬 획득: {skull.skullName}");
            }
        }

        // Getters
        public SkullData CurrentSkull => currentSkull;
        public List<SkullData> OwnedSkulls => ownedSkulls;
    }
}
```

**체크리스트**:
- [ ] SkullManager.cs 파일 생성
- [ ] 소유 스컬 목록 관리
- [ ] TransformToNextSkull() 메서드 구현
- [ ] EquipSkull() 메서드 구현
- [ ] ApplySkullStats() 메서드 구현
- [ ] ApplySkullVisuals() 메서드 구현
- [ ] ApplySkullAbilities() 메서드 구현
- [ ] AddSkull() 메서드 구현

---

## 💎 Phase E-3: 메타 진행 시스템

**예상 기간**: 1-2주
**현재 상태**: 미착수 (0%)
**선행 요구사항**: SaveSystem 완료

---

### Step 1: 메타 화폐 시스템 (3-5일)

#### 1.1 MetaCurrency Enum

**파일 위치**: `Assets/_Project/Scripts/Core/Enums/MetaCurrency.cs`

**작업 내용**:
```csharp
namespace Core.Enums
{
    /// <summary>
    /// 메타 화폐 타입
    /// </summary>
    public enum MetaCurrency
    {
        Bone,   // 뼈 (플레이 중 획득)
        Soul    // 영혼 (보스 처치 시 획득)
    }
}
```

---

#### 1.2 MetaCurrencySystem 클래스

**파일 위치**: `Assets/_Project/Scripts/Economy/MetaCurrencySystem.cs`

**핵심 로직**:
```csharp
using UnityEngine;
using Core.Enums;
using System;

namespace GASPT.Economy
{
    /// <summary>
    /// 메타 화폐 시스템
    /// 뼈, 영혼 관리
    /// </summary>
    public class MetaCurrencySystem : MonoBehaviour
    {
        public static MetaCurrencySystem Instance { get; private set; }

        // 현재 보유량
        private int boneCount = 0;
        private int soulCount = 0;

        // 이벤트
        public event Action<MetaCurrency, int> OnCurrencyChanged;

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

        /// <summary>
        /// 화폐 추가
        /// </summary>
        public void AddCurrency(MetaCurrency type, int amount)
        {
            if (amount <= 0)
                return;

            switch (type)
            {
                case MetaCurrency.Bone:
                    boneCount += amount;
                    break;
                case MetaCurrency.Soul:
                    soulCount += amount;
                    break;
            }

            OnCurrencyChanged?.Invoke(type, GetCurrency(type));
            Debug.Log($"[MetaCurrency] {type} +{amount} (총: {GetCurrency(type)})");
        }

        /// <summary>
        /// 화폐 사용
        /// </summary>
        public bool SpendCurrency(MetaCurrency type, int amount)
        {
            if (amount <= 0)
                return false;

            int current = GetCurrency(type);
            if (current < amount)
            {
                Debug.LogWarning($"[MetaCurrency] {type} 부족 (보유: {current}, 필요: {amount})");
                return false;
            }

            switch (type)
            {
                case MetaCurrency.Bone:
                    boneCount -= amount;
                    break;
                case MetaCurrency.Soul:
                    soulCount -= amount;
                    break;
            }

            OnCurrencyChanged?.Invoke(type, GetCurrency(type));
            Debug.Log($"[MetaCurrency] {type} -{amount} (남은: {GetCurrency(type)})");
            return true;
        }

        /// <summary>
        /// 현재 보유량 확인
        /// </summary>
        public int GetCurrency(MetaCurrency type)
        {
            return type switch
            {
                MetaCurrency.Bone => boneCount,
                MetaCurrency.Soul => soulCount,
                _ => 0
            };
        }

        /// <summary>
        /// 세이브/로드
        /// </summary>
        public void SaveToData(SaveData saveData)
        {
            saveData.metaBoneCount = boneCount;
            saveData.metaSoulCount = soulCount;
        }

        public void LoadFromData(SaveData saveData)
        {
            boneCount = saveData.metaBoneCount;
            soulCount = saveData.metaSoulCount;
        }
    }
}
```

**체크리스트**:
- [ ] MetaCurrencySystem.cs 파일 생성
- [ ] Singleton 패턴 구현
- [ ] AddCurrency() 메서드 구현
- [ ] SpendCurrency() 메서드 구현
- [ ] GetCurrency() 메서드 구현
- [ ] SaveToData(), LoadFromData() 메서드 구현
- [ ] SaveData.cs에 metaBoneCount, metaSoulCount 필드 추가

---

## ✅ 시스템별 구현 체크리스트

### Room Generator 체크리스트
- [ ] RoomType Enum 생성
- [ ] RoomData 확장 (roomType, width, height 등)
- [ ] RoomGenerator 클래스 생성
- [ ] GenerateRoom() 메서드 구현
- [ ] 바닥/플랫폼/장애물 생성 로직
- [ ] Unity에서 Tilemap 설정
- [ ] 테스트 씬에서 동작 확인

### Dungeon Generator 체크리스트
- [ ] DungeonLayout, DungeonNode 클래스 생성
- [ ] DungeonGenerator 클래스 생성
- [ ] GenerateDungeon() 메서드 구현
- [ ] 시작 방 → 보스 방 경로 보장
- [ ] 선택적 경로 생성
- [ ] 테스트 씬에서 레이아웃 확인

### Minimap 체크리스트
- [ ] MinimapUI 클래스 생성 (BaseUI 상속)
- [ ] GenerateMinimap() 메서드 구현
- [ ] UpdateCurrentRoom() 메서드 구현
- [ ] 방 아이콘 프리팹 생성
- [ ] Unity에서 UI 배치
- [ ] 테스트 씬에서 동작 확인

### Skull System 체크리스트
- [ ] SkullData ScriptableObject 생성
- [ ] SkullManager 클래스 생성
- [ ] TransformToNextSkull() 메서드 구현
- [ ] EquipSkull() 메서드 구현
- [ ] 스탯/비주얼/어빌리티 적용 로직
- [ ] Unity에서 SkullData 에셋 생성 (5개 이상)
- [ ] 테스트 씬에서 Q키 변신 확인

### Meta Currency 체크리스트
- [ ] MetaCurrency Enum 생성
- [ ] MetaCurrencySystem 클래스 생성
- [ ] AddCurrency(), SpendCurrency() 메서드 구현
- [ ] SaveData에 메타 화폐 필드 추가
- [ ] SaveSystem 연동
- [ ] 테스트 씬에서 획득/사용 확인

---

## 🐛 트러블슈팅 가이드

### 문제 1: Tilemap이 표시되지 않음

**증상**:
- RoomGenerator로 방을 생성했지만 Tilemap이 화면에 표시되지 않음

**원인**:
- Tilemap Renderer의 Sorting Layer 설정 누락
- Tile 참조가 null
- Tilemap GameObject가 비활성화

**해결 방법**:
1. Tilemap GameObject 확인:
   - Hierarchy에서 Tilemap 선택
   - Inspector에서 Tilemap Renderer 확인
   - Sorting Layer: "Ground" 또는 "Default"
   - Order in Layer: 0

2. Tile 참조 확인:
   - RoomGenerator Inspector에서 groundTile, platformTile 등이 할당되어 있는지 확인
   - Tile 에셋이 Project 폴더에 존재하는지 확인

3. Tilemap 활성화 확인:
   - Hierarchy에서 Tilemap GameObject가 체크되어 있는지 확인

---

### 문제 2: 스컬 변신 시 스프라이트가 바뀌지 않음

**증상**:
- Q키를 눌러도 스프라이트가 변경되지 않음

**원인**:
- SkullData의 animatorController 또는 idleSprite가 null
- PlayerRenderer 참조가 올바르지 않음
- Animator Component가 없음

**해결 방법**:
1. SkullData 확인:
   - Project 폴더에서 SkullData 에셋 선택
   - Inspector에서 Animator Controller, Idle Sprite 할당 확인

2. PlayerRenderer 확인:
   - Player GameObject에 SpriteRenderer Component 존재 확인
   - SkullManager의 Start()에서 GetComponent<SpriteRenderer>() 제대로 동작하는지 디버그

3. Animator 확인:
   - Player GameObject에 Animator Component 존재 확인
   - Runtime Animator Controller가 할당되어 있는지 확인

---

### 문제 3: BaseUI를 상속한 UI가 표시되지 않음

**증상**:
- Show() 메서드를 호출해도 UI가 화면에 나타나지 않음

**원인**:
- Panel GameObject가 null
- Canvas가 비활성화되어 있음
- Panel의 부모 계층 구조가 잘못됨

**해결 방법**:
1. Panel 확인:
   - Hierarchy에서 UI GameObject 선택
   - "Panel"이라는 이름의 자식 GameObject가 있는지 확인
   - Inspector에서 panel 필드가 할당되어 있는지 확인

2. Canvas 확인:
   - Hierarchy에서 "=== UI CANVAS ===" GameObject 확인
   - Canvas Component가 활성화되어 있는지 확인

3. InitializePanel() 디버그:
   - BaseUI의 InitializePanel()에 Debug.Log 추가
   - Panel이 제대로 찾아지는지 확인

---

## 📚 코드 스니펫 라이브러리

### Singleton 패턴 (DontDestroyOnLoad)

```csharp
public class ExampleManager : MonoBehaviour
{
    public static ExampleManager Instance { get; private set; }

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
}
```

---

### ScriptableObject 생성 템플릿

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewData", menuName = "GASPT/Data/New Data")]
public class ExampleData : ScriptableObject
{
    [Header("Basic Info")]
    public string dataName;

    [TextArea(3, 5)]
    public string description;

    [Header("Settings")]
    [Range(0, 100)]
    public int someValue = 50;
}
```

---

### BaseUI 상속 템플릿

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace GASPT.UI
{
    public class ExampleUI : BaseUI
    {
        [Header("UI Elements")]
        [SerializeField] private Text titleText;
        [SerializeField] private Button confirmButton;

        protected override void Awake()
        {
            base.Awake(); // Panel 자동 찾기

            // 버튼 이벤트 연결
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
        }

        protected override void Initialize()
        {
            // 추가 초기화 로직
        }

        private void OnConfirmClicked()
        {
            Hide();
        }
    }
}
```

---

### async Awaitable 패턴

```csharp
using UnityEngine;

public class ExampleAsync : MonoBehaviour
{
    private async Awaitable LoadDataAsync()
    {
        Debug.Log("로딩 시작");

        // 1초 대기
        await Awaitable.WaitForSecondsAsync(1f);

        Debug.Log("로딩 완료");
    }

    private void Start()
    {
        LoadDataAsync().Forget(); // Fire and forget
    }
}
```

---

### Event System 패턴

```csharp
using System;

public class ExampleEventSystem
{
    // 이벤트 정의
    public event Action<int> OnValueChanged;

    private int value;

    // 값 변경 시 이벤트 발생
    public void SetValue(int newValue)
    {
        value = newValue;
        OnValueChanged?.Invoke(value);
    }
}

// 사용 예시
public class ExampleListener : MonoBehaviour
{
    private ExampleEventSystem system;

    private void Start()
    {
        system = new ExampleEventSystem();

        // 이벤트 구독
        system.OnValueChanged += OnValueChangedHandler;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (system != null)
        {
            system.OnValueChanged -= OnValueChangedHandler;
        }
    }

    private void OnValueChangedHandler(int newValue)
    {
        Debug.Log($"값 변경: {newValue}");
    }
}
```

---

### Object Pooling 패턴

```csharp
using UnityEngine;
using GASPT.ObjectPool;

public class ExamplePooling : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private ObjectPool<GameObject> pool;

    private void Start()
    {
        // 풀 초기화
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            onGet: obj => obj.SetActive(true),
            onRelease: obj => obj.SetActive(false),
            onDestroy: obj => Destroy(obj),
            defaultCapacity: 10,
            maxSize: 100
        );
    }

    private void SpawnObject()
    {
        // 풀에서 가져오기
        GameObject obj = pool.Get();

        // 5초 후 풀에 반환
        Invoke(nameof(ReturnObject), 5f);
    }

    private void ReturnObject(GameObject obj)
    {
        pool.Release(obj);
    }
}
```

---

## 🔗 관련 문서

- [PROJECT_MASTER_ROADMAP.md](PROJECT_MASTER_ROADMAP.md) - 전체 로드맵
- [WORK_HISTORY.md](WORK_HISTORY.md) - 완료된 작업 내역
- [CodingGuidelines.md](CodingGuidelines.md) - 코딩 규칙
- [UI_SYSTEM_DESIGN.md](../guides/UI_SYSTEM_DESIGN.md) - UI 시스템 설계

---

**최종 업데이트**: 2025-11-19
**작성자**: GASPT 개발팀

---

*이 가이드는 서버 오류 시 수동으로 작업을 진행할 수 있도록 설계되었습니다.*
*체크리스트를 따라 단계별로 구현하고, 코드 스니펫을 참조하세요.*
