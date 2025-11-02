using UnityEngine;
using GASPT.Stats;
using GASPT.Economy;
using GASPT.Inventory;

namespace GASPT.Save
{
    /// <summary>
    /// Save/Load 시스템 테스트 스크립트
    /// Context Menu를 통해 저장/불러오기 기능 테스트
    /// </summary>
    public class SaveTest : MonoBehaviour
    {
        // ====== 참조 ======

        [Header("시스템 참조 (자동 검색)")]
        [SerializeField] [Tooltip("플레이어 스탯")]
        private PlayerStats playerStats;

        [SerializeField] [Tooltip("저장 시스템")]
        private SaveSystem saveSystem;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            FindReferences();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }


        // ====== 참조 검색 ======

        private void FindReferences()
        {
            // PlayerStats 찾기
            if (playerStats == null)
            {
                playerStats = FindAnyObjectByType<PlayerStats>();

                if (playerStats == null)
                {
                    Debug.LogWarning("[SaveTest] PlayerStats를 찾을 수 없습니다.");
                }
            }

            // SaveSystem 찾기 (싱글톤)
            saveSystem = SaveSystem.Instance;

            if (saveSystem == null)
            {
                Debug.LogWarning("[SaveTest] SaveSystem을 찾을 수 없습니다.");
            }
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (saveSystem != null)
            {
                saveSystem.OnSaved += OnGameSaved;
                saveSystem.OnLoaded += OnGameLoaded;
                saveSystem.OnSaveFailed += OnSaveFailed;
                saveSystem.OnLoadFailed += OnLoadFailed;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (saveSystem != null)
            {
                saveSystem.OnSaved -= OnGameSaved;
                saveSystem.OnLoaded -= OnGameLoaded;
                saveSystem.OnSaveFailed -= OnSaveFailed;
                saveSystem.OnLoadFailed -= OnLoadFailed;
            }
        }


        // ====== 이벤트 핸들러 ======

        private void OnGameSaved(string filePath)
        {
            Debug.Log($"[SaveTest] 게임 저장 완료! 파일: {filePath}");
        }

        private void OnGameLoaded(string filePath)
        {
            Debug.Log($"[SaveTest] 게임 불러오기 완료! 파일: {filePath}");
        }

        private void OnSaveFailed(string error)
        {
            Debug.LogError($"[SaveTest] 저장 실패: {error}");
        }

        private void OnLoadFailed(string error)
        {
            Debug.LogError($"[SaveTest] 불러오기 실패: {error}");
        }


        // ====== Context Menu 테스트 ======

        [ContextMenu("1. Save Game")]
        private void TestSaveGame()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[SaveTest] SaveSystem이 없습니다.");
                return;
            }

            Debug.Log("\n========== Save Game Test ==========");

            // 현재 상태 출력
            PrintCurrentGameState();

            // 저장 실행
            saveSystem.SaveGame();

            Debug.Log("====================================\n");
        }

        [ContextMenu("2. Load Game")]
        private void TestLoadGame()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[SaveTest] SaveSystem이 없습니다.");
                return;
            }

            Debug.Log("\n========== Load Game Test ==========");

            // 불러오기 전 상태
            Debug.Log("불러오기 전:");
            PrintCurrentGameState();

            // 불러오기 실행
            saveSystem.LoadGame();

            // 불러오기 후 상태
            Debug.Log("\n불러오기 후:");
            PrintCurrentGameState();

            Debug.Log("====================================\n");
        }

        [ContextMenu("3. Delete Save File")]
        private void TestDeleteSaveFile()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[SaveTest] SaveSystem이 없습니다.");
                return;
            }

            Debug.Log("\n========== Delete Save File Test ==========");

            saveSystem.DeleteSaveFile();

            Debug.Log("===========================================\n");
        }

        [ContextMenu("4. Print Save File Info")]
        private void TestPrintSaveFileInfo()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[SaveTest] SaveSystem이 없습니다.");
                return;
            }

            Debug.Log("\n========== Save File Info ==========");
            Debug.Log(saveSystem.GetSaveFileInfo());
            Debug.Log("====================================\n");
        }

        [ContextMenu("5. Print Current Game State")]
        private void TestPrintCurrentGameState()
        {
            Debug.Log("\n========== Current Game State ==========");
            PrintCurrentGameState();
            Debug.Log("========================================\n");
        }

        [ContextMenu("6. Full Save/Load Test")]
        private void TestFullSaveLoad()
        {
            if (saveSystem == null)
            {
                Debug.LogError("[SaveTest] SaveSystem이 없습니다.");
                return;
            }

            Debug.Log("\n========== Full Save/Load Test ==========");

            // 1. 저장 전 상태 출력
            Debug.Log("1. 저장 전 상태:");
            PrintCurrentGameState();

            // 2. 저장
            Debug.Log("\n2. 저장 중...");
            saveSystem.SaveGame();

            // 3. 일부 데이터 변경
            Debug.Log("\n3. 데이터 변경...");
            ModifyGameState();

            // 4. 변경 후 상태
            Debug.Log("\n4. 변경 후 상태:");
            PrintCurrentGameState();

            // 5. 불러오기
            Debug.Log("\n5. 불러오기 중...");
            saveSystem.LoadGame();

            // 6. 불러오기 후 상태 (원래대로 복원되어야 함)
            Debug.Log("\n6. 복원된 상태:");
            PrintCurrentGameState();

            Debug.Log("=========================================\n");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 현재 게임 상태 출력
        /// </summary>
        private void PrintCurrentGameState()
        {
            // PlayerStats
            if (playerStats != null)
            {
                Debug.Log($"  PlayerStats: HP={playerStats.CurrentHP}/{playerStats.MaxHP}, " +
                          $"Attack={playerStats.Attack}, Defense={playerStats.Defense}");
            }
            else
            {
                Debug.Log("  PlayerStats: 없음");
            }

            // CurrencySystem
            CurrencySystem currencySystem = CurrencySystem.Instance;
            if (currencySystem != null)
            {
                Debug.Log($"  CurrencySystem: Gold={currencySystem.Gold}");
            }
            else
            {
                Debug.Log("  CurrencySystem: 없음");
            }

            // InventorySystem
            InventorySystem inventorySystem = InventorySystem.Instance;
            if (inventorySystem != null)
            {
                Debug.Log($"  InventorySystem: Items={inventorySystem.ItemCount}");
            }
            else
            {
                Debug.Log("  InventorySystem: 없음");
            }
        }

        /// <summary>
        /// 게임 상태 임시 변경 (테스트용)
        /// </summary>
        private void ModifyGameState()
        {
            // HP 감소
            if (playerStats != null)
            {
                playerStats.TakeDamage(20);
                Debug.Log("  - PlayerStats HP 20 감소");
            }

            // 골드 추가
            CurrencySystem currencySystem = CurrencySystem.Instance;
            if (currencySystem != null)
            {
                currencySystem.AddGold(50);
                Debug.Log("  - CurrencySystem 골드 50 추가");
            }

            // 인벤토리는 변경하지 않음 (ScriptableObject 필요)
        }
    }
}
