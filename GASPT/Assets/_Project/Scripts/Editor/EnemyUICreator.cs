using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// Enemy UI 프리팹을 자동으로 생성하는 Editor Tool
    /// Menu: Tools > GASPT > Create Enemy UIs
    /// EnemyNameTag (World Space) + BossHealthBar (Screen Space) 두 개의 프리팹을 생성합니다.
    /// </summary>
    public static class EnemyUICreator
    {
        private const string ENEMY_NAME_TAG_PREFAB_PATH = "Assets/_Project/Prefabs/UI/EnemyNameTag.prefab";
        private const string BOSS_HEALTH_BAR_PREFAB_PATH = "Assets/_Project/Prefabs/UI/BossHealthBar.prefab";

        /// <summary>
        /// Enemy UI 프리팹 생성 (메뉴 항목)
        /// </summary>
        [MenuItem("Tools/GASPT/Create Enemy UIs")]
        public static void CreateEnemyUIs()
        {
            Debug.Log("[EnemyUICreator] Enemy UIs 생성 시작...");

            // 1. EnemyNameTag 프리팹 생성 (World Space)
            CreateEnemyNameTag();

            // 2. BossHealthBar 프리팹 생성 (Screen Space Overlay)
            CreateBossHealthBar();

            Debug.Log($"[EnemyUICreator] Enemy UIs 생성 완료!");
            Debug.Log($"[EnemyUICreator] - EnemyNameTag: {ENEMY_NAME_TAG_PREFAB_PATH}");
            Debug.Log($"[EnemyUICreator] - BossHealthBar: {BOSS_HEALTH_BAR_PREFAB_PATH}");
        }


        // ====== EnemyNameTag 생성 (World Space) ======

        /// <summary>
        /// EnemyNameTag 프리팹 생성
        /// </summary>
        private static void CreateEnemyNameTag()
        {
            Debug.Log("[EnemyUICreator] EnemyNameTag 생성 시작...");

            // 1. Canvas 생성 (World Space)
            GameObject nameTag = new GameObject("EnemyNameTag");
            Canvas canvas = nameTag.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // RectTransform 설정
            RectTransform canvasRect = nameTag.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 50);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // GraphicRaycaster 추가
            nameTag.AddComponent<GraphicRaycaster>();

            // 2. Background Image 생성
            GameObject background = new GameObject("Background");
            background.transform.SetParent(nameTag.transform, false);

            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(-10, -5);
            bgRect.offsetMax = new Vector2(10, 5);

            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.59f); // 검은색 반투명 (A:150/255)

            // 3. NameText 생성
            GameObject nameText = new GameObject("NameText");
            nameText.transform.SetParent(nameTag.transform, false);

            RectTransform textRect = nameText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = nameText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Elite Orc";
            tmp.fontSize = 24;
            tmp.color = new Color(1f, 0.78f, 0f); // Yellow
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

            // 4. EnemyNameTag 스크립트 추가 및 참조 연결
            EnemyNameTag nameTagScript = nameTag.AddComponent<EnemyNameTag>();
            AssignEnemyNameTagReferences(nameTagScript, nameText, background);

            // 5. 프리팹으로 저장
            SaveAsPrefab(nameTag, ENEMY_NAME_TAG_PREFAB_PATH);

            // 6. Scene에서 제거
            Object.DestroyImmediate(nameTag);

            Debug.Log("[EnemyUICreator] EnemyNameTag 생성 완료");
        }

        /// <summary>
        /// EnemyNameTag 컴포넌트의 참조 연결
        /// </summary>
        private static void AssignEnemyNameTagReferences(EnemyNameTag nameTag, GameObject nameText, GameObject background)
        {
            SerializedObject serializedObject = new SerializedObject(nameTag);

            SerializedProperty nameTextProp = serializedObject.FindProperty("nameText");
            SerializedProperty backgroundImageProp = serializedObject.FindProperty("backgroundImage");
            SerializedProperty faceCameraProp = serializedObject.FindProperty("faceCamera");
            SerializedProperty verticalOffsetProp = serializedObject.FindProperty("verticalOffset");

            if (nameTextProp != null && backgroundImageProp != null && faceCameraProp != null && verticalOffsetProp != null)
            {
                nameTextProp.objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
                backgroundImageProp.objectReferenceValue = background.GetComponent<Image>();
                faceCameraProp.boolValue = true;
                verticalOffsetProp.floatValue = 1.5f;

                serializedObject.ApplyModifiedProperties();

                Debug.Log("[EnemyUICreator] EnemyNameTag 참조 연결 완료");
            }
            else
            {
                Debug.LogError("[EnemyUICreator] EnemyNameTag 필드를 찾을 수 없습니다.");
            }
        }


        // ====== BossHealthBar 생성 (Screen Space Overlay) ======

        /// <summary>
        /// BossHealthBar 프리팹 생성
        /// </summary>
        private static void CreateBossHealthBar()
        {
            Debug.Log("[EnemyUICreator] BossHealthBar 생성 시작...");

            // 1. Canvas 찾기 또는 생성 (Screen Space Overlay)
            Canvas canvas = FindOrCreateOverlayCanvas();

            // 2. BossHealthBar GameObject 생성
            GameObject bossHealthBar = new GameObject("BossHealthBar");
            bossHealthBar.transform.SetParent(canvas.transform, false);

            RectTransform bossRect = bossHealthBar.AddComponent<RectTransform>();

            // Anchor: Top Center
            bossRect.anchorMin = new Vector2(0.5f, 1);
            bossRect.anchorMax = new Vector2(0.5f, 1);
            bossRect.pivot = new Vector2(0.5f, 1);

            // Position and Size
            bossRect.anchoredPosition = new Vector2(0, -50);
            bossRect.sizeDelta = new Vector2(600, 80);

            // 3. BossNameText 생성
            GameObject bossNameText = CreateBossNameText(bossHealthBar.transform);

            // 4. HealthBarBackground 생성
            GameObject healthBarBg = CreateHealthBarBackground(bossHealthBar.transform);

            // 5. HealthBarFill 생성
            GameObject healthBarFill = CreateHealthBarFill(healthBarBg.transform);

            // 6. HealthText 생성
            GameObject healthText = CreateHealthText(healthBarBg.transform);

            // 7. BossHealthBar 스크립트 추가 및 참조 연결
            BossHealthBar bossHealthBarScript = bossHealthBar.AddComponent<BossHealthBar>();
            AssignBossHealthBarReferences(bossHealthBarScript, bossNameText, healthBarFill, healthText);

            // 8. 프리팹으로 저장
            SaveAsPrefab(bossHealthBar, BOSS_HEALTH_BAR_PREFAB_PATH);

            Debug.Log("[EnemyUICreator] BossHealthBar 생성 완료");

            // Scene에서 선택
            Selection.activeGameObject = bossHealthBar;
        }

        /// <summary>
        /// Canvas 찾기 또는 생성 (Screen Space Overlay)
        /// </summary>
        private static Canvas FindOrCreateOverlayCanvas()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.Log("[EnemyUICreator] Screen Space Overlay Canvas를 찾을 수 없어 새로 생성합니다.");

                // Canvas 생성
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // CanvasScaler 추가
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // GraphicRaycaster 추가
                canvasObj.AddComponent<GraphicRaycaster>();

                Debug.Log("[EnemyUICreator] Canvas 생성 완료");
            }
            else
            {
                Debug.Log("[EnemyUICreator] 기존 Canvas를 사용합니다.");
            }

            return canvas;
        }

        /// <summary>
        /// BossNameText 생성
        /// </summary>
        private static GameObject CreateBossNameText(Transform parent)
        {
            GameObject bossNameText = new GameObject("BossNameText");
            bossNameText.transform.SetParent(parent, false);

            RectTransform textRect = bossNameText.AddComponent<RectTransform>();

            // Anchor: Top Center
            textRect.anchorMin = new Vector2(0.5f, 1);
            textRect.anchorMax = new Vector2(0.5f, 1);
            textRect.pivot = new Vector2(0.5f, 1);

            // Position and Size
            textRect.anchoredPosition = new Vector2(0, -10);
            textRect.sizeDelta = new Vector2(600, 30);

            // TextMeshProUGUI
            TextMeshProUGUI tmp = bossNameText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Fire Dragon";
            tmp.fontSize = 28;
            tmp.color = new Color(1f, 0.2f, 0.2f); // Red
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.fontStyle = FontStyles.Bold;

            Debug.Log("[EnemyUICreator] BossNameText 생성 완료");
            return bossNameText;
        }

        /// <summary>
        /// HealthBarBackground 생성
        /// </summary>
        private static GameObject CreateHealthBarBackground(Transform parent)
        {
            GameObject healthBarBg = new GameObject("HealthBarBackground");
            healthBarBg.transform.SetParent(parent, false);

            RectTransform bgRect = healthBarBg.AddComponent<RectTransform>();

            // Anchor: Bottom Center
            bgRect.anchorMin = new Vector2(0.5f, 0);
            bgRect.anchorMax = new Vector2(0.5f, 0);
            bgRect.pivot = new Vector2(0.5f, 0);

            // Position and Size
            bgRect.anchoredPosition = new Vector2(0, 15);
            bgRect.sizeDelta = new Vector2(560, 30);

            // Image
            Image bgImage = healthBarBg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f); // 검은색

            Debug.Log("[EnemyUICreator] HealthBarBackground 생성 완료");
            return healthBarBg;
        }

        /// <summary>
        /// HealthBarFill 생성
        /// </summary>
        private static GameObject CreateHealthBarFill(Transform parent)
        {
            GameObject healthBarFill = new GameObject("HealthBarFill");
            healthBarFill.transform.SetParent(parent, false);

            RectTransform fillRect = healthBarFill.AddComponent<RectTransform>();

            // Anchor: Stretch
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // Image (Filled)
            Image fillImage = healthBarFill.AddComponent<Image>();
            fillImage.color = Color.green;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;

            Debug.Log("[EnemyUICreator] HealthBarFill 생성 완료");
            return healthBarFill;
        }

        /// <summary>
        /// HealthText 생성
        /// </summary>
        private static GameObject CreateHealthText(Transform parent)
        {
            GameObject healthText = new GameObject("HealthText");
            healthText.transform.SetParent(parent, false);

            RectTransform textRect = healthText.AddComponent<RectTransform>();

            // Anchor: Stretch
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // TextMeshProUGUI
            TextMeshProUGUI tmp = healthText.AddComponent<TextMeshProUGUI>();
            tmp.text = "150 / 150";
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.fontStyle = FontStyles.Bold;

            Debug.Log("[EnemyUICreator] HealthText 생성 완료");
            return healthText;
        }

        /// <summary>
        /// BossHealthBar 컴포넌트의 참조 연결
        /// </summary>
        private static void AssignBossHealthBarReferences(BossHealthBar bossHealthBar, GameObject bossNameText, GameObject healthBarFill, GameObject healthText)
        {
            SerializedObject serializedObject = new SerializedObject(bossHealthBar);

            SerializedProperty bossNameTextProp = serializedObject.FindProperty("bossNameText");
            SerializedProperty healthBarFillProp = serializedObject.FindProperty("healthBarFill");
            SerializedProperty healthTextProp = serializedObject.FindProperty("healthText");
            SerializedProperty healthyColorProp = serializedObject.FindProperty("healthyColor");
            SerializedProperty dangerColorProp = serializedObject.FindProperty("dangerColor");
            SerializedProperty fillSpeedProp = serializedObject.FindProperty("fillSpeed");

            if (bossNameTextProp != null && healthBarFillProp != null && healthTextProp != null)
            {
                bossNameTextProp.objectReferenceValue = bossNameText.GetComponent<TextMeshProUGUI>();
                healthBarFillProp.objectReferenceValue = healthBarFill.GetComponent<Image>();
                healthTextProp.objectReferenceValue = healthText.GetComponent<TextMeshProUGUI>();
                healthyColorProp.colorValue = Color.green;
                dangerColorProp.colorValue = Color.red;
                fillSpeedProp.floatValue = 5f;

                serializedObject.ApplyModifiedProperties();

                Debug.Log("[EnemyUICreator] BossHealthBar 참조 연결 완료");
            }
            else
            {
                Debug.LogError("[EnemyUICreator] BossHealthBar 필드를 찾을 수 없습니다.");
            }
        }


        // ====== 프리팹으로 저장 ======

        /// <summary>
        /// GameObject를 프리팹으로 저장
        /// </summary>
        private static void SaveAsPrefab(GameObject gameObject, string prefabPath)
        {
            // 프리팹 디렉토리 생성 (없으면)
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                Debug.Log($"[EnemyUICreator] 디렉토리 생성: {directory}");
            }

            // 기존 프리팹이 있으면 경고
            if (System.IO.File.Exists(prefabPath))
            {
                Debug.LogWarning($"[EnemyUICreator] 기존 프리팹이 있습니다. 덮어씁니다: {prefabPath}");
            }

            // 프리팹으로 저장
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);

            // AssetDatabase 새로고침
            AssetDatabase.Refresh();

            Debug.Log($"[EnemyUICreator] 프리팹 저장 완료: {prefabPath}");
        }
    }
}
