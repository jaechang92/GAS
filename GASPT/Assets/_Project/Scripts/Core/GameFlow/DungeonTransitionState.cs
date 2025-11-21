using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.Gameplay.Level;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 방 전환 상태
    /// - 다음 방으로 이동
    /// - 마지막 방인 경우 DungeonCleared 상태로 전환
    /// - 일반 방인 경우 DungeonCombat 상태로 복귀
    /// </summary>
    public class DungeonTransitionState : State
    {
        public override string Name => "DungeonTransition";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonTransitionState] 다음 방으로 전환 시작");

            var roomManager = RoomManager.Instance;
            if (roomManager == null)
            {
                Debug.LogError("[DungeonTransitionState] RoomManager를 찾을 수 없습니다!");
                return;
            }

            int currentIndex = roomManager.CurrentRoomIndex;
            int totalRooms = roomManager.TotalRoomCount;

            Debug.Log($"[DungeonTransitionState] 현재 진행도: {currentIndex + 1}/{totalRooms}");

            // 다음 방이 있는지 확인
            bool hasNextRoom = (currentIndex + 1) < totalRooms;

            if (hasNextRoom)
            {
                // 다음 방으로 이동
                await roomManager.MoveToNextRoomAsync();
                Debug.Log($"[DungeonTransitionState] 다음 방 이동 완료: {roomManager.CurrentRoom?.name ?? "Unknown"}");

                // GameFlowFSM에 다음 방 준비 완료 알림
                var gameFlowFSM = GameFlowStateMachine.Instance;
                if (gameFlowFSM != null)
                {
                    gameFlowFSM.NotifyNextRoomReady();
                }
            }
            else
            {
                // 마지막 방 클리어 - 던전 완료
                Debug.Log("[DungeonTransitionState] 마지막 방 클리어 - 던전 완료!");

                // GameFlowFSM에 던전 완료 알림
                var gameFlowFSM = GameFlowStateMachine.Instance;
                if (gameFlowFSM != null)
                {
                    gameFlowFSM.NotifyDungeonComplete();
                }
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonTransitionState] 방 전환 완료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }
    }
}
