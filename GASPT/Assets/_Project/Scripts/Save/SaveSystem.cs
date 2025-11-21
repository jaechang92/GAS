using System;
using System.IO;
using UnityEngine;
using GASPT.Stats;
using GASPT.Economy;
using GASPT.Inventory;
using System.Collections.Generic;

namespace GASPT.Save
{
    /// <summary>
    /// 게임 저장/불러오기 시스템 (싱글톤)
    /// JSON 기반 로컬 파일 저장
    /// </summary>
    public class SaveSystem : SingletonManager<SaveSystem>
    {


        // ====== 설정 ======

        /// <summary>
        /// 저장 파일명 (런 데이터)
        /// </summary>
        private const string SaveFileName = "gamesave.json";

        /// <summary>
        /// 메타 저장 파일명 (메타 진행 데이터)
        /// </summary>
        private const string MetaSaveFileName = "metasave.json";

        /// <summary>
        /// 저장 파일 전체 경로 (런 데이터)
        /// </summary>
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// 메타 저장 파일 전체 경로
        /// </summary>
        private string MetaSaveFilePath => Path.Combine(Application.persistentDataPath, MetaSaveFileName);


        // ====== 이벤트 ======

        /// <summary>
        /// 저장 완료 시 발생하는 이벤트
        /// 매개변수: (저장 파일 경로)
        /// </summary>
        public event Action<string> OnSaved;

        /// <summary>
        /// 불러오기 완료 시 발생하는 이벤트
        /// 매개변수: (저장 파일 경로)
        /// </summary>
        public event Action<string> OnLoaded;

        /// <summary>
        /// 저장 실패 시 발생하는 이벤트
        /// 매개변수: (에러 메시지)
        /// </summary>
        public event Action<string> OnSaveFailed;

        /// <summary>
        /// 불러오기 실패 시 발생하는 이벤트
        /// 매개변수: (에러 메시지)
        /// </summary>
        public event Action<string> OnLoadFailed;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            Debug.Log($"[SaveSystem] 초기화 완료. 저장 경로: {SaveFilePath}");
        }


        // ====== 저장 ======

        /// <summary>
        /// 현재 게임 상태를 저장합니다
        /// </summary>
        public void SaveGame()
        {
            try
            {
                // 1. 현재 데이터 수집
                GameSaveData saveData = CollectSaveData();

                // 2. JSON으로 직렬화
                string json = JsonUtility.ToJson(saveData, true);

                // 3. 파일에 쓰기
                File.WriteAllText(SaveFilePath, json);

                Debug.Log($"[SaveSystem] 게임 저장 완료: {SaveFilePath}");
                Debug.Log($"[SaveSystem] 저장 시간: {saveData.SaveTime}");

                // 이벤트 발생
                OnSaved?.Invoke(SaveFilePath);
            }
            catch (Exception e)
            {
                string errorMsg = $"저장 실패: {e.Message}";
                Debug.LogError($"[SaveSystem] {errorMsg}");
                OnSaveFailed?.Invoke(errorMsg);
            }
        }

        /// <summary>
        /// 현재 게임 데이터를 수집합니다
        /// </summary>
        private GameSaveData CollectSaveData()
        {
            GameSaveData saveData = new GameSaveData();

            // PlayerStats 데이터 수집 (RunManager/GameManager 우선)
            PlayerStats playerStats = null;
            if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                playerStats = GASPT.Core.RunManager.Instance.CurrentPlayer;
            else if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                playerStats = GASPT.Core.GameManager.Instance.PlayerStats;
            if (playerStats != null)
            {
                saveData.playerStats = playerStats.GetSaveData();
            }
            else
            {
                Debug.LogWarning("[SaveSystem] PlayerStats를 찾을 수 없습니다. 기본값으로 저장합니다.");
            }

            // CurrencySystem 데이터 수집
            CurrencySystem currencySystem = CurrencySystem.Instance;
            if (currencySystem != null)
            {
                saveData.currency = currencySystem.GetSaveData();
            }
            else
            {
                Debug.LogWarning("[SaveSystem] CurrencySystem을 찾을 수 없습니다. 기본값으로 저장합니다.");
            }

            // InventorySystem 데이터 수집
            InventorySystem inventorySystem = InventorySystem.Instance;
            if (inventorySystem != null)
            {
                saveData.inventory = inventorySystem.GetSaveData();
            }
            else
            {
                Debug.LogWarning("[SaveSystem] InventorySystem을 찾을 수 없습니다. 기본값으로 저장합니다.");
            }

            // 저장 시간 설정
            saveData.SaveTime = DateTime.Now;

            return saveData;
        }


