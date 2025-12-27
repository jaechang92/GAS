using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Core;
using GASPT.Core.Enums;
using GASPT.Gameplay.Enemies;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 방(Room) MonoBehaviour
    /// 방의 적 스폰, 클리어 조건, 상태 관리
    /// </summary>
    public class Room : MonoBehaviour
    {
        // ====== 방 데이터 ======

        [Header("방 설정")]
        [SerializeField] private RoomData roomData;


        // ====== 스폰 포인트 ======

        [Header("스폰 포인트")]
        [Tooltip("자식 오브젝트에서 자동 찾기")]
        [SerializeField] private bool autoFindSpawnPoints = true;

        [Tooltip("수동 할당 (autoFindSpawnPoints가 false일 때)")]
        [SerializeField] private EnemySpawnPoint[] manualSpawnPoints;

        private EnemySpawnPoint[] spawnPoints;


        // ====== 상태 ======

        private RoomState currentState = RoomState.Inactive;
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private int aliveEnemyCount = 0;
        private float roomStartTime;
        private CancellationTokenSource roomCts;


        // ====== 이벤트 ======

        public event Action<Room> OnRoomEnter;
        public event Action<Room> OnRoomClear;
        public event Action<Room, string> OnRoomFail;
        public event Action<Room, int> OnEnemyCountChanged;


        // ====== 프로퍼티 ======

        public RoomData Data => roomData;
        public RoomState State => currentState;
        public int AliveEnemyCount => aliveEnemyCount;
        public bool IsCleared => currentState == RoomState.Cleared;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            InitializeSpawnPoints();
        }

        private void OnDestroy()
        {
            roomCts?.Cancel();
            roomCts?.Dispose();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 스폰 포인트 초기화
        /// </summary>
        private void InitializeSpawnPoints()
        {
            if (autoFindSpawnPoints)
            {
                spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();
                Debug.Log($"[Room] {name}: {spawnPoints.Length}개의 스폰 포인트 자동 탐색");
            }
            else
            {
                spawnPoints = manualSpawnPoints;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning($"[Room] {name}: 스폰 포인트가 없습니다!");
            }
        }

        /// <summary>
        /// RoomData로 Room 초기화 (동적 생성 시 사용)
        /// </summary>
        public void Initialize(RoomData data)
        {
            if (data == null)
            {
                Debug.LogError($"[Room] {name}: RoomData가 null입니다!");
                return;
            }

            // RoomData 할당
            roomData = data;

            // Room 이름 업데이트
            name = $"Room_{data.roomName}";

            Debug.Log($"[Room] {name}: RoomData로 초기화 완료 (Difficulty: {data.difficulty}, Type: {data.roomType})");
        }


        // ====== 방 진입 ======

        /// <summary>
        /// 방 진입 (RoomManager가 호출)
        /// </summary>
        public async Awaitable EnterRoomAsync()
        {
            if (currentState != RoomState.Inactive)
            {
                Debug.LogWarning($"[Room] {name}: 이미 활성화된 방입니다.");
                return;
            }

            ChangeState(RoomState.Entering);

            // 이벤트 발생
            OnRoomEnter?.Invoke(this);

            // 방 입장 연출 (페이드 인 등)
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 적 스폰
            SpawnEnemies();

            // 전투 시작
            ChangeState(RoomState.InProgress);
            roomStartTime = Time.time;

            // CancellationToken 생성
            roomCts?.Cancel();
            roomCts?.Dispose();
            roomCts = new CancellationTokenSource();

            // 클리어 조건 체크 시작
            CheckClearConditionAsync(roomCts.Token).Forget();

            Debug.Log($"[Room] {name} 진입 완료 - 적 {aliveEnemyCount}마리");
        }


        // ====== 적 스폰 ======

        /// <summary>
        /// 적 스폰 실행
        /// </summary>
        private void SpawnEnemies()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning($"[Room] {name}: 스폰 포인트가 없습니다!");
                return;
            }

            spawnedEnemies.Clear();
            aliveEnemyCount = 0;

            // RoomData의 enemySpawns 사용 (RoomData가 있고 enemySpawns가 설정된 경우)
            if (roomData != null && roomData.enemySpawns != null && roomData.enemySpawns.Length > 0)
            {
                SpawnFromRoomData();
            }
            else
            {
                // 스폰 포인트의 기본 EnemyData 사용 (RoomData가 없거나 enemySpawns가 비어있는 경우)
                SpawnFromSpawnPoints();

                if (roomData == null)
                {
                    Debug.Log($"[Room] {name}: RoomData가 없으므로 스폰 포인트 기본 설정 사용");
                }
            }

            OnEnemyCountChanged?.Invoke(this, aliveEnemyCount);
        }

        /// <summary>
        /// RoomData 기반 스폰
        /// </summary>
        private void SpawnFromRoomData()
        {
            int spawnIndex = 0;

            foreach (var spawnData in roomData.enemySpawns)
            {
                if (spawnData.enemyData == null) continue;
                if (!spawnData.ShouldSpawn()) continue;

                int count = spawnData.GetRandomCount();

                for (int i = 0; i < count; i++)
                {
                    if (spawnIndex >= spawnPoints.Length)
                    {
                        Debug.LogWarning($"[Room] {name}: 스폰 포인트 부족! (필요: {spawnIndex + 1}, 보유: {spawnPoints.Length})");
                        break;
                    }

                    GameObject enemy = spawnPoints[spawnIndex].SpawnEnemy(spawnData.enemyData);
                    if (enemy != null)
                    {
                        RegisterEnemy(enemy);
                        spawnIndex++;
                    }
                }
            }
        }

        /// <summary>
        /// 스폰 포인트 기본 EnemyData 사용
        /// </summary>
        private void SpawnFromSpawnPoints()
        {
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.EnemyData == null) continue;

                GameObject enemy = spawnPoint.SpawnEnemy();
                if (enemy != null)
                {
                    RegisterEnemy(enemy);
                }
            }
        }


        // ====== 적 관리 ======

        /// <summary>
        /// 적 등록 및 사망 이벤트 구독
        /// </summary>
        private void RegisterEnemy(GameObject enemyObj)
        {
            spawnedEnemies.Add(enemyObj);
            aliveEnemyCount++;

            // Enemy 컴포넌트의 OnDeath 이벤트 구독
            var enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnDeath += OnEnemyDeath;
            }
        }

        /// <summary>
        /// 적 사망 시 호출
        /// </summary>
        private void OnEnemyDeath(Enemy enemy)
        {
            aliveEnemyCount--;
            OnEnemyCountChanged?.Invoke(this, aliveEnemyCount);

            Debug.Log($"[Room] {name}: 적 사망 - 남은 적: {aliveEnemyCount}마리");

            // 모든 적 처치 시 클리어 체크
            if (aliveEnemyCount <= 0 && roomData.clearCondition == ClearCondition.KillAllEnemies)
            {
                ClearRoom();
            }
        }


        // ====== 클리어 조건 체크 ======

        /// <summary>
        /// 클리어 조건 비동기 체크
        /// </summary>
        private async Awaitable CheckClearConditionAsync(CancellationToken token)
        {
            if (roomData == null) return;

            while (!token.IsCancellationRequested && currentState == RoomState.InProgress)
            {
                // 시간 제한 체크
                if (roomData.timeLimit > 0f)
                {
                    float elapsedTime = Time.time - roomStartTime;
                    if (elapsedTime >= roomData.timeLimit)
                    {
                        FailRoom("시간 초과");
                        return;
                    }
                }

                // token 없이 대기 (while 조건으로 취소 체크)
                await Awaitable.WaitForSecondsAsync(0.5f);
            }
        }


        // ====== 방 클리어/실패 ======

        /// <summary>
        /// 방 클리어
        /// </summary>
        private void ClearRoom()
        {
            if (currentState == RoomState.Cleared) return;

            ChangeState(RoomState.Cleared);

            // 보상 지급
            GiveRewards();

            // 이벤트 발생
            OnRoomClear?.Invoke(this);

            // CancellationToken 취소
            roomCts?.Cancel();

            Debug.Log($"[Room] {name} 클리어!");
        }

        /// <summary>
        /// 방 실패
        /// </summary>
        private void FailRoom(string reason)
        {
            if (currentState == RoomState.Failed) return;

            ChangeState(RoomState.Failed);

            // 이벤트 발생
            OnRoomFail?.Invoke(this, reason);

            // CancellationToken 취소
            roomCts?.Cancel();

            Debug.Log($"[Room] {name} 실패: {reason}");
        }


        // ====== 보상 지급 ======

        /// <summary>
        /// 클리어 보상 지급
        /// </summary>
        private void GiveRewards()
        {
            if (roomData == null) return;

            // 골드 지급
            if (roomData.bonusGold > 0)
            {
                var currencySystem = GASPT.Economy.CurrencySystem.Instance;
                if (currencySystem != null)
                {
                    currencySystem.AddGold(roomData.bonusGold);
                    Debug.Log($"[Room] 보너스 골드 {roomData.bonusGold} 획득!");
                }
            }

            // 경험치 지급
            if (roomData.bonusExp > 0)
            {
                var playerLevel = GASPT.Level.PlayerLevel.Instance;
                if (playerLevel != null)
                {
                    playerLevel.AddExp(roomData.bonusExp);
                    Debug.Log($"[Room] 보너스 경험치 {roomData.bonusExp} 획득!");
                }
            }

            // 체력 회복 (현재 MaxHP의 30%)
            HealPlayer(0.3f);
        }

        /// <summary>
        /// 플레이어 체력 회복
        /// </summary>
        /// <param name="healPercent">회복 비율 (0.0 ~ 1.0)</param>
        private void HealPlayer(float healPercent)
        {
            var playerStats = FindAnyObjectByType<GASPT.Stats.PlayerStats>();

            if (playerStats == null)
            {
                Debug.LogWarning("[Room] PlayerStats를 찾을 수 없습니다. 체력 회복 불가.");
                return;
            }

            // 현재 MaxHP의 healPercent만큼 회복
            int maxHp = playerStats.MaxHP;
            int healAmount = Mathf.RoundToInt(maxHp * healPercent);

            if (healAmount > 0)
            {
                playerStats.Heal(healAmount);
                Debug.Log($"[Room] 체력 회복 요청: {healAmount} HP");
            }
        }


        // ====== 상태 관리 ======

        /// <summary>
        /// 상태 변경
        /// </summary>
        private void ChangeState(RoomState newState)
        {
            if (currentState == newState) return;

            RoomState oldState = currentState;
            currentState = newState;

            Debug.Log($"[Room] {name} 상태 변경: {oldState} → {newState}");
        }


        // ====== 디버그 ======

        [ContextMenu("Print Room Info")]
        private void PrintRoomInfo()
        {
            Debug.Log($"=== Room: {name} ===\n" +
                     $"State: {currentState}\n" +
                     $"RoomData: {(roomData != null ? roomData.roomName : "None")}\n" +
                     $"Spawn Points: {(spawnPoints != null ? spawnPoints.Length : 0)}\n" +
                     $"Alive Enemies: {aliveEnemyCount}\n" +
                     $"====================");
        }

        [ContextMenu("Force Clear Room")]
        private void ForceClearRoom()
        {
            if (Application.isPlaying)
            {
                ClearRoom();
            }
        }
    }


}
