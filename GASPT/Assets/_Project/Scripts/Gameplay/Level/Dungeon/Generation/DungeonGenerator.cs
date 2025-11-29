using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Gameplay.Level.Graph;

namespace GASPT.Gameplay.Level.Generation
{
    /// <summary>
    /// 던전 생성 파사드 클래스
    /// GraphBuilder와 RoomPlacer를 조합하여 완전한 던전 생성
    /// </summary>
    public class DungeonGenerator
    {
        // ====== 이벤트 ======

        /// <summary>
        /// 던전 생성 완료 이벤트
        /// </summary>
        public event Action<DungeonGraph> OnDungeonGenerated;

        /// <summary>
        /// 던전 생성 진행 이벤트 (0~1)
        /// </summary>
        public event Action<float> OnGenerationProgress;


        // ====== 내부 컴포넌트 ======

        private GraphBuilder graphBuilder;
        private RoomPlacer roomPlacer;


        // ====== 상태 ======

        private DungeonGraph currentGraph;
        private Dictionary<string, Room> currentRooms;
        private bool isGenerating;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 생성된 그래프
        /// </summary>
        public DungeonGraph CurrentGraph => currentGraph;

        /// <summary>
        /// 현재 생성된 Room 목록
        /// </summary>
        public IReadOnlyDictionary<string, Room> CurrentRooms => currentRooms;

        /// <summary>
        /// 생성 중 여부
        /// </summary>
        public bool IsGenerating => isGenerating;


        // ====== 생성자 ======

        public DungeonGenerator()
        {
            graphBuilder = new GraphBuilder();
        }


        // ====== 던전 생성 ======

        /// <summary>
        /// 던전 비동기 생성
        /// </summary>
        public async Awaitable<DungeonGraph> GenerateDungeonAsync(DungeonConfig config, int? seed = null)
        {
            if (config == null)
            {
                Debug.LogError("[DungeonGenerator] DungeonConfig가 null입니다!");
                return null;
            }

            if (isGenerating)
            {
                Debug.LogWarning("[DungeonGenerator] 이미 생성 중입니다!");
                return null;
            }

            isGenerating = true;
            OnGenerationProgress?.Invoke(0f);

            try
            {
                Debug.Log($"[DungeonGenerator] 던전 생성 시작: {config.dungeonName}");

                // 1. 시드 결정
                int actualSeed = seed ?? SeedManager.GenerateRandomSeed();
                OnGenerationProgress?.Invoke(0.1f);

                // 2. 그래프 생성
                await Awaitable.NextFrameAsync(); // UI 업데이트 허용
                currentGraph = graphBuilder.GenerateGraph(config, actualSeed);

                if (currentGraph == null)
                {
                    Debug.LogError("[DungeonGenerator] 그래프 생성 실패!");
                    return null;
                }

                OnGenerationProgress?.Invoke(0.5f);

                // 3. Room 배치
                await Awaitable.NextFrameAsync();

                if (config.roomDataPool != null && config.roomTemplatePrefab != null)
                {
                    roomPlacer = new RoomPlacer(config.roomDataPool, config.roomTemplatePrefab);

                    // Room Container 생성
                    var container = CreateRoomContainer();
                    currentRooms = roomPlacer.PlaceRooms(currentGraph, container);
                }
                else
                {
                    Debug.LogWarning("[DungeonGenerator] roomDataPool 또는 roomTemplatePrefab이 없어 Room 배치를 건너뜁니다.");
                    currentRooms = new Dictionary<string, Room>();
                }

                OnGenerationProgress?.Invoke(0.9f);

                // 4. 초기화
                currentGraph.ResetState();

                OnGenerationProgress?.Invoke(1f);

                Debug.Log($"[DungeonGenerator] 던전 생성 완료! Seed:{actualSeed}, Nodes:{currentGraph.NodeCount}");

                // 이벤트 발생
                OnDungeonGenerated?.Invoke(currentGraph);

                return currentGraph;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DungeonGenerator] 던전 생성 중 오류: {e.Message}\n{e.StackTrace}");
                return null;
            }
            finally
            {
                isGenerating = false;
            }
        }

