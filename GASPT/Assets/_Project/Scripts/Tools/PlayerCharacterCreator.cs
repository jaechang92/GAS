using UnityEngine;
using UnityEditor;
using Character.Physics;
using Player;
using GAS.Core;
using System.IO;

namespace ProjectTools
{
    /// <summary>
    /// 새로운 물리 시스템을 사용하는 PlayerCharacter 자동 생성 도구
    /// PhysicsEngine + CollisionDetector + MovementProcessor + PhysicsState 기반
    /// </summary>
    public class PlayerCharacterCreator : EditorWindow
    {
        [Header("기본 설정")]
        private Sprite playerSprite;
        private CharacterPhysicsConfig physicsConfig;

        [Header("고급 설정")]
        private bool autoCreatePhysicsConfig = true;
        private bool createPrefab = true;
        private bool placeInScene = false;
        private Vector3 spawnPosition = Vector3.zero;

        [Header("컴포넌트 설정")]
        private bool addInputHandler = true;
        private bool addAnimationController = true;
        private bool addAbilitySystem = true;
        private bool setupCollider = true;

        [Header("물리 설정")]
        private bool useSkullPreset = true;
        private float moveSpeed = 10f;
        private float jumpForce = 18f;
        private float dashSpeed = 25f;

        private Vector2 scrollPosition;

        [MenuItem("Tools/Project/Character Creator (New Physics)")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerCharacterCreator>("Character Creator");
            window.minSize = new Vector2(400, 600);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawBasicSettings();
            DrawAdvancedSettings();
            DrawComponentSettings();
            DrawPhysicsSettings();
            DrawButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);

            GUILayout.Label("🎮 플레이어 캐릭터 생성 도구", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "새로운 물리 시스템을 사용하는 플레이어 캐릭터를 자동으로 생성합니다.\n\n" +
                "포함 컴포넌트:\n" +
                "• PlayerController (FSM 상태 관리)\n" +
                "• PhysicsEngine (통합 물리 시스템)\n" +
                "• CollisionDetector (레이캐스트 충돌 검사)\n" +
                "• MovementProcessor (이동 계산)\n" +
                "• PhysicsState (상태 관리)\n" +
                "• InputHandler (입력 처리)\n" +
                "• AnimationController (애니메이션)\n" +
                "• AbilitySystem (GAS 통합)",
                MessageType.Info
            );

            EditorGUILayout.Space(10);
        }

