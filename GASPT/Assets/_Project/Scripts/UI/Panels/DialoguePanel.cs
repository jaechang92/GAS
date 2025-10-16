using System.Collections.Generic;
using System.Threading;
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

        // 타이핑 효과
        private CancellationTokenSource typingCancellation;
        private bool isTyping = false;

        // 키 입력 처리
        private CancellationTokenSource keyInputCancellation;

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
            Log("OnEnable 시작");

            // DialogueManager에 리스너 등록
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.RegisterListener(this);
                Log("DialogueManager 리스너 등록 완료");
            }

            // 키 입력 감지 시작
            keyInputCancellation = new CancellationTokenSource();
            _ = WaitForKeyInputAsync(keyInputCancellation.Token);
            Log("키 입력 감지 시작");

            Log($"DialoguePanel 활성화 완료 (IsOpen: {IsOpen})");
        }

        private void OnDisable()
        {
            // DialogueManager에서 리스너 제거
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.UnregisterListener(this);
            }

            // 키 입력 감지 중지
            keyInputCancellation?.Cancel();
            keyInputCancellation?.Dispose();
            keyInputCancellation = null;

            // 타이핑 효과 중지
            typingCancellation?.Cancel();
            typingCancellation?.Dispose();
            typingCancellation = null;

            Log("DialoguePanel 비활성화");
        }

        #region IDialogueListener 구현

        public void OnDialogueStart(string episodeID)
        {
            Log($"OnDialogueStart 호출: {episodeID} (IsOpen: {IsOpen})");

            // UI 초기화
            ClearChoices();
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }

            // currentNode 명시적으로 null 초기화
            currentNode = null;
            Log("대화 시작 준비 완료");
        }

        public async Awaitable OnDialogueNodeChanged(DialogueNode node)
        {
            Log($"OnDialogueNodeChanged 호출: {node?.dialogueText} (IsOpen: {IsOpen})");

            currentNode = node;

            // 화자 이름 표시
            if (speakerNameText != null)
            {
                speakerNameText.text = node.speakerName;
            }

            // 대화 텍스트 표시
            if (useTypingEffect)
            {
                await ShowDialogueWithTyping(node.dialogueText);
            }
            else
            {
                ShowDialogueInstant(node.dialogueText);
            }

            // Continue 버튼 표시 (선택지가 없는 경우)
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(!node.hasChoices);
                Log($"Continue 버튼 표시: {!node.hasChoices}");
            }

            // 선택지 숨김
            ClearChoices();

            Log($"노드 변경 완료 (currentNode: {currentNode != null}, hasChoices: {node.hasChoices})");
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
        private async Awaitable ShowDialogueWithTyping(string text)
        {
            typingCancellation?.Cancel();
            typingCancellation?.Dispose();

            typingCancellation = new CancellationTokenSource();
            await TypeTextAsync(text, typingCancellation.Token);
        }

        /// <summary>
        /// 타이핑 효과 비동기 메서드
        /// </summary>
        private async Awaitable TypeTextAsync(string text, CancellationToken cancellationToken)
        {
            isTyping = true;
            dialogueText.text = "";

            try
            {
                foreach (char c in text)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    dialogueText.text += c;
                    await Awaitable.WaitForSecondsAsync(typingSpeed, cancellationToken);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 취소된 경우 예외 무시
            }
            finally
            {
                isTyping = false;
            }
        }

        /// <summary>
        /// 타이핑 효과 스킵
        /// </summary>
        private void SkipTyping()
        {
            if (isTyping)
            {
                typingCancellation?.Cancel();
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
            Log($"OnContinueButtonClicked 호출 (isTyping: {isTyping})");

            // 타이핑 중이면 스킵
            if (isTyping)
            {
                Log("타이핑 중 - 스킵");
                SkipTyping();
                return;
            }

            // 다음 노드로 진행
            Log("DialogueManager.ShowNextNode() 호출");
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

        #region 키 입력 처리

        /// <summary>
        /// 키 입력 대기 (이벤트 기반)
        /// Space 또는 Enter로 대화 진행
        /// </summary>
        private async Awaitable WaitForKeyInputAsync(CancellationToken cancellationToken)
        {
            Log("WaitForKeyInputAsync 시작");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);

                    // Panel이 열려있고, currentNode가 null이 아니며, 선택지가 없을 때만 체크
                    if (IsOpen && currentNode != null && !currentNode.hasChoices)
                    {
                        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                        {
                            Log("키 입력 감지! OnContinueButtonClicked 호출");
                            OnContinueButtonClicked();
                        }
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                Log("WaitForKeyInputAsync 취소됨");
            }
        }

        #endregion

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[DialoguePanel] {message}");
        }
    }
}
