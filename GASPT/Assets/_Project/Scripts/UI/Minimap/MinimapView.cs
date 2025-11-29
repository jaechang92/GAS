using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GASPT.UI.Minimap
{
    /// <summary>
    /// 미니맵 뷰 구현
    /// 전체 미니맵 UI를 관리하는 메인 컴포넌트
    /// </summary>
    public class MinimapView : MonoBehaviour, IMinimapView, IDragHandler, IScrollHandler
    {
        // ====== UI 요소 ======

        [Header("컨테이너")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private RectTransform nodesContainer;
        [SerializeField] private RectTransform edgesContainer;

        [Header("프리팹")]
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject edgePrefab;

        [Header("설정")]
        [SerializeField] private MinimapConfig config;


        // ====== 상태 ======

        private Dictionary<string, MinimapNodeUI> nodeUIs = new Dictionary<string, MinimapNodeUI>();
        private List<MinimapEdgeUI> edgeUIs = new List<MinimapEdgeUI>();

        private string currentNodeId;
        private List<string> highlightedNodeIds = new List<string>();

        private float currentZoom = 1f;
        private Vector2 panOffset = Vector2.zero;
        private bool isVisible;


        // ====== 이벤트 ======

        public event Action<string> OnNodeClicked;
        public event Action<string> OnNodeHovered;
        public event Action OnNodeHoverExit;


        // ====== 프로퍼티 ======

        public bool IsVisible => isVisible;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // 노드 이벤트 구독 해제
            foreach (var nodeUI in nodeUIs.Values)
            {
                if (nodeUI != null)
                {
                    nodeUI.OnClicked -= HandleNodeClicked;
                    nodeUI.OnHoverEnter -= HandleNodeHoverEnter;
                    nodeUI.OnHoverExit -= HandleNodeHoverExit;
                }
            }
        }


        // ====== IMinimapView 구현 ======

        public void Show()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
            isVisible = true;

            Debug.Log("[MinimapView] 미니맵 표시");
        }

        public void Hide()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
            isVisible = false;

            Debug.Log("[MinimapView] 미니맵 숨김");
        }

        public void SetMapData(List<MinimapNodeData> nodes, List<MinimapEdgeData> edges)
        {
            // 기존 요소 제거
            ClearAll();

            // 엣지 먼저 생성 (노드 아래에 그려지도록)
            foreach (var edgeData in edges)
            {
                CreateEdgeUI(edgeData);
            }

            // 노드 생성
            foreach (var nodeData in nodes)
            {
                CreateNodeUI(nodeData);
            }

            Debug.Log($"[MinimapView] 맵 데이터 설정: 노드 {nodes.Count}개, 엣지 {edges.Count}개");
        }

        public void UpdateNode(MinimapNodeData node)
        {
            if (nodeUIs.TryGetValue(node.nodeId, out var nodeUI))
            {
                nodeUI.UpdateData(node);
            }
        }

        public void UpdateNodes(List<MinimapNodeData> nodes)
        {
            foreach (var node in nodes)
            {
                UpdateNode(node);
            }
        }

        public void SetCurrentNode(string nodeId)
        {
            // 이전 현재 노드 해제
            if (!string.IsNullOrEmpty(currentNodeId) && nodeUIs.TryGetValue(currentNodeId, out var prevNode))
            {
                prevNode.SetAsCurrent(false);
            }

            // 새 현재 노드 설정
            currentNodeId = nodeId;
            if (!string.IsNullOrEmpty(nodeId) && nodeUIs.TryGetValue(nodeId, out var newNode))
            {
                newNode.SetAsCurrent(true);
            }

            Debug.Log($"[MinimapView] 현재 노드 설정: {nodeId}");
        }

        public void HighlightSelectableNodes(List<string> nodeIds)
        {
            // 기존 하이라이트 해제
            ClearHighlights();

            // 새 하이라이트 설정
            highlightedNodeIds = new List<string>(nodeIds);
            foreach (var nodeId in nodeIds)
            {
                if (nodeUIs.TryGetValue(nodeId, out var nodeUI))
                {
                    nodeUI.SetSelectable(true);
                }
            }

            // 관련 엣지 하이라이트
            HighlightEdgesToNodes(nodeIds);
        }

        public void ClearHighlights()
        {
            foreach (var nodeId in highlightedNodeIds)
            {
                if (nodeUIs.TryGetValue(nodeId, out var nodeUI))
                {
                    nodeUI.SetSelectable(false);
                }
            }
            highlightedNodeIds.Clear();

            // 엣지 하이라이트 해제
            foreach (var edgeUI in edgeUIs)
            {
                edgeUI.SetHighlighted(false);
            }
        }

        public void FocusOnNode(string nodeId, bool animate = true)
        {
            if (!nodeUIs.TryGetValue(nodeId, out var nodeUI)) return;

            Vector2 nodePos = nodeUI.Data.position;

            if (animate && config != null)
            {
                // 애니메이션으로 이동 (TODO: DOTween 또는 코루틴으로 구현)
                panOffset = -nodePos;
            }
            else
            {
                panOffset = -nodePos;
            }

            ApplyTransform();
        }

        public void FitToView()
        {
            if (nodeUIs.Count == 0) return;

            // 모든 노드의 바운딩 박스 계산
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;

            foreach (var nodeUI in nodeUIs.Values)
            {
                Vector2 pos = nodeUI.Data.position;
                min = Vector2.Min(min, pos);
                max = Vector2.Max(max, pos);
            }

            // 패딩 적용
            if (config != null)
            {
                min -= config.padding;
                max += config.padding;
            }

            // 줌 레벨 계산
            Vector2 mapSize = max - min;
            Vector2 viewSize = mapContainer != null ? mapContainer.rect.size : new Vector2(400, 600);

            float zoomX = viewSize.x / mapSize.x;
            float zoomY = viewSize.y / mapSize.y;
            currentZoom = Mathf.Min(zoomX, zoomY, 1f);

            if (config != null)
            {
                currentZoom = Mathf.Clamp(currentZoom, config.minZoom, config.maxZoom);
            }

            // 중심점으로 이동
            Vector2 center = (min + max) / 2f;
            panOffset = -center;

            ApplyTransform();
        }

        public void SetZoom(float zoomLevel)
        {
            if (config != null)
            {
                zoomLevel = Mathf.Clamp(zoomLevel, config.minZoom, config.maxZoom);
            }

            currentZoom = zoomLevel;
            ApplyTransform();
        }

        public void Reset()
        {
            ClearAll();
            currentNodeId = null;
            currentZoom = 1f;
            panOffset = Vector2.zero;
            ApplyTransform();
        }


        // ====== 드래그/스크롤 핸들러 ======

        public void OnDrag(PointerEventData eventData)
        {
            if (config == null || !config.allowDrag) return;

            panOffset += eventData.delta / currentZoom;
            ApplyTransform();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (config == null || !config.allowZoom) return;

            float scrollDelta = eventData.scrollDelta.y;
            float zoomChange = scrollDelta > 0 ? 1.1f : 0.9f;

            float newZoom = currentZoom * zoomChange;
            newZoom = Mathf.Clamp(newZoom, config.minZoom, config.maxZoom);

            currentZoom = newZoom;
            ApplyTransform();
        }


        // ====== 내부 메서드 ======

        private void CreateNodeUI(MinimapNodeData data)
        {
            if (nodePrefab == null || nodesContainer == null)
            {
                Debug.LogWarning("[MinimapView] nodePrefab 또는 nodesContainer가 설정되지 않았습니다.");
                return;
            }

            var nodeObj = Instantiate(nodePrefab, nodesContainer);
            var nodeUI = nodeObj.GetComponent<MinimapNodeUI>();

            if (nodeUI != null)
            {
                nodeUI.Initialize(data, config);

                // 이벤트 구독
                nodeUI.OnClicked += HandleNodeClicked;
                nodeUI.OnHoverEnter += HandleNodeHoverEnter;
                nodeUI.OnHoverExit += HandleNodeHoverExit;

                nodeUIs[data.nodeId] = nodeUI;
            }
        }

        private void CreateEdgeUI(MinimapEdgeData data)
        {
            if (edgePrefab == null || edgesContainer == null)
            {
                Debug.LogWarning("[MinimapView] edgePrefab 또는 edgesContainer가 설정되지 않았습니다.");
                return;
            }

            var edgeObj = Instantiate(edgePrefab, edgesContainer);
            var edgeUI = edgeObj.GetComponent<MinimapEdgeUI>();

            if (edgeUI != null)
            {
                edgeUI.Initialize(data, config);
                edgeUIs.Add(edgeUI);
            }
        }

        private void ClearAll()
        {
            // 노드 제거
            foreach (var nodeUI in nodeUIs.Values)
            {
                if (nodeUI != null)
                {
                    nodeUI.OnClicked -= HandleNodeClicked;
                    nodeUI.OnHoverEnter -= HandleNodeHoverEnter;
                    nodeUI.OnHoverExit -= HandleNodeHoverExit;
                    Destroy(nodeUI.gameObject);
                }
            }
            nodeUIs.Clear();

            // 엣지 제거
            foreach (var edgeUI in edgeUIs)
            {
                if (edgeUI != null)
                {
                    Destroy(edgeUI.gameObject);
                }
            }
            edgeUIs.Clear();

            highlightedNodeIds.Clear();
        }

        private void ApplyTransform()
        {
            if (mapContainer == null) return;

            mapContainer.localScale = Vector3.one * currentZoom;
            mapContainer.anchoredPosition = panOffset * currentZoom;
        }

        private void HighlightEdgesToNodes(List<string> targetNodeIds)
        {
            if (string.IsNullOrEmpty(currentNodeId)) return;

            foreach (var edgeUI in edgeUIs)
            {
                bool shouldHighlight = edgeUI.FromNodeId == currentNodeId &&
                                       targetNodeIds.Contains(edgeUI.ToNodeId);
                edgeUI.SetHighlighted(shouldHighlight);
            }
        }


        // ====== 이벤트 핸들러 ======

        private void HandleNodeClicked(string nodeId)
        {
            OnNodeClicked?.Invoke(nodeId);
        }

        private void HandleNodeHoverEnter(string nodeId)
        {
            OnNodeHovered?.Invoke(nodeId);
        }

        private void HandleNodeHoverExit()
        {
            OnNodeHoverExit?.Invoke();
        }
    }
}