        private void DrawBasicSettings()
        {
            EditorGUILayout.LabelField("기본 설정", EditorStyles.boldLabel);

            playerSprite = (Sprite)EditorGUILayout.ObjectField(
                "플레이어 스프라이트",
                playerSprite,
                typeof(Sprite),
                false
            );

            physicsConfig = (CharacterPhysicsConfig)EditorGUILayout.ObjectField(
                "물리 설정",
                physicsConfig,
                typeof(CharacterPhysicsConfig),
                false
            );

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 자동으로 스프라이트 찾기", GUILayout.Height(25)))
            {
                FindPlayerSprite();
            }
            if (GUILayout.Button("⚙️ 자동으로 Physics Config 찾기", GUILayout.Height(25)))
            {
                FindPhysicsConfig();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField("고급 설정", EditorStyles.boldLabel);

            autoCreatePhysicsConfig = EditorGUILayout.Toggle("Physics Config 자동 생성", autoCreatePhysicsConfig);
            createPrefab = EditorGUILayout.Toggle("프리팹으로 저장", createPrefab);
            placeInScene = EditorGUILayout.Toggle("씬에 배치", placeInScene);

            if (placeInScene)
            {
                EditorGUI.indentLevel++;
                spawnPosition = EditorGUILayout.Vector3Field("생성 위치", spawnPosition);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
        }

        private void DrawComponentSettings()
        {
            EditorGUILayout.LabelField("컴포넌트 설정", EditorStyles.boldLabel);

            addInputHandler = EditorGUILayout.Toggle("InputHandler 추가", addInputHandler);
            addAnimationController = EditorGUILayout.Toggle("AnimationController 추가", addAnimationController);
            addAbilitySystem = EditorGUILayout.Toggle("AbilitySystem 추가", addAbilitySystem);
            setupCollider = EditorGUILayout.Toggle("Collider 자동 설정", setupCollider);

            EditorGUILayout.Space(10);
        }

        private void DrawPhysicsSettings()
        {
            EditorGUILayout.LabelField("물리 설정", EditorStyles.boldLabel);

            useSkullPreset = EditorGUILayout.Toggle("Skul 프리셋 사용", useSkullPreset);

            if (!useSkullPreset)
            {
                EditorGUI.indentLevel++;
                moveSpeed = EditorGUILayout.FloatField("이동 속도", moveSpeed);
                jumpForce = EditorGUILayout.FloatField("점프력", jumpForce);
                dashSpeed = EditorGUILayout.FloatField("대시 속도", dashSpeed);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
        }

        private void DrawButtons()
        {
            EditorGUILayout.LabelField("생성", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(playerSprite == null && !autoCreatePhysicsConfig);

            if (GUILayout.Button("🎯 플레이어 캐릭터 생성", GUILayout.Height(40)))
            {
                CreatePlayerCharacter();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("📋 CharacterPhysicsConfig 생성", GUILayout.Height(30)))
            {
                CreateCharacterPhysicsConfig();
            }

            if (GUILayout.Button("🔧 기존 캐릭터 업그레이드", GUILayout.Height(30)))
            {
                UpgradeExistingCharacter();
            }
        }

        /// <summary>
        /// 플레이어 스프라이트 자동 찾기
        /// </summary>
        private void FindPlayerSprite()
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path).ToLower();

                if (fileName.Contains("skull") || fileName.Contains("player") || fileName.Contains("character"))
                {
                    playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    Debug.Log($"✅ 플레이어 스프라이트를 찾았습니다: {path}");
                    break;
                }
            }

            if (playerSprite == null)
            {
                Debug.LogWarning("❌ 적절한 플레이어 스프라이트를 찾을 수 없습니다. 수동으로 할당해주세요.");
            }
        }

        /// <summary>
        /// Physics Config 자동 찾기
        /// </summary>
        private void FindPhysicsConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterPhysicsConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                physicsConfig = AssetDatabase.LoadAssetAtPath<CharacterPhysicsConfig>(path);
                Debug.Log($"✅ Physics Config를 찾았습니다: {path}");
            }
            else
            {
                Debug.LogWarning("❌ CharacterPhysicsConfig를 찾을 수 없습니다. 자동 생성을 활성화하거나 수동으로 생성해주세요.");
            }
        }

        /// <summary>
        /// 플레이어 캐릭터 생성
        /// </summary>
        private void CreatePlayerCharacter()
        {
            Debug.Log("🎮 새로운 물리 시스템 기반 플레이어 캐릭터 생성 시작...");

            // Physics Config 자동 생성
            if (physicsConfig == null && autoCreatePhysicsConfig)
            {
                physicsConfig = CreateCharacterPhysicsConfig();
            }

            // 게임오브젝트 생성
            GameObject playerCharacter = CreatePlayerCharacterGameObject();

            // 프리팹으로 저장
            if (createPrefab)
            {
                SaveAsPrefab(playerCharacter);
            }

            // 씬에 배치
            if (placeInScene)
            {
                playerCharacter.transform.position = spawnPosition;
                Selection.activeGameObject = playerCharacter;
                Debug.Log("✅ 플레이어 캐릭터가 씬에 배치되었습니다!");
            }
            else if (!createPrefab)
            {
                // 프리팹도 안 만들고 씬에도 안 놓으면 임시 오브젝트 삭제
                DestroyImmediate(playerCharacter);
                Debug.Log("✅ 플레이어 캐릭터 생성 완료 (임시 오브젝트)");
            }

            Debug.Log("🎯 새로운 물리 시스템 기반 플레이어 캐릭터 생성 완료!");
        }

        /// <summary>
        /// 플레이어 캐릭터 게임오브젝트 생성
        /// </summary>
        private GameObject CreatePlayerCharacterGameObject()
        {
            GameObject playerCharacter = new GameObject("PlayerCharacter");
            playerCharacter.transform.position = Vector3.zero;
            playerCharacter.transform.localScale = Vector3.one;
            playerCharacter.tag = "Player";

            // 1. SpriteRenderer 추가
            SetupSpriteRenderer(playerCharacter);

            // 2. BoxCollider2D 추가
            if (setupCollider)
            {
                SetupCollider(playerCharacter);
            }

            // 3. AbilitySystem 추가 (PlayerController의 의존성)
            if (addAbilitySystem)
            {
                playerCharacter.AddComponent<AbilitySystem>();
                Debug.Log("- AbilitySystem 추가됨");
            }

            // 4. PhysicsEngine 추가 (새로운 통합 물리 시스템)
            var physicsEngine = playerCharacter.AddComponent<Character.Physics.PhysicsEngine>();
            Debug.Log("- PhysicsEngine 추가됨 (통합 물리 시스템)");

            // 5. InputHandler 추가
            if (addInputHandler)
            {
                playerCharacter.AddComponent<InputHandler>();
                Debug.Log("- InputHandler 추가됨");
            }

            // 6. AnimationController 추가
            if (addAnimationController)
            {
                playerCharacter.AddComponent<AnimationController>();
                Debug.Log("- AnimationController 추가됨");
            }

            // 7. PlayerController 추가 (마지막에 추가)
            playerCharacter.AddComponent<PlayerController>();
            Debug.Log("- PlayerController 추가됨");

            // 8. Physics Config 설정
            SetupPhysicsConfig(physicsEngine);

            return playerCharacter;
        }

        /// <summary>
        /// SpriteRenderer 설정
        /// </summary>
        private void SetupSpriteRenderer(GameObject playerCharacter)
        {
            var spriteRenderer = playerCharacter.AddComponent<SpriteRenderer>();

            if (playerSprite != null)
            {
                spriteRenderer.sprite = playerSprite;
            }
            else
            {
                // 기본 스프라이트 생성
                spriteRenderer.sprite = CreateDefaultSprite();
                Debug.Log("- 기본 스프라이트 생성됨");
            }

            spriteRenderer.sortingOrder = 10;
            Debug.Log("- SpriteRenderer 설정됨");
        }

        /// <summary>
        /// Collider 설정
        /// </summary>
        private void SetupCollider(GameObject playerCharacter)
        {
            var boxCollider = playerCharacter.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(0.8f, 0.9f);
            boxCollider.offset = new Vector2(0, -0.05f);
            boxCollider.isTrigger = false;
            Debug.Log("- BoxCollider2D 설정됨");
        }

        /// <summary>
        /// Physics Config 설정
        /// </summary>
        private void SetupPhysicsConfig(Character.Physics.PhysicsEngine physicsEngine)
        {
            if (physicsConfig != null)
            {
                var configField = typeof(Character.Physics.PhysicsEngine).GetField("config",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (configField != null)
                {
                    configField.SetValue(physicsEngine, physicsConfig);
                    Debug.Log($"- CharacterPhysicsConfig 할당됨 (Skul Preset: {physicsConfig.useSkulPreset})");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ CharacterPhysicsConfig가 없습니다. 런타임에 오류가 발생할 수 있습니다.");
            }
        }

        /// <summary>
        /// 기본 스프라이트 생성
        /// </summary>
        private Sprite CreateDefaultSprite()
        {
            var texture = new Texture2D(64, 128);
            var pixels = new Color[texture.width * texture.height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.3f, 0.7f, 1f, 1f); // 하늘색
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        /// <summary>
        /// CharacterPhysicsConfig 생성
        /// </summary>
        private CharacterPhysicsConfig CreateCharacterPhysicsConfig()
        {
            string configPath = "Assets/_Project/Data/CharacterPhysicsConfig.asset";

            // 디렉토리 생성
            string directory = Path.GetDirectoryName(configPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            }

            // CharacterPhysicsConfig 생성
            var config = ScriptableObject.CreateInstance<CharacterPhysicsConfig>();

            // 설정 적용
            if (useSkullPreset)
            {
                config.ApplySkulPreset();
            }
            else
            {
                config.moveSpeed = moveSpeed;
                config.jumpForce = jumpForce;
                config.dashSpeed = dashSpeed;
            }

            // 에셋 저장
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ CharacterPhysicsConfig 생성됨: {configPath}");
            return config;
        }

        /// <summary>
        /// 프리팹으로 저장
        /// </summary>
        private void SaveAsPrefab(GameObject playerCharacter)
        {
            string prefabPath = "Assets/_Project/Prefabs/PlayerCharacter.prefab";

            // 디렉토리 생성
            string directory = Path.GetDirectoryName(prefabPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }

            // 프리팹 생성
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(playerCharacter, prefabPath);

            if (!placeInScene)
            {
                DestroyImmediate(playerCharacter);
            }

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"✅ 프리팹 저장됨: {prefabPath}");
        }

        /// <summary>
        /// 기존 캐릭터 업그레이드
        /// </summary>
        private void UpgradeExistingCharacter()
        {
            var selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("오류", "업그레이드할 게임오브젝트를 선택해주세요.", "확인");
                return;
            }

            Debug.Log($"🔧 {selectedObject.name} 업그레이드 시작...");

            // 기존 구 시스템 컴포넌트 제거
            RemoveOldComponents(selectedObject);

            // 새로운 시스템 컴포넌트 추가
            AddNewComponents(selectedObject);

            Debug.Log("✅ 캐릭터 업그레이드 완료!");
        }

        /// <summary>
        /// 구 시스템 컴포넌트 제거
        /// </summary>
        private void RemoveOldComponents(GameObject target)
        {
            // RaycastController 제거
            var oldRaycast = target.GetComponent<RaycastController>();
            if (oldRaycast != null)
            {
                DestroyImmediate(oldRaycast);
                Debug.Log("- RaycastController 제거됨");
            }

        }

        /// <summary>
        /// 새로운 시스템 컴포넌트 추가
        /// </summary>
        private void AddNewComponents(GameObject target)
        {
            // PhysicsEngine 추가
            if (target.GetComponent<Character.Physics.PhysicsEngine>() == null)
            {
                var physicsEngine = target.AddComponent<Character.Physics.PhysicsEngine>();
                SetupPhysicsConfig(physicsEngine);
                Debug.Log("- PhysicsEngine 추가됨");
            }

            // 필수 컴포넌트들 확인 및 추가
            if (addInputHandler && target.GetComponent<InputHandler>() == null)
            {
                target.AddComponent<InputHandler>();
                Debug.Log("- InputHandler 추가됨");
            }

            if (addAnimationController && target.GetComponent<AnimationController>() == null)
            {
                target.AddComponent<AnimationController>();
                Debug.Log("- AnimationController 추가됨");
            }

            if (addAbilitySystem && target.GetComponent<AbilitySystem>() == null)
            {
                target.AddComponent<AbilitySystem>();
                Debug.Log("- AbilitySystem 추가됨");
            }
        }
    }
}
