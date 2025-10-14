namespace UI.Core
{
    /// <summary>
    /// UI 레이어 정의
    /// Canvas의 SortingOrder를 결정하며 UI의 렌더링 순서를 제어
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 배경 레이어 (sortingOrder: 0)
        /// 전체 화면을 차지하는 배경 UI
        /// 예: MainMenu, Lobby
        /// </summary>
        Background = 0,

        /// <summary>
        /// 일반 레이어 (sortingOrder: 100)
        /// 일반적인 게임플레이 UI
        /// 예: GameplayHUD, Inventory, Shop
        /// </summary>
        Normal = 100,

        /// <summary>
        /// 팝업 레이어 (sortingOrder: 200)
        /// 다른 UI 위에 표시되는 팝업
        /// 예: Pause, Settings, Dialog
        /// </summary>
        Popup = 200,

        /// <summary>
        /// 시스템 레이어 (sortingOrder: 300)
        /// 시스템 메시지 및 로딩
        /// 예: Loading, Toast
        /// </summary>
        System = 300,

        /// <summary>
        /// 전환 레이어 (sortingOrder: 9999)
        /// 화면 전환 효과 (최상위)
        /// 예: Fade, Transition
        /// </summary>
        Transition = 9999
    }
}
