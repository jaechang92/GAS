using UnityEngine;
using GASPT.Core;
using GASPT.Core.GameFlow;
using GASPT.Gameplay.Level;
using GASPT.Economy;
using GASPT.Stats;
using GASPT.UI;
using GASPT.Gameplay.Reward;

namespace GASPT.Test
{
    /// <summary>
    /// 게임 전체 플로우 테스트 도구
    /// 에디터에서 각 단계별 테스트 가능
    /// </summary>
    public class GameFlowPlayTest : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool autoTest = false;
        [SerializeField] private float autoTestDelay = 2f;

        private float autoTestTimer;


        private void Update()
        {
            // 자동 테스트
            if (autoTest)
            {
                autoTestTimer += Time.deltaTime;
                if (autoTestTimer >= autoTestDelay)
                {
                    autoTestTimer = 0f;
                    RunAutoTest();
                }
            }

            // 수동 테스트 키
            HandleTestInput();
        }

        private void HandleTestInput()
        {
            // F1: 현재 상태 출력
            if (Input.GetKeyDown(KeyCode.F1))
            {
                PrintCurrentState();
            }

            // F2: 던전 입장 테스트
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestEnterDungeon();
            }

            // F3: 방 클리어 테스트
            if (Input.GetKeyDown(KeyCode.F3))
            {
                TestRoomClear();
            }

            // F4: 다음 방 이동 테스트
            if (Input.GetKeyDown(KeyCode.F4))
            {
                TestNextRoom();
            }

            // F5: 플레이어 사망 테스트
            if (Input.GetKeyDown(KeyCode.F5))
            {
                TestPlayerDeath();
            }

            // F6: 던전 클리어 테스트
            if (Input.GetKeyDown(KeyCode.F6))
            {
                TestDungeonClear();
            }

            // F7: 보상 스폰 테스트
            if (Input.GetKeyDown(KeyCode.F7))
            {
                TestRewardSpawn();
            }

            // F8: 골드 추가 테스트
            if (Input.GetKeyDown(KeyCode.F8))
            {
                TestAddGold();
            }

            // F9: 플레이어 회복 테스트
            if (Input.GetKeyDown(KeyCode.F9))
            {
                TestPlayerHeal();
            }

