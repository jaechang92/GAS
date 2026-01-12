using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;
using GASPT.Core;
using GASPT.Core.Enums;
using GASPT.Gameplay.Level.Graph;
using GASPT.Gameplay.Level.Generation;
using Random = UnityEngine.Random;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 방 관리 싱글톤
    /// 여러 방의 전환 및 상태 관리
    /// - DungeonConfig 기반 동적 Room 로딩 지원
    /// </summary>
    public class RoomManager : SingletonManager<RoomManager>
    {
        // ====== 던전 설정 ======

        [Header("던전 설정")]
        [Tooltip("현재 로드된 던전")]
        [SerializeField] private DungeonConfig currentDungeon;

        [Tooltip("Room Container (동적 생성된 Room들의 부모)")]
        private Transform roomContainer;


        // ====== 방 목록 (레거시 - 하위 호환성) ======

        [Header("방 목록 (레거시)")]
        [Tooltip("Scene에 있는 모든 방 (자동 찾기) - 레거시 방식")]
        [SerializeField] private bool autoFindRooms = false; // 기본값 false로 변경

        [Tooltip("수동 할당 (autoFindRooms가 false일 때) - 레거시 방식")]
        [SerializeField] private Room[] manualRooms;

        [Header("자동 시작")]
        [Tooltip("게임 시작 시 자동으로 첫 번째 방 진입")]
        [SerializeField] private bool autoStartFirstRoom = true;

        [Header("디버그 (읽기 전용)")]
        [Tooltip("현재 등록된 방 목록 (자동 업데이트)")]
        [SerializeField] private List<Room> rooms = new List<Room>();



        // ====== 현재 상태 ======

        private Room currentRoom;
        private int currentRoomIndex = -1;
        private int totalGoldEarned = 0;
        private int totalExpEarned = 0;


        // ====== 그래프 기반 던전 (신규) ======

        [Header("그래프 기반 던전")]
        [Tooltip("현재 던전 그래프")]
        private DungeonGraph dungeonGraph;

        [Tooltip("노드별 Room 인스턴스")]
        private Dictionary<string, Room> nodeRoomMap = new Dictionary<string, Room>();

        [Tooltip("던전 생성기")]
        private DungeonGenerator dungeonGenerator;


        // ====== 이벤트 ======

        public event Action<Room> OnRoomChanged;
        public event Action<Room> OnRoomCleared;
        public event Action OnDungeonCompleted;

        /// <summary>
        /// 노드 변경 이벤트 (그래프 기반)
        /// </summary>
        public event Action<DungeonNode> OnNodeChanged;

        /// <summary>
        /// 인접 노드 공개 이벤트 (미니맵용)
        /// </summary>
        public event Action<List<DungeonNode>> OnAdjacentNodesRevealed;


        // ====== 프로퍼티 ======

        public Room CurrentRoom => currentRoom;
        public int TotalRoomCount => rooms.Count;
        public int CurrentRoomIndex => currentRoomIndex;

        /// <summary>
        /// 클리어한 방 개수 (현재 방 인덱스 = 클리어한 방 수)
        /// </summary>
        public int CompletedRoomCount => Mathf.Max(0, currentRoomIndex);

        /// <summary>
        /// 현재 던전 그래프
        /// </summary>
        public DungeonGraph DungeonGraph => dungeonGraph;

        /// <summary>
        /// 현재 노드
        /// </summary>
        public DungeonNode CurrentNode => dungeonGraph?.CurrentNode;

        /// <summary>
        /// 그래프 기반 던전 사용 중 여부
        /// </summary>
        public bool IsUsingGraphDungeon => dungeonGraph != null;


        // ====== 초기화 ======

        protected override void Awake()
        {
            base.Awake();
            // Awake에서는 초기화하지 않음 (Scene 로드 전이므로)
        }

        private void Start()
        {
            // Start에서 초기화 (Scene 로드 완료 후)
            InitializeRooms();

            // GameManager가 있으면 자동 시작하지 않음 (GameManager가 제어)
            bool hasGameManager = GameManager.Instance != null;

            // 자동 시작 옵션이 켜져 있고 GameManager가 없으면 첫 방으로 자동 진입
            if (autoStartFirstRoom && !hasGameManager && rooms.Count > 0)
            {
                StartDungeonAsync().Forget();
            }
        }

        // ====== 던전 로딩 ======

        /// <summary>
        /// DungeonConfig 기반으로 던전 로드 (그래프 기반 생성)
        /// 참고: 일반적으로 DungeonGenerator + LoadDungeonGraph를 사용하는 것을 권장
        /// </summary>
        public void LoadDungeon(DungeonConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[RoomManager] DungeonConfig가 null입니다!");
                return;
            }

            currentDungeon = config;

            // 기존 방들 정리
            ClearRooms();

            // Room Container 생성
            CreateRoomContainer();

            // 그래프 기반 던전 생성
            GenerateProceduralDungeon(config.generationRules, config.roomDataPool, config.roomTemplatePrefab);

            // 모든 방 이벤트 구독
            SubscribeRoomEvents();
        }

        /// <summary>
        /// Room Container 생성
        /// </summary>
        private void CreateRoomContainer()
        {
            if (roomContainer != null)
            {
                Destroy(roomContainer.gameObject);
            }

            GameObject containerObj = new GameObject("RoomContainer");
            roomContainer = containerObj.transform;
            roomContainer.SetParent(transform); // RoomManager의 자식으로
        }

        /// <summary>
        /// Prefab 방식: Room Prefab 로드
        /// </summary>
        private void LoadRoomsFromPrefabs(Room[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError("[RoomManager] roomPrefabs가 비어있습니다!");
                return;
            }

            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                {
                    Debug.LogWarning("[RoomManager] null Prefab을 건너뜁니다.");
                    continue;
                }

                // Prefab 인스턴스화
                Room room = Instantiate(prefab, roomContainer);
                room.gameObject.SetActive(false); // 초기 비활성화
                rooms.Add(room);
            }
        }

        /// <summary>
        /// Data 방식: RoomData로 Room 동적 생성
        /// </summary>
        private void GenerateRoomsFromData(RoomData[] dataList, Room templatePrefab)
        {
            if (dataList == null || dataList.Length == 0)
            {
                Debug.LogError("[RoomManager] roomDataList가 비어있습니다!");
                return;
            }

            if (templatePrefab == null)
            {
                Debug.LogError("[RoomManager] roomTemplatePrefab이 없습니다!");
                return;
            }

            foreach (var data in dataList)
            {
                if (data == null)
                {
                    Debug.LogWarning("[RoomManager] null RoomData를 건너뜁니다.");
                    continue;
                }

                // 템플릿 Prefab으로 Room 생성
                Room room = Instantiate(templatePrefab, roomContainer);
                room.gameObject.SetActive(false);
                room.name = $"Room_{data.roomName}";

                // RoomData 주입 (Room.Initialize 호출)
                room.Initialize(data);

                rooms.Add(room);
            }
        }

        /// <summary>
        /// Procedural 방식: 룰 기반 랜덤 던전 생성
        /// </summary>
        private void GenerateProceduralDungeon(RoomGenerationRules rules, RoomData[] pool, Room templatePrefab)
        {
            if (rules == null)
            {
                Debug.LogError("[RoomManager] generationRules가 없습니다!");
                return;
            }

            if (pool == null || pool.Length == 0)
            {
                Debug.LogError("[RoomManager] roomDataPool이 비어있습니다!");
                return;
            }

            if (templatePrefab == null)
            {
                Debug.LogError("[RoomManager] roomTemplatePrefab이 없습니다!");
                return;
            }

            // 방 개수 결정
            int roomCount = Random.Range(rules.minRooms, rules.maxRooms + 1);

            // 방 생성
            for (int i = 0; i < roomCount; i++)
            {
                // 진행도 계산 (0~1)
                float progress = roomCount > 1 ? (float)i / (roomCount - 1) : 0f;

                // RoomData 선택
                RoomData selectedData = SelectRoomDataForProcedural(pool, i, roomCount, progress, rules);

                if (selectedData == null)
                {
                    Debug.LogWarning($"[RoomManager] {i}번째 방의 RoomData 선택 실패!");
                    continue;
                }

                // Room 생성
                Room room = Instantiate(templatePrefab, roomContainer);
                room.gameObject.SetActive(false);
                room.name = $"Room_{i:D2}_{selectedData.roomName}";

                // RoomData 주입
                room.Initialize(selectedData);

                rooms.Add(room);
            }

            // 보스 방 추가
            if (rules.includeBossRoom)
            {
                AddProceduralBossRoom(pool, templatePrefab);
            }
        }

        /// <summary>
        /// Procedural용 RoomData 선택
        /// </summary>
        private RoomData SelectRoomDataForProcedural(RoomData[] pool, int index, int totalRooms, float progress, RoomGenerationRules rules)
        {
            // 첫 번째 방은 쉬운 방
            if (index == 0 && rules.firstRoomAlwaysEasy)
            {
                var easyRooms = System.Array.FindAll(pool, r => r.difficulty <= 2);
                return easyRooms.Length > 0 ? easyRooms[Random.Range(0, easyRooms.Length)] : pool[Random.Range(0, pool.Length)];
            }

            // 난이도 기반 선택
            int targetDifficulty = rules.GetDifficultyForProgress(progress);

            // 난이도가 가장 가까운 RoomData 찾기
            RoomData closestRoom = null;
            int minDiff = int.MaxValue;

            foreach (var data in pool)
            {
                // 보스 방은 제외
                if (data.roomType == RoomType.Boss) continue;

                int diff = Mathf.Abs(data.difficulty - targetDifficulty);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestRoom = data;
                }
            }

            return closestRoom ?? pool[Random.Range(0, pool.Length)];
        }

        /// <summary>
        /// Procedural 보스 방 추가
        /// </summary>
        private void AddProceduralBossRoom(RoomData[] pool, Room templatePrefab)
        {
            // 보스 방 RoomData 찾기
            var bossRooms = System.Array.FindAll(pool, r => r.roomType == RoomType.Boss);

            if (bossRooms.Length == 0)
            {
                Debug.LogWarning("[RoomManager] roomDataPool에 보스 방이 없습니다!");
                return;
            }

            RoomData bossData = bossRooms[Random.Range(0, bossRooms.Length)];

            // 보스 방 생성
            Room bossRoom = Instantiate(templatePrefab, roomContainer);
            bossRoom.gameObject.SetActive(false);
            bossRoom.name = $"Room_Boss_{bossData.roomName}";
            bossRoom.Initialize(bossData);

            rooms.Add(bossRoom);
        }

        /// <summary>
        /// 기존 방들 정리
        /// </summary>
        private void ClearRooms()
        {
            // 이벤트 구독 해제
            foreach (var room in rooms)
            {
                if (room != null)
                {
                    room.OnRoomEnter -= OnRoomEnter;
                    room.OnRoomClear -= OnRoomClear;
                    room.OnRoomFail -= OnRoomFail;
                }
            }

            rooms.Clear();

            // Room Container 제거
            if (roomContainer != null)
            {
                Destroy(roomContainer.gameObject);
                roomContainer = null;
            }
        }

        /// <summary>
        /// 모든 방 이벤트 구독
        /// </summary>
        private void SubscribeRoomEvents()
        {
            foreach (var room in rooms)
            {
                room.OnRoomEnter += OnRoomEnter;
                room.OnRoomClear += OnRoomClear;
                room.OnRoomFail += OnRoomFail;
            }
        }


        // ====== 방 목록 초기화 (레거시) ======

        /// <summary>
        /// 방 목록 초기화 (레거시 - 하위 호환성)
        /// </summary>
        private void InitializeRooms()
        {
            rooms.Clear();

            if (autoFindRooms)
            {
                // FindObjectsInactive.Include로 비활성 GameObject도 찾음
                Room[] foundRooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                rooms.AddRange(foundRooms);
            }
            else
            {
                if (manualRooms != null)
                {
                    rooms.AddRange(manualRooms);
                }
            }

            // 방 순서 정렬 (StartRoom → 일반 방들 → BossRoom)
            SortRooms();

            // 모든 방 이벤트 구독
            foreach (var room in rooms)
            {
                room.OnRoomEnter += OnRoomEnter;
                room.OnRoomClear += OnRoomClear;
                room.OnRoomFail += OnRoomFail;
            }
        }

        /// <summary>
        /// 방 목록 정렬 (StartRoom → 일반 방들 → BossRoom)
        /// </summary>
        private void SortRooms()
        {
            if (rooms.Count <= 1)
            {
                return; // 1개 이하면 정렬 불필요
            }

            rooms.Sort((a, b) =>
            {
                // StartRoom이 항상 첫 번째
                if (a.name.Contains("StartRoom")) return -1;
                if (b.name.Contains("StartRoom")) return 1;

                // BossRoom이 항상 마지막
                if (a.name.Contains("BossRoom")) return 1;
                if (b.name.Contains("BossRoom")) return -1;

                // 나머지는 X 좌표 또는 이름순으로 정렬 (X 좌표 기준)
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });
        }

        /// <summary>
        /// 방 목록 새로고침 (Scene 로드 후 수동 호출용)
        /// </summary>
        public void RefreshRooms()
        {
            InitializeRooms();
        }


        // ====== 방 전환 ======

        /// <summary>
        /// 다음 방으로 이동
        /// </summary>
        public async Awaitable MoveToNextRoomAsync()
        {
            int nextIndex = currentRoomIndex + 1;

            if (nextIndex >= rooms.Count)
            {
                Debug.LogWarning("[RoomManager] 더 이상 방이 없습니다! (던전 클리어)");
                OnDungeonComplete();
                return;
            }

            await MoveToRoomAsync(nextIndex);
        }

        /// <summary>
        /// 특정 방으로 이동
        /// </summary>
        public async Awaitable MoveToRoomAsync(int roomIndex)
        {
            if (roomIndex < 0 || roomIndex >= rooms.Count)
            {
                Debug.LogError($"[RoomManager] 잘못된 방 인덱스: {roomIndex} (총 {rooms.Count}개)");
                return;
            }

            Room nextRoom = rooms[roomIndex];

            // 이전 방 비활성화
            if (currentRoom != null)
            {
                currentRoom.gameObject.SetActive(false);
            }

            // 새 방 활성화 및 진입
            currentRoom = nextRoom;
            currentRoomIndex = roomIndex;
            currentRoom.gameObject.SetActive(true);

            // 이벤트 발생
            OnRoomChanged?.Invoke(currentRoom);

            // 방 진입 실행
            await currentRoom.EnterRoomAsync();
        }

        /// <summary>
        /// 첫 방으로 이동 (게임 시작)
        /// </summary>
        public async Awaitable StartDungeonAsync()
        {
            if (rooms.Count == 0)
            {
                Debug.LogError("[RoomManager] 방이 하나도 없습니다!");
                return;
            }

            await MoveToRoomAsync(0);
        }


        // ====== 방 이벤트 처리 ======

        /// <summary>
        /// 방 진입 시 호출
        /// </summary>
        private void OnRoomEnter(Room room)
        {
            // 방 진입 처리
        }

        /// <summary>
        /// 방 클리어 시 호출
        /// </summary>
        private void OnRoomClear(Room room)
        {
            OnRoomCleared?.Invoke(room);

            // 자동으로 다음 방 이동 (선택사항)
            // MoveToNextRoomAsync().Forget();
        }

        /// <summary>
        /// 방 실패 시 호출
        /// </summary>
        private void OnRoomFail(Room room, string reason)
        {
            // TODO: 게임 오버 처리
        }

        /// <summary>
        /// 던전 클리어 (모든 방 완료)
        /// </summary>
        private void OnDungeonComplete()
        {

            // 보스 방 여부 확인
            bool isBossRoom = currentRoom != null && currentRoom.name.Contains("Boss");

            // 던전 완주 보상 지급
            GiveDungeonCompleteRewards(isBossRoom);

            // 플레이어 완전 회복
            HealPlayerFull();

            // 이벤트 발생 (GameFlowStateMachine이 UI 표시 담당)
            OnDungeonCompleted?.Invoke();

            // GameManager에 던전 클리어 알림 (메타 골드 저장)
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnDungeonCleared();
            }
            else
            {
                Debug.LogWarning("[RoomManager] GameManager가 없습니다. 메타 골드가 저장되지 않습니다.");
            }
        }

        /// <summary>
        /// 던전 완주 보상 지급
        /// </summary>
        private void GiveDungeonCompleteRewards(bool isBossRoom)
        {
            // 보스 방 클리어 시 특별 보상 (x2)
            int bonusGold = isBossRoom ? 500 : 200;
            int bonusExp = isBossRoom ? 1000 : 500;

            // 골드 지급
            var currencySystem = GASPT.Economy.CurrencySystem.Instance;
            if (currencySystem != null)
            {
                currencySystem.AddGold(bonusGold);
                totalGoldEarned += bonusGold;
            }

            // 경험치 지급
            var playerLevel = GASPT.Level.PlayerLevel.Instance;
            if (playerLevel != null)
            {
                playerLevel.AddExp(bonusExp);
                totalExpEarned += bonusExp;
            }
        }

        /// <summary>
        /// 플레이어 완전 회복
        /// </summary>
        private void HealPlayerFull()
        {
            var playerStats = FindAnyObjectByType<GASPT.Stats.PlayerStats>();

            if (playerStats == null)
            {
                Debug.LogWarning("[RoomManager] PlayerStats를 찾을 수 없습니다. 체력 회복 불가.");
                return;
            }

            // 완전 회복 (Revive 메서드 사용)
            playerStats.Revive();
        }


        // ====== 그래프 기반 던전 관리 (신규) ======

        /// <summary>
        /// 던전 그래프 로드
        /// </summary>
        public void LoadDungeonGraph(DungeonGraph graph, Dictionary<string, Room> roomMap)
        {
            if (graph == null)
            {
                Debug.LogError("[RoomManager] DungeonGraph가 null입니다!");
                return;
            }

            // 기존 방 정리
            ClearRooms();

            // 그래프 설정
            dungeonGraph = graph;
            nodeRoomMap = roomMap ?? new Dictionary<string, Room>();

            // rooms 리스트도 업데이트 (하위 호환성)
            rooms.Clear();
            rooms.AddRange(nodeRoomMap.Values);
        }

        /// <summary>
        /// 특정 노드로 이동 (그래프 기반)
        /// </summary>
        public async Awaitable MoveToNodeAsync(DungeonNode targetNode)
        {
            if (targetNode == null)
            {
                Debug.LogError("[RoomManager] 대상 노드가 null입니다!");
                return;
            }

            if (dungeonGraph == null)
            {
                Debug.LogError("[RoomManager] 던전 그래프가 로드되지 않았습니다!");
                return;
            }

            // 대상 Room 가져오기
            if (!nodeRoomMap.TryGetValue(targetNode.nodeId, out var targetRoom))
            {
                Debug.LogError($"[RoomManager] 노드 {targetNode.nodeId}에 대응하는 Room이 없습니다!");
                return;
            }

            // 현재 방 비활성화
            if (currentRoom != null)
            {
                currentRoom.gameObject.SetActive(false);
            }

            // 그래프 상태 업데이트
            dungeonGraph.SetCurrentNode(targetNode.nodeId);

            // 새 방 활성화
            currentRoom = targetRoom;
            currentRoom.gameObject.SetActive(true);

            // 이벤트 발생
            OnRoomChanged?.Invoke(currentRoom);
            OnNodeChanged?.Invoke(targetNode);

            // 인접 노드 공개
            var adjacentNodes = dungeonGraph.GetAdjacentNodes(targetNode.nodeId);
            foreach (var adj in adjacentNodes)
            {
                adj.Reveal();
            }
            OnAdjacentNodesRevealed?.Invoke(adjacentNodes);

            // 방 진입 실행
            await currentRoom.EnterRoomAsync();
        }

        /// <summary>
        /// 노드 ID로 이동
        /// </summary>
        public async Awaitable MoveToNodeAsync(string nodeId)
        {
            var node = dungeonGraph?.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogError($"[RoomManager] 노드 {nodeId}를 찾을 수 없습니다!");
                return;
            }
            await MoveToNodeAsync(node);
        }

        /// <summary>
        /// 이동 가능한 다음 노드들 조회
        /// </summary>
        public List<DungeonNode> GetAvailableNextNodes()
        {
            if (dungeonGraph == null || dungeonGraph.currentNodeId == null)
            {
                return new List<DungeonNode>();
            }

            return dungeonGraph.GetAdjacentNodes(dungeonGraph.currentNodeId);
        }

        /// <summary>
        /// 미방문 다음 노드들 조회
        /// </summary>
        public List<DungeonNode> GetUnvisitedNextNodes()
        {
            if (dungeonGraph == null || dungeonGraph.currentNodeId == null)
            {
                return new List<DungeonNode>();
            }

            return dungeonGraph.GetUnvisitedAdjacentNodes(dungeonGraph.currentNodeId);
        }

        /// <summary>
        /// 그래프 기반 던전 시작 (Entry 노드로 이동)
        /// </summary>
        public async Awaitable StartGraphDungeonAsync()
        {
            if (dungeonGraph == null)
            {
                Debug.LogError("[RoomManager] 던전 그래프가 로드되지 않았습니다!");
                return;
            }

            // Entry 노드로 이동
            var entryNode = dungeonGraph.EntryNode;
            if (entryNode == null)
            {
                Debug.LogError("[RoomManager] Entry 노드를 찾을 수 없습니다!");
                return;
            }

            await MoveToNodeAsync(entryNode);
        }

        /// <summary>
        /// 노드 ID로 Room 가져오기
        /// </summary>
        public Room GetRoomByNodeId(string nodeId)
        {
            if (nodeRoomMap.TryGetValue(nodeId, out var room))
            {
                return room;
            }
            return null;
        }


        // ====== 방 조회 ======

        /// <summary>
        /// 특정 인덱스의 방 가져오기
        /// </summary>
        public Room GetRoom(int index)
        {
            if (index < 0 || index >= rooms.Count)
            {
                Debug.LogWarning($"[RoomManager] 잘못된 방 인덱스: {index}");
                return null;
            }

            return rooms[index];
        }

        /// <summary>
        /// 모든 방 가져오기
        /// </summary>
        public List<Room> GetAllRooms()
        {
            return new List<Room>(rooms);
        }


        // ====== 디버그 ======

        [ContextMenu("Print Room List")]
        private void PrintRoomList()
        {
            Debug.Log($"=== Room Manager ===");
            Debug.Log($"Total Rooms: {rooms.Count}");
            Debug.Log($"Current Room: {(currentRoom != null ? currentRoom.name : "None")} ({currentRoomIndex + 1}/{rooms.Count})");
            Debug.Log("Rooms:");

            for (int i = 0; i < rooms.Count; i++)
            {
                string status = (i == currentRoomIndex) ? "[CURRENT]" : "";
                string cleared = rooms[i].IsCleared ? "[CLEARED]" : "";
                Debug.Log($"  [{i}] {rooms[i].name} {status} {cleared}");
            }

            Debug.Log("====================");
        }

        [ContextMenu("Refresh Rooms")]
        private void DebugRefreshRooms()
        {
            RefreshRooms();
        }

        [ContextMenu("Start Dungeon (Test)")]
        private void TestStartDungeon()
        {
            if (Application.isPlaying)
            {
                StartDungeonAsync().Forget();
            }
            else
            {
                Debug.LogWarning("[RoomManager] Play 모드에서만 실행 가능합니다.");
            }
        }

        [ContextMenu("Move To Next Room (Test)")]
        private void TestMoveToNextRoom()
        {
            if (Application.isPlaying)
            {
                MoveToNextRoomAsync().Forget();
            }
            else
            {
                Debug.LogWarning("[RoomManager] Play 모드에서만 실행 가능합니다.");
            }
        }
    }
}
