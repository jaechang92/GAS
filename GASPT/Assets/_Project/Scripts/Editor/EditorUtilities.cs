using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace GASPT.Editor
{
    /// <summary>
    /// Editor Creator 클래스들을 위한 공통 유틸리티
    ///
    /// 주요 기능:
    /// - Canvas 찾기/생성
    /// - 프리팹 저장
    /// - TextMeshPro UI 생성
    /// - SerializedProperty 할당
    ///
    /// 작성일: 2025-11-16
    /// 목적: Editor Creator 파일들의 중복 코드 제거 (300-400줄 절감)
    /// </summary>
    public static class EditorUtilities
    {
        #region Canvas 관련

        /// <summary>
        /// 씬에서 Canvas를 찾거나, 없으면 Screen Space Overlay Canvas를 생성합니다.
        /// </summary>
        /// <param name="logPrefix">디버그 로그에 표시할 접두사 (예: "[ShopUICreator]")</param>
        /// <returns>찾거나 생성한 Canvas</returns>
        public static Canvas FindOrCreateCanvas(string logPrefix = "[EditorUtilities]")
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.Log($"{logPrefix} Screen Space Overlay Canvas를 찾을 수 없어 새로 생성합니다.");

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

                Debug.Log($"{logPrefix} Canvas 생성 완료");
            }
            else
            {
                Debug.Log($"{logPrefix} 기존 Canvas를 사용합니다.");
            }

            return canvas;
        }

        #endregion

        #region 프리팹 관련

        /// <summary>
        /// GameObject를 프리팹으로 저장합니다.
        /// 디렉토리가 없으면 자동으로 생성합니다.
        /// </summary>
        /// <param name="gameObject">저장할 GameObject</param>
        /// <param name="prefabPath">프리팹 저장 경로 (예: "Assets/Prefabs/MyPrefab.prefab")</param>
        /// <param name="logPrefix">디버그 로그에 표시할 접두사</param>
        public static void SaveAsPrefab(GameObject gameObject, string prefabPath, string logPrefix = "[EditorUtilities]")
        {
            // 프리팹 디렉토리 생성 (없으면)
            string directory = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"{logPrefix} 디렉토리 생성: {directory}");
            }

            // 기존 프리팹이 있으면 경고
            if (File.Exists(prefabPath))
            {
                Debug.LogWarning($"{logPrefix} 기존 프리팹이 있습니다. 덮어씁니다: {prefabPath}");
            }

            // 프리팹으로 저장
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);

            // AssetDatabase 새로고침
            AssetDatabase.Refresh();

            Debug.Log($"{logPrefix} 프리팹 저장 완료: {prefabPath}");
        }

        #endregion

        #region UI 생성 관련

        /// <summary>
        /// TextMeshProUGUI 컴포넌트를 생성합니다.
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <param name="name">GameObject 이름</param>
        /// <param name="text">초기 텍스트</param>
        /// <param name="fontSize">폰트 크기</param>
        /// <param name="color">텍스트 색상</param>
        /// <param name="alignment">텍스트 정렬</param>
        /// <returns>생성된 TextMeshProUGUI 컴포넌트</returns>
        public static TextMeshProUGUI CreateTextMeshPro(
            Transform parent,
            string name,
            string text = "",
            float fontSize = 24f,
            Color? color = null,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.color = color ?? Color.white;
            tmpText.alignment = alignment;

            return tmpText;
        }

        /// <summary>
        /// Image 컴포넌트를 생성합니다.
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <param name="name">GameObject 이름</param>
        /// <param name="color">이미지 색상</param>
        /// <returns>생성된 Image 컴포넌트</returns>
        public static Image CreateImage(Transform parent, string name, Color? color = null)
        {
            GameObject imageObj = new GameObject(name);
            imageObj.transform.SetParent(parent, false);

            Image image = imageObj.AddComponent<Image>();
            image.color = color ?? Color.white;

            return image;
        }

        /// <summary>
        /// RectTransform을 설정합니다.
        /// </summary>
        /// <param name="rectTransform">설정할 RectTransform</param>
        /// <param name="anchorMin">앵커 최소값</param>
        /// <param name="anchorMax">앵커 최대값</param>
        /// <param name="pivot">피벗</param>
        /// <param name="sizeDelta">크기</param>
        /// <param name="anchoredPosition">앵커 위치</param>
        public static void SetRectTransform(
            RectTransform rectTransform,
            Vector2? anchorMin = null,
            Vector2? anchorMax = null,
            Vector2? pivot = null,
            Vector2? sizeDelta = null,
            Vector2? anchoredPosition = null)
        {
            if (anchorMin.HasValue)
                rectTransform.anchorMin = anchorMin.Value;
            if (anchorMax.HasValue)
                rectTransform.anchorMax = anchorMax.Value;
            if (pivot.HasValue)
                rectTransform.pivot = pivot.Value;
            if (sizeDelta.HasValue)
                rectTransform.sizeDelta = sizeDelta.Value;
            if (anchoredPosition.HasValue)
                rectTransform.anchoredPosition = anchoredPosition.Value;
        }

        #endregion

        #region SerializedProperty 관련

        /// <summary>
        /// SerializedProperty에 Object를 할당합니다.
        /// </summary>
        /// <param name="serializedObject">SerializedObject</param>
        /// <param name="propertyName">프로퍼티 이름</param>
        /// <param name="value">할당할 값</param>
        /// <param name="logPrefix">디버그 로그에 표시할 접두사</param>
        public static void AssignSerializedProperty(
            SerializedObject serializedObject,
            string propertyName,
            Object value,
            string logPrefix = "[EditorUtilities]")
        {
            SerializedProperty prop = serializedObject.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                Debug.Log($"{logPrefix} SerializedProperty 할당: {propertyName} = {value?.name ?? "null"}");
            }
            else
            {
                Debug.LogError($"{logPrefix} SerializedProperty를 찾을 수 없습니다: {propertyName}");
            }
        }

        /// <summary>
        /// SerializedProperty에 배열 요소를 할당합니다.
        /// </summary>
        /// <param name="serializedObject">SerializedObject</param>
        /// <param name="arrayPropertyName">배열 프로퍼티 이름</param>
        /// <param name="values">할당할 값 배열</param>
        /// <param name="logPrefix">디버그 로그에 표시할 접두사</param>
        public static void AssignSerializedPropertyArray(
            SerializedObject serializedObject,
            string arrayPropertyName,
            Object[] values,
            string logPrefix = "[EditorUtilities]")
        {
            SerializedProperty arrayProp = serializedObject.FindProperty(arrayPropertyName);
            if (arrayProp != null && arrayProp.isArray)
            {
                arrayProp.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++)
                {
                    SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(i);
                    elementProp.objectReferenceValue = values[i];
                }
                Debug.Log($"{logPrefix} SerializedProperty 배열 할당: {arrayPropertyName} (크기: {values.Length})");
            }
            else
            {
                Debug.LogError($"{logPrefix} SerializedProperty 배열을 찾을 수 없습니다: {arrayPropertyName}");
            }
        }

        #endregion

        #region 에셋 관련

        /// <summary>
        /// ScriptableObject 에셋을 생성합니다.
        /// </summary>
        /// <typeparam name="T">ScriptableObject 타입</typeparam>
        /// <param name="assetPath">에셋 저장 경로</param>
        /// <param name="logPrefix">디버그 로그에 표시할 접두사</param>
        /// <returns>생성된 에셋</returns>
        public static T CreateScriptableObjectAsset<T>(string assetPath, string logPrefix = "[EditorUtilities]") where T : ScriptableObject
        {
            // 디렉토리 생성 (없으면)
            string directory = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"{logPrefix} 디렉토리 생성: {directory}");
            }

            // ScriptableObject 인스턴스 생성
            T asset = ScriptableObject.CreateInstance<T>();

            // 에셋으로 저장
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{logPrefix} ScriptableObject 에셋 생성 완료: {assetPath}");

            return asset;
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// GameObject가 프리팹인지 확인합니다.
        /// </summary>
        public static bool IsPrefab(GameObject gameObject)
        {
            return PrefabUtility.IsPartOfPrefabAsset(gameObject);
        }

        /// <summary>
        /// 경로가 유효한지 확인합니다 (Assets 폴더 내부).
        /// </summary>
        public static bool IsValidAssetPath(string path)
        {
            return path.StartsWith("Assets/") && (path.EndsWith(".prefab") || path.EndsWith(".asset"));
        }

        #endregion
    }
}
