#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Graph;
using GASPT.Gameplay.Level.Generation;

namespace GASPT.Editor
{
    /// <summary>
    /// 던전 그래프 시각화 에디터 윈도우
    /// 생성된 그래프를 시각적으로 확인하고 디버깅
    /// </summary>
    public class DungeonGraphViewer : EditorWindow
    {
        // ====== 설정 ======

        private DungeonConfig dungeonConfig;
        private int seed = 12345;
        private bool autoGenerate = false;

        // ====== 그래프 데이터 ======

        private DungeonGraph currentGraph;
        private Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();

        // ====== 뷰 설정 ======

        private Vector2 scrollPosition;
        private float zoom = 1f;
        private Vector2 panOffset = Vector2.zero;
        private bool isDragging = false;
        private Vector2 lastMousePos;

        // ====== 스타일 ======

        private const float NodeSize = 40f;
        private const float NodeSpacingX = 80f;
        private const float NodeSpacingY = 100f;
        private const float CanvasPadding = 50f;

        private static readonly Dictionary<RoomType, Color> RoomColors = new Dictionary<RoomType, Color>
        {
            { RoomType.Start, new Color(0.6f, 0.6f, 0.6f) },
            { RoomType.Normal, new Color(0.8f, 0.8f, 0.8f) },
            { RoomType.Elite, new Color(0.8f, 0.4f, 0.8f) },
            { RoomType.Boss, new Color(1f, 0.3f, 0.3f) },
            { RoomType.Shop, new Color(0.3f, 0.8f, 0.3f) },
            { RoomType.Rest, new Color(0.5f, 0.8f, 1f) },
            { RoomType.Treasure, new Color(1f, 0.85f, 0.3f) }
        };


