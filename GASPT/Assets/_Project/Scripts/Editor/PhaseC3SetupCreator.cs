using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using GASPT.UI;
using GASPT.Gameplay.Level;

namespace GASPT.Editor
{
    /// <summary>
    /// Phase C-3 던전 진행 UI 자동 생성 도구
    /// Portal, PortalUI 프리팹 및 연결 자동화
    /// </summary>
    public class PhaseC3SetupCreator : EditorWindow
    {
        // ====== 경로 상수 ======

        private const string UIPrefabsPath = "Assets/Resources/Prefabs/UI";
        private const string LevelPrefabsPath = "Assets/Resources/Prefabs/Level";


        // ====== 생성 옵션 ======

        private bool createPortalPrefab = true;
        private bool createPortalUI = true;
        private bool setupSceneConnections = false;

        private Vector2 scrollPosition;


        // ====== 메뉴 ======

        [MenuItem("Tools/GASPT/Phase C-3 Setup Creator")]
        public static void ShowWindow()
        {
            PhaseC3SetupCreator window = GetWindow<PhaseC3SetupCreator>("Phase C-3 Setup");
            window.minSize = new Vector2(500, 700);
            window.Show();
        }


        // ====== GUI ======

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== Phase C-3 UI Setup Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Phase C-3 던전 진행 시스템을 자동으로 생성합니다.\n\n" +
                "생성될 프리팹:\n" +
                "1. Portal.prefab - 포탈 오브젝트\n" +
                "2. PortalUI.prefab - E키 안내 UI\n" +
                "3. Scene 연결 및 Portal 자동 배치\n\n" +
                "⚠️ 기존 프리팹이 있으면 덮어씁니다!",
                MessageType.Info
            );

            GUILayout.Space(20);

            // 생성 옵션
            EditorGUILayout.LabelField("생성할 프리팹 선택:", EditorStyles.boldLabel);
            createPortalPrefab = EditorGUILayout.Toggle("Portal.prefab", createPortalPrefab);
            createPortalUI = EditorGUILayout.Toggle("PortalUI.prefab", createPortalUI);
            setupSceneConnections = EditorGUILayout.Toggle("Scene 연결 + Portal 배치 (씬 필요)", setupSceneConnections);

            GUILayout.Space(20);

            // 전체 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 모든 UI 자동 생성", GUILayout.Height(50)))
            {
                EditorApplication.delayCall += CreateAllUI;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼
            EditorGUILayout.LabelField("개별 생성:", EditorStyles.boldLabel);

            if (GUILayout.Button("1. Portal.prefab 생성"))
            {
                CreatePortalPrefab();
            }

            if (GUILayout.Button("2. PortalUI.prefab 생성"))
            {
                CreatePortalUIPrefab();
            }

            if (GUILayout.Button("3. Scene 연결 + Portal 배치 (씬 필요)"))
            {
                SetupSceneConnections();
            }

            GUILayout.Space(20);

            // 도움말
            EditorGUILayout.HelpBox(
                "생성 순서:\n" +
                "1. Portal.prefab (포탈 오브젝트)\n" +
                "2. PortalUI.prefab (E키 안내)\n" +
                "3. Scene 연결 (Portal 연결)\n\n" +
                "⚠️ Scene 연결은 GameplayScene을 열어야 합니다!",
                MessageType.None
            );

            GUILayout.Space(10);

            // 폴더 열기
            if (GUILayout.Button("📁 생성된 UI 폴더 열기"))
            {
                string fullPath = Path.Combine(Application.dataPath, "Resources/Prefabs/UI");
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                EditorUtility.RevealInFinder(fullPath);
            }

            EditorGUILayout.EndScrollView();
        }


        // ====== 전체 생성 ======

