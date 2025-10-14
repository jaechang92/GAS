namespace Core.Enums
{
    /// <summary>
    /// 게임 내 물리적 씬 타입 정의
    /// Build Settings의 씬 순서와 일치해야 함
    ///
    /// 주의: Loading과 Pause는 씬이 아닌 GameState임
    /// - Loading State: 씬 전환 중 로딩 UI 표시
    /// - Pause State: 게임 일시정지 및 Pause UI 표시
    /// </summary>
    public enum SceneType
    {
        /// <summary>
        /// 게임 진입점 - 핵심 매니저 초기화
        /// Build Index: 0
        /// </summary>
        Bootstrap = 0,

        /// <summary>
        /// 초기 리소스 로딩 (Essential + MainMenu)
        /// Build Index: 1
        /// </summary>
        Preload = 1,

        /// <summary>
        /// 메인 메뉴 (MainMenu 씬)
        /// Build Index: 2
        /// </summary>
        MainMenu = 2,

        /// <summary>
        /// 게임플레이 (Gameplay 씬)
        /// Build Index: 3
        /// </summary>
        Game = 3
    }
}
