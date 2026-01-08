using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Enums;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Enemies;
using GASPT.Combat;

namespace GASPT.Level.Spawn
{
    /// <summary>
    /// 몬스터 스폰 매니저
    /// 스폰 테이블을 기반으로 몬스터를 생성하고 관리
    /// </summary>
    public class MonsterSpawnManager : MonoBehaviour
    {
        // 싱글톤
        private static MonsterSpawnManager instance;
        public static MonsterSpawnManager Instance => instance;
        public static bool HasInstance => instance != null;


        [Header("스폰 테이블")]
        [Tooltip("스테이지별 스폰 테이블 (인덱스 0 = Stage 1)")]
        [SerializeField] private MonsterSpawnTable[] spawnTables;


        [Header("현재 상태")]
        [SerializeField] private int currentStage = 1;
        [SerializeField] private int currentDifficulty = 1; // 0=Easy, 1=Normal, 2=Hard


        [Header("스폰 설정")]
        [Tooltip("스폰 시 스테이지 스케일링 적용")]
        [SerializeField] private bool applyStageScaling = true;

        [Tooltip("스폰된 몬스터의 부모 Transform")]
        [SerializeField] private Transform monstersParent;


        // Named 몬스터 스폰 추적 (스테이지 내)
        private Dictionary<string, int> namedSpawnCounts = new Dictionary<string, int>();

        // 현재 스테이지의 활성 몬스터들
        private List<Enemy> activeMonsters = new List<Enemy>();


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }


        // ====== 스테이지 관리 ======

        /// <summary>
        /// 새 스테이지 시작
        /// </summary>
        public void StartStage(int stage)
        {
            currentStage = Mathf.Clamp(stage, 1, 5);
            namedSpawnCounts.Clear();
            activeMonsters.Clear();

            Debug.Log($"[MonsterSpawnManager] Stage {currentStage} 시작");
        }

        /// <summary>
        /// 난이도 설정
        /// </summary>
        public void SetDifficulty(int difficulty)
        {
            currentDifficulty = Mathf.Clamp(difficulty, 0, 2);
            Debug.Log($"[MonsterSpawnManager] 난이도 설정: {(difficulty == 0 ? "Easy" : difficulty == 1 ? "Normal" : "Hard")}");
        }


        // ====== 스폰 테이블 접근 ======

        /// <summary>
        /// 현재 스테이지의 스폰 테이블 반환
        /// </summary>
        public MonsterSpawnTable GetCurrentSpawnTable()
        {
            int index = currentStage - 1;
            if (spawnTables == null || index < 0 || index >= spawnTables.Length)
            {
                Debug.LogError($"[MonsterSpawnManager] Stage {currentStage}의 스폰 테이블이 없습니다.");
                return null;
            }
            return spawnTables[index];
        }

        /// <summary>
        /// 특정 스테이지의 스폰 테이블 반환
        /// </summary>
        public MonsterSpawnTable GetSpawnTable(int stage)
        {
            int index = stage - 1;
            if (spawnTables == null || index < 0 || index >= spawnTables.Length)
            {
                return null;
            }
            return spawnTables[index];
        }


        // ====== 방 스폰 ======

        /// <summary>
        /// 일반 방에 몬스터 스폰
        /// </summary>
        /// <param name="spawnPoints">스폰 위치들</param>
        /// <param name="roomType">방 타입</param>
        /// <returns>스폰된 몬스터 리스트</returns>
        public List<Enemy> SpawnMonstersForRoom(Transform[] spawnPoints, RoomType roomType = RoomType.Normal)
        {
            var spawnedMonsters = new List<Enemy>();
            var table = GetCurrentSpawnTable();

            if (table == null || spawnPoints == null || spawnPoints.Length == 0)
            {
                return spawnedMonsters;
            }

            // 스폰할 몬스터 데이터 리스트 가져오기
            var monstersToSpawn = table.GetSpawnListForRoom(roomType);

            // 스폰 위치 인덱스
            int spawnIndex = 0;

            foreach (var enemyData in monstersToSpawn)
            {
                if (enemyData == null) continue;

                // 스폰 위치 순환
                Transform spawnPoint = spawnPoints[spawnIndex % spawnPoints.Length];
                spawnIndex++;

                // 몬스터 스폰
                var monster = SpawnMonster(enemyData, spawnPoint.position);
                if (monster != null)
                {
                    spawnedMonsters.Add(monster);
                }
            }

            Debug.Log($"[MonsterSpawnManager] 방에 {spawnedMonsters.Count}마리 스폰 완료 (Type: {roomType})");
            return spawnedMonsters;
        }

        /// <summary>
        /// 엘리트 방에 Named 몬스터 포함 스폰
        /// </summary>
        public List<Enemy> SpawnMonstersForEliteRoom(Transform[] spawnPoints, int roomNumber)
        {
            var spawnedMonsters = SpawnMonstersForRoom(spawnPoints, RoomType.Elite);
            var table = GetCurrentSpawnTable();

            if (table == null) return spawnedMonsters;

            // Named 몬스터 스폰 시도
            var namedData = table.TryGetNamedMonster(roomNumber, namedSpawnCounts);
            if (namedData != null && spawnPoints.Length > 0)
            {
                // 중앙 스폰 포인트 선택
                Transform bossSpawn = spawnPoints[spawnPoints.Length / 2];
                var named = SpawnMonster(namedData, bossSpawn.position);
                if (named != null)
                {
                    spawnedMonsters.Add(named);
                    Debug.Log($"[MonsterSpawnManager] Named 몬스터 스폰: {namedData.enemyName}");
                }
            }

            return spawnedMonsters;
        }

