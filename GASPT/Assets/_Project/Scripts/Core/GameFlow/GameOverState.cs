using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.UI;
using GASPT.Gameplay.Level;
using GASPT.Meta;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 게임 오버 상태
    /// - 플레이어 사망 시 진입
    /// - 런 결과 UI 표시 (RunResultView)
    /// - 메타 재화 확정 및 통계 업데이트
    /// - 재시작 또는 로비 복귀 선택
    /// </summary>
    public class GameOverState : State
    {
        public override string Name => "GameOver";

        // UI 완료 대기용 플래그
        private bool isWaitingForUI;

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[GameOverState] 게임 오버!");

            isWaitingForUI = true;

            var gameManager = GameManager.Instance;

            // 게임 일시정지는 RunResultView에서 처리
            // (NotifyFullScreenUIOpened 호출)

            // 통계 수집
            int goldEarned = gameManager?.CurrentGold ?? 0;
            int roomsCount = RoomManager.Instance?.TotalRoomCount ?? 0;
            int stage = gameManager?.CurrentStage ?? 1;
            int enemiesKilled = 0; // TODO: EnemyManager에서 추적
            float playTime = Time.time; // TODO: RunManager에서 정확한 시간 추적

            // 메타 재화 수집 (확정 전)
            int tempBone = 0;
            int tempSoul = 0;

            if (MetaProgressionManager.HasInstance)
            {
                tempBone = MetaProgressionManager.Instance.Currency.TempBone;
                tempSoul = MetaProgressionManager.Instance.Currency.TempSoul;

                // 런 종료 및 재화 확정
                MetaProgressionManager.Instance.EndRun(cleared: false, stageReached: stage, enemiesKilled: enemiesKilled);
            }

            // UIManager를 통해 런 결과 표시
            if (UIManager.HasInstance)
            {
                // 콜백 설정 (게임오버 시에는 로비 복귀만 가능)
                UIManager.Instance.SetRunResultReturnCallback(() =>
                {
                    isWaitingForUI = false;
                });

                UIManager.Instance.SetRunResultRestartCallback(() =>
                {
                    isWaitingForUI = false;
                });

                // 결과 표시
                UIManager.Instance.ShowRunResult(
                    cleared: false,
                    stage: stage,
                    rooms: roomsCount,
                    enemies: enemiesKilled,
                    time: playTime,
                    gold: goldEarned,
                    bone: tempBone,
                    soul: tempSoul
                );

                // UI 버튼 클릭 대기
                while (isWaitingForUI && !cancellationToken.IsCancellationRequested)
                {
                    await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);
                }
            }
            else
            {
                // UIManager가 없으면 3초 후 자동 복귀
                Debug.LogWarning("[GameOverState] UIManager가 없습니다. 3초 후 자동 복귀");
                await Awaitable.WaitForSecondsAsync(3f, cancellationToken);
            }

            // 상태 전환 (로비로 복귀)
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.TriggerRestartGame();
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[GameOverState] 게임 오버 상태 종료");

            // 런 결과 UI 숨기기
            if (UIManager.HasInstance)
            {
                UIManager.Instance.HideRunResult();
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
            // 플레이어 부활
            var playerStats = Object.FindAnyObjectByType<GASPT.Stats.PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Revive();
            }

            // 런 골드 리셋
            var currencySystem = GASPT.Economy.CurrencySystem.Instance;
            if (currencySystem != null)
            {
                currencySystem.ResetGold();
            }
        }
    }
}
