namespace Gameplay.NPC
{
    /// <summary>
    /// NPC 상호작용 인터페이스
    /// 플레이어가 NPC와 상호작용할 때 호출되는 메서드
    /// </summary>
    public interface INPCInteractable
    {
        /// <summary>
        /// 플레이어가 상호작용 범위에 진입했을 때
        /// </summary>
        void OnPlayerEnterRange();

        /// <summary>
        /// 플레이어가 상호작용 범위를 벗어났을 때
        /// </summary>
        void OnPlayerExitRange();

        /// <summary>
        /// 플레이어가 상호작용 키(E)를 눌렀을 때
        /// </summary>
        void OnInteract();

        /// <summary>
        /// 상호작용 가능한 상태인지
        /// </summary>
        bool CanInteract { get; }
    }
}
