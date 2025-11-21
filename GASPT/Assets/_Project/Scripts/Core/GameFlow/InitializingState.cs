using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.UI;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 게임 최초 시작 시 초기화 상태
    /// - SingletonPreloader 초기화 대기
    /// - 필수 시스템 로딩
    /// - 로딩 완료 후 StartRoom으로 전환
    /// </summary>
    public class InitializingState : State
    {
        public override string Name => "Initializing";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[InitializingState] 게임 초기화 시작");

            // FadeController로 검은 화면 시작
            var fadeController = FadeController.Instance;
            if (fadeController != null)
            {
                fadeController.SetBlack();
                Debug.Log("[InitializingState] 검은 화면 설정 완료");
            }

            // TODO: 로딩 UI 표시
            // LoadingUI.Show();
            // LoadingUI.SetProgress(0f);

            // SingletonPreloader 초기화 대기
            await WaitForSingletonPreloader(cancellationToken);

            // TODO: 추가 데이터 로드
            // - 세이브 데이터
            // - 설정 데이터
            // - 리소스 프리로드

            // TODO: 로딩 UI 진행도 업데이트
            // LoadingUI.SetProgress(0.5f);

            await Awaitable.WaitForSecondsAsync(0.5f, cancellationToken);

            // TODO: 로딩 UI 진행도 완료
            // LoadingUI.SetProgress(1f);
            await Awaitable.WaitForSecondsAsync(0.3f, cancellationToken);

            // TODO: 로딩 UI 숨기기
            // LoadingUI.Hide();

            Debug.Log("[InitializingState] 게임 초기화 완료");

            // StartRoom 로딩으로 전환
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.TriggerInitializationComplete();
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[InitializingState] 초기화 상태 종료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// SingletonPreloader 초기화 완료 대기
        /// </summary>
        private async Awaitable WaitForSingletonPreloader(CancellationToken cancellationToken)
        {
            Debug.Log("[InitializingState] SingletonPreloader 초기화 대기 중...");

            int maxAttempts = 100; // 최대 10초 대기
            int attempts = 0;

            // GameManager가 초기화될 때까지 대기 (모든 싱글톤의 마지막)
            while (!GameManager.HasInstance && attempts < maxAttempts)
            {
                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);
                attempts++;

                // TODO: 로딩 UI 진행도 업데이트
                // float progress = Mathf.Clamp01((float)attempts / maxAttempts);
                // LoadingUI.SetProgress(progress * 0.5f); // 0~50%
            }

            if (!GameManager.HasInstance)
            {
                Debug.LogError("[InitializingState] SingletonPreloader 초기화 실패 - 타임아웃");
                return;
            }

            Debug.Log("[InitializingState] SingletonPreloader 초기화 완료!");

            // 추가 안전 대기 (모든 싱글톤 Awake 완료 보장)
            await Awaitable.WaitForSecondsAsync(0.2f, cancellationToken);
        }
    }
}
