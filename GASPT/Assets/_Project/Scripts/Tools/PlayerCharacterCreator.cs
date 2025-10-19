using UnityEngine;
using UnityEditor;
using Player.Physics;
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
        private SkulPhysicsConfig physicsConfig;

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

        [MenuItem("GASPT/Character/Create Player (Skul Physics)")]
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
                "Skul 스타일 물리 시스템을 사용하는 플레이어 캐릭터를 자동으로 생성합니다.\n\n" +
                "포함 컴포넌트:\n" +
                "• PlayerController (FSM 상태 관리)\n" +
                "• CharacterPhysics (통합 물리 시스템)\n" +
                "• Rigidbody2D (Unity 물리 엔진)\n" +
                "• BoxCollider2D (충돌 감지)\n" +
                "• SkulPhysicsConfig (Skul 스타일 설정)\n" +
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

            physicsConfig = (SkulPhysicsConfig)EditorGUILayout.ObjectField(
                "물리 설정",
                physicsConfig,
                typeof(SkulPhysicsConfig),
                false
            );

            EditorGUILayout.Space(5);

            if (GUILayout.Button("🔍 자동으로 스프라이트 찾기", GUILayout.Height(25)))
            {
                FindPlayerSprite();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⚙️ Skul Physics Config 찾기", GUILayout.Height(25)))
            {
                FindPhysicsConfig();
            }
            if (GUILayout.Button("🔄 새로고침", GUILayout.Height(25), GUILayout.Width(80)))
            {
                RefreshAndFindPhysicsConfig();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        private void DrawAdvancedSettings()
        {
            EditorGUILayout.LabelField("고급 설정", EditorStyles.boldLabel);

            autoCreatePhysicsConfig = EditorGUILayout.Toggle("Skul Physics Config 자동 생성", autoCreatePhysicsConfig);
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

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 SkulPhysicsConfig 생성", GUILayout.Height(30)))
            {
                CreateSkulPhysicsConfig();
            }
            if (GUILayout.Button("🔧 안전 재생성", GUILayout.Height(30), GUILayout.Width(100)))
            {
                SafeRecreatePhysicsConfig();
            }
            EditorGUILayout.EndHorizontal();

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
        /// Physics Config 자동 찾기 (다중 검색 방식)
        /// </summary>
        private void FindPhysicsConfig()
        {
            // 방법 1: 타입 기반 검색
            string[] guids = AssetDatabase.FindAssets("t:SkulPhysicsConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                physicsConfig = AssetDatabase.LoadAssetAtPath<SkulPhysicsConfig>(path);
                Debug.Log($"✅ Skul Physics Config를 찾았습니다 (타입 검색): {path}");
                return;
            }

            // 방법 2: 파일명 기반 검색
            guids = AssetDatabase.FindAssets("SkulPhysicsConfig");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".asset"))
                {
                    var config = AssetDatabase.LoadAssetAtPath<SkulPhysicsConfig>(path);
                    if (config != null)
                    {
                        physicsConfig = config;
                        Debug.Log($"✅ Skul Physics Config를 찾았습니다 (파일명 검색): {path}");
                        return;
                    }
                }
            }

            // 방법 3: 특정 경로에서 직접 검색
            string[] searchPaths = {
                "Assets/_Project/Data/SkulPhysicsConfig.asset",
                "Assets/Data/SkulPhysicsConfig.asset",
                "Assets/SkulPhysicsConfig.asset"
            };

            foreach (string searchPath in searchPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<SkulPhysicsConfig>(searchPath) != null)
                {
                    physicsConfig = AssetDatabase.LoadAssetAtPath<SkulPhysicsConfig>(searchPath);
                    Debug.Log($"✅ Skul Physics Config를 찾았습니다 (경로 검색): {searchPath}");
                    return;
                }
            }

            // 방법 4: 모든 에셋 폴더 검색
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssetPaths)
            {
                if (assetPath.Contains("SkulPhysicsConfig") && assetPath.EndsWith(".asset"))
                {
                    var config = AssetDatabase.LoadAssetAtPath<SkulPhysicsConfig>(assetPath);
                    if (config != null)
                    {
                        physicsConfig = config;
                        Debug.Log($"✅ Skul Physics Config를 찾았습니다 (전체 검색): {assetPath}");
                        return;
                    }
                }
            }

            Debug.LogWarning("❌ SkulPhysicsConfig를 찾을 수 없습니다.");
            Debug.LogWarning("해결 방법:");
            Debug.LogWarning("1. Unity 에디터에서 Assets → Refresh 실행");
            Debug.LogWarning("2. SkulPhysicsConfig.asset 파일의 스크립트 연결 확인");
            Debug.LogWarning("3. 자동 생성 옵션 활성화");
            Debug.LogWarning("4. Project 창에서 우클릭 → Create → Skul → Physics Config로 수동 생성");
        }

        /// <summary>
        /// AssetDatabase 새로고침 후 Physics Config 찾기
        /// </summary>
        private void RefreshAndFindPhysicsConfig()
        {
            Debug.Log("🔄 AssetDatabase 새로고침 중...");
            AssetDatabase.Refresh();

            // 잠시 대기 후 검색
            System.Threading.Thread.Sleep(100);

            FindPhysicsConfig();

            if (physicsConfig == null)
            {
                Debug.LogWarning("새로고침 후에도 SkulPhysicsConfig를 찾을 수 없습니다.");
                Debug.LogWarning("스크립트 참조가 끊어진 것 같습니다. 안전한 해결 방법:");
                Debug.LogWarning("1. Project창에서 SkulPhysicsConfig.asset 선택");
                Debug.LogWarning("2. Inspector에서 Script 필드의 오른쪽 동그라미 클릭");
                Debug.LogWarning("3. SkulPhysicsConfig 스크립트 다시 선택");
                Debug.LogWarning("4. 또는 '에셋 안전 재생성' 버튼 클릭");
            }
        }

        /// <summary>
        /// 에셋 안전 재생성 (기존 설정 보존)
        /// </summary>
        [ContextMenu("SkulPhysicsConfig 안전 재생성")]
        public void SafeRecreatePhysicsConfig()
        {
            string assetPath = "Assets/_Project/Data/SkulPhysicsConfig.asset";

            // 기존 에셋 로드 시도 (설정 백업용)
            var existingConfig = AssetDatabase.LoadAssetAtPath<SkulPhysicsConfig>(assetPath);

            // 디렉토리 확인
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            }

            // 새 인스턴스 생성
            var newConfig = ScriptableObject.CreateInstance<SkulPhysicsConfig>();

            // 기존 설정이 있다면 복사, 없다면 기본값 적용
            if (existingConfig != null)
            {
                Debug.Log("기존 설정을 새 에셋으로 복사 중...");
                // 기존 설정값들 복사 (리플렉션 사용)
                var fields = typeof(SkulPhysicsConfig).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.IsPublic || field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
                    {
                        field.SetValue(newConfig, field.GetValue(existingConfig));
                    }
                }
            }
            else
            {
                Debug.Log("기존 설정이 없어 Skul 기본 프리셋을 적용합니다...");
                newConfig.ApplyPerfectSkulPreset();
            }

            // 기존 에셋 삭제
            if (AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object)) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            // 새 에셋 생성
            AssetDatabase.CreateAsset(newConfig, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 생성된 에셋 할당
            physicsConfig = newConfig;

            Debug.Log($"✅ SkulPhysicsConfig가 안전하게 재생성되었습니다: {assetPath}");
            Debug.Log("GUID가 새로 생성되어 스크립트 참조 문제가 해결되었습니다.");
        }

        /// <summary>
        /// 플레이어 캐릭터 생성
        /// </summary>
        private void CreatePlayerCharacter()
        {
            Debug.Log("🎮 Skul 스타일 물리 시스템 기반 플레이어 캐릭터 생성 시작...");

            // Skul Physics Config 자동 생성
            if (physicsConfig == null && autoCreatePhysicsConfig)
            {
                physicsConfig = CreateSkulPhysicsConfig();
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

            Debug.Log("🎯 Skul 스타일 물리 시스템 기반 플레이어 캐릭터 생성 완료!");
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

            // 2. Rigidbody2D 추가 (CharacterPhysics 의존성)
            var rigidbody = playerCharacter.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f; // CharacterPhysics가 중력 제어
            rigidbody.freezeRotation = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log("- Rigidbody2D 추가됨");

            // 3. BoxCollider2D 추가
            if (setupCollider)
            {
                SetupCollider(playerCharacter);
            }

            // 4. AbilitySystem 추가 (PlayerController의 의존성)
            if (addAbilitySystem)
            {
                playerCharacter.AddComponent<AbilitySystem>();
                Debug.Log("- AbilitySystem 추가됨");
            }

            // 5. CharacterPhysics 추가 (새로운 Skul 스타일 물리 시스템)
            var characterPhysics = playerCharacter.AddComponent<CharacterPhysics>();
            Debug.Log("- CharacterPhysics 추가됨 (Skul 스타일 물리 시스템)");

            // 6. InputHandler 추가
            if (addInputHandler)
            {
                playerCharacter.AddComponent<InputHandler>();
                Debug.Log("- InputHandler 추가됨");
            }

            // 7. AnimationController 추가
            if (addAnimationController)
            {
                playerCharacter.AddComponent<AnimationController>();
                Debug.Log("- AnimationController 추가됨");
            }

            // 8. PlayerController 추가 (마지막에 추가)
            playerCharacter.AddComponent<PlayerController>();
            Debug.Log("- PlayerController 추가됨");

            // 9. SkulPhysicsTestRunner 추가 (테스트용)
            playerCharacter.AddComponent<SkulPhysicsTestRunner>();
            Debug.Log("- SkulPhysicsTestRunner 추가됨 (테스트용)");

            // 10. Skul Physics Config 설정
            SetupPhysicsConfig(characterPhysics);

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
        /// Skul Physics Config 설정
        /// </summary>
        private void SetupPhysicsConfig(CharacterPhysics characterPhysics)
        {
            if (physicsConfig != null)
            {
                var configField = typeof(CharacterPhysics).GetField("config",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (configField != null)
                {
                    configField.SetValue(characterPhysics, physicsConfig);
                    Debug.Log($"- SkulPhysicsConfig 할당됨");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ SkulPhysicsConfig가 없습니다. 런타임에 오류가 발생할 수 있습니다.");
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
        /// SkulPhysicsConfig 생성
        /// </summary>
        private SkulPhysicsConfig CreateSkulPhysicsConfig()
        {
            string configPath = "Assets/_Project/Data/SkulPhysicsConfig.asset";

            // 디렉토리 생성
            string directory = Path.GetDirectoryName(configPath);
            if (!AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            }

            // SkulPhysicsConfig 생성
            var config = ScriptableObject.CreateInstance<SkulPhysicsConfig>();

            // 설정 적용
            if (useSkullPreset)
            {
                config.ApplyPerfectSkulPreset();
            }
            else
            {
                config.moveSpeed = moveSpeed;
                config.jumpVelocity = jumpForce;
                config.dashSpeed = dashSpeed;
            }

            // 에셋 저장
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ SkulPhysicsConfig 생성됨: {configPath}");
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

            // 새로운 Skul 스타일 시스템 컴포넌트 추가
            AddNewComponents(selectedObject);

            Debug.Log("✅ 캐릭터 업그레이드 완료!");
        }

        /// <summary>
        /// 새로운 Skul 스타일 시스템 컴포넌트 추가
        /// </summary>
        private void AddNewComponents(GameObject target)
        {
            // Rigidbody2D 확인 및 설정
            var rigidbody = target.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = target.AddComponent<Rigidbody2D>();
                Debug.Log("- Rigidbody2D 추가됨");
            }
            rigidbody.gravityScale = 0f;
            rigidbody.freezeRotation = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log("- Rigidbody2D 설정 업데이트됨");

            // BoxCollider2D 확인
            if (target.GetComponent<BoxCollider2D>() == null)
            {
                var collider = target.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(0.8f, 0.9f);
                collider.offset = new Vector2(0, -0.05f);
                Debug.Log("- BoxCollider2D 추가됨");
            }

            // CharacterPhysics 추가
            if (target.GetComponent<CharacterPhysics>() == null)
            {
                var characterPhysics = target.AddComponent<CharacterPhysics>();
                SetupPhysicsConfig(characterPhysics);
                Debug.Log("- CharacterPhysics 추가됨 (Skul 스타일)");
            }

            // SkulPhysicsTestRunner 추가
            if (target.GetComponent<SkulPhysicsTestRunner>() == null)
            {
                target.AddComponent<SkulPhysicsTestRunner>();
                Debug.Log("- SkulPhysicsTestRunner 추가됨");
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
