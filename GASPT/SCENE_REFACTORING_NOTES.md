# Scene/State 리팩토링 완료 노트

## 변경 사항

### 1. SceneType 재정의 (물리적 씬만)

**변경 전:**
```csharp
public enum SceneType
{
    Bootstrap, Preload, Main, Loading, Gameplay, Pause
}
```

**변경 후:**
```csharp
public enum SceneType
{
    Bootstrap,  // 게임 진입점
    Preload,    // 초기 리소스 로딩
    MainMenu,   // 메인 메뉴
    Game        // 게임플레이
}
```

**제거된 항목:**
- `Loading` → GameStateType.Loading으로 논리적 상태로만 존재
- `Pause` → GameStateType.Pause로 논리적 상태로만 존재

### 2. GameStateType (논리적 상태)

**유지 (변경 없음):**
```csharp
public enum GameStateType
{
    Preload,   // 초기 리소스 로딩 중
    Main,      // 메인 메뉴
    Loading,   // 게임 로딩 중 (씬은 Game)
    Ingame,    // 게임플레이 중
    Pause,     // 일시정지
    Menu,      // 인게임 메뉴
    Lobby,     // 로비
    GameOver,  // 게임오버
    Settings   // 설정
}
```

### 3. 씬-상태 매핑 테이블

| GameState | 로드되는 SceneType | 설명 |
|-----------|-------------------|------|
| Preload   | Preload           | 초기 리소스 로딩 |
| Main      | MainMenu          | 메인 메뉴 화면 |
| Loading   | (씬 로드 없음)     | 로딩 UI만 표시 |
| Ingame    | Game              | 게임플레이 씬 |
| Pause     | Game (유지)       | 현재 씬 유지, Pause UI 표시 |

### 4. 씬 파일명 매핑 (임시)

SceneLoader에 매핑 메서드 추가:

```csharp
private string GetSceneName(SceneType sceneType)
{
    return sceneType switch
    {
        SceneType.MainMenu => "Main",      // Main.unity
        SceneType.Game => "Gameplay",      // Gameplay.unity
        _ => sceneType.ToString()
    };
}
```

## TODO: 씬 파일명 변경 (Unity Editor에서 수행)

추후 Unity Editor에서 씬 파일명을 enum과 일치시키기:

1. **Main.unity → MainMenu.unity**
   - Unity에서 씬 파일 이름 변경
   - Build Settings에서 경로 업데이트

2. **Gameplay.unity → Game.unity**
   - Unity에서 씬 파일 이름 변경
   - Build Settings에서 경로 업데이트

3. **매핑 메서드 제거**
   - SceneLoader.cs의 GetSceneName() 제거
   - sceneType.ToString()으로 직접 사용

## 효과

### Before
- 씬(SceneType)과 상태(GameStateType)가 혼용
- Loading.unity, Pause.unity가 실제로는 사용되지 않음
- 물리적 씬과 논리적 상태 구분 불명확

### After
- **물리적 씬**: Bootstrap, Preload, MainMenu, Game
- **논리적 상태**: Preload, Main, Loading, Ingame, Pause, Menu, Lobby, GameOver, Settings
- Loading/Pause는 UI 상태로만 존재
- 명확한 책임 분리

## 참고

- 기존 GameState.cs 구현은 변경 없음 (사용자 요청)
- GameFlow.SceneType 중복 enum 제거됨
- Core.Enums.SceneType을 프로젝트 전역에서 사용
