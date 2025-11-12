using UnityEngine;
using UnityEditor;
using System.IO;
using GASPT.Gameplay.Player;
using GASPT.Form;
using GASPT.Gameplay.Projectiles;
using GASPT.Gameplay.Effects;
using GASPT.Enemies;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Enemy;

namespace GASPT.Editor
{
    /// <summary>
    /// 게임 프리팹 자동 생성 에디터 도구
    /// 한 번의 클릭으로 모든 필요한 프리팹 생성
    /// </summary>
    public class PrefabCreator : EditorWindow
    {
        private const string PrefabsPath = "Assets/Resources/Prefabs";
        private const string PlayerPrefabsPath = "Assets/Resources/Prefabs/Player";
        private const string ProjectilesPrefabsPath = "Assets/Resources/Prefabs/Projectiles";
        private const string EffectsPrefabsPath = "Assets/Resources/Prefabs/Effects";
        private const string EnemiesPrefabsPath = "Assets/Resources/Prefabs/Enemies";
        private const string TexturesPath = "Assets/Resources/Textures/Placeholders";

        private Vector2 scrollPosition;
        private bool createMageForm = true;
        private bool createProjectiles = true;
        private bool createEffects = true;
        private bool createEnemy = true;

        [MenuItem("Tools/GASPT/Prefab Creator")]
        public static void ShowWindow()
        {
            PrefabCreator window = GetWindow<PrefabCreator>("Prefab Creator");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== GASPT Prefab Creator ===", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "이 도구는 게임에 필요한 모든 프리팹을 자동으로 생성합니다.\n" +
                "생성 위치: Resources/Prefabs/\n\n" +
                "생성될 프리팹:\n" +
                "- MageForm (플레이어)\n" +
                "- MagicMissileProjectile\n" +
                "- FireballProjectile\n" +
                "- VisualEffect (범용 효과)\n" +
                "- BasicMeleeEnemy",
                MessageType.Info
            );

            GUILayout.Space(20);

            // 프리팹 선택
            EditorGUILayout.LabelField("생성할 프리팹 선택:", EditorStyles.boldLabel);
            createMageForm = EditorGUILayout.Toggle("MageForm (플레이어)", createMageForm);
            createProjectiles = EditorGUILayout.Toggle("Projectiles (투사체)", createProjectiles);
            createEffects = EditorGUILayout.Toggle("VisualEffect (효과)", createEffects);
            createEnemy = EditorGUILayout.Toggle("BasicMeleeEnemy (적)", createEnemy);

            GUILayout.Space(20);

            // 전체 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 모든 프리팹 생성", GUILayout.Height(40)))
            {
                CreateAllPrefabs();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼들
            EditorGUILayout.LabelField("개별 생성:", EditorStyles.boldLabel);

            if (GUILayout.Button("MageForm 프리팹 생성"))
            {
                CreateMageFormPrefab();
            }

            if (GUILayout.Button("Projectile 프리팹 생성"))
            {
                CreateProjectilePrefabs();
            }

            if (GUILayout.Button("VisualEffect 프리팹 생성"))
            {
                CreateVisualEffectPrefab();
            }

            if (GUILayout.Button("BasicMeleeEnemy 프리팹 생성"))
            {
                CreateBasicMeleeEnemyPrefab();
            }

            GUILayout.Space(20);

            // 폴더 생성 버튼
            if (GUILayout.Button("프리팹 폴더 생성"))
            {
                CreatePrefabFolders();
            }

            GUILayout.Space(10);

            // 정보 표시
            EditorGUILayout.HelpBox(
                $"프리팹 저장 경로:\n" +
                $"Player: {PlayerPrefabsPath}\n" +
                $"Projectiles: {ProjectilesPrefabsPath}\n" +
                $"Effects: {EffectsPrefabsPath}\n" +
                $"Enemies: {EnemiesPrefabsPath}",
                MessageType.None
            );

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 모든 프리팹 생성
        /// </summary>
        private void CreateAllPrefabs()
        {
            Debug.Log("=== 프리팹 생성 시작 ===");

            // 폴더 생성
            CreatePrefabFolders();

            int createdCount = 0;

            if (createMageForm)
            {
                CreateMageFormPrefab();
                createdCount++;
            }

            if (createProjectiles)
            {
                CreateProjectilePrefabs();
                createdCount += 2; // MagicMissile + Fireball
            }

            if (createEffects)
            {
                CreateVisualEffectPrefab();
                createdCount++;
            }

            if (createEnemy)
            {
                CreateBasicMeleeEnemyPrefab();
                createdCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"=== 프리팹 생성 완료! 총 {createdCount}개 ===");
            EditorUtility.DisplayDialog("완료", $"{createdCount}개의 프리팹이 생성되었습니다!", "확인");
        }

        /// <summary>
        /// 프리팹 폴더 생성
        /// </summary>
        private void CreatePrefabFolders()
        {
            CreateFolderIfNotExists(PrefabsPath);
            CreateFolderIfNotExists(PlayerPrefabsPath);
            CreateFolderIfNotExists(ProjectilesPrefabsPath);
            CreateFolderIfNotExists(EffectsPrefabsPath);
            CreateFolderIfNotExists(EnemiesPrefabsPath);
            CreateFolderIfNotExists(TexturesPath);

            AssetDatabase.Refresh();
            Debug.Log("[PrefabCreator] 프리팹 폴더 생성 완료");
        }

        /// <summary>
        /// MageForm 프리팹 생성
        /// </summary>
        private void CreateMageFormPrefab()
        {
            string prefabPath = $"{PlayerPrefabsPath}/MageForm.prefab";

            // GameObject 생성
            GameObject mageFormObj = new GameObject("MageForm");

            // 컴포넌트 추가
            Rigidbody2D rb = mageFormObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 3f;

            BoxCollider2D collider = mageFormObj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 2f);
            collider.offset = new Vector2(0f, 1f);

            SpriteRenderer sr = mageFormObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.5f, 0.5f, 1f, 1f)); // 파란색 (Mage)
            sr.color = Color.white; // 스프라이트 색상 유지

