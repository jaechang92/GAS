using System;

namespace Gameplay.Dialogue
{
    /// <summary>
    /// 대화 선택지 데이터
    /// CSV의 한 행에 해당
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        public string episodeID;        // 에피소드 ID
        public int nodeID;              // 선택지가 표시되는 노드 ID
        public int choiceID;            // 선택지 ID (해당 노드 내 순서)
        public string choiceText;       // 선택지 텍스트
        public int nextNodeID;          // 선택 시 이동할 노드 ID (999면 종료)
        public int requiredGold;        // 필요한 골드 (상점용)
        public string rewardItem;       // 보상 아이템 ID (상점용)

        public DialogueChoice(string episodeID, int nodeID, int choiceID, string choiceText, int nextNodeID, int requiredGold, string rewardItem)
        {
            this.episodeID = episodeID;
            this.nodeID = nodeID;
            this.choiceID = choiceID;
            this.choiceText = choiceText;
            this.nextNodeID = nextNodeID;
            this.requiredGold = requiredGold;
            this.rewardItem = rewardItem;
        }

        /// <summary>
        /// 상점 아이템인지 확인
        /// </summary>
        public bool IsShopItem => requiredGold > 0;

        /// <summary>
        /// 종료 선택지인지 확인
        /// </summary>
        public bool IsExitChoice => nextNodeID == 999;

        public override string ToString()
        {
            return $"[{episodeID}:{nodeID}:{choiceID}] {choiceText} → Node {nextNodeID}";
        }
    }
}
