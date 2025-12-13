using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.UI;
using GASPT.Gameplay.Level;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 게임 오버 상태
    /// - 플레이어 사망 시 진입
    /// - 게임 오버 UI 표시
    /// - 재시작 또는 준비실 복귀 선택
    /// </summary>
    public class GameOverState : State
    {
        public override string Name => "GameOver";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[GameOverState] 게임 오버!");

            // 게임 일시정지 (Time.timeScale = 0)
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.Pause();
            }

            // 통계 수집
            int goldEarned = gameManager?.CurrentGold ?? 0;
            int roomsCount = RoomManager.Instance?.TotalRoomCount ?? 0;

            // 게임 오버 UI 표시
            if (GameOverUI.HasInstance)
            {
                GameOverUI.Instance.ShowGameOver(goldEarned, roomsCount);
                Debug.Log("[GameOverState] GameOverUI 표시 완료");

                // UI에서 버튼 클릭 시 자동으로 TriggerRestartGame() 호출됨
                // 무한 대기 (UI에서 처리)
                while (GameOverUI.Instance.IsVisible && !cancellationToken.IsCancellationRequested)
                {
                    await Awaitable.WaitForSecondsAsync(0.5f, cancellationToken);
                }
            }
            else
            {
                Debug.LogWarning("[GameOverState] GameOverUI가 없습니다. 3초 후 자동 복귀");

                // 런 골드 손실 로그
                Debug.Log($"[GameOverState] 손실한 골드: {goldEarned}");

                // 폴백: 3초 후 자동 준비실 복귀
                await Awaitable.WaitForSecondsAsync(3f, cancellationToken);

                var gameFlowFSM = GameFlowStateMachine.Instance;
                if (gameFlowFSM != null)
                {
                    gameFlowFSM.TriggerRestartGame();
                }
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[GameOverState] 게임 오버 상태 종료");

            // 게임 재개 (Time.timeScale = 1)
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.Resume();
            }

            // 게임 오버 UI 숨기기
            if (GameOverUI.HasInstance && GameOverUI.Instance.IsVisible)
            {
                GameOverUI.Instance.Hide();
            }

            // 던전 데이터 정리
            CleanupDungeonData();

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// 던전 데이터 정리
        /// </summary>
        private void CleanupDungeonData()
        {
            Debug.Log("[GameOverState] 던전 데이터 정리");

            // 플레이어 부활
            var playerStats = UnityEngine.Object.FindAnyObjectByType<GASPT.Stats.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Revive();
                Debug.Log("[GameOverState] 플레이어 부활");
            }

            // 런 골드 리셋
            var currencySystem = GASPT.Economy.CurrencySystem.Instance;
            if (currencySystem != null)
            {
                currencySystem.ResetGold();
                Debug.Log("[GameOverState] 런 골드 리셋");
            }

            // TODO: 던전 전용 버프/아이템 제거
        }
    }
}
