using UnityEngine;
using UnityEditor;
using Core.Data;
using Core.Enums;
using System.IO;

namespace Editor
{
    /// <summary>
    /// ResourceManifest를 자동으로 생성하는 에디터 도구
    /// </summary>
    public class ResourceManifestCreator
    {
        private const string MANIFEST_PATH = "Assets/_Project/Resources/Manifests";

        [MenuItem("GASPT/Resources/Create All Manifests")]
        public static void CreateAllManifests()
        {
            // 폴더 확인 및 생성
            if (!Directory.Exists(MANIFEST_PATH))
            {
                Directory.CreateDirectory(MANIFEST_PATH);
                AssetDatabase.Refresh();
            }

            CreateEssentialManifest();
            CreateMainMenuManifest();
            CreateGameplayManifest();
            CreateCommonManifest();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ResourceManifestCreator] 모든 매니페스트 생성 완료!");
        }

        [MenuItem("GASPT/Resources/Create Essential Manifest")]
        public static void CreateEssentialManifest()
        {
            string path = $"{MANIFEST_PATH}/EssentialManifest.asset";

            // 이미 존재하면 로드
            ResourceManifest manifest = AssetDatabase.LoadAssetAtPath<ResourceManifest>(path);

            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<ResourceManifest>();
                AssetDatabase.CreateAsset(manifest, path);
            }

            manifest.category = ResourceCategory.Essential;
            manifest.displayName = "필수 리소스";
            manifest.useAsyncLoading = true;
            manifest.minimumLoadTime = 0.5f;

            // 리소스 초기화
            manifest.resources.Clear();

            // SkulPhysicsConfig 추가
            manifest.AddResource(
                "Data/SkulPhysicsConfig",
                "Player.Physics.SkulPhysicsConfig",
                "플레이어 물리 설정"
            );

            EditorUtility.SetDirty(manifest);
            Debug.Log($"[ResourceManifestCreator] Essential 매니페스트 생성 완료: {path}");
        }

        [MenuItem("GASPT/Resources/Create MainMenu Manifest")]
        public static void CreateMainMenuManifest()
        {
            string path = $"{MANIFEST_PATH}/MainMenuManifest.asset";

            ResourceManifest manifest = AssetDatabase.LoadAssetAtPath<ResourceManifest>(path);

            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<ResourceManifest>();
                AssetDatabase.CreateAsset(manifest, path);
            }

            manifest.category = ResourceCategory.MainMenu;
            manifest.displayName = "메인 메뉴 리소스";
            manifest.useAsyncLoading = true;
            manifest.minimumLoadTime = 0.5f;

            // 리소스 초기화
            manifest.resources.Clear();

            // 예제: 메인 메뉴 리소스 (향후 추가 예정)
            // manifest.AddResource("UI/MainMenuPanel", "GameObject", "메인 메뉴 패널");
            // manifest.AddResource("Audio/Music/MainMenuBGM", "AudioClip", "메인 메뉴 배경음악");

            EditorUtility.SetDirty(manifest);
            Debug.Log($"[ResourceManifestCreator] MainMenu 매니페스트 생성 완료: {path}");
        }

        [MenuItem("GASPT/Resources/Create Gameplay Manifest")]
        public static void CreateGameplayManifest()
        {
            string path = $"{MANIFEST_PATH}/GameplayManifest.asset";

            ResourceManifest manifest = AssetDatabase.LoadAssetAtPath<ResourceManifest>(path);

            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<ResourceManifest>();
                AssetDatabase.CreateAsset(manifest, path);
            }

            manifest.category = ResourceCategory.Gameplay;
            manifest.displayName = "게임플레이 리소스";
            manifest.useAsyncLoading = true;
            manifest.minimumLoadTime = 1.0f;

            // 리소스 초기화
            manifest.resources.Clear();

            // 예제: 게임플레이 리소스 (향후 추가 예정)
            // manifest.AddResource("Prefabs/Player", "GameObject", "플레이어 프리팹");
            // manifest.AddResource("Prefabs/Enemies/BasicEnemy", "GameObject", "기본 적 프리팹");
            // manifest.AddResource("Audio/SFX/Jump", "AudioClip", "점프 효과음");

            EditorUtility.SetDirty(manifest);
            Debug.Log($"[ResourceManifestCreator] Gameplay 매니페스트 생성 완료: {path}");
        }

        [MenuItem("GASPT/Resources/Create Common Manifest")]
        public static void CreateCommonManifest()
        {
            string path = $"{MANIFEST_PATH}/CommonManifest.asset";

            ResourceManifest manifest = AssetDatabase.LoadAssetAtPath<ResourceManifest>(path);

            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<ResourceManifest>();
                AssetDatabase.CreateAsset(manifest, path);
            }

            manifest.category = ResourceCategory.Common;
            manifest.displayName = "공통 리소스";
            manifest.useAsyncLoading = true;
            manifest.minimumLoadTime = 0.3f;

            // 리소스 초기화
            manifest.resources.Clear();

            // 예제: 공통 리소스 (향후 추가 예정)
            // manifest.AddResource("VFX/HitEffect", "GameObject", "피격 이펙트");
            // manifest.AddResource("Audio/SFX/Click", "AudioClip", "클릭 효과음");

            EditorUtility.SetDirty(manifest);
            Debug.Log($"[ResourceManifestCreator] Common 매니페스트 생성 완료: {path}");
        }

        [MenuItem("GASPT/Resources/Delete All Manifests")]
        public static void DeleteAllManifests()
        {
            if (EditorUtility.DisplayDialog(
                "매니페스트 삭제",
                "모든 ResourceManifest를 삭제하시겠습니까?",
                "삭제",
                "취소"))
            {
                string[] manifests = new string[]
                {
                    $"{MANIFEST_PATH}/EssentialManifest.asset",
                    $"{MANIFEST_PATH}/MainMenuManifest.asset",
                    $"{MANIFEST_PATH}/GameplayManifest.asset",
                    $"{MANIFEST_PATH}/CommonManifest.asset"
                };

                foreach (string path in manifests)
                {
                    if (File.Exists(path))
                    {
                        AssetDatabase.DeleteAsset(path);
                        Debug.Log($"[ResourceManifestCreator] 삭제됨: {path}");
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log("[ResourceManifestCreator] 모든 매니페스트 삭제 완료!");
            }
        }
    }
}