        private void CreateAllUI()
        {
            Debug.Log("[PhaseC3SetupCreator] Phase C-3 시스템 자동 생성 시작...");

            EnsureDirectoriesExist();

            int createdCount = 0;

            // 1. Portal 프리팹 생성
            if (createPortalPrefab)
            {
                if (CreatePortalPrefab())
                {
                    createdCount++;
                }
            }

            // 2. PortalUI 생성
            if (createPortalUI)
            {
                if (CreatePortalUIPrefab())
                {
                    createdCount++;
                }
            }

            // 3. Scene 연결
            if (setupSceneConnections)
            {
                if (SetupSceneConnections())
                {
                    createdCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PhaseC3SetupCreator] ✅ 완료! 총 {createdCount}개 생성/설정");

            EditorUtility.DisplayDialog(
                "Phase C-3 Setup 완료!",
                $"생성 완료:\n- {createdCount}개 프리팹/설정\n\nResources/Prefabs 폴더를 확인하세요.",
                "확인"
            );
        }


        // ====== 디렉토리 생성 ======

        private void EnsureDirectoriesExist()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(UIPrefabsPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Prefabs", "UI");
            }

            if (!AssetDatabase.IsValidFolder(LevelPrefabsPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Prefabs", "Level");
            }

            Debug.Log("[PhaseC3SetupCreator] 폴더 구조 확인 완료");
        }


        // ====== Portal 프리팹 생성 ======

        private bool CreatePortalPrefab()
        {
            Debug.Log("[PhaseC3SetupCreator] Portal 프리팹 생성 중...");

            // Portal GameObject 생성
            GameObject portalObj = new GameObject("Portal");

            // Sprite Renderer 추가 (시각적 표현)
            SpriteRenderer sr = portalObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 1f, 1f, 0.7f); // 시안색 반투명

            // 기본 스프라이트 생성 (원형)
            Sprite portalSprite = CreateCircleSprite(64);
            sr.sprite = portalSprite;
            sr.sortingOrder = 10;

            // CircleCollider2D 추가 (Trigger)
            CircleCollider2D collider = portalObj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 1.5f;

            // ParticleSystem 추가 (옵션)
            ParticleSystem ps = portalObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0f, 1f, 1f, 0.8f);
            main.startSize = 0.3f;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.rateOverTime = 20;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;

            // Portal 컴포넌트 추가
            Portal portal = portalObj.AddComponent<Portal>();

            // SerializedObject로 private 필드 설정
            SerializedObject so = new SerializedObject(portal);
            so.FindProperty("portalType").enumValueIndex = 0; // NextRoom
            so.FindProperty("autoActivateOnRoomClear").boolValue = true;
            so.FindProperty("startActive").boolValue = false;
            so.FindProperty("portalSprite").objectReferenceValue = sr;
            so.FindProperty("portalEffect").objectReferenceValue = ps;
            so.FindProperty("inactiveColor").colorValue = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            so.FindProperty("activeColor").colorValue = new Color(0f, 1f, 1f, 1f);
            so.ApplyModifiedProperties();

            // 프리팹 저장
            string prefabPath = Path.Combine(LevelPrefabsPath, "Portal.prefab");
            PrefabUtility.SaveAsPrefabAsset(portalObj, prefabPath);

            // 씬에서 제거
            DestroyImmediate(portalObj);

