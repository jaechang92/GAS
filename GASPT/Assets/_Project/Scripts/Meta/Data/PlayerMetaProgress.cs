using System;
using System.Collections.Generic;
using UnityEngine;

namespace GASPT.Meta
{
    /// <summary>
    /// 플레이어 메타 진행 저장 데이터
    /// JSON 직렬화 가능한 구조
    /// </summary>
    [Serializable]
    public class PlayerMetaProgress
    {
        // ====== 재화 ======

        /// <summary>
        /// 영구 Bone 보유량
        /// </summary>
        public int bone;

        /// <summary>
        /// 영구 Soul 보유량
        /// </summary>
        public int soul;


        // ====== 업그레이드 ======

        /// <summary>
        /// 업그레이드 레벨 목록 (업그레이드ID → 현재레벨)
        /// </summary>
        public List<UpgradeLevelEntry> upgradeLevels = new List<UpgradeLevelEntry>();


        // ====== 해금 ======

        /// <summary>
        /// 해금된 폼 ID 목록
        /// </summary>
        public List<string> unlockedForms = new List<string>();

        /// <summary>
        /// 해금된 아이템 ID 목록
        /// </summary>
        public List<string> unlockedItems = new List<string>();


        // ====== 업적 ======

        /// <summary>
        /// 완료된 업적 ID 목록
        /// </summary>
        public List<string> completedAchievements = new List<string>();

        /// <summary>
        /// 업적 진행도 목록 (업적ID → 현재 진행도)
        /// </summary>
        public List<AchievementProgressEntry> achievementProgress = new List<AchievementProgressEntry>();


        // ====== 통계 ======

        /// <summary>
        /// 총 플레이 시간 (초)
        /// </summary>
        public float totalPlayTime;

        /// <summary>
        /// 총 런 횟수
        /// </summary>
        public int totalRuns;

        /// <summary>
        /// 최고 도달 스테이지
        /// </summary>
        public int highestStage;

        /// <summary>
        /// 총 처치한 적 수
        /// </summary>
        public int totalEnemiesKilled;

        /// <summary>
        /// 총 사망 횟수
        /// </summary>
        public int totalDeaths;

        /// <summary>
        /// 클리어 횟수
        /// </summary>
        public int totalClears;


        // ====== 메타데이터 ======

        /// <summary>
        /// 저장 시간
        /// </summary>
        public string saveTime;

        /// <summary>
        /// 저장 버전 (마이그레이션용)
        /// </summary>
        public int saveVersion = 1;


        // ====== 생성자 ======

        public PlayerMetaProgress()
        {
            bone = 0;
            soul = 0;
            upgradeLevels = new List<UpgradeLevelEntry>();
            unlockedForms = new List<string>();
            unlockedItems = new List<string>();
            completedAchievements = new List<string>();
            achievementProgress = new List<AchievementProgressEntry>();
            totalPlayTime = 0f;
            totalRuns = 0;
            highestStage = 0;
            totalEnemiesKilled = 0;
            totalDeaths = 0;
            totalClears = 0;
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveVersion = 1;
        }


        // ====== 업그레이드 레벨 관리 ======

        /// <summary>
        /// 업그레이드 레벨 가져오기
        /// </summary>
        public int GetUpgradeLevel(string upgradeId)
        {
            var entry = upgradeLevels.Find(e => e.upgradeId == upgradeId);
            return entry != null ? entry.level : 0;
        }

        /// <summary>
        /// 업그레이드 레벨 설정
        /// </summary>
        public void SetUpgradeLevel(string upgradeId, int level)
        {
            var entry = upgradeLevels.Find(e => e.upgradeId == upgradeId);
            if (entry != null)
            {
                entry.level = level;
            }
            else
            {
                upgradeLevels.Add(new UpgradeLevelEntry(upgradeId, level));
            }
        }


        // ====== 업적 진행도 관리 ======

        /// <summary>
        /// 업적 진행도 가져오기
        /// </summary>
        public int GetAchievementProgress(string achievementId)
        {
            var entry = achievementProgress.Find(e => e.achievementId == achievementId);
            return entry != null ? entry.progress : 0;
        }

        /// <summary>
        /// 업적 진행도 설정
        /// </summary>
        public void SetAchievementProgress(string achievementId, int progress)
        {
            var entry = achievementProgress.Find(e => e.achievementId == achievementId);
            if (entry != null)
            {
                entry.progress = progress;
            }
            else
            {
                achievementProgress.Add(new AchievementProgressEntry(achievementId, progress));
            }
        }


        // ====== 해금 확인 ======

        /// <summary>
        /// 폼이 해금되었는지 확인
        /// </summary>
        public bool IsFormUnlocked(string formId)
        {
            return unlockedForms.Contains(formId);
        }

        /// <summary>
        /// 아이템이 해금되었는지 확인
        /// </summary>
        public bool IsItemUnlocked(string itemId)
        {
            return unlockedItems.Contains(itemId);
        }

        /// <summary>
        /// 업적이 완료되었는지 확인
        /// </summary>
        public bool IsAchievementCompleted(string achievementId)
        {
            return completedAchievements.Contains(achievementId);
        }


        // ====== 저장 전 업데이트 ======

        /// <summary>
        /// 저장 전 메타데이터 업데이트
        /// </summary>
        public void UpdateSaveTime()
        {
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }


        // ====== 디버그 ======

        public override string ToString()
        {
            return $"[PlayerMetaProgress] Bone: {bone}, Soul: {soul}, " +
                   $"Upgrades: {upgradeLevels.Count}, Forms: {unlockedForms.Count}, " +
                   $"Items: {unlockedItems.Count}, Achievements: {completedAchievements.Count}, " +
                   $"Runs: {totalRuns}, HighestStage: {highestStage}";
        }
    }


    /// <summary>
    /// 업그레이드 레벨 엔트리 (Dictionary 대체, JSON 직렬화용)
    /// </summary>
    [Serializable]
    public class UpgradeLevelEntry
    {
        public string upgradeId;
        public int level;

        public UpgradeLevelEntry() { }

        public UpgradeLevelEntry(string upgradeId, int level)
        {
            this.upgradeId = upgradeId;
            this.level = level;
        }
    }


    /// <summary>
    /// 업적 진행도 엔트리 (Dictionary 대체, JSON 직렬화용)
    /// </summary>
    [Serializable]
    public class AchievementProgressEntry
    {
        public string achievementId;
        public int progress;

        public AchievementProgressEntry() { }

        public AchievementProgressEntry(string achievementId, int progress)
        {
            this.achievementId = achievementId;
            this.progress = progress;
        }
    }
}
