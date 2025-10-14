using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.Core;
using Gameplay.Dialogue;

namespace UI.Panels
{
    /// <summary>
    /// 대화 UI Panel
    /// DialogueManager의 이벤트를 받아 대화를 표시
    /// </summary>
    public class DialoguePanel : BasePanel, IDialogueListener
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Transform choiceButtonContainer;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("타이핑 효과")]
        [SerializeField] private bool useTypingEffect = true;
        [SerializeField] private float typingSpeed = 0.05f;

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = true;

        // 현재 표시 중인 노드
        private DialogueNode currentNode;
        private List<Button> activeChoiceButtons = new List<Button>();

        // 타이핑 효과 코루틴
        private Coroutine typingCoroutine;
        private bool isTyping = false;

        protected override void Awake()
        {
            base.Awake();

            // Continue 버튼 이벤트
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueButtonClicked);
            }
        }

        private void OnEnable()
        {
            // DialogueManager에 리스너 등록
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.RegisterListener(this);
            }

            Log("DialoguePanel 활성화");
        }

        private void OnDisable()
        {
            // DialogueManager에서 리스너 제거
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.UnregisterListener(this);
            }

            // 타이핑 효과 중지
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            Log("DialoguePanel 비활성화");
        }

        #region IDialogueListener 구현

        public void OnDialogueStart(string episodeID)
        {
            Log($"대화 시작: {episodeID}");

            // UI 초기화
            ClearChoices();
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }
        }

        public void OnDialogueNodeChanged(DialogueNode node)
        {
            currentNode = node;
            Log($"노드 변경: {node}");

            // 화자 이름 표시
            if (speakerNameText != null)
            {
                speakerNameText.text = node.speakerName;
            }

            // 대화 텍스트 표시
            if (useTypingEffect)
            {
                ShowDialogueWithTyping(node.dialogueText);
            }
            else
            {
                ShowDialogueInstant(node.dialogueText);
            }

            // Continue 버튼 표시 (선택지가 없는 경우)
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(!node.hasChoices);
            }

            // 선택지 숨김
            ClearChoices();
        }

        public void OnDialogueChoicesShown(DialogueNode node, List<DialogueChoice> choices)
        {
            Log($"선택지 표시: {choices.Count}개");

            // Continue 버튼 숨김
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }

            // 선택지 버튼 생성
            ShowChoices(choices);
        }

        public void OnDialogueEnd()
        {
            Log("대화 종료");

            // Panel 닫기
            Close();
        }

        #endregion

        #region 대화 표시

        /// <summary>
        /// 대화 즉시 표시
        /// </summary>
        private void ShowDialogueInstant(string text)
        {
            if (dialogueText != null)
            {
                dialogueText.text = text;
            }
            isTyping = false;
        }

        /// <summary>
        /// 대화 타이핑 효과로 표시
        /// </summary>
        private void ShowDialogueWithTyping(string text)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText(text));
        }

        /// <summary>
        /// 타이핑 효과 코루틴
        /// </summary>
        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";

            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            typingCoroutine = null;
        }

        /// <summary>
        /// 타이핑 효과 스킵
        /// </summary>
        private void SkipTyping()
        {
            if (isTyping && typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                isTyping = false;

                if (currentNode != null)
                {
                    dialogueText.text = currentNode.dialogueText;
                }
            }
        }

        #endregion

        #region 선택지 관리

        /// <summary>
        /// 선택지 버튼 생성 및 표시
        /// </summary>
        private void ShowChoices(List<DialogueChoice> choices)
        {
            ClearChoices();

            if (choiceButtonPrefab == null || choiceButtonContainer == null)
            {
                Debug.LogError("[DialoguePanel] choiceButtonPrefab 또는 choiceButtonContainer가 설정되지 않았습니다.");
                return;
            }

            foreach (var choice in choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                button.gameObject.SetActive(true);

                // 버튼 텍스트 설정
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choice.choiceText;
                }

                // 버튼 클릭 이벤트
                int choiceID = choice.choiceID; // 로컬 변수로 캡처
                button.onClick.AddListener(() => OnChoiceSelected(choiceID));

                activeChoiceButtons.Add(button);
            }

            Log($"선택지 버튼 {activeChoiceButtons.Count}개 생성");
        }

        /// <summary>
        /// 선택지 버튼 전부 제거
        /// </summary>
        private void ClearChoices()
        {
            foreach (var button in activeChoiceButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            activeChoiceButtons.Clear();
        }

        #endregion

        #region UI 이벤트 핸들러

        /// <summary>
        /// Continue 버튼 클릭
        /// </summary>
        private void OnContinueButtonClicked()
        {
            // 타이핑 중이면 스킵
            if (isTyping)
            {
                SkipTyping();
                return;
            }

            // 다음 노드로 진행
            DialogueManager.Instance?.ShowNextNode();
        }

        /// <summary>
        /// 선택지 선택
        /// </summary>
        private void OnChoiceSelected(int choiceID)
        {
            Log($"선택지 선택: {choiceID}");

            // DialogueManager에 선택 전달
            DialogueManager.Instance?.SelectChoice(choiceID);
        }

        #endregion

        private void Update()
        {
            // Space 또는 Enter로 대화 진행
            if (IsOpen && !currentNode.hasChoices)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnContinueButtonClicked();
                }
            }
        }

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[DialoguePanel] {message}");
        }
    }
}
