using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GASPT.Forms;
using GASPT.UI.Forms;

namespace GASPT.Editor
{
    /// <summary>
    /// 폼 시스템 테스트 씬 자동 구성 도구
    /// Play 모드 테스트를 위한 환경을 자동으로 설정
    /// </summary>
    public static class FormTestSceneSetup
    {
        private const string TestScenePath = "Assets/_Project/Scenes/FormSystemTestScene.unity";
        private const string FormsDataPath = "Assets/Resources/Data/Forms";

        [MenuItem("Tools/GASPT/Forms/Setup Test Scene (Current Scene)", false, 100)]
        public static void SetupCurrentScene()
        {
            if (!EditorUtility.DisplayDialog(
                "테스트 씬 설정",
                "현재 씬에 폼 시스템 테스트 환경을 구성합니다.\n\n" +
                "생성되는 오브젝트:\n" +
                "• FormTestPlayer (FormManager, FormSwapSystem)\n" +
                "• FormTestUI (FormUIManager, 슬롯/쿨다운 UI)\n" +
                "• TestFormPickups (테스트용 픽업 아이템)\n" +
                "• Ground (테스트용 바닥)\n\n" +
                "계속하시겠습니까?",
                "설정", "취소"))
            {
                return;
            }

            // 폼 에셋 확인 및 생성
            EnsureFormAssetsExist();

            // 테스트 환경 구성
            CreateTestPlayer();
            CreateTestUI();
            CreateTestPickups();
            CreateTestGround();

            // 씬 저장 확인
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[FormTestSceneSetup] 테스트 씬 설정 완료!");
            EditorUtility.DisplayDialog("완료", "테스트 씬 설정이 완료되었습니다.\n\nPlay 버튼을 눌러 테스트를 시작하세요.", "확인");
        }

        [MenuItem("Tools/GASPT/Forms/Create New Test Scene", false, 101)]
        public static void CreateNewTestScene()
        {
            // 새 씬 생성
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 폼 에셋 확인 및 생성
            EnsureFormAssetsExist();

            // 테스트 환경 구성
            CreateTestPlayer();
            CreateTestUI();
            CreateTestPickups();
            CreateTestGround();
            CreateTestCamera();

            // 씬 저장
            EnsureDirectoryExists("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(newScene, TestScenePath);

            Debug.Log($"[FormTestSceneSetup] 새 테스트 씬 생성: {TestScenePath}");
            EditorUtility.DisplayDialog("완료", $"테스트 씬이 생성되었습니다.\n\n경로: {TestScenePath}", "확인");
        }

        [MenuItem("Tools/GASPT/Forms/Add Test Player Only", false, 110)]
        public static void AddTestPlayerOnly()
        {
            EnsureFormAssetsExist();
            CreateTestPlayer();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[FormTestSceneSetup] 테스트 플레이어 추가 완료");
        }

        [MenuItem("Tools/GASPT/Forms/Add Test UI Only", false, 111)]
        public static void AddTestUIOnly()
        {
            CreateTestUI();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[FormTestSceneSetup] 테스트 UI 추가 완료");
        }

        [MenuItem("Tools/GASPT/Forms/Add Test Pickups Only", false, 112)]
        public static void AddTestPickupsOnly()
        {
            EnsureFormAssetsExist();
            CreateTestPickups();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[FormTestSceneSetup] 테스트 픽업 추가 완료");
        }


        // ====== 폼 에셋 확인 ======

        private static void EnsureFormAssetsExist()
        {
            var forms = Resources.LoadAll<FormData>("Data/Forms");

            if (forms == null || forms.Length == 0)
            {
                Debug.Log("[FormTestSceneSetup] 폼 에셋이 없습니다. 기본 폼 생성 중...");
                FormAssetCreator.CreateDefaultForms();
            }
        }


        // ====== 테스트 플레이어 생성 ======

        private static void CreateTestPlayer()
        {
            // 기존 테스트 플레이어 제거
            var existing = GameObject.Find("FormTestPlayer");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            // 플레이어 오브젝트 생성
            var player = new GameObject("FormTestPlayer");
            player.transform.position = Vector3.zero;
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");

            // 스프라이트 렌더러
            var spriteRenderer = player.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.white;

            // 기본 스프라이트 설정 (흰색 사각형)
            spriteRenderer.sprite = CreateDefaultSprite();

            // Rigidbody2D
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // BoxCollider2D
            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);

            // AudioSource
            player.AddComponent<AudioSource>();

            // FormSwapSystem
            var swapSystem = player.AddComponent<FormSwapSystem>();

            // FormManager
            var formManager = player.AddComponent<FormManager>();

            // FormManager에 SwapSystem 연결 (Reflection 사용)
            var swapSystemField = typeof(FormManager).GetField("swapSystem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            swapSystemField?.SetValue(formManager, swapSystem);

            // 기본 폼 설정
            var forms = Resources.LoadAll<FormData>("Data/Forms");
            if (forms != null && forms.Length > 0)
            {
                var defaultFormField = typeof(FormManager).GetField("defaultForm",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                defaultFormField?.SetValue(formManager, forms[0]);
            }

            // FormAwakeningEffects
            player.AddComponent<FormAwakeningEffects>();

            // 테스트용 이동 스크립트
            player.AddComponent<FormTestPlayerController>();

            Selection.activeGameObject = player;
            Debug.Log("[FormTestSceneSetup] FormTestPlayer 생성 완료");
        }


        // ====== 테스트 UI 생성 ======

        private static void CreateTestUI()
        {
            // 기존 UI 제거
            var existingCanvas = GameObject.Find("FormTestUI");
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas);
            }

            // Canvas 생성
            var canvasObj = new GameObject("FormTestUI");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 폼 슬롯 UI 패널
            var slotsPanel = CreateUIPanel(canvasObj.transform, "FormSlotsPanel",
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(300, 100));
            slotsPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            slotsPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            slotsPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
            slotsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 20);

