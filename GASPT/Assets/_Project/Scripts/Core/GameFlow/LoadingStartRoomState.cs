using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using FSM.Core;
using GASPT.UI;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 준비실 복귀 로딩 상태
    /// - StartRoom 씬 로드 (나중에 씬 분리 시)
    /// - 현재는 던전 정리 및 초기화
    /// </summary>
    public class LoadingStartRoomState : State
    {
        public override string Name => "LoadingStartRoom";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingStartRoomState] 준비실 복귀 로딩 시작");

            // Fade Out (화면을 검게)
            var fadeController = FadeController.Instance;
            if (fadeController != null)
            {
                await fadeController.FadeOut(0.5f);
                Debug.Log("[LoadingStartRoomState] Fade Out 완료");
            }

            // TODO: 로딩 UI 표시
            // LoadingUI.Show();

            // 던전 데이터 정리
            CleanupDungeonData();

            // Player 초기화 대기 (씬 로딩 전 Player 해제 확인)
            await WaitForPlayerUnregistered(cancellationToken);

            // StartRoom 씬 로드 (Single 모드 - 기존 씬 완전 교체)
            Debug.Log("[LoadingStartRoomState] StartRoom 씬 로딩 시작...");

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync("StartRoom", LoadSceneMode.Single);
            if (loadOperation != null)
            {
                while (!loadOperation.isDone)
                {
                    // 로딩 진행도 표시 (TODO: LoadingUI 업데이트)
                    // LoadingUI.SetProgress(loadOperation.progress);
                    await Awaitable.NextFrameAsync(cancellationToken);
                }

                Debug.Log("[LoadingStartRoomState] StartRoom 씬 로딩 완료!");
            }
            else
            {
                Debug.LogError("[LoadingStartRoomState] StartRoom 씬 로드 실패! Build Settings에 씬이 추가되었는지 확인하세요.");
                return;
            }

            // Player 초기화 대기 (씬 로딩 후 Player 등록 확인)
            await WaitForPlayerReady(cancellationToken);

            // TODO: 로딩 UI 숨기기
            // LoadingUI.Hide();

            // Fade In (화면을 밝게)
            if (fadeController != null)
            {
                await fadeController.FadeIn(1.0f);
                Debug.Log("[LoadingStartRoomState] Fade In 완료");
            }

            // 준비실 로딩 완료 알림
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.NotifyStartRoomLoaded();
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingStartRoomState] 준비실 로딩 완료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }

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
            var playerStats = UnityEngine.Object.FindAnyObjectByType<GASPT.Stats.PlayerStats>();
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
                if (!GASPT.Core.GameManager.HasInstance || GASPT.Core.GameManager.Instance.PlayerStats == null)
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
        /// Player 초기화 대기 (GameManager.PlayerStats가 등록될 때까지)
        /// </summary>
        private async Awaitable WaitForPlayerReady(CancellationToken cancellationToken)
        {
            int maxAttempts = 100; // 최대 10초 대기
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // GameManager.PlayerStats 확인
                if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
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
