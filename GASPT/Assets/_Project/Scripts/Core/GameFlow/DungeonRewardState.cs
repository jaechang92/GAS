using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.Gameplay.Level;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 보상 선택 상태
    /// - 보상 생성 및 표시
    /// - 다음 방 포탈 활성화
    /// - 플레이어가 보상 선택 후 포탈 입장 시 다음 상태로 전환
    /// </summary>
    public class DungeonRewardState : State
    {
        public override string Name => "DungeonReward";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonRewardState] 보상 선택 단계");

            var roomManager = RoomManager.Instance;
            if (roomManager != null)
            {
                // 현재 방 정보
                Room currentRoom = roomManager.CurrentRoom;
                Debug.Log($"[DungeonRewardState] 클리어한 방: {currentRoom?.name ?? "Unknown"}");

                // TODO: 보상 생성
                // SpawnRewards(currentRoom);
                Debug.Log("[DungeonRewardState] 보상 생성 (TODO)");

                // TODO: 다음 방 포탈 활성화
                // ActivateNextPortal(currentRoom);
                Debug.Log("[DungeonRewardState] 다음 방 포탈 활성화 (TODO)");
            }

            // TODO: 보상 UI 표시
            // RewardUI.Show();

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonRewardState] 보상 단계 종료");

            // TODO: 보상 UI 숨기기
            // RewardUI.Hide();

            // TODO: 선택되지 않은 보상 제거
            // ClearRemainingRewards();

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void OnUpdateState(float deltaTime)
        {
            // 보상 선택 대기
            // 포탈 입장은 Portal 스크립트에서 처리
            // Portal → GameFlowStateMachine.TriggerEnterNextRoom() 호출
        }

        /// <summary>
        /// 보상 생성 (TODO: RewardSpawner 구현 후 사용)
        /// </summary>
        private void SpawnRewards(Room room)
        {
            // RewardSpawner.SpawnRewards(room.transform.position, rewardCount: 3);
        }

        /// <summary>
        /// 다음 방 포탈 활성화 (TODO: Portal 시스템 구현 후 사용)
        /// </summary>
        private void ActivateNextPortal(Room room)
        {
            // RoomPortal portal = room.GetComponentInChildren<RoomPortal>();
            // portal?.Activate();
        }
    }
}
