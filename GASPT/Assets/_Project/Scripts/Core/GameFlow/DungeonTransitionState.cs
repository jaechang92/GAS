using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.Gameplay.Level;
using GASPT.Core.SceneManagement;
using GASPT.UI;

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
                // Fade Out (화면을 검게)
                var fadeController = FadeController.Instance;
                if (fadeController != null)
                {
                    await fadeController.FadeOut(0.3f);
                }

                // 다음 방으로 이동
                await roomManager.MoveToNextRoomAsync();
                Debug.Log($"[DungeonTransitionState] 다음 방 이동 완료: {roomManager.CurrentRoom?.name ?? "Unknown"}");

                // ★ Room 전환 후 검증 및 재할당 (카메라 Bounds 등)
                await ValidateRoomReferences(cancellationToken);

                // Fade In (화면을 밝게)
                if (fadeController != null)
                {
                    await fadeController.FadeIn(0.3f);
                }

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

        /// <summary>
        /// Room 전환 후 참조 검증 및 재할당
        /// 카메라 Bounds 등이 새 Room의 것으로 업데이트됨
        /// </summary>
        private async Awaitable ValidateRoomReferences(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonTransitionState] Room 검증 시작...");

            if (SceneValidationManager.HasInstance)
            {
                bool success = await SceneValidationManager.Instance.ValidateAllAsync();
                if (success)
                {
                    Debug.Log("[DungeonTransitionState] Room 검증 완료 - 모든 참조 유효");
                }
                else
                {
                    Debug.LogWarning("[DungeonTransitionState] Room 검증 완료 - 일부 참조 실패 (게임 계속 진행)");
                }
            }
            else
            {
                Debug.LogWarning("[DungeonTransitionState] SceneValidationManager 없음 - 검증 스킵");
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonTransitionState] 방 전환 완료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }
    }
}
