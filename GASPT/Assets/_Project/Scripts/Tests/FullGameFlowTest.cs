using UnityEngine;
using GameFlow;
using Core.Managers;
using UI.Menu;

namespace Tests
{
    /// <summary>
    /// 전체 게임 흐름 통합 테스트
    /// Bootstrap → Preload → Main → Loading → Gameplay 전체 흐름 검증
    /// </summary>
    public class FullGameFlowTest : MonoBehaviour
    {
        [Header("자동 설정")]
        [SerializeField] private bool autoSetup = true;

        private void Start()
        {
            if (autoSetup)
            {
                SetupFullGameFlow();
            }
        }

        /// <summary>
        /// 전체 게임 흐름 설정
        /// </summary>
        public void SetupFullGameFlow()
        {
            Debug.Log("[FullGameFlowTest] 전체 게임 흐름 테스트 시작");

            // 1. GameFlowManager 생성 (DontDestroyOnLoad)
            SetupGameFlowManager();

            // 2. SceneLoader 생성 (DontDestroyOnLoad)
            SetupSceneLoader();

            // 3. SceneTransitionManager 생성 (DontDestroyOnLoad)
            SetupSceneTransitionManager();

            // 4. Main 씬 UI 생성
            SetupMainMenuUI();

            // 5. Loading 씬 UI 생성 (필요시)
            // Loading UI는 Loading 씬에서 자동 생성됨

            // 6. Gameplay 씬 Manager 설정은 Gameplay 씬에서 자동 처리

            Debug.Log("[FullGameFlowTest] 전체 게임 흐름 설정 완료");
            Debug.Log("[FullGameFlowTest] 게임 시작 버튼을 클릭하면 Gameplay 씬으로 이동합니다.");
        }

        /// <summary>
        /// GameFlowManager 생성
        /// </summary>
        private void SetupGameFlowManager()
        {
            if (GameFlowManager.Instance != null)
            {
                Debug.Log("[FullGameFlowTest] GameFlowManager 이미 존재함");
                return;
            }

            GameObject gfmObject = new GameObject("GameFlowManager");
            gfmObject.AddComponent<GameFlowManager>();
            Debug.Log("[FullGameFlowTest] GameFlowManager 생성 완료");
        }

        /// <summary>
        /// SceneLoader 생성
        /// </summary>
        private void SetupSceneLoader()
        {
            if (SceneLoader.Instance != null)
            {
                Debug.Log("[FullGameFlowTest] SceneLoader 이미 존재함");
                return;
            }

            GameObject slObject = new GameObject("SceneLoader");
            slObject.AddComponent<SceneLoader>();
            Debug.Log("[FullGameFlowTest] SceneLoader 생성 완료");
        }

        /// <summary>
        /// SceneTransitionManager 생성
        /// </summary>
        private void SetupSceneTransitionManager()
        {
            if (SceneTransitionManager.Instance != null)
            {
                Debug.Log("[FullGameFlowTest] SceneTransitionManager 이미 존재함");
                return;
            }

            GameObject stmObject = new GameObject("SceneTransitionManager");
            stmObject.AddComponent<SceneTransitionManager>();
            Debug.Log("[FullGameFlowTest] SceneTransitionManager 생성 완료");
        }

        /// <summary>
        /// Main 메뉴 UI 생성
        /// </summary>
        private void SetupMainMenuUI()
        {
            // 이미 존재하는지 확인
            var existingUI = FindAnyObjectByType<MainMenuUI>();
            if (existingUI != null)
            {
                Debug.Log("[FullGameFlowTest] MainMenuUI 이미 존재함");
                return;
            }

            GameObject uiObject = new GameObject("MainMenuUI");
            uiObject.AddComponent<MainMenuUI>();
            Debug.Log("[FullGameFlowTest] MainMenuUI 생성 완료");
        }
    }
}
