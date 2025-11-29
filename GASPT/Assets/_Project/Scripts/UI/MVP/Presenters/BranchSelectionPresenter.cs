using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Graph;
using Core;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 분기 선택 UI 프레젠터
    /// Portal과 BranchSelectionView를 연결
    /// </summary>
    public class BranchSelectionPresenter : MonoBehaviour
    {
        // ====== 참조 ======

        [Header("뷰")]
        [SerializeField] private BranchSelectionView view;

        [Header("설정")]
        [SerializeField] private bool autoFindView = true;


        // ====== 상태 ======

        private List<DungeonNode> currentNodes = new List<DungeonNode>();
        private Portal currentPortal;


        // ====== 이벤트 ======

        /// <summary>
        /// 노드 선택 완료 이벤트
        /// </summary>
        public event Action<DungeonNode> OnNodeSelected;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (autoFindView && view == null)
            {
                view = FindAnyObjectByType<BranchSelectionView>();
            }
        }

        private void Start()
        {
            // 뷰 이벤트 구독
            if (view != null)
            {
                view.OnOptionSelected += HandleOptionSelected;
                view.OnCancelled += HandleCancelled;
            }
        }

        private void OnDestroy()
        {
            // 뷰 이벤트 구독 해제
            if (view != null)
            {
                view.OnOptionSelected -= HandleOptionSelected;
                view.OnCancelled -= HandleCancelled;
            }
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 분기 선택 표시
        /// </summary>
        public void ShowBranchSelection(List<DungeonNode> nodes, Portal portal = null)
        {
            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogWarning("[BranchSelectionPresenter] 선택 가능한 노드가 없습니다.");
                return;
            }

            currentNodes = nodes;
            currentPortal = portal;

            // 옵션 데이터 생성
            var options = nodes.Select(n => new BranchOptionData(n)).ToList();

            // 뷰에 데이터 설정
            if (view != null)
            {
                view.SetOptions(options);
                view.Show();
            }
            else
            {
                Debug.LogWarning("[BranchSelectionPresenter] View가 설정되지 않았습니다.");

                // View 없이도 첫 번째 옵션 자동 선택 (폴백)
                if (currentNodes.Count > 0)
                {
                    HandleOptionSelected(0);
                }
            }

            Debug.Log($"[BranchSelectionPresenter] 분기 선택 표시: {nodes.Count}개 옵션");
        }

        /// <summary>
        /// 분기 선택 숨김
        /// </summary>
        public void HideBranchSelection()
        {
            if (view != null)
            {
                view.Hide();
            }

            currentNodes.Clear();
            currentPortal = null;
        }

        /// <summary>
        /// 특정 노드 직접 선택
        /// </summary>
        public void SelectNode(DungeonNode node)
        {
            if (node == null) return;

            HideBranchSelection();

            // Portal을 통해 이동
            if (currentPortal != null)
            {
                currentPortal.SelectNode(node);
            }
            // 또는 RoomManager 직접 사용
            else if (RoomManager.Instance != null)
            {
                RoomManager.Instance.MoveToNodeAsync(node).Forget();
            }

            OnNodeSelected?.Invoke(node);
        }


        // ====== 이벤트 핸들러 ======

        private void HandleOptionSelected(int index)
        {
            if (index < 0 || index >= currentNodes.Count)
            {
                Debug.LogError($"[BranchSelectionPresenter] 잘못된 인덱스: {index}");
                return;
            }

            var selectedNode = currentNodes[index];
            Debug.Log($"[BranchSelectionPresenter] 옵션 선택됨: {selectedNode.nodeId} ({selectedNode.roomType})");

            SelectNode(selectedNode);
        }

        private void HandleCancelled()
        {
            Debug.Log("[BranchSelectionPresenter] 선택 취소됨");
            HideBranchSelection();
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// Portal 이벤트 구독 (Portal.OnBranchSelectionRequired)
        /// </summary>
        public void SubscribeToPortal(Portal portal)
        {
            if (portal == null) return;

            portal.OnBranchSelectionRequired += (nodes) => ShowBranchSelection(nodes, portal);

            Debug.Log($"[BranchSelectionPresenter] Portal 이벤트 구독: {portal.name}");
        }

        /// <summary>
        /// 모든 Portal에 자동 구독
        /// </summary>
        public void SubscribeToAllPortals()
        {
            var portals = FindObjectsByType<Portal>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var portal in portals)
            {
                SubscribeToPortal(portal);
            }

            Debug.Log($"[BranchSelectionPresenter] {portals.Length}개 Portal에 구독됨");
        }
    }
}
