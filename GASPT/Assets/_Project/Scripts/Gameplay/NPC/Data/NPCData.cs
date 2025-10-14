using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.NPC
{
    /// <summary>
    /// NPC 설정 데이터 (ScriptableObject)
    /// Inspector에서 설정 가능
    /// </summary>
    [CreateAssetMenu(fileName = "NPCData", menuName = "GASPT/NPC/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Header("기본 정보")]
        public string npcName = "NPC";
        public NPCType npcType = NPCType.Story;
        public Sprite npcSprite;

        [Header("에피소드 설정")]
        [Tooltip("이 NPC가 재생할 에피소드 ID 목록 (순서대로 진행)")]
        public List<string> episodeIDs = new List<string>();

        [Header("상호작용 설정")]
        public float interactionRange = 2f;
        public KeyCode interactionKey = KeyCode.E;

        [Header("UI 설정")]
        public bool showInteractionPrompt = true;
        public string interactionPromptText = "E를 눌러 대화하기";
    }
}
