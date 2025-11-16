using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using GASPT.Gameplay.Player;
using GASPT.Form;
using GASPT.Gameplay.Projectiles;
using GASPT.Gameplay.Effects;
using GASPT.Gameplay.Enemy;
using GASPT.Core.Pooling;
using GASPT.UI;
using Unity.VisualScripting;
using GASPT.Stats;

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
        private const string UIPrefabsPath = "Assets/Resources/Prefabs/UI";
        private const string TexturesPath = "Assets/Resources/Textures/Placeholders";

        private Vector2 scrollPosition;
        private bool createMageForm = true;
        private bool createProjectiles = true;
        private bool createEffects = true;
        private bool createEnemy = true;
        private bool createUI = true;

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
                "- BasicMeleeEnemy\n" +
                "- BuffIcon (버프 아이콘)\n" +
                "- PickupSlot (아이템 슬롯)",
                MessageType.Info
            );

            GUILayout.Space(20);

            // 프리팹 선택
            EditorGUILayout.LabelField("생성할 프리팹 선택:", EditorStyles.boldLabel);
            createMageForm = EditorGUILayout.Toggle("MageForm (플레이어)", createMageForm);
            createProjectiles = EditorGUILayout.Toggle("Projectiles (투사체)", createProjectiles);
            createEffects = EditorGUILayout.Toggle("VisualEffect (효과)", createEffects);
            createEnemy = EditorGUILayout.Toggle("BasicMeleeEnemy (적)", createEnemy);
            createUI = EditorGUILayout.Toggle("UI Prefabs (버프/아이템)", createUI);

            GUILayout.Space(20);

            // 전체 생성 버튼
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 모든 프리팹 생성", GUILayout.Height(40)))
            {
                EditorApplication.delayCall += CreateAllPrefabs;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 개별 생성 버튼들
            EditorGUILayout.LabelField("개별 생성:", EditorStyles.boldLabel);

            if (GUILayout.Button("MageForm 프리팹 생성"))
            {
                EditorApplication.delayCall += CreateMageFormPrefab;
            }

            if (GUILayout.Button("Projectile 프리팹 생성"))
            {
                EditorApplication.delayCall += CreateProjectilePrefabs;
            }

            if (GUILayout.Button("VisualEffect 프리팹 생성"))
            {
                EditorApplication.delayCall += CreateVisualEffectPrefab;
            }

            if (GUILayout.Button("BasicMeleeEnemy 프리팹 생성"))
            {
                EditorApplication.delayCall += CreateBasicMeleeEnemyPrefab;
            }

            if (GUILayout.Button("UI 프리팹 생성 (BuffIcon)"))
            {
                EditorApplication.delayCall += CreateUIPrefabs;
            }

            GUILayout.Space(20);

            // 폴더 생성 버튼
            if (GUILayout.Button("프리팹 폴더 생성"))
            {
                EditorApplication.delayCall += CreatePrefabFolders;
            }

            GUILayout.Space(10);

            // 정보 표시
            EditorGUILayout.HelpBox(
                $"프리팹 저장 경로:\n" +
                $"Player: {PlayerPrefabsPath}\n" +
                $"Projectiles: {ProjectilesPrefabsPath}\n" +
                $"Effects: {EffectsPrefabsPath}\n" +
                $"Enemies: {EnemiesPrefabsPath}\n" +
                $"UI: {UIPrefabsPath}",
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
                CreateEnemyProjectilePrefab();
                createdCount += 3; // MagicMissile + Fireball + EnemyProjectile
            }

            if (createEffects)
            {
                CreateVisualEffectPrefab();
                createdCount++;
            }

            if (createEnemy)
            {
                CreateBasicMeleeEnemyPrefab();
                CreateRangedEnemyPrefab();
                CreateFlyingEnemyPrefab();
                CreateEliteEnemyPrefab();
                createdCount += 4; // BasicMelee + Ranged + Flying + Elite
            }

            if (createUI)
            {
                CreateUIPrefabs();
                createdCount += 2; // BuffIcon + PickupSlot
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
            CreateFolderIfNotExists(UIPrefabsPath);
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
            mageFormObj.layer = LayerMask.NameToLayer("Player");
            mageFormObj.tag = "Player";

            // 컴포넌트 추가
            Rigidbody2D rb = mageFormObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 3f;

            BoxCollider2D collider = mageFormObj.AddComponent<BoxCollider2D>();

            SpriteRenderer sr = mageFormObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.5f, 0.5f, 1f, 1f)); // 파란색 (Mage)
            sr.color = Color.white; // 스프라이트 색상 유지

            PlayerController playerController = mageFormObj.AddComponent<PlayerController>();
            SerializedObject so = new SerializedObject(playerController);
            so.FindProperty("groundLayer").intValue = LayerMask.GetMask("Ground");
            so.ApplyModifiedProperties();

            FormInputHandler formInputHandler = mageFormObj.AddComponent<FormInputHandler>();
            MageForm mageForm = mageFormObj.AddComponent<MageForm>();

            // GroundCheck 자식 오브젝트 생성
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(mageFormObj.transform);
            groundCheck.transform.localPosition = new Vector3(0f, 0f, 0f); // 발 위치

            PlayerStats playerStats = mageFormObj.AddComponent<PlayerStats>();

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

            // TrailRenderer 추가
            TrailRenderer trail = projectileObj.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.15f;
            trail.endWidth = 0.02f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.cyan;
            trail.endColor = new Color(0f, 1f, 1f, 0f); // 투명한 cyan

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

            // TrailRenderer 추가
            TrailRenderer trail = projectileObj.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(1f, 0.5f, 0f, 1f); // 주황색
            trail.endColor = new Color(1f, 0f, 0f, 0f); // 투명한 빨강

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

            // BasicMeleeEnemy만 추가 (Enemy는 abstract class이므로 직접 추가 불가)
            // BasicMeleeEnemy가 PlatformerEnemy를 상속하고, PlatformerEnemy가 Enemy를 상속함
            BasicMeleeEnemy basicMeleeEnemy = enemyObj.AddComponent<BasicMeleeEnemy>();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);
            DestroyImmediate(enemyObj);

            Debug.Log($"[PrefabCreator] BasicMeleeEnemy 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// RangedEnemy 프리팹 생성
        /// </summary>
        private void CreateRangedEnemyPrefab()
        {
            string prefabPath = $"{EnemiesPrefabsPath}/RangedEnemy.prefab";

            GameObject enemyObj = new GameObject("RangedEnemy");

            // 컴포넌트 추가
            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 3f;

            BoxCollider2D collider = enemyObj.AddComponent<BoxCollider2D>();

            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.5f, 1f, 0.5f, 1f)); // 연두색 (원거리 적)
            sr.color = Color.white;

            // Layer 설정 (Enemy)
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[PrefabCreator] 'Enemy' Layer가 없습니다!");
                enemyObj.layer = 0;
            }
            else
            {
                enemyObj.layer = enemyLayer;
            }

            // FirePoint 자식 오브젝트 생성
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(enemyObj.transform);
            firePointObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);

            PooledObject pooledObject = enemyObj.AddComponent<PooledObject>();
            RangedEnemy rangedEnemy = enemyObj.AddComponent<RangedEnemy>();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);
            DestroyImmediate(enemyObj);

            Debug.Log($"[PrefabCreator] RangedEnemy 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// FlyingEnemy 프리팹 생성
        /// </summary>
        private void CreateFlyingEnemyPrefab()
        {
            string prefabPath = $"{EnemiesPrefabsPath}/FlyingEnemy.prefab";

            GameObject enemyObj = new GameObject("FlyingEnemy");

            // 컴포넌트 추가
            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 0f; // 비행 적은 중력 무시

            CircleCollider2D collider = enemyObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = true; // 급강하 공격 감지용

            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.5f, 0.5f, 1f, 1f)); // 하늘색 (비행 적)
            sr.color = Color.white;

            // Layer 설정 (Enemy)
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[PrefabCreator] 'Enemy' Layer가 없습니다!");
                enemyObj.layer = 0;
            }
            else
            {
                enemyObj.layer = enemyLayer;
            }

            PooledObject pooledObject = enemyObj.AddComponent<PooledObject>();
            FlyingEnemy flyingEnemy = enemyObj.AddComponent<FlyingEnemy>();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);
            DestroyImmediate(enemyObj);

            Debug.Log($"[PrefabCreator] FlyingEnemy 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// EliteEnemy 프리팹 생성
        /// </summary>
        private void CreateEliteEnemyPrefab()
        {
            string prefabPath = $"{EnemiesPrefabsPath}/EliteEnemy.prefab";

            GameObject enemyObj = new GameObject("EliteEnemy");

            // 컴포넌트 추가
            Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 3f;

            BoxCollider2D collider = enemyObj.AddComponent<BoxCollider2D>();

            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(1f, 0.5f, 0f, 1f)); // 주황색 (정예 적)
            sr.color = Color.white;

            // Layer 설정 (Enemy)
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[PrefabCreator] 'Enemy' Layer가 없습니다!");
                enemyObj.layer = 0;
            }
            else
            {
                enemyObj.layer = enemyLayer;
            }

            PooledObject pooledObject = enemyObj.AddComponent<PooledObject>();
            EliteEnemy eliteEnemy = enemyObj.AddComponent<EliteEnemy>();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);
            DestroyImmediate(enemyObj);

            Debug.Log($"[PrefabCreator] EliteEnemy 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// EnemyProjectile 프리팹 생성
        /// </summary>
        private void CreateEnemyProjectilePrefab()
        {
            string prefabPath = $"{ProjectilesPrefabsPath}/EnemyProjectile.prefab";

            GameObject projObj = new GameObject("EnemyProjectile");

            // CircleCollider2D 추가 (Trigger)
            CircleCollider2D collider = projObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.isTrigger = true;

            // SpriteRenderer 추가
            SpriteRenderer sr = projObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(1f, 0.2f, 0.2f, 1f)); // 빨간색 (적 투사체)
            sr.color = Color.white;

            // Layer 설정 (Projectile이나 Default)
            projObj.layer = 0; // Default layer

            // PooledObject 추가
            PooledObject pooledObject = projObj.AddComponent<PooledObject>();

            // EnemyProjectile 추가
            EnemyProjectile enemyProjectile = projObj.AddComponent<EnemyProjectile>();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(projObj, prefabPath);
            DestroyImmediate(projObj);

            Debug.Log($"[PrefabCreator] EnemyProjectile 프리팹 생성 완료: {prefabPath}");
        }

        /// <summary>
        /// UI 프리팹들 생성
        /// </summary>
        private void CreateUIPrefabs()
        {
            CreateBuffIconPrefab();
        }

        /// <summary>
        /// BuffIcon UI 프리팹 생성
        /// </summary>
        private void CreateBuffIconPrefab()
        {
            string prefabPath = $"{UIPrefabsPath}/BuffIcon.prefab";

            GameObject buffIconObj = new GameObject("BuffIcon");

            // RectTransform 추가 (UI 요소)
            RectTransform rectTransform = buffIconObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50f, 50f);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // BorderImage (테두리)
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(buffIconObj.transform);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            borderRect.anchoredPosition = Vector2.zero;
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // 기본 테두리 색

            // Background (배경)
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(buffIconObj.transform);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.05f, 0.05f);
            bgRect.anchorMax = new Vector2(0.95f, 0.95f);
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // 어두운 배경

            // Icon 자식 오브젝트 생성
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(buffIconObj.transform);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // TimerFill (radial fill 타이머)
            GameObject timerObj = new GameObject("TimerFill");
            timerObj.transform.SetParent(buffIconObj.transform);
            RectTransform timerRect = timerObj.AddComponent<RectTransform>();
            timerRect.anchorMin = Vector2.zero;
            timerRect.anchorMax = Vector2.one;
            timerRect.sizeDelta = Vector2.zero;
            timerRect.anchoredPosition = Vector2.zero;
            Image timerImage = timerObj.AddComponent<Image>();
            timerImage.color = new Color(0f, 0f, 0f, 0.5f); // 반투명 검정
            timerImage.type = Image.Type.Filled;
            timerImage.fillMethod = Image.FillMethod.Radial360;
            timerImage.fillOrigin = (int)Image.Origin360.Top;
            timerImage.fillAmount = 1f;
            timerImage.fillClockwise = false;

            // StackCount 텍스트 (우하단)
            GameObject stackTextObj = new GameObject("StackCount");
            stackTextObj.transform.SetParent(buffIconObj.transform);
            RectTransform stackRect = stackTextObj.AddComponent<RectTransform>();
            stackRect.sizeDelta = new Vector2(25f, 18f);
            stackRect.anchorMin = new Vector2(1f, 0f);
            stackRect.anchorMax = new Vector2(1f, 0f);
            stackRect.pivot = new Vector2(1f, 0f);
            stackRect.anchoredPosition = new Vector2(-2f, 2f);
            TMPro.TextMeshProUGUI stackText = stackTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            stackText.text = "1";
            stackText.fontSize = 12f;
            stackText.color = Color.white;
            stackText.alignment = TMPro.TextAlignmentOptions.BottomRight;
            stackText.fontStyle = TMPro.FontStyles.Bold;
            stackText.enableAutoSizing = true;
            stackText.fontSizeMin = 8f;
            stackText.fontSizeMax = 12f;

            // TimeText (중앙 하단)
            GameObject timeTextObj = new GameObject("TimeText");
            timeTextObj.transform.SetParent(buffIconObj.transform);
            RectTransform timeRect = timeTextObj.AddComponent<RectTransform>();
            timeRect.sizeDelta = new Vector2(45f, 15f);
            timeRect.anchorMin = new Vector2(0.5f, 0f);
            timeRect.anchorMax = new Vector2(0.5f, 0f);
            timeRect.pivot = new Vector2(0.5f, 0f);
            timeRect.anchoredPosition = new Vector2(0f, 1f);
            TMPro.TextMeshProUGUI timeText = timeTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            timeText.text = "10s";
            timeText.fontSize = 10f;
            timeText.color = Color.yellow;
            timeText.alignment = TMPro.TextAlignmentOptions.Bottom;
            timeText.fontStyle = TMPro.FontStyles.Bold;
            timeText.enableAutoSizing = true;
            timeText.fontSizeMin = 6f;
            timeText.fontSizeMax = 10f;

            // BuffIcon 컴포넌트 추가 및 참조 설정
            BuffIcon buffIcon = buffIconObj.AddComponent<BuffIcon>();
            SerializedObject so = new SerializedObject(buffIcon);
            so.FindProperty("iconImage").objectReferenceValue = iconImage;
            so.FindProperty("timerFillImage").objectReferenceValue = timerImage;
            so.FindProperty("stackText").objectReferenceValue = stackText;
            so.FindProperty("timeText").objectReferenceValue = timeText;
            so.FindProperty("borderImage").objectReferenceValue = borderImage;
            so.ApplyModifiedProperties();

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(buffIconObj, prefabPath);
            DestroyImmediate(buffIconObj);

            Debug.Log($"[PrefabCreator] BuffIcon 프리팹 생성 완료: {prefabPath}");
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
