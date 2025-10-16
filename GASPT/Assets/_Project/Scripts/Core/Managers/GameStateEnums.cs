namespace GameFlow
{
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameStateType
    {
        Preload,    // 초기 리소스 로딩 (Essential + MainMenu)
        Main,       // 메인 메뉴
        Loading,    // 게임플레이 리소스 로딩 (Gameplay)
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
        StartGame,          // 게임 시작 (Main -> Lobby)
        EnterGameplay,      // Gameplay 진입 (Lobby -> Loading -> Ingame)
        LoadComplete,       // 로딩 완료
        PauseGame,          // 게임 일시정지
        ResumeGame,         // 게임 재개
        OpenMenu,           // 메뉴 열기
        CloseMenu,          // 메뉴 닫기
        GoToLobby,          // 로비로 이동
        GoToMain,           // 메인으로 이동
        GameEnd,            // 게임 종료
        OpenSettings,       // 설정 열기
        CloseSettings,      // 설정 닫기
        ReturnToMainMenu    // 메인 메뉴로 복귀
    }

    // SceneType은 Core.Enums.SceneType을 사용
    // 물리적 씬: Bootstrap, Preload, MainMenu, Game
}
