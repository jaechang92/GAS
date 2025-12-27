using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Gameplay.Level.Graph;

namespace GASPT.Gameplay.Level.Generation
{
    /// <summary>
    /// 던전 그래프 빌더
    /// Slay the Spire 스타일의 층(Floor) 기반 그래프 생성
    /// </summary>
    public class GraphBuilder
    {
        // ====== 설정 ======

        private RoomGenerationRules rules;
        private int totalFloors;
        private int stageNumber;


        // ====== 생성 결과 ======

        private DungeonGraph graph;
        private List<List<DungeonNode>> nodesByFloor;


        // ====== 생성 메서드 ======

        /// <summary>
        /// 던전 그래프 생성
        /// </summary>
        public DungeonGraph GenerateGraph(DungeonConfig config, int seed)
        {
            if (config == null || config.generationRules == null)
            {
                Debug.LogError("[GraphBuilder] DungeonConfig 또는 generationRules가 null입니다!");
                return null;
            }

            // 시드 설정
            SeedManager.SetSeed(seed);

            // 초기화
            rules = config.generationRules;
            stageNumber = config.recommendedLevel;
            graph = new DungeonGraph
            {
                seed = seed,
                stageNumber = stageNumber
            };
            nodesByFloor = new List<List<DungeonNode>>();

            // 총 층 수 결정
            totalFloors = CalculateTotalFloors();
            graph.totalFloors = totalFloors;

            Debug.Log($"[GraphBuilder] 그래프 생성 시작 - Seed:{seed}, Floors:{totalFloors}");

            // 1. 각 층별 노드 생성
            GenerateNodesForAllFloors();

            // 2. 층 간 연결 생성
            ConnectFloors();

            // 3. 방 타입 할당
            AssignRoomTypes();

            // 4. Entry, Boss 노드 설정
            SetSpecialNodes();

            // 5. 그래프 검증
            ValidateAndFix();

            Debug.Log($"[GraphBuilder] 그래프 생성 완료 - Nodes:{graph.NodeCount}, Edges:{graph.EdgeCount}");
            Debug.Log(graph.ToDetailedString());

            return graph;
        }


        // ====== 층 수 계산 ======

        private int CalculateTotalFloors()
        {
            // minPathLength ~ maxPathLength 사이에서 결정
            int floors = SeedManager.Range(rules.minPathLength, rules.maxPathLength + 1);
            return Mathf.Max(floors, 4); // 최소 4층 (Entry + 중간 + 휴식 + Boss)
        }


        // ====== 노드 생성 ======

        private void GenerateNodesForAllFloors()
        {
            for (int floor = 0; floor < totalFloors; floor++)
            {
                var floorNodes = new List<DungeonNode>();
                int nodeCount = GetNodeCountForFloor(floor);

                for (int col = 0; col < nodeCount; col++)
                {
                    string nodeId = $"F{floor}C{col}";
                    var node = new DungeonNode(nodeId, RoomType.Normal, floor, col);

                    // 미니맵 위치 계산 (중앙 정렬)
                    float xOffset = (nodeCount - 1) / 2f;
                    node.minimapPosition = new Vector2(col - xOffset, floor);

                    graph.AddNode(node);
                    floorNodes.Add(node);
                }

                nodesByFloor.Add(floorNodes);
                Debug.Log($"[GraphBuilder] Floor {floor}: {nodeCount}개 노드 생성");
            }
        }

        /// <summary>
        /// 층별 노드 수 결정
        /// </summary>
        private int GetNodeCountForFloor(int floor)
        {
            // 첫 층 (Entry): 항상 1개
            if (floor == 0)
                return 1;

            // 마지막 층 (Boss): 항상 1개
            if (floor == totalFloors - 1)
                return 1;

            // 보스 전 층 (휴식/상점): 1~2개
            if (floor == totalFloors - 2)
                return SeedManager.Range(1, 3);

            // 중간 층: 규칙에 따라 1~3개
            int min = rules.minNodesPerFloor;
            int max = rules.maxNodesPerFloor;

            // 분기 확률에 따라 노드 수 조정
            if (SeedManager.Chance(rules.branchingFactor))
            {
                return SeedManager.Range(2, max + 1);
            }

            return SeedManager.Range(min, max);
        }


        // ====== 층 간 연결 ======

        private void ConnectFloors()
        {
            for (int floor = 0; floor < totalFloors - 1; floor++)
            {
                var currentFloor = nodesByFloor[floor];
                var nextFloor = nodesByFloor[floor + 1];

                ConnectToPreviousFloor(currentFloor, nextFloor, floor);
            }
        }