            // 슬롯 1
            var slot1 = CreateFormSlotUI(slotsPanel.transform, "Slot1", new Vector2(0, 0));

            // 슬롯 2
            var slot2 = CreateFormSlotUI(slotsPanel.transform, "Slot2", new Vector2(110, 0));

            // 쿨다운 UI
            var cooldownUI = CreateCooldownUI(canvasObj.transform);

            // 상태 표시 텍스트
            var statusText = CreateStatusText(canvasObj.transform);

            // FormUIManager 추가
            var uiManager = canvasObj.AddComponent<FormUIManager>();

            // FormTestUIController 추가 (테스트용)
            canvasObj.AddComponent<FormTestUIController>();

            Debug.Log("[FormTestSceneSetup] FormTestUI 생성 완료");
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;

            var image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.5f);

            return panel;
        }

        private static GameObject CreateFormSlotUI(Transform parent, string name, Vector2 position)
        {
            var slot = new GameObject(name);
            slot.transform.SetParent(parent, false);

            var rect = slot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(100, 100);

            // 배경
            var bg = slot.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 아이콘
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slot.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(5, 25);
            iconRect.offsetMax = new Vector2(-5, -5);
            var icon = iconObj.AddComponent<UnityEngine.UI.Image>();
            icon.color = Color.gray;

            // 이름 텍스트
            var nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(slot.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0);
            nameRect.pivot = new Vector2(0.5f, 0);
            nameRect.anchoredPosition = new Vector2(0, 5);
            nameRect.sizeDelta = new Vector2(0, 20);
            var nameText = nameObj.AddComponent<UnityEngine.UI.Text>();
            nameText.text = "Empty";
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.fontSize = 12;
            nameText.color = Color.white;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // FormSlotUI 컴포넌트
            slot.AddComponent<FormSlotUI>();

            return slot;
        }

        private static GameObject CreateCooldownUI(Transform parent)
        {
            var cooldown = new GameObject("CooldownUI");
            cooldown.transform.SetParent(parent, false);

            var rect = cooldown.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 20);
            rect.sizeDelta = new Vector2(200, 30);

            // 배경
            var bg = cooldown.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            // Fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(cooldown.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);
            var fill = fillObj.AddComponent<UnityEngine.UI.Image>();
            fill.color = new Color(0.3f, 0.7f, 1f, 1f);
            fill.type = UnityEngine.UI.Image.Type.Filled;
            fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;

            // 텍스트
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(cooldown.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Ready [Q]";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 14;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            cooldown.AddComponent<FormCooldownUI>();

            return cooldown;
        }

        private static GameObject CreateStatusText(Transform parent)
        {
            var status = new GameObject("StatusText");
            status.transform.SetParent(parent, false);

            var rect = status.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(400, 200);

            var text = status.AddComponent<UnityEngine.UI.Text>();
            text.text = "폼 시스템 테스트\n\n[Q] 폼 교체\n[F] 픽업 상호작용\n[방향키/WASD] 이동\n[Space] 점프";
            text.alignment = TextAnchor.UpperLeft;
            text.fontSize = 16;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 그림자
            var shadow = status.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(1, -1);

            return status;
        }


        // ====== 테스트 픽업 생성 ======

        private static void CreateTestPickups()
        {
            // 기존 픽업 제거
            var existing = GameObject.Find("TestFormPickups");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var pickupsParent = new GameObject("TestFormPickups");
            pickupsParent.transform.position = Vector3.zero;

            var forms = Resources.LoadAll<FormData>("Data/Forms");
            if (forms == null || forms.Length == 0)
            {
                Debug.LogWarning("[FormTestSceneSetup] 폼 에셋이 없어 픽업을 생성할 수 없습니다.");
                return;
            }

            float spacing = 2f;
            float startX = -((forms.Length - 1) * spacing) / 2f;

            for (int i = 0; i < forms.Length; i++)
            {
                var pickup = CreateSinglePickup(forms[i], new Vector3(startX + i * spacing, 1f, 0));
                pickup.transform.SetParent(pickupsParent.transform);
            }

            Debug.Log($"[FormTestSceneSetup] {forms.Length}개 테스트 픽업 생성 완료");
        }

        private static GameObject CreateSinglePickup(FormData formData, Vector3 position)
        {
            var pickup = new GameObject($"Pickup_{formData.formName}");
            pickup.transform.position = position;
            pickup.layer = LayerMask.NameToLayer("Default");

            // 스프라이트 렌더러
            var sr = pickup.AddComponent<SpriteRenderer>();
            sr.sprite = formData.icon != null ? formData.icon : CreateDefaultSprite();
            sr.color = formData.formColor;
            sr.sortingOrder = 5;

            // 콜라이더
            var collider = pickup.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            // FormPickup 컴포넌트
            var formPickup = pickup.AddComponent<FormPickup>();

            // FormData 설정 (Reflection)
            var formDataField = typeof(FormPickup).GetField("formData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            formDataField?.SetValue(formPickup, formData);

            // 이름 표시
            var nameLabel = new GameObject("NameLabel");
            nameLabel.transform.SetParent(pickup.transform, false);
            nameLabel.transform.localPosition = new Vector3(0, -0.8f, 0);

            var textMesh = nameLabel.AddComponent<TextMesh>();
            textMesh.text = formData.formName;
            textMesh.fontSize = 24;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            return pickup;
        }


        // ====== 테스트 환경 ======

        private static void CreateTestGround()
        {
            // 기존 바닥 제거
            var existing = GameObject.Find("TestGround");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var ground = new GameObject("TestGround");
            ground.transform.position = new Vector3(0, -2f, 0);
            ground.layer = LayerMask.NameToLayer("Default");

            var sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDefaultSprite();
            sr.color = new Color(0.3f, 0.3f, 0.3f);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(20f, 1f);
            sr.sortingOrder = -1;

            var collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(20f, 1f);

            Debug.Log("[FormTestSceneSetup] TestGround 생성 완료");
        }

        private static void CreateTestCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 0, -10);
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 5f;
                mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            }
        }


        // ====== 유틸리티 ======

        private static Sprite CreateDefaultSprite()
        {
            var texture = new Texture2D(64, 64);
            var colors = new Color[64 * 64];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                var currentPath = parts[0];

                for (int i = 1; i < parts.Length; i++)
                {
                    var newPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = newPath;
                }
            }
        }
    }
}
