using UnityEngine;

namespace Gameplay.Dialogue
{
    /// <summary>
    /// 대화 이벤트 리스너 인터페이스
    /// DialoguePanel이나 다른 UI가 구현하여 대화 이벤트를 수신
    /// </summary>
    public interface IDialogueListener
    {
        /// <summary>
        /// 대화 시작 시 호출
        /// </summary>
        void OnDialogueStart(string episodeID);

        /// <summary>
        /// 대화 노드 변경 시 호출
        /// </summary>
        Awaitable OnDialogueNodeChanged(DialogueNode node);

        /// <summary>
        /// 선택지 표시 시 호출
        /// </summary>
        void OnDialogueChoicesShown(DialogueNode node, System.Collections.Generic.List<DialogueChoice> choices);

        /// <summary>
        /// 대화 종료 시 호출
        /// </summary>
        void OnDialogueEnd();
    }
}