        /// <summary>
        /// 던전 동기 생성 (에디터용)
        /// </summary>
        public DungeonGraph GenerateDungeonSync(DungeonConfig config, int? seed = null)
        {
            if (config == null)
            {
                Debug.LogError("[DungeonGenerator] DungeonConfig가 null입니다!");
                return null;
            }

            Debug.Log($"[DungeonGenerator] 던전 동기 생성: {config.dungeonName}");

            // 시드 결정
            int actualSeed = seed ?? SeedManager.GenerateRandomSeed();

            // 그래프 생성
            currentGraph = graphBuilder.GenerateGraph(config, actualSeed);

            if (currentGraph == null)
            {
                Debug.LogError("[DungeonGenerator] 그래프 생성 실패!");
                return null;
            }

            // Room 배치 (선택적)
            if (config.roomDataPool != null && config.roomTemplatePrefab != null)
            {
                roomPlacer = new RoomPlacer(config.roomDataPool, config.roomTemplatePrefab);
                var container = CreateRoomContainer();
                currentRooms = roomPlacer.PlaceRooms(currentGraph, container);
            }
            else
            {
                currentRooms = new Dictionary<string, Room>();
            }

            // 초기화
            currentGraph.ResetState();

            Debug.Log($"[DungeonGenerator] 던전 동기 생성 완료! Seed:{actualSeed}");

            // 이벤트 발생
            OnDungeonGenerated?.Invoke(currentGraph);

            return currentGraph;
        }


        // ====== Room Container ======

        private Transform CreateRoomContainer()
        {
            // 기존 컨테이너 제거
            var existingContainer = GameObject.Find("DungeonRoomContainer");
            if (existingContainer != null)
            {
                UnityEngine.Object.Destroy(existingContainer);
            }

            // 새 컨테이너 생성
            var containerObj = new GameObject("DungeonRoomContainer");
            return containerObj.transform;
        }


        // ====== 던전 정리 ======

        /// <summary>
        /// 현재 던전 정리
        /// </summary>
        public void ClearDungeon()
        {
            if (roomPlacer != null)
            {
                roomPlacer.ClearAllRooms();
            }

            currentGraph = null;
            currentRooms = null;

            // Room Container 제거
            var container = GameObject.Find("DungeonRoomContainer");
            if (container != null)
            {
                UnityEngine.Object.Destroy(container);
            }

            Debug.Log("[DungeonGenerator] 던전 정리 완료");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 노드 ID로 Room 가져오기
        /// </summary>
        public Room GetRoom(string nodeId)
        {
            if (currentRooms != null && currentRooms.TryGetValue(nodeId, out var room))
            {
                return room;
            }
            return null;
        }

        /// <summary>
        /// 현재 노드의 Room 가져오기
        /// </summary>
        public Room GetCurrentRoom()
        {
            if (currentGraph == null) return null;
            return GetRoom(currentGraph.currentNodeId);
        }

        /// <summary>
        /// 그래프 검증
        /// </summary>
        public bool ValidateCurrentGraph(out List<string> errors)
        {
            errors = new List<string>();

            if (currentGraph == null)
            {
                errors.Add("No graph generated");
                return false;
            }

            return currentGraph.ValidateGraph(out errors);
        }


        // ====== 테스트용 ======

        /// <summary>
        /// 그래프만 생성 (Room 배치 없이) - 에디터 테스트용
        /// </summary>
        public DungeonGraph GenerateGraphOnly(DungeonConfig config, int seed)
        {
            if (config == null || config.generationRules == null)
            {
                Debug.LogError("[DungeonGenerator] 설정이 올바르지 않습니다!");
                return null;
            }

            return graphBuilder.GenerateGraph(config, seed);
        }
    }
}
