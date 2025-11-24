using UnityEngine;
using GASPT.Core;
using GASPT.Data;
using System;
using GASPT.Stats;
using GASPT.Economy;
using GASPT.Inventory;

namespace GASPT.Save
{
    /// <summary>
    /// 저장 데이터 수집/복원 관리자
    /// 단일 책임: ISaveable 구현 시스템들의 데이터 수집 및 복원만 담당
    /// 파일 I/O는 SaveSystem이 담당
    /// </summary>
    public class SaveManager : SingletonManager<SaveManager>
    {
        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            Debug.Log("[SaveManager] 초기화 완료");
        }


        // ====== 데이터 수집 (단일 책임) ======

        /// <summary>
        /// 모든 ISaveable 시스템으로부터 저장 데이터 수집
        /// </summary>
        /// <returns>수집된 게임 저장 데이터</returns>
        public GameSaveData CollectSaveData()
        {
            GameSaveData saveData = new GameSaveData();

            try
            {
                // PlayerStats 데이터 수집 (RunManager/GameManager 우선)
                PlayerStats playerStats = GetPlayerStats();
                if (playerStats != null)
                {
                    saveData.playerStats = playerStats.GetSaveData();
                    Debug.Log("[SaveManager] PlayerStats 데이터 수집 완료");
                }
                else
                {
                    Debug.LogWarning("[SaveManager] PlayerStats를 찾을 수 없습니다.");
                }

                // CurrencySystem 데이터 수집
                if (CurrencySystem.HasInstance)
                {
                    var currencyData = CurrencySystem.Instance.GetSaveData();
                    saveData.currency = currencyData;
                    Debug.Log("[SaveManager] CurrencySystem 데이터 수집 완료");
                }

                // InventorySystem 데이터 수집
                if (InventorySystem.HasInstance)
                {
                    var inventoryData = InventorySystem.Instance.GetSaveData();
                    saveData.inventory = inventoryData;
                    Debug.Log("[SaveManager] InventorySystem 데이터 수집 완료");
                }

                // 저장 시간 기록
                saveData.SaveTime = DateTime.Now;

                Debug.Log($"[SaveManager] 데이터 수집 완료: {saveData.SaveTime}");
                return saveData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 데이터 수집 중 오류 발생: {e.Message}");
                throw;
            }
        }


        // ====== 데이터 복원 (단일 책임) ======

        /// <summary>
        /// 저장된 데이터를 모든 ISaveable 시스템에 복원
        /// </summary>
        /// <param name="saveData">복원할 게임 저장 데이터</param>
        public void RestoreSaveData(GameSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogError("[SaveManager] 복원할 데이터가 null입니다.");
                return;
            }

            try
            {
                Debug.Log($"[SaveManager] 데이터 복원 시작: {saveData.SaveTime}");

                // PlayerStats 데이터 복원
                PlayerStats playerStats = GetPlayerStats();
                if (playerStats != null && saveData.playerStats != null)
                {
                    playerStats.LoadFromSaveData(saveData.playerStats);
                    Debug.Log("[SaveManager] PlayerStats 데이터 복원 완료");
                }

                // CurrencySystem 데이터 복원
                if (CurrencySystem.HasInstance && saveData.currency != null)
                {
                    CurrencySystem.Instance.LoadFromSaveData(saveData.currency);
                    Debug.Log("[SaveManager] CurrencySystem 데이터 복원 완료");
                }

                // InventorySystem 데이터 복원
                if (InventorySystem.HasInstance && saveData.inventory != null)
                {
                    InventorySystem.Instance.LoadFromSaveData(saveData.inventory);
                    Debug.Log("[SaveManager] InventorySystem 데이터 복원 완료");
                }

                Debug.Log("[SaveManager] 데이터 복원 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 데이터 복원 중 오류 발생: {e.Message}");
                throw;
            }
        }


        // ====== 헬퍼 메서드 (Private) ======

        /// <summary>
        /// PlayerStats 참조 획득 (RunManager 우선, GameManager 차선)
        /// </summary>
        private PlayerStats GetPlayerStats()
        {
            // RunManager 우선 (런 진행 중)
            if (RunManager.HasInstance && RunManager.Instance.CurrentPlayer != null)
            {
                return RunManager.Instance.CurrentPlayer;
            }

            // GameManager 차선 (메인 메뉴 등)
            if (GameManager.HasInstance && GameManager.Instance.PlayerStats != null)
            {
                return GameManager.Instance.PlayerStats;
            }

            return null;
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test: Collect Save Data")]
        private void TestCollectSaveData()
        {
            try
            {
                var data = CollectSaveData();
                Debug.Log("========== Collected Save Data ==========");
                Debug.Log($"Save Time: {data.SaveTime}");
                Debug.Log($"PlayerStats: {(data.playerStats != null ? "Collected" : "Null")}");
                Debug.Log($"Currency: {(data.currency != null ? "Collected" : "Null")}");
                Debug.Log($"Inventory: {(data.inventory != null ? "Collected" : "Null")}");
                Debug.Log("========================================");
            }
            catch (Exception e)
            {
                Debug.LogError($"Test failed: {e.Message}");
            }
        }
    }
}
