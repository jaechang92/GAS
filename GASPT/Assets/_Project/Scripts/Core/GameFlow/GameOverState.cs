using System.Threading;
using UnityEngine;
using FSM.Core;

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

            // TODO: 게임 오버 UI 표시
            // GameOverUI.Show();

            // TODO: 통계 표시
            // - 도달한 스테이지
            // - 획득한 골드 (메타 골드로 전환되지 않음)
            // - 처치한 적 수
            // - 플레이 시간

            // 런 골드 손실 (게임 오버 시에는 메타 골드 저장 안 함)
            if (gameManager != null)
            {
                int lostGold = gameManager.CurrentGold;
                Debug.Log($"[GameOverState] 손실한 골드: {lostGold}");
            }

            // 사용자 입력 대기 (재시작 또는 준비실 복귀)
            // TODO: UI 버튼 클릭 시 처리
            // - "재시작" 버튼 → TriggerRestartGame()
            // - "준비실 복귀" 버튼 → TriggerReturnToStart()

            // 임시: 3초 후 자동 준비실 복귀
            await Awaitable.WaitForSecondsAsync(3f, cancellationToken);

            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.TriggerReturnToStart();
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

            // TODO: 게임 오버 UI 숨기기
            // GameOverUI.Hide();

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
