using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GASPT.Editor
{
    /// <summary>
    /// Bootstrap Scene 자동 생성 에디터 툴
    ///
    /// Bootstrap Scene은 게임의 진입점 (Build Index 0)
    /// - AdditiveSceneLoader 초기화
    /// - PersistentManagers Scene 로드
    /// - 기본 Content Scene (StartRoom) 로드
    /// </summary>
    public class BootstrapSceneCreator : EditorWindow
    {
        private const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";

        [MenuItem("Tools/GASPT/🚀 Create Bootstrap Scene")]
        public static void ShowWindow()
        {
            var window = GetWindow<BootstrapSceneCreator>("Bootstrap Scene Creator");
            window.minSize = new Vector2(400, 350);
        }

        private void OnGUI()
        {
            GUILayout.Label("=== Bootstrap Scene Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Bootstrap Scene은 게임의 진입점입니다.\n\n" +
                "역할:\n" +
                "✓ Build Settings Index 0 (최초 로드)\n" +
                "✓ AdditiveSceneLoader 초기화\n" +
                "✓ PersistentManagers Scene을 Additive로 로드\n" +
                "✓ 기본 Content Scene (StartRoom) 로드\n\n" +
                "생성되는 오브젝트:\n" +
                "- Bootstrapper (Bootstrapper.cs 컴포넌트)",
                MessageType.Info
            );

            GUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 Bootstrap Scene 생성", GUILayout.Height(50)))
            {
                EditorApplication.delayCall += CreateBootstrapScene;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "⚠️ 생성 후 필수 작업:\n\n" +
                "1. File > Build Settings 열기\n" +
                "2. Bootstrap을 Index 0으로 설정\n" +
                "3. PersistentManagers를 Index 1로 설정\n" +
                "4. StartRoom을 Index 2로 설정\n" +
                "5. GameplayScene을 Index 3으로 설정",
                MessageType.Warning
            );

            GUILayout.Space(10);

            if (GUILayout.Button("📂 Build Settings 열기"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
        }

        private void CreateBootstrapScene()
        {
            Debug.Log("=== Bootstrap Scene 생성 시작 ===");

            // 새 씬 생성 (빈 씬)
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Bootstrapper 오브젝트 생성
            CreateBootstrapper();

            // 폴더 확인/생성
            string scenesFolder = "Assets/_Project/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            // 씬 저장
            bool saved = EditorSceneManager.SaveScene(newScene, ScenePath);

            if (saved)
            {
                Debug.Log($"=== Bootstrap Scene 생성 완료! ===\n위치: {ScenePath}");

                EditorUtility.DisplayDialog("생성 완료",
                    "Bootstrap Scene이 생성되었습니다!\n\n" +
                    "⚠️ 반드시 Build Settings에서:\n" +
                    "1. Bootstrap → Index 0\n" +
                    "2. PersistentManagers → Index 1\n" +
                    "3. StartRoom → Index 2\n" +
                    "4. GameplayScene → Index 3\n\n" +
                    "순서로 설정하세요!",
                    "확인");

                // 씬 선택
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.LogError("[BootstrapSceneCreator] 씬 저장 실패!");
            }
        }

        private void CreateBootstrapper()
        {
            GameObject bootstrapperObj = new GameObject("Bootstrapper");

            // Bootstrapper 컴포넌트 추가
            var bootstrapper = bootstrapperObj.AddComponent<GASPT.Core.SceneManagement.Bootstrapper>();

            Debug.Log("[BootstrapSceneCreator] Bootstrapper 생성 완료");
        }
    }
}
