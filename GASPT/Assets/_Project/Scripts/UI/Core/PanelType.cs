namespace UI.Core
{
    /// <summary>
    /// UI Panel 타입 정의
    /// 각 Panel을 고유하게 식별하기 위한 Enum
    /// </summary>
    public enum PanelType
    {
        None,

        // === Background Layer (sortingOrder: 0) ===
        MainMenu,           // 메인 메뉴
        Lobby,              // 로비 화면

        // === Normal Layer (sortingOrder: 100) ===
        GameplayHUD,        // 게임플레이 HUD
        Inventory,          // 인벤토리
        Shop,               // 상점
        CharacterStatus,    // 캐릭터 정보

        // === Popup Layer (sortingOrder: 200) ===
        Pause,              // 일시정지
        Settings,           // 설정
        Dialog,             // 대화창
        Reward,             // 보상 팝업
        Confirm,            // 확인 대화상자

        // === System Layer (sortingOrder: 300) ===
        Loading,            // 로딩 화면
        Toast,              // 토스트 메시지

        // === Transition Layer (sortingOrder: 9999) ===
        Fade                // 페이드 전환
    }
}
