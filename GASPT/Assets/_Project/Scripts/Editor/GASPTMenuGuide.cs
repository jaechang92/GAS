#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Editor
{
    /// <summary>
    /// GASPT 에디터 메뉴 가이드 윈도우
    /// 모든 GASPT 도구를 한눈에 볼 수 있는 도움말
    /// </summary>
    public class GASPTMenuGuide : EditorWindow
    {
        private Vector2 scrollPosition;
        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle buttonStyle;

        [MenuItem("GASPT/Help/Open Menu Guide", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<GASPTMenuGuide>("GASPT Menu Guide");
            window.minSize = new Vector2(600, 700);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeStyles();
        }

        private void OnGUI()
        {
            InitializeStyles();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawMenuStructure();
            DrawQuickActions();

            EditorGUILayout.EndScrollView();
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.3f, 0.7f, 1f) }
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
                };
            }

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 5, 5)
                };
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("GASPT 에디터 도구 가이드", headerStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "GASPT 프로젝트의 모든 에디터 도구를 한눈에 볼 수 있습니다.\n" +
                "버튼을 클릭하면 해당 기능이 실행됩니다.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);
        }

        private void DrawMenuStructure()
        {
            // Prefabs 섹션
            DrawSection("Prefabs - UI Panels", new[]
            {
                ("Create All Panels", "모든 UI Panel Prefab 생성 (MainMenu, Loading, GameplayHUD, Pause)"),
                ("Create MainMenu Panel", "메인 메뉴 Panel Prefab 생성"),
                ("Create Loading Panel", "로딩 화면 Panel Prefab 생성"),
                ("Create GameplayHUD Panel", "게임플레이 HUD Panel Prefab 생성"),
                ("Create Pause Panel", "일시정지 Panel Prefab 생성"),
                ("Open Prefabs Folder", "Prefab 저장 폴더 열기")
            }, "GASPT/Prefabs/UI Panels/");

            EditorGUILayout.Space(5);

            // NPC 섹션
            DrawSection("Prefabs - NPC", new[]
            {
                ("Open Creator Window", "NPC 생성 윈도우 열기 (커스텀 NPC 생성)"),
                ("Create StoryNPC", "스토리 NPC 빠른 생성 (마을사람)"),
                ("Create ShopNPC", "상점 NPC 빠른 생성 (상인)"),
                ("Create All NPCs", "모든 기본 NPC 생성"),
                ("Open NPC Folder", "NPC Prefab 폴더 열기")
            }, "GASPT/NPC Creator/");

            EditorGUILayout.Space(5);

            // Scene Setup 섹션
            DrawSection("Scene Setup", new[]
            {
                ("Open Scene Setup Tool", "씬 설정 도구 윈도우 열기"),
                ("Create All Scenes", "모든 기본 씬 생성 (Bootstrap, Preload, Main, Gameplay, Lobby)"),
                ("Update Build Settings", "Build Settings에 씬 추가"),
                ("Open Scene Folder", "씬 폴더 열기")
            }, "GASPT/Scene Setup/");

            EditorGUILayout.Space(5);

            // Resources 섹션
            DrawSection("Resources", new[]
            {
                ("Create All Manifests", "모든 Resource Manifest 생성"),
                ("Create Essential Manifest", "필수 리소스 Manifest 생성"),
                ("Create MainMenu Manifest", "메인 메뉴 리소스 Manifest 생성"),
                ("Create Gameplay Manifest", "게임플레이 리소스 Manifest 생성"),
                ("Create Common Manifest", "공통 리소스 Manifest 생성"),
                ("Delete All Manifests", "모든 Manifest 삭제")
            }, "GASPT/Resources/");

            EditorGUILayout.Space(5);

            // Dialogue 섹션
            DrawSection("Prefabs - Dialogue", new[]
            {
                ("Create DialoguePanel", "대화 패널 Prefab 생성 (NPC 대화 시스템용)"),
                ("Create ChoiceButton", "선택지 버튼 Prefab 생성 (DialoguePanel용)")
            }, "GASPT/Prefabs/Dialogue/");

            EditorGUILayout.Space(5);

            // Character 섹션
            DrawSection("Character", new[]
            {
                ("Create Player (Skul Physics)", "Skul 스타일 물리 시스템 플레이어 캐릭터 생성 도구")
            }, "GASPT/Character/");
        }

        private void DrawSection(string title, (string name, string description)[] items, string menuPath)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"📁 {title}", sectionStyle);
            EditorGUILayout.Space(5);

            foreach (var item in items)
            {
                EditorGUILayout.BeginHorizontal();

                // 메뉴 실행 버튼
                if (GUILayout.Button($"▶ {item.name}", buttonStyle, GUILayout.Height(25)))
                {
                    EditorApplication.ExecuteMenuItem(menuPath + item.name);
                }

                EditorGUILayout.EndHorizontal();

                // 설명
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField(item.description, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("⚡ 빠른 작업", sectionStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("🎨 모든 UI Panel Prefab 생성", GUILayout.Height(35)))
            {
                EditorApplication.ExecuteMenuItem("GASPT/Prefabs/UI Panels/Create All Panels");
            }

            if (GUILayout.Button("🎮 모든 기본 씬 생성", GUILayout.Height(35)))
            {
                EditorApplication.ExecuteMenuItem("GASPT/Scene Setup/Create All Scenes");
            }

            if (GUILayout.Button("📦 모든 Resource Manifest 생성", GUILayout.Height(35)))
            {
                EditorApplication.ExecuteMenuItem("GASPT/Resources/Create All Manifests");
            }

            if (GUILayout.Button("👤 모든 기본 NPC 생성", GUILayout.Height(35)))
            {
                EditorApplication.ExecuteMenuItem("GASPT/NPC Creator/Create All NPCs");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "💡 팁:\n" +
                "• Unity 메뉴바에서 GASPT 메뉴를 찾아 직접 실행할 수도 있습니다.\n" +
                "• 각 도구는 독립적으로 실행 가능하며, 필요한 폴더는 자동으로 생성됩니다.\n" +
                "• 기존 파일이 있으면 덮어쓰기 여부를 물어봅니다.",
                MessageType.Info
            );
        }
    }
}
#endif
