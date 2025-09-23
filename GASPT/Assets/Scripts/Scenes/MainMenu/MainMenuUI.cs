using UnityEngine;
using UnityEngine.UI;
using Managers;

namespace Scenes.MainMenu
{
    /// <summary>
    /// 메인 메뉴 UI 관리
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI 버튼들")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("UI 요소들")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text versionText;

        [Header("오디오")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip backgroundMusic;

        private void Start()
        {
            InitializeUI();
            PlayBackgroundMusic();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(OnLevelSelectClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // 텍스트 설정
            if (titleText != null)
                titleText.text = "GASPT 플랫포머";

            if (versionText != null)
                versionText.text = $"Version {Application.version}";

            Debug.Log("[MainMenuUI] 메인 메뉴 UI 초기화 완료");
        }

        /// <summary>
        /// 배경 음악 재생
        /// </summary>
        private void PlayBackgroundMusic()
        {
            if (backgroundMusic != null && AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayMusic(backgroundMusic);
            }
        }

        /// <summary>
        /// 게임 시작 버튼 클릭
        /// </summary>
        private void OnStartGameClicked()
        {
            PlayButtonSound();
            Debug.Log("[MainMenuUI] 게임 시작 버튼 클릭");

            // 첫 번째 레벨로 바로 이동
            _ = StartGameplay();
        }

        /// <summary>
        /// 레벨 선택 버튼 클릭
        /// </summary>
        private void OnLevelSelectClicked()
        {
            PlayButtonSound();
            Debug.Log("[MainMenuUI] 레벨 선택 버튼 클릭");

            // 레벨 선택 씬으로 이동
            _ = GoToLevelSelect();
        }

        /// <summary>
        /// 설정 버튼 클릭
        /// </summary>
        private void OnSettingsClicked()
        {
            PlayButtonSound();
            Debug.Log("[MainMenuUI] 설정 버튼 클릭");

            // 설정 씬으로 이동
            _ = GoToSettings();
        }

        /// <summary>
        /// 크레딧 버튼 클릭
        /// </summary>
        private void OnCreditsClicked()
        {
            PlayButtonSound();
            Debug.Log("[MainMenuUI] 크레딧 버튼 클릭");

            // TODO: 크레딧 팝업 또는 씬 표시
            ShowCreditsPopup();
        }

        /// <summary>
        /// 종료 버튼 클릭
        /// </summary>
        private void OnQuitClicked()
        {
            PlayButtonSound();
            Debug.Log("[MainMenuUI] 종료 버튼 클릭");

            // 게임 종료 확인 대화상자
            ShowQuitConfirmation();
        }

        /// <summary>
        /// 게임플레이 시작
        /// </summary>
        private async Awaitable StartGameplay()
        {
            if (SceneManager.TryGetInstance(out var sceneManager))
            {
                await sceneManager.StartGameplay();
            }
        }

        /// <summary>
        /// 레벨 선택으로 이동
        /// </summary>
        private async Awaitable GoToLevelSelect()
        {
            if (SceneManager.TryGetInstance(out var sceneManager))
            {
                await sceneManager.LoadScene(SceneType.LevelSelect);
            }
        }

        /// <summary>
        /// 설정으로 이동
        /// </summary>
        private async Awaitable GoToSettings()
        {
            if (SceneManager.TryGetInstance(out var sceneManager))
            {
                await sceneManager.OpenSettings();
            }
        }

        /// <summary>
        /// 크레딧 팝업 표시
        /// </summary>
        private void ShowCreditsPopup()
        {
            // TODO: 크레딧 팝업 UI 구현
            Debug.Log("크레딧 정보를 표시합니다.");
        }

        /// <summary>
        /// 종료 확인 대화상자
        /// </summary>
        private void ShowQuitConfirmation()
        {
            // TODO: 확인 대화상자 구현
            Debug.Log("게임을 종료하시겠습니까?");

            // 임시로 바로 종료
            QuitGame();
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 버튼 클릭 사운드 재생
        /// </summary>
        private void PlayButtonSound()
        {
            if (buttonClickSound != null && AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlaySFX(buttonClickSound);
            }
        }

        // 컨텍스트 메뉴 테스트
        [ContextMenu("게임 시작 테스트")]
        private void TestStartGame()
        {
            OnStartGameClicked();
        }

        [ContextMenu("레벨 선택 테스트")]
        private void TestLevelSelect()
        {
            OnLevelSelectClicked();
        }

        [ContextMenu("설정 테스트")]
        private void TestSettings()
        {
            OnSettingsClicked();
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 정리
            if (startGameButton != null)
                startGameButton.onClick.RemoveAllListeners();

            if (levelSelectButton != null)
                levelSelectButton.onClick.RemoveAllListeners();

            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();

            if (creditsButton != null)
                creditsButton.onClick.RemoveAllListeners();

            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
    }
}