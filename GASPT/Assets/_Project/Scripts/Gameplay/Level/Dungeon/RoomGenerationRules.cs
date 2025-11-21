using UnityEngine;

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
