using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GUIDValidation
{
    /// <summary>
    /// Unity GUID 검증 도구
    /// Git 작업 중 발생하는 GUID 문제들을 탐지하고 수정하는 독립적인 패키지
    ///
    /// 주요 기능:
    /// - GUID 중복 검사
    /// - 손상된 참조 검사 (guid: 0)
    /// - 고아 메타파일 검사
    /// - 자동 복구 기능
    /// </summary>
    public class GUIDValidator
    {
        #region 데이터 구조

        /// <summary>
        /// GUID 검증 결과
        /// </summary>
        public class ValidationResult
        {
            public bool HasErrors => DuplicateGuids.Count > 0 || BrokenReferences.Count > 0 || OrphanedMetaFiles.Count > 0;
            public List<DuplicateGuidInfo> DuplicateGuids = new List<DuplicateGuidInfo>();
            public List<BrokenReferenceInfo> BrokenReferences = new List<BrokenReferenceInfo>();
            public List<OrphanedMetaInfo> OrphanedMetaFiles = new List<OrphanedMetaInfo>();
            public int TotalAssetsScanned;
            public float ScanDuration;
        }

        /// <summary>
        /// 중복 GUID 정보
        /// </summary>
        public class DuplicateGuidInfo
        {
            public string Guid;
            public List<string> AssetPaths = new List<string>();

            public override string ToString()
            {
                return $"GUID: {Guid} → 파일들: {string.Join(", ", AssetPaths)}";
            }
        }

        /// <summary>
        /// 손상된 참조 정보
        /// </summary>
        public class BrokenReferenceInfo
        {
            public string AssetPath;
            public string AssetType;
            public string BrokenGuid;
            public string FieldName;

            public override string ToString()
            {
                return $"{AssetPath} ({AssetType}) - {FieldName}: {BrokenGuid}";
            }
        }

        /// <summary>
        /// 고아 메타파일 정보
        /// </summary>
        public class OrphanedMetaInfo
        {
            public string MetaFilePath;
            public string ExpectedAssetPath;
            public string Guid;

            public override string ToString()
            {
                return $"{MetaFilePath} → 대상: {ExpectedAssetPath}";
            }
        }

        #endregion

        #region 정적 검증 메서드

        /// <summary>
        /// 전체 프로젝트 GUID 검증 실행
        /// </summary>
        /// <param name="includePackages">Packages 폴더 포함 여부</param>
        /// <param name="selectedFolders">검증할 폴더 목록 (null이면 전체)</param>
        public static ValidationResult ValidateProject(bool includePackages = false, List<string> selectedFolders = null)
        {
            var startTime = EditorApplication.timeSinceStartup;
            var result = new ValidationResult();

            try
            {
                Debug.Log("[GUIDValidator] 프로젝트 GUID 검증 시작...");

                if (selectedFolders != null && selectedFolders.Count > 0)
                {
                    Debug.Log($"[GUIDValidator] 검증 대상 폴더: {string.Join(", ", selectedFolders)}");
                }

                // 1. 모든 에셋 파일 수집
                var assetFiles = CollectAssetFiles(includePackages, selectedFolders);
                result.TotalAssetsScanned = assetFiles.Count;

                Debug.Log($"[GUIDValidator] {assetFiles.Count}개 에셋 파일 스캔 중...");

                // 2. GUID 맵핑 구성
                var guidToAssets = BuildGuidMapping(assetFiles);

                // 3. 중복 GUID 검사
                result.DuplicateGuids = FindDuplicateGuids(guidToAssets);

                // 4. 손상된 참조 검사
                result.BrokenReferences = FindBrokenReferences(assetFiles);

                // 5. 고아 메타파일 검사
                result.OrphanedMetaFiles = FindOrphanedMetaFiles(selectedFolders);

                result.ScanDuration = (float)(EditorApplication.timeSinceStartup - startTime);

                LogValidationResults(result);
                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GUIDValidator] 검증 중 오류 발생: {e.Message}\n{e.StackTrace}");
                result.ScanDuration = (float)(EditorApplication.timeSinceStartup - startTime);
                return result;
            }
        }

        /// <summary>
        /// 에셋 파일 수집
        /// </summary>
        /// <param name="includePackages">Packages 폴더 포함 여부</param>
        /// <param name="selectedFolders">검증할 폴더 목록 (null이면 전체)</param>
        private static List<string> CollectAssetFiles(bool includePackages, List<string> selectedFolders)
        {
            var assetFiles = new List<string>();

            // 검증할 폴더 결정
            string[] searchFolders;

            if (selectedFolders != null && selectedFolders.Count > 0)
            {
                // 선택된 폴더만 검색
                searchFolders = selectedFolders.ToArray();
            }
            else if (includePackages)
            {
                // 전체 (Assets + Packages)
                searchFolders = null;
            }
            else
            {
                // Assets만
                searchFolders = new[] { "Assets" };
            }

            string[] guids = AssetDatabase.FindAssets("", searchFolders);

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath) && !assetPath.EndsWith(".meta"))
                {
                    assetFiles.Add(assetPath);
                }
            }

            return assetFiles;
        }

        /// <summary>
        /// GUID → 에셋 경로 맵핑 구성
        /// </summary>
        private static Dictionary<string, List<string>> BuildGuidMapping(List<string> assetFiles)
        {
            var guidToAssets = new Dictionary<string, List<string>>();

            foreach (string assetPath in assetFiles)
            {
                string metaPath = assetPath + ".meta";
                if (File.Exists(metaPath))
                {
                    string guid = ExtractGuidFromMetaFile(metaPath);
                    if (!string.IsNullOrEmpty(guid) && guid != "0")
                    {
                        if (!guidToAssets.ContainsKey(guid))
                        {
                            guidToAssets[guid] = new List<string>();
                        }
                        guidToAssets[guid].Add(assetPath);
                    }
                }
            }

            return guidToAssets;
        }

        /// <summary>
        /// 메타파일에서 GUID 추출
        /// </summary>
        private static string ExtractGuidFromMetaFile(string metaPath)
        {
            try
            {
                string content = File.ReadAllText(metaPath);
                var match = Regex.Match(content, @"guid:\s*([a-fA-F0-9]+)");
                return match.Success ? match.Groups[1].Value : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 중복 GUID 찾기
        /// </summary>
        private static List<DuplicateGuidInfo> FindDuplicateGuids(Dictionary<string, List<string>> guidToAssets)
        {
            var duplicates = new List<DuplicateGuidInfo>();

            foreach (var kvp in guidToAssets)
            {
                if (kvp.Value.Count > 1)
                {
                    duplicates.Add(new DuplicateGuidInfo
                    {
                        Guid = kvp.Key,
                        AssetPaths = new List<string>(kvp.Value)
                    });
                }
            }

            return duplicates;
        }

        /// <summary>
        /// 손상된 참조 찾기
        /// </summary>
        private static List<BrokenReferenceInfo> FindBrokenReferences(List<string> assetFiles)
        {
            var brokenRefs = new List<BrokenReferenceInfo>();

            foreach (string assetPath in assetFiles)
            {
                if (assetPath.EndsWith(".asset") || assetPath.EndsWith(".prefab") || assetPath.EndsWith(".unity"))
                {
                    try
                    {
                        string content = File.ReadAllText(assetPath);

                        // 실제로 손상된 GUID만 찾기 (0, 빈값, 너무 짧은 값)
                        var brokenGuidMatches = Regex.Matches(content, @"guid:\s*(0+(?:[^a-fA-F0-9]|$)|[a-fA-F0-9]{1,7}(?:[^a-fA-F0-9]|$))");
                        foreach (Match match in brokenGuidMatches)
                        {
                            string guidValue = match.Groups[1].Value.TrimEnd();

                            brokenRefs.Add(new BrokenReferenceInfo
                            {
                                AssetPath = assetPath,
                                AssetType = Path.GetExtension(assetPath),
                                BrokenGuid = guidValue,
                                FieldName = "Broken Reference"
                            });
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[GUIDValidator] {assetPath} 읽기 실패: {e.Message}");
                    }
                }
            }

            return brokenRefs;
        }

        /// <summary>
        /// 고아 메타파일 찾기
        /// </summary>
        /// <param name="selectedFolders">검증할 폴더 목록 (null이면 Assets 전체)</param>
        private static List<OrphanedMetaInfo> FindOrphanedMetaFiles(List<string> selectedFolders)
        {
            var orphanedMetas = new List<OrphanedMetaInfo>();

            // 검색할 폴더 결정
            var foldersToScan = new List<string>();

            if (selectedFolders != null && selectedFolders.Count > 0)
            {
                foldersToScan.AddRange(selectedFolders);
            }
            else
            {
                foldersToScan.Add("Assets");
            }

            // 각 폴더에서 메타파일 검색
            foreach (string folder in foldersToScan)
            {
                if (!Directory.Exists(folder))
                    continue;

                string[] metaFiles = Directory.GetFiles(folder, "*.meta", SearchOption.AllDirectories);

                foreach (string metaPath in metaFiles)
                {
                    string assetPath = metaPath.Substring(0, metaPath.Length - 5); // .meta 제거

                    if (!File.Exists(assetPath) && !Directory.Exists(assetPath))
                    {
                        string guid = ExtractGuidFromMetaFile(metaPath);
                        orphanedMetas.Add(new OrphanedMetaInfo
                        {
                            MetaFilePath = metaPath,
                            ExpectedAssetPath = assetPath,
                            Guid = guid ?? "Unknown"
                        });
                    }
                }
            }

            return orphanedMetas;
        }

        #endregion

        #region 복구 메서드

        /// <summary>
        /// 중복 GUID 자동 복구
        /// </summary>
        public static bool FixDuplicateGuids(List<DuplicateGuidInfo> duplicates)
        {
            bool success = true;

            foreach (var duplicate in duplicates)
            {
                Debug.Log($"[GUIDValidator] 중복 GUID 복구 중: {duplicate.Guid}");

                // 첫 번째 파일은 유지하고 나머지 파일들의 GUID 재생성
                for (int i = 1; i < duplicate.AssetPaths.Count; i++)
                {
                    string assetPath = duplicate.AssetPaths[i];
                    if (!RegenerateAssetGuid(assetPath))
                    {
                        success = false;
                        Debug.LogError($"[GUIDValidator] {assetPath} GUID 재생성 실패");
                    }
                }
            }

            if (success)
            {
                AssetDatabase.Refresh();
                Debug.Log("[GUIDValidator] 중복 GUID 복구 완료");
            }

            return success;
        }

        /// <summary>
        /// 에셋의 GUID 재생성
        /// </summary>
        private static bool RegenerateAssetGuid(string assetPath)
        {
            try
            {
                string metaPath = assetPath + ".meta";
                if (!File.Exists(metaPath))
                {
                    return false;
                }

                // 새 GUID 생성
                string newGuid = System.Guid.NewGuid().ToString("N");

                // 메타파일 내용 읽기
                string content = File.ReadAllText(metaPath);

                // GUID 교체
                content = Regex.Replace(content, @"guid:\s*[a-fA-F0-9]+", $"guid: {newGuid}");

                // 메타파일 저장
                File.WriteAllText(metaPath, content);

                Debug.Log($"[GUIDValidator] {assetPath} GUID 재생성: {newGuid}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GUIDValidator] {assetPath} GUID 재생성 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 고아 메타파일 제거
        /// </summary>
        public static bool CleanOrphanedMetaFiles(List<OrphanedMetaInfo> orphanedMetas)
        {
            bool success = true;

            foreach (var orphaned in orphanedMetas)
            {
                try
                {
                    File.Delete(orphaned.MetaFilePath);
                    Debug.Log($"[GUIDValidator] 고아 메타파일 제거: {orphaned.MetaFilePath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[GUIDValidator] {orphaned.MetaFilePath} 제거 실패: {e.Message}");
                    success = false;
                }
            }

            if (success)
            {
                AssetDatabase.Refresh();
                Debug.Log("[GUIDValidator] 고아 메타파일 정리 완료");
            }

            return success;
        }

        #endregion

        #region 로깅

        /// <summary>
        /// 검증 결과 로깅
        /// </summary>
        private static void LogValidationResults(ValidationResult result)
        {
            Debug.Log($"[GUIDValidator] 검증 완료 - 소요시간: {result.ScanDuration:F2}초, 스캔된 에셋: {result.TotalAssetsScanned}개");

            if (!result.HasErrors)
            {
                Debug.Log("[GUIDValidator] ✅ 모든 GUID가 정상입니다!");
                return;
            }

            Debug.LogWarning($"[GUIDValidator] ⚠️ GUID 문제 발견:");
            Debug.LogWarning($"- 중복 GUID: {result.DuplicateGuids.Count}건");
            Debug.LogWarning($"- 손상된 참조: {result.BrokenReferences.Count}건");
            Debug.LogWarning($"- 고아 메타파일: {result.OrphanedMetaFiles.Count}건");

            // 상세 정보 로깅
            foreach (var duplicate in result.DuplicateGuids)
            {
                Debug.LogError($"[중복 GUID] {duplicate}");
            }

            foreach (var broken in result.BrokenReferences)
            {
                Debug.LogError($"[손상된 참조] {broken}");
            }

            foreach (var orphaned in result.OrphanedMetaFiles)
            {
                Debug.LogWarning($"[고아 메타파일] {orphaned}");
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 특정 에셋의 GUID 정보 가져오기
        /// </summary>
        public static string GetAssetGuid(string assetPath)
        {
            string metaPath = assetPath + ".meta";
            return File.Exists(metaPath) ? ExtractGuidFromMetaFile(metaPath) : null;
        }

        /// <summary>
        /// GUID 유효성 검사
        /// </summary>
        public static bool IsValidGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid) || guid == "0")
                return false;

            // Unity GUID는 보통 31-32자이지만, 더 짧을 수도 있음 (앞자리 0 생략)
            // 최소 8자 이상의 hex 문자열이면 유효한 것으로 간주
            return guid.Length >= 8 &&
                   guid.Length <= 32 &&
                   Regex.IsMatch(guid, "^[a-fA-F0-9]+$");
        }

        #endregion
    }
}