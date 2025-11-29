#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Generation;

namespace GASPT.Editor
{
    /// <summary>
    /// DungeonConfig 커스텀 에디터
    /// </summary>
    [CustomEditor(typeof(DungeonConfig))]
    public class DungeonConfigEditor : UnityEditor.Editor
    {
        private DungeonConfig config;
        private bool showPreview = false;
        private int previewSeed = 12345;

        private SerializedProperty dungeonNameProp;
        private SerializedProperty recommendedLevelProp;
        private SerializedProperty descriptionProp;
        private SerializedProperty generationTypeProp;
        private SerializedProperty roomPrefabsProp;
        private SerializedProperty roomDataListProp;
        private SerializedProperty roomTemplatePrefabProp;
        private SerializedProperty generationRulesProp;
        private SerializedProperty roomDataPoolProp;


        private void OnEnable()
        {
            config = (DungeonConfig)target;

            dungeonNameProp = serializedObject.FindProperty("dungeonName");
            recommendedLevelProp = serializedObject.FindProperty("recommendedLevel");
            descriptionProp = serializedObject.FindProperty("description");
            generationTypeProp = serializedObject.FindProperty("generationType");
            roomPrefabsProp = serializedObject.FindProperty("roomPrefabs");
            roomDataListProp = serializedObject.FindProperty("roomDataList");
            roomTemplatePrefabProp = serializedObject.FindProperty("roomTemplatePrefab");
            generationRulesProp = serializedObject.FindProperty("generationRules");
            roomDataPoolProp = serializedObject.FindProperty("roomDataPool");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 기본 정보
            EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dungeonNameProp, new GUIContent("던전 이름"));
            EditorGUILayout.PropertyField(recommendedLevelProp, new GUIContent("권장 레벨"));
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("설명"));

            EditorGUILayout.Space(10);

