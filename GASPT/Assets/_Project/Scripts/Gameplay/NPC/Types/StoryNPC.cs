using UnityEngine;
using Gameplay.Dialogue;

namespace Gameplay.NPC
{
    /// <summary>
    /// 스토리를 알려주는 NPC
    /// 대화 완료 시 자동으로 다음 에피소드로 진행
    /// </summary>
    public class StoryNPC : NPCController
    {
        [Header("스토리 NPC 설정")]
        [SerializeField] private bool autoAdvanceEpisode = true;  // 대화 완료 시 자동으로 다음 에피소드로
        [SerializeField] private bool repeatLastEpisode = true;   // 마지막 에피소드를 반복할지

        protected override void Start()
        {
            base.Start();

            // DialogueManager 이벤트 구독
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
            }
        }

        protected override void OnDialogueStarted()
        {
            base.OnDialogueStarted();
            Log($"스토리 대화 시작: {GetCurrentEpisodeID()}");
        }

        /// <summary>
        /// 대화 종료 시 호출
        /// </summary>
        private void HandleDialogueEnded()
        {
            if (!playerInRange) return; // 이 NPC와의 대화가 아니면 무시

            Log("스토리 대화 종료");

            // 자동으로 다음 에피소드로 진행
            if (autoAdvanceEpisode)
            {
                AdvanceToNextEpisode();
            }
        }

        protected override string GetCurrentEpisodeID()
        {
            if (npcData == null || npcData.episodeIDs.Count == 0)
            {
                return null;
            }

            // 인덱스 범위 확인
            if (currentEpisodeIndex >= npcData.episodeIDs.Count)
            {
                if (repeatLastEpisode)
                {
                    currentEpisodeIndex = npcData.episodeIDs.Count - 1; // 마지막 에피소드 반복
                }
                else
                {
                    return null; // 더 이상 에피소드 없음
                }
            }

            return npcData.episodeIDs[currentEpisodeIndex];
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
            }
        }
    }
}
