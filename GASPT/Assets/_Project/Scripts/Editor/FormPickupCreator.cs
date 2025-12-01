using UnityEngine;
using UnityEditor;
using GASPT.Forms;

namespace GASPT.Editor
{
    /// <summary>
    /// FormPickup 프리팹 및 테스트 오브젝트 생성 도구
    /// </summary>
    public class FormPickupCreator
    {
        private const string PREFAB_PATH = "Assets/Resources/Prefabs/Forms/";

        [MenuItem("Tools/GASPT/Forms/Create FormPickup Prefab", false, 110)]
        public static void CreateFormPickupPrefab()
        {
            EnsureFolder();

            // 기본 오브젝트 생성
            var pickupObj = new GameObject("FormPickup");

            // SpriteRenderer 추가
            var sr = pickupObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.color = Color.white;

            // Collider 추가
            var collider = pickupObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.75f;
            collider.isTrigger = true;

            // FormPickup 컴포넌트 추가
            pickupObj.AddComponent<FormPickup>();

            // 하이라이트 이펙트 자식 오브젝트
            var highlightObj = new GameObject("HighlightEffect");
            highlightObj.transform.SetParent(pickupObj.transform);
            highlightObj.transform.localPosition = Vector3.zero;
            var highlightSr = highlightObj.AddComponent<SpriteRenderer>();
            highlightSr.sortingOrder = 9;
            highlightSr.color = new Color(1f, 1f, 0.5f, 0.3f);
            highlightObj.SetActive(false);

            // 플로팅 이펙트 자식 오브젝트
            var floatingObj = new GameObject("FloatingEffect");
            floatingObj.transform.SetParent(pickupObj.transform);
            floatingObj.transform.localPosition = Vector3.up * 0.5f;

            // 프리팹으로 저장
            string prefabPath = PREFAB_PATH + "FormPickup.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (existingPrefab != null)
            {
                PrefabUtility.SaveAsPrefabAsset(pickupObj, prefabPath);
                Debug.Log($"[FormPickupCreator] 프리팹 업데이트: {prefabPath}");
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(pickupObj, prefabPath);
                Debug.Log($"[FormPickupCreator] 프리팹 생성: {prefabPath}");
            }

            // 씬에서 제거
            Object.DestroyImmediate(pickupObj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", $"FormPickup 프리팹이 생성되었습니다.\n\n경로: {prefabPath}", "확인");
        }

        [MenuItem("Tools/GASPT/Forms/Spawn Test FormPickup", false, 111)]
        public static void SpawnTestFormPickup()
        {
            // 폼 데이터 로드
            var forms = Resources.LoadAll<FormData>("Data/Forms");

            if (forms == null || forms.Length == 0)
            {
                EditorUtility.DisplayDialog("오류", "폼 데이터가 없습니다.\n먼저 'Create Default Forms'를 실행하세요.", "확인");
                return;
            }

            // 랜덤 폼 선택
            var randomForm = forms[Random.Range(0, forms.Length)];

            // Scene View 중심에 생성
            Vector3 spawnPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                spawnPos = SceneView.lastActiveSceneView.pivot;
                spawnPos.z = 0;
            }

            // 프리팹 로드 시도
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH + "FormPickup.prefab");
            GameObject pickupObj;

            if (prefab != null)
            {
                pickupObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                pickupObj.transform.position = spawnPos;
            }
            else
            {
                // 프리팹 없으면 직접 생성
                pickupObj = new GameObject($"FormPickup_{randomForm.formName}");
                pickupObj.transform.position = spawnPos;

                var sr = pickupObj.AddComponent<SpriteRenderer>();
                sr.sprite = randomForm.icon;
                sr.sortingOrder = 10;

                var collider = pickupObj.AddComponent<CircleCollider2D>();
                collider.radius = 0.5f;
                collider.isTrigger = true;

                pickupObj.AddComponent<FormPickup>();
            }

            // 폼 데이터 설정
            var pickup = pickupObj.GetComponent<FormPickup>();
            pickup.Initialize(randomForm);

            // 선택
            Selection.activeGameObject = pickupObj;

            Debug.Log($"[FormPickupCreator] 테스트 픽업 생성: {randomForm.formName} at {spawnPos}");
        }

        [MenuItem("Tools/GASPT/Forms/Spawn All Form Pickups (Row)", false, 112)]
        public static void SpawnAllFormPickupsInRow()
        {
            var forms = Resources.LoadAll<FormData>("Data/Forms");

            if (forms == null || forms.Length == 0)
            {
                EditorUtility.DisplayDialog("오류", "폼 데이터가 없습니다.\n먼저 'Create Default Forms'를 실행하세요.", "확인");
                return;
            }

            // Scene View 중심 기준
            Vector3 startPos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                startPos = SceneView.lastActiveSceneView.pivot;
                startPos.z = 0;
            }

            // 가로로 일렬 배치
            float spacing = 2f;
            float startX = startPos.x - (forms.Length - 1) * spacing / 2f;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH + "FormPickup.prefab");

            for (int i = 0; i < forms.Length; i++)
            {
                Vector3 pos = new Vector3(startX + i * spacing, startPos.y, 0);
                GameObject pickupObj;

                if (prefab != null)
                {
                    pickupObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    pickupObj.transform.position = pos;
                }
                else
                {
                    pickupObj = new GameObject($"FormPickup_{forms[i].formName}");
                    pickupObj.transform.position = pos;

                    var sr = pickupObj.AddComponent<SpriteRenderer>();
                    sr.sprite = forms[i].icon;
                    sr.sortingOrder = 10;

                    var collider = pickupObj.AddComponent<CircleCollider2D>();
                    collider.radius = 0.5f;
                    collider.isTrigger = true;

                    pickupObj.AddComponent<FormPickup>();
                }

                var pickup = pickupObj.GetComponent<FormPickup>();
                pickup.Initialize(forms[i]);
            }

            Debug.Log($"[FormPickupCreator] 모든 폼 픽업 생성 완료: {forms.Length}개");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs"))
                AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs/Forms"))
                AssetDatabase.CreateFolder("Assets/Resources/Prefabs", "Forms");
        }
    }
}
