using UnityEngine;
using Player.Physics;

namespace Core.Managers
{
    /// <summary>
    /// 리소스 관리 매니저
    /// 게임에 필요한 모든 리소스를 중앙에서 로드하고 관리
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager instance;
        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // 씬에서 찾기
                    instance = FindFirstObjectByType<ResourceManager>();

                    // 없으면 생성
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ResourceManager");
                        instance = go.AddComponent<ResourceManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("로드 상태")]
        [SerializeField] private bool isInitialized = false;

        [Header("Physics Config")]
        [SerializeField] private SkulPhysicsConfig skulPhysicsConfig;

        // 리소스 경로 상수
        private const string SKUL_PHYSICS_CONFIG_PATH = "Data/SkulPhysicsConfig";

        #region 프로퍼티

        /// <summary>
        /// Skul 물리 설정
        /// </summary>
        public SkulPhysicsConfig SkulPhysicsConfig
        {
            get
            {
                if (skulPhysicsConfig == null)
                {
                    LoadSkulPhysicsConfig();
                }
                return skulPhysicsConfig;
            }
        }

        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized => isInitialized;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // 싱글톤 패턴
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 리소스 매니저 초기화
        /// </summary>
        [ContextMenu("Initialize Resources")]
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[ResourceManager] 이미 초기화되었습니다.");
                return;
            }

            Debug.Log("[ResourceManager] 리소스 로드 시작...");

            // 모든 리소스 로드
            LoadAllResources();

            isInitialized = true;
            Debug.Log("[ResourceManager] 리소스 로드 완료!");
        }

        /// <summary>
        /// 모든 리소스 로드
        /// </summary>
        private void LoadAllResources()
        {
            LoadSkulPhysicsConfig();
            // 여기에 다른 리소스 로드 추가
        }

        #endregion

        #region 리소스 로드

        /// <summary>
        /// SkulPhysicsConfig 로드
        /// </summary>
        private void LoadSkulPhysicsConfig()
        {
            if (skulPhysicsConfig != null)
            {
                Debug.Log("[ResourceManager] SkulPhysicsConfig 이미 로드됨");
                return;
            }

            // Resources 폴더에서 로드 시도
            skulPhysicsConfig = Resources.Load<SkulPhysicsConfig>(SKUL_PHYSICS_CONFIG_PATH);

            if (skulPhysicsConfig != null)
            {
                Debug.Log($"[ResourceManager] SkulPhysicsConfig 로드 성공: {SKUL_PHYSICS_CONFIG_PATH}");
            }
            else
            {
                Debug.LogError($"[ResourceManager] SkulPhysicsConfig 로드 실패: {SKUL_PHYSICS_CONFIG_PATH}");
                Debug.LogError("[ResourceManager] 파일이 Assets/_Project/Resources/Data/SkulPhysicsConfig.asset 경로에 있는지 확인하세요!");

                // 폴백: 런타임에 기본 Config 생성
                CreateDefaultSkulPhysicsConfig();
            }
        }

        /// <summary>
        /// 기본 SkulPhysicsConfig 생성 (폴백)
        /// </summary>
        private void CreateDefaultSkulPhysicsConfig()
        {
            Debug.LogWarning("[ResourceManager] 기본 SkulPhysicsConfig를 런타임에 생성합니다.");
            skulPhysicsConfig = ScriptableObject.CreateInstance<SkulPhysicsConfig>();

            // 기본값은 ScriptableObject에 이미 정의되어 있음
            Debug.Log("[ResourceManager] 기본 SkulPhysicsConfig 생성 완료");
        }

        #endregion

        #region 리소스 접근

        /// <summary>
        /// SkulPhysicsConfig 가져오기 (정적 메서드)
        /// </summary>
        public static SkulPhysicsConfig GetSkulPhysicsConfig()
        {
            return Instance.SkulPhysicsConfig;
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 리소스 상태 출력
        /// </summary>
        [ContextMenu("Print Resource Status")]
        public void PrintResourceStatus()
        {
            Debug.Log("=== ResourceManager 상태 ===");
            Debug.Log($"초기화 완료: {isInitialized}");
            Debug.Log($"SkulPhysicsConfig: {(skulPhysicsConfig != null ? "로드됨" : "없음")}");

            if (skulPhysicsConfig != null)
            {
                Debug.Log($"  - Move Speed: {skulPhysicsConfig.moveSpeed}");
                Debug.Log($"  - Jump Velocity: {skulPhysicsConfig.jumpVelocity}");
                Debug.Log($"  - Dash Speed: {skulPhysicsConfig.dashSpeed}");
            }
        }

        /// <summary>
        /// 리소스 재로드
        /// </summary>
        [ContextMenu("Reload Resources")]
        public void ReloadResources()
        {
            Debug.Log("[ResourceManager] 리소스 재로드 시작...");

            skulPhysicsConfig = null;
            isInitialized = false;

            Initialize();
        }

        #endregion
    }
}
