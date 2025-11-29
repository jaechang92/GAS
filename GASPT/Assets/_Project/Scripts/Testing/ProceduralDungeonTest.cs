using System.Collections.Generic;
using UnityEngine;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Graph;
using GASPT.Gameplay.Level.Generation;
using GASPT.UI.Minimap;

namespace GASPT.Testing
{
    /// <summary>
    /// 절차적 던전 생성 테스트 스크립트
    /// 에디터에서 테스트용으로 사용
    /// </summary>
    public class ProceduralDungeonTest : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("던전 설정")]
        [SerializeField] private DungeonConfig dungeonConfig;
        [SerializeField] private int testSeed = 12345;
        [SerializeField] private bool useRandomSeed = false;

        [Header("테스트 옵션")]
        [SerializeField] private bool autoTestOnStart = false;
        [SerializeField] private bool logDetailedInfo = true;
        [SerializeField] private bool showGraphVisualization = true;

        [Header("시각화 설정")]
        [SerializeField] private float nodeSize = 30f;
        [SerializeField] private float horizontalSpacing = 80f;
        [SerializeField] private float verticalSpacing = 50f;

        [Header("참조 (선택)")]
        [SerializeField] private MinimapPresenter minimapPresenter;


        // ====== 상태 ======

        private DungeonGraph currentGraph;
        private DungeonGenerator generator;

        // 시각화용 캐시
        private Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();
        private Texture2D nodeTexture;
        private Texture2D lineTexture;
        private GUIStyle nodeStyle;
        private GUIStyle labelStyle;
        private bool stylesInitialized = false;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            generator = new DungeonGenerator();
        }

        private void Start()
        {
            if (autoTestOnStart)
            {
                TestGenerateDungeon();
            }
        }

        private void Update()
        {
            // 테스트 키 바인딩
            if (Input.GetKeyDown(KeyCode.G))
            {
                TestGenerateDungeon();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TestRandomSeed();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                TestValidation();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                TestPathfinding();
            }
        }


        // ====== 테스트 메서드 ======

        /// <summary>
        /// 던전 생성 테스트
        /// </summary>
        [ContextMenu("Test Generate Dungeon")]
        public void TestGenerateDungeon()
        {
            if (dungeonConfig == null)
            {
                Debug.LogError("[ProceduralDungeonTest] DungeonConfig가 설정되지 않았습니다!");
                return;
            }

            int seed = useRandomSeed ? SeedManager.GenerateRandomSeed() : testSeed;

            Debug.Log($"========== 던전 생성 테스트 시작 ==========");
            Debug.Log($"Seed: {seed}");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            currentGraph = generator.GenerateDungeonSync(dungeonConfig, seed);

            stopwatch.Stop();

            if (currentGraph == null)
            {
                Debug.LogError("[ProceduralDungeonTest] 그래프 생성 실패!");
                return;
            }

            Debug.Log($"생성 시간: {stopwatch.ElapsedMilliseconds}ms");
            Debug.Log($"노드 수: {currentGraph.NodeCount}");
            Debug.Log($"엣지 수: {currentGraph.EdgeCount}");

            if (logDetailedInfo)
            {
                LogDetailedGraphInfo();
            }

            // 미니맵에 표시 (있는 경우)
            if (minimapPresenter != null)
            {
                minimapPresenter.InitializeWithGraph(currentGraph);
                minimapPresenter.SetCurrentNode(currentGraph.EntryNode?.nodeId);
                minimapPresenter.ShowMinimap();
            }

            Debug.Log($"========== 던전 생성 테스트 완료 ==========");
        }

        /// <summary>
        /// 랜덤 시드 테스트
        /// </summary>
        [ContextMenu("Test Random Seeds")]
        public void TestRandomSeed()
        {
            if (dungeonConfig == null)
            {
                Debug.LogError("[ProceduralDungeonTest] DungeonConfig가 설정되지 않았습니다!");
                return;
            }

            Debug.Log($"========== 랜덤 시드 테스트 ==========");

            int successCount = 0;
            int totalTests = 10;
            var nodeCounts = new List<int>();
            var edgeCounts = new List<int>();

            for (int i = 0; i < totalTests; i++)
            {
                int seed = SeedManager.GenerateRandomSeed();
                var graph = generator.GenerateDungeonSync(dungeonConfig, seed);

                if (graph != null && graph.NodeCount > 0)
                {
                    successCount++;
                    nodeCounts.Add(graph.NodeCount);
                    edgeCounts.Add(graph.EdgeCount);
                    Debug.Log($"  [{i + 1}] Seed: {seed} -> 노드: {graph.NodeCount}, 엣지: {graph.EdgeCount}");
                }
                else
                {
                    Debug.LogWarning($"  [{i + 1}] Seed: {seed} -> 실패!");
                }
            }

            Debug.Log($"성공률: {successCount}/{totalTests}");

            if (nodeCounts.Count > 0)
            {
                float avgNodes = 0;
                float avgEdges = 0;
                foreach (var n in nodeCounts) avgNodes += n;
                foreach (var e in edgeCounts) avgEdges += e;
                avgNodes /= nodeCounts.Count;
                avgEdges /= edgeCounts.Count;

                Debug.Log($"평균 노드 수: {avgNodes:F1}");
                Debug.Log($"평균 엣지 수: {avgEdges:F1}");
            }

            Debug.Log($"========== 테스트 완료 ==========");
        }

        /// <summary>
        /// 그래프 유효성 검증 테스트
        /// </summary>
        [ContextMenu("Test Validation")]
        public void TestValidation()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("[ProceduralDungeonTest] 먼저 던전을 생성해주세요.");
                return;
            }

            Debug.Log($"========== 유효성 검증 테스트 ==========");

            bool isValid = currentGraph.ValidateGraph(out var errors);

            if (isValid)
            {
                Debug.Log("그래프 유효성 검증 통과!");
            }
            else
            {
                Debug.LogError($"그래프 유효성 검증 실패! ({errors.Count}개 오류)");
                foreach (var error in errors)
                {
                    Debug.LogError($"  - {error}");
                }
            }

            // 추가 검증
            Debug.Log($"시작 노드: {(currentGraph.EntryNode != null ? currentGraph.EntryNode.nodeId : "없음")}");
            Debug.Log($"보스 노드: {(currentGraph.BossNode != null ? currentGraph.BossNode.nodeId : "없음")}");

            Debug.Log($"========== 검증 완료 ==========");
        }

        /// <summary>
        /// 경로 탐색 테스트
        /// </summary>
        [ContextMenu("Test Pathfinding")]
        public void TestPathfinding()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("[ProceduralDungeonTest] 먼저 던전을 생성해주세요.");
                return;
            }

            if (currentGraph.EntryNode == null || currentGraph.BossNode == null)
            {
                Debug.LogError("[ProceduralDungeonTest] 시작 또는 보스 노드가 없습니다!");
                return;
            }

            Debug.Log($"========== 경로 탐색 테스트 ==========");

            var path = currentGraph.GetPath(currentGraph.EntryNode.nodeId, currentGraph.BossNode.nodeId);

            if (path == null || path.Count == 0)
            {
                Debug.LogError("경로를 찾을 수 없습니다!");
            }
            else
            {
                Debug.Log($"경로 길이: {path.Count}개 노드");
                Debug.Log("경로:");

                for (int i = 0; i < path.Count; i++)
                {
                    var node = path[i];
                    Debug.Log($"  [{i + 1}] {node.nodeId} ({node.roomType}) - 층 {node.floor}");
                }
            }

            Debug.Log($"========== 테스트 완료 ==========");
        }


        // ====== 유틸리티 ======

        private void LogDetailedGraphInfo()
        {
            if (currentGraph == null) return;

            Debug.Log("--- 상세 정보 ---");

            // 방 타입별 카운트
            var typeCounts = new Dictionary<RoomType, int>();
            foreach (var node in currentGraph.GetAllNodes())
            {
                if (!typeCounts.ContainsKey(node.roomType))
                {
                    typeCounts[node.roomType] = 0;
                }
                typeCounts[node.roomType]++;
            }

            Debug.Log("방 타입 분포:");
            foreach (var kvp in typeCounts)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}개");
            }

            // 층별 노드 수
            var floorCounts = new Dictionary<int, int>();
            foreach (var node in currentGraph.GetAllNodes())
            {
                if (!floorCounts.ContainsKey(node.floor))
                {
                    floorCounts[node.floor] = 0;
                }
                floorCounts[node.floor]++;
            }

            Debug.Log("층별 노드 수:");
            for (int i = 0; i < floorCounts.Count; i++)
            {
                if (floorCounts.ContainsKey(i))
                {
                    Debug.Log($"  층 {i}: {floorCounts[i]}개");
                }
            }

            // 분기점 수
            int branchPoints = 0;
            foreach (var node in currentGraph.GetAllNodes())
            {
                if (node.IsBranchPoint)
                {
                    branchPoints++;
                }
            }
            Debug.Log($"분기점 수: {branchPoints}개");
        }


        // ====== 에디터 GUI ======

        private void OnGUI()
        {
            InitializeStyles();

            // 좌측 상단: 조작 안내
            DrawControlPanel();

            // 우측: 그래프 시각화
            if (showGraphVisualization && currentGraph != null)
            {
                DrawGraphVisualization();
            }
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            // 노드 텍스처 생성
            nodeTexture = new Texture2D(1, 1);
            nodeTexture.SetPixel(0, 0, Color.white);
            nodeTexture.Apply();

            // 라인 텍스처 생성
            lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.8f));
            lineTexture.Apply();

            // 노드 스타일
            nodeStyle = new GUIStyle(GUI.skin.box);
            nodeStyle.alignment = TextAnchor.MiddleCenter;
            nodeStyle.fontSize = 10;
            nodeStyle.fontStyle = FontStyle.Bold;

            // 라벨 스타일
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 9;

            stylesInitialized = true;
        }

        private void DrawControlPanel()
        {
            GUILayout.BeginArea(new Rect(10, 10, 280, 220));

            // 배경
            GUI.Box(new Rect(0, 0, 280, 220), "");

            GUILayout.Space(5);
            GUILayout.Label("=== 절차적 던전 테스트 ===", GUI.skin.box);

            GUILayout.Space(5);

            GUILayout.Label("키 바인딩:");
            GUILayout.Label("  G: 던전 생성");
            GUILayout.Label("  R: 랜덤 시드 테스트 (10회)");
            GUILayout.Label("  V: 유효성 검증");
            GUILayout.Label("  P: 경로 탐색 테스트");

            GUILayout.Space(5);

            if (currentGraph != null)
            {
                GUILayout.Label($"노드: {currentGraph.NodeCount}, 엣지: {currentGraph.EdgeCount}");
                GUILayout.Label($"시작→보스 경로: {currentGraph.GetShortestPathLength()}");
            }
            else
            {
                GUI.color = Color.yellow;
                GUILayout.Label("그래프 없음 - G를 눌러 생성");
                GUI.color = Color.white;
            }

            GUILayout.EndArea();
        }

        private void DrawGraphVisualization()
        {
            // 시각화 영역 (화면 오른쪽)
            float panelWidth = 400f;
            float panelHeight = Screen.height - 40f;
            float panelX = Screen.width - panelWidth - 20f;
            float panelY = 20f;

            // 배경 패널
            GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "던전 그래프");

            // 노드 위치 계산
            CalculateNodePositions(panelX, panelY, panelWidth, panelHeight);

            // 엣지 그리기 (노드 아래에)
            DrawEdges();

            // 노드 그리기
            DrawNodes();

            // 범례
            DrawLegend(panelX, panelY + panelHeight - 80f);
        }

        private void CalculateNodePositions(float panelX, float panelY, float panelWidth, float panelHeight)
        {
            nodePositions.Clear();

            if (currentGraph == null) return;

            float contentX = panelX + 20f;
            float contentY = panelY + 40f;
            float contentWidth = panelWidth - 40f;
            float contentHeight = panelHeight - 140f;

            // 층별로 노드 배치
            int maxFloor = 0;
            var floorNodes = new Dictionary<int, List<DungeonNode>>();

            foreach (var node in currentGraph.GetAllNodes())
            {
                if (!floorNodes.ContainsKey(node.floor))
                {
                    floorNodes[node.floor] = new List<DungeonNode>();
                }
                floorNodes[node.floor].Add(node);

                if (node.floor > maxFloor)
                {
                    maxFloor = node.floor;
                }
            }

            // 각 층의 노드들 위치 계산
            float floorHeight = contentHeight / Mathf.Max(1, maxFloor + 1);

            foreach (var kvp in floorNodes)
            {
                int floor = kvp.Key;
                var nodes = kvp.Value;

                // Y 위치 (아래에서 위로)
                float y = contentY + contentHeight - (floor + 0.5f) * floorHeight;

                // X 위치 (균등 분배)
                float nodeWidth = contentWidth / Mathf.Max(1, nodes.Count);

                for (int i = 0; i < nodes.Count; i++)
                {
                    float x = contentX + (i + 0.5f) * nodeWidth;
                    nodePositions[nodes[i].nodeId] = new Vector2(x, y);
                }
            }
        }

        private void DrawEdges()
        {
            if (currentGraph == null) return;

            foreach (var edge in currentGraph.GetAllEdges())
            {
                if (!nodePositions.ContainsKey(edge.fromNodeId) ||
                    !nodePositions.ContainsKey(edge.toNodeId))
                    continue;

                Vector2 from = nodePositions[edge.fromNodeId];
                Vector2 to = nodePositions[edge.toNodeId];

                // 엣지 색상
                Color edgeColor = edge.edgeType switch
                {
                    EdgeType.Normal => new Color(0.6f, 0.6f, 0.6f),
                    EdgeType.Secret => new Color(0.8f, 0.5f, 0.8f),
                    EdgeType.Locked => new Color(0.8f, 0.4f, 0.2f),
                    _ => Color.gray
                };

                DrawLine(from, to, edgeColor, 2f);
            }
        }

        private void DrawLine(Vector2 from, Vector2 to, Color color, float width)
        {
            // 저장된 GUI 행렬
            Matrix4x4 matrix = GUI.matrix;

            // 라인 길이와 각도 계산
            float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
            float length = Vector2.Distance(from, to);

            // 피벗을 라인 시작점으로 설정
            GUIUtility.RotateAroundPivot(angle, from);

            // 라인 그리기
            GUI.color = color;
            GUI.DrawTexture(new Rect(from.x, from.y - width / 2, length, width), lineTexture);
            GUI.color = Color.white;

            // 행렬 복원
            GUI.matrix = matrix;
        }

        private void DrawNodes()
        {
            if (currentGraph == null) return;

            foreach (var node in currentGraph.GetAllNodes())
            {
                if (!nodePositions.ContainsKey(node.nodeId)) continue;

                Vector2 pos = nodePositions[node.nodeId];
                float size = nodeSize;

                // 노드 색상 (방 타입별)
                Color nodeColor = GetRoomTypeColor(node.roomType);

                // 현재 노드 하이라이트
                bool isEntry = node.nodeId == currentGraph.entryNodeId;
                bool isBoss = node.nodeId == currentGraph.bossNodeId;

                if (isEntry || isBoss)
                {
                    // 외곽선 효과
                    GUI.color = isEntry ? Color.green : Color.red;
                    GUI.DrawTexture(new Rect(pos.x - size / 2 - 3, pos.y - size / 2 - 3, size + 6, size + 6), nodeTexture);
                }

                // 노드 배경
                GUI.color = nodeColor;
                GUI.DrawTexture(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), nodeTexture);

                // 노드 라벨 (방 타입 약자)
                GUI.color = Color.white;
                string label = GetRoomTypeLabel(node.roomType);
                GUI.Label(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), label, nodeStyle);

                // 층 번호 (노드 아래)
                GUI.color = new Color(0.8f, 0.8f, 0.8f);
                GUI.Label(new Rect(pos.x - 15, pos.y + size / 2, 30, 15), $"F{node.floor}", labelStyle);

                GUI.color = Color.white;
            }
        }

        private Color GetRoomTypeColor(RoomType type)
        {
            return type switch
            {
                RoomType.Start => new Color(0.2f, 0.7f, 0.2f),      // 초록
                RoomType.Normal => new Color(0.4f, 0.4f, 0.5f),     // 회색
                RoomType.Elite => new Color(0.8f, 0.5f, 0.2f),      // 주황
                RoomType.Boss => new Color(0.8f, 0.2f, 0.2f),       // 빨강
                RoomType.Shop => new Color(0.9f, 0.8f, 0.2f),       // 노랑
                RoomType.Rest => new Color(0.2f, 0.6f, 0.8f),       // 파랑
                RoomType.Treasure => new Color(0.9f, 0.7f, 0.3f),   // 금색
                _ => Color.gray
            };
        }

        private string GetRoomTypeLabel(RoomType type)
        {
            return type switch
            {
                RoomType.Start => "S",
                RoomType.Normal => "N",
                RoomType.Elite => "!",
                RoomType.Boss => "B",
                RoomType.Shop => "$",
                RoomType.Rest => "R",
                RoomType.Treasure => "T",
                _ => "?"
            };
        }

        private void DrawLegend(float x, float y)
        {
            GUI.Box(new Rect(x, y, 380, 70), "범례");

            float legendX = x + 10;
            float legendY = y + 20;
            float itemWidth = 50;
            float itemHeight = 20;

            var types = new[] {
                (RoomType.Start, "시작"),
                (RoomType.Normal, "일반"),
                (RoomType.Elite, "엘리트"),
                (RoomType.Boss, "보스"),
                (RoomType.Shop, "상점"),
                (RoomType.Rest, "휴식"),
                (RoomType.Treasure, "보물")
            };

            for (int i = 0; i < types.Length; i++)
            {
                int row = i / 4;  // 한 줄에 4개
                int col = i % 4;

                float itemX = legendX + col * (itemWidth + 60);
                float itemY = legendY + row * (itemHeight + 5);

                // 색상 박스
                GUI.color = GetRoomTypeColor(types[i].Item1);
                GUI.DrawTexture(new Rect(itemX, itemY, 15, 15), nodeTexture);

                // 라벨
                GUI.color = Color.white;
                GUI.Label(new Rect(itemX + 20, itemY - 2, 50, 20), types[i].Item2);
            }

            GUI.color = Color.white;
        }
    }
}
