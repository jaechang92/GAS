#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GASPT.Core.Enums;

namespace GASPT.Editor
{
    /// <summary>
    /// 상태 효과 VFX 프리팹 자동 생성 에디터 도구
    /// </summary>
    public class StatusEffectVFXGenerator : EditorWindow
    {
        // ====== 설정 ======

        private const string VFX_OUTPUT_PATH = "Assets/_Project/Prefabs/VFX/StatusEffects";
        private const string CONFIG_OUTPUT_PATH = "Assets/_Project/Resources/Data";

        private bool generateBurn = true;
        private bool generateFreeze = true;
        private bool generateSlow = true;
        private bool generatePoison = true;
        private bool generateBleed = true;
        private bool generateStun = true;


        // ====== 메뉴 ======

        [MenuItem("GASPT/VFX Generator/Status Effect VFX")]
        public static void ShowWindow()
        {
            GetWindow<StatusEffectVFXGenerator>("VFX Generator");
        }


        // ====== GUI ======

        private void OnGUI()
        {
            GUILayout.Label("상태 효과 VFX 생성기", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GUILayout.Label("생성할 효과 선택", EditorStyles.boldLabel);
            generateBurn = EditorGUILayout.Toggle("화상 (Burn)", generateBurn);
            generateFreeze = EditorGUILayout.Toggle("빙결 (Freeze)", generateFreeze);
            generateSlow = EditorGUILayout.Toggle("감속 (Slow)", generateSlow);
            generatePoison = EditorGUILayout.Toggle("독 (Poison)", generatePoison);
            generateBleed = EditorGUILayout.Toggle("출혈 (Bleed)", generateBleed);
            generateStun = EditorGUILayout.Toggle("기절 (Stun)", generateStun);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "VFX 프리팹이 생성되면 StatusEffectVisualConfig에 수동 연결이 필요합니다.\n" +
                "경로: Assets/_Project/Prefabs/VFX/StatusEffects/",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("선택한 VFX 프리팹 생성", GUILayout.Height(30)))
            {
                GenerateSelectedVFX();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("모든 VFX 프리팹 생성 (일괄)", GUILayout.Height(30)))
            {
                GenerateAllVFX();
            }
        }


        // ====== VFX 생성 ======

        private void GenerateSelectedVFX()
        {
            EnsureFolderExists(VFX_OUTPUT_PATH);

            int created = 0;

            if (generateBurn) { CreateBurnVFX(); created++; }
            if (generateFreeze) { CreateFreezeVFX(); created++; }
            if (generateSlow) { CreateSlowVFX(); created++; }
            if (generatePoison) { CreatePoisonVFX(); created++; }
            if (generateBleed) { CreateBleedVFX(); created++; }
            if (generateStun) { CreateStunVFX(); created++; }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VFXGenerator] {created}개 VFX 프리팹 생성 완료");
            EditorUtility.DisplayDialog("완료", $"{created}개 VFX 프리팹이 생성되었습니다.", "확인");
        }

        [MenuItem("GASPT/VFX Generator/Generate All Status VFX")]
        public static void GenerateAllVFX()
        {
            EnsureFolderExists(VFX_OUTPUT_PATH);

            CreateBurnVFX();
            CreateFreezeVFX();
            CreateSlowVFX();
            CreatePoisonVFX();
            CreateBleedVFX();
            CreateStunVFX();
            CreateDefaultVFX();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[VFXGenerator] 모든 VFX 프리팹 생성 완료!");
        }


        // ====== 개별 VFX 생성 ======

