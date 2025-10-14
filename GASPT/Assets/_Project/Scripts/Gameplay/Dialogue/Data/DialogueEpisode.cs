using System;
using System.Collections.Generic;

namespace Gameplay.Dialogue
{
    /// <summary>
    /// 대화 에피소드 데이터
    /// 하나의 완전한 대화 흐름을 나타냄
    /// </summary>
    [Serializable]
    public class DialogueEpisode
    {
        public string episodeID;                            // 에피소드 ID (예: EP_STORY_001)
        public string episodeName;                          // 에피소드 이름
        public string npcName;                              // NPC 이름
        public EpisodeType episodeType;                     // 에피소드 타입
        public string description;                          // 설명

        public List<DialogueNode> nodes;                    // 대화 노드 리스트
        public Dictionary<int, List<DialogueChoice>> choices; // 노드별 선택지 딕셔너리

        public DialogueEpisode(string episodeID, string episodeName, string npcName, EpisodeType episodeType, string description)
        {
            this.episodeID = episodeID;
            this.episodeName = episodeName;
            this.npcName = npcName;
            this.episodeType = episodeType;
            this.description = description;

            nodes = new List<DialogueNode>();
            choices = new Dictionary<int, List<DialogueChoice>>();
        }

        /// <summary>
        /// 노드 추가
        /// </summary>
        public void AddNode(DialogueNode node)
        {
            if (node.episodeID != episodeID)
            {
                UnityEngine.Debug.LogError($"[DialogueEpisode] 노드의 에피소드 ID가 일치하지 않습니다: {node.episodeID} != {episodeID}");
                return;
            }

            nodes.Add(node);
        }

        /// <summary>
        /// 선택지 추가
        /// </summary>
        public void AddChoice(DialogueChoice choice)
        {
            if (choice.episodeID != episodeID)
            {
                UnityEngine.Debug.LogError($"[DialogueEpisode] 선택지의 에피소드 ID가 일치하지 않습니다: {choice.episodeID} != {episodeID}");
                return;
            }

            if (!choices.ContainsKey(choice.nodeID))
            {
                choices[choice.nodeID] = new List<DialogueChoice>();
            }

            choices[choice.nodeID].Add(choice);
        }

        /// <summary>
        /// 특정 노드 가져오기
        /// </summary>
        public DialogueNode GetNode(int nodeID)
        {
            return nodes.Find(n => n.nodeID == nodeID);
        }

        /// <summary>
        /// 특정 노드의 선택지 가져오기
        /// </summary>
        public List<DialogueChoice> GetChoices(int nodeID)
        {
            if (choices.ContainsKey(nodeID))
            {
                return choices[nodeID];
            }
            return new List<DialogueChoice>();
        }

        /// <summary>
        /// 시작 노드 가져오기 (보통 nodeID=1)
        /// </summary>
        public DialogueNode GetStartNode()
        {
            return GetNode(1);
        }

        public override string ToString()
        {
            return $"[{episodeID}] {episodeName} ({episodeType}) - Nodes: {nodes.Count}, Choices: {choices.Count}";
        }
    }
}
