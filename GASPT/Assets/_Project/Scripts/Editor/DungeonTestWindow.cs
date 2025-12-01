using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Graph;
using GASPT.Gameplay.Level.Generation;

namespace GASPT.Editor
{
    /// <summary>
    /// 던전 생성 시스템 종합 테스트 에디터 윈도우
    /// Play 모드 없이 던전 그래프 생성 및 검증 가능
    /// Menu: Tools > GASPT > Dungeon Test Window
    /// </summary>
    public class DungeonTestWindow : EditorWindow
    {
        // ====== 설정 ======
        private DungeonConfig dungeonConfig;
        private int testSeed = 12345;
        private bool useRandomSeed = false;
        private int batchTestCount = 10;

        // ====== 상태 ======
        private DungeonGraph currentGraph;
        private DungeonGenerator generator;
        private List<TestResult> testResults = new List<TestResult>();
        private Vector2 scrollPosition;
        private Vector2 graphScrollPosition;
        private int selectedTab = 0;
        private string[] tabNames = { "단일 테스트", "배치 테스트", "그래프 뷰", "통계" };

        // ====== 시각화 ======
        private Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();
        private float nodeSize = 25f;
        private float graphScale = 1f;

        // ====== 스타일 ======
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle resultBoxStyle;
        private bool stylesInitialized = false;


        [MenuItem("Tools/GASPT/Procedural Dungeon/🧪 Dungeon Test Window", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<DungeonTestWindow>("던전 테스트");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }


        private void OnEnable()
        {
            generator = new DungeonGenerator();
            TryLoadDefaultConfig();
        }

        private void TryLoadDefaultConfig()
        {
            if (dungeonConfig == null)
            {
                dungeonConfig = AssetDatabase.LoadAssetAtPath<DungeonConfig>(
                    "Assets/Resources/Data/Dungeons/TestDungeon.asset");
            }
        }


        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            resultBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(5, 5, 5, 5),
                margin = new RectOffset(0, 0, 2, 2)
            };

            stylesInitialized = true;
        }


        private void OnGUI()
        {
            InitializeStyles();

            // 헤더
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🏰 던전 생성 시스템 테스트", headerStyle);
            EditorGUILayout.Space(5);

            // 설정 영역
            DrawConfigSection();

            EditorGUILayout.Space(10);

            // 탭 선택
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            EditorGUILayout.Space(10);

            // 탭별 내용
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0:
                    DrawSingleTestTab();
                    break;
                case 1:
                    DrawBatchTestTab();
                    break;
                case 2:
                    DrawGraphViewTab();
                    break;
                case 3:
                    DrawStatisticsTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }


        // ====== 설정 영역 ======

        private void DrawConfigSection()
        {
            EditorGUILayout.BeginVertical(boxStyle);

            EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);

            dungeonConfig = (DungeonConfig)EditorGUILayout.ObjectField(
                "Dungeon Config", dungeonConfig, typeof(DungeonConfig), false);

            EditorGUILayout.BeginHorizontal();
            useRandomSeed = EditorGUILayout.Toggle("랜덤 시드", useRandomSeed);

