using UnityEngine;
using TMPro;
using GASPT.Gameplay.Level;

namespace GASPT.UI
{
    /// <summary>
    /// 방 정보 UI 컴포넌트
    /// 현재 방 번호, 총 방 수, 적 수를 실시간 표시
    /// </summary>
    public class RoomInfoUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private TMP_Text roomText;
        [SerializeField] private TMP_Text enemyText;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            // Start에서 구독 (모든 Awake 완료 후 → RoomManager 싱글톤 보장)
            SubscribeToRoomManager();
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            UnsubscribeFromRoomManager();
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 방 변경 시 호출
        /// </summary>
        private void OnRoomChanged(Room newRoom)
        {
            // 이전 방 이벤트 구독 해제
            UnsubscribeFromCurrentRoom();

            // 새 방 이벤트 구독
            if (newRoom != null)
            {
                newRoom.OnEnemyCountChanged += OnEnemyCountChanged;
            }

            // UI 업데이트
            UpdateRoomInfoUI();
        }

        /// <summary>
        /// 적 수 변경 시 호출
        /// </summary>
        private void OnEnemyCountChanged(Room room, int enemyCount)
        {
            UpdateEnemyCountUI(enemyCount);
        }


        // ====== UI 업데이트 ======

        /// <summary>
        /// 방 정보 UI 업데이트 (방 번호/총 방 수)
        /// </summary>
        private void UpdateRoomInfoUI()
        {
            if (!RoomManager.HasInstance)
            {
                if (roomText != null)
                {
                    roomText.text = "Room - / -";
                }
                if (enemyText != null)
                {
                    enemyText.text = "Enemies: 0";
                }
                return;
            }

            RoomManager roomManager = RoomManager.Instance;

            // 방 번호 업데이트
            if (roomText != null)
            {
                int currentIndex = roomManager.CurrentRoomIndex + 1; // 0-based → 1-based
                int totalCount = roomManager.TotalRoomCount;
                roomText.text = $"Room {currentIndex} / {totalCount}";
            }

            // 적 수 업데이트
            Room currentRoom = roomManager.CurrentRoom;
            if (currentRoom != null && enemyText != null)
            {
                enemyText.text = $"Enemies: {currentRoom.AliveEnemyCount}";
            }
        }

        /// <summary>
        /// 적 수 UI 업데이트
        /// </summary>
        private void UpdateEnemyCountUI(int enemyCount)
        {
            if (enemyText != null)
            {
                enemyText.text = $"Enemies: {enemyCount}";
            }
        }


        // ====== 이벤트 구독 관리 ======

        /// <summary>
        /// RoomManager 이벤트 구독
        /// </summary>
        private void SubscribeToRoomManager()
        {
            if (!RoomManager.HasInstance)
            {
                Debug.LogWarning("[RoomInfoUI] RoomManager를 찾을 수 없습니다.");
                return;
            }

            RoomManager.Instance.OnRoomChanged += OnRoomChanged;

            // 현재 방이 이미 설정되어 있다면 수동으로 호출하여 이벤트 구독
            if (RoomManager.Instance.CurrentRoom != null)
            {
                OnRoomChanged(RoomManager.Instance.CurrentRoom);
            }
        }

        /// <summary>
        /// RoomManager 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromRoomManager()
        {
            if (RoomManager.HasInstance)
            {
                RoomManager.Instance.OnRoomChanged -= OnRoomChanged;
            }

            // 현재 방 이벤트 구독 해제
            UnsubscribeFromCurrentRoom();
        }

        /// <summary>
        /// 현재 방 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromCurrentRoom()
        {
            if (RoomManager.HasInstance)
            {
                Room currentRoom = RoomManager.Instance.CurrentRoom;
                if (currentRoom != null)
                {
                    currentRoom.OnEnemyCountChanged -= OnEnemyCountChanged;
                }
            }
        }


        // ====== Context Menu 테스트 ======

        [ContextMenu("Update Room Info (Test)")]
        private void TestUpdateRoomInfo()
        {
            UpdateRoomInfoUI();
        }

        [ContextMenu("Print Room Info")]
        private void PrintRoomInfo()
        {
            if (!RoomManager.HasInstance)
            {
                Debug.LogWarning("[RoomInfoUI] RoomManager를 찾을 수 없습니다.");
                return;
            }

            RoomManager roomManager = RoomManager.Instance;
            Room currentRoom = roomManager.CurrentRoom;

            Debug.Log($"=== Room Info UI ===");
            Debug.Log($"Current Room: {currentRoom?.name ?? "None"}");
            Debug.Log($"Room Index: {roomManager.CurrentRoomIndex + 1} / {roomManager.TotalRoomCount}");
            Debug.Log($"Enemies: {currentRoom?.AliveEnemyCount ?? 0}");
            Debug.Log($"====================");
        }
    }
}
