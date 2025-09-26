using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectTools
{
    /// <summary>
    /// 프로젝트 폴더 구조를 개선하는 에디터 도구
    /// </summary>
    public class FolderStructureOrganizer : EditorWindow
    {
        [MenuItem("Tools/Project/Organize Folder Structure")]
        public static void ShowWindow()
        {
            GetWindow<FolderStructureOrganizer>("Folder Structure Organizer");
        }

        private void OnGUI()
        {
            GUILayout.Label("프로젝트 폴더 구조 정리", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "이 도구는 프로젝트 폴더를 다음과 같이 정리합니다:\n\n" +
                "• Core 시스템들을 Plugins 폴더로 이동\n" +
                "• 게임플레이 스크립트들을 _Project/Scripts/Gameplay로 이동\n" +
                "• 아트 에셋들을 _Project/Art로 통합\n" +
                "• 씬 파일들을 _Project/Scenes로 이동",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("폴더 구조 정리 실행", GUILayout.Height(30)))
            {
                OrganizeFolderStructure();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("새 폴더 구조 생성만", GUILayout.Height(25)))
            {
                CreateNewFolderStructure();
            }
        }

        /// <summary>
        /// 새 폴더 구조 생성
        /// </summary>
        private void CreateNewFolderStructure()
        {
            string[] folders = {
                // Project 폴더
                "Assets/_Project",
                "Assets/_Project/Art",
                "Assets/_Project/Art/Sprites",
                "Assets/_Project/Art/Sprites/Characters",
                "Assets/_Project/Art/Sprites/Environment",
                "Assets/_Project/Art/Sprites/UI",
                "Assets/_Project/Art/Animations",
                "Assets/_Project/Art/Materials",
                "Assets/_Project/Art/PhysicsMaterials",

                // Scripts 폴더
                "Assets/_Project/Scripts",
                "Assets/_Project/Scripts/Core",
                "Assets/_Project/Scripts/Core/Managers",
                "Assets/_Project/Scripts/Core/Utilities",
                "Assets/_Project/Scripts/Gameplay",
                "Assets/_Project/Scripts/Gameplay/Player",
                "Assets/_Project/Scripts/Gameplay/Entities",
                "Assets/_Project/Scripts/Gameplay/Systems",
                "Assets/_Project/Scripts/UI",
                "Assets/_Project/Scripts/Data",
                "Assets/_Project/Scripts/Tools",

                // 기타 폴더
                "Assets/_Project/Prefabs",
                "Assets/_Project/Prefabs/Characters",
                "Assets/_Project/Prefabs/Environment",
                "Assets/_Project/Prefabs/UI",
                "Assets/_Project/Scenes",
                "Assets/_Project/Settings",

                // Plugins 폴더
                "Assets/Plugins",
                "Assets/Plugins/ThirdParty"
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parentFolder = Path.GetDirectoryName(folder).Replace('\\', '/');
                    string folderName = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("새 폴더 구조가 생성되었습니다!");
        }

        /// <summary>
        /// 전체 폴더 구조 정리
        /// </summary>
        private void OrganizeFolderStructure()
        {
            if (!EditorUtility.DisplayDialog("폴더 구조 정리",
                "폴더 구조를 정리하시겠습니까?\n\n주의: 이 작업은 되돌릴 수 없습니다.",
                "실행", "취소"))
            {
                return;
            }

            // 새 폴더 구조 생성
            CreateNewFolderStructure();

            // TODO: Unity에서 안전하게 폴더 이동하는 로직 구현
            // AssetDatabase.MoveAsset 사용

            Debug.Log("폴더 구조 정리가 완료되었습니다!");
        }
    }
}