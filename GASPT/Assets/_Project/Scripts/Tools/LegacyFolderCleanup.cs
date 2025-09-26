using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ProjectTools
{
    /// <summary>
    /// 기존 폴더 구조를 정리하고 새 구조로 전환하는 도구
    /// </summary>
    public class LegacyFolderCleanup : EditorWindow
    {
        private bool confirmCleanup = false;
        private Vector2 scrollPosition;

        [MenuItem("Tools/Project/Clean Legacy Folders")]
        public static void ShowWindow()
        {
            GetWindow<LegacyFolderCleanup>("Legacy Folder Cleanup");
        }

        private void OnGUI()
        {
            GUILayout.Label("기존 폴더 정리 도구", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "⚠️ 주의: 이 도구는 기존 폴더들을 정리합니다.\n\n" +
                "정리될 폴더들:\n" +
                "• Assets/Scripts/ → _Project/Scripts/로 이동 완료 후 정리\n" +
                "• Assets/Image/ → _Project/Art/Sprites/로 이동 완료 후 정리\n" +
                "• Assets/Animation/ → _Project/Art/Animations/로 이동 완료 후 정리\n" +
                "• Assets/PhysicsMaterial/ → _Project/Art/PhysicsMaterials/로 이동 완료 후 정리\n" +
                "• Assets/Scenes/ → _Project/Scenes/로 이동 완료 후 정리\n" +
                "• Assets/Settings/ → _Project/Settings/로 이동 완료 후 정리\n" +
                "• Assets/ScriptableObjects/ → _Project/Scripts/Data/로 이동 완료 후 정리\n\n" +
                "이동될 폴더들:\n" +
                "• Assets/FSM_Core/ → Assets/Plugins/FSM_Core/\n" +
                "• Assets/GAS_Core/ → Assets/Plugins/GAS_Core/",
                MessageType.Warning
            );

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 이동 상태 확인
            DrawMigrationStatus();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            confirmCleanup = EditorGUILayout.Toggle("정리 작업 확인", confirmCleanup);

            EditorGUI.BeginDisabledGroup(!confirmCleanup);

            if (GUILayout.Button("Core 시스템들을 Plugins로 이동", GUILayout.Height(30)))
            {
                MoveCoreSystemsToPlugins();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("빈 폴더 및 레거시 폴더 정리", GUILayout.Height(30)))
            {
                CleanupLegacyFolders();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("전체 정리 실행", GUILayout.Height(40)))
            {
                PerformFullCleanup();
            }

            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// 이동 상태 표시
        /// </summary>
        private void DrawMigrationStatus()
        {
            GUILayout.Label("📊 이동 상태 확인", EditorStyles.boldLabel);

            var migrations = new Dictionary<string, (string from, string to, bool completed)>
            {
                { "플레이어 스크립트", ("Assets/Scripts/Player", "Assets/_Project/Scripts/Gameplay/Player",
                    Directory.Exists("Assets/_Project/Scripts/Gameplay/Player") && Directory.GetFiles("Assets/_Project/Scripts/Gameplay/Player", "*.cs").Length > 0) },

                { "매니저 스크립트", ("Assets/Scripts/Managers", "Assets/_Project/Scripts/Core/Managers",
                    Directory.Exists("Assets/_Project/Scripts/Core/Managers") && Directory.GetFiles("Assets/_Project/Scripts/Core/Managers", "*.cs").Length > 0) },

                { "캐릭터 아트", ("Assets/Image/Character", "Assets/_Project/Art/Sprites/Characters",
                    Directory.Exists("Assets/_Project/Art/Sprites/Characters") && Directory.GetDirectories("Assets/_Project/Art/Sprites/Characters").Length > 0) },

                { "애니메이션", ("Assets/Animation", "Assets/_Project/Art/Animations",
                    Directory.Exists("Assets/_Project/Art/Animations")) },

                { "씬 파일", ("Assets/Scenes", "Assets/_Project/Scenes",
                    Directory.Exists("Assets/_Project/Scenes") && Directory.GetFiles("Assets/_Project/Scenes", "*.unity").Length > 0) },

                { "FSM Core", ("Assets/FSM_Core", "Assets/Plugins/FSM_Core",
                    Directory.Exists("Assets/Plugins/FSM_Core")) },

                { "GAS Core", ("Assets/GAS_Core", "Assets/Plugins/GAS_Core",
                    Directory.Exists("Assets/Plugins/GAS_Core")) }
            };

            foreach (var migration in migrations)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(migration.Value.completed ? "✅" : "⏳", GUILayout.Width(20));
                GUILayout.Label(migration.Key, GUILayout.Width(120));
                GUILayout.Label("→", GUILayout.Width(20));
                GUILayout.Label(migration.Value.to, EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Core 시스템들을 Plugins로 이동
        /// </summary>
        private void MoveCoreSystemsToPlugins()
        {
            if (!EditorUtility.DisplayDialog("Core 시스템 이동",
                "FSM_Core와 GAS_Core를 Plugins 폴더로 이동하시겠습니까?",
                "이동", "취소"))
            {
                return;
            }

            try
            {
                // FSM_Core 이동
                if (AssetDatabase.IsValidFolder("Assets/FSM_Core") && !AssetDatabase.IsValidFolder("Assets/Plugins/FSM_Core"))
                {
                    string result = AssetDatabase.MoveAsset("Assets/FSM_Core", "Assets/Plugins/FSM_Core");
                    if (string.IsNullOrEmpty(result))
                    {
                        Debug.Log("✅ FSM_Core를 Plugins로 이동했습니다.");
                    }
                    else
                    {
                        Debug.LogError($"❌ FSM_Core 이동 실패: {result}");
                    }
                }

                // GAS_Core 이동
                if (AssetDatabase.IsValidFolder("Assets/GAS_Core") && !AssetDatabase.IsValidFolder("Assets/Plugins/GAS_Core"))
                {
                    string result = AssetDatabase.MoveAsset("Assets/GAS_Core", "Assets/Plugins/GAS_Core");
                    if (string.IsNullOrEmpty(result))
                    {
                        Debug.Log("✅ GAS_Core를 Plugins로 이동했습니다.");
                    }
                    else
                    {
                        Debug.LogError($"❌ GAS_Core 이동 실패: {result}");
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log("🔄 Core 시스템 이동이 완료되었습니다!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Core 시스템 이동 중 오류: {e.Message}");
            }
        }

        /// <summary>
        /// 레거시 폴더들 정리
        /// </summary>
        private void CleanupLegacyFolders()
        {
            if (!EditorUtility.DisplayDialog("레거시 폴더 정리",
                "비어있거나 이동 완료된 레거시 폴더들을 정리하시겠습니까?",
                "정리", "취소"))
            {
                return;
            }

            var foldersToCheck = new List<string>
            {
                "Assets/Scripts",
                "Assets/Image",
                "Assets/Animation",
                "Assets/PhysicsMaterial",
                "Assets/ScriptableObjects"
            };

            foreach (string folder in foldersToCheck)
            {
                CleanupFolderIfEmpty(folder);
            }

            AssetDatabase.Refresh();
            Debug.Log("🧹 레거시 폴더 정리가 완료되었습니다!");
        }

        /// <summary>
        /// 폴더가 비어있으면 정리
        /// </summary>
        private void CleanupFolderIfEmpty(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                return;

            try
            {
                string[] subFolders = AssetDatabase.GetSubFolders(folderPath);
                string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".meta"))
                    .ToArray();

                if (subFolders.Length == 0 && files.Length == 0)
                {
                    AssetDatabase.DeleteAsset(folderPath);
                    Debug.Log($"🗑️ 빈 폴더 삭제: {folderPath}");
                }
                else
                {
                    Debug.Log($"⏭️ 폴더에 파일이 남아있음: {folderPath} (파일: {files.Length}, 폴더: {subFolders.Length})");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 폴더 정리 실패 {folderPath}: {e.Message}");
            }
        }

        /// <summary>
        /// 전체 정리 실행
        /// </summary>
        private void PerformFullCleanup()
        {
            if (!EditorUtility.DisplayDialog("전체 정리",
                "Core 시스템 이동과 레거시 폴더 정리를 모두 실행하시겠습니까?",
                "실행", "취소"))
            {
                return;
            }

            MoveCoreSystemsToPlugins();
            System.Threading.Thread.Sleep(1000); // 잠시 대기
            CleanupLegacyFolders();

            Debug.Log("🎉 폴더 구조 정리가 완전히 완료되었습니다!");
        }
    }
}

#if UNITY_EDITOR
using System.Linq;
#endif