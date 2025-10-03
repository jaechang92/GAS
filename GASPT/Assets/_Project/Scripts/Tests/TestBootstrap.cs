using UnityEngine;
using System.Threading;
using Core.Managers;
using Core.Enums;

namespace Tests
{
    /// <summary>
    /// 테스트 씬용 경량 초기화 시스템
    /// 전체 게임 흐름(GameFlow) 없이 필요한 것들만 초기화
    /// </summary>
    public class TestBootstrap : MonoBehaviour
    {
        [Header("초기화 옵션")]
        [Tooltip("ResourceManager 초기화 여부")]
        [SerializeField] private bool initializeResourceManager = true;

        [Tooltip("AudioManager 초기화 여부")]
        [SerializeField] private bool initializeAudioManager = false;

        [Tooltip("UIManager 초기화 여부")]
        [SerializeField] private bool initializeUIManager = false;

        [Tooltip("GameManager 초기화 여부")]
        [SerializeField] private bool initializeGameManager = false;

        [Header("리소스 로딩 옵션")]
        [Tooltip("자동으로 로드할 카테고리들")]
        [SerializeField] private ResourceCategory[] categoriesToLoad = new ResourceCategory[0];

        [Tooltip("개별적으로 로드할 리소스 경로들")]
        [SerializeField] private string[] individualResources = new string[0];

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = true;

        // 초기화 완료 여부
        public bool IsInitialized { get; private set; }

        // 이벤트
        public event System.Action OnInitializationComplete;

        private async void Start()
        {
            await InitializeAsync(destroyCancellationToken);
        }

        /// <summary>
        /// 테스트 환경 초기화
        /// </summary>
        public async Awaitable InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
            {
                LogDebug("[TestBootstrap] 이미 초기화되었습니다.");
                return;
            }

            LogDebug("[TestBootstrap] 테스트 환경 초기화 시작...");

            // 1. 매니저 초기화
            await InitializeManagers(cancellationToken);

            // 2. 리소스 로딩
            await LoadResources(cancellationToken);

            IsInitialized = true;
            LogDebug("[TestBootstrap] 테스트 환경 초기화 완료!");

            OnInitializationComplete?.Invoke();
        }

        /// <summary>
        /// 매니저들 초기화
        /// </summary>
        private async Awaitable InitializeManagers(CancellationToken cancellationToken)
        {
            LogDebug("[TestBootstrap] 매니저 초기화 중...");

            if (initializeResourceManager)
            {
                var resourceManager = ResourceManager.Instance;
                LogDebug("[TestBootstrap] ResourceManager 초기화 완료");
            }

            if (initializeAudioManager)
            {
                var audioManager = AudioManager.Instance;
                LogDebug("[TestBootstrap] AudioManager 초기화 완료");
            }

            if (initializeUIManager)
            {
                var uiManager = UIManager.Instance;
                LogDebug("[TestBootstrap] UIManager 초기화 완료");
            }

            if (initializeGameManager)
            {
                var gameManager = GameManager.Instance;
                LogDebug("[TestBootstrap] GameManager 초기화 완료");
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 리소스 로딩
        /// </summary>
        private async Awaitable LoadResources(CancellationToken cancellationToken)
        {
            if (!initializeResourceManager)
            {
                LogDebug("[TestBootstrap] ResourceManager가 비활성화되어 리소스 로딩을 건너뜁니다.");
                return;
            }

            var resourceManager = ResourceManager.Instance;

            // 카테고리별 로딩
            if (categoriesToLoad != null && categoriesToLoad.Length > 0)
            {
                LogDebug($"[TestBootstrap] {categoriesToLoad.Length}개 카테고리 로딩 중...");

                foreach (var category in categoriesToLoad)
                {
                    bool success = await resourceManager.LoadCategoryAsync(category, cancellationToken);
                    if (success)
                    {
                        LogDebug($"[TestBootstrap] {category} 카테고리 로딩 완료");
                    }
                    else
                    {
                        Debug.LogWarning($"[TestBootstrap] {category} 카테고리 로딩 실패");
                    }
                }
            }

            // 개별 리소스 로딩
            if (individualResources != null && individualResources.Length > 0)
            {
                LogDebug($"[TestBootstrap] {individualResources.Length}개 개별 리소스 로딩 중...");

                foreach (var resourcePath in individualResources)
                {
                    if (string.IsNullOrEmpty(resourcePath)) continue;

                    // ScriptableObject로 시도
                    var resource = resourceManager.Load<ScriptableObject>(resourcePath);
                    if (resource != null)
                    {
                        LogDebug($"[TestBootstrap] 리소스 로딩 완료: {resourcePath}");
                    }
                    else
                    {
                        Debug.LogWarning($"[TestBootstrap] 리소스 로딩 실패: {resourcePath}");
                    }
                }
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 특정 리소스만 로드 (외부에서 호출 가능)
        /// </summary>
        public T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            if (!initializeResourceManager)
            {
                Debug.LogError("[TestBootstrap] ResourceManager가 초기화되지 않았습니다.");
                return null;
            }

            return ResourceManager.Instance.Load<T>(path);
        }

        /// <summary>
        /// 특정 카테고리 로드 (외부에서 호출 가능)
        /// </summary>
        public async Awaitable<bool> LoadCategory(ResourceCategory category, CancellationToken cancellationToken = default)
        {
            if (!initializeResourceManager)
            {
                Debug.LogError("[TestBootstrap] ResourceManager가 초기화되지 않았습니다.");
                return false;
            }

            return await ResourceManager.Instance.LoadCategoryAsync(category, cancellationToken);
        }

        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        #region 컨텍스트 메뉴

        [ContextMenu("Initialize Now")]
        private void InitializeNow()
        {
            if (Application.isPlaying)
            {
                _ = InitializeAsync(destroyCancellationToken);
            }
            else
            {
                Debug.LogWarning("[TestBootstrap] Play 모드에서만 실행 가능합니다.");
            }
        }

        [ContextMenu("Print Status")]
        private void PrintStatus()
        {
            Debug.Log("=== TestBootstrap 상태 ===");
            Debug.Log($"초기화 완료: {IsInitialized}");
            Debug.Log($"ResourceManager: {initializeResourceManager}");
            Debug.Log($"AudioManager: {initializeAudioManager}");
            Debug.Log($"UIManager: {initializeUIManager}");
            Debug.Log($"GameManager: {initializeGameManager}");
            Debug.Log($"로드할 카테고리 수: {categoriesToLoad?.Length ?? 0}");
            Debug.Log($"개별 리소스 수: {individualResources?.Length ?? 0}");
        }

        #endregion
    }
}
