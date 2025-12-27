using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// Procedural 던전 생성 룰
    /// 방 개수, 난이도 곡선, 특수 방 배치 등을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "RoomGenerationRules", menuName = "GASPT/Level/Room Generation Rules")]
    public class RoomGenerationRules : ScriptableObject
    {
        // ====== 방 개수 ======

        [Header("방 개수")]
        [Tooltip("최소 방 개수 (보스 방 제외)")]
        [Range(1, 20)]
        public int minRooms = 3;

        [Tooltip("최대 방 개수 (보스 방 제외)")]
        [Range(1, 20)]
        public int maxRooms = 7;

        [Tooltip("보스 방 포함 여부")]
        public bool includeBossRoom = true;


        // ====== 난이도 곡선 ======

        [Header("난이도 곡선")]
        [Tooltip("난이도 증가 곡선 (AnimationCurve)")]
        public AnimationCurve difficultyCurve = AnimationCurve.Linear(0, 1, 1, 10);

        [Tooltip("기본 난이도 배율")]
        [Range(0.5f, 3f)]
        public float difficultyMultiplier = 1f;


        // ====== 특수 방 확률 ======

        [Header("특수 방 확률")]
        [Tooltip("엘리트 방 확률 (0~1)")]
        [Range(0f, 1f)]
        public float eliteRoomChance = 0.2f;

        [Tooltip("휴식 방 확률 (0~1)")]
        [Range(0f, 1f)]
        public float restRoomChance = 0.1f;

        [Tooltip("상점 방 확률 (0~1)")]
        [Range(0f, 1f)]
        public float shopRoomChance = 0.15f;

        [Tooltip("보물 방 확률 (0~1)")]
        [Range(0f, 1f)]
        public float treasureRoomChance = 0.1f;


        // ====== 배치 규칙 ======

        [Header("배치 규칙")]
        [Tooltip("첫 번째 방은 항상 쉬운 방")]
        public bool firstRoomAlwaysEasy = true;

        [Tooltip("마지막 방 전에 휴식 방 배치")]
        public bool restRoomBeforeBoss = true;

        [Tooltip("연속 특수 방 방지 (최소 일반 방 개수)")]
        [Range(0, 5)]
        public int minNormalRoomsBetweenSpecial = 1;


        // ====== 그래프 생성 규칙 ======

        [Header("그래프 생성 규칙")]
        [Tooltip("총 층 수 (Entry 포함)")]
        [Range(3, 20)]
        public int totalFloors = 10;

        [Tooltip("분기 생성 확률 (0~1)")]
        [Range(0f, 1f)]
        public float branchingFactor = 0.4f;

        [Tooltip("층당 최대 분기 수")]
        [Range(1, 3)]
        public int maxBranches = 2;

        [Tooltip("Entry→Boss 최소 경로 길이")]
        [Range(3, 15)]
        public int minPathLength = 5;

        [Tooltip("Entry→Boss 최대 경로 길이")]
        [Range(5, 20)]
        public int maxPathLength = 10;

        [Tooltip("비밀 방 생성 확률 (0~1)")]
        [Range(0f, 1f)]
        public float secretRoomChance = 0.1f;

        [Tooltip("층당 최소 노드 수")]
        [Range(1, 3)]
        public int minNodesPerFloor = 1;

        [Tooltip("층당 최대 노드 수")]
        [Range(1, 4)]
        public int maxNodesPerFloor = 3;


        // ====== 방 타입 비율 ======

        [Header("방 타입 비율")]
        [Tooltip("엘리트 방 비율 (0~1)")]
        [Range(0f, 0.5f)]
        public float eliteRoomRatio = 0.15f;

        [Tooltip("상점 방 비율 (0~1)")]
        [Range(0f, 0.5f)]
        public float shopRoomRatio = 0.1f;

        [Tooltip("휴식 방 비율 (0~1)")]
        [Range(0f, 0.5f)]
        public float restRoomRatio = 0.1f;

        [Tooltip("보물 방 비율 (0~1)")]
        [Range(0f, 0.5f)]
        public float treasureRoomRatio = 0.05f;


        // ====== 유효성 검증 ======

        private void OnValidate()
        {
            // 최소 방 개수가 최대보다 크면 조정
            if (minRooms > maxRooms)
            {
                minRooms = maxRooms;
            }

            // 확률 합계 경고
            float totalChance = eliteRoomChance + restRoomChance + shopRoomChance + treasureRoomChance;
            if (totalChance > 1f)
            {
                Debug.LogWarning($"[RoomGenerationRules] 특수 방 확률 합계가 1을 초과합니다! (현재: {totalChance:F2})");
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 방 진행도(0~1)에 따른 난이도 계산
        /// </summary>
        public int GetDifficultyForProgress(float progress)
        {
            float curveValue = difficultyCurve.Evaluate(progress);
            return Mathf.RoundToInt(curveValue * difficultyMultiplier);
        }

        /// <summary>
        /// 특수 방 타입 랜덤 선택
        /// </summary>
        public RoomType GetRandomSpecialRoomType()
        {
            float roll = Random.value;
            float cumulative = 0f;

            cumulative += eliteRoomChance;
            if (roll < cumulative) return RoomType.Elite;

            cumulative += restRoomChance;
            if (roll < cumulative) return RoomType.Rest;

            cumulative += shopRoomChance;
            if (roll < cumulative) return RoomType.Shop;

            cumulative += treasureRoomChance;
            if (roll < cumulative) return RoomType.Treasure;

            // 기본값: 일반 방
            return RoomType.Normal;
        }
    }
}
