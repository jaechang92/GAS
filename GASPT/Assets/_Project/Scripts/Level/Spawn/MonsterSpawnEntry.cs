using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Enums;

namespace GASPT.Level.Spawn
{
    /// <summary>
    /// 몬스터 스폰 엔트리
    /// 스폰 테이블의 각 항목을 정의
    /// </summary>
    [Serializable]
    public class MonsterSpawnEntry
    {
        [Tooltip("스폰할 몬스터 데이터")]
        public EnemyData enemyData;

        [Tooltip("스폰 가중치 (높을수록 자주 등장)")]
        [Range(1, 100)]
        public int weight = 10;

        [Tooltip("최소 스폰 수 (한 번에 스폰되는 최소 수)")]
        [Range(1, 5)]
        public int minSpawnCount = 1;

        [Tooltip("최대 스폰 수 (한 번에 스폰되는 최대 수)")]
        [Range(1, 5)]
        public int maxSpawnCount = 1;

        [Tooltip("특정 방 타입에서만 스폰 (비어있으면 모든 방)")]
        public RoomType[] restrictToRoomTypes;

        [Tooltip("이 엔트리가 활성화되어 있는지")]
        public bool isEnabled = true;

        /// <summary>
        /// 랜덤 스폰 수 반환
        /// </summary>
        public int GetRandomSpawnCount()
        {
            return UnityEngine.Random.Range(minSpawnCount, maxSpawnCount + 1);
        }

        /// <summary>
        /// 특정 방 타입에서 스폰 가능한지 확인
        /// </summary>
        public bool CanSpawnInRoom(RoomType roomType)
        {
            if (!isEnabled) return false;
            if (enemyData == null) return false;

            // 제한이 없으면 모든 방에서 스폰 가능
            if (restrictToRoomTypes == null || restrictToRoomTypes.Length == 0)
            {
                return true;
            }

            // 제한된 방 타입 체크
            foreach (var allowedType in restrictToRoomTypes)
            {
                if (allowedType == roomType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 유효성 검사
        /// </summary>
        public bool IsValid()
        {
            return isEnabled && enemyData != null && weight > 0;
        }

        public override string ToString()
        {
            string name = enemyData != null ? enemyData.enemyName : "null";
            return $"[SpawnEntry] {name} (Weight: {weight}, Count: {minSpawnCount}-{maxSpawnCount})";
        }
    }

    /// <summary>
    /// Named/Boss 전용 스폰 엔트리
    /// 등장 조건이 더 복잡함
    /// </summary>
    [Serializable]
    public class NamedSpawnEntry : MonsterSpawnEntry
    {
        [Tooltip("등장 가능한 최소 방 번호 (예: 3 = 3번째 방부터)")]
        [Range(1, 10)]
        public int minRoomNumber = 3;

        [Tooltip("스테이지당 최대 등장 횟수")]
        [Range(1, 5)]
        public int maxSpawnsPerStage = 1;

        [Tooltip("등장 확률 (0-100%)")]
        [Range(0, 100)]
        public int spawnChance = 50;

        /// <summary>
        /// 특정 방에서 스폰 가능한지 확인 (방 번호 조건 포함)
        /// </summary>
        public bool CanSpawnInRoomNumber(int roomNumber, int currentSpawnCount)
        {
            if (!IsValid()) return false;
            if (roomNumber < minRoomNumber) return false;
            if (currentSpawnCount >= maxSpawnsPerStage) return false;

            // 확률 체크
            return UnityEngine.Random.Range(0, 100) < spawnChance;
        }
    }

    /// <summary>
    /// 보스 스폰 엔트리
    /// </summary>
    [Serializable]
    public class BossSpawnEntry
    {
        [Tooltip("보스 몬스터 데이터")]
        public EnemyData bossData;

        [Tooltip("등장 스테이지")]
        [Range(1, 5)]
        public int stage = 1;

        [Tooltip("보스 등급")]
        public BossGrade bossGrade = BossGrade.MiniBoss;

        [Tooltip("등장하는 방 번호 (예: 5 = 5번째 방)")]
        [Range(1, 10)]
        public int roomNumber = 5;

        /// <summary>
        /// 유효성 검사
        /// </summary>
        public bool IsValid()
        {
            return bossData != null && bossData.enemyType == EnemyType.Boss;
        }
    }
}
