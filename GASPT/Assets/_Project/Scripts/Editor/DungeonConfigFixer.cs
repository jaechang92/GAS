using UnityEngine;
using UnityEditor;
using GASPT.Gameplay.Level;

namespace GASPT.Editor
{
    /// <summary>
    /// DungeonConfig 에셋의 누락된 참조를 자동으로 설정하는 도구
    /// </summary>
    public class DungeonConfigFixer : EditorWindow
    {
        [MenuItem("GASPT/Tools/Fix DungeonConfig References")]
        public static void ShowWindow()
        {
            GetWindow<DungeonConfigFixer>("DungeonConfig Fixer");
        }

        [MenuItem("GASPT/Tools/Auto-Fix All DungeonConfigs")]
        public static void AutoFixAll()
        {
            FixAllDungeonConfigs();
        }

        private void OnGUI()
        {
            GUILayout.Label("DungeonConfig 참조 수정 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "이 도구는 모든 DungeonConfig 에셋의 누락된 참조를 자동으로 설정합니다.\n\n" +
                "수정 항목:\n" +
                "- roomTemplatePrefab: Room_1 프리팹 자동 할당\n" +
                "- roomDataPool: 기본 RoomData 할당 (없을 경우)",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("모든 DungeonConfig 수정", GUILayout.Height(40)))
            {
                FixAllDungeonConfigs();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("TestDungeon만 수정", GUILayout.Height(30)))
            {
                FixTestDungeon();
            }
        }

        /// <summary>
        /// 모든 DungeonConfig 에셋 수정
        /// </summary>
        private static void FixAllDungeonConfigs()
        {
            // Room 템플릿 프리팹 찾기
            Room roomTemplate = FindRoomTemplatePrefab();
            if (roomTemplate == null)
            {
                Debug.LogError("[DungeonConfigFixer] Room 템플릿 프리팹을 찾을 수 없습니다!");
                EditorUtility.DisplayDialog("오류", "Room 템플릿 프리팹(Room_1.prefab)을 찾을 수 없습니다.", "확인");
                return;
            }

            // 모든 DungeonConfig 에셋 찾기
            string[] guids = AssetDatabase.FindAssets("t:DungeonConfig");
            int fixedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DungeonConfig config = AssetDatabase.LoadAssetAtPath<DungeonConfig>(path);

                if (config != null)
                {
                    bool modified = false;

                    // roomTemplatePrefab 설정
                    if (config.roomTemplatePrefab == null)
                    {
                        config.roomTemplatePrefab = roomTemplate;
                        modified = true;
                        Debug.Log($"[DungeonConfigFixer] {config.dungeonName}: roomTemplatePrefab 설정 완료");
                    }

                    // roomDataPool 확인 및 기본값 설정
                    if (config.roomDataPool == null || config.roomDataPool.Length == 0)
                    {
                        RoomData[] defaultPool = FindDefaultRoomDataPool();
                        if (defaultPool != null && defaultPool.Length > 0)
                        {
                            config.roomDataPool = defaultPool;
                            modified = true;
                            Debug.Log($"[DungeonConfigFixer] {config.dungeonName}: roomDataPool 설정 완료 ({defaultPool.Length}개)");
                        }
                    }

                    // generationRules 확인 및 기본값 설정
                    if (config.generationRules == null)
                    {
                        RoomGenerationRules defaultRules = FindDefaultGenerationRules();
                        if (defaultRules != null)
                        {
                            config.generationRules = defaultRules;
                            modified = true;
                            Debug.Log($"[DungeonConfigFixer] {config.dungeonName}: generationRules 설정 완료");
                        }
                    }

                    if (modified)
                    {
                        EditorUtility.SetDirty(config);
                        fixedCount++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DungeonConfigFixer] 완료! {fixedCount}개의 DungeonConfig 수정됨");
            EditorUtility.DisplayDialog("완료", $"{fixedCount}개의 DungeonConfig가 수정되었습니다.", "확인");
        }

        /// <summary>
        /// TestDungeon만 수정
        /// </summary>
        private static void FixTestDungeon()
        {
            // Room 템플릿 프리팹 찾기
            Room roomTemplate = FindRoomTemplatePrefab();
            if (roomTemplate == null)
            {
                Debug.LogError("[DungeonConfigFixer] Room 템플릿 프리팹을 찾을 수 없습니다!");
                return;
            }

            // TestDungeon 에셋 찾기
            string[] paths = new string[]
            {
                "Assets/Resources/Data/Dungeons/TestDungeon.asset",
                "Assets/Resources/Data/Dungeons/TestDungeonConfig.asset"
            };

            foreach (string path in paths)
            {
                DungeonConfig config = AssetDatabase.LoadAssetAtPath<DungeonConfig>(path);
                if (config != null)
                {
                    FixSingleConfig(config, roomTemplate);
                }
            }
        }

        /// <summary>
        /// 단일 DungeonConfig 수정
        /// </summary>
        private static void FixSingleConfig(DungeonConfig config, Room roomTemplate)
        {
            bool modified = false;

            if (config.roomTemplatePrefab == null)
            {
                config.roomTemplatePrefab = roomTemplate;
                modified = true;
            }

            if (config.roomDataPool == null || config.roomDataPool.Length == 0)
            {
                RoomData[] defaultPool = FindDefaultRoomDataPool();
                if (defaultPool != null && defaultPool.Length > 0)
                {
                    config.roomDataPool = defaultPool;
                    modified = true;
                }
            }

            if (config.generationRules == null)
            {
                RoomGenerationRules defaultRules = FindDefaultGenerationRules();
                if (defaultRules != null)
                {
                    config.generationRules = defaultRules;
                    modified = true;
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                Debug.Log($"[DungeonConfigFixer] {config.dungeonName} 수정 완료!");
            }
            else
            {
                Debug.Log($"[DungeonConfigFixer] {config.dungeonName}: 수정 필요 없음");
            }
        }

        /// <summary>
        /// Room 템플릿 프리팹 찾기
        /// </summary>
        private static Room FindRoomTemplatePrefab()
        {
            // 우선순위: Resources/Rooms/Room_1.prefab
            string[] searchPaths = new string[]
            {
                "Assets/Resources/Rooms/Room_1.prefab",
                "Assets/Resources/Prefabs/Level/Room.prefab",
                "Assets/_Project/Prefabs/Level/Room.prefab"
            };

            foreach (string path in searchPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Room room = prefab.GetComponent<Room>();
                    if (room != null)
                    {
                        Debug.Log($"[DungeonConfigFixer] Room 템플릿 발견: {path}");
                        return room;
                    }
                }
            }

            // 전체 검색
            string[] guids = AssetDatabase.FindAssets("t:Prefab Room");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Room room = prefab.GetComponent<Room>();
                    if (room != null)
                    {
                        Debug.Log($"[DungeonConfigFixer] Room 템플릿 발견 (검색): {path}");
                        return room;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 기본 RoomData 풀 찾기
        /// </summary>
        private static RoomData[] FindDefaultRoomDataPool()
        {
            string[] guids = AssetDatabase.FindAssets("t:RoomData");
            if (guids.Length == 0) return null;

            RoomData[] pool = new RoomData[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                pool[i] = AssetDatabase.LoadAssetAtPath<RoomData>(path);
            }

            return pool;
        }

        /// <summary>
        /// 기본 RoomGenerationRules 찾기
        /// </summary>
        private static RoomGenerationRules FindDefaultGenerationRules()
        {
            string[] guids = AssetDatabase.FindAssets("t:RoomGenerationRules");
            if (guids.Length == 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<RoomGenerationRules>(path);
        }
    }
}
