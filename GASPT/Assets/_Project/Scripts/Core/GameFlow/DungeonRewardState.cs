using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using FSM.Core;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Reward;

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

        private List<GameObject> spawnedRewards = new List<GameObject>();

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonRewardState] 보상 선택 단계");

            var roomManager = RoomManager.Instance;
            if (roomManager != null)
            {
                // 현재 방 정보
                Room currentRoom = roomManager.CurrentRoom;
                Debug.Log($"[DungeonRewardState] 클리어한 방: {currentRoom?.name ?? "Unknown"}");

                // 보상 생성
                await SpawnRewardsAsync(currentRoom);

                // 다음 방 포탈 활성화
                ActivateNextPortal(currentRoom);
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonRewardState] 보상 단계 종료");

            // 선택되지 않은 보상 제거
            ClearRemainingRewards();

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void OnUpdateState(float deltaTime)
        {
            // 보상 선택 대기
            // 포탈 입장은 Portal 스크립트에서 처리
            // Portal → GameFlowStateMachine.TriggerEnterNextRoom() 호출
        }

        /// <summary>
        /// 보상 생성
        /// </summary>
        private async Awaitable SpawnRewardsAsync(Room room)
        {
            if (room == null) return;

            // RewardSpawner 사용
            if (RewardSpawner.HasInstance)
            {
                int difficulty = room.Data?.difficulty ?? 1;
                Vector3 spawnCenter = room.transform.position;

                spawnedRewards = await RewardSpawner.Instance.SpawnRoomRewardsAsync(spawnCenter, difficulty);
                Debug.Log($"[DungeonRewardState] 보상 {spawnedRewards.Count}개 생성 완료");
            }
            else
            {
                Debug.LogWarning("[DungeonRewardState] RewardSpawner가 없습니다.");
            }
        }

        /// <summary>
        /// 다음 방 포탈 활성화
        /// </summary>
        private void ActivateNextPortal(Room room)
        {
            if (room == null) return;

            // Room 내의 Portal 찾기
            Portal portal = room.GetComponentInChildren<Portal>(true);
            if (portal != null)
            {
                portal.SetActive(true);
                Debug.Log("[DungeonRewardState] 다음 방 포탈 활성화 완료");
            }
            else
            {
                Debug.LogWarning("[DungeonRewardState] Room에서 Portal을 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 남은 보상 제거
        /// </summary>
        private void ClearRemainingRewards()
        {
            foreach (var reward in spawnedRewards)
            {
                if (reward != null)
                {
                    Object.Destroy(reward);
                }
            }
            spawnedRewards.Clear();
            Debug.Log("[DungeonRewardState] 남은 보상 제거 완료");
        }
    }
}