        private static void CreateBurnVFX()
        {
            string path = $"{VFX_OUTPUT_PATH}/BurnVFX.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[VFXGenerator] BurnVFX 이미 존재");
                return;
            }

            GameObject vfx = new GameObject("BurnVFX");
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();

            // Main 설정
            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.25f);
            main.startColor = new Color(1f, 0.42f, 0f, 1f); // 주황색
            main.gravityModifier = -0.2f;
            main.maxParticles = 30;

            // Emission
            var emission = ps.emission;
            emission.rateOverTime = 8f;

            // Shape
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            // Color over Lifetime
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.42f, 0f), 0f),
                    new GradientColorKey(new Color(1f, 0f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = gradient;

            // Size over Lifetime
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0.3f)
            ));

            // Renderer
            var renderer = vfx.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            renderer.material = GetDefaultParticleMaterial();

            SavePrefab(vfx, path);
            Debug.Log("[VFXGenerator] BurnVFX 생성 완료");
        }

        private static void CreateFreezeVFX()
        {
            string path = $"{VFX_OUTPUT_PATH}/FreezeVFX.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[VFXGenerator] FreezeVFX 이미 존재");
                return;
            }

            GameObject vfx = new GameObject("FreezeVFX");
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = new Color(0f, 0.75f, 1f, 1f); // 하늘색
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 20;

            var emission = ps.emission;
            emission.rateOverTime = 5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            // Velocity over Lifetime (회전)
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.orbitalY = 0.5f;
            vel.radial = 0.1f;

            // Color over Lifetime (페이드 인/아웃)
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0f, 0.75f, 1f), 0.5f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.3f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = gradient;

            var renderer = vfx.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            renderer.material = GetDefaultParticleMaterial();

            SavePrefab(vfx, path);
            Debug.Log("[VFXGenerator] FreezeVFX 생성 완료");
        }

        private static void CreateSlowVFX()
        {
            string path = $"{VFX_OUTPUT_PATH}/SlowVFX.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[VFXGenerator] SlowVFX 이미 존재");
                return;
            }

            GameObject vfx = new GameObject("SlowVFX");
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.25f);
            main.startColor = new Color(0.55f, 0f, 1f, 1f); // 보라색
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 15;

            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            // Rotation over Lifetime
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = 45f * Mathf.Deg2Rad;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.55f, 0f, 1f), 0.5f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.8f, 0.3f),
                    new GradientAlphaKey(0.8f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = gradient;

            var renderer = vfx.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            renderer.material = GetDefaultParticleMaterial();

            SavePrefab(vfx, path);
            Debug.Log("[VFXGenerator] SlowVFX 생성 완료");
        }

        private static void CreatePoisonVFX()
        {
            string path = $"{VFX_OUTPUT_PATH}/PoisonVFX.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[VFXGenerator] PoisonVFX 이미 존재");
                return;
            }

            GameObject vfx = new GameObject("PoisonVFX");
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = new Color(0.6f, 0.2f, 0.8f, 1f); // 진보라
            main.gravityModifier = -0.15f;
            main.maxParticles = 25;

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            // Noise
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.2f;
            noise.frequency = 1.5f;
            noise.scrollSpeed = 0.5f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.6f, 0.2f, 0.8f), 0.5f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = gradient;

            var renderer = vfx.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            renderer.material = GetDefaultParticleMaterial();

            SavePrefab(vfx, path);
            Debug.Log("[VFXGenerator] PoisonVFX 생성 완료");
        }

        private static void CreateBleedVFX()
        {
            string path = $"{VFX_OUTPUT_PATH}/BleedVFX.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[VFXGenerator] BleedVFX 이미 존재");
                return;
            }

            GameObject vfx = new GameObject("BleedVFX");
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.12f);
            main.startColor = new Color(0.86f, 0.08f, 0.24f, 1f); // 진홍
            main.gravityModifier = 1f;
            main.maxParticles = 40;

            var emission = ps.emission;
            emission.rateOverTime = 10f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.86f, 0.08f, 0.24f), 0.5f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = gradient;

            var renderer = vfx.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            renderer.material = GetDefaultParticleMaterial();

            SavePrefab(vfx, path);
            Debug.Log("[VFXGenerator] BleedVFX 생성 완료");
        }

        private static void CreateStunVFX()
        {
            string path = $"{VFX_OUTPUT_PATH}/StunVFX.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[VFXGenerator] StunVFX 이미 존재");
                return;
            }

            GameObject vfx = new GameObject("StunVFX");

            // 머리 위 위치 조정
            vfx.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 2f;
            main.startSpeed = 0f;
            main.startSize = 0.15f;
            main.startColor = new Color(1f, 0.84f, 0f, 1f); // 금색
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 10;

            var emission = ps.emission;
            emission.rateOverTime = 3f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;
            shape.arc = 360f;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Loop;
            shape.arcSpeed = 1f;

            // Velocity over Lifetime (회전)
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.orbitalY = 2f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.84f, 0f), 0.5f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            col.color = gradient;

            var renderer = vfx.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            renderer.material = GetDefaultParticleMaterial();

            SavePrefab(vfx, path);
            Debug.Log("[VFXGenerator] StunVFX 생성 완료");
        }

        private static void CreateDefaultVFX()
        {
            string path = $"{VFX_OUTPUT_PATH}/DefaultStatusVFX.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[VFXGenerator] DefaultStatusVFX 이미 존재");
                return;
            }

            GameObject vfx = new GameObject("DefaultStatusVFX");
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 1f;
            main.startSpeed = 0.2f;
            main.startSize = 0.15f;
            main.startColor = Color.white;
            main.maxParticles = 15;

            var emission = ps.emission;
            emission.rateOverTime = 5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var renderer = vfx.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 10;
            renderer.material = GetDefaultParticleMaterial();

            SavePrefab(vfx, path);
            Debug.Log("[VFXGenerator] DefaultStatusVFX 생성 완료");
        }


        // ====== 유틸리티 ======

        private static Material GetDefaultParticleMaterial()
        {
            // 기본 파티클 머티리얼 찾기 또는 생성
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/DefaultParticle.mat");

            if (mat == null)
            {
                // 기본 Sprite-Default 사용
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    mat = new Material(shader);
                    mat.SetColor("_Color", Color.white);

                    EnsureFolderExists("Assets/_Project/Materials");
                    AssetDatabase.CreateAsset(mat, "Assets/_Project/Materials/DefaultParticle.mat");
                    Debug.Log("[VFXGenerator] DefaultParticle.mat 생성 완료");
                }
            }

            return mat;
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
        }

        private static void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = "";

            for (int i = 0; i < folders.Length; i++)
            {
                string parentPath = currentPath;
                currentPath = string.IsNullOrEmpty(currentPath)
                    ? folders[i]
                    : $"{currentPath}/{folders[i]}";

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        parentPath = "Assets";
                        currentPath = $"Assets/{folders[i]}";
                    }

                    AssetDatabase.CreateFolder(parentPath, folders[i]);
                }
            }
        }
    }
}
#endif
