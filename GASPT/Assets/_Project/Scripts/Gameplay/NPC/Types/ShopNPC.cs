using UnityEngine;
using Gameplay.Dialogue;

namespace Gameplay.NPC
{
    /// <summary>
    /// 상점 NPC
    /// 아이템과 스킬을 판매하는 NPC
    /// 대화 시스템을 통해 상점 기능을 제공
    /// </summary>
    public class ShopNPC : NPCController
    {
        [Header("상점 NPC 설정")]
        [SerializeField] private bool alwaysUseSameEpisode = true;  // 항상 같은 에피소드 사용 (상점은 보통 고정)

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
            Log($"상점 대화 시작: {GetCurrentEpisodeID()}");
        }

        /// <summary>
        /// 대화 종료 시 호출
        /// </summary>
        private void HandleDialogueEnded()
        {
            if (!playerInRange) return; // 이 NPC와의 대화가 아니면 무시

            Log("상점 대화 종료");

            // 상점은 대화가 끝나도 에피소드를 진행하지 않음 (항상 같은 대화)
        }

        protected override string GetCurrentEpisodeID()
        {
            if (npcData == null || npcData.episodeIDs.Count == 0)
            {
                return null;
            }

            // 상점은 보통 첫 번째 에피소드만 사용
            if (alwaysUseSameEpisode)
            {
                return npcData.episodeIDs[0];
            }

            // 또는 일반 NPC처럼 진행
            return base.GetCurrentEpisodeID();
        }

        /// <summary>
        /// 아이템 구매 처리 (DialogueManager에서 호출됨)
        /// </summary>
        public void OnItemPurchased(string itemID, int price)
        {
            Log($"아이템 구매: {itemID} ({price}G)");

            // 추가 로직 (예: 구매 애니메이션, 사운드 등)
        }

        /// <summary>
        /// 스킬 구매 처리 (DialogueManager에서 호출됨)
        /// </summary>
        public void OnSkillPurchased(string skillID, int price)
        {
            Log($"스킬 구매: {skillID} ({price}G)");

            // 추가 로직 (예: 스킬 습득 이펙트 등)
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
