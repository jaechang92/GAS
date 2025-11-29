using System;
using System.IO;
using UnityEngine;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 스테이지 진행 저장/로드 관리
    /// </summary>
    public static class StageProgressSaver
    {
        private const string SaveFolderName = "StageProgress";
        private const string CurrentRunFileName = "current_run.json";
        private const string BestRunFileNameFormat = "best_{0}.json"; // best_stageId.json

        /// <summary>
        /// 저장 폴더 경로
        /// </summary>
        private static string SaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);


        // ====== 현재 진행 저장/로드 ======

        /// <summary>
        /// 현재 스테이지 진행 상황 저장
        /// </summary>
        public static void SaveCurrentProgress(StageProgressData progress)
        {
            if (progress == null) return;

            try
            {
                EnsureSaveFolderExists();

                string json = JsonUtility.ToJson(progress, true);
                string filePath = Path.Combine(SaveFolderPath, CurrentRunFileName);

                File.WriteAllText(filePath, json);

                Debug.Log($"[StageProgressSaver] 진행 상황 저장됨: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[StageProgressSaver] 저장 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 현재 스테이지 진행 상황 로드
        /// </summary>
        public static StageProgressData LoadCurrentProgress()
        {
            try
            {
                string filePath = Path.Combine(SaveFolderPath, CurrentRunFileName);

                if (!File.Exists(filePath))
                {
                    Debug.Log("[StageProgressSaver] 저장된 진행 상황 없음");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                var progress = JsonUtility.FromJson<StageProgressData>(json);

                Debug.Log($"[StageProgressSaver] 진행 상황 로드됨: {progress.stageId}");
                return progress;
            }
            catch (Exception e)
            {
                Debug.LogError($"[StageProgressSaver] 로드 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 현재 진행 상황 삭제
        /// </summary>
        public static void DeleteCurrentProgress()
        {
            try
            {
                string filePath = Path.Combine(SaveFolderPath, CurrentRunFileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log("[StageProgressSaver] 현재 진행 상황 삭제됨");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[StageProgressSaver] 삭제 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 저장된 진행 상황 존재 여부
        /// </summary>
        public static bool HasSavedProgress()
        {
            string filePath = Path.Combine(SaveFolderPath, CurrentRunFileName);
            return File.Exists(filePath);
        }


        // ====== 최고 기록 저장/로드 ======

        /// <summary>
        /// 최고 기록 저장
        /// </summary>
        public static void SaveBestRecord(StageProgressData progress)
        {
            if (progress == null || string.IsNullOrEmpty(progress.stageId)) return;

            try
            {
                EnsureSaveFolderExists();

                // 기존 최고 기록 확인
                var existingBest = LoadBestRecord(progress.stageId);

                // 더 나은 기록인지 확인
                if (existingBest != null && !IsBetterRecord(progress, existingBest))
                {
                    Debug.Log("[StageProgressSaver] 기존 최고 기록이 더 좋음 - 저장 안 함");
                    return;
                }

                string json = JsonUtility.ToJson(progress, true);
                string fileName = string.Format(BestRunFileNameFormat, progress.stageId);
                string filePath = Path.Combine(SaveFolderPath, fileName);

                File.WriteAllText(filePath, json);

                Debug.Log($"[StageProgressSaver] 최고 기록 저장됨: {progress.stageId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[StageProgressSaver] 최고 기록 저장 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 최고 기록 로드
        /// </summary>
        public static StageProgressData LoadBestRecord(string stageId)
        {
            if (string.IsNullOrEmpty(stageId)) return null;

            try
            {
                string fileName = string.Format(BestRunFileNameFormat, stageId);
                string filePath = Path.Combine(SaveFolderPath, fileName);

                if (!File.Exists(filePath))
                {
                    return null;
                }

                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<StageProgressData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StageProgressSaver] 최고 기록 로드 실패: {e.Message}");
                return null;
            }
        }


        // ====== 유틸리티 ======

        private static void EnsureSaveFolderExists()
        {
            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
            }
        }

        /// <summary>
        /// 더 나은 기록인지 비교
        /// </summary>
        private static bool IsBetterRecord(StageProgressData newRecord, StageProgressData existingRecord)
        {
            // 완료된 기록이 더 우선
            if (newRecord.isCompleted && !existingRecord.isCompleted)
                return true;

            if (!newRecord.isCompleted && existingRecord.isCompleted)
                return false;

            // 둘 다 완료된 경우: 시간이 짧은 쪽이 우선
            if (newRecord.isCompleted && existingRecord.isCompleted)
            {
                return newRecord.PlayTimeSeconds < existingRecord.PlayTimeSeconds;
            }

            // 둘 다 미완료인 경우: 더 많은 층을 클리어한 쪽이 우선
            return newRecord.clearedFloors.Count > existingRecord.clearedFloors.Count;
        }

        /// <summary>
        /// 모든 저장 데이터 삭제
        /// </summary>
        public static void ClearAllSaves()
        {
            try
            {
                if (Directory.Exists(SaveFolderPath))
                {
                    Directory.Delete(SaveFolderPath, true);
                    Debug.Log("[StageProgressSaver] 모든 저장 데이터 삭제됨");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[StageProgressSaver] 삭제 실패: {e.Message}");
            }
        }
    }
}
