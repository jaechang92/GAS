using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Extensions;
using GASPT.Core;
using GASPT.Gameplay.Level.Graph;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 방 관리 싱글톤
    /// 여러 방의 전환 및 상태 관리
    /// - DungeonConfig 기반 동적 Room 로딩 지원
    /// </summary>
    public partial class RoomManager : SingletonManager<RoomManager>
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
    }
}