            PlayerController playerController = mageFormObj.AddComponent<PlayerController>();
            FormInputHandler formInputHandler = mageFormObj.AddComponent<FormInputHandler>();
            MageForm mageForm = mageFormObj.AddComponent<MageForm>();

            // GroundCheck 자식 오브젝트 생성
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(mageFormObj.transform);
            groundCheck.transform.localPosition = new Vector3(0f, 0f, 0f); // 발 위치

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(mageFormObj, prefabPath);
            DestroyImmediate(mageFormObj);

            Debug.Log($"[PrefabCreator] MageForm 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// Projectile 프리팹들 생성
        /// </summary>
        private void CreateProjectilePrefabs()
        {
            CreateMagicMissileProjectilePrefab();
            CreateFireballProjectilePrefab();
        }

        /// <summary>
        /// MagicMissileProjectile 프리팹 생성
        /// </summary>
        private void CreateMagicMissileProjectilePrefab()
        {
            string prefabPath = $"{ProjectilesPrefabsPath}/MagicMissileProjectile.prefab";

            GameObject projectileObj = new GameObject("MagicMissileProjectile");

            // SpriteRenderer 추가 (작은 파란 구체)
            SpriteRenderer sr = projectileObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(Color.cyan);
            sr.color = Color.white; // 스프라이트 색상 유지
            projectileObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            // Collider2D 추가
            CircleCollider2D collider = projectileObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.15f;
            collider.isTrigger = true;

            // PooledObject 추가
            PooledObject pooledObject = projectileObj.AddComponent<PooledObject>();

            // MagicMissileProjectile 추가
            MagicMissileProjectile projectile = projectileObj.AddComponent<MagicMissileProjectile>();

            // SerializedObject로 private 필드 설정
            SerializedObject so = new SerializedObject(projectile);
            so.FindProperty("speed").floatValue = 15f;
            so.FindProperty("maxDistance").floatValue = 20f;
            so.FindProperty("damage").floatValue = 10f;
            so.FindProperty("collisionRadius").floatValue = 0.3f;
            so.FindProperty("projectileRenderer").objectReferenceValue = sr;

            // targetLayers 설정 (Enemy Layer 포함)
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[PrefabCreator] 'Enemy' Layer가 없습니다! Project Settings > Tags and Layers에서 Layer 6을 'Enemy'로 추가하세요.");
                // 기본적으로 모든 레이어 대상
                so.FindProperty("targetLayers").intValue = ~0;
            }
            else
            {
                so.FindProperty("targetLayers").intValue = 1 << enemyLayer;
            }

            so.ApplyModifiedProperties();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(projectileObj, prefabPath);
            DestroyImmediate(projectileObj);

            Debug.Log($"[PrefabCreator] MagicMissileProjectile 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// FireballProjectile 프리팹 생성
        /// </summary>
        private void CreateFireballProjectilePrefab()
        {
            string prefabPath = $"{ProjectilesPrefabsPath}/FireballProjectile.prefab";

            GameObject projectileObj = new GameObject("FireballProjectile");

            // SpriteRenderer 추가 (큰 빨간 구체)
            SpriteRenderer sr = projectileObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(1f, 0.5f, 0f, 1f)); // 주황색
            sr.color = Color.white; // 스프라이트 색상 유지
            projectileObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            // Collider2D 추가
            CircleCollider2D collider = projectileObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.25f;
            collider.isTrigger = true;

            // PooledObject 추가
            PooledObject pooledObject = projectileObj.AddComponent<PooledObject>();

            // FireballProjectile 추가
            FireballProjectile projectile = projectileObj.AddComponent<FireballProjectile>();

            // SerializedObject로 private 필드 설정
            SerializedObject so = new SerializedObject(projectile);
            so.FindProperty("speed").floatValue = 8f;
            so.FindProperty("maxDistance").floatValue = 15f;
            so.FindProperty("damage").floatValue = 50f;
            so.FindProperty("collisionRadius").floatValue = 0.5f;
            so.FindProperty("explosionRadius").floatValue = 3f;
            so.FindProperty("projectileRenderer").objectReferenceValue = sr;

            // targetLayers 설정 (Enemy Layer 포함)
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[PrefabCreator] 'Enemy' Layer가 없습니다! Project Settings > Tags and Layers에서 Layer 6을 'Enemy'로 추가하세요.");
                // 기본적으로 모든 레이어 대상
                so.FindProperty("targetLayers").intValue = ~0;
            }
            else
            {
                so.FindProperty("targetLayers").intValue = 1 << enemyLayer;
            }

            so.ApplyModifiedProperties();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(projectileObj, prefabPath);
            DestroyImmediate(projectileObj);

            Debug.Log($"[PrefabCreator] FireballProjectile 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// VisualEffect 프리팹 생성
        /// </summary>
        private void CreateVisualEffectPrefab()
        {
            string prefabPath = $"{EffectsPrefabsPath}/VisualEffect.prefab";

            GameObject effectObj = new GameObject("VisualEffect");

            // SpriteRenderer 추가 (흰색 원)
            SpriteRenderer sr = effectObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(Color.white);
            sr.color = Color.white;
            effectObj.transform.localScale = new Vector3(1f, 1f, 1f);

            // PooledObject 추가
            PooledObject pooledObject = effectObj.AddComponent<PooledObject>();

            // VisualEffect 추가
            VisualEffect visualEffect = effectObj.AddComponent<VisualEffect>();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(effectObj, prefabPath);
            DestroyImmediate(effectObj);

            Debug.Log($"[PrefabCreator] VisualEffect 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// BasicMeleeEnemy 프리팹 생성
        /// </summary>
        private void CreateBasicMeleeEnemyPrefab()
        {
            string prefabPath = $"{EnemiesPrefabsPath}/BasicMeleeEnemy.prefab";

            GameObject enemyObj = new GameObject("BasicMeleeEnemy");

            // 컴포넌트 추가
            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 3f;

            BoxCollider2D collider = enemyObj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1.5f);
            collider.offset = new Vector2(0f, 0.75f);

            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(1f, 0.3f, 0.3f, 1f)); // 빨간색 (Enemy)
            sr.color = Color.white; // 스프라이트 색상 유지

            // Layer 설정 (Enemy)
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[PrefabCreator] 'Enemy' Layer가 없습니다! Project Settings > Tags and Layers에서 Layer 6을 'Enemy'로 추가하세요.");
                enemyObj.layer = 0; // Default layer
            }
            else
            {
                enemyObj.layer = enemyLayer;
            }

            PooledObject pooledObject = enemyObj.AddComponent<PooledObject>();
            Enemy enemy = enemyObj.AddComponent<Enemy>();
            BasicMeleeEnemy basicMeleeEnemy = enemyObj.AddComponent<BasicMeleeEnemy>();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);
            DestroyImmediate(enemyObj);

            Debug.Log($"[PrefabCreator] BasicMeleeEnemy 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// Placeholder 스프라이트 생성 및 에셋으로 저장
        /// </summary>
        private Sprite CreatePlaceholderSprite(Color color)
        {
            // 색상 기반 파일명 생성
            string colorName = GetColorName(color);
            string texturePath = $"{TexturesPath}/Placeholder_{colorName}.png";
            string textureAssetPath = texturePath;

            // 이미 존재하면 로드
            Texture2D existingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            if (existingTexture != null)
            {
                // 기존 텍스처에서 스프라이트 로드
                string[] assetPaths = AssetDatabase.FindAssets($"Placeholder_{colorName} t:Sprite", new[] { TexturesPath });
                if (assetPaths.Length > 0)
                {
                    string spritePath = AssetDatabase.GUIDToAssetPath(assetPaths[0]);
                    Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (existingSprite != null)
                    {
                        return existingSprite;
                    }
                }
            }

            // 새 텍스처 생성
            Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            // PNG로 인코딩 및 저장
            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(texturePath, pngData);

            AssetDatabase.ImportAsset(textureAssetPath);

            // TextureImporter 설정 (Sprite로 변환)
            TextureImporter importer = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            // 생성된 Sprite 로드
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(textureAssetPath);

            if (sprite == null)
            {
                Debug.LogError($"[PrefabCreator] Sprite 생성 실패: {textureAssetPath}");
            }
            else
            {
                Debug.Log($"[PrefabCreator] Sprite 생성 완료: {textureAssetPath}");
            }

            return sprite;
        }

        /// <summary>
        /// 색상에 따른 이름 생성
        /// </summary>
        private string GetColorName(Color color)
        {
            // RGB 값을 16진수로 변환
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// 폴더가 없으면 생성
        /// </summary>
        private void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = Path.GetDirectoryName(path);
                string folderName = Path.GetFileName(path);

                // 부모 폴더가 없으면 재귀적으로 생성
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    CreateFolderIfNotExists(parentPath);
                }

                AssetDatabase.CreateFolder(parentPath, folderName);
                Debug.Log($"[PrefabCreator] 폴더 생성: {path}");
            }
        }
    }
}
