using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Enums;

namespace GASPT.Level.Spawn
{
    /// <summary>
    /// 몬스터 스폰 테이블 ScriptableObject
    /// 스테이지별 몬스터 구성을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnTable_Stage", menuName = "GASPT/Level/Monster Spawn Table")]
    public class MonsterSpawnTable : ScriptableObject
    {
        [Header("스테이지 정보")]
        [Tooltip("이 스폰 테이블이 적용되는 스테이지")]
        [Range(1, 5)]
        public int stage = 1;

        [Tooltip("스테이지 이름")]
        public string stageName = "숲의 유적";

        [Tooltip("스테이지 테마 설명")]
        [TextArea(2, 4)]
        public string themeDescription;


        [Header("일반 몬스터 (Normal)")]
        [Tooltip("일반 방에서 스폰되는 몬스터들")]
        public List<MonsterSpawnEntry> normalMonsters = new List<MonsterSpawnEntry>();


        [Header("네임드 몬스터 (Named)")]
        [Tooltip("엘리트 방에서 스폰되는 강화된 몬스터들")]
        public List<NamedSpawnEntry> namedMonsters = new List<NamedSpawnEntry>();


        [Header("보스 몬스터 (Boss)")]
        [Tooltip("보스 방에서 스폰되는 보스")]
        public BossSpawnEntry stageBoss;


        [Header("스폰 설정")]
        [Tooltip("일반 방당 스폰되는 몬스터 수 (최소)")]
        [Range(1, 10)]
        public int minMonstersPerRoom = 3;

        [Tooltip("일반 방당 스폰되는 몬스터 수 (최대)")]
        [Range(1, 15)]
        public int maxMonstersPerRoom = 5;

        [Tooltip("엘리트 방당 추가 몬스터 수")]
        [Range(0, 5)]
        public int eliteRoomExtraMonsters = 2;


        // 가중치 총합 (캐싱)
        private int totalNormalWeight = -1;

        /// <summary>
        /// 일반 몬스터 가중치 기반 랜덤 선택
        /// </summary>
        /// <param name="roomType">방 타입</param>
        /// <returns>선택된 몬스터 데이터</returns>
        public EnemyData GetRandomNormalMonster(RoomType roomType = RoomType.Normal)
        {
            // 유효한 엔트리만 필터링
            var validEntries = new List<MonsterSpawnEntry>();
            int totalWeight = 0;

            foreach (var entry in normalMonsters)
            {
                if (entry.IsValid() && entry.CanSpawnInRoom(roomType))
                {
                    validEntries.Add(entry);
                    totalWeight += entry.weight;
                }
            }

            if (validEntries.Count == 0 || totalWeight <= 0)
            {
                Debug.LogWarning($"[MonsterSpawnTable] {stageName}: 유효한 일반 몬스터가 없습니다.");
                return null;
            }

            // 가중치 기반 랜덤 선택
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in validEntries)
            {
                currentWeight += entry.weight;
                if (randomValue < currentWeight)
                {
                    return entry.enemyData;
                }
            }

            // 폴백
            return validEntries[validEntries.Count - 1].enemyData;
        }

        /// <summary>
        /// 일반 몬스터 스폰 리스트 생성 (방 하나분)
        /// </summary>
        /// <param name="roomType">방 타입</param>
        /// <returns>스폰할 몬스터 데이터 리스트</returns>
        public List<EnemyData> GetSpawnListForRoom(RoomType roomType)
        {
            var spawnList = new List<EnemyData>();

            // 방당 몬스터 수 결정
            int monsterCount = Random.Range(minMonstersPerRoom, maxMonstersPerRoom + 1);

            // 엘리트 방이면 추가
            if (roomType == RoomType.Elite)
            {
                monsterCount += eliteRoomExtraMonsters;
            }

            // 몬스터 선택
            for (int i = 0; i < monsterCount; i++)
            {
                var monster = GetRandomNormalMonster(roomType);
                if (monster != null)
                {
                    spawnList.Add(monster);
                }
            }

            return spawnList;
        }

        /// <summary>
        /// Named 몬스터 스폰 가능 여부 및 선택
        /// </summary>
        /// <param name="roomNumber">현재 방 번호</param>
        /// <param name="spawnedCounts">이미 스폰된 Named 몬스터 카운트 (ID별)</param>
        /// <returns>스폰할 Named 몬스터 (없으면 null)</returns>
        public EnemyData TryGetNamedMonster(int roomNumber, Dictionary<string, int> spawnedCounts)
        {
            foreach (var entry in namedMonsters)
            {
                if (!entry.IsValid()) continue;

                string id = entry.enemyData.name;
                int currentCount = spawnedCounts.TryGetValue(id, out int count) ? count : 0;

                if (entry.CanSpawnInRoomNumber(roomNumber, currentCount))
                {
                    // 카운트 업데이트
                    spawnedCounts[id] = currentCount + 1;
                    return entry.enemyData;
                }
            }

            return null;
        }

        /// <summary>
        /// 보스 몬스터 반환
        /// </summary>
        public EnemyData GetBoss()
        {
            if (stageBoss != null && stageBoss.IsValid())
            {
                return stageBoss.bossData;
            }

            Debug.LogWarning($"[MonsterSpawnTable] {stageName}: 유효한 보스가 없습니다.");
            return null;
        }

        /// <summary>
        /// 전체 몬스터 목록 반환 (에디터/디버그용)
        /// </summary>
        public List<EnemyData> GetAllMonsters()
        {
            var allMonsters = new List<EnemyData>();

            foreach (var entry in normalMonsters)
            {
                if (entry.enemyData != null && !allMonsters.Contains(entry.enemyData))
                {
                    allMonsters.Add(entry.enemyData);
                }
            }

            foreach (var entry in namedMonsters)
            {
                if (entry.enemyData != null && !allMonsters.Contains(entry.enemyData))
                {
                    allMonsters.Add(entry.enemyData);
                }
            }

            if (stageBoss?.bossData != null && !allMonsters.Contains(stageBoss.bossData))
            {
                allMonsters.Add(stageBoss.bossData);
            }

            return allMonsters;
        }

        /// <summary>
        /// 스폰 테이블 검증 (에디터용)
        /// </summary>
        private void OnValidate()
        {
            // 가중치 캐시 무효화
            totalNormalWeight = -1;

            // 최소/최대 검증
            if (minMonstersPerRoom > maxMonstersPerRoom)
            {
                minMonstersPerRoom = maxMonstersPerRoom;
            }

            // 보스 검증
            if (stageBoss != null && stageBoss.bossData != null)
            {
                if (stageBoss.bossData.enemyType != EnemyType.Boss)
                {
                    Debug.LogWarning($"[MonsterSpawnTable] {stageName}: 보스 슬롯에 Boss 타입이 아닌 적이 설정되어 있습니다.");
                }
            }

            // 일반 몬스터 검증
            foreach (var entry in normalMonsters)
            {
                if (entry.enemyData != null && entry.enemyData.enemyType != EnemyType.Normal)
                {
                    Debug.LogWarning($"[MonsterSpawnTable] {stageName}: 일반 몬스터 슬롯에 Normal 타입이 아닌 적이 있습니다: {entry.enemyData.enemyName}");
                }
            }

            // Named 몬스터 검증
            foreach (var entry in namedMonsters)
            {
                if (entry.enemyData != null && entry.enemyData.enemyType != EnemyType.Named)
                {
                    Debug.LogWarning($"[MonsterSpawnTable] {stageName}: Named 몬스터 슬롯에 Named 타입이 아닌 적이 있습니다: {entry.enemyData.enemyName}");
                }
            }
        }
    }
}
