using UnityEngine;
using Managers;

namespace Scenes.Bootstrap
{
    /// <summary>
    /// 게임 시작 시 시스템 초기화를 담당하는 매니저
    /// </summary>
    public class BootstrapManager : MonoBehaviour
    {
        [Header("초기화 설정")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float initializationDelay = 1f;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = true;

        private async void Start()
        {
            await InitializeGame();
        }

        /// <summary>
        /// 게임 전체 시스템 초기화
        /// </summary>
        private async Awaitable InitializeGame()
        {
            LogDebug("=== 게임 시스템 초기화 시작 ===");

            // 1. 핵심 매니저들 초기화
            await InitializeManagers();

            // 2. 게임 설정 로드
            await LoadGameSettings();

            // 3. 시스템 검증
            await ValidateSystems();

            // 4. 초기화 완료 대기
            await Awaitable.WaitForSecondsAsync(initializationDelay);

            // 5. 메인 메뉴로 전환
            if (autoStartGame)
            {
                await TransitionToMainMenu();
            }

            LogDebug("=== 게임 시스템 초기화 완료 ===");
        }

        /// <summary>
        /// 매니저들 초기화
        /// </summary>
        private async Awaitable InitializeManagers()
        {
            LogDebug("매니저들 초기화 중...");

            // GameManager 초기화
            var gameManager = GameManager.Instance;
            LogDebug($"GameManager 초기화: {gameManager != null}");

            // AudioManager 초기화
            var audioManager = AudioManager.Instance;
            LogDebug($"AudioManager 초기화: {audioManager != null}");

            // UIManager 초기화
            var uiManager = UIManager.Instance;
            LogDebug($"UIManager 초기화: {uiManager != null}");

            // SceneManager 초기화
            var sceneManager = SceneManager.Instance;
            LogDebug($"SceneManager 초기화: {sceneManager != null}");

            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 게임 설정 로드
        /// </summary>
        private async Awaitable LoadGameSettings()
        {
            LogDebug("게임 설정 로드 중...");

            // 오디오 설정 로드
            LoadAudioSettings();

            // 그래픽 설정 로드
            LoadGraphicsSettings();

            // 게임플레이 설정 로드
            LoadGameplaySettings();

            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 오디오 설정 로드
        /// </summary>
        private void LoadAudioSettings()
        {
            if (!AudioManager.TryGetInstance(out var audioManager)) return;

            // PlayerPrefs에서 오디오 설정 로드
            float masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1.0f);
            float musicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", 0.7f);
            float sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1.0f);

            audioManager.MasterVolume = masterVolume;
            audioManager.MusicVolume = musicVolume;
            audioManager.SFXVolume = sfxVolume;

            LogDebug($"오디오 설정 로드: Master={masterVolume}, Music={musicVolume}, SFX={sfxVolume}");
        }

        /// <summary>
        /// 그래픽 설정 로드
        /// </summary>
        private void LoadGraphicsSettings()
        {
            // 해상도 설정
            int screenWidth = PlayerPrefs.GetInt("Graphics_ScreenWidth", Screen.currentResolution.width);
            int screenHeight = PlayerPrefs.GetInt("Graphics_ScreenHeight", Screen.currentResolution.height);
            bool fullscreen = PlayerPrefs.GetInt("Graphics_Fullscreen", 1) == 1;

            if (screenWidth != Screen.width || screenHeight != Screen.height)
            {
                Screen.SetResolution(screenWidth, screenHeight, fullscreen);
            }

            // 품질 설정
            int qualityLevel = PlayerPrefs.GetInt("Graphics_QualityLevel", QualitySettings.GetQualityLevel());
            QualitySettings.SetQualityLevel(qualityLevel);

            LogDebug($"그래픽 설정 로드: {screenWidth}x{screenHeight}, 풀스크린={fullscreen}, 품질={qualityLevel}");
        }

        /// <summary>
        /// 게임플레이 설정 로드
        /// </summary>
        private void LoadGameplaySettings()
        {
            if (!GameManager.TryGetInstance(out var gameManager)) return;

            // 게임 난이도 등 설정 로드
            // int difficulty = PlayerPrefs.GetInt("Gameplay_Difficulty", 1);

            LogDebug("게임플레이 설정 로드 완료");
        }

        /// <summary>
        /// 시스템 검증
        /// </summary>
        private async Awaitable ValidateSystems()
        {
            LogDebug("시스템 검증 중...");

            bool allSystemsValid = true;

            // 매니저들 검증
            allSystemsValid &= GameManager.HasInstance;
            allSystemsValid &= AudioManager.HasInstance;
            allSystemsValid &= UIManager.HasInstance;
            allSystemsValid &= SceneManager.HasInstance;

            if (!allSystemsValid)
            {
                Debug.LogError("[Bootstrap] 일부 시스템 초기화 실패!");
            }

            LogDebug($"시스템 검증 결과: {(allSystemsValid ? "성공" : "실패")}");

            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 메인 메뉴로 전환
        /// </summary>
        private async Awaitable TransitionToMainMenu()
        {
            LogDebug("메인 메뉴로 전환 중...");

            if (SceneManager.TryGetInstance(out var sceneManager))
            {
                await sceneManager.LoadScene(SceneType.MainMenu);
            }
            else
            {
                Debug.LogError("[Bootstrap] SceneManager를 찾을 수 없어 수동으로 씬을 로드합니다.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("01_MainMenu");
            }
        }

        /// <summary>
        /// 디버그 로그 출력
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[Bootstrap] {message}");
            }
        }

        // 컨텍스트 메뉴 테스트
        [ContextMenu("시스템 재초기화")]
        private void ReinitializeSystems()
        {
            _ = InitializeGame();
        }

        [ContextMenu("메인 메뉴로 이동")]
        private void GoToMainMenu()
        {
            _ = TransitionToMainMenu();
        }

        [ContextMenu("현재 시스템 상태 확인")]
        private void CheckSystemStatus()
        {
            Debug.Log("=== 시스템 상태 ===");
            Debug.Log($"GameManager: {GameManager.HasInstance}");
            Debug.Log($"AudioManager: {AudioManager.HasInstance}");
            Debug.Log($"UIManager: {UIManager.HasInstance}");
            Debug.Log($"SceneManager: {SceneManager.HasInstance}");
        }
    }
}