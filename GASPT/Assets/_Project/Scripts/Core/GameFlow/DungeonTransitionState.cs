using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using FSM.Core;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Graph;
using GASPT.Core.SceneManagement;
using GASPT.UI;
using GASPT.UI.MVP;
using GASPT.UI.Minimap;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 방 전환 상태
    /// - 다음 방으로 이동
    /// - 그래프 기반 던전: 분기 선택 UI 표시
    /// - 마지막 방인 경우 DungeonCleared 상태로 전환
    /// - 일반 방인 경우 DungeonCombat 상태로 복귀
    /// </summary>
    public class DungeonTransitionState : State
    {
        public override string Name => "DungeonTransition";

        // 분기 선택 대기용
        private DungeonNode selectedNode;
        private bool isWaitingForBranchSelection;

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonTransitionState] 다음 방으로 전환 시작");

            var roomManager = RoomManager.Instance;
            if (roomManager == null)
            {
                Debug.LogError("[DungeonTransitionState] RoomManager를 찾을 수 없습니다!");
                return;
            }

            // 그래프 기반 던전 처리
            if (roomManager.IsUsingGraphDungeon)
            {
                await HandleGraphBasedTransition(roomManager, cancellationToken);
            }
            // 선형 던전 처리 (기존 방식)
            else
            {
                await HandleLinearTransition(roomManager, cancellationToken);
            }
        }

        /// <summary>
        /// 그래프 기반 던전 전환 처리
        /// </summary>
        private async Awaitable HandleGraphBasedTransition(RoomManager roomManager, CancellationToken cancellationToken)
        {
            var graph = roomManager.DungeonGraph;
            var currentNode = graph?.CurrentNode;

            if (currentNode == null)
            {
                Debug.LogError("[DungeonTransitionState] 현재 노드가 없습니다!");
                return;
            }

            Debug.Log($"[DungeonTransitionState] 현재 노드: {currentNode.nodeId} ({currentNode.roomType})");

            // 보스 노드인 경우 던전 완료
            if (currentNode.nodeId == graph.bossNodeId)
            {
                Debug.Log("[DungeonTransitionState] 보스 방 클리어 - 던전 완료!");
                NotifyDungeonComplete();
                return;
            }

            // 이동 가능한 다음 노드들
            var nextNodes = roomManager.GetAvailableNextNodes();

            if (nextNodes == null || nextNodes.Count == 0)
            {
                Debug.Log("[DungeonTransitionState] 더 이상 이동 가능한 노드가 없음 - 던전 완료!");
                NotifyDungeonComplete();
                return;
            }

            Debug.Log($"[DungeonTransitionState] 이동 가능 노드: {nextNodes.Count}개");

            // 분기 선택이 필요한 경우 (다중 경로)
            if (nextNodes.Count > 1)
            {
                await HandleBranchSelection(roomManager, nextNodes, cancellationToken);
            }
            // 단일 경로인 경우 바로 이동
            else
            {
                await MoveToNode(roomManager, nextNodes[0], cancellationToken);
            }
        }

        /// <summary>
        /// 분기 선택 처리
        /// </summary>
        private async Awaitable HandleBranchSelection(RoomManager roomManager, List<DungeonNode> nodes, CancellationToken cancellationToken)
        {
            Debug.Log("[DungeonTransitionState] 분기 선택 UI 표시");

            // 분기 선택 프레젠터 찾기
            var branchPresenter = Object.FindAnyObjectByType<BranchSelectionPresenter>();

            if (branchPresenter != null)
            {
                // 선택 대기 상태 설정
                isWaitingForBranchSelection = true;
                selectedNode = null;

                // 이벤트 구독
                branchPresenter.OnNodeSelected += OnBranchSelected;

                // UI 표시
                branchPresenter.ShowBranchSelection(nodes);

                // 선택 대기
                while (isWaitingForBranchSelection && !cancellationToken.IsCancellationRequested)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }

                // 이벤트 구독 해제
                branchPresenter.OnNodeSelected -= OnBranchSelected;

                // 선택된 노드로 이동
                if (selectedNode != null)
                {
                    await MoveToNode(roomManager, selectedNode, cancellationToken);
                }
            }
            else
            {
                // 분기 선택 UI가 없으면 첫 번째 노드로 자동 이동
                Debug.LogWarning("[DungeonTransitionState] BranchSelectionPresenter 없음 - 첫 번째 노드로 자동 이동");
                await MoveToNode(roomManager, nodes[0], cancellationToken);
            }
        }

        /// <summary>
        /// 분기 선택 콜백
        /// </summary>
        private void OnBranchSelected(DungeonNode node)
        {
            Debug.Log($"[DungeonTransitionState] 분기 선택됨: {node.nodeId}");
            selectedNode = node;
            isWaitingForBranchSelection = false;
        }

        /// <summary>
        /// 특정 노드로 이동
        /// </summary>
        private async Awaitable MoveToNode(RoomManager roomManager, DungeonNode targetNode, CancellationToken cancellationToken)
        {
            Debug.Log($"[DungeonTransitionState] 노드 이동: {targetNode.nodeId} ({targetNode.roomType})");

            // Fade Out
            var fadeController = FadeController.Instance;
            if (fadeController != null)
            {
                await fadeController.FadeOut(0.3f);
            }

            // 노드로 이동
            await roomManager.MoveToNodeAsync(targetNode);

            // 미니맵 업데이트
            UpdateMinimap(targetNode);

            // Room 검증
            await ValidateRoomReferences(cancellationToken);

            // Fade In
            if (fadeController != null)
            {
                await fadeController.FadeIn(0.3f);
            }

            // 다음 방 준비 완료 알림
            NotifyNextRoomReady();
        }

        /// <summary>
        /// 미니맵 업데이트
        /// </summary>
        private void UpdateMinimap(DungeonNode currentNode)
        {
            var minimapPresenter = Object.FindAnyObjectByType<MinimapPresenter>();
            if (minimapPresenter != null)
            {
                minimapPresenter.SetCurrentNode(currentNode.nodeId);
            }
        }

        /// <summary>
        /// 선형 던전 전환 처리 (기존 방식)
        /// </summary>
        private async Awaitable HandleLinearTransition(RoomManager roomManager, CancellationToken cancellationToken)
        {
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
                NotifyNextRoomReady();
            }
            else
            {
                // 마지막 방 클리어 - 던전 완료
                Debug.Log("[DungeonTransitionState] 마지막 방 클리어 - 던전 완료!");
                NotifyDungeonComplete();
            }
        }

        /// <summary>
        /// 다음 방 준비 완료 알림
        /// </summary>
        private void NotifyNextRoomReady()
        {
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.NotifyNextRoomReady();
            }
        }

        /// <summary>
        /// 던전 완료 알림
        /// </summary>
        private void NotifyDungeonComplete()
        {
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.NotifyDungeonComplete();
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
