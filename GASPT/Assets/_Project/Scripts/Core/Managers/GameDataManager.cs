using UnityEngine;
using Core.Data;

namespace Core.Managers
{
    /// <summary>
    /// 게임 데이터 관리 싱글톤
    /// 런타임 플레이어 데이터 및 메타 프로그레션 데이터 관리
    /// DontDestroyOnLoad로 씬 전환 시에도 데이터 유지
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        private static GameDataManager instance;

        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static GameDataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // 씬에서 기존 인스턴스 찾기
                    instance = FindAnyObjectByType<GameDataManager>();

                    // 없으면 새로 생성
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameDataManager");
                        instance = go.AddComponent<GameDataManager>();
                    }
                }
                return instance;
            }
        }

        [Header("=== 플레이어 런타임 데이터 ===")]
        [SerializeField]
        [Tooltip("현재 플레이어 런타임 데이터")]
        private PlayerRuntimeData currentPlayerData;

        /// <summary>
        /// 현재 플레이어 런타임 데이터 프로퍼티
        /// </summary>
        public PlayerRuntimeData PlayerData
        {
            get
            {
                if (currentPlayerData == null)
                {
                    currentPlayerData = new PlayerRuntimeData();
                    currentPlayerData.ResetToDefault();
                }
                return currentPlayerData;
            }
        }

        [Header("=== 런 진행 상태 ===")]
        [Tooltip("현재 런이 진행 중인지 여부")]
        public bool isRunInProgress = false;

        [Tooltip("현재 런 시작 시간")]
        public float runStartTime = 0f;

        [Header("=== 디버그 ===")]
        [Tooltip("디버그 로그 출력")]
        public bool debugLog = true;

        private void Awake()
        {
            // 싱글톤 중복 방지
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // 초기 데이터 생성
            if (currentPlayerData == null)
            {
                currentPlayerData = new PlayerRuntimeData();
                currentPlayerData.ResetToDefault();
            }

            LogDebug("GameDataManager 초기화 완료");
        }

        private void OnDestroy()
        {
            // 인스턴스 정리
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            // 애플리케이션 종료 시 정리
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 새 런 시작
        /// </summary>
        public void StartNewRun()
        {
            currentPlayerData = new PlayerRuntimeData();
            currentPlayerData.ResetToDefault();

            isRunInProgress = true;
            runStartTime = Time.time;

            LogDebug("새 런 시작");
        }

        /// <summary>
        /// 런 종료 (클리어 또는 사망)
        /// </summary>
        public void EndRun(bool isVictory)
        {
            float runDuration = Time.time - runStartTime;
            isRunInProgress = false;

            LogDebug($"런 종료 - {(isVictory ? "승리" : "패배")} (소요 시간: {runDuration:F1}초)");

            // TODO: 메타 프로그레션 데이터 저장 (골드, 경험치 등)
            // TODO: 런 통계 저장
        }

        /// <summary>
        /// 런 일시정지
        /// </summary>
        public void PauseRun()
        {
            LogDebug("런 일시정지");
            // TODO: 일시정지 로직
        }

        /// <summary>
        /// 런 재개
        /// </summary>
        public void ResumeRun()
        {
            LogDebug("런 재개");
            // TODO: 재개 로직
        }

        /// <summary>
        /// 데이터 저장 (로컬 저장소)
        /// </summary>
        public void SaveData()
        {
            // TODO: PlayerPrefs 또는 JSON 파일로 저장
            LogDebug("데이터 저장");
        }

        /// <summary>
        /// 데이터 로드 (로컬 저장소)
        /// </summary>
        public void LoadData()
        {
            // TODO: PlayerPrefs 또는 JSON 파일에서 로드
            LogDebug("데이터 로드");
        }

        /// <summary>
        /// 모든 데이터 초기화 (개발용)
        /// </summary>
        public void ResetAllData()
        {
            currentPlayerData = new PlayerRuntimeData();
            currentPlayerData.ResetToDefault();
            isRunInProgress = false;
            runStartTime = 0f;

            LogDebug("모든 데이터 초기화");
        }

        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugLog)
            {
                Debug.Log($"[GameDataManager] {message}");
            }
        }

        /// <summary>
        /// 에디터용: 현재 데이터 상태 출력
        /// </summary>
        [ContextMenu("Print Current Data")]
        private void PrintCurrentData()
        {
            Debug.Log("=== GameDataManager 현재 상태 ===");
            Debug.Log($"런 진행 중: {isRunInProgress}");
            Debug.Log($"HP: {PlayerData.currentHP}/{PlayerData.maxHP}");
            Debug.Log($"MP: {PlayerData.currentMP}/{PlayerData.maxMP}");
            Debug.Log($"골드: {PlayerData.gold}");
            Debug.Log($"레벨: {PlayerData.level}");
            Debug.Log($"잠금 해제된 어빌리티 수: {PlayerData.unlockedAbilityIds.Count}");
            Debug.Log($"획득한 아이템 수: {PlayerData.acquiredItemIds.Count}");
        }

        /// <summary>
        /// 에디터용: 테스트 데이터 설정
        /// </summary>
        [ContextMenu("Set Test Data")]
        private void SetTestData()
        {
            PlayerData.gold = 1000;
            PlayerData.level = 5;
            PlayerData.attackPower = 25f;
            PlayerData.UnlockAbility("Combo_0");
            PlayerData.UnlockAbility("Combo_1");
            PlayerData.UnlockAbility("Combo_2");
            PlayerData.EquipAbility("Combo_0");
            PlayerData.EquipAbility("Combo_1");
            PlayerData.AcquireItem("TestItem_1");
            PlayerData.AcquireItem("TestItem_2");

            Debug.Log("테스트 데이터 설정 완료");
        }
    }
}
