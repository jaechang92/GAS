using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GASPT.Core.Pooling;

namespace GASPT.UI.Minimap
{
    public class MinimapView : MonoBehaviour, IMinimapView, IDragHandler, IScrollHandler
    {
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

        private Dictionary<string, MinimapNodeUI> nodeUIs = new Dictionary<string, MinimapNodeUI>();
        private List<MinimapEdgeUI> edgeUIs = new List<MinimapEdgeUI>();

        private ObjectPool<MinimapNodeUI> nodePool;
        private ObjectPool<MinimapEdgeUI> edgePool;
        private bool poolsInitialized = false;

        private string currentNodeId;
        private List<string> highlightedNodeIds = new List<string>();
        private float currentZoom = 1f;
        private Vector2 panOffset = Vector2.zero;
        private bool isVisible;

        public event Action<string> OnNodeClicked;
        public event Action<string> OnNodeHovered;
        public event Action OnNodeHoverExit;

        public bool IsVisible => isVisible;

        private void Awake()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
        }

        private void OnDestroy()
        {
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

        public void Show()
        {
            if (rootPanel != null) rootPanel.SetActive(true);
            isVisible = true;
        }

        public void Hide()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            isVisible = false;
        }

        public void SetMapData(List<MinimapNodeData> nodes, List<MinimapEdgeData> edges)
        {
            ClearAll();
            foreach (var edgeData in edges) CreateEdgeUI(edgeData);
            foreach (var nodeData in nodes) CreateNodeUI(nodeData);
        }

        public void UpdateNode(MinimapNodeData node)
        {
            if (nodeUIs.TryGetValue(node.nodeId, out var nodeUI)) nodeUI.UpdateData(node);
        }

        public void UpdateNodes(List<MinimapNodeData> nodes)
        {
            foreach (var node in nodes) UpdateNode(node);
        }

        public void SetCurrentNode(string nodeId)
        {
            if (!string.IsNullOrEmpty(currentNodeId) && nodeUIs.TryGetValue(currentNodeId, out var prevNode))
                prevNode.SetAsCurrent(false);
            currentNodeId = nodeId;
            if (!string.IsNullOrEmpty(nodeId) && nodeUIs.TryGetValue(nodeId, out var newNode))
                newNode.SetAsCurrent(true);
        }

        public void HighlightSelectableNodes(List<string> nodeIds)
        {
            ClearHighlights();
            highlightedNodeIds = new List<string>(nodeIds);
            foreach (var nodeId in nodeIds)
            {
                if (nodeUIs.TryGetValue(nodeId, out var nodeUI)) nodeUI.SetSelectable(true);
            }
            HighlightEdgesToNodes(nodeIds);
        }

        public void ClearHighlights()
        {
            foreach (var nodeId in highlightedNodeIds)
            {
                if (nodeUIs.TryGetValue(nodeId, out var nodeUI)) nodeUI.SetSelectable(false);
            }
            highlightedNodeIds.Clear();
            foreach (var edgeUI in edgeUIs) edgeUI.SetHighlighted(false);
        }

        public void FocusOnNode(string nodeId, bool animate = true)
        {
            if (!nodeUIs.TryGetValue(nodeId, out var nodeUI)) return;
            panOffset = -nodeUI.Data.position;
            ApplyTransform();
        }

        public void FitToView()
        {
            if (nodeUIs.Count == 0) return;
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;
            foreach (var nodeUI in nodeUIs.Values)
            {
                Vector2 pos = nodeUI.Data.position;
                min = Vector2.Min(min, pos);
                max = Vector2.Max(max, pos);
            }
            if (config != null) { min -= config.padding; max += config.padding; }
            Vector2 mapSize = max - min;
            Vector2 viewSize = mapContainer != null ? mapContainer.rect.size : new Vector2(400, 600);
            float zoomX = viewSize.x / mapSize.x;
            float zoomY = viewSize.y / mapSize.y;
            currentZoom = Mathf.Min(zoomX, zoomY, 1f);
            if (config != null) currentZoom = Mathf.Clamp(currentZoom, config.minZoom, config.maxZoom);
            Vector2 center = (min + max) / 2f;
            panOffset = -center;
            ApplyTransform();
        }

