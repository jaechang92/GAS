using UnityEngine;
using Managers;

namespace Core
{
    /// <summary>
    /// 각 매니저들의 기본 기능을 테스트하는 스크립트
    /// </summary>
    public class ManagerTest : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool testOnStart = true;

        private void Start()
        {
            if (testOnStart)
            {
                TestAllManagers();
            }
        }

        private void TestAllManagers()
        {
            Debug.Log("=== 매니저 테스트 시작 ===");

            TestGameManager();
            TestAudioManager();
            TestUIManager();

            Debug.Log("=== 매니저 테스트 완료 ===");
        }

        private void TestGameManager()
        {
            Debug.Log("--- GameManager 테스트 ---");

            var gm = GameManager.Instance;
            Debug.Log($"초기 생명: {gm.CurrentLives}, 점수: {gm.CurrentScore}");

            gm.AddScore(100);
            gm.LoseLife();

            Debug.Log($"테스트 후 생명: {gm.CurrentLives}, 점수: {gm.CurrentScore}");
        }

        private void TestAudioManager()
        {
            Debug.Log("--- AudioManager 테스트 ---");

            var am = AudioManager.Instance;
            Debug.Log($"마스터 볼륨: {am.MasterVolume}");
            Debug.Log($"음악 재생 중: {am.IsMusicPlaying}");

            am.MasterVolume = 0.8f;
            Debug.Log($"변경된 마스터 볼륨: {am.MasterVolume}");
        }

        private void TestUIManager()
        {
            Debug.Log("--- UIManager 테스트 ---");

            var um = UIManager.Instance;
            Debug.Log($"메인 캔버스: {um.GetMainCanvas() != null}");
            Debug.Log($"활성 UI 개수: {um.GetActiveUICount()}");

            um.ShowLoadingOverlay(true);
            Debug.Log("로딩 오버레이 표시 테스트 완료");

            // 2초 후 숨김
            Invoke(nameof(HideLoadingTest), 2f);
        }

        private void HideLoadingTest()
        {
            UIManager.Instance.ShowLoadingOverlay(false);
            Debug.Log("로딩 오버레이 숨김 테스트 완료");
        }

        // 컨텍스트 메뉴 테스트들
        [ContextMenu("GameManager 테스트")]
        public void TestGameManagerOnly()
        {
            TestGameManager();
        }

        [ContextMenu("AudioManager 테스트")]
        public void TestAudioManagerOnly()
        {
            TestAudioManager();
        }

        [ContextMenu("UIManager 테스트")]
        public void TestUIManagerOnly()
        {
            TestUIManager();
        }

        [ContextMenu("게임 시작 테스트")]
        public void TestGameStart()
        {
            var gm = GameManager.Instance;
            gm.StartGame();
        }

        [ContextMenu("게임 재시작 테스트")]
        public void TestGameRestart()
        {
            var gm = GameManager.Instance;
            gm.RestartGame();
        }

        [ContextMenu("로딩 오버레이 토글")]
        public void ToggleLoadingOverlay()
        {
            var um = UIManager.Instance;
            // 현재 로딩 오버레이 상태를 확인하여 토글
            um.ShowLoadingOverlay(um.GetActiveUICount() == 0);
        }
    }
}