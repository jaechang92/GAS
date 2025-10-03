namespace Core.Enums
{
    /// <summary>
    /// 리소스 카테고리 분류
    /// 게임 흐름에 따라 단계별로 리소스를 로드하기 위한 분류
    /// </summary>
    public enum ResourceCategory
    {
        /// <summary>
        /// 필수 리소스 - 게임 시작 시 반드시 로드되어야 하는 리소스
        /// (매니저 설정, 기본 데이터 등)
        /// </summary>
        Essential,

        /// <summary>
        /// 메인 메뉴 리소스 - 메인 메뉴에서 사용되는 리소스
        /// (메뉴 UI, 메인 메뉴 BGM, 로고 등)
        /// </summary>
        MainMenu,

        /// <summary>
        /// 게임플레이 리소스 - 실제 게임 플레이에 사용되는 리소스
        /// (캐릭터, 적, 레벨, 게임플레이 사운드 등)
        /// </summary>
        Gameplay,

        /// <summary>
        /// 공통 리소스 - 여러 곳에서 사용되는 공통 리소스
        /// (공통 이펙트, 공통 사운드, UI 요소 등)
        /// </summary>
        Common
    }
}
