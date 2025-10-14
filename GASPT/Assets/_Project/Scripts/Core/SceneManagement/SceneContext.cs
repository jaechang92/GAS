using UnityEngine;
using Core.Enums;

namespace Core.SceneManagement
{
    /// <summary>
    /// 각 씬의 컨텍스트 기본 클래스
    /// 씬별로 독립적인 데이터와 오브젝트 관리
    /// </summary>
    public abstract class SceneContext : MonoBehaviour
    {
        [Header("씬 정보")]
        [SerializeField] protected SceneType sceneType;

        /// <summary>
        /// 씬 타입
        /// </summary>
        public SceneType SceneType => sceneType;

        /// <summary>
        /// 씬이 초기화되었는지 여부
        /// </summary>
        public bool IsInitialized { get; protected set; }

        protected virtual void Awake()
        {
            Debug.Log($"[{sceneType}Context] Awake");
        }

        protected virtual void Start()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 씬 초기화
        /// </summary>
        public virtual void Initialize()
        {
            Debug.Log($"[{sceneType}Context] Initialize");
            IsInitialized = true;
            OnInitialize();
        }

        /// <summary>
        /// 씬 정리 (씬 언로드 전 호출)
        /// </summary>
        public virtual void Cleanup()
        {
            Debug.Log($"[{sceneType}Context] Cleanup");
            OnCleanup();
            IsInitialized = false;
        }

        /// <summary>
        /// 씬 활성화 (Additive 씬에서 유용)
        /// </summary>
        public virtual void Activate()
        {
            Debug.Log($"[{sceneType}Context] Activate");
            gameObject.SetActive(true);
            OnActivate();
        }

        /// <summary>
        /// 씬 비활성화 (Additive 씬에서 유용)
        /// </summary>
        public virtual void Deactivate()
        {
            Debug.Log($"[{sceneType}Context] Deactivate");
            OnDeactivate();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 하위 클래스에서 구현할 초기화 로직
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 하위 클래스에서 구현할 정리 로직
        /// </summary>
        protected virtual void OnCleanup() { }

        /// <summary>
        /// 하위 클래스에서 구현할 활성화 로직
        /// </summary>
        protected virtual void OnActivate() { }

        /// <summary>
        /// 하위 클래스에서 구현할 비활성화 로직
        /// </summary>
        protected virtual void OnDeactivate() { }

        protected virtual void OnDestroy()
        {
            if (IsInitialized)
            {
                Cleanup();
            }
        }
    }
}
