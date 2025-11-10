using UnityEngine;
using GASPT.Data;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 방(Room) 데이터 ScriptableObject
    /// 방의 적 스폰 정보, 클리어 조건, 보상 등을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "RoomData", menuName = "GASPT/Level/Room Data")]
    public class RoomData : ScriptableObject
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("방 이름")]
        public string roomName = "New Room";

        [Tooltip("방 타입")]
        public RoomType roomType = RoomType.Normal;

        [Tooltip("난이도 (1~10)")]
        [Range(1, 10)]
        public int difficulty = 1;


        // ====== 적 스폰 설정 ======

        [Header("적 스폰 설정")]
        [Tooltip("스폰할 적 데이터 목록")]
        public EnemySpawnData[] enemySpawns;

        [Tooltip("최소 스폰 수 (랜덤 스폰 시)")]
        [Range(0, 20)]
        public int minEnemyCount = 2;

        [Tooltip("최대 스폰 수 (랜덤 스폰 시)")]
        [Range(1, 20)]
        public int maxEnemyCount = 5;


        // ====== 클리어 조건 ======

        [Header("클리어 조건")]
        [Tooltip("클리어 조건")]
        public ClearCondition clearCondition = ClearCondition.KillAllEnemies;

        [Tooltip("시간 제한 (초, 0이면 무제한)")]
        [Range(0, 600)]
        public float timeLimit = 0f;


        // ====== 보상 ======

        [Header("보상")]
        [Tooltip("클리어 시 보너스 골드")]
        [Range(0, 1000)]
        public int bonusGold = 50;

        [Tooltip("클리어 시 보너스 경험치")]
        [Range(0, 500)]
        public int bonusExp = 20;


        // ====== 유효성 검증 ======

        private void OnValidate()
        {
            // 최소 적 수가 최대보다 크면 조정
            if (minEnemyCount > maxEnemyCount)
            {
                minEnemyCount = maxEnemyCount;
                Debug.LogWarning($"[RoomData] {roomName}: minEnemyCount가 maxEnemyCount보다 큽니다. 조정했습니다.");
            }

            // 보스 방 경고
            if (roomType == RoomType.Boss && (enemySpawns == null || enemySpawns.Length == 0))
            {
                Debug.LogWarning($"[RoomData] {roomName}: Boss 방인데 적 스폰 데이터가 없습니다!");
            }
        }


        // ====== 디버그 정보 ======

        public override string ToString()
        {
            return $"[RoomData] {roomName} ({roomType}): " +
                   $"Difficulty={difficulty}, Enemies={minEnemyCount}~{maxEnemyCount}";
        }
    }


    // ====== 열거형 ======

    /// <summary>
    /// 방 타입
    /// </summary>
    public enum RoomType
    {
        Start,      // 시작 방
        Normal,     // 일반 전투 방
        Elite,      // 엘리트 전투 방
        Boss,       // 보스 방
        Rest,       // 휴식 방 (회복)
        Shop,       // 상점 방
        Treasure    // 보물 방
    }

    /// <summary>
    /// 클리어 조건
    /// </summary>
    public enum ClearCondition
    {
        KillAllEnemies,     // 모든 적 처치
        Survival,           // 생존 (시간 제한)
        BossKill,           // 보스 처치
        Automatic           // 자동 (시작 방, 휴식 방 등)
    }


    // ====== 적 스폰 데이터 ======

    /// <summary>
    /// 적 스폰 정보
    /// </summary>
    [System.Serializable]
    public class EnemySpawnData
    {
        [Tooltip("스폰할 적 데이터")]
        public EnemyData enemyData;

        [Tooltip("스폰 확률 (0~1, 0이면 항상 스폰)")]
        [Range(0f, 1f)]
        public float spawnChance = 1f;

        [Tooltip("스폰 수량 (최소)")]
        [Range(1, 10)]
        public int minCount = 1;

        [Tooltip("스폰 수량 (최대)")]
        [Range(1, 10)]
        public int maxCount = 1;

        /// <summary>
        /// 이 적을 스폰할지 확률 체크
        /// </summary>
        public bool ShouldSpawn()
        {
            // spawnChance가 0이면 항상 스폰
            if (spawnChance <= 0f) return true;

            return Random.value <= spawnChance;
        }

        /// <summary>
        /// 랜덤 스폰 수량 계산
        /// </summary>
        public int GetRandomCount()
        {
            return Random.Range(minCount, maxCount + 1);
        }
    }
}
