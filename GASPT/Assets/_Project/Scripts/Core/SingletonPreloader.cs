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
using GASPT.Core.SceneManagement;

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

            // 0-1. AdditiveSceneLoader (씬 관리 - 다른 시스템보다 먼저)
            PreloadAdditiveSceneLoader();

            // 0-2. Object Pooling (게임플레이 최적화)
            PreloadPoolManager();

            // 1. UI Systems (게임플레이 중 자주 사용)
            PreloadFadeController();
            PreloadDamageNumberPool();

            // 2. Economy Systems
            PreloadCurrencySystem();
            PreloadInventorySystem();

            // 2-1. Shop System
            PreloadShopSystem();

            // 3. Level System
            PreloadPlayerLevel();
            // RoomManager는 Scene별로 관리 (DontDestroyOnLoad 불필요)
            // PreloadRoomManager();

            // 4. Save System
            PreloadSaveSystem();
            PreloadSaveManager();

            // 5. StatusEffect System
            PreloadStatusEffectManager();

            // 6. Skill System
            PreloadSkillSystem();

            // 7. Loot System
            PreloadLootSystem();

            // 8. Skill Item System (LootSystem 의존)
            PreloadSkillItemManager();

            // 9. RunManager (런 데이터 관리 - GameManager 이전에 초기화)
            PreloadRunManager();

            // 10. GameManager (최종 - 모든 시스템 참조 허브)
            PreloadGameManager();

            // 11. UIManager (씬 로드 후 UI 참조 필요 - 지연 초기화)
            // Note: UIManager는 씬에 배치된 UI를 참조해야 하므로
            // BeforeSceneLoad 시점에서는 완전한 초기화가 불가능합니다.
            // 씬 로드 후 자동으로 Instance 접근 시 초기화됩니다.
            PreloadUIManager();

            // 12. GameFlowStateMachine (게임 Flow FSM - GameManager 의존)
            PreloadGameFlowStateMachine();

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
        /// AdditiveSceneLoader 사전 로딩 (씬 관리 시스템)
        /// </summary>
        private void PreloadAdditiveSceneLoader()
        {
            LogMessage("AdditiveSceneLoader 초기화 중...");

            var instance = AdditiveSceneLoader.Instance;

            if (instance != null)
            {
                LogMessage("✓ AdditiveSceneLoader 초기화 완료");
            }
            else
            {
                LogError("✗ AdditiveSceneLoader 초기화 실패");
            }
        }

        /// <summary>
        /// FadeController 사전 로딩
        /// </summary>
        private void PreloadFadeController()
        {
            LogMessage("FadeController 초기화 중...");

            var instance = FadeController.Instance;

            if (instance != null)
            {
                LogMessage("✓ FadeController 초기화 완료");
            }
            else
            {
                LogError("✗ FadeController 초기화 실패");
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
        /// ShopSystem 사전 로딩
        /// </summary>
        private void PreloadShopSystem()
        {
            LogMessage("ShopSystem 초기화 중...");

            var instance = GASPT.Shop.ShopSystem.Instance;

            if (instance != null)
            {
                LogMessage("✓ ShopSystem 초기화 완료");
            }
            else
            {
                LogError("✗ ShopSystem 초기화 실패");
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
        /// SaveManager 사전 로딩
        /// </summary>
        private void PreloadSaveManager()
        {
            LogMessage("SaveManager 초기화 중...");

            var instance = SaveManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ SaveManager 초기화 완료");
            }
            else
            {
                LogError("✗ SaveManager 초기화 실패");
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
        /// RunManager 사전 로딩 (런 데이터 관리)
        /// </summary>
        private void PreloadRunManager()
        {
            LogMessage("RunManager 초기화 중...");

            var instance = RunManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ RunManager 초기화 완료");
            }
            else
            {
                LogError("✗ RunManager 초기화 실패");
            }
        }

        /// <summary>
        /// GameManager 사전 로딩 (모든 시스템의 참조 허브)
        /// </summary>
        private void PreloadGameManager()
        {
            LogMessage("GameManager 초기화 중...");

            var instance = GameManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ GameManager 초기화 완료");
                LogMessage($"  - Bone: {instance.Meta?.Currency?.Bone ?? 0}");
                LogMessage($"  - Soul: {instance.Meta?.Currency?.Soul ?? 0}");
            }
            else
            {
                LogError("✗ GameManager 초기화 실패");
            }
        }

        /// <summary>
        /// UIManager 사전 로딩
        /// Note: 씬에 배치된 UI 참조가 필요하므로 BeforeSceneLoad에서는 완전 초기화 불가
        /// </summary>
        private void PreloadUIManager()
        {
            LogMessage("UIManager 초기화 중...");

            // UIManager는 씬에 배치된 UI를 참조해야 함
            // BeforeSceneLoad 시점에서는 씬이 아직 로드되지 않았으므로
            // Instance 접근만 하여 싱글톤 객체 생성
            var instance = UIManager.Instance;

            if (instance != null)
            {
                LogMessage("✓ UIManager 초기화 완료 (UI 참조는 씬 로드 후 설정됨)");
            }
            else
            {
                LogError("✗ UIManager 초기화 실패");
            }
        }

        /// <summary>
        /// GameFlowStateMachine 사전 로딩 (게임 Flow FSM)
        /// </summary>
        private void PreloadGameFlowStateMachine()
        {
            LogMessage("GameFlowStateMachine 초기화 중...");

            var instance = GameFlowStateMachine.Instance;

            if (instance != null)
            {
                LogMessage("✓ GameFlowStateMachine 초기화 완료");

                // 게임 자동 시작 (Initializing 상태로 진입)
                if (!instance.IsRunning)
                {
                    instance.StartGame();
                    LogMessage("✓ GameFlowStateMachine 자동 시작 (Initializing 상태)");
                }

                LogMessage($"  - FSM 상태: {instance.CurrentStateId}");
                LogMessage($"  - FSM 실행 중: {instance.IsRunning}");
            }
            else
            {
                LogError("✗ GameFlowStateMachine 초기화 실패");
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
            if (AdditiveSceneLoader.HasInstance) count++;
            if (PoolManager.HasInstance) count++;
            if (FadeController.HasInstance) count++;
            if (DamageNumberPool.HasInstance) count++;
            if (CurrencySystem.HasInstance) count++;
            if (InventorySystem.HasInstance) count++;
            if (GASPT.Shop.ShopSystem.HasInstance) count++;
            if (PlayerLevel.HasInstance) count++;
            // if (RoomManager.HasInstance) count++; // Scene별로 관리
            if (SaveSystem.HasInstance) count++;
            if (SaveManager.HasInstance) count++;
            if (StatusEffectManager.HasInstance) count++;
            if (SkillSystem.HasInstance) count++;
            if (LootSystem.HasInstance) count++;
            if (SkillItemManager.HasInstance) count++;
            if (RunManager.HasInstance) count++;
            if (GameManager.HasInstance) count++;
            if (UIManager.HasInstance) count++;
            if (GameFlowStateMachine.HasInstance) count++;

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
            Debug.Log($"AdditiveSceneLoader: {(AdditiveSceneLoader.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"PoolManager: {(PoolManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"FadeController: {(FadeController.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"DamageNumberPool: {(DamageNumberPool.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"CurrencySystem: {(CurrencySystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"InventorySystem: {(InventorySystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"ShopSystem: {(GASPT.Shop.ShopSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"PlayerLevel: {(PlayerLevel.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            // Debug.Log($"RoomManager: {(RoomManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}"); // Scene별로 관리
            Debug.Log($"SaveSystem: {(SaveSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"SaveManager: {(SaveManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"StatusEffectManager: {(StatusEffectManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"SkillSystem: {(SkillSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"LootSystem: {(LootSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"SkillItemManager: {(SkillItemManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"RunManager: {(RunManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"GameManager: {(GameManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"UIManager: {(UIManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"GameFlowStateMachine: {(GameFlowStateMachine.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"총 {GetPreloadedCount()}/19개 싱글톤 생성됨");
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
