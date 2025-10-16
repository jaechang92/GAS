using UnityEngine;
using TMPro;
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

            Log($"NPCData 확인: {npcData.npcName}, showPrompt={npcData.showInteractionPrompt}");

            // 상호작용 프롬프트 생성
            if (npcData.showInteractionPrompt)
            {
                CreateInteractionPrompt();
            }
            else
            {
                Log("showInteractionPrompt가 false로 설정되어 프롬프트를 생성하지 않습니다.");
            }

            Log($"NPC 초기화 완료: {GetNPCName()}");
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
        /// 상호작용 프롬프트 생성 (World Space Canvas + TextMeshPro)
        /// </summary>
        protected virtual void CreateInteractionPrompt()
        {
            Log("상호작용 프롬프트 생성 시작");

            // Canvas 오브젝트 생성
            interactionPrompt = new GameObject("InteractionPrompt");
            interactionPrompt.transform.SetParent(transform);
            interactionPrompt.transform.localPosition = new Vector3(0, 1.5f, 0);

            // Canvas 추가 (World Space)
            var canvas = interactionPrompt.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var canvasScaler = interactionPrompt.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10;

            // RectTransform 설정
            var rectTransform = interactionPrompt.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(2, 0.5f);

            // TextMeshProUGUI 추가
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(interactionPrompt.transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one;

            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = npcData.interactionPromptText;
            tmpText.fontSize = 18;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // 배경 추가 (가독성 향상)
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(interactionPrompt.transform);
            bgObj.transform.SetAsFirstSibling(); // 텍스트 뒤로
            bgObj.transform.localPosition = Vector3.zero;
            bgObj.transform.localRotation = Quaternion.identity;
            bgObj.transform.localScale = Vector3.one;

            var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);

            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // 초기에는 숨김
            interactionPrompt.SetActive(false);

            Log("상호작용 프롬프트 생성 완료");
        }

        /// <summary>
        /// 상호작용 프롬프트 업데이트
        /// </summary>
        protected virtual void UpdateInteractionPrompt()
        {
            if (interactionPrompt == null)
            {
                if (showDebugLog && playerInRange)
                {
                    Debug.LogWarning($"[{GetNPCName()}] interactionPrompt가 null입니다!");
                }
                return;
            }

            // 상호작용 가능할 때만 표시
            bool shouldShow = CanInteract;
            if (interactionPrompt.activeSelf != shouldShow)
            {
                interactionPrompt.SetActive(shouldShow);
                Log($"프롬프트 표시 상태 변경: {shouldShow} (playerInRange={playerInRange}, DialogueManager={(DialogueManager.Instance != null)})");
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