            EditorGUI.BeginDisabledGroup(useRandomSeed);
            testSeed = EditorGUILayout.IntField("시드", testSeed);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("새 시드", GUILayout.Width(60)))
            {
                testSeed = SeedManager.GenerateRandomSeed();
            }
            EditorGUILayout.EndHorizontal();

            if (dungeonConfig == null)
            {
                EditorGUILayout.HelpBox("DungeonConfig를 설정해주세요.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }


        // ====== 단일 테스트 탭 ======

        private void DrawSingleTestTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);

            EditorGUILayout.LabelField("단일 던전 생성 테스트", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(dungeonConfig == null);

            if (GUILayout.Button("🎲 던전 생성", GUILayout.Height(30)))
            {
                GenerateSingleDungeon();
            }

            if (GUILayout.Button("✅ 유효성 검증", GUILayout.Height(30)))
            {
                ValidateCurrentGraph();
            }

            if (GUILayout.Button("🛤️ 경로 탐색", GUILayout.Height(30)))
            {
                FindPathToBoss();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 현재 그래프 정보
            if (currentGraph != null)
            {
                DrawCurrentGraphInfo();
            }
            else
            {
                EditorGUILayout.HelpBox("던전을 생성해주세요.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void GenerateSingleDungeon()
        {
            if (dungeonConfig == null) return;

            int seed = useRandomSeed ? SeedManager.GenerateRandomSeed() : testSeed;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            currentGraph = generator.GenerateDungeonSync(dungeonConfig, seed);
            stopwatch.Stop();

            if (currentGraph != null)
            {
                Debug.Log($"[DungeonTest] 던전 생성 완료 - Seed: {seed}, " +
                         $"노드: {currentGraph.NodeCount}, 엣지: {currentGraph.EdgeCount}, " +
                         $"시간: {stopwatch.ElapsedMilliseconds}ms");

                // 그래프 뷰 탭으로 이동
                selectedTab = 2;
            }
            else
            {
                Debug.LogError("[DungeonTest] 던전 생성 실패!");
            }

            Repaint();
        }

        private void ValidateCurrentGraph()
        {
            if (currentGraph == null)
            {
                Debug.LogWarning("[DungeonTest] 먼저 던전을 생성해주세요.");
                return;
            }

            bool isValid = currentGraph.ValidateGraph(out var errors);

            if (isValid)
            {
                Debug.Log("[DungeonTest] ✅ 그래프 유효성 검증 통과!");
            }
            else
            {
                Debug.LogError($"[DungeonTest] ❌ 유효성 검증 실패 ({errors.Count}개 오류):");
                foreach (var error in errors)
                {
                    Debug.LogError($"  - {error}");
                }
            }
        }

        private void FindPathToBoss()
        {
            if (currentGraph == null || currentGraph.EntryNode == null || currentGraph.BossNode == null)
            {
                Debug.LogWarning("[DungeonTest] 유효한 그래프가 필요합니다.");
                return;
            }

            var path = currentGraph.GetPath(currentGraph.entryNodeId, currentGraph.bossNodeId);

            if (path != null && path.Count > 0)
            {
                Debug.Log($"[DungeonTest] 🛤️ Entry→Boss 경로 ({path.Count}개 노드):");
                for (int i = 0; i < path.Count; i++)
                {
                    Debug.Log($"  [{i + 1}] {path[i].nodeId} ({path[i].roomType}) - Floor {path[i].floor}");
                }
            }
            else
            {
                Debug.LogError("[DungeonTest] ❌ 경로를 찾을 수 없습니다!");
            }
        }

        private void DrawCurrentGraphInfo()
        {
            EditorGUILayout.BeginVertical(resultBoxStyle);

            EditorGUILayout.LabelField("현재 그래프 정보", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"시드: {currentGraph.seed}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"노드: {currentGraph.NodeCount}", GUILayout.Width(100));
            EditorGUILayout.LabelField($"엣지: {currentGraph.EdgeCount}", GUILayout.Width(100));
            EditorGUILayout.LabelField($"층: {currentGraph.totalFloors}", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 방 타입별 카운트
            var typeStats = currentGraph.GetRoomTypeStats();
            EditorGUILayout.LabelField("방 타입 분포:");

            EditorGUILayout.BeginHorizontal();
            foreach (var kvp in typeStats)
            {
                EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value}", GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();

            // 경로 정보
            EditorGUILayout.Space(5);
            int pathLength = currentGraph.GetShortestPathLength();
            int branchCount = currentGraph.GetBranchPointCount();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"최단 경로: {pathLength}", GUILayout.Width(120));
            EditorGUILayout.LabelField($"분기점: {branchCount}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }


        // ====== 배치 테스트 탭 ======

        private void DrawBatchTestTab()
        {
            EditorGUILayout.BeginVertical(boxStyle);

            EditorGUILayout.LabelField("배치 테스트 (다중 시드)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            batchTestCount = EditorGUILayout.IntSlider("테스트 횟수", batchTestCount, 5, 100);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(dungeonConfig == null);

            if (GUILayout.Button("🚀 배치 테스트 실행", GUILayout.Height(30)))
            {
                RunBatchTest();
            }

            if (GUILayout.Button("🗑️ 결과 초기화", GUILayout.Height(30)))
            {
                testResults.Clear();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 테스트 결과
            if (testResults.Count > 0)
            {
                DrawBatchTestResults();
            }
            else
            {
                EditorGUILayout.HelpBox("배치 테스트를 실행해주세요.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void RunBatchTest()
        {
            if (dungeonConfig == null) return;

            testResults.Clear();

            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < batchTestCount; i++)
            {
                int seed = SeedManager.GenerateRandomSeed();
                var result = new TestResult { seed = seed, testIndex = i + 1 };

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    var graph = generator.GenerateDungeonSync(dungeonConfig, seed);
                    stopwatch.Stop();

                    if (graph != null)
                    {
                        result.success = true;
                        result.nodeCount = graph.NodeCount;
                        result.edgeCount = graph.EdgeCount;
                        result.floorCount = graph.totalFloors;
                        result.pathLength = graph.GetShortestPathLength();
                        result.branchCount = graph.GetBranchPointCount();
                        result.generationTimeMs = stopwatch.ElapsedMilliseconds;

                        // 유효성 검증
                        result.isValid = graph.ValidateGraph(out var errors);
                        result.validationErrors = errors;
                    }
                    else
                    {
                        result.success = false;
                        result.errorMessage = "그래프 생성 실패";
                    }
                }
                catch (System.Exception e)
                {
                    result.success = false;
                    result.errorMessage = e.Message;
                }

                testResults.Add(result);

                // 진행 상황 표시
                EditorUtility.DisplayProgressBar("배치 테스트",
                    $"테스트 중... ({i + 1}/{batchTestCount})",
                    (float)(i + 1) / batchTestCount);
            }

            totalStopwatch.Stop();
            EditorUtility.ClearProgressBar();

            int successCount = testResults.Count(r => r.success);
            int validCount = testResults.Count(r => r.isValid);

            Debug.Log($"[DungeonTest] 배치 테스트 완료: {successCount}/{batchTestCount} 성공, " +
                     $"{validCount}/{batchTestCount} 유효, " +
                     $"총 시간: {totalStopwatch.ElapsedMilliseconds}ms");

            // 통계 탭으로 이동
            selectedTab = 3;
            Repaint();
        }

        private void DrawBatchTestResults()
        {
            EditorGUILayout.LabelField($"테스트 결과 ({testResults.Count}개)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            int successCount = testResults.Count(r => r.success);
            int validCount = testResults.Count(r => r.isValid);

            EditorGUILayout.BeginHorizontal();

            // 성공률 표시
            GUI.color = successCount == testResults.Count ? Color.green : Color.yellow;
            EditorGUILayout.LabelField($"성공: {successCount}/{testResults.Count}", GUILayout.Width(100));

            GUI.color = validCount == testResults.Count ? Color.green : Color.yellow;
            EditorGUILayout.LabelField($"유효: {validCount}/{testResults.Count}", GUILayout.Width(100));

            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 개별 결과 (실패한 것만 표시)
            var failedResults = testResults.Where(r => !r.success || !r.isValid).ToList();

            if (failedResults.Count > 0)
            {
                EditorGUILayout.LabelField("⚠️ 실패/무효 항목:", EditorStyles.boldLabel);

                foreach (var result in failedResults)
                {
                    EditorGUILayout.BeginHorizontal(resultBoxStyle);

                    GUI.color = result.success ? Color.yellow : Color.red;
                    EditorGUILayout.LabelField($"[{result.testIndex}] Seed: {result.seed}", GUILayout.Width(200));

                    if (!result.success)
                    {
                        EditorGUILayout.LabelField($"❌ {result.errorMessage}");
                    }
                    else if (!result.isValid)
                    {
                        EditorGUILayout.LabelField($"⚠️ 유효성 오류: {result.validationErrors?.Count ?? 0}개");
                    }

                    GUI.color = Color.white;

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("모든 테스트가 성공했습니다!", MessageType.Info);
            }
        }


        // ====== 그래프 뷰 탭 ======

        private void DrawGraphViewTab()
        {
            if (currentGraph == null)
            {
                EditorGUILayout.HelpBox("먼저 던전을 생성해주세요.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(boxStyle);

            EditorGUILayout.LabelField("그래프 시각화", EditorStyles.boldLabel);

            // 스케일 조절
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("확대/축소:", GUILayout.Width(70));
            graphScale = EditorGUILayout.Slider(graphScale, 0.5f, 2f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // 그래프 그리기 영역
            Rect graphArea = GUILayoutUtility.GetRect(position.width - 40, 400);
            GUI.Box(graphArea, "");

            CalculateNodePositions(graphArea);
            DrawGraphInRect(graphArea);

            // 범례
            DrawLegend();
        }

        private void CalculateNodePositions(Rect area)
        {
            nodePositions.Clear();

            if (currentGraph == null) return;

            // 층별 노드 그룹화
            var floorNodes = new Dictionary<int, List<DungeonNode>>();
            int maxFloor = 0;

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

            float padding = 30f * graphScale;
            float contentWidth = area.width - padding * 2;
            float contentHeight = area.height - padding * 2;
            float floorHeight = contentHeight / Mathf.Max(1, maxFloor + 1);

            foreach (var kvp in floorNodes)
            {
                int floor = kvp.Key;
                var nodes = kvp.Value;

                // Y 위치 (아래에서 위로)
                float y = area.y + padding + contentHeight - (floor + 0.5f) * floorHeight;

                // X 위치 (균등 분배)
                float nodeWidth = contentWidth / Mathf.Max(1, nodes.Count);

                for (int i = 0; i < nodes.Count; i++)
                {
                    float x = area.x + padding + (i + 0.5f) * nodeWidth;
                    nodePositions[nodes[i].nodeId] = new Vector2(x, y);
                }
            }
        }

        private void DrawGraphInRect(Rect area)
        {
            if (currentGraph == null) return;

            // 엣지 그리기
            Handles.BeginGUI();

            foreach (var edge in currentGraph.GetAllEdges())
            {
                if (!nodePositions.ContainsKey(edge.fromNodeId) ||
                    !nodePositions.ContainsKey(edge.toNodeId))
                    continue;

                Vector2 from = nodePositions[edge.fromNodeId];
                Vector2 to = nodePositions[edge.toNodeId];

                Color edgeColor = edge.edgeType switch
                {
                    EdgeType.Normal => new Color(0.5f, 0.5f, 0.5f),
                    EdgeType.Secret => new Color(0.8f, 0.4f, 0.8f),
                    EdgeType.Locked => new Color(0.8f, 0.3f, 0.2f),
                    _ => Color.gray
                };

                Handles.color = edgeColor;
                Handles.DrawLine(from, to, 2f);
            }

            Handles.EndGUI();

            // 노드 그리기
            float size = nodeSize * graphScale;

            foreach (var node in currentGraph.GetAllNodes())
            {
                if (!nodePositions.ContainsKey(node.nodeId)) continue;

                Vector2 pos = nodePositions[node.nodeId];
                Rect nodeRect = new Rect(pos.x - size / 2, pos.y - size / 2, size, size);

                // 노드 색상
                Color nodeColor = GetRoomTypeColor(node.roomType);

                // Entry/Boss 하이라이트
                bool isEntry = node.nodeId == currentGraph.entryNodeId;
                bool isBoss = node.nodeId == currentGraph.bossNodeId;

                if (isEntry || isBoss)
                {
                    Rect highlightRect = new Rect(nodeRect.x - 3, nodeRect.y - 3,
                                                   nodeRect.width + 6, nodeRect.height + 6);
                    EditorGUI.DrawRect(highlightRect, isEntry ? Color.green : Color.red);
                }

                // 노드 배경
                EditorGUI.DrawRect(nodeRect, nodeColor);

                // 노드 라벨
                string label = GetRoomTypeLabel(node.roomType);
                GUI.Label(nodeRect, label, new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                });

                // 층 번호
                Rect floorRect = new Rect(pos.x - 10, pos.y + size / 2 + 2, 20, 15);
                GUI.Label(floorRect, $"F{node.floor}", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9,
                    normal = { textColor = Color.gray }
                });
            }
        }

        private void DrawLegend()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            var types = new[]
            {
                (RoomType.Start, "시작", GetRoomTypeColor(RoomType.Start)),
                (RoomType.Normal, "일반", GetRoomTypeColor(RoomType.Normal)),
                (RoomType.Elite, "엘리트", GetRoomTypeColor(RoomType.Elite)),
                (RoomType.Boss, "보스", GetRoomTypeColor(RoomType.Boss)),
                (RoomType.Shop, "상점", GetRoomTypeColor(RoomType.Shop)),
                (RoomType.Rest, "휴식", GetRoomTypeColor(RoomType.Rest)),
                (RoomType.Treasure, "보물", GetRoomTypeColor(RoomType.Treasure))
            };

            foreach (var (type, name, color) in types)
            {
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(15, 15), color);
                EditorGUILayout.LabelField(name, GUILayout.Width(45));
            }

            EditorGUILayout.EndHorizontal();
        }


        // ====== 통계 탭 ======

        private void DrawStatisticsTab()
        {
            if (testResults.Count == 0)
            {
                EditorGUILayout.HelpBox("배치 테스트를 실행하면 통계를 볼 수 있습니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(boxStyle);

            EditorGUILayout.LabelField("📊 테스트 통계", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var successResults = testResults.Where(r => r.success).ToList();

            if (successResults.Count == 0)
            {
                EditorGUILayout.HelpBox("성공한 테스트가 없습니다.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            // 기본 통계
            DrawStatRow("테스트 수", $"{testResults.Count}");
            DrawStatRow("성공률", $"{(float)successResults.Count / testResults.Count * 100:F1}%");
            DrawStatRow("유효성 통과율",
                $"{(float)successResults.Count(r => r.isValid) / successResults.Count * 100:F1}%");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("노드 통계", EditorStyles.boldLabel);

            float avgNodes = ((float)successResults.Average(r => r.nodeCount));
            float avgEdges = ((float)successResults.Average(r => r.edgeCount));
            float avgPath = ((float)successResults.Average(r => r.pathLength));
            float avgBranch = ((float)successResults.Average(r => r.branchCount));
            float avgTime = ((float)successResults.Average(r => r.generationTimeMs));

            DrawStatRow("평균 노드 수", $"{avgNodes:F1}");
            DrawStatRow("평균 엣지 수", $"{avgEdges:F1}");
            DrawStatRow("평균 경로 길이", $"{avgPath:F1}");
            DrawStatRow("평균 분기점", $"{avgBranch:F1}");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("성능", EditorStyles.boldLabel);

            DrawStatRow("평균 생성 시간", $"{avgTime:F1}ms");
            DrawStatRow("최소 시간", $"{successResults.Min(r => r.generationTimeMs)}ms");
            DrawStatRow("최대 시간", $"{successResults.Max(r => r.generationTimeMs)}ms");

            EditorGUILayout.EndVertical();
        }

        private void DrawStatRow(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));
            EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }


        // ====== 유틸리티 ======

        private Color GetRoomTypeColor(RoomType type)
        {
            return type switch
            {
                RoomType.Start => new Color(0.2f, 0.7f, 0.2f),
                RoomType.Normal => new Color(0.4f, 0.4f, 0.5f),
                RoomType.Elite => new Color(0.8f, 0.5f, 0.2f),
                RoomType.Boss => new Color(0.8f, 0.2f, 0.2f),
                RoomType.Shop => new Color(0.9f, 0.8f, 0.2f),
                RoomType.Rest => new Color(0.2f, 0.6f, 0.8f),
                RoomType.Treasure => new Color(0.9f, 0.7f, 0.3f),
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


        // ====== 테스트 결과 구조체 ======

        private struct TestResult
        {
            public int testIndex;
            public int seed;
            public bool success;
            public bool isValid;
            public int nodeCount;
            public int edgeCount;
            public int floorCount;
            public int pathLength;
            public int branchCount;
            public long generationTimeMs;
            public string errorMessage;
            public List<string> validationErrors;
        }
    }
}