            Debug.Log($"[PhaseC3SetupCreator] ✅ Portal.prefab 생성 완료: {prefabPath}");
            return true;
        }

        /// <summary>
        /// 원형 스프라이트 생성 (기본 비주얼)
        /// </summary>
        private Sprite CreateCircleSprite(int resolution)
        {
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];

            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution / 2f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);

                    if (distance < radius)
                    {
                        // 중심에서 멀어질수록 투명해지는 그라데이션
                        float alpha = 1f - (distance / radius);
                        pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        pixels[y * resolution + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f),
                resolution / 2f
            );
        }


        // ====== PortalUI 생성 ======

        private bool CreatePortalUIPrefab()
        {
            Debug.Log("[PhaseC3SetupCreator] PortalUI 생성 중 (Scene에 직접 생성)...");

            // UI CANVAS 찾기
            GameObject uiCanvas = GameObject.Find("=== UI CANVAS ===");
            if (uiCanvas == null)
            {
                Debug.LogError("[PhaseC3SetupCreator] '=== UI CANVAS ===' 오브젝트를 찾을 수 없습니다!");
                EditorUtility.DisplayDialog("오류", "Scene에 '=== UI CANVAS ===' 오브젝트가 없습니다!", "확인");
                return false;
            }

            // PortalUI 부모 생성 (항상 활성화, PortalUI 컴포넌트 포함)
            GameObject portalUIObj = new GameObject("PortalUI");
            portalUIObj.transform.SetParent(uiCanvas.transform, false);

            RectTransform portalUIRect = portalUIObj.AddComponent<RectTransform>();
            portalUIRect.anchorMin = Vector2.zero;
            portalUIRect.anchorMax = Vector2.one;
            portalUIRect.sizeDelta = Vector2.zero;
            portalUIRect.anchoredPosition = Vector2.zero;

            // Panel 자식 생성 (실제 UI, SetActive로 제어됨)
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(portalUIObj.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0, 100);
            panelRect.sizeDelta = new Vector2(400, 80);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            // Text 생성
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(panelObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            Text text = textObj.AddComponent<Text>();
            text.text = "E 키를 눌러 다음 방으로 이동";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            // PortalUI 컴포넌트 추가 (부모 오브젝트에 추가)
            PortalUI portalUI = portalUIObj.AddComponent<PortalUI>();

            // SerializedObject를 사용하여 private 필드 설정
            SerializedObject so = new SerializedObject(portalUI);
            so.FindProperty("panel").objectReferenceValue = panelObj;
            so.FindProperty("messageText").objectReferenceValue = text;
            so.FindProperty("defaultMessage").stringValue = "E 키를 눌러 다음 방으로 이동";
            so.ApplyModifiedProperties();

            Debug.Log("[PhaseC3SetupCreator] ✅ PortalUI 생성 완료 (구조: PortalUI > Panel)");

            EditorGUIUtility.PingObject(portalUIObj);
            Selection.activeGameObject = portalUIObj;

            return true;
        }


        // ====== Button 생성 헬퍼 ======

        private GameObject CreateButton(string name, string text, Transform parent)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200, 50);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            // Button Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 20;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            return buttonObj;
        }


        // ====== Scene 연결 ======

        private bool SetupSceneConnections()
        {
            Debug.Log("[PhaseC3SetupCreator] Scene 연결 및 Portal 배치 시작...");

            // Portal 프리팹 로드
            GameObject portalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                Path.Combine(LevelPrefabsPath, "Portal.prefab")
            );

            if (portalPrefab == null)
            {
                Debug.LogError("[PhaseC3SetupCreator] Portal.prefab을 찾을 수 없습니다. 먼저 Portal 프리팹을 생성하세요!");
                return false;
            }

            // Scene에서 PortalUI 찾기 (이미 생성되어 있어야 함)
            PortalUI portalUI = FindAnyObjectByType<PortalUI>();

            if (portalUI == null)
            {
                Debug.LogWarning("[PhaseC3SetupCreator] PortalUI를 찾을 수 없습니다. 먼저 PortalUI를 생성하세요.");
            }

            // 모든 Room 찾기
            Room[] rooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int portalCreatedCount = 0;

            foreach (Room room in rooms)
            {
                // 이미 Portal이 있는지 확인
                Portal existingPortal = room.GetComponentInChildren<Portal>();

                if (existingPortal == null)
                {
                    // Portal 프리팹 인스턴스화
                    GameObject portalInstance = PrefabUtility.InstantiatePrefab(portalPrefab) as GameObject;
                    portalInstance.transform.SetParent(room.transform);

                    // Portal 위치 설정 (Room 중앙에서 오른쪽으로 5유닛)
                    portalInstance.transform.localPosition = new Vector3(5f, 0f, 0f);

                    portalCreatedCount++;
                    Debug.Log($"[PhaseC3SetupCreator] ✅ {room.name}에 Portal 생성");
                }
                else
                {
                    Debug.Log($"[PhaseC3SetupCreator] {room.name}에 이미 Portal이 있음 (스킵)");
                }
            }

            Debug.Log($"[PhaseC3SetupCreator] ✅ {portalCreatedCount}개 Portal 생성 완료");

            // 모든 Portal 찾기 및 PortalUI 연결
            Portal[] portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
            int portalConnectedCount = 0;

            if (portalUI != null)
            {
                foreach (Portal portal in portals)
                {
                    SerializedObject portalSo = new SerializedObject(portal);
                    portalSo.FindProperty("portalUI").objectReferenceValue = portalUI;
                    portalSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(portal);
                    portalConnectedCount++;
                }

                Debug.Log($"[PhaseC3SetupCreator] ✅ {portalConnectedCount}개 Portal에 PortalUI 연결 완료");
            }
            else
            {
                Debug.LogWarning("[PhaseC3SetupCreator] PortalUI가 없어 Portal 연결을 건너뜁니다.");
            }

            Debug.Log("[PhaseC3SetupCreator] ✅ Scene 연결 및 Portal 배치 완료!");

            return true;
        }
    }
}
