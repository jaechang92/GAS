using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
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
        private Vector2 folderScrollPosition;
        private int selectedTab = 0;
        private readonly string[] tabNames = { "검증 결과", "중복 GUID", "손상된 참조", "고아 메타파일" };

        // 폴더 선택 (기존 방식 - 호환성)
        private List<FolderSelection> assetFolders = new List<FolderSelection>();
        private bool showFolderSelection = true;

        // 에셋 트리 (새 방식)
        private AssetTreeNode rootNode;
        private HashSet<string> previouslyScannedPaths = new HashSet<string>();
        private const string PREF_KEY_SCANNED_PATHS = "GUIDValidator.ScannedPaths";
        private GUIStyle newLabelStyle;
        private GUIStyle foldoutStyle;

        // UI 스타일
        private GUIStyle headerStyle;
        private GUIStyle errorStyle;
        private GUIStyle warningStyle;
        private GUIStyle successStyle;

        #endregion

        #region 데이터 구조

        [System.Serializable]
        private class FolderSelection
        {
            public string Path;
            public bool IsSelected;
            public string DisplayName;

            public FolderSelection(string path, bool isSelected)
            {
                Path = path;
                IsSelected = isSelected;
                DisplayName = path.Replace("Assets/", "");
                if (string.IsNullOrEmpty(DisplayName))
                    DisplayName = "Assets (Root)";
            }
        }

        /// <summary>
        /// 에셋 트리 노드 (폴더 또는 파일)
        /// </summary>
        [System.Serializable]
        private class AssetTreeNode
        {
            public string Path;                     // 전체 경로 (예: "Assets/_Project/Scripts")
            public string Name;                     // 이름만 (예: "Scripts")
            public bool IsFolder;                   // 폴더 여부
            public bool IsSelected;                 // 체크박스 선택 여부
            public bool IsFoldedOut;                // 펼쳐짐 여부 (폴더만)
            public bool IsNew;                      // 새로 생성된 항목 여부
            public List<AssetTreeNode> Children;    // 하위 노드들
            public int Depth;                       // 들여쓰기 깊이

            public AssetTreeNode(string path, bool isFolder, int depth)
            {
                Path = path;
                Name = System.IO.Path.GetFileName(path);
                if (string.IsNullOrEmpty(Name))
                    Name = "Assets";
                IsFolder = isFolder;
                IsSelected = true; // 기본적으로 모두 선택
                IsFoldedOut = depth < 2; // 2단계까지만 기본 펼침
                IsNew = false;
                Children = new List<AssetTreeNode>();
                Depth = depth;
            }
        }

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
            LoadPreviouslyScannedPaths();
            BuildAssetTree();
        }

        /// <summary>
        /// 이전에 검사했던 경로 불러오기
        /// </summary>
        private void LoadPreviouslyScannedPaths()
        {
            string savedPaths = EditorPrefs.GetString(PREF_KEY_SCANNED_PATHS, "");
            previouslyScannedPaths.Clear();

            if (!string.IsNullOrEmpty(savedPaths))
            {
                string[] paths = savedPaths.Split(';');
                foreach (string path in paths)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        previouslyScannedPaths.Add(path);
                    }
                }
            }

            Debug.Log($"[GUIDValidator] 이전 검사 기록: {previouslyScannedPaths.Count}개 항목");
        }

        /// <summary>
        /// 에셋 트리 구축
        /// </summary>
        private void BuildAssetTree()
        {
            rootNode = new AssetTreeNode("Assets", true, 0);
            BuildTreeRecursive(rootNode, "Assets", 0);
            Debug.Log("[GUIDValidator] 에셋 트리 구축 완료");
        }

        /// <summary>
        /// 재귀적으로 트리 구축
        /// </summary>
        private void BuildTreeRecursive(AssetTreeNode parentNode, string directoryPath, int depth)
        {
            if (!System.IO.Directory.Exists(directoryPath))
                return;

            try
            {
                // 하위 폴더들 추가
                string[] directories = System.IO.Directory.GetDirectories(directoryPath);
                foreach (string dir in directories)
                {
                    string folderPath = dir.Replace("\\", "/");

                    // .meta 폴더나 숨김 폴더 제외
                    string folderName = System.IO.Path.GetFileName(folderPath);
                    if (folderName.StartsWith("."))
                        continue;

                    var folderNode = new AssetTreeNode(folderPath, true, depth + 1);
                    folderNode.IsNew = !previouslyScannedPaths.Contains(folderPath);
                    parentNode.Children.Add(folderNode);

                    // 재귀적으로 하위 폴더 탐색 (최대 5단계까지)
                    if (depth < 5)
                    {
                        BuildTreeRecursive(folderNode, folderPath, depth + 1);
                    }
                }

                // 하위 파일들 추가
                string[] files = System.IO.Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    string filePath = file.Replace("\\", "/");

                    // .meta 파일 제외
                    if (filePath.EndsWith(".meta"))
                        continue;

                    var fileNode = new AssetTreeNode(filePath, false, depth + 1);
                    fileNode.IsNew = !previouslyScannedPaths.Contains(filePath);
                    parentNode.Children.Add(fileNode);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[GUIDValidator] {directoryPath} 스캔 실패: {e.Message}");
            }
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

            if (newLabelStyle == null)
            {
                newLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.5f, 1f, 0.5f) }, // 연두색
                    fontSize = 10,
                    fontStyle = FontStyle.Bold
                };
            }

            if (foldoutStyle == null)
            {
                foldoutStyle = new GUIStyle(EditorStyles.foldout);
                foldoutStyle.margin = new RectOffset(0, 0, 0, 0); // 오른쪽 여백을 음수로 설정하여 제거
                foldoutStyle.padding = new RectOffset(0, 0, 0, 0);
                foldoutStyle.fixedWidth = 12f;
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
            // 폴더 선택 UI (트리 구조)
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            showFolderSelection = EditorGUILayout.Foldout(showFolderSelection, "검증 대상 폴더/파일 선택 (Export Package 스타일)", true);

            if (GUILayout.Button("모두 선택", GUILayout.Width(80)))
            {
                SelectAllTreeNodes(rootNode, true);
            }

            if (GUILayout.Button("모두 해제", GUILayout.Width(80)))
            {
                SelectAllTreeNodes(rootNode, false);
            }

            if (GUILayout.Button("새로고침", GUILayout.Width(80)))
            {
                BuildAssetTree();
            }

            EditorGUILayout.EndHorizontal();

            if (showFolderSelection && rootNode != null)
            {
                folderScrollPosition = EditorGUILayout.BeginScrollView(folderScrollPosition, GUILayout.MaxHeight(300));

                // 트리 루트부터 그리기
                DrawTreeNode(rootNode);

                EditorGUILayout.EndScrollView();

                // 통계 표시
                int selectedCount = CountSelectedNodes(rootNode);
                int totalCount = CountTotalNodes(rootNode);
                int newCount = CountNewNodes(rootNode);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"선택: {selectedCount} / {totalCount}", EditorStyles.miniLabel);
                if (newCount > 0)
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField($"새 항목: {newCount}개", newLabelStyle);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // 스캔 옵션 및 버튼
            EditorGUILayout.BeginHorizontal();

            // 패키지 포함 옵션
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

        /// <summary>
        /// 트리 노드 재귀적으로 그리기
        /// </summary>
        private void DrawTreeNode(AssetTreeNode node)
        {
            if (node == null)
                return;

            EditorGUILayout.BeginHorizontal();

            // 들여쓰기
            GUILayout.Space(node.Depth * 16);

            // NEW 라벨 (새 항목인 경우)
            if (node.IsNew)
            {
                GUILayout.Label("NEW", newLabelStyle, GUILayout.Width(35));
            }
            else
            {
                GUILayout.Space(35); // 정렬을 위한 공백
            }

            // 폴더인 경우 접기/펼치기 아이콘
            if (node.IsFolder && node.Children.Count > 0)
            {
                // Foldout 대신 클릭 가능한 화살표 표시
                string arrow = node.IsFoldedOut ? "▼" : "▶";
                if (GUILayout.Button(arrow, EditorStyles.label, GUILayout.Width(12)))
                {
                    node.IsFoldedOut = !node.IsFoldedOut;
                }
            }
            else
            {
                GUILayout.Space(12);
            }

            // 체크박스와 이름
            bool newSelected = EditorGUILayout.ToggleLeft(
                node.IsFolder ? $"📁 {node.Name}" : $"📄 {node.Name}",
                node.IsSelected
            );

            // 체크박스 변경 시 하위 항목도 일괄 변경
            if (newSelected != node.IsSelected)
            {
                node.IsSelected = newSelected;
                if (node.IsFolder)
                {
                    PropagateSelectionToChildren(node, newSelected);
                }
            }

            EditorGUILayout.EndHorizontal();

            // 하위 항목 그리기 (펼쳐져 있을 때만)
            if (node.IsFoldedOut && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    DrawTreeNode(child);
                }
            }
        }

        /// <summary>
        /// 하위 노드들에게 선택 상태 전파
        /// </summary>
        private void PropagateSelectionToChildren(AssetTreeNode node, bool isSelected)
        {
            foreach (var child in node.Children)
            {
                child.IsSelected = isSelected;
                if (child.IsFolder)
                {
                    PropagateSelectionToChildren(child, isSelected);
                }
            }
        }

        /// <summary>
        /// 모든 트리 노드 선택/해제
        /// </summary>
        private void SelectAllTreeNodes(AssetTreeNode node, bool selected)
        {
            if (node == null)
                return;

            node.IsSelected = selected;
            foreach (var child in node.Children)
            {
                SelectAllTreeNodes(child, selected);
            }
        }

        /// <summary>
        /// 선택된 노드 개수 계산
        /// </summary>
        private int CountSelectedNodes(AssetTreeNode node)
        {
            if (node == null)
                return 0;

            int count = node.IsSelected ? 1 : 0;
            foreach (var child in node.Children)
            {
                count += CountSelectedNodes(child);
            }
            return count;
        }

        /// <summary>
        /// 전체 노드 개수 계산
        /// </summary>
        private int CountTotalNodes(AssetTreeNode node)
        {
            if (node == null)
                return 0;

            int count = 1;
            foreach (var child in node.Children)
            {
                count += CountTotalNodes(child);
            }
            return count;
        }

        /// <summary>
        /// 새 항목 개수 계산
        /// </summary>
        private int CountNewNodes(AssetTreeNode node)
        {
            if (node == null)
                return 0;

            int count = node.IsNew ? 1 : 0;
            foreach (var child in node.Children)
            {
                count += CountNewNodes(child);
            }
            return count;
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
                // 선택된 폴더 목록 구성 (트리에서)
                var selectedFolders = new List<string>();
                CollectSelectedFolders(rootNode, selectedFolders);

                // 선택된 폴더가 없으면 경고
                if (selectedFolders.Count == 0)
                {
                    EditorUtility.DisplayDialog("경고", "검증할 폴더를 하나 이상 선택해주세요.", "확인");
                    return;
                }

                // 검증 실행
                lastResult = GUIDValidator.ValidateProject(includePackages, selectedFolders);
                selectedTab = 0; // 결과 탭으로 이동

                // 검증 완료 후 경로 저장 (다음에 NEW 표시 안 하도록)
                SaveScannedPaths(rootNode);
            }
            finally
            {
                isScanning = false;
            }
        }

        /// <summary>
        /// 선택된 폴더만 수집
        /// </summary>
        private void CollectSelectedFolders(AssetTreeNode node, List<string> selectedFolders)
        {
            if (node == null)
                return;

            // 선택되고 폴더인 경우에만 추가
            if (node.IsSelected && node.IsFolder)
            {
                selectedFolders.Add(node.Path);
            }

            // 하위 노드도 재귀적으로 검사
            foreach (var child in node.Children)
            {
                CollectSelectedFolders(child, selectedFolders);
            }
        }

        /// <summary>
        /// 검증한 경로들 저장
        /// </summary>
        private void SaveScannedPaths(AssetTreeNode node)
        {
            if (node == null)
                return;

            previouslyScannedPaths.Add(node.Path);
            foreach (var child in node.Children)
            {
                SaveScannedPaths(child);
            }

            // EditorPrefs에 저장
            string pathsString = string.Join(";", previouslyScannedPaths);
            EditorPrefs.SetString(PREF_KEY_SCANNED_PATHS, pathsString);

            Debug.Log($"[GUIDValidator] {previouslyScannedPaths.Count}개 경로 저장 완료");
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
