using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Gameplay.Dialogue
{
    /// <summary>
    /// 대화 재생 관리자
    /// 에피소드 ID를 받아 대화를 진행하는 싱글톤
    /// </summary>
    public class DialogueManager : SingletonManager<DialogueManager>
    {
        [Header("디버그")]
        [SerializeField] private bool showDebugLog = true;

        // 현재 재생 중인 에피소드 및 노드
        private DialogueEpisode currentEpisode;
        private DialogueNode currentNode;
        private List<DialogueChoice> currentChoices;

        // 대화 진행 상태
        public bool IsPlaying { get; private set; } = false;
        public string CurrentEpisodeID => currentEpisode?.episodeID;

        // 이벤트 리스너 (UI)
        private List<IDialogueListener> listeners = new List<IDialogueListener>();

        // 이벤트 (필요 시 사용)
        public event Action<string> OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<DialogueNode> OnNodeChanged;
        public event Action<DialogueNode, List<DialogueChoice>> OnChoicesShown;

        protected override void OnSingletonAwake()
        {
            Log("DialogueManager 초기화");
        }

        #region 리스너 관리

        /// <summary>
        /// 대화 리스너 등록
        /// </summary>
        public void RegisterListener(IDialogueListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
                Log($"리스너 등록: {listener.GetType().Name}");
            }
        }

        /// <summary>
        /// 대화 리스너 제거
        /// </summary>
        public void UnregisterListener(IDialogueListener listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
                Log($"리스너 제거: {listener.GetType().Name}");
            }
        }

        #endregion

        #region 대화 재생

        /// <summary>
        /// 에피소드 시작
        /// </summary>
        public bool StartEpisode(string episodeID)
        {
            if (IsPlaying)
            {
                Debug.LogWarning($"[DialogueManager] 이미 대화가 진행 중입니다. (현재: {CurrentEpisodeID})");
                return false;
            }

            // 데이터베이스에서 에피소드 로드
            currentEpisode = DialogueDatabase.Instance?.GetEpisode(episodeID);
            if (currentEpisode == null)
            {
                Debug.LogError($"[DialogueManager] 에피소드 '{episodeID}'를 찾을 수 없습니다.");
                return false;
            }

            // 시작 노드로 이동
            currentNode = currentEpisode.GetStartNode();
            if (currentNode == null)
            {
                Debug.LogError($"[DialogueManager] 에피소드 '{episodeID}'의 시작 노드를 찾을 수 없습니다.");
                return false;
            }

            IsPlaying = true;
            Log($"=== 에피소드 시작: {currentEpisode} ===");

            // DialoguePanel 열기
            OpenDialoguePanel();

            // 리스너에게 통보
            NotifyDialogueStart(episodeID);
            OnDialogueStarted?.Invoke(episodeID);

            // 첫 노드 표시
            ShowCurrentNode();

            return true;
        }

        /// <summary>
        /// DialoguePanel 열기
        /// </summary>
        private async void OpenDialoguePanel()
        {
            if (Core.Managers.UIManager.Instance != null)
            {
                await Core.Managers.UIManager.Instance.OpenPanel(UI.Core.PanelType.Dialog);
            }
        }

        /// <summary>
        /// 다음 노드로 진행
        /// </summary>
        public void ShowNextNode()
        {
            if (!IsPlaying)
            {
                Debug.LogWarning("[DialogueManager] 진행 중인 대화가 없습니다.");
                return;
            }

            if (currentNode.hasChoices)
            {
                Debug.LogWarning("[DialogueManager] 선택지가 있는 노드입니다. SelectChoice()를 사용하세요.");
                return;
            }

            // 다음 노드 ID 확인
            int nextNodeID = currentNode.nextNodeID;

            if (nextNodeID == 0)
            {
                // 대화 종료
                EndDialogue();
                return;
            }

            // 다음 노드로 이동
            currentNode = currentEpisode.GetNode(nextNodeID);
            if (currentNode == null)
            {
                Debug.LogError($"[DialogueManager] 노드 {nextNodeID}를 찾을 수 없습니다.");
                EndDialogue();
                return;
            }

            ShowCurrentNode();
        }

        /// <summary>
        /// 선택지 선택
        /// </summary>
        public void SelectChoice(int choiceID)
        {
            if (!IsPlaying)
            {
                Debug.LogWarning("[DialogueManager] 진행 중인 대화가 없습니다.");
                return;
            }

            if (currentChoices == null || currentChoices.Count == 0)
            {
                Debug.LogWarning("[DialogueManager] 현재 표시된 선택지가 없습니다.");
                return;
            }

            // 선택지 찾기
            DialogueChoice selectedChoice = currentChoices.Find(c => c.choiceID == choiceID);
            if (selectedChoice == null)
            {
                Debug.LogError($"[DialogueManager] 선택지 ID {choiceID}를 찾을 수 없습니다.");
                return;
            }

            Log($"선택지 선택: [{selectedChoice.choiceID}] {selectedChoice.choiceText}");

            // 상점 아이템인 경우 구매 처리
            if (selectedChoice.IsShopItem)
            {
                bool purchased = ProcessShopPurchase(selectedChoice);
                if (!purchased)
                {
                    // 구매 실패 시 대화 유지
                    return;
                }
            }

            // 종료 선택지인 경우
            if (selectedChoice.IsExitChoice)
            {
                EndDialogue();
                return;
            }

            // 다음 노드로 이동
            int nextNodeID = selectedChoice.nextNodeID;
            if (nextNodeID == 0)
            {
                EndDialogue();
                return;
            }

            currentNode = currentEpisode.GetNode(nextNodeID);
            if (currentNode == null)
            {
                Debug.LogError($"[DialogueManager] 노드 {nextNodeID}를 찾을 수 없습니다.");
                EndDialogue();
                return;
            }

            currentChoices = null;
            ShowCurrentNode();
        }

        /// <summary>
        /// 대화 강제 종료
        /// </summary>
        public void EndDialogue()
        {
            if (!IsPlaying) return;

            Log($"=== 에피소드 종료: {currentEpisode.episodeID} ===");

            IsPlaying = false;
            currentEpisode = null;
            currentNode = null;
            currentChoices = null;

            // 리스너에게 통보
            NotifyDialogueEnd();
            OnDialogueEnded?.Invoke();
        }

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 현재 노드 표시
        /// </summary>
        private void ShowCurrentNode()
        {
            Log($"노드 표시: {currentNode}");

            // 리스너에게 노드 변경 통보
            NotifyNodeChanged(currentNode);
            OnNodeChanged?.Invoke(currentNode);

            // 선택지가 있는 경우
            if (currentNode.hasChoices)
            {
                currentChoices = currentEpisode.GetChoices(currentNode.nodeID);
                Log($"선택지 표시: {currentChoices.Count}개");

                // 리스너에게 선택지 통보
                NotifyChoicesShown(currentNode, currentChoices);
                OnChoicesShown?.Invoke(currentNode, currentChoices);
            }
        }

        /// <summary>
        /// 상점 구매 처리
        /// </summary>
        private bool ProcessShopPurchase(DialogueChoice choice)
        {
            // GameManager에서 골드 확인
            var gameManager = Core.Managers.GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[DialogueManager] GameManager를 찾을 수 없습니다.");
                return false;
            }

            // 골드 확인
            if (!gameManager.HasEnoughGold(choice.requiredGold))
            {
                Debug.LogWarning($"[DialogueManager] 골드가 부족합니다. (필요: {choice.requiredGold}G, 보유: {gameManager.CurrentGold}G)");
                // UI에 메시지 표시 (차후 구현)
                return false;
            }

            // 골드 차감
            bool spent = gameManager.SpendGold(choice.requiredGold);
            if (!spent)
            {
                Debug.LogError("[DialogueManager] 골드 차감 실패");
                return false;
            }

            Log($"아이템 구매 성공: {choice.rewardItem} ({choice.requiredGold}G)");

            // 아이템 지급 처리 (차후 구현)
            // TODO: 인벤토리 시스템 또는 AbilitySystem에 아이템/스킬 추가

            return true;
        }

        #endregion

        #region 리스너 통보

        private void NotifyDialogueStart(string episodeID)
        {
            foreach (var listener in listeners)
            {
                listener.OnDialogueStart(episodeID);
            }
        }

        private void NotifyNodeChanged(DialogueNode node)
        {
            foreach (var listener in listeners)
            {
                listener.OnDialogueNodeChanged(node);
            }
        }

        private void NotifyChoicesShown(DialogueNode node, List<DialogueChoice> choices)
        {
            foreach (var listener in listeners)
            {
                listener.OnDialogueChoicesShown(node, choices);
            }
        }

        private void NotifyDialogueEnd()
        {
            foreach (var listener in listeners)
            {
                listener.OnDialogueEnd();
            }
        }

        #endregion

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[DialogueManager] {message}");
        }
    }
}
