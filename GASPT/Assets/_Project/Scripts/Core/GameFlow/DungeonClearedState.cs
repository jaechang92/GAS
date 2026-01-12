using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.UI;
using GASPT.Gameplay.Level;
using GASPT.Meta;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 클리어 상태
    /// - 스테이지 클리어 시 진입
    /// - 런 결과 UI 표시 (RunResultView)
    /// - 획득한 골드/Bone/Soul 결산
    /// - 메타 재화 확정 및 저장
    /// - 로비 복귀 또는 다음 스테이지 진행
    /// </summary>
    public class DungeonClearedState : State
    {
        public override string Name => "DungeonCleared";

        // UI 완료 대기용 플래그
        private bool isWaitingForUI;

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonClearedState] 던전 클리어!");

            isWaitingForUI = true;

            var gameManager = GameManager.Instance;

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
                MetaProgressionManager.Instance.EndRun(cleared: true, stageReached: stage, enemiesKilled: enemiesKilled);
            }

            // UIManager를 통해 런 결과 표시
            if (UIManager.HasInstance && UIManager.Instance.RunResult != null)
            {
                // 콜백 설정 (클리어 시에도 로비 복귀, 다음 스테이지 기능은 추후 구현)
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
                    cleared: true,
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
                // UIManager 또는 RunResultView가 없으면 3초 후 자동 복귀
                Debug.LogWarning("[DungeonClearedState] UIManager 또는 RunResultView가 없습니다. 3초 후 자동 복귀");
                await Awaitable.WaitForSecondsAsync(3f, cancellationToken);
            }

            // 상태 전환
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                // 모두 로비로 복귀 (다음 스테이지 진행 기능은 추후 구현)
                gameFlowFSM.TriggerReturnToStart();
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonClearedState] 클리어 상태 종료 - 로비 복귀");

            // 런 결과 UI 숨기기
            if (UIManager.HasInstance)
            {
                UIManager.Instance.HideRunResult();
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }
    }
}