        [MenuItem("GASPT/Dungeon Graph Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow<DungeonGraphViewer>("Dungeon Graph Viewer");
            window.minSize = new Vector2(600, 400);
        }


        private void OnGUI()
        {
            DrawToolbar();

            if (currentGraph != null)
            {
                DrawGraph();
            }
            else
            {
                DrawEmptyMessage();
            }

            HandleInput();
        }


        // ====== 툴바 ======

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // DungeonConfig 선택
            EditorGUI.BeginChangeCheck();
            dungeonConfig = (DungeonConfig)EditorGUILayout.ObjectField(
                dungeonConfig, typeof(DungeonConfig), false, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck() && autoGenerate && dungeonConfig != null)
            {
                GenerateGraph();
            }

            // Seed 입력
            EditorGUILayout.LabelField("Seed:", GUILayout.Width(40));
            EditorGUI.BeginChangeCheck();
            seed = EditorGUILayout.IntField(seed, GUILayout.Width(80));
            if (EditorGUI.EndChangeCheck() && autoGenerate && dungeonConfig != null)
            {
                GenerateGraph();
            }

            // 자동 생성 토글
            autoGenerate = GUILayout.Toggle(autoGenerate, "Auto", EditorStyles.toolbarButton, GUILayout.Width(50));

            // 생성 버튼
            if (GUILayout.Button("Generate", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                GenerateGraph();
            }

            // 랜덤 시드 버튼
            if (GUILayout.Button("Random", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                seed = Random.Range(0, int.MaxValue);
                if (dungeonConfig != null)
                {
                    GenerateGraph();
                }
            }

            GUILayout.FlexibleSpace();

            // 줌 리셋
            if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                zoom = 1f;
                panOffset = Vector2.zero;
            }

            // 줌 표시
            EditorGUILayout.LabelField($"Zoom: {zoom:F1}x", GUILayout.Width(70));

            EditorGUILayout.EndHorizontal();
        }


        // ====== 그래프 그리기 ======

        private void DrawGraph()
        {
            var graphRect = new Rect(0, EditorGUIUtility.singleLineHeight + 4,
                position.width, position.height - EditorGUIUtility.singleLineHeight - 4);

            GUI.BeginGroup(graphRect);

            // 배경
            EditorGUI.DrawRect(new Rect(0, 0, graphRect.width, graphRect.height),
                new Color(0.2f, 0.2f, 0.2f));

            // 그리드 (선택적)
            DrawGrid(graphRect.size);

            // 엣지 먼저 그리기
            DrawEdges();

            // 노드 그리기
            DrawNodes();

            // 정보 표시
            DrawInfo(graphRect);

            GUI.EndGroup();
        }

        private void DrawGrid(Vector2 size)
        {
            float gridSize = 50f * zoom;
            Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            Handles.BeginGUI();
            Handles.color = gridColor;

            float startX = (panOffset.x % gridSize);
            float startY = (panOffset.y % gridSize);

            for (float x = startX; x < size.x; x += gridSize)
            {
                Handles.DrawLine(new Vector3(x, 0), new Vector3(x, size.y));
            }

            for (float y = startY; y < size.y; y += gridSize)
            {
                Handles.DrawLine(new Vector3(0, y), new Vector3(size.x, y));
            }

            Handles.EndGUI();
        }

        private void DrawEdges()
        {
            if (currentGraph == null) return;

            Handles.BeginGUI();

            foreach (var edge in currentGraph.GetAllEdges())
            {
                if (!nodePositions.TryGetValue(edge.fromNodeId, out var fromPos)) continue;
                if (!nodePositions.TryGetValue(edge.toNodeId, out var toPos)) continue;

                Vector2 start = TransformPoint(fromPos);
                Vector2 end = TransformPoint(toPos);

                // 엣지 타입에 따른 색상
                Color lineColor = edge.edgeType switch
                {
                    EdgeType.Secret => new Color(0.8f, 0.4f, 0.8f, 0.8f),
                    EdgeType.OneWay => new Color(1f, 0.5f, 0f, 0.8f),
                    _ => new Color(0.7f, 0.7f, 0.7f, 0.8f)
                };

                Handles.color = lineColor;
                Handles.DrawLine(start, end);

                // 화살표 방향 표시
                Vector2 direction = (end - start).normalized;
                Vector2 midPoint = (start + end) / 2f;
                Vector2 arrowLeft = midPoint - direction * 8f + new Vector2(-direction.y, direction.x) * 5f;
                Vector2 arrowRight = midPoint - direction * 8f - new Vector2(-direction.y, direction.x) * 5f;

                Handles.DrawLine(midPoint, arrowLeft);
                Handles.DrawLine(midPoint, arrowRight);
            }

            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (currentGraph == null) return;

            foreach (var node in currentGraph.GetAllNodes())
            {
                if (!nodePositions.TryGetValue(node.nodeId, out var pos)) continue;

                Vector2 screenPos = TransformPoint(pos);
                float size = NodeSize * zoom;

                // 노드 배경
                Color nodeColor = RoomColors.TryGetValue(node.roomType, out var color) ? color : Color.gray;
                Rect nodeRect = new Rect(screenPos.x - size / 2, screenPos.y - size / 2, size, size);
                EditorGUI.DrawRect(nodeRect, nodeColor);

                // 노드 테두리
                Handles.BeginGUI();
                Handles.color = Color.white;
                Handles.DrawWireDisc(screenPos, Vector3.forward, size / 2);
                Handles.EndGUI();

                // 노드 라벨
                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.black }
                };

                string label = $"{node.roomType.ToString()[0]}\n{node.floor}";
                GUI.Label(nodeRect, label, labelStyle);
            }
        }

