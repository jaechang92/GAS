using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 방 관리 싱글톤
    /// 여러 방의 전환 및 상태 관리
    /// </summary>
    public class RoomManager : SingletonManager<RoomManager>
    {
        // ====== 방 목록 ======

        [Header("방 목록")]
        [Tooltip("Scene에 있는 모든 방 (자동 찾기)")]
        [SerializeField] private bool autoFindRooms = true;

        [Tooltip("수동 할당 (autoFindRooms가 false일 때)")]
        [SerializeField] private Room[] manualRooms;

        [Header("디버그 (읽기 전용)")]
        [Tooltip("현재 등록된 방 목록 (자동 업데이트)")]
        [SerializeField] private List<Room> rooms = new List<Room>();


        // ====== 현재 상태 ======

        private Room currentRoom;
        private int currentRoomIndex = -1;


        // ====== 이벤트 ======

        public event Action<Room> OnRoomChanged;
        public event Action<Room> OnRoomCleared;


        // ====== 프로퍼티 ======

        public Room CurrentRoom => currentRoom;
        public int TotalRoomCount => rooms.Count;
        public int CurrentRoomIndex => currentRoomIndex;


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
        }

        /// <summary>
        /// 방 목록 초기화
        /// </summary>
        private void InitializeRooms()
        {
            rooms.Clear();

            if (autoFindRooms)
            {
                // FindObjectsInactive.Include로 비활성 GameObject도 찾음
                Room[] foundRooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                rooms.AddRange(foundRooms);
                Debug.Log($"[RoomManager] {foundRooms.Length}개의 방 자동 탐색 (비활성 포함)");
            }
            else
            {
                if (manualRooms != null)
                {
                    rooms.AddRange(manualRooms);
                }
            }

            // 모든 방 이벤트 구독
            foreach (var room in rooms)
            {
                room.OnRoomEnter += OnRoomEnter;
                room.OnRoomClear += OnRoomClear;
                room.OnRoomFail += OnRoomFail;
            }

            Debug.Log($"[RoomManager] 총 {rooms.Count}개의 방 초기화 완료");
        }

        /// <summary>
        /// 방 목록 새로고침 (Scene 로드 후 수동 호출용)
        /// </summary>
        public void RefreshRooms()
        {
            Debug.Log("[RoomManager] 방 목록 새로고침 시작...");
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

            Debug.Log($"[RoomManager] {currentRoom.name}으로 이동 ({currentRoomIndex + 1}/{rooms.Count})");
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
            Debug.Log("[RoomManager] 던전 시작!");
        }


        // ====== 방 이벤트 처리 ======

        /// <summary>
        /// 방 진입 시 호출
        /// </summary>
        private void OnRoomEnter(Room room)
        {
            Debug.Log($"[RoomManager] 방 진입: {room.name}");
        }

        /// <summary>
        /// 방 클리어 시 호출
        /// </summary>
        private void OnRoomClear(Room room)
        {
            Debug.Log($"[RoomManager] 방 클리어: {room.name}");
            OnRoomCleared?.Invoke(room);

            // 자동으로 다음 방 이동 (선택사항)
            // MoveToNextRoomAsync().Forget();
        }

        /// <summary>
        /// 방 실패 시 호출
        /// </summary>
        private void OnRoomFail(Room room, string reason)
        {
            Debug.Log($"[RoomManager] 방 실패: {room.name} - {reason}");
            // TODO: 게임 오버 처리
        }

        /// <summary>
        /// 던전 클리어 (모든 방 완료)
        /// </summary>
        private void OnDungeonComplete()
        {
            Debug.Log("[RoomManager] 던전 클리어!");
            // TODO: 클리어 UI, 보상 등
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
