using UnityEngine;
using UnityEditor;
using System.Linq;

namespace GUIDValidation
{
    /// <summary>
    /// GUID 검증 도구 에디터 윈도우
    /// Unity 메뉴에서 접근 가능한 독립적인 검증 도구 UI
    /// </summary>
    public class GUIDValidatorWindow : EditorWindow
    {
        #region 필드

        private GUIDValidator.ValidationResult lastResult;
        private bool isScanning = false;
        private bool includePackages = false;
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabNames = { "검증 결과", "중복 GUID", "손상된 참조", "고아 메타파일" };

        // UI 스타일
        private GUIStyle headerStyle;
        private GUIStyle errorStyle;
        private GUIStyle warningStyle;
        private GUIStyle successStyle;

        #endregion

        #region 메뉴 등록

        [MenuItem("Tools/GUID Validator", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<GUIDValidatorWindow>("GUID Validator");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        #endregion

        #region Unity 콜백

        void OnEnable()
        {
            InitializeStyles();
        }

        void OnGUI()
        {
            InitializeStyles(); // 매번 체크 (도메인 리로드 대응)

            DrawHeader();
            DrawScanControls();

            if (lastResult != null)
            {
                DrawResultTabs();
                DrawTabContent();
            }
            else
            {
                DrawEmptyState();
            }
        }

        #endregion

        #region UI 그리기

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (errorStyle == null)
            {
                errorStyle = new GUIStyle(EditorStyles.helpBox);
                errorStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
            }

            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(EditorStyles.helpBox);
                warningStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
            }

            if (successStyle == null)
            {
                successStyle = new GUIStyle(EditorStyles.helpBox);
                successStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unity GUID 검증 도구", headerStyle);
            EditorGUILayout.LabelField("Git 작업 중 발생하는 GUID 문제를 탐지하고 수정합니다", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
        }

        private void DrawScanControls()
        {
            EditorGUILayout.BeginHorizontal();

            // 스캔 옵션
            includePackages = EditorGUILayout.Toggle("패키지 포함", includePackages);

            GUILayout.FlexibleSpace();

            // 스캔 버튼
            GUI.enabled = !isScanning;
            if (GUILayout.Button(isScanning ? "스캔 중..." : "GUID 검증 시작", GUILayout.Width(120)))
            {
                StartValidation();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawResultTabs()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space();
        }

        private void DrawTabContent()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0: DrawOverviewTab(); break;
                case 1: DrawDuplicateGuidsTab(); break;
                case 2: DrawBrokenReferencesTab(); break;
                case 3: DrawOrphanedMetaFilesTab(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawOverviewTab()
        {
            // 전체 결과 요약
            EditorGUILayout.LabelField("검증 결과 요약", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"스캔된 에셋: {lastResult.TotalAssetsScanned:N0}개");
            EditorGUILayout.LabelField($"소요 시간: {lastResult.ScanDuration:F2}초");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // 문제 요약
            if (!lastResult.HasErrors)
            {
                EditorGUILayout.BeginVertical(successStyle);
                EditorGUILayout.LabelField("✅ 모든 GUID가 정상입니다!", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("발견된 문제", EditorStyles.boldLabel);

                if (lastResult.DuplicateGuids.Count > 0)
                {
                    EditorGUILayout.BeginVertical(errorStyle);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"❌ 중복 GUID: {lastResult.DuplicateGuids.Count}건");
                    if (GUILayout.Button("자동 수정", GUILayout.Width(80)))
                    {
                        FixDuplicateGuids();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                if (lastResult.BrokenReferences.Count > 0)
                {
                    EditorGUILayout.BeginVertical(errorStyle);
                    EditorGUILayout.LabelField($"❌ 손상된 참조: {lastResult.BrokenReferences.Count}건");
                    EditorGUILayout.HelpBox("손상된 참조는 수동으로 수정해야 합니다.", MessageType.Warning);
                    EditorGUILayout.EndVertical();
                }

                if (lastResult.OrphanedMetaFiles.Count > 0)
                {
                    EditorGUILayout.BeginVertical(warningStyle);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"⚠️ 고아 메타파일: {lastResult.OrphanedMetaFiles.Count}건");
                    if (GUILayout.Button("정리", GUILayout.Width(80)))
                    {
                        CleanOrphanedMetaFiles();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void DrawDuplicateGuidsTab()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"중복 GUID ({lastResult.DuplicateGuids.Count}건)", EditorStyles.boldLabel);

            if (lastResult.DuplicateGuids.Count > 0)
            {
                if (GUILayout.Button("모두 자동 수정", GUILayout.Width(100)))
                {
                    FixDuplicateGuids();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (lastResult.DuplicateGuids.Count == 0)
            {
                EditorGUILayout.HelpBox("중복된 GUID가 발견되지 않았습니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();

            foreach (var duplicate in lastResult.DuplicateGuids)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"GUID: {duplicate.Guid}", EditorStyles.boldLabel);

                foreach (string assetPath in duplicate.AssetPaths)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • {assetPath}");

                    if (GUILayout.Button("선택", GUILayout.Width(50)))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        if (asset != null)
                        {
                            Selection.activeObject = asset;
                            EditorGUIUtility.PingObject(asset);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        private void DrawBrokenReferencesTab()
        {
            EditorGUILayout.LabelField($"손상된 참조 ({lastResult.BrokenReferences.Count}건)", EditorStyles.boldLabel);

            if (lastResult.BrokenReferences.Count == 0)
            {
                EditorGUILayout.HelpBox("손상된 참조가 발견되지 않았습니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("손상된 참조는 해당 에셋에서 직접 수정해야 합니다.", MessageType.Warning);
            EditorGUILayout.Space();

            foreach (var broken in lastResult.BrokenReferences)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{broken.AssetPath} ({broken.AssetType})");

                if (GUILayout.Button("열기", GUILayout.Width(50)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(broken.AssetPath);
                    if (asset != null)
                    {
                        AssetDatabase.OpenAsset(asset);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"  필드: {broken.FieldName}");
                EditorGUILayout.LabelField($"  손상된 GUID: {broken.BrokenGuid}");
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        private void DrawOrphanedMetaFilesTab()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"고아 메타파일 ({lastResult.OrphanedMetaFiles.Count}건)", EditorStyles.boldLabel);

            if (lastResult.OrphanedMetaFiles.Count > 0)
            {
                if (GUILayout.Button("모두 정리", GUILayout.Width(80)))
                {
                    CleanOrphanedMetaFiles();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (lastResult.OrphanedMetaFiles.Count == 0)
            {
                EditorGUILayout.HelpBox("고아 메타파일이 발견되지 않았습니다.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("이 메타파일들은 대응하는 에셋이 없으므로 안전하게 제거할 수 있습니다.", MessageType.Info);
            EditorGUILayout.Space();

            foreach (var orphaned in lastResult.OrphanedMetaFiles)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"메타파일: {orphaned.MetaFilePath}");
                EditorGUILayout.LabelField($"대상 에셋: {orphaned.ExpectedAssetPath}");
                EditorGUILayout.LabelField($"GUID: {orphaned.Guid}");
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        private void DrawEmptyState()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("GUID 검증을 시작하려면 위의 버튼을 클릭하세요", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 작업 실행

        private void StartValidation()
        {
            isScanning = true;

            try
            {
                lastResult = GUIDValidator.ValidateProject(includePackages);
                selectedTab = 0; // 결과 탭으로 이동
            }
            finally
            {
                isScanning = false;
            }
        }

        private void FixDuplicateGuids()
        {
            if (lastResult?.DuplicateGuids?.Count > 0)
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "GUID 자동 수정",
                    $"{lastResult.DuplicateGuids.Count}개의 중복 GUID를 수정하시겠습니까?\n\n" +
                    "이 작업은 되돌릴 수 없으며, 영향받는 에셋들의 참조가 변경될 수 있습니다.",
                    "수정",
                    "취소"
                );

                if (confirm)
                {
                    EditorUtility.DisplayProgressBar("GUID 수정", "중복 GUID를 수정하는 중...", 0.5f);

                    try
                    {
                        bool success = GUIDValidator.FixDuplicateGuids(lastResult.DuplicateGuids);
                        if (success)
                        {
                            EditorUtility.DisplayDialog("완료", "중복 GUID 수정이 완료되었습니다.", "확인");
                            StartValidation(); // 재검증
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("오류", "일부 GUID 수정에 실패했습니다. 콘솔을 확인해주세요.", "확인");
                        }
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
            }
        }

        private void CleanOrphanedMetaFiles()
        {
            if (lastResult?.OrphanedMetaFiles?.Count > 0)
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "고아 메타파일 정리",
                    $"{lastResult.OrphanedMetaFiles.Count}개의 고아 메타파일을 제거하시겠습니까?\n\n" +
                    "이 메타파일들은 대응하는 에셋이 없으므로 안전하게 제거할 수 있습니다.",
                    "제거",
                    "취소"
                );

                if (confirm)
                {
                    EditorUtility.DisplayProgressBar("메타파일 정리", "고아 메타파일을 제거하는 중...", 0.5f);

                    try
                    {
                        bool success = GUIDValidator.CleanOrphanedMetaFiles(lastResult.OrphanedMetaFiles);
                        if (success)
                        {
                            EditorUtility.DisplayDialog("완료", "고아 메타파일 정리가 완료되었습니다.", "확인");
                            StartValidation(); // 재검증
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("오류", "일부 메타파일 제거에 실패했습니다. 콘솔을 확인해주세요.", "확인");
                        }
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }
            }
        }

        #endregion
    }
}