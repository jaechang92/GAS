using System.Threading;
using UnityEngine;

namespace GASPT.Core.SceneManagement
{
    /// <summary>
    /// 게임 진입점 - Bootstrap Scene에 배치
    ///
    /// 역할:
    /// 1. AdditiveSceneLoader 초기화
    /// 2. PersistentManagers Scene 로드 (Additive)
    /// 3. 기본 Content Scene (StartRoom) 로드 (Additive)
    /// 4. Bootstrap Scene은 비활성 상태로 유지 (또는 언로드)
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private bool unloadBootstrapAfterInit = true;
        [SerializeField] private bool showDebugLogs = true;

        private CancellationTokenSource cancellationTokenSource;

        private async void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();

            LogMessage("=== Bootstrap 시작 ===");

            try
            {
                await InitializeGame(cancellationTokenSource.Token);
            }
            catch (System.OperationCanceledException)
            {
                LogMessage("Bootstrap 취소됨");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Bootstrapper] 초기화 실패: {e.Message}\n{e.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// 게임 초기화 프로세스
        /// </summary>
        private async Awaitable InitializeGame(CancellationToken cancellationToken)
        {
            LogMessage("1. AdditiveSceneLoader 초기화 중...");

            // AdditiveSceneLoader 인스턴스 접근 (SingletonManager 자동 생성)
            var sceneLoader = AdditiveSceneLoader.Instance;

            if (sceneLoader == null)
            {
                Debug.LogError("[Bootstrapper] AdditiveSceneLoader 생성 실패!");
                return;
            }

            LogMessage("2. PersistentManagers + Content Scene 로드 중...");

            // PersistentManagers와 기본 Content Scene 로드
            await sceneLoader.InitializeFromBootstrap(cancellationToken);

            LogMessage("3. GameFlowStateMachine 시작 중...");

            // GameFlowStateMachine 시작 (SingletonPreloader에서 이미 초기화됨)
            // InitializingState에서 자동으로 시작됨

            LogMessage("=== Bootstrap 완료 ===");

            // Bootstrap Scene 처리
            if (unloadBootstrapAfterInit)
            {
                LogMessage("Bootstrap Scene 유지 (빈 씬 상태)");
                // Bootstrap Scene은 언로드하지 않고 빈 상태로 유지
                // (언로드하면 AdditiveSceneLoader 등이 파괴될 수 있음)
            }
        }

        private void LogMessage(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[Bootstrapper] {message}");
            }
        }
    }
}
