using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Enums;

namespace Gameplay.Systems
{
    /// <summary>
    /// 씬 전환 포탈
    /// 플레이어가 상호작용하면 지정된 씬으로 이동
    /// </summary>
    public class Portal : MonoBehaviour
    {
        [Header("씬 설정")]
        [SerializeField] private SceneType targetScene = SceneType.Game;
        [SerializeField] private bool useSceneIndex = false;
        [SerializeField] private int targetSceneIndex = 3; // Gameplay 씬 기본값

        [Header("상호작용 설정")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool showPrompt = true;
        [SerializeField] private string promptText = "E를 눌러 입장";

        [Header("비주얼 설정")]
        [SerializeField] private Color portalColor = new Color(0.3f, 0.7f, 1f, 1f);
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = true;

        private bool playerInRange = false;
        private GameObject player;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private float pulseTimer = 0f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = portalColor;
                spriteRenderer.color = originalColor;
            }
        }

        private void Update()
        {
            // 펄스 애니메이션
            if (spriteRenderer != null)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
                Color targetColor = originalColor * (1f + pulse);
                targetColor.a = originalColor.a;
                spriteRenderer.color = targetColor;
            }

            // 플레이어가 범위 안에 있고 E키를 누르면 씬 전환
            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                EnterPortal();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                player = other.gameObject;

                if (showDebugLog)
                    Debug.Log($"[Portal] 플레이어가 포탈 범위에 진입했습니다. ({promptText})");

                // TODO: 프롬프트 UI 표시
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                player = null;

                if (showDebugLog)
                    Debug.Log("[Portal] 플레이어가 포탈 범위를 벗어났습니다.");

                // TODO: 프롬프트 UI 숨김
            }
        }

        private void EnterPortal()
        {
            if (showDebugLog)
                Debug.Log($"[Portal] 포탈 진입! 목표 씬: {(useSceneIndex ? targetSceneIndex.ToString() : targetScene.ToString())}");

            // 씬 전환
            if (useSceneIndex)
            {
                LoadSceneByIndex(targetSceneIndex);
            }
            else
            {
                LoadSceneByType(targetScene);
            }
        }

        private void LoadSceneByIndex(int sceneIndex)
        {
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError($"[Portal] 잘못된 씬 인덱스입니다: {sceneIndex}");
            }
        }

        private void LoadSceneByType(SceneType sceneType)
        {
            // SceneType을 씬 이름으로 변환
            string sceneName = GetSceneNameFromType(sceneType);

            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError($"[Portal] SceneType을 씬 이름으로 변환할 수 없습니다: {sceneType}");
            }
        }

        private string GetSceneNameFromType(SceneType sceneType)
        {
            // SceneType Enum을 실제 씬 이름으로 매핑
            switch (sceneType)
            {
                case SceneType.Bootstrap:
                    return "Bootstrap";
                case SceneType.Preload:
                    return "Preload";
                case SceneType.MainMenu:
                    return "Main";
                case SceneType.Game:
                    return "Gameplay";
                case SceneType.Lobby:
                    return "Lobby";
                default:
                    return string.Empty;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위 표시
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        #region Public Methods

        /// <summary>
        /// 목표 씬을 SceneType으로 설정
        /// </summary>
        public void SetTargetScene(SceneType sceneType)
        {
            targetScene = sceneType;
            useSceneIndex = false;

            if (showDebugLog)
                Debug.Log($"[Portal] 목표 씬 설정: {sceneType}");
        }

        /// <summary>
        /// 목표 씬을 인덱스로 설정
        /// </summary>
        public void SetTargetSceneIndex(int sceneIndex)
        {
            targetSceneIndex = sceneIndex;
            useSceneIndex = true;

            if (showDebugLog)
                Debug.Log($"[Portal] 목표 씬 인덱스 설정: {sceneIndex}");
        }

        /// <summary>
        /// 포탈 색상 변경
        /// </summary>
        public void SetPortalColor(Color color)
        {
            portalColor = color;
            originalColor = color;

            if (spriteRenderer != null)
                spriteRenderer.color = color;
        }

        #endregion
    }
}
