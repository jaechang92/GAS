using System;

namespace Gameplay.Dialogue
{
    /// <summary>
    /// 대화 노드 데이터
    /// CSV의 한 행에 해당
    /// </summary>
    [Serializable]
    public class DialogueNode
    {
        public string episodeID;        // 에피소드 ID (예: EP_STORY_001)
        public int nodeID;              // 노드 ID (해당 에피소드 내 순서)
        public string speakerName;      // 화자 이름
        public string dialogueText;     // 대화 텍스트
        public int nextNodeID;          // 다음 노드 ID (0이면 종료)
        public bool hasChoices;         // 선택지가 있는지

        public DialogueNode(string episodeID, int nodeID, string speakerName, string dialogueText, int nextNodeID, bool hasChoices)
        {
            this.episodeID = episodeID;
            this.nodeID = nodeID;
            this.speakerName = speakerName;
            this.dialogueText = dialogueText;
            this.nextNodeID = nextNodeID;
            this.hasChoices = hasChoices;
        }

        public override string ToString()
        {
            return $"[{episodeID}:{nodeID}] {speakerName}: {dialogueText}";
        }
    }
}
