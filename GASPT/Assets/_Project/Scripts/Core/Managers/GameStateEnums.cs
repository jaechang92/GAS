namespace GameFlow
{
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameStateType
    {
        Main,       // 메인 메뉴
        Loading,    // 로딩 화면
        Ingame,     // 인게임
        Pause,      // 일시정지
        Menu,       // 인게임 메뉴
        Lobby,      // 로비
        GameOver,   // 게임오버
        Settings    // 설정
    }

    /// <summary>
    /// 게임 이벤트 타입
    /// </summary>
    public enum GameEventType
    {
        StartGame,      // 게임 시작
        LoadComplete,   // 로딩 완료
        PauseGame,      // 게임 일시정지
        ResumeGame,     // 게임 재개
        OpenMenu,       // 메뉴 열기
        CloseMenu,      // 메뉴 닫기
        GoToLobby,      // 로비로 이동
        GoToMain,       // 메인으로 이동
        GameEnd,        // 게임 종료
        OpenSettings,   // 설정 열기
        CloseSettings   // 설정 닫기
    }

    /// <summary>
    /// 씬 타입
    /// </summary>
    public enum SceneType
    {
        MainMenu,
        GameScene,
        LoadingScene
    }
}