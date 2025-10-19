#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Gameplay.Systems;
using Core.Enums;

namespace Editor.Gameplay
{
    /// <summary>
    /// Portal Prefab을 자동으로 생성하는 Editor 도구
    /// Menu: GASPT/Prefabs/Gameplay/Create Portal
    /// </summary>
    public static class PortalPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Gameplay/Portal.prefab";
        private const string FOLDER_PATH = "Assets/_Project/Prefabs/Gameplay";

        [MenuItem("GASPT/Prefabs/Gameplay/Create Portal", priority = 31)]
        public static void CreatePortalPrefab()
        {
            // 폴더 생성
            CreateFoldersIfNeeded();

            // GameObject 생성
            GameObject portalObj = CreatePortalHierarchy();

            // Prefab으로 저장
            SaveAsPrefab(portalObj);

            Debug.Log($"[PortalPrefabGenerator] Portal Prefab 생성 완료: {PREFAB_PATH}");
            EditorUtility.DisplayDialog("생성 완료", "Portal Prefab이 생성되었습니다!", "확인");
        }

        [MenuItem("GASPT/Prefabs/Gameplay/Open Gameplay Prefabs Folder", priority = 41)]
        private static void OpenGameplayPrefabsFolder()
        {
            CreateFoldersIfNeeded();
            EditorUtility.RevealInFinder(FOLDER_PATH);
        }

        private static void CreateFoldersIfNeeded()
        {
            string[] folders = FOLDER_PATH.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                    Debug.Log($"[PortalPrefabGenerator] 폴더 생성: {nextPath}");
                }
                currentPath = nextPath;
            }
        }

        private static GameObject CreatePortalHierarchy()
        {
            // Root: Portal
            GameObject root = new GameObject("Portal");

            // Transform 설정
            root.transform.position = Vector3.zero;
            root.transform.localScale = new Vector3(2f, 2f, 1f); // 크기 2배

            // Tag 설정
            if (TagExists("Portal"))
                root.tag = "Portal";

            // Layer 설정 (있다면)
            // root.layer = LayerMask.NameToLayer("Portal");

            // SpriteRenderer 추가
            SpriteRenderer spriteRenderer = root.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateCircleSprite();
            spriteRenderer.color = new Color(0.3f, 0.7f, 1f, 0.7f); // 반투명 파란색
            spriteRenderer.sortingOrder = 5;

            // CircleCollider2D 추가 (Trigger)
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.6f;
            collider.isTrigger = true;

            // Portal 스크립트 추가 및 설정
            Portal portal = root.AddComponent<Portal>();
            SetupPortalComponent(portal);

            // 프롬프트 텍스트 (자식 오브젝트)
            CreatePromptText(root.transform);

            return root;
        }

        private static void SetupPortalComponent(Portal portal)
        {
            SerializedObject so = new SerializedObject(portal);

            // 씬 설정 (Lobby → Gameplay)
            so.FindProperty("targetScene").enumValueIndex = System.Array.IndexOf(
                System.Enum.GetValues(typeof(SceneType)),
                SceneType.Game
            );
            so.FindProperty("useSceneIndex").boolValue = false;
            so.FindProperty("targetSceneIndex").intValue = 3;

            // 상호작용 설정
            so.FindProperty("interactionRange").floatValue = 2f;
            so.FindProperty("interactionKey").enumValueIndex = System.Array.IndexOf(
                System.Enum.GetValues(typeof(KeyCode)),
                KeyCode.E
            );
            so.FindProperty("showPrompt").boolValue = true;
            so.FindProperty("promptText").stringValue = "E를 눌러 게임 시작";

            // 비주얼 설정
            so.FindProperty("portalColor").colorValue = new Color(0.3f, 0.7f, 1f, 1f);
            so.FindProperty("pulseSpeed").floatValue = 2f;
            so.FindProperty("pulseIntensity").floatValue = 0.3f;

            // 디버그
            so.FindProperty("showDebugLog").boolValue = true;

            so.ApplyModifiedProperties();
        }

        private static GameObject CreatePromptText(Transform parent)
        {
            GameObject promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(parent, false);
            promptObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            promptObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); // Parent의 Scale에 영향받지 않도록

            // TextMesh 컴포넌트 추가
            TextMesh textMesh = promptObj.AddComponent<TextMesh>();
            textMesh.text = "E를 눌러 게임 시작";
            textMesh.fontSize = 40;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            // MeshRenderer 설정
            MeshRenderer meshRenderer = promptObj.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 10;

            // 기본적으로 비활성화 (Portal 스크립트에서 제어)
            promptObj.SetActive(false);

            return promptObj;
        }

        private static Sprite CreateCircleSprite()
        {
            // 원형 텍스처 생성
            int size = 128;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);

                    if (distance <= radius)
                    {
                        // 원 내부
                        float alpha = 1f - (distance / radius) * 0.5f; // 중심으로 갈수록 진함
                        pixels[index] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        // 원 외부 (투명)
                        pixels[index] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            // Sprite 생성
            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        private static bool TagExists(string tag)
        {
            try
            {
                GameObject.FindGameObjectWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SaveAsPrefab(GameObject obj)
        {
            // 기존 Prefab이 있으면 삭제
            if (File.Exists(PREFAB_PATH))
            {
                AssetDatabase.DeleteAsset(PREFAB_PATH);
            }

            // Prefab으로 저장
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_PATH);

            // Scene에서 제거
            Object.DestroyImmediate(obj);

            // Prefab 선택
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PortalPrefabGenerator] Prefab 저장 완료: {PREFAB_PATH}");
        }
    }
}
#endif