            // F10: StartRoom 복귀 테스트
            if (Input.GetKeyDown(KeyCode.F10))
            {
                TestReturnToStart();
            }
        }


        // ====== 테스트 메서드 ======

        [ContextMenu("1. 현재 상태 출력 (F1)")]
        private void PrintCurrentState()
        {
            Debug.Log("========== 게임 플로우 상태 ==========");

            // GameFlowStateMachine 상태
            if (GameFlowStateMachine.Instance != null)
            {
                Debug.Log($"현재 상태: {GameFlowStateMachine.Instance.CurrentStateId}");
            }
            else
            {
                Debug.LogWarning("GameFlowStateMachine이 없습니다!");
            }

            // GameManager 상태
            if (GameManager.Instance != null)
            {
                Debug.Log($"골드: {GameManager.Instance.CurrentGold}");
                Debug.Log($"스테이지: {GameManager.Instance.CurrentStage}");
            }

            // RoomManager 상태
            if (RoomManager.Instance != null)
            {
                Debug.Log($"현재 방: {RoomManager.Instance.CurrentRoom?.name ?? "None"}");
                Debug.Log($"총 방 개수: {RoomManager.Instance.TotalRoomCount}");
            }

            // PlayerStats 상태
            var playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log($"플레이어 HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
                Debug.Log($"플레이어 사망: {playerStats.IsDead}");
            }

            Debug.Log("======================================");
        }

        [ContextMenu("2. 던전 입장 테스트 (F2)")]
        private void TestEnterDungeon()
        {
            Debug.Log("[Test] 던전 입장 테스트");

            if (GameFlowStateMachine.Instance != null)
            {
                GameFlowStateMachine.Instance.TriggerEnterDungeon();
            }
            else
            {
                Debug.LogError("[Test] GameFlowStateMachine이 없습니다!");
            }
        }

        [ContextMenu("3. 방 클리어 테스트 (F3)")]
        private void TestRoomClear()
        {
            Debug.Log("[Test] 방 클리어 테스트");

            if (RoomManager.Instance != null && RoomManager.Instance.CurrentRoom != null)
            {
                // 강제로 방 클리어 이벤트 발생
                var room = RoomManager.Instance.CurrentRoom;

                // Room의 private 메서드 호출 대신 반사 사용
                var clearMethod = typeof(Room).GetMethod("ClearRoom",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (clearMethod != null)
                {
                    clearMethod.Invoke(room, null);
                }
                else
                {
                    Debug.LogWarning("[Test] Room.ClearRoom 메서드를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[Test] 현재 방이 없습니다!");
            }
        }

        [ContextMenu("4. 다음 방 이동 테스트 (F4)")]
        private void TestNextRoom()
        {
            Debug.Log("[Test] 다음 방 이동 테스트");

            if (GameFlowStateMachine.Instance != null)
            {
                GameFlowStateMachine.Instance.TriggerEnterNextRoom();
            }
            else
            {
                Debug.LogError("[Test] GameFlowStateMachine이 없습니다!");
            }
        }

        [ContextMenu("5. 플레이어 사망 테스트 (F5)")]
        private void TestPlayerDeath()
        {
            Debug.Log("[Test] 플레이어 사망 테스트");

            var playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                // 현재 HP만큼 데미지 (확실한 사망)
                playerStats.TakeDamage(playerStats.CurrentHP + 100);
            }
            else
            {
                Debug.LogWarning("[Test] PlayerStats를 찾을 수 없습니다!");
            }
        }

        [ContextMenu("6. 적 전멸 테스트 (F6)")]
        private void TestDungeonClear()
        {
            Debug.Log("[Test] 적 전멸 테스트 (DungeonReward로 전환)");

            if (GameFlowStateMachine.Instance != null)
            {
                GameFlowStateMachine.Instance.TriggerEnemiesCleared();
            }
            else
            {
                Debug.LogError("[Test] GameFlowStateMachine이 없습니다!");
            }
        }

        [ContextMenu("7. 보상 스폰 테스트 (F7)")]
        private void TestRewardSpawn()
        {
            Debug.Log("[Test] 보상 스폰 테스트");

            if (RewardSpawner.HasInstance)
            {
                Vector3 spawnPos = Camera.main != null
                    ? Camera.main.transform.position + Vector3.forward * 5f
                    : Vector3.zero;
                spawnPos.z = 0;

                // Awaitable은 fire-and-forget으로 discard 연산자 사용
                _ = RewardSpawner.Instance.SpawnRoomRewardsAsync(spawnPos, 1);
            }
            else
            {
                Debug.LogWarning("[Test] RewardSpawner가 없습니다!");
            }
        }

        [ContextMenu("8. 골드 추가 테스트 (F8)")]
        private void TestAddGold()
        {
            Debug.Log("[Test] 골드 100 추가");

            if (CurrencySystem.HasInstance)
            {
                CurrencySystem.Instance.AddGold(100);
            }
            else
            {
                Debug.LogWarning("[Test] CurrencySystem이 없습니다!");
            }
        }

        [ContextMenu("9. 플레이어 회복 테스트 (F9)")]
        private void TestPlayerHeal()
        {
            Debug.Log("[Test] 플레이어 50 HP 회복");

            var playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Heal(50);
            }
            else
            {
                Debug.LogWarning("[Test] PlayerStats를 찾을 수 없습니다!");
            }
        }

        [ContextMenu("10. StartRoom 복귀 테스트 (F10)")]
        private void TestReturnToStart()
        {
            Debug.Log("[Test] StartRoom 복귀 테스트");

            if (GameFlowStateMachine.Instance != null)
            {
                GameFlowStateMachine.Instance.TriggerReturnToStart();
            }
            else
            {
                Debug.LogError("[Test] GameFlowStateMachine이 없습니다!");
            }
        }


        // ====== 자동 테스트 ======

        private void RunAutoTest()
        {
            Debug.Log("[AutoTest] 자동 테스트 실행");

            // 현재 상태에 따라 다음 액션 결정
            if (GameFlowStateMachine.Instance == null) return;

            string currentState = GameFlowStateMachine.Instance.CurrentStateId;

            switch (currentState)
            {
                case "StartRoom":
                    Debug.Log("[AutoTest] StartRoom → 던전 입장");
                    TestEnterDungeon();
                    break;

                case "DungeonCombat":
                    Debug.Log("[AutoTest] DungeonCombat → 방 클리어");
                    TestRoomClear();
                    break;

                case "DungeonReward":
                    Debug.Log("[AutoTest] DungeonReward → 다음 방 이동");
                    TestNextRoom();
                    break;

                default:
                    Debug.Log($"[AutoTest] {currentState} 상태 - 대기 중");
                    break;
            }
        }


        // ====== UI ======

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== 게임 플로우 테스트 ===");
            GUILayout.Label($"현재 상태: {GameFlowStateMachine.Instance?.CurrentStateId ?? "None"}");
            GUILayout.Space(10);
            GUILayout.Label("F1: 상태 출력");
            GUILayout.Label("F2: 던전 입장");
            GUILayout.Label("F3: 방 클리어");
            GUILayout.Label("F4: 다음 방");
            GUILayout.Label("F5: 플레이어 사망");
            GUILayout.Label("F6: 적 전멸");
            GUILayout.Label("F7: 보상 스폰");
            GUILayout.Label("F8: 골드 +100");
            GUILayout.Label("F9: HP +50");
            GUILayout.Label("F10: StartRoom 복귀");
            GUILayout.EndArea();
        }
    }
}
