using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Graph;
using GASPT.Core.Extensions;

namespace GASPT.UI.Minimap
{
    /// <summary>
    /// 미니맵 프레젠터
    /// RoomManager와 MinimapView를 연결하는 MVP Presenter
    /// </summary>
    public class MinimapPresenter : MonoBehaviour
    {
        // ====== 참조 ======

        [Header("뷰")]
        [SerializeField] private MinimapView view;

        [Header("설정")]
        [SerializeField] private bool autoFindView = true;
        [SerializeField] private bool autoSubscribe = true;

        [Header("입력")]
        [SerializeField] private KeyCode toggleKey = KeyCode.M;
        [SerializeField] private KeyCode tabToggleKey = KeyCode.Tab;


        // ====== 상태 ======

        private DungeonGraph currentGraph;
        private string currentNodeId;


        // ====== 이벤트 ======

        /// <summary>
        /// 노드 선택 이벤트 (미니맵에서 클릭)
        /// </summary>
        public event Action<DungeonNode> OnNodeSelectedFromMinimap;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (autoFindView && view == null)
            {
                view = FindAnyObjectByType<MinimapView>();
            }
        }

        private void Start()
        {
            // 뷰 이벤트 구독
            if (view != null)
            {
                view.OnNodeClicked += HandleNodeClicked;
                view.OnNodeHovered += HandleNodeHovered;
                view.OnNodeHoverExit += HandleNodeHoverExit;
            }

            // RoomManager 이벤트 구독
            if (autoSubscribe)
            {
                SubscribeToRoomManager();
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void OnDestroy()
        {
            // 뷰 이벤트 구독 해제
            if (view != null)
            {
                view.OnNodeClicked -= HandleNodeClicked;
                view.OnNodeHovered -= HandleNodeHovered;
                view.OnNodeHoverExit -= HandleNodeHoverExit;
            }

            // RoomManager 이벤트 구독 해제
            UnsubscribeFromRoomManager();
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 던전 그래프로 미니맵 초기화
        /// </summary>
        public void InitializeWithGraph(DungeonGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("[MinimapPresenter] 그래프가 null입니다.");
                return;
            }

            currentGraph = graph;

            // 노드 데이터 변환
            var nodeDataList = new List<MinimapNodeData>();
            foreach (var node in graph.GetAllNodes())
            {
                var nodeData = MinimapNodeData.FromDungeonNode(node);
                nodeDataList.Add(nodeData);
            }

            // 엣지 데이터 변환
            var edgeDataList = new List<MinimapEdgeData>();
            foreach (var edge in graph.GetAllEdges())
            {
                var edgeData = MinimapEdgeData.FromDungeonEdge(edge, graph);
                edgeDataList.Add(edgeData);
            }

            // 뷰에 데이터 설정
            if (view != null)
            {
                view.SetMapData(nodeDataList, edgeDataList);
                view.FitToView();
            }

            Debug.Log($"[MinimapPresenter] 미니맵 초기화: 노드 {nodeDataList.Count}개, 엣지 {edgeDataList.Count}개");
        }

        /// <summary>
        /// 현재 노드 설정
        /// </summary>
        public void SetCurrentNode(string nodeId)
        {
            currentNodeId = nodeId;

            if (view != null)
            {
                view.SetCurrentNode(nodeId);
                view.FocusOnNode(nodeId, true);
            }

            // 인접 노드 하이라이트
            UpdateSelectableNodes();
        }

        /// <summary>
        /// 노드 상태 업데이트 (방문, 클리어 등)
        /// </summary>
        public void UpdateNodeState(string nodeId)
        {
            if (currentGraph == null) return;

            var node = currentGraph.GetNode(nodeId);
            if (node == null) return;

            var nodeData = MinimapNodeData.FromDungeonNode(node);

            // 현재 노드면 Current 상태로
            if (nodeId == currentNodeId)
            {
                nodeData = nodeData.AsCurrent();
            }

            if (view != null)
            {
                view.UpdateNode(nodeData);
            }
        }

        /// <summary>
        /// 인접 노드 공개 (Fog of War 해제)
        /// </summary>
        public void RevealAdjacentNodes(string fromNodeId)
        {
            if (currentGraph == null) return;

            var adjacentNodes = currentGraph.GetAdjacentNodes(fromNodeId);
            var nodesToUpdate = new List<MinimapNodeData>();

            foreach (var node in adjacentNodes)
            {
                if (!node.isRevealed)
                {
                    node.isRevealed = true;
                }

                var nodeData = MinimapNodeData.FromDungeonNode(node);
                nodesToUpdate.Add(nodeData);
            }

            if (view != null)
            {
                view.UpdateNodes(nodesToUpdate);
            }

            Debug.Log($"[MinimapPresenter] 인접 노드 공개: {nodesToUpdate.Count}개");
        }

        /// <summary>
        /// 미니맵 표시/숨김 토글
        /// </summary>
        public void ToggleMinimap()
        {
            if (view == null) return;

            if (view.IsVisible)
            {
                view.Hide();
            }
            else
            {
                view.Show();
            }
        }

        /// <summary>
        /// 미니맵 표시
        /// </summary>
        public void ShowMinimap()
        {
            view?.Show();
        }

        /// <summary>
        /// 미니맵 숨김
        /// </summary>
        public void HideMinimap()
        {
            view?.Hide();
        }


        // ====== 내부 메서드 ======

        private void HandleInput()
        {
            // M 또는 Tab으로 토글
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(tabToggleKey))
            {
                ToggleMinimap();
            }
        }

        private void UpdateSelectableNodes()
        {
            if (currentGraph == null || view == null) return;

            // 현재 노드에서 이동 가능한 노드들
            var selectableNodes = currentGraph.GetAdjacentNodes(currentNodeId)
                .Where(n => !n.isVisited || n.roomType == RoomType.Shop || n.roomType == RoomType.Rest)
                .Select(n => n.nodeId)
                .ToList();

            view.HighlightSelectableNodes(selectableNodes);
        }

        private void SubscribeToRoomManager()
        {
            if (RoomManager.Instance == null) return;

            RoomManager.Instance.OnNodeChanged += HandleNodeChanged;
            RoomManager.Instance.OnAdjacentNodesRevealed += HandleAdjacentNodesRevealed;

            Debug.Log("[MinimapPresenter] RoomManager 이벤트 구독 완료");
        }

        private void UnsubscribeFromRoomManager()
        {
            if (RoomManager.Instance == null) return;

            RoomManager.Instance.OnNodeChanged -= HandleNodeChanged;
            RoomManager.Instance.OnAdjacentNodesRevealed -= HandleAdjacentNodesRevealed;
        }


        // ====== 이벤트 핸들러 ======

        private void HandleNodeClicked(string nodeId)
        {
            if (currentGraph == null) return;

            var node = currentGraph.GetNode(nodeId);
            if (node == null) return;

            // 이동 가능 여부 확인
            if (!CanMoveToNode(nodeId))
            {
                Debug.Log($"[MinimapPresenter] 이동 불가: {nodeId}");
                return;
            }

            Debug.Log($"[MinimapPresenter] 노드 선택: {nodeId}");
            OnNodeSelectedFromMinimap?.Invoke(node);

            // RoomManager를 통해 이동
            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.MoveToNodeAsync(node).Forget();
            }
        }

        private void HandleNodeHovered(string nodeId)
        {
            // TODO: 노드 정보 툴팁 표시
            Debug.Log($"[MinimapPresenter] 노드 호버: {nodeId}");
        }

        private void HandleNodeHoverExit()
        {
            // TODO: 툴팁 숨김
        }

        private void HandleNodeChanged(DungeonNode node)
        {
            if (node == null) return;

            SetCurrentNode(node.nodeId);
            UpdateNodeState(node.nodeId);
        }

        private void HandleAdjacentNodesRevealed(List<DungeonNode> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;

            var nodesToUpdate = nodes.Select(MinimapNodeData.FromDungeonNode).ToList();

            if (view != null)
            {
                view.UpdateNodes(nodesToUpdate);
            }

            UpdateSelectableNodes();
        }

        private bool CanMoveToNode(string nodeId)
        {
            if (currentGraph == null || string.IsNullOrEmpty(currentNodeId)) return false;

            // 현재 노드와 같으면 불가
            if (nodeId == currentNodeId) return false;

            // 인접 노드인지 확인
            var currentNode = currentGraph.GetNode(currentNodeId);
            if (currentNode == null) return false;

            return currentNode.outgoingConnections.Contains(nodeId);
        }
    }
}
