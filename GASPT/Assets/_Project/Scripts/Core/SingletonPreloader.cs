using UnityEngine;
using GASPT.UI;
using GASPT.Economy;
using GASPT.Inventory;
using GASPT.Level;
using GASPT.Gameplay.Level;
using GASPT.Save;
using GASPT.StatusEffects;
using GASPT.ResourceManagement;
using GASPT.Skills;
using GASPT.Loot;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Item;

namespace GASPT.Core
{
    /// <summary>
    /// 싱글톤 사전 로딩 시스템
    /// Loading 씬에서 게임에 필요한 모든 싱글톤을 미리 생성
    /// 게임플레이 중 지연 방지 및 메모리 할당 최적화
    /// </summary>
    public class SingletonPreloader : MonoBehaviour
    {
        private static bool isInitialized = false;

        [Header("Preload Settings")]
        [SerializeField] [Tooltip("싱글톤 초기화 로그 출력 여부")]
        private bool showDebugLogs = true;

        [SerializeField] [Tooltip("초기화 지연 시간 (초) - 로딩 화면 표시용")]
        private float delayBetweenInit = 0.1f;


        // ====== 자동 초기화 ======
        // Play 모드 진입 시 자동으로 모든 싱글톤 초기화

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (isInitialized)
                return;

            Debug.Log("[SingletonPreloader] 자동 초기화 시작...");

            // GameObject 생성 및 컴포넌트 추가
            GameObject preloaderObj = new GameObject("SingletonPreloader");
            SingletonPreloader preloader = preloaderObj.AddComponent<SingletonPreloader>();
            preloader.showDebugLogs = true;

            // DontDestroyOnLoad로 씬 전환 시에도 유지
            DontDestroyOnLoad(preloaderObj);

            // 즉시 초기화
            preloader.PreloadAllSingletons();

            isInitialized = true;
            Debug.Log("[SingletonPreloader] 자동 초기화 완료");
        }


        // ====== 초기화 순서 ======
        // 의존성이 있는 경우 순서대로 초기화

        private void Start()
        {
            // RuntimeInitializeOnLoadMethod로 이미 초기화됨
            if (isInitialized)
                return;

            LogMessage("========== 싱글톤 사전 로딩 시작 ==========");
            PreloadAllSingletons();
            LogMessage("========== 싱글톤 사전 로딩 완료 ==========");

            isInitialized = true;
        }


        // ====== 사전 로딩 ======

        /// <summary>
        /// 모든 필수 싱글톤 사전 로딩
        /// </summary>
        public void PreloadAllSingletons()
        {
            // 0. Resource Management (최우선 - 다른 시스템들이 의존)
            PreloadGameResourceManager();

            // 0-1. Object Pooling (게임플레이 최적화)
            PreloadPoolManager();

            // 1. UI Systems (게임플레이 중 자주 사용)
            PreloadDamageNumberPool();

            // 2. Economy Systems
            PreloadCurrencySystem();
            PreloadInventorySystem();

            // 3. Level System
            PreloadPlayerLevel();
            // RoomManager는 Scene별로 관리 (DontDestroyOnLoad 불필요)
            // PreloadRoomManager();

            // 4. Save System
            PreloadSaveSystem();

            // 5. StatusEffect System
            PreloadStatusEffectManager();

            // 6. Skill System
            PreloadSkillSystem();

            // 7. Loot System
            PreloadLootSystem();

            // 8. Skill Item System (LootSystem 의존)
            PreloadSkillItemManager();

            // Note: Pool 초기화는 PoolInitializer.cs에서 자동으로 처리됩니다
            // (RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)로 자동 실행)

            LogMessage($"총 {GetPreloadedCount()}개의 싱글톤 사전 로딩 완료");
        }