        /// <summary>
        /// 보스 방에 보스 스폰
        /// </summary>
        public Enemy SpawnBoss(Vector3 spawnPosition)
        {
            var table = GetCurrentSpawnTable();
            if (table == null) return null;

            var bossData = table.GetBoss();
            if (bossData == null) return null;

            var boss = SpawnMonster(bossData, spawnPosition);
            if (boss != null)
            {
                Debug.Log($"[MonsterSpawnManager] 보스 스폰: {bossData.enemyName}");
            }

            return boss;
        }


        // ====== 개별 몬스터 스폰 ======

        /// <summary>
        /// 단일 몬스터 스폰
        /// </summary>
        public Enemy SpawnMonster(EnemyData data, Vector3 position)
        {
            if (data == null)
            {
                Debug.LogError("[MonsterSpawnManager] EnemyData가 null입니다.");
                return null;
            }

            // 프리팹 경로 결정 (클래스별)
            string prefabPath = GetPrefabPath(data.enemyClass);
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[MonsterSpawnManager] 프리팹을 찾을 수 없습니다: {prefabPath}");
                return null;
            }

            // 오브젝트 풀링 또는 직접 생성
            GameObject monsterObj;
            if (PoolManager.Instance != null)
            {
                monsterObj = PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);
            }
            else
            {
                monsterObj = Instantiate(prefab, position, Quaternion.identity);
            }

            if (monsterObj == null) return null;

            // 부모 설정
            if (monstersParent != null)
            {
                monsterObj.transform.SetParent(monstersParent);
            }

            // Enemy 컴포넌트 가져오기 및 초기화
            var enemy = monsterObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.InitializeWithData(data);

                // 스테이지 스케일링 적용 (옵션)
                if (applyStageScaling)
                {
                    ApplyStageScaling(enemy, data);
                }

                activeMonsters.Add(enemy);
                enemy.OnDeath += OnMonsterDeath;
            }

            return enemy;
        }

        /// <summary>
        /// 클래스별 프리팹 경로 반환
        /// </summary>
        private string GetPrefabPath(EnemyClass enemyClass)
        {
            return enemyClass switch
            {
                EnemyClass.BasicMelee => "Prefabs/Enemies/BasicMeleeEnemy",
                EnemyClass.Ranged => "Prefabs/Enemies/RangedEnemy",
                EnemyClass.Flying => "Prefabs/Enemies/FlyingEnemy",
                EnemyClass.Elite => "Prefabs/Enemies/EliteEnemy",
                EnemyClass.Boss => "Prefabs/Enemies/BossEnemy",
                _ => "Prefabs/Enemies/BasicMeleeEnemy"
            };
        }

        /// <summary>
        /// 스테이지 스케일링 적용
        /// </summary>
        private void ApplyStageScaling(Enemy enemy, EnemyData data)
        {
            if (enemy == null || data == null) return;

            // StageScalingCalculator를 사용하여 스탯 계산
            var scaledStats = StageScalingCalculator.CalculateScaledStats(data, currentStage);

            // 난이도 적용
            scaledStats = StageScalingCalculator.ApplyDifficulty(scaledStats, currentDifficulty);

            // Enemy에 스케일링된 스탯 적용
            enemy.ApplyScaledStats(scaledStats);

            Debug.Log($"[MonsterSpawnManager] {data.enemyName} 스케일링 완료 (Stage {currentStage}, Difficulty {currentDifficulty})");
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 몬스터 사망 시 호출
        /// </summary>
        private void OnMonsterDeath(Enemy enemy)
        {
            activeMonsters.Remove(enemy);
            enemy.OnDeath -= OnMonsterDeath;

            Debug.Log($"[MonsterSpawnManager] 몬스터 사망: {enemy.Data?.enemyName}, 남은 몬스터: {activeMonsters.Count}");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 현재 방의 모든 몬스터가 처치되었는지 확인
        /// </summary>
        public bool AreAllMonstersDead()
        {
            return activeMonsters.Count == 0;
        }

        /// <summary>
        /// 활성 몬스터 수 반환
        /// </summary>
        public int GetActiveMonsterCount()
        {
            return activeMonsters.Count;
        }

        /// <summary>
        /// 모든 활성 몬스터 제거
        /// </summary>
        public void ClearAllMonsters()
        {
            foreach (var monster in activeMonsters)
            {
                if (monster != null && monster.gameObject != null)
                {
                    monster.OnDeath -= OnMonsterDeath;

                    if (PoolManager.Instance != null)
                    {
                        PoolManager.Instance.Despawn(monster.gameObject);
                    }
                    else
                    {
                        Destroy(monster.gameObject);
                    }
                }
            }

            activeMonsters.Clear();
            Debug.Log("[MonsterSpawnManager] 모든 몬스터 제거됨");
        }
    }
}
