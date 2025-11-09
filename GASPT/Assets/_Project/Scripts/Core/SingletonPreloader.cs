using UnityEngine;
using GASPT.UI;
using GASPT.Economy;
using GASPT.Inventory;
using GASPT.Level;
using GASPT.Save;
using GASPT.StatusEffects;
using GASPT.ResourceManagement;
using GASPT.Skills;

namespace GASPT.Core
{
    /// <summary>
    /// 싱글톤 사전 로딩 시스템
    /// Loading 씬에서 게임에 필요한 모든 싱글톤을 미리 생성
    /// 게임플레이 중 지연 방지 및 메모리 할당 최적화
    /// </summary>
    public class SingletonPreloader : MonoBehaviour
    {
        [Header("Preload Settings")]
        [SerializeField] [Tooltip("싱글톤 초기화 로그 출력 여부")]
        private bool showDebugLogs = true;

        [SerializeField] [Tooltip("초기화 지연 시간 (초) - 로딩 화면 표시용")]
        private float delayBetweenInit = 0.1f;


        // ====== 초기화 순서 ======
        // 의존성이 있는 경우 순서대로 초기화

        private void Start()
        {
            LogMessage("========== 싱글톤 사전 로딩 시작 ==========");
            PreloadAllSingletons();
            LogMessage("========== 싱글톤 사전 로딩 완료 ==========");
        }


        // ====== 사전 로딩 ======

        /// <summary>
        /// 모든 필수 싱글톤 사전 로딩
        /// </summary>
        private void PreloadAllSingletons()
        {
            // 0. Resource Management (최우선 - 다른 시스템들이 의존)
            PreloadGameResourceManager();

            // 1. UI Systems (게임플레이 중 자주 사용)
            PreloadDamageNumberPool();

            // 2. Economy Systems
            PreloadCurrencySystem();
            PreloadInventorySystem();

            // 3. Level System
            PreloadPlayerLevel();

            // 4. Save System
            PreloadSaveSystem();

            // 5. StatusEffect System
            PreloadStatusEffectManager();

            // 6. Skill System
            PreloadSkillSystem();

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


        // ====== 유틸리티 ======

        /// <summary>
        /// 사전 로딩된 싱글톤 개수 확인
        /// </summary>
        private int GetPreloadedCount()
        {
            int count = 0;

            if (GameResourceManager.HasInstance) count++;
            if (DamageNumberPool.HasInstance) count++;
            if (CurrencySystem.HasInstance) count++;
            if (InventorySystem.HasInstance) count++;
            if (PlayerLevel.HasInstance) count++;
            if (SaveSystem.HasInstance) count++;
            if (StatusEffectManager.HasInstance) count++;
            if (SkillSystem.HasInstance) count++;

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
            Debug.Log($"DamageNumberPool: {(DamageNumberPool.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"CurrencySystem: {(CurrencySystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"InventorySystem: {(InventorySystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"PlayerLevel: {(PlayerLevel.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"SaveSystem: {(SaveSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"StatusEffectManager: {(StatusEffectManager.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"SkillSystem: {(SkillSystem.HasInstance ? "✓ 생성됨" : "✗ 미생성")}");
            Debug.Log($"총 {GetPreloadedCount()}/8개 싱글톤 생성됨");
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