        // ====== 불러오기 ======

        /// <summary>
        /// 저장된 게임을 불러옵니다
        /// </summary>
        public void LoadGame()
        {
            try
            {
                // 1. 파일 존재 확인
                if (!File.Exists(SaveFilePath))
                {
                    string errorMsg = "저장 파일이 존재하지 않습니다.";
                    Debug.LogWarning($"[SaveSystem] {errorMsg}: {SaveFilePath}");
                    OnLoadFailed?.Invoke(errorMsg);
                    return;
                }

                // 2. 파일에서 읽기
                string json = File.ReadAllText(SaveFilePath);

                // 3. JSON 역직렬화
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

                if (saveData == null)
                {
                    throw new Exception("JSON 역직렬화 실패 (saveData == null)");
                }

                // 4. 데이터 적용
                ApplySaveData(saveData);

                Debug.Log($"[SaveSystem] 게임 불러오기 완료: {SaveFilePath}");
                Debug.Log($"[SaveSystem] 저장 시간: {saveData.SaveTime}");

                // 이벤트 발생
                OnLoaded?.Invoke(SaveFilePath);
            }
            catch (Exception e)
            {
                string errorMsg = $"불러오기 실패: {e.Message}";
                Debug.LogError($"[SaveSystem] {errorMsg}");
                OnLoadFailed?.Invoke(errorMsg);
            }
        }

        /// <summary>
        /// 저장된 데이터를 게임에 적용합니다
        /// </summary>
        private void ApplySaveData(GameSaveData saveData)
        {
            // PlayerStats 데이터 적용 (RunManager/GameManager 우선)
            PlayerStats playerStats = null;
            if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                playerStats = GASPT.Core.RunManager.Instance.CurrentPlayer;
            else if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                playerStats = GASPT.Core.GameManager.Instance.PlayerStats;
            if (playerStats != null && saveData.playerStats != null)
            {
                playerStats.LoadFromSaveData(saveData.playerStats);
            }
            else
            {
                Debug.LogWarning("[SaveSystem] PlayerStats를 찾을 수 없거나 데이터가 null입니다.");
            }

            // CurrencySystem 데이터 적용
            CurrencySystem currencySystem = CurrencySystem.Instance;
            if (currencySystem != null && saveData.currency != null)
            {
                currencySystem.LoadFromSaveData(saveData.currency);
            }
            else
            {
                Debug.LogWarning("[SaveSystem] CurrencySystem을 찾을 수 없거나 데이터가 null입니다.");
            }

            // InventorySystem 데이터 적용
            InventorySystem inventorySystem = InventorySystem.Instance;
            if (inventorySystem != null && saveData.inventory != null)
            {
                inventorySystem.LoadFromSaveData(saveData.inventory);
            }
            else
            {
                Debug.LogWarning("[SaveSystem] InventorySystem을 찾을 수 없거나 데이터가 null입니다.");
            }
        }


        // ====== 저장 파일 관리 ======

        /// <summary>
        /// 저장 파일이 존재하는지 확인합니다
        /// </summary>
        public bool SaveFileExists()
        {
            return File.Exists(SaveFilePath);
        }

