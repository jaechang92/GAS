using UnityEngine;
using GameFlow;

namespace Gameplay.World
{
    /// <summary>
    /// 씬 전환 포탈
    /// Lobby에서 Gameplay 씬으로 전환하는 포탈
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ScenePortal : MonoBehaviour
    {
        [Header("포탈 설정")]
        [SerializeField] private GameEventType targetEvent = GameEventType.EnterGameplay;
        [SerializeField] private string promptText = "E를 눌러 던전 입장";
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Header("시각적 표현")]
        [SerializeField] private SpriteRenderer portalSprite;
        [SerializeField] private Color activeColor = Color.cyan;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = true;

        private Collider2D portalCollider;
        private bool playerInRange = false;
        private bool isActivated = false;

        private void Awake()
        {
            portalCollider = GetComponent<Collider2D>();
            portalCollider.isTrigger = true;

            if (portalSprite != null)
            {
                portalSprite.color = inactiveColor;
            }
        }

        private void Update()
        {
            // 플레이어가 범위 내에 있고 E키를 누르면
            if (playerInRange && !isActivated && Input.GetKeyDown(interactionKey))
            {
                ActivatePortal();
            }
        }

        /// <summary>
        /// 포탈 활성화 (씬 전환 트리거)
        /// </summary>
        private void ActivatePortal()
        {
            if (isActivated) return;

            isActivated = true;
            Log($"포탈 활성화! 이벤트 트리거: {targetEvent}");

            // 프롬프트 숨김
            HideInteractionPrompt();

            // 색상 변경
            if (portalSprite != null)
            {
                portalSprite.color = activeColor;
            }

            // GameFlowManager에 이벤트 전달
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.TriggerEvent(targetEvent);
            }
            else
            {
                Debug.LogError("[ScenePortal] GameFlowManager를 찾을 수 없습니다!");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Log($"OnTriggerEnter2D: {other.gameObject.name} (Tag: {other.tag})");

            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                ShowInteractionPrompt();
                Log("플레이어 진입: 프롬프트 표시");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Log($"OnTriggerExit2D: {other.gameObject.name} (Tag: {other.tag})");

            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                HideInteractionPrompt();
                Log("플레이어 이탈: 프롬프트 숨김");
            }
        }

        /// <summary>
        /// 상호작용 프롬프트 표시
        /// </summary>
        private void ShowInteractionPrompt()
        {
            if (Core.Managers.UIManager.Instance != null)
            {
                Core.Managers.UIManager.Instance.ShowInteractionPrompt(
                    transform,
                    promptText,
                    new Vector3(0, 2f, 0)
                );
            }
        }

        /// <summary>
        /// 상호작용 프롬프트 숨김
        /// </summary>
        private void HideInteractionPrompt()
        {
            if (Core.Managers.UIManager.Instance != null)
            {
                Core.Managers.UIManager.Instance.HideInteractionPrompt();
            }
        }

        private void OnDisable()
        {
            // 포탈이 비활성화될 때 프롬프트 숨김
            HideInteractionPrompt();
        }

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[ScenePortal - {gameObject.name}] {message}");
        }

        private void OnDrawGizmosSelected()
        {
            // 트리거 범위 표시
            Gizmos.color = Color.cyan;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawWireCube(transform.position, col.bounds.size);
            }
        }
    }
}