        private void DrawInfo(Rect graphRect)
        {
            if (currentGraph == null) return;

            // 통계 정보
            var allNodes = currentGraph.GetAllNodes();
            var allEdges = currentGraph.GetAllEdges();

            int normalCount = 0, eliteCount = 0, bossCount = 0, shopCount = 0, restCount = 0;

            foreach (var node in allNodes)
            {
                switch (node.roomType)
                {
                    case RoomType.Normal: normalCount++; break;
                    case RoomType.Elite: eliteCount++; break;
                    case RoomType.Boss: bossCount++; break;
                    case RoomType.Shop: shopCount++; break;
                    case RoomType.Rest: restCount++; break;
                }
            }

            string info = $"Nodes: {allNodes.Count} | Edges: {allEdges.Count}\n" +
                          $"Normal: {normalCount} | Elite: {eliteCount} | Boss: {bossCount}\n" +
                          $"Shop: {shopCount} | Rest: {restCount}";

            GUIStyle infoStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(10, graphRect.height - 50, 300, 50), info, infoStyle);
        }

        private void DrawEmptyMessage()
        {
            var rect = new Rect(0, 0, position.width, position.height);
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

            GUIStyle style = new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.gray }
            };

            GUI.Label(rect, "Select a DungeonConfig and click Generate", style);
        }


        // ====== 입력 처리 ======

        private void HandleInput()
        {
            Event e = Event.current;

            // 줌 (마우스 휠)
            if (e.type == EventType.ScrollWheel)
            {
                float zoomDelta = -e.delta.y * 0.05f;
                zoom = Mathf.Clamp(zoom + zoomDelta, 0.3f, 3f);
                e.Use();
                Repaint();
            }

            // 팬 (마우스 드래그)
            if (e.type == EventType.MouseDown && e.button == 2) // 중간 버튼
            {
                isDragging = true;
                lastMousePos = e.mousePosition;
                e.Use();
            }

            if (e.type == EventType.MouseDrag && isDragging)
            {
                panOffset += e.mousePosition - lastMousePos;
                lastMousePos = e.mousePosition;
                e.Use();
                Repaint();
            }

            if (e.type == EventType.MouseUp && e.button == 2)
            {
                isDragging = false;
                e.Use();
            }

            // 우클릭으로도 팬 가능
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                isDragging = true;
                lastMousePos = e.mousePosition;
                e.Use();
            }

            if (e.type == EventType.MouseDrag && isDragging && e.button == 1)
            {
                panOffset += e.mousePosition - lastMousePos;
                lastMousePos = e.mousePosition;
                e.Use();
                Repaint();
            }

            if (e.type == EventType.MouseUp && e.button == 1)
            {
                isDragging = false;
                e.Use();
            }
        }


        // ====== 그래프 생성 ======

        private void GenerateGraph()
        {
            if (dungeonConfig == null)
            {
                Debug.LogWarning("[DungeonGraphViewer] DungeonConfig가 설정되지 않았습니다.");
                return;
            }

            var graphBuilder = new GraphBuilder();
            currentGraph = graphBuilder.GenerateGraph(dungeonConfig, seed);

            if (currentGraph == null)
            {
                Debug.LogError("[DungeonGraphViewer] 그래프 생성 실패!");
                return;
            }

            // 노드 위치 계산
            CalculateNodePositions();

            Debug.Log($"[DungeonGraphViewer] 그래프 생성 완료: {currentGraph.NodeCount}개 노드, {currentGraph.EdgeCount}개 엣지");

            Repaint();
        }

        private void CalculateNodePositions()
        {
            nodePositions.Clear();

            if (currentGraph == null) return;

            // 층별로 노드 그룹핑
            var nodesByFloor = new Dictionary<int, List<DungeonNode>>();

            foreach (var node in currentGraph.GetAllNodes())
            {
                if (!nodesByFloor.ContainsKey(node.floor))
                {
                    nodesByFloor[node.floor] = new List<DungeonNode>();
                }
                nodesByFloor[node.floor].Add(node);
            }

            // 위치 계산
            foreach (var kvp in nodesByFloor)
            {
                int floor = kvp.Key;
                var nodes = kvp.Value;

                // 층 내 노드 정렬 (column 기준)
                nodes.Sort((a, b) => a.column.CompareTo(b.column));

                float y = CanvasPadding + floor * NodeSpacingY;
                float totalWidth = (nodes.Count - 1) * NodeSpacingX;
                float startX = CanvasPadding + 200f - totalWidth / 2f; // 중앙 정렬

                for (int i = 0; i < nodes.Count; i++)
                {
                    float x = startX + i * NodeSpacingX;
                    nodePositions[nodes[i].nodeId] = new Vector2(x, y);
                }
            }
        }


        // ====== 유틸리티 ======

        private Vector2 TransformPoint(Vector2 point)
        {
            return (point * zoom) + panOffset;
        }
    }
}
#endif
