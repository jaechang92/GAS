using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.Gameplay.Level;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 전투 상태
    /// - 적 스폰 및 전투 진행
    /// - 적 전멸 시 DungeonReward 상태로 전환
    /// </summary>
    public class DungeonCombatState : State
    {
        public override string Name => "DungeonCombat";

        private RoomManager roomManager;

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonCombatState] 전투 시작");

            // RoomManager 이벤트 구독
            roomManager = RoomManager.Instance;
            if (roomManager != null)
            {
                roomManager.OnRoomCleared += OnRoomCleared;
                Debug.Log($"[DungeonCombatState] 현재 방: {roomManager.CurrentRoom?.name ?? "None"}");
                Debug.Log($"[DungeonCombatState] 진행도: {roomManager.CurrentRoomIndex + 1}/{roomManager.TotalRoomCount}");
            }
            else
            {
                Debug.LogError("[DungeonCombatState] RoomManager를 찾을 수 없습니다!");
            }

            // 전투 UI는 각 컴포넌트에서 자동으로 표시됨:
            // - ResourceBarPresenter: HP/마나 (PlayerStats 이벤트 구독)
            // - SkillUIPanel: 스킬 쿨다운 (SkillSystem 이벤트 구독)
            // - DamageNumberPool: 데미지/회복 표시 (싱글톤)
            // - BossHealthBar: 보스 체력 (자동 초기화)
            // - BuffIcon: 버프/디버프 (StatusEffectManager 이벤트 구독)

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonCombatState] 전투 종료");

            // RoomManager 이벤트 구독 해제
            if (roomManager != null)
            {
                roomManager.OnRoomCleared -= OnRoomCleared;
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void OnUpdateState(float deltaTime)
        {
            // 전투 중 로직 (필요 시)
            // 예: 시간 제한, 특수 이벤트 등
        }

        /// <summary>
        /// 방 클리어 시 호출되는 이벤트 핸들러
        /// </summary>
        private void OnRoomCleared(Room room)
        {
            Debug.Log($"[DungeonCombatState] 방 클리어: {room.name}");

            // GameFlowFSM에 적 전멸 알림
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.TriggerEnemiesCleared();
            }
        }
    }
}