        /// <summary>
        /// GameResourceManager 사전 로딩
        /// </summary>
        private void PreloadGameResourceManager()
        {
            LogMessage("GameResourceManager 초기화 중...");

            var instance = GameResourceManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ GameResourceManager 초기화 완료");
            }
            else
            {
                LogError("✗ GameResourceManager 초기화 실패");
            }
        }

        /// <summary>
        /// DamageNumberPool 사전 로딩
        /// </summary>
        private void PreloadDamageNumberPool()
        {
            LogMessage("DamageNumberPool 초기화 중...");

            var instance = DamageNumberPool.Instance;

            if (instance != null)
            {
                LogMessage("✓ DamageNumberPool 초기화 완료");
            }
            else
            {
                LogError("✗ DamageNumberPool 초기화 실패");
            }
        }

        /// <summary>
        /// CurrencySystem 사전 로딩
        /// </summary>
        private void PreloadCurrencySystem()
        {
            LogMessage("CurrencySystem 초기화 중...");

            var instance = CurrencySystem.Instance;

            if (instance != null)
            {
                LogMessage("✓ CurrencySystem 초기화 완료");
            }
            else
            {
                LogError("✗ CurrencySystem 초기화 실패");
            }
        }

        /// <summary>
        /// InventorySystem 사전 로딩
        /// </summary>
        private void PreloadInventorySystem()
        {
            LogMessage("InventorySystem 초기화 중...");

            var instance = InventorySystem.Instance;

            if (instance != null)
            {
                LogMessage("✓ InventorySystem 초기화 완료");
            }
            else
            {
                LogError("✗ InventorySystem 초기화 실패");
            }
        }

        /// <summary>
        /// PlayerLevel 사전 로딩
        /// </summary>
        private void PreloadPlayerLevel()
        {
            LogMessage("PlayerLevel 초기화 중...");

            var instance = PlayerLevel.Instance;

            if (instance != null)
            {
                LogMessage("✓ PlayerLevel 초기화 완료");
            }
            else
            {
                LogError("✗ PlayerLevel 초기화 실패");
            }
        }

        /// <summary>
        /// RoomManager 사전 로딩
        /// </summary>
        private void PreloadRoomManager()
        {
            LogMessage("RoomManager 초기화 중...");

            var instance = RoomManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ RoomManager 초기화 완료");
            }
            else
            {
                LogError("✗ RoomManager 초기화 실패");
            }
        }

        /// <summary>
        /// SaveSystem 사전 로딩
        /// </summary>
        private void PreloadSaveSystem()
        {
            LogMessage("SaveSystem 초기화 중...");

            var instance = SaveSystem.Instance;

            if (instance != null)
            {
                LogMessage("✓ SaveSystem 초기화 완료");
            }
            else
            {
                LogError("✗ SaveSystem 초기화 실패");
            }
        }

        /// <summary>
        /// StatusEffectManager 사전 로딩
        /// </summary>
        private void PreloadStatusEffectManager()
        {
            LogMessage("StatusEffectManager 초기화 중...");

            var instance = StatusEffectManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ StatusEffectManager 초기화 완료");
            }
            else
            {
                LogError("✗ StatusEffectManager 초기화 실패");
            }
        }

        /// <summary>
        /// SkillSystem 사전 로딩
        /// </summary>
        private void PreloadSkillSystem()
        {
            LogMessage("SkillSystem 초기화 중...");

            var instance = SkillSystem.Instance;

            if (instance != null)
            {
                LogMessage("✓ SkillSystem 초기화 완료");
            }
            else
            {
                LogError("✗ SkillSystem 초기화 실패");
            }
        }

        /// <summary>
        /// LootSystem 사전 로딩
        /// </summary>
        private void PreloadLootSystem()
        {
            LogMessage("LootSystem 초기화 중...");

            var instance = LootSystem.Instance;

            if (instance != null)
            {
                LogMessage("✓ LootSystem 초기화 완료");
            }
            else
            {
                LogError("✗ LootSystem 초기화 실패");
            }
        }

        /// <summary>
        /// SkillItemManager 사전 로딩
        /// </summary>
        private void PreloadSkillItemManager()
        {
            LogMessage("SkillItemManager 초기화 중...");

            var instance = SkillItemManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ SkillItemManager 초기화 완료");
            }
            else
            {
                LogError("✗ SkillItemManager 초기화 실패");
            }
        }

        /// <summary>
        /// PoolManager 사전 로딩
        /// </summary>
        private void PreloadPoolManager()
        {
            LogMessage("PoolManager 초기화 중...");

            var instance = PoolManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ PoolManager 초기화 완료");
            }
            else
            {
                LogError("✗ PoolManager 초기화 실패");
            }
        }



        // ====== 유틸리티 ======

        /// <summary>
        /// 사전 로딩된 싱글톤 개수 확인
        /// </summary>
        private int GetPreloadedCount()
        {
            int count = 0;

            if (GameResourceManager.HasInstance) count++;
            if (PoolManager.HasInstance) count++;
            if (DamageNumberPool.HasInstance) count++;
            if (CurrencySystem.HasInstance) count++;
            if (InventorySystem.HasInstance) count++;
            if (PlayerLevel.HasInstance) count++;
            // if (RoomManager.HasInstance) count++; // Scene별로 관리
            if (SaveSystem.HasInstance) count++;
            if (StatusEffectManager.HasInstance) count++;
            if (SkillSystem.HasInstance) count++;
            if (LootSystem.HasInstance) count++;
            if (SkillItemManager.HasInstance) count++;

            return count;
        }

        /// <summary>
        /// 로그 메시지 출력
        /// </summary>
        private void LogMessage(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SingletonPreloader] {message}");
            }
        }

        /// <summary>
        /// 에러 로그 출력
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[SingletonPreloader] {message}");
        }


        // ====== Context Menu (테스트용) ======

        /// <summary>
        /// 싱글톤 상태 확인
        /// </summary>
        [ContextMenu("Check Singleton Status")]
        private void CheckSingletonStatus()
        {
            Debug.Log("========== 싱글톤 상태 확인 ==========");
            Debug.Log($"GameResourceManager: {(GameResourceManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"PoolManager: {(PoolManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"DamageNumberPool: {(DamageNumberPool.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"CurrencySystem: {(CurrencySystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"InventorySystem: {(InventorySystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"PlayerLevel: {(PlayerLevel.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            // Debug.Log($"RoomManager: {(RoomManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}"); // Scene별로 관리
            Debug.Log($"SaveSystem: {(SaveSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"StatusEffectManager: {(StatusEffectManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"SkillSystem: {(SkillSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"LootSystem: {(LootSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"SkillItemManager: {(SkillItemManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"총 {GetPreloadedCount()}/11개 싱글톤 생성됨");
            Debug.Log("=====================================");
        }

        /// <summary>
        /// 강제 재로딩
        /// </summary>
        [ContextMenu("Force Reload All")]
        private void ForceReloadAll()
        {
            LogMessage("강제 재로딩 시작...");
            PreloadAllSingletons();
            CheckSingletonStatus();
        }
    }
}
