using UnityEngine;

namespace Scenes
{
    /// <summary>
    /// Unity 에디터에서 씬을 생성하고 설정하는 가이드
    /// 이 스크립트는 실제 씬 파일을 생성하지는 않지만,
    /// 각 씬에 필요한 오브젝트와 컴포넌트 설정을 도와줍니다.
    /// </summary>
    public class SceneCreationGuide : MonoBehaviour
    {
        [Header("씬 생성 가이드")]
        [TextArea(10, 20)]
        [SerializeField] private string creationSteps = @"
=== Unity 에디터에서 씬 생성 방법 ===

1. Bootstrap Scene (00_Bootstrap.unity)
   - 새 씬 생성: File > New Scene > Basic
   - 씬 저장: Assets/Scenes/00_Bootstrap.unity
   - GameObject 생성:
     * SystemBootstrap (Empty GameObject)
       - BootstrapManager 컴포넌트 추가

2. MainMenu Scene (01_MainMenu.unity)
   - 새 씬 생성
   - UI Canvas 생성:
     * Canvas (UI > Canvas)
       - Render Mode: Screen Space - Overlay
     * 자식으로 UI 요소들 추가:
       - Title Text
       - Start Game Button
       - Level Select Button
       - Settings Button
       - Credits Button
       - Quit Button
   - GameObject 생성:
     * MainMenuManager (Empty GameObject)
       - MainMenuUI 컴포넌트 추가

3. LevelSelect Scene (02_LevelSelect.unity)
   - UI Canvas + 레벨 선택 버튼들
   - LevelSelectManager 오브젝트

4. Gameplay Scene (03_Gameplay.unity)
   - Player GameObject
   - Level Environment
   - Camera
   - UI Canvas (게임 UI)
   - LevelManager 오브젝트

5. 나머지 씬들도 동일한 방식으로 생성

=== 자동 설정 기능 ===
- 이 컴포넌트의 컨텍스트 메뉴 사용
- 각 씬별로 필요한 기본 오브젝트 생성 가능
";

        [ContextMenu("Bootstrap 씬 설정")]
        public void CreateBootstrapScene()
        {
            Debug.Log("Bootstrap 씬 설정을 시작합니다...");

            // Bootstrap 관련 오브젝트 생성
            GameObject bootstrapGO = CreateBasicGameObject("SystemBootstrap");

            var bootstrapManager = bootstrapGO.AddComponent<Bootstrap.BootstrapManager>();

            Debug.Log("Bootstrap 씬 기본 설정 완료");
            Debug.Log("- SystemBootstrap 오브젝트 생성됨");
            Debug.Log("- BootstrapManager 컴포넌트 추가됨");
        }

        [ContextMenu("MainMenu 씬 설정")]
        public void CreateMainMenuScene()
        {
            Debug.Log("MainMenu 씬 설정을 시작합니다...");

            // Canvas 생성
            GameObject canvasGO = CreateUICanvas("MainMenuCanvas");

            // MainMenu Manager 생성
            GameObject managerGO = CreateBasicGameObject("MainMenuManager");
            var mainMenuUI = managerGO.AddComponent<MainMenu.MainMenuUI>();

            // 기본 UI 버튼들 생성
            CreateMainMenuButtons(canvasGO);

            Debug.Log("MainMenu 씬 기본 설정 완료");
            Debug.Log("- UI Canvas 생성됨");
            Debug.Log("- MainMenuManager 오브젝트 생성됨");
            Debug.Log("- 기본 UI 버튼들 생성됨");
        }

        [ContextMenu("Gameplay 씬 설정")]
        public void CreateGameplayScene()
        {
            Debug.Log("Gameplay 씬 설정을 시작합니다...");

            // Player 오브젝트 생성
            GameObject playerGO = CreateBasicGameObject("Player");
            playerGO.transform.position = Vector3.zero;

            // Rigidbody2D와 Collider2D 추가
            var rb = playerGO.AddComponent<Rigidbody2D>();
            var collider = playerGO.AddComponent<BoxCollider2D>();

            // 기본 Ground 플랫폼 생성
            CreateBasicPlatform("Ground", new Vector3(0, -2, 0), new Vector3(10, 1, 1));

            // 게임플레이 UI Canvas
            GameObject gameplayCanvas = CreateUICanvas("GameplayCanvas");

            Debug.Log("Gameplay 씬 기본 설정 완료");
            Debug.Log("- Player 오브젝트 생성됨 (Rigidbody2D, BoxCollider2D 포함)");
            Debug.Log("- 기본 Ground 플랫폼 생성됨");
            Debug.Log("- Gameplay UI Canvas 생성됨");
        }

        [ContextMenu("모든 필수 매니저 생성")]
        public void CreateAllManagers()
        {
            Debug.Log("모든 필수 매니저를 생성합니다...");

            // 각 매니저들이 이미 존재하는지 확인 후 생성
            CreateManagerIfNotExists<Managers.GameManager>("GameManager");
            CreateManagerIfNotExists<Managers.AudioManager>("AudioManager");
            CreateManagerIfNotExists<Managers.UIManager>("UIManager");
            CreateManagerIfNotExists<SceneManager>("SceneManager");

            Debug.Log("모든 매니저 생성 완료");
        }

        /// <summary>
        /// 기본 게임오브젝트 생성
        /// </summary>
        private GameObject CreateBasicGameObject(string name)
        {
            GameObject go = new GameObject(name);
            return go;
        }

        /// <summary>
        /// UI Canvas 생성
        /// </summary>
        private GameObject CreateUICanvas(string name)
        {
            GameObject canvasGO = new GameObject(name);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            return canvasGO;
        }

        /// <summary>
        /// 메인메뉴 기본 버튼들 생성
        /// </summary>
        private void CreateMainMenuButtons(GameObject canvas)
        {
            string[] buttonNames = { "StartGameButton", "LevelSelectButton", "SettingsButton", "CreditsButton", "QuitButton" };

            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject buttonGO = new GameObject(buttonNames[i]);
                buttonGO.transform.SetParent(canvas.transform, false);

                var rectTransform = buttonGO.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 50);
                rectTransform.anchoredPosition = new Vector2(0, 100 - i * 60);

                var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
                var image = buttonGO.AddComponent<UnityEngine.UI.Image>();

                // 버튼 텍스트
                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform, false);

                var textRect = textGO.AddComponent<RectTransform>();
                textRect.sizeDelta = Vector2.zero;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;

                var text = textGO.AddComponent<UnityEngine.UI.Text>();
                text.text = buttonNames[i].Replace("Button", "");
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 14;
                text.color = Color.black;
                text.alignment = TextAnchor.MiddleCenter;
            }
        }

        /// <summary>
        /// 기본 플랫폼 생성
        /// </summary>
        private GameObject CreateBasicPlatform(string name, Vector3 position, Vector3 scale)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.transform.position = position;
            platform.transform.localScale = scale;

            // 플랫폼 태그 설정
            platform.tag = "Ground";

            return platform;
        }

        /// <summary>
        /// 매니저가 존재하지 않으면 생성
        /// </summary>
        private void CreateManagerIfNotExists<T>(string name) where T : Core.Singleton<T>
        {
            if (!Core.Singleton<T>.HasInstance)
            {
                GameObject managerGO = CreateBasicGameObject(name);
                managerGO.AddComponent<T>();
                Debug.Log($"- {name} 생성됨");
            }
            else
            {
                Debug.Log($"- {name} 이미 존재함");
            }
        }
    }
}