        public void SetZoom(float zoomLevel)
        {
            if (config != null) zoomLevel = Mathf.Clamp(zoomLevel, config.minZoom, config.maxZoom);
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

        public void OnDrag(PointerEventData eventData)
        {
            if (config == null || !config.allowDrag) return;
            panOffset += eventData.delta / currentZoom;
            ApplyTransform();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (config == null || !config.allowZoom) return;
            float zoomChange = eventData.scrollDelta.y > 0 ? 1.1f : 0.9f;
            currentZoom = Mathf.Clamp(currentZoom * zoomChange, config.minZoom, config.maxZoom);
            ApplyTransform();
        }

        private void InitializePoolsIfNeeded()
        {
            if (poolsInitialized) return;
            if (nodePrefab != null)
            {
                var nodeComponent = nodePrefab.GetComponent<MinimapNodeUI>();
                if (nodeComponent != null)
                    nodePool = new ObjectPool<MinimapNodeUI>(nodeComponent, nodesContainer, 20, true);
            }
            if (edgePrefab != null)
            {
                var edgeComponent = edgePrefab.GetComponent<MinimapEdgeUI>();
                if (edgeComponent != null)
                    edgePool = new ObjectPool<MinimapEdgeUI>(edgeComponent, edgesContainer, 30, true);
            }
            poolsInitialized = true;
        }

        private void CreateNodeUI(MinimapNodeData data)
        {
            if (nodePrefab == null || nodesContainer == null) return;
            InitializePoolsIfNeeded();
            MinimapNodeUI nodeUI;
            if (nodePool != null)
            {
                nodeUI = nodePool.Get(Vector3.zero, Quaternion.identity);
                nodeUI.transform.SetParent(nodesContainer, false);
            }
            else
            {
                var nodeObj = Instantiate(nodePrefab, nodesContainer);
                nodeUI = nodeObj.GetComponent<MinimapNodeUI>();
            }
            if (nodeUI != null)
            {
                nodeUI.Initialize(data, config);
                nodeUI.OnClicked += HandleNodeClicked;
                nodeUI.OnHoverEnter += HandleNodeHoverEnter;
                nodeUI.OnHoverExit += HandleNodeHoverExit;
                nodeUIs[data.nodeId] = nodeUI;
            }
        }

        private void CreateEdgeUI(MinimapEdgeData data)
        {
            if (edgePrefab == null || edgesContainer == null) return;
            InitializePoolsIfNeeded();
            MinimapEdgeUI edgeUI;
            if (edgePool != null)
            {
                edgeUI = edgePool.Get(Vector3.zero, Quaternion.identity);
                edgeUI.transform.SetParent(edgesContainer, false);
            }
            else
            {
                var edgeObj = Instantiate(edgePrefab, edgesContainer);
                edgeUI = edgeObj.GetComponent<MinimapEdgeUI>();
            }
            if (edgeUI != null)
            {
                edgeUI.Initialize(data, config);
                edgeUIs.Add(edgeUI);
            }
        }

        private void ClearAll()
        {
            foreach (var nodeUI in nodeUIs.Values)
            {
                if (nodeUI != null)
                {
                    nodeUI.OnClicked -= HandleNodeClicked;
                    nodeUI.OnHoverEnter -= HandleNodeHoverEnter;
                    nodeUI.OnHoverExit -= HandleNodeHoverExit;
                    if (nodePool != null) nodePool.Release(nodeUI);
                    else Destroy(nodeUI.gameObject);
                }
            }
            nodeUIs.Clear();
            foreach (var edgeUI in edgeUIs)
            {
                if (edgeUI != null)
                {
                    if (edgePool != null) edgePool.Release(edgeUI);
                    else Destroy(edgeUI.gameObject);
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
                bool shouldHighlight = edgeUI.FromNodeId == currentNodeId && targetNodeIds.Contains(edgeUI.ToNodeId);
                edgeUI.SetHighlighted(shouldHighlight);
            }
        }

        private void HandleNodeClicked(string nodeId) => OnNodeClicked?.Invoke(nodeId);
        private void HandleNodeHoverEnter(string nodeId) => OnNodeHovered?.Invoke(nodeId);
        private void HandleNodeHoverExit() => OnNodeHoverExit?.Invoke();
    }
}
