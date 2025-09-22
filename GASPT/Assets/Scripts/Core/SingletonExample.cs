using Managers;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Singleton 패턴 사용 예제 및 테스트 코드
    /// 실제 매니저들은 별도 파일로 분리됨
    /// </summary>
    public class SingletonUsageDemo : MonoBehaviour
    {
        [Header("데모 설정")]
        [SerializeField] private bool runDemoOnStart = false;

        private void Start()
        {
            if (runDemoOnStart)
            {
                RunSingletonDemo();
            }
        }

        private void RunSingletonDemo()
        {
            Debug.Log("=== Singleton 패턴 사용 데모 ===");

            // 1. 기본 사용법 데모
            DemoBasicUsage();

            // 2. 안전한 접근 방법 데모
            DemoSafeAccess();

            Debug.Log("=== 데모 완료 ===");
        }

        private void DemoBasicUsage()
        {
            Debug.Log("--- 기본 사용법 ---");

            // 일반적인 싱글톤 접근
            var gameManager = Managers.GameManager.Instance;
            gameManager.AddScore(100);

            var audioManager = Managers.AudioManager.Instance;
            audioManager.MasterVolume = 0.8f;

            var uiManager = Managers.UIManager.Instance;
            uiManager.ShowLoadingOverlay(true);
        }

        private void DemoSafeAccess()
        {
            Debug.Log("--- 안전한 접근 방법 ---");

            // HasInstance로 확인
            if (Managers.GameManager.HasInstance)
            {
                Debug.Log("GameManager 인스턴스 존재함");
            }

            // TryGetInstance 패턴
            if (Managers.AudioManager.TryGetInstance(out var audioManager))
            {
                Debug.Log($"AudioManager 볼륨: {audioManager.MasterVolume}");
            }
        }

        // 컨텍스트 메뉴를 통한 개별 테스트
        [ContextMenu("Singleton 데모 실행")]
        public void RunDemo()
        {
            RunSingletonDemo();
        }

        [ContextMenu("모든 매니저 초기화 테스트")]
        public void TestAllManagersInitialization()
        {
            Debug.Log("모든 매니저 초기화 테스트 시작...");

            // 각 매니저에 접근하여 초기화 트리거
            var gm = Managers.GameManager.Instance;
            var am = Managers.AudioManager.Instance;
            var um = Managers.UIManager.Instance;

            Debug.Log($"GameManager 초기화: {gm != null}");
            Debug.Log($"AudioManager 초기화: {am != null}");
            Debug.Log($"UIManager 초기화: {um != null}");
        }
    }
}

// 사용 예제
namespace Core.Examples
{
    /// <summary>
    /// Singleton 사용 예제 클래스
    /// </summary>
    public class SingletonUsageExample : MonoBehaviour
    {
        private void Start()
        {
            // 사용 예제들
            DemonstrateUsage();
        }

        private void DemonstrateUsage()
        {
            // GameManager 사용
            GameManager.Instance.AddScore(100);
            GameManager.Instance.LoseLife();

            // AudioManager 사용
            AudioManager.Instance.MasterVolume = 0.8f;

            // UIManager 사용
            //UIManager.Instance.ShowMainMenu();

            // 안전한 접근 방법
            if (GameManager.TryGetInstance(out var gameManager))
            {
                gameManager.AddScore(50);
            }

            // 인스턴스 존재 확인
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.StopMusic();
            }
        }

        [ContextMenu("게임 매니저 테스트")]
        private void TestGameManager()
        {
            var gm = GameManager.Instance;
            gm.AddScore(500);
            gm.LoseLife();
            //Debug.Log($"현재 점수: {gm.CurrentScore}, 남은 생명: {gm.PlayerLives}");
        }

        [ContextMenu("오디오 매니저 테스트")]
        private void TestAudioManager()
        {
            var am = AudioManager.Instance;
            am.MasterVolume = 0.5f;
            Debug.Log($"마스터 볼륨: {am.MasterVolume}");
        }
    }
}
