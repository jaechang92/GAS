using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.UI;
using GASPT.Gameplay.Level;

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

            // 통계 수집
            int goldEarned = gameManager?.CurrentGold ?? 0;
            int boneEarned = 0;
            int soulEarned = 0;
            int roomsCount = RoomManager.Instance?.TotalRoomCount ?? 0;

            if (gameManager != null)
            {
                // 런 종료 및 메타 재화 확정
                gameManager.Meta?.EndRun(cleared: true, stageReached: gameManager.CurrentStage);
                Debug.Log("[DungeonClearedState] 메타 재화 확정 완료");

                boneEarned = gameManager.Meta?.Currency?.Bone ?? 0;
                soulEarned = gameManager.Meta?.Currency?.Soul ?? 0;

                Debug.Log($"[DungeonClearedState] 총 Bone: {boneEarned}, Soul: {soulEarned}");
            }

            // 던전 클리어 UI 표시
            var dungeonCompleteUI = Object.FindAnyObjectByType<DungeonCompleteUI>();
            if (dungeonCompleteUI != null)
            {
                dungeonCompleteUI.Show(goldEarned, 0); // totalExp는 0으로 임시 설정
                Debug.Log("[DungeonClearedState] DungeonCompleteUI 표시 완료");

                // UI가 표시되면 버튼 클릭 대기
                while (dungeonCompleteUI.IsVisible && !cancellationToken.IsCancellationRequested)
                {
                    await Awaitable.WaitForSecondsAsync(0.5f, cancellationToken);
                }
            }
            else
            {
                Debug.LogWarning("[DungeonClearedState] DungeonCompleteUI가 없습니다. 3초 후 자동 복귀");

                // 폴백: 자동 복귀 대기 (3초 후)
                await Awaitable.WaitForSecondsAsync(3f, cancellationToken);

                // 준비실 복귀
                var gameFlowFSM = GameFlowStateMachine.Instance;
                if (gameFlowFSM != null)
                {
                    gameFlowFSM.TriggerReturnToStart();
                }
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonClearedState] 준비실 복귀 시작");

            // 던전 클리어 UI 숨기기
            var dungeonCompleteUI = Object.FindAnyObjectByType<DungeonCompleteUI>();
            if (dungeonCompleteUI != null && dungeonCompleteUI.IsVisible)
            {
                dungeonCompleteUI.Hide();
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }
    }
}
