using System;
using System.Collections.Generic;
using UnityEngine;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 스테이지 설정 ScriptableObject
    /// 여러 던전(층)을 포함하는 전체 스테이지 구성
    /// </summary>
    [CreateAssetMenu(fileName = "StageConfig", menuName = "GASPT/Level/Stage Config")]
    public class StageConfig : ScriptableObject
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("스테이지 이름")]
        public string stageName = "New Stage";

        [Tooltip("스테이지 ID (고유 식별자)")]
        public string stageId = "";

        [Tooltip("스테이지 설명")]
        [TextArea(3, 5)]
        public string description = "";

        [Tooltip("스테이지 아이콘")]
        public Sprite stageIcon;

        [Tooltip("스테이지 배경 이미지")]
        public Sprite backgroundImage;


        // ====== 던전/층 구성 ======

        [Header("던전 구성")]
        [Tooltip("스테이지에 포함된 던전(층) 목록")]
        public StageFloorConfig[] floors;


        // ====== 난이도 설정 ======

        [Header("난이도")]
        [Tooltip("기본 난이도 (1-10)")]
        [Range(1, 10)]
        public int baseDifficulty = 1;

        [Tooltip("층당 난이도 증가량")]
        [Range(0f, 2f)]
        public float difficultyPerFloor = 0.5f;

        [Tooltip("권장 플레이어 레벨")]
        [Range(1, 100)]
        public int recommendedLevel = 1;


        // ====== 보상 설정 ======

        [Header("보상")]
        [Tooltip("스테이지 클리어 보상 골드 기본값")]
        public int baseClearGold = 100;

        [Tooltip("스테이지 클리어 보상 경험치 기본값")]
        public int baseClearExp = 50;

        [Tooltip("층당 보상 증가 배율")]
        [Range(1f, 2f)]
        public float rewardMultiplierPerFloor = 1.1f;


        // ====== 특수 규칙 ======

        [Header("특수 규칙")]
        [Tooltip("시간 제한 (0이면 무제한)")]
        public float timeLimit = 0f;

        [Tooltip("플레이어 부활 가능 여부")]
        public bool allowRevive = true;

        [Tooltip("최대 부활 횟수")]
        public int maxRevives = 1;

        [Tooltip("중간 저장 가능 여부")]
        public bool allowCheckpoint = true;


        // ====== 해금 조건 ======

        [Header("해금 조건")]
        [Tooltip("선행 스테이지 ID (비어있으면 바로 플레이 가능)")]
        public string[] prerequisiteStageIds;

        [Tooltip("필요 플레이어 레벨")]
        public int requiredLevel = 1;


        // ====== 헬퍼 메서드 ======

        /// <summary>
        /// 총 층 수
        /// </summary>
        public int TotalFloors => floors?.Length ?? 0;

        /// <summary>
        /// 특정 층의 던전 설정 가져오기
        /// </summary>
        public DungeonConfig GetDungeonConfig(int floorIndex)
        {
            if (floors == null || floorIndex < 0 || floorIndex >= floors.Length)
                return null;

            return floors[floorIndex].dungeonConfig;
        }

        /// <summary>
        /// 특정 층의 난이도 계산
        /// </summary>
        public float GetFloorDifficulty(int floorIndex)
        {
            return baseDifficulty + (floorIndex * difficultyPerFloor);
        }

        /// <summary>
        /// 특정 층의 보상 배율 계산
        /// </summary>
        public float GetFloorRewardMultiplier(int floorIndex)
        {
            return Mathf.Pow(rewardMultiplierPerFloor, floorIndex);
        }

        /// <summary>
        /// 유효성 검증
        /// </summary>
        private void OnValidate()
        {
            // stageId 자동 생성
            if (string.IsNullOrEmpty(stageId))
            {
                stageId = stageName.Replace(" ", "_").ToLower();
            }

            // 층 검증
            if (floors != null)
            {
                for (int i = 0; i < floors.Length; i++)
                {
                    if (floors[i].dungeonConfig == null)
                    {
                        Debug.LogWarning($"[StageConfig] {stageName}: 층 {i}의 dungeonConfig가 없습니다!");
                    }
                }
            }
        }
    }


    /// <summary>
    /// 스테이지 내 층 설정
    /// </summary>
    [Serializable]
    public class StageFloorConfig
    {
        [Tooltip("층 이름 (예: '1층', 'B1', '보스층')")]
        public string floorName = "Floor";

        [Tooltip("이 층의 던전 설정")]
        public DungeonConfig dungeonConfig;

        [Tooltip("이 층의 테마/환경")]
        public string environmentTheme = "Default";

        [Tooltip("배경 음악")]
        public AudioClip bgm;

        [Tooltip("층 진입 조건 (이전 층 클리어 외 추가 조건)")]
        public FloorEntryCondition entryCondition;

        [Tooltip("이 층이 보스 층인지")]
        public bool isBossFloor;

        [Tooltip("이 층이 휴식 층인지")]
        public bool isRestFloor;
    }


    /// <summary>
    /// 층 진입 조건
    /// </summary>
    [Serializable]
    public class FloorEntryCondition
    {
        [Tooltip("필요 아이템 ID")]
        public string requiredItemId;

        [Tooltip("필요 골드")]
        public int requiredGold;

        [Tooltip("특수 조건 설명")]
        public string conditionDescription;

        /// <summary>
        /// 조건 충족 여부 확인
        /// </summary>
        public bool IsMet()
        {
            // TODO: 실제 조건 검사 구현
            // 예: InventorySystem.Instance.HasItem(requiredItemId)
            // 예: PlayerStats.Instance.Gold >= requiredGold

            if (!string.IsNullOrEmpty(requiredItemId))
            {
                // 아이템 체크
                return false; // TODO: 구현
            }

            if (requiredGold > 0)
            {
                // 골드 체크
                return false; // TODO: 구현
            }

            return true;
        }
    }
}
