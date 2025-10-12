namespace Core.Enums
{
    /// <summary>
    /// 게임 내 씬 타입 정의
    /// Build Settings의 씬 순서와 일치해야 함
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
        /// 메인 메뉴 및 로비
        /// Build Index: 2
        /// </summary>
        Main = 2,

        /// <summary>
        /// 로딩 화면 (Additive 오버레이)
        /// Build Index: 3
        /// </summary>
        Loading = 3,

        /// <summary>
        /// 실제 게임플레이
        /// Build Index: 4
        /// </summary>
        Gameplay = 4,

        /// <summary>
        /// 일시정지 메뉴 (Additive)
        /// Build Index: 5
        /// </summary>
        Pause = 5
    }
}
