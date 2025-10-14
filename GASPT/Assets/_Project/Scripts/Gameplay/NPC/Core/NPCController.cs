using UnityEngine;
using Gameplay.Dialogue;

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

        // 상호작용 프롬프트 UI (World Space)
        protected GameObject interactionPrompt;

        public virtual bool CanInteract => playerInRange && !DialogueManager.Instance.IsPlaying;

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
            // 상호작용 프롬프트 생성
            if (npcData != null && npcData.showInteractionPrompt)
            {
                CreateInteractionPrompt();
            }

            Log($"NPC 초기화: {GetNPCName()}");
        }

        protected virtual void Update()
        {
            // 플레이어가 범위 내에 있고 상호작용 키를 누르면
            if (CanInteract && Input.GetKeyDown(npcData.interactionKey))
            {
                OnInteract();
            }

            // 상호작용 프롬프트 표시/숨김
            UpdateInteractionPrompt();
        }

        #region INPCInteractable 구현

        public virtual void OnPlayerEnterRange()
        {
            playerInRange = true;
            Log($"플레이어 진입: {GetNPCName()}");
        }

        public virtual void OnPlayerExitRange()
        {
            playerInRange = false;
            Log($"플레이어 이탈: {GetNPCName()}");
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
        /// 상호작용 프롬프트 생성 (World Space UI)
        /// </summary>
        protected virtual void CreateInteractionPrompt()
        {
            // 프롬프트 오브젝트 생성 (간단한 TextMesh로 구현)
            interactionPrompt = new GameObject("InteractionPrompt");
            interactionPrompt.transform.SetParent(transform);
            interactionPrompt.transform.localPosition = new Vector3(0, 1.5f, 0);

            // TextMesh 추가 (간단한 구현)
            var textMesh = interactionPrompt.AddComponent<TextMesh>();
            textMesh.text = npcData.interactionPromptText;
            textMesh.fontSize = 20;
            textMesh.color = Color.white;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            // 초기에는 숨김
            interactionPrompt.SetActive(false);
        }

        /// <summary>
        /// 상호작용 프롬프트 업데이트
        /// </summary>
        protected virtual void UpdateInteractionPrompt()
        {
            if (interactionPrompt == null) return;

            // 상호작용 가능할 때만 표시
            bool shouldShow = CanInteract;
            if (interactionPrompt.activeSelf != shouldShow)
            {
                interactionPrompt.SetActive(shouldShow);
            }
        }

        #endregion

        #region 대화 콜백

        /// <summary>
        /// 대화 시작 시 호출
        /// </summary>
        protected virtual void OnDialogueStarted()
        {
            Log("대화 시작됨");
        }

        /// <summary>
        /// 대화 종료 시 호출
        /// </summary>
        protected virtual void OnDialogueEnded()
        {
            Log("대화 종료됨");
        }

        #endregion

        #region Unity 이벤트

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEnterRange();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
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
