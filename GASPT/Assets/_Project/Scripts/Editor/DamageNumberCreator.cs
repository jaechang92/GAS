using UnityEngine;
using UnityEditor;
using TMPro;
using GASPT.UI;

namespace GASPT.Editor
{
    /// <summary>
    /// DamageNumber 프리팹 자동 생성 에디터 도구
    /// Tools > GASPT > Create DamageNumber Prefab
    /// Canvas는 DamageNumberPool에서 공용으로 관리 (성능 최적화)
    /// </summary>
    public class DamageNumberCreator : EditorWindow
    {
        [MenuItem("Tools/GASPT/Create DamageNumber Prefab")]
        public static void CreateDamageNumberPrefab()
        {
            // 1. DamageNumber GameObject 생성 (Canvas 없이)
            GameObject damageNumberObj = CreateDamageNumberObject();

            // 2. 프리팹으로 저장
            string prefabPath = "Assets/_Project/Prefabs/UI/DamageNumber.prefab";

            // 디렉토리 확인 및 생성
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // 기존 프리팹이 있으면 덮어쓰기 확인
            if (System.IO.File.Exists(prefabPath))
            {
                if (!EditorUtility.DisplayDialog(
                    "프리팹 덮어쓰기",
                    "DamageNumber.prefab이 이미 존재합니다. 덮어쓰시겠습니까?",
                    "덮어쓰기",
                    "취소"))
                {
                    DestroyImmediate(damageNumberObj);
                    return;
                }
            }

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(damageNumberObj, prefabPath);

            // 생성한 GameObject 삭제
            DestroyImmediate(damageNumberObj);

            // 완료 메시지
            Debug.Log($"[DamageNumberCreator] DamageNumber 프리팹 생성 완료: {prefabPath}");
            EditorUtility.DisplayDialog(
                "DamageNumber 프리팹 생성 완료",
                $"DamageNumber 프리팹이 생성되었습니다!\n\n" +
                $"경로: {prefabPath}\n\n" +
                $"성능 최적화: Canvas는 DamageNumberPool에서 공용으로 관리됩니다.\n" +
                $"DamageNumberPool에서 이 프리팹을 할당하세요.",
                "확인"
            );

            // 생성된 프리팹 선택
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }


        // ====== DamageNumber 오브젝트 생성 (Canvas 없이) ======

        private static GameObject CreateDamageNumberObject()
        {
            // 루트 GameObject (RectTransform만)
            GameObject root = new GameObject("DamageNumber");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200f, 50f);

            // TextMeshPro 생성
            GameObject textObj = new GameObject("DamageText");
            textObj.transform.SetParent(root.transform, false);

            TextMeshProUGUI damageText = textObj.AddComponent<TextMeshProUGUI>();
            damageText.text = "100";
            damageText.fontSize = 36;
            damageText.color = Color.white;
            damageText.alignment = TextAlignmentOptions.Center;
            damageText.fontStyle = FontStyles.Bold;

            // RectTransform 설정
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Outline 추가 (가독성 향상)
            var outline = textObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            // DamageNumber 스크립트 추가
            DamageNumber damageNumberScript = root.AddComponent<DamageNumber>();

            // Reflection으로 private 필드 설정
            var damageTextField = typeof(DamageNumber).GetField("damageText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (damageTextField != null)
            {
                damageTextField.SetValue(damageNumberScript, damageText);
            }

            // EditorUtility로 변경사항 저장
            EditorUtility.SetDirty(damageNumberScript);

            return root;
        }
    }
}