        /// <summary>
        /// 현재 층과 다음 층 연결
        /// </summary>
        private void ConnectToPreviousFloor(List<DungeonNode> currentFloor, List<DungeonNode> nextFloor, int floorIndex)
        {
            int currentCount = currentFloor.Count;
            int nextCount = nextFloor.Count;

            // 각 현재 층 노드가 최소 1개 연결을 가지도록 보장
            foreach (var currentNode in currentFloor)
            {
                // 가장 가까운 다음 층 노드 찾기
                var closestNext = GetClosestNode(currentNode, nextFloor);
                if (closestNext != null && !graph.HasEdge(currentNode.nodeId, closestNext.nodeId))
                {
                    graph.Connect(currentNode.nodeId, closestNext.nodeId);
                }
            }

            // 각 다음 층 노드가 최소 1개 입력 연결을 가지도록 보장
            foreach (var nextNode in nextFloor)
            {
                if (nextNode.incomingConnections.Count == 0)
                {
                    var closestCurrent = GetClosestNode(nextNode, currentFloor);
                    if (closestCurrent != null)
                    {
                        graph.Connect(closestCurrent.nodeId, nextNode.nodeId);
                    }
                }
            }

            // 추가 분기 연결 (교차하지 않는 범위 내에서)
            AddBranchConnections(currentFloor, nextFloor);
        }

        /// <summary>
        /// 가장 가까운 노드 찾기
        /// </summary>
        private DungeonNode GetClosestNode(DungeonNode node, List<DungeonNode> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;

            return candidates
                .OrderBy(c => Mathf.Abs(c.column - node.column))
                .FirstOrDefault();
        }