        /// <summary>
        /// 저장 파일을 삭제합니다
        /// </summary>
        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log($"[SaveSystem] 저장 파일 삭제 완료: {SaveFilePath}");
                }
                else
                {
                    Debug.LogWarning($"[SaveSystem] 삭제할 저장 파일이 존재하지 않습니다: {SaveFilePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 저장 파일 삭제 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 저장 파일 정보를 반환합니다
        /// </summary>
        public string GetSaveFileInfo()
        {
            if (!SaveFileExists())
            {
                return "저장 파일이 존재하지 않습니다.";
            }

            try
            {
                FileInfo fileInfo = new FileInfo(SaveFilePath);
                return $"파일 경로: {SaveFilePath}\n" +
                       $"파일 크기: {fileInfo.Length} bytes\n" +
                       $"수정 시간: {fileInfo.LastWriteTime}";
            }
            catch (Exception e)
            {
                return $"파일 정보 읽기 실패: {e.Message}";
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Save Game")]
        private void TestSaveGame()
        {
            SaveGame();
        }

        [ContextMenu("Load Game")]
        private void TestLoadGame()
        {
            LoadGame();
        }

        [ContextMenu("Delete Save File")]
        private void TestDeleteSaveFile()
        {
            DeleteSaveFile();
        }

        [ContextMenu("Print Save File Info")]
        private void TestPrintSaveFileInfo()
        {
            Debug.Log($"[SaveSystem] ========== Save File Info ==========");
            Debug.Log(GetSaveFileInfo());
            Debug.Log($"[SaveSystem] =========================================");
        }


        // ====== 메타 데이터 저장/로드 ======

        /// <summary>
        /// 메타 진행 데이터를 저장합니다 (메타 골드, 언락, 업적 등)
        /// </summary>
        public void SaveMetaData(MetaProgressionData metaData)
        {
            if (metaData == null)
            {
                Debug.LogError("[SaveSystem] SaveMetaData(): metaData가 null입니다.");
                return;
            }

            try
            {
                // JSON으로 직렬화
                string json = JsonUtility.ToJson(metaData, true);

                // 파일에 쓰기
                File.WriteAllText(MetaSaveFilePath, json);

                Debug.Log($"[SaveSystem] 메타 데이터 저장 완료: {MetaSaveFilePath}");
                Debug.Log($"[SaveSystem] 메타 골드: {metaData.totalGold}, Form: {metaData.unlockedForms.Count}개");
            }
            catch (Exception e)
            {
                string errorMsg = $"메타 데이터 저장 실패: {e.Message}";
                Debug.LogError($"[SaveSystem] {errorMsg}");
            }
        }

        /// <summary>
        /// 메타 진행 데이터를 불러옵니다
        /// </summary>
        public MetaProgressionData LoadMetaData()
        {
            try
            {
                // 파일 존재 확인
                if (!File.Exists(MetaSaveFilePath))
                {
                    Debug.Log("[SaveSystem] 메타 저장 파일이 없습니다. 새로운 메타 데이터를 반환합니다.");
                    return new MetaProgressionData();
                }

                // 파일에서 읽기
                string json = File.ReadAllText(MetaSaveFilePath);

                // JSON 역직렬화
                MetaProgressionData metaData = JsonUtility.FromJson<MetaProgressionData>(json);

                if (metaData == null)
                {
                    throw new Exception("JSON 역직렬화 실패 (metaData == null)");
                }

                Debug.Log($"[SaveSystem] 메타 데이터 불러오기 완료: {MetaSaveFilePath}");
                Debug.Log($"[SaveSystem] 메타 골드: {metaData.totalGold}, Form: {metaData.unlockedForms.Count}개");

                return metaData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 메타 데이터 불러오기 실패: {e.Message}");
                return new MetaProgressionData();
            }
        }

        /// <summary>
        /// 메타 저장 파일이 존재하는지 확인합니다
        /// </summary>
        public bool MetaSaveFileExists()
        {
            return File.Exists(MetaSaveFilePath);
        }

        /// <summary>
        /// 메타 저장 파일을 삭제합니다
        /// </summary>
        public void DeleteMetaSaveFile()
        {
            try
            {
                if (File.Exists(MetaSaveFilePath))
                {
                    File.Delete(MetaSaveFilePath);
                    Debug.Log($"[SaveSystem] 메타 저장 파일 삭제 완료: {MetaSaveFilePath}");
                }
                else
                {
                    Debug.LogWarning($"[SaveSystem] 삭제할 메타 저장 파일이 존재하지 않습니다: {MetaSaveFilePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] 메타 저장 파일 삭제 실패: {e.Message}");
            }
        }


        // ====== Context Menu (메타 데이터 테스트용) ======

        [ContextMenu("Save Meta Data")]
        private void TestSaveMetaData()
        {
            // 테스트용 더미 메타 데이터 생성
            MetaProgressionData testData = new MetaProgressionData
            {
                totalGold = 5000,
                unlockedForms = new List<string> { "MageForm", "WarriorForm" },
                metaUpgrades = new List<MetaUpgradeEntry>
                {
                    new MetaUpgradeEntry("MaxHP", 3),
                    new MetaUpgradeEntry("Attack", 2)
                }
            };

            SaveMetaData(testData);
        }

        [ContextMenu("Load Meta Data")]
        private void TestLoadMetaData()
        {
            MetaProgressionData metaData = LoadMetaData();
            Debug.Log($"[SaveSystem] 불러온 메타 골드: {metaData.totalGold}");
        }

        [ContextMenu("Delete Meta Save File")]
        private void TestDeleteMetaSaveFile()
        {
            DeleteMetaSaveFile();
        }
    }
}