            // 생성 방식
            EditorGUILayout.LabelField("생성 방식", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(generationTypeProp, new GUIContent("생성 타입"));

            EditorGUILayout.Space(5);

            // 생성 방식에 따른 필드 표시
            DungeonGenerationType genType = (DungeonGenerationType)generationTypeProp.enumValueIndex;

            switch (genType)
            {
                case DungeonGenerationType.Prefab:
                    DrawPrefabMode();
                    break;

                case DungeonGenerationType.Data:
                    DrawDataMode();
                    break;

                case DungeonGenerationType.Procedural:
                    DrawProceduralMode();
                    break;
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(15);

            // 프리뷰 섹션
            DrawPreviewSection();
        }


        private void DrawPrefabMode()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Prefab 방식", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("미리 디자인된 Room Prefab을 순서대로 사용합니다.", MessageType.Info);

            EditorGUILayout.PropertyField(roomPrefabsProp, new GUIContent("Room Prefabs"), true);

            if (roomPrefabsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Room Prefab을 추가해주세요!", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawDataMode()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Data 방식", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("RoomData로 Room을 동적 생성합니다.", MessageType.Info);

            EditorGUILayout.PropertyField(roomTemplatePrefabProp, new GUIContent("Room 템플릿"));
            EditorGUILayout.PropertyField(roomDataListProp, new GUIContent("Room Data 목록"), true);

            if (roomTemplatePrefabProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Room 템플릿 Prefab을 설정해주세요!", MessageType.Warning);
            }

            if (roomDataListProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("RoomData를 추가해주세요!", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawProceduralMode()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Procedural 방식", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("룰 기반으로 던전을 자동 생성합니다. (Slay the Spire 스타일)", MessageType.Info);

            EditorGUILayout.PropertyField(generationRulesProp, new GUIContent("생성 규칙"));
            EditorGUILayout.PropertyField(roomDataPoolProp, new GUIContent("RoomData 풀"), true);

            if (generationRulesProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("생성 규칙(RoomGenerationRules)을 설정해주세요!", MessageType.Warning);
            }

            if (roomDataPoolProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("RoomData 풀을 추가해주세요!", MessageType.Warning);
            }

            // 빠른 생성 규칙 조정
            if (generationRulesProp.objectReferenceValue != null)
            {
                EditorGUILayout.Space(5);
                DrawQuickRulesEditor();
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawQuickRulesEditor()
        {
            var rules = (RoomGenerationRules)generationRulesProp.objectReferenceValue;
            if (rules == null) return;

            EditorGUILayout.LabelField("빠른 규칙 조정", EditorStyles.miniBoldLabel);

            EditorGUI.BeginChangeCheck();

            rules.totalFloors = EditorGUILayout.IntSlider("총 층 수", rules.totalFloors, 3, 20);
            rules.branchingFactor = EditorGUILayout.Slider("분기 확률", rules.branchingFactor, 0f, 1f);
            rules.maxNodesPerFloor = EditorGUILayout.IntSlider("층당 최대 노드", rules.maxNodesPerFloor, 1, 4);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(rules);
            }
        }


        private void DrawPreviewSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            showPreview = EditorGUILayout.Foldout(showPreview, "그래프 미리보기", true);

            if (showPreview)
            {
                EditorGUILayout.BeginHorizontal();

                previewSeed = EditorGUILayout.IntField("Seed", previewSeed);

                if (GUILayout.Button("랜덤", GUILayout.Width(50)))
                {
                    previewSeed = Random.Range(0, int.MaxValue);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Graph Viewer에서 열기"))
                {
                    OpenInGraphViewer();
                }

                if (GUILayout.Button("콘솔에 출력"))
                {
                    PrintGraphToConsole();
                }

                EditorGUILayout.EndHorizontal();

                // 간단한 통계
                if (config.generationType == DungeonGenerationType.Procedural)
                {
                    DrawQuickStats();
                }
            }

            EditorGUILayout.EndVertical();
        }


        private void DrawQuickStats()
        {
            if (config.generationRules == null) return;

            var rules = config.generationRules;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("예상 통계", EditorStyles.miniBoldLabel);

            int minNodes = rules.totalFloors * rules.minNodesPerFloor;
            int maxNodes = rules.totalFloors * rules.maxNodesPerFloor;

            EditorGUILayout.LabelField($"예상 노드 수: {minNodes} ~ {maxNodes}");
            EditorGUILayout.LabelField($"예상 엘리트: {Mathf.FloorToInt(maxNodes * rules.eliteRoomRatio)}개");
            EditorGUILayout.LabelField($"예상 상점: {Mathf.FloorToInt(maxNodes * rules.shopRoomRatio)}개");
        }


        private void OpenInGraphViewer()
        {
            var viewer = EditorWindow.GetWindow<DungeonGraphViewer>("Dungeon Graph Viewer");

            // DungeonConfig와 seed 설정 (리플렉션 또는 public 메서드로)
            var field = typeof(DungeonGraphViewer).GetField("dungeonConfig",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(viewer, config);

            var seedField = typeof(DungeonGraphViewer).GetField("seed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (seedField != null) seedField.SetValue(viewer, previewSeed);

            // Generate 호출
            var method = typeof(DungeonGraphViewer).GetMethod("GenerateGraph",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null) method.Invoke(viewer, null);
        }


        private void PrintGraphToConsole()
        {
            if (config.generationType != DungeonGenerationType.Procedural)
            {
                Debug.Log("[DungeonConfigEditor] Procedural 타입만 미리보기 가능합니다.");
                return;
            }

            var graphBuilder = new GraphBuilder();
            var graph = graphBuilder.GenerateGraph(config, previewSeed);

            if (graph == null)
            {
                Debug.LogError("[DungeonConfigEditor] 그래프 생성 실패!");
                return;
            }

            Debug.Log($"=== 던전 그래프 (Seed: {previewSeed}) ===");
            Debug.Log($"노드: {graph.NodeCount}개, 엣지: {graph.EdgeCount}개");
            Debug.Log("");

            // 층별 출력
            var nodesByFloor = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>();

            foreach (var node in graph.GetAllNodes())
            {
                if (!nodesByFloor.ContainsKey(node.floor))
                {
                    nodesByFloor[node.floor] = new System.Collections.Generic.List<string>();
                }
                nodesByFloor[node.floor].Add($"{node.roomType.ToString()[0]}({node.nodeId.Substring(0, 4)})");
            }

            foreach (var kvp in nodesByFloor)
            {
                Debug.Log($"층 {kvp.Key}: {string.Join(" - ", kvp.Value)}");
            }
        }
    }
}
#endif