        /// <summary>
        /// 추가 분기 연결 (교차하지 않도록)
        /// </summary>
        private void AddBranchConnections(List<DungeonNode> currentFloor, List<DungeonNode> nextFloor)
        {
            // 분기 확률에 따라 추가 연결
            if (!SeedManager.Chance(rules.branchingFactor)) return;

            foreach (var currentNode in currentFloor)
            {
                // 이미 최대 분기 수에 도달했으면 스킵
                if (currentNode.outgoingConnections.Count >= rules.maxBranches) continue;

                // 인접한 다음 층 노드에 추가 연결 시도
                foreach (var nextNode in nextFloor)
                {
                    // 이미 연결되어 있으면 스킵
                    if (graph.HasEdge(currentNode.nodeId, nextNode.nodeId)) continue;

                    // 열 차이가 1 이하인 경우에만 연결 (교차 방지)
                    if (Mathf.Abs(currentNode.column - nextNode.column) <= 1)
                    {
                        // 교차 검사
                        if (!WouldCrossExistingEdge(currentNode, nextNode))
                        {
                            if (SeedManager.Chance(0.5f)) // 50% 확률로 추가 연결
                            {
                                graph.Connect(currentNode.nodeId, nextNode.nodeId);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 엣지 교차 검사
        /// </summary>
        private bool WouldCrossExistingEdge(DungeonNode from, DungeonNode to)
        {
            // 같은 층 간 연결의 교차 검사
            foreach (var edge in graph.edges)
            {
                var edgeFrom = graph.GetNode(edge.fromNodeId);
                var edgeTo = graph.GetNode(edge.toNodeId);

                if (edgeFrom == null || edgeTo == null) continue;
                if (edgeFrom.floor != from.floor) continue;

                // 두 선분이 교차하는지 체크
                // A->B와 C->D가 교차: (A.col < C.col && B.col > D.col) || (A.col > C.col && B.col < D.col)
                bool crosses = (from.column < edgeFrom.column && to.column > edgeTo.column) ||
                              (from.column > edgeFrom.column && to.column < edgeTo.column);

                if (crosses) return true;
            }

            return false;
        }


        // ====== 방 타입 할당 ======

        private void AssignRoomTypes()
        {
            // Entry 노드
            var entryNode = nodesByFloor[0].FirstOrDefault();
            if (entryNode != null)
            {
                entryNode.roomType = RoomType.Start;
            }

            // Boss 노드
            var bossNode = nodesByFloor[totalFloors - 1].FirstOrDefault();
            if (bossNode != null)
            {
                bossNode.roomType = RoomType.Boss;
            }

            // 보스 전 층: 휴식/상점
            if (rules.restRoomBeforeBoss && totalFloors >= 3)
            {
                var preBoosFloor = nodesByFloor[totalFloors - 2];
                foreach (var node in preBoosFloor)
                {
                    // 50% 휴식, 50% 상점
                    node.roomType = SeedManager.Chance(0.5f) ? RoomType.Rest : RoomType.Shop;
                }
            }

            // 중간 층: 특수 방 배치
            AssignMiddleFloorTypes();
        }

        /// <summary>
        /// 중간 층 방 타입 할당
        /// </summary>
        private void AssignMiddleFloorTypes()
        {
            int normalRoomCount = 0;

            for (int floor = 1; floor < totalFloors - 2; floor++)
            {
                foreach (var node in nodesByFloor[floor])
                {
                    // 이미 타입이 할당된 노드 스킵
                    if (node.roomType != RoomType.Normal) continue;

                    // 연속 특수 방 방지
                    if (normalRoomCount < rules.minNormalRoomsBetweenSpecial)
                    {
                        normalRoomCount++;
                        continue;
                    }

                    // 특수 방 타입 결정
                    RoomType specialType = rules.GetRandomSpecialRoomType();

                    if (specialType != RoomType.Normal)
                    {
                        node.roomType = specialType;
                        normalRoomCount = 0;
                    }
                    else
                    {
                        normalRoomCount++;
                    }
                }
            }

            // 엘리트 방 최소 1개 보장 (4층 이상일 때)
            EnsureMinimumEliteRooms();

            // 보물 방 배치 (선택적)
            PlaceTreasureRoom();
        }

        /// <summary>
        /// 최소 엘리트 방 보장
        /// </summary>
        private void EnsureMinimumEliteRooms()
        {
            var eliteCount = graph.GetNodesByType(RoomType.Elite).Count;
            if (eliteCount > 0 || totalFloors < 5) return;

            // 중간 층에서 Normal 방 하나를 Elite로 변경
            for (int floor = 2; floor < totalFloors - 2; floor++)
            {
                var normalNode = nodesByFloor[floor].FirstOrDefault(n => n.roomType == RoomType.Normal);
                if (normalNode != null)
                {
                    normalNode.roomType = RoomType.Elite;
                    Debug.Log($"[GraphBuilder] 엘리트 방 추가: {normalNode.nodeId}");
                    break;
                }
            }
        }

        /// <summary>
        /// 보물 방 배치
        /// </summary>
        private void PlaceTreasureRoom()
        {
            if (!SeedManager.Chance(rules.treasureRoomChance)) return;

            // 중간 층에서 Normal 방 하나를 Treasure로 변경
            for (int floor = totalFloors / 2; floor < totalFloors - 2; floor++)
            {
                var normalNode = nodesByFloor[floor].FirstOrDefault(n => n.roomType == RoomType.Normal);
                if (normalNode != null)
                {
                    normalNode.roomType = RoomType.Treasure;
                    Debug.Log($"[GraphBuilder] 보물 방 추가: {normalNode.nodeId}");
                    break;
                }
            }
        }


        // ====== 특수 노드 설정 ======

        private void SetSpecialNodes()
        {
            // Entry 노드 ID 설정
            var entry = nodesByFloor[0].FirstOrDefault();
            if (entry != null)
            {
                graph.entryNodeId = entry.nodeId;
                graph.currentNodeId = entry.nodeId;
            }

            // Boss 노드 ID 설정
            var boss = nodesByFloor[totalFloors - 1].FirstOrDefault();
            if (boss != null)
            {
                graph.bossNodeId = boss.nodeId;
            }
        }


        // ====== 검증 및 수정 ======

        private void ValidateAndFix()
        {
            if (graph.ValidateGraph(out var errors))
            {
                Debug.Log("[GraphBuilder] 그래프 검증 통과!");
                return;
            }

            Debug.LogWarning($"[GraphBuilder] 그래프 검증 실패: {string.Join(", ", errors)}");

            // Entry → Boss 경로가 없으면 강제 연결
            if (!graph.HasPathToBoss())
            {
                Debug.LogWarning("[GraphBuilder] Entry→Boss 경로 없음, 강제 연결 시도...");
                ForcePathToBoss();
            }

            // 재검증
            if (graph.ValidateGraph(out var remainingErrors))
            {
                Debug.Log("[GraphBuilder] 수정 후 검증 통과!");
            }
            else
            {
                Debug.LogError($"[GraphBuilder] 수정 후에도 검증 실패: {string.Join(", ", remainingErrors)}");
            }
        }

        /// <summary>
        /// Entry → Boss 강제 경로 생성
        /// </summary>
        private void ForcePathToBoss()
        {
            // 각 층의 첫 번째 노드를 연결
            for (int floor = 0; floor < totalFloors - 1; floor++)
            {
                var currentNode = nodesByFloor[floor].FirstOrDefault();
                var nextNode = nodesByFloor[floor + 1].FirstOrDefault();

                if (currentNode != null && nextNode != null)
                {
                    if (!graph.HasEdge(currentNode.nodeId, nextNode.nodeId))
                    {
                        graph.Connect(currentNode.nodeId, nextNode.nodeId);
                        Debug.Log($"[GraphBuilder] 강제 연결: {currentNode.nodeId} → {nextNode.nodeId}");
                    }
                }
            }
        }
    }
}
