using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Core;
using GASPT.Gameplay.Level.Graph;
using GASPT.Gameplay.Level.Generation;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 스테이지 매니저
    /// 전체 스테이지(여러 층으로 구성된 던전) 진행을 관리
    /// </summary>
    public class StageManager : SingletonManager<StageManager>
    {
        // ====== 설정 ======

        [Header("현재 스테이지")]
        [SerializeField] private StageConfig currentStageConfig;


        // ====== 상태 ======

        private StageProgressData progressData;
        private DungeonGenerator dungeonGenerator;


        // ====== 이벤트 ======

        /// <summary>
        /// 스테이지 시작 이벤트
        /// </summary>
        public event Action<StageConfig> OnStageStarted;

        /// <summary>
        /// 층 변경 이벤트
        /// </summary>
        public event Action<int, StageFloorConfig> OnFloorChanged;

        /// <summary>
        /// 층 클리어 이벤트
        /// </summary>
        public event Action<int> OnFloorCleared;

        /// <summary>
        /// 스테이지 클리어 이벤트
        /// </summary>
        public event Action<StageConfig, StageProgressData> OnStageCleared;

        /// <summary>
        /// 스테이지 실패 이벤트
        /// </summary>
        public event Action<StageConfig, StageProgressData> OnStageFailed;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 스테이지 설정
        /// </summary>
        public StageConfig CurrentStage => currentStageConfig;

        /// <summary>
        /// 현재 층 인덱스
        /// </summary>
        public int CurrentFloorIndex => progressData?.currentFloor ?? 0;

        /// <summary>
        /// 현재 층 설정
        /// </summary>
        public StageFloorConfig CurrentFloor
        {
            get
            {
                if (currentStageConfig == null || currentStageConfig.floors == null)
                    return null;

                int index = CurrentFloorIndex;
                if (index < 0 || index >= currentStageConfig.floors.Length)
                    return null;

                return currentStageConfig.floors[index];
            }
        }

        /// <summary>
        /// 스테이지 진행 데이터
        /// </summary>
        public StageProgressData Progress => progressData;

        /// <summary>
        /// 스테이지 진행 중 여부
        /// </summary>
        public bool IsInProgress => progressData != null && !progressData.isCompleted && !progressData.isFailed;


        // ====== Unity 생명주기 ======

        protected override void Awake()
        {
            base.Awake();
            dungeonGenerator = new DungeonGenerator();
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 스테이지 시작
        /// </summary>
        public async Awaitable StartStageAsync(StageConfig config, int? seed = null)
        {
            if (config == null)
            {
                Debug.LogError("[StageManager] StageConfig가 null입니다!");
                return;
            }

            currentStageConfig = config;

            // 진행 데이터 초기화
            progressData = new StageProgressData
            {
                stageId = config.stageId,
                currentFloor = 0,
                seed = seed ?? SeedManager.GenerateRandomSeed(),
                startTime = DateTime.Now,
                isCompleted = false,
                isFailed = false
            };

            SeedManager.SetSeed(progressData.seed);

            Debug.Log($"[StageManager] 스테이지 시작: {config.stageName} (Seed: {progressData.seed})");

            OnStageStarted?.Invoke(config);

            // 첫 번째 층 로드
            await LoadFloorAsync(0);
        }

        /// <summary>
        /// 특정 층 로드
        /// </summary>
        public async Awaitable LoadFloorAsync(int floorIndex)
        {
            if (currentStageConfig == null)
            {
                Debug.LogError("[StageManager] 스테이지가 설정되지 않았습니다!");
                return;
            }

            if (floorIndex < 0 || floorIndex >= currentStageConfig.TotalFloors)
            {
                Debug.LogError($"[StageManager] 잘못된 층 인덱스: {floorIndex}");
                return;
            }

            var floorConfig = currentStageConfig.floors[floorIndex];

            // 진입 조건 확인
            if (floorConfig.entryCondition != null && !floorConfig.entryCondition.IsMet())
            {
                Debug.LogWarning($"[StageManager] 층 진입 조건 미충족: {floorConfig.floorName}");
                return;
            }

            progressData.currentFloor = floorIndex;

            Debug.Log($"[StageManager] 층 로드: {floorConfig.floorName} ({floorIndex + 1}/{currentStageConfig.TotalFloors})");

            // 던전 생성
            var dungeonConfig = floorConfig.dungeonConfig;
            if (dungeonConfig != null && RoomManager.Instance != null)
            {
                if (dungeonConfig.generationType == DungeonGenerationType.Procedural)
                {
                    // 절차적 생성: 그래프 생성 후 RoomPlacer로 Room 배치
                    int floorSeed = progressData.seed + floorIndex * 1000;
                    var graph = await dungeonGenerator.GenerateDungeonAsync(dungeonConfig, floorSeed);

                    if (graph != null)
                    {
                        // RoomPlacer를 사용하여 Room 인스턴스 생성
                        var roomPlacer = new RoomPlacer(dungeonConfig.roomDataPool, dungeonConfig.roomTemplatePrefab);
                        var roomMap = roomPlacer.PlaceRooms(graph, RoomManager.Instance.transform);

                        // RoomManager에 그래프와 Room 맵 로드
                        RoomManager.Instance.LoadDungeonGraph(graph, roomMap);
                        await RoomManager.Instance.StartGraphDungeonAsync();
                    }
                }
                else
                {
                    // 기존 방식 (Prefab/Data) - LoadDungeon 사용 (동기)
                    RoomManager.Instance.LoadDungeon(dungeonConfig);
                    await RoomManager.Instance.StartDungeonAsync();
                }
            }

            OnFloorChanged?.Invoke(floorIndex, floorConfig);
        }

        /// <summary>
        /// 다음 층으로 이동
        /// </summary>
        public async Awaitable MoveToNextFloorAsync()
        {
            if (!IsInProgress) return;

            int nextFloor = progressData.currentFloor + 1;

            // 마지막 층이었으면 스테이지 클리어
            if (nextFloor >= currentStageConfig.TotalFloors)
            {
                await CompleteStageAsync();
                return;
            }

            // 현재 층 클리어 기록
            progressData.clearedFloors.Add(progressData.currentFloor);
            OnFloorCleared?.Invoke(progressData.currentFloor);

            // 다음 층 로드
            await LoadFloorAsync(nextFloor);
        }

        /// <summary>
        /// 현재 층 클리어
        /// </summary>
        public void ClearCurrentFloor()
        {
            if (!IsInProgress) return;

            if (!progressData.clearedFloors.Contains(progressData.currentFloor))
            {
                progressData.clearedFloors.Add(progressData.currentFloor);
                OnFloorCleared?.Invoke(progressData.currentFloor);

                Debug.Log($"[StageManager] 층 클리어: {CurrentFloor?.floorName}");
            }
        }

        /// <summary>
        /// 스테이지 완료
        /// </summary>
        public async Awaitable CompleteStageAsync()
        {
            if (!IsInProgress) return;

            progressData.isCompleted = true;
            progressData.endTime = DateTime.Now;

            // 보상 계산
            CalculateRewards();

            Debug.Log($"[StageManager] 스테이지 클리어: {currentStageConfig.stageName}");
            Debug.Log($"[StageManager] 획득 골드: {progressData.earnedGold}, 경험치: {progressData.earnedExp}");

            OnStageCleared?.Invoke(currentStageConfig, progressData);

            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 스테이지 실패
        /// </summary>
        public void FailStage(string reason = "")
        {
            if (!IsInProgress) return;

            progressData.isFailed = true;
            progressData.endTime = DateTime.Now;
            progressData.failReason = reason;

            Debug.Log($"[StageManager] 스테이지 실패: {currentStageConfig.stageName} - {reason}");

            OnStageFailed?.Invoke(currentStageConfig, progressData);
        }

        /// <summary>
        /// 스테이지 중단 (세이브 포인트로 복귀 등)
        /// </summary>
        public void AbandonStage()
        {
            if (progressData != null)
            {
                progressData.isFailed = true;
                progressData.failReason = "포기";
            }

            currentStageConfig = null;
            progressData = null;

            Debug.Log("[StageManager] 스테이지 중단");
        }

        /// <summary>
        /// 플레이어 사망 처리
        /// </summary>
        public bool HandlePlayerDeath()
        {
            if (!IsInProgress) return false;

            progressData.deathCount++;

            // 부활 가능 여부 확인
            if (currentStageConfig.allowRevive && progressData.reviveCount < currentStageConfig.maxRevives)
            {
                progressData.reviveCount++;
                Debug.Log($"[StageManager] 플레이어 부활 ({progressData.reviveCount}/{currentStageConfig.maxRevives})");
                return true; // 부활 가능
            }

            // 부활 불가 - 스테이지 실패
            FailStage("플레이어 사망");
            return false;
        }


        // ====== 내부 메서드 ======

        private void CalculateRewards()
        {
            if (currentStageConfig == null || progressData == null) return;

            float totalMultiplier = 1f;

            // 클리어한 층 수에 따른 보상
            foreach (int floorIndex in progressData.clearedFloors)
            {
                totalMultiplier *= currentStageConfig.GetFloorRewardMultiplier(floorIndex);
            }

            // 사망 횟수에 따른 패널티
            float deathPenalty = Mathf.Max(0.5f, 1f - (progressData.deathCount * 0.1f));

            progressData.earnedGold = Mathf.RoundToInt(currentStageConfig.baseClearGold * totalMultiplier * deathPenalty);
            progressData.earnedExp = Mathf.RoundToInt(currentStageConfig.baseClearExp * totalMultiplier * deathPenalty);

            // 클리어 시간 보너스 (TODO: 구현)
        }
    }


    /// <summary>
    /// 스테이지 진행 데이터
    /// </summary>
    [Serializable]
    public class StageProgressData
    {
        /// <summary>
        /// 스테이지 ID
        /// </summary>
        public string stageId;

        /// <summary>
        /// 현재 층
        /// </summary>
        public int currentFloor;

        /// <summary>
        /// 시드 값
        /// </summary>
        public int seed;

        /// <summary>
        /// 클리어한 층 목록
        /// </summary>
        public List<int> clearedFloors = new List<int>();

        /// <summary>
        /// 시작 시간
        /// </summary>
        public DateTime startTime;

        /// <summary>
        /// 종료 시간
        /// </summary>
        public DateTime endTime;

        /// <summary>
        /// 완료 여부
        /// </summary>
        public bool isCompleted;

        /// <summary>
        /// 실패 여부
        /// </summary>
        public bool isFailed;

        /// <summary>
        /// 실패 사유
        /// </summary>
        public string failReason;

        /// <summary>
        /// 사망 횟수
        /// </summary>
        public int deathCount;

        /// <summary>
        /// 부활 횟수
        /// </summary>
        public int reviveCount;

        /// <summary>
        /// 획득 골드
        /// </summary>
        public int earnedGold;

        /// <summary>
        /// 획득 경험치
        /// </summary>
        public int earnedExp;

        /// <summary>
        /// 방문한 노드 ID 목록
        /// </summary>
        public List<string> visitedNodeIds = new List<string>();


        /// <summary>
        /// 플레이 시간 (초)
        /// </summary>
        public double PlayTimeSeconds
        {
            get
            {
                if (endTime > startTime)
                    return (endTime - startTime).TotalSeconds;
                else
                    return (DateTime.Now - startTime).TotalSeconds;
            }
        }
    }
}
