using UnityEngine;
using TMPro;
using Gameplay.Dialogue;
using Core.Managers;

namespace Gameplay.NPC
{
    /// <summary>
    /// NPC 베이스 컨트롤러
    /// 모든 NPC의 기본 동작을 정의
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class NPCController : MonoBehaviour, INPCInteractable
    {
        [Header("NPC 설정")]
        [SerializeField] protected NPCData npcData;

        [Header("디버그")]
        [SerializeField] protected bool showDebugLog = true;

        // 컴포넌트
        protected SpriteRenderer spriteRenderer;
        protected Collider2D npcCollider;

        // 상태
        protected bool playerInRange = false;
        protected int currentEpisodeIndex = 0;

        public virtual bool CanInteract => playerInRange &&
                                           DialogueManager.Instance != null &&
                                           !DialogueManager.Instance.IsPlaying;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            npcCollider = GetComponent<Collider2D>();

            // NPC 스프라이트 설정
            if (npcData != null && npcData.npcSprite != null)
            {
                spriteRenderer.sprite = npcData.npcSprite;
            }

            // Collider를 Trigger로 설정
            npcCollider.isTrigger = true;
        }

        protected virtual void Start()
        {
            Log($"NPC 초기화 시작: {gameObject.name}");

            // NPCData 검증
            if (npcData == null)
            {
                Debug.LogError($"[{gameObject.name}] NPCData가 설정되지 않았습니다! Inspector에서 NPCData를 할당하세요.");
                return;
            }

            // DialogueManager 이벤트 구독
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
            }

            Log($"NPCData 확인: {npcData.npcName}, showPrompt={npcData.showInteractionPrompt}");
            Log($"NPC 초기화 완료: {GetNPCName()}");
        }

        protected virtual void Update()
        {
            // 플레이어가 범위 내에 있고 상호작용 키를 누르면
            if (CanInteract && Input.GetKeyDown(npcData.interactionKey))
            {
                OnInteract();
            }
        }

        protected virtual void OnDisable()
        {
            // DialogueManager 이벤트 구독 해제
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
            }

            // NPC가 비활성화될 때 프롬프트 숨김
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractionPrompt();
            }
        }

        #region INPCInteractable 구현

        public virtual void OnPlayerEnterRange()
        {
            playerInRange = true;
            Log($"플레이어 진입: {GetNPCName()}");

            // 프롬프트 표시
            ShowInteractionPrompt();
        }

        public virtual void OnPlayerExitRange()
        {
            playerInRange = false;
            Log($"플레이어 이탈: {GetNPCName()}");

            // 프롬프트 숨김
            HideInteractionPrompt();
        }

        public virtual void OnInteract()
        {
            if (!CanInteract) return;

            Log($"상호작용 시작: {GetNPCName()}");

            // 현재 에피소드 ID 가져오기
            string episodeID = GetCurrentEpisodeID();
            if (string.IsNullOrEmpty(episodeID))
            {
                Debug.LogWarning($"[NPCController] {GetNPCName()}의 에피소드가 없습니다.");
                return;
            }

            // DialogueManager를 통해 대화 시작
            bool started = DialogueManager.Instance.StartEpisode(episodeID);
            if (started)
            {
                OnDialogueStarted();
            }
        }

        #endregion

        #region 에피소드 관리

        /// <summary>
        /// 현재 에피소드 ID 가져오기
        /// </summary>
        protected virtual string GetCurrentEpisodeID()
        {
            if (npcData == null || npcData.episodeIDs.Count == 0)
            {
                return null;
            }

            // 인덱스 범위 확인
            if (currentEpisodeIndex >= npcData.episodeIDs.Count)
            {
                currentEpisodeIndex = npcData.episodeIDs.Count - 1; // 마지막 에피소드 반복
            }

            return npcData.episodeIDs[currentEpisodeIndex];
        }

        /// <summary>
        /// 다음 에피소드로 진행
        /// </summary>
        public virtual void AdvanceToNextEpisode()
        {
            if (npcData == null || npcData.episodeIDs.Count == 0) return;

            currentEpisodeIndex++;
            if (currentEpisodeIndex >= npcData.episodeIDs.Count)
            {
                currentEpisodeIndex = npcData.episodeIDs.Count - 1; // 마지막 에피소드에 머무름
            }

            Log($"다음 에피소드로 진행: {GetCurrentEpisodeID()}");
        }

        #endregion

        #region 상호작용 프롬프트

        /// <summary>
        /// 상호작용 프롬프트 표시
        /// </summary>
        protected virtual void ShowInteractionPrompt()
        {
            // NPCData가 없거나 프롬프트 표시 설정이 false면 스킵
            if (npcData == null || !npcData.showInteractionPrompt)
                return;

            // UIManager가 없으면 경고
            if (UIManager.Instance == null)
            {
                if (showDebugLog)
                {
                    Debug.LogWarning($"[{GetNPCName()}] UIManager.Instance가 null입니다!");
                }
                return;
            }

            // 대화 중이 아닐 때만 표시
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
                return;

            // UIManager를 통해 프롬프트 표시
            UIManager.Instance.ShowInteractionPrompt(
                transform,
                npcData.interactionPromptText,
                new Vector3(0, 1.5f, 0)
            );

            Log("상호작용 프롬프트 표시");
        }

        /// <summary>
        /// 상호작용 프롬프트 숨김
        /// </summary>
        protected virtual void HideInteractionPrompt()
        {
            if (UIManager.Instance == null)
                return;

            UIManager.Instance.HideInteractionPrompt();
            Log("상호작용 프롬프트 숨김");
        }

        #endregion

        #region 대화 콜백

        /// <summary>
        /// 대화 시작 시 호출
        /// </summary>
        protected virtual void OnDialogueStarted()
        {
            Log("대화 시작됨");

            // 대화 중에는 프롬프트 숨김
            HideInteractionPrompt();
        }

        /// <summary>
        /// 대화 종료 시 호출
        /// </summary>
        protected virtual void OnDialogueEnded()
        {
            Log("대화 종료됨");

            // 플레이어가 여전히 범위 내에 있으면 프롬프트 다시 표시
            if (playerInRange)
            {
                ShowInteractionPrompt();
            }
        }

        #endregion

        #region Unity 이벤트

        private void OnTriggerEnter2D(Collider2D other)
        {
            Log($"OnTriggerEnter2D: {other.gameObject.name} (Tag: {other.tag})");

            if (other.CompareTag("Player"))
            {
                OnPlayerEnterRange();
            }
            else
            {
                Log($"Player 태그가 아님! 현재 태그: {other.tag}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Log($"OnTriggerExit2D: {other.gameObject.name} (Tag: {other.tag})");

            if (other.CompareTag("Player"))
            {
                OnPlayerExitRange();
            }
        }

        #endregion

        #region 유틸리티

        protected string GetNPCName()
        {
            return npcData != null ? npcData.npcName : gameObject.name;
        }

        protected void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[{GetNPCName()}] {message}");
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (npcData == null) return;

            // 상호작용 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, npcData.interactionRange);
        }

        #endregion
    }
}
