using System.Threading;
using UnityEngine;
using FSM.Core;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 클리어 상태
    /// - 던전 클리어 UI 표시
    /// - 획득한 골드/경험치 결산
    /// - 메타 골드 저장
    /// - 준비실 복귀 대기
    /// </summary>
    public class DungeonClearedState : State
    {
        public override string Name => "DungeonCleared";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonClearedState] 던전 클리어!");

            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                // 런 골드를 메타 골드로 저장
                int runGold = gameManager.CurrentGold;
                if (runGold > 0)
                {
                    gameManager.Meta?.AddGold(runGold);
                    Debug.Log($"[DungeonClearedState] 메타 골드 저장: {runGold}");
                }

                Debug.Log($"[DungeonClearedState] 총 메타 골드: {gameManager.Meta?.TotalGold ?? 0}");
            }

            // TODO: 던전 클리어 UI 표시
            // DungeonCompleteUI.Show(totalGold, totalExp);

            // TODO: 통계 표시
            // - 클리어 타임
            // - 처치한 적 수
            // - 획득한 아이템 수
            // - 최종 골드/경험치

            // 자동 복귀 대기 (3초 후) 또는 사용자 입력 대기
            await Awaitable.WaitForSecondsAsync(3f, cancellationToken);

            // 준비실 복귀
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.TriggerReturnToStart();
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonClearedState] 준비실 복귀 시작");

            // TODO: 던전 클리어 UI 숨기기
            // DungeonCompleteUI.Hide();

            await Awaitable.NextFrameAsync(cancellationToken);
        }
    }
}
