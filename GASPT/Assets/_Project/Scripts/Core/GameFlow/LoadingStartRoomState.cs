using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.UI;
using GASPT.Core.SceneManagement;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 준비실 복귀 로딩 상태
    /// State를 직접 상속 (FadeIn/Out, Scene 검증은 TransitionOrchestrator에서 처리)
    ///
    /// 담당 역할:
    /// - StartRoom 씬 로드 (Additive)
    /// - 던전 데이터 정리
    /// - Player 해제 확인 후 재초기화
    /// </summary>
    public class LoadingStartRoomState : State
    {
        public override string Name => "LoadingStartRoom";


        // ====== 상태 진입/종료 ======

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingStartRoomState] 준비실 로딩 시작");
            // FadeOut은 TransitionOrchestrator에서 처리됨

            // 로딩 UI 표시
            var loadingPresenter = LoadingPresenter.Instance;
            if (loadingPresenter != null)
            {
                loadingPresenter.StartLoading("준비실로 이동 중...");
            }

            // 던전 데이터 정리
            CleanupDungeonData();

            // Player 해제 대기 (씬 전환 전 Player 파괴 확인)
            await WaitForPlayerUnregistered(cancellationToken);

            // StartRoom 씬 로드
            await LoadStartRoomScene(cancellationToken);

            // 씬 로딩 완료 알림
            if (loadingPresenter != null)
            {
                loadingPresenter.NotifySceneLoaded();
            }

            // Player 초기화 대기
            await WaitForPlayerReady(cancellationToken);

            // 초기화 완료 알림
            if (loadingPresenter != null)
            {
                loadingPresenter.NotifyInitComplete();
            }

            // 로딩 UI 숨김
            if (loadingPresenter != null)
            {
                loadingPresenter.NotifyFinalComplete();
            }

            // Scene 검증 및 FadeIn은 TransitionOrchestrator에서 처리됨

            // 준비실 로딩 완료 알림
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.NotifyStartRoomLoaded();
            }

            Debug.Log("[LoadingStartRoomState] 준비실 로딩 완료");
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingStartRoomState] 상태 종료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }


        // ====== 씬 로드 ======

        /// <summary>
        /// StartRoom 씬 로드
        /// </summary>
        private async Awaitable LoadStartRoomScene(CancellationToken cancellationToken)
        {
            var sceneLoader = AdditiveSceneLoader.Instance;
            if (sceneLoader != null)
            {
                Debug.Log("[LoadingStartRoomState] StartRoom 씬 로딩 시작 (Additive)...");
                await sceneLoader.SwitchContentSceneAsync("StartRoom", cancellationToken);
                Debug.Log("[LoadingStartRoomState] StartRoom 씬 로딩 완료!");
            }
            else
            {
                Debug.LogError("[LoadingStartRoomState] AdditiveSceneLoader 없음 - 씬 로드 실패");
            }
        }


        // ====== 준비실 전용 로직 ======

        /// <summary>
        /// 던전 데이터 정리
        /// </summary>
        private void CleanupDungeonData()
        {
            Debug.Log("[LoadingStartRoomState] 던전 데이터 정리");

            // TODO: 던전 관련 데이터 초기화
            // - 임시 버프/디버프 제거
            // - 던전 전용 아이템 제거
            // - 런 골드는 이미 메타 골드로 전환됨 (DungeonClearedState에서)

            // 플레이어 체력 회복
            var playerStats = Object.FindAnyObjectByType<GASPT.Stats.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Revive();
                Debug.Log("[LoadingStartRoomState] 플레이어 체력 회복");
            }

            // CurrencySystem 초기화 (런 골드 리셋)
            var currencySystem = GASPT.Economy.CurrencySystem.Instance;
            if (currencySystem != null)
            {
                currencySystem.ResetGold();
                Debug.Log("[LoadingStartRoomState] 런 골드 리셋");
            }
        }

        /// <summary>
        /// Player 해제 대기 (씬 전환 전 Player가 파괴되었는지 확인)
        /// </summary>
        private async Awaitable WaitForPlayerUnregistered(CancellationToken cancellationToken)
        {
            int maxAttempts = 50; // 최대 5초 대기
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // GameManager.PlayerStats가 null이면 해제 완료
                if (!GameManager.HasInstance || GameManager.Instance.PlayerStats == null)
                {
                    Debug.Log("[LoadingStartRoomState] Player 해제 확인 완료");
                    return;
                }

                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);
                attempts++;
            }

            Debug.LogWarning("[LoadingStartRoomState] Player 해제 대기 타임아웃 (무시하고 진행)");
        }

        /// <summary>
        /// Player 초기화 대기
        /// </summary>
        private async Awaitable WaitForPlayerReady(CancellationToken cancellationToken)
        {
            int maxAttempts = 100; // 최대 10초 대기
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                if (GameManager.HasInstance && GameManager.Instance.PlayerStats != null)
                {
                    Debug.Log("[LoadingStartRoomState] Player 초기화 완료");
                    return;
                }

                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);
                attempts++;
            }

            Debug.LogError("[LoadingStartRoomState] Player 초기화 실패 - 타임아웃");
        }
    }
}
