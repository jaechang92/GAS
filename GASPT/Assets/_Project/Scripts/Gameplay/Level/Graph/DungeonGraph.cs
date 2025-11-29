using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GASPT.Gameplay.Level.Graph
{
    /// <summary>
    /// 던전 그래프 (전체 던전 구조)
    /// 노드(방)와 엣지(연결)로 구성된 그래프 자료구조
    /// </summary>
    [System.Serializable]
    public class DungeonGraph
    {
        // ====== 그래프 데이터 ======

        /// <summary>
        /// 노드 딕셔너리 (nodeId -> DungeonNode)
        /// </summary>
        public Dictionary<string, DungeonNode> nodes = new Dictionary<string, DungeonNode>();

        /// <summary>
        /// 엣지 목록
        /// </summary>
        public List<DungeonEdge> edges = new List<DungeonEdge>();


        // ====== 특수 노드 ======

        /// <summary>
        /// 시작 노드 ID
        /// </summary>
        public string entryNodeId;

        /// <summary>
        /// 보스 노드 ID
        /// </summary>
        public string bossNodeId;

        /// <summary>
        /// 현재 플레이어 위치 노드 ID
        /// </summary>
        public string currentNodeId;


        // ====== 메타 정보 ======

        /// <summary>
        /// 생성에 사용된 시드
        /// </summary>
        public int seed;

        /// <summary>
        /// 총 층 수
        /// </summary>
        public int totalFloors;

        /// <summary>
        /// 스테이지 번호
        /// </summary>
        public int stageNumber;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 시작 노드 가져오기
        /// </summary>
        public DungeonNode EntryNode => GetNode(entryNodeId);

        /// <summary>
        /// 보스 노드 가져오기
        /// </summary>
        public DungeonNode BossNode => GetNode(bossNodeId);

        /// <summary>
        /// 현재 노드 가져오기
        /// </summary>
        public DungeonNode CurrentNode => GetNode(currentNodeId);

        /// <summary>
        /// 총 노드 수
        /// </summary>
        public int NodeCount => nodes.Count;

        /// <summary>
        /// 총 엣지 수
        /// </summary>
        public int EdgeCount => edges.Count;


        // ====== 노드 관리 ======

        /// <summary>
        /// 노드 추가
        /// </summary>
        public void AddNode(DungeonNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.nodeId))
            {
                Debug.LogError("[DungeonGraph] Invalid node");
                return;
            }

            if (nodes.ContainsKey(node.nodeId))
            {
                Debug.LogWarning($"[DungeonGraph] Node {node.nodeId} already exists");
                return;
            }

            nodes[node.nodeId] = node;
        }

        /// <summary>
        /// 노드 가져오기
        /// </summary>
        public DungeonNode GetNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            return nodes.TryGetValue(nodeId, out var node) ? node : null;
        }

        /// <summary>
        /// 특정 층의 모든 노드 가져오기
        /// </summary>
        public List<DungeonNode> GetNodesAtFloor(int floor)
        {
            return nodes.Values.Where(n => n.floor == floor).OrderBy(n => n.column).ToList();
        }

        /// <summary>
        /// 특정 타입의 모든 노드 가져오기
        /// </summary>
        public List<DungeonNode> GetNodesByType(RoomType type)
        {
            return nodes.Values.Where(n => n.roomType == type).ToList();
        }

        public List<DungeonNode> GetAllNodes()
        {
            return nodes.Values.ToList();
        }

        /// <summary>
        /// 모든 엣지 가져오기
        /// </summary>
        public List<DungeonEdge> GetAllEdges()
        {
            return new List<DungeonEdge>(edges);
        }


        // ====== 엣지 관리 ======

        /// <summary>
        /// 엣지 추가 (양쪽 노드에 연결 정보도 추가)
        /// </summary>
        public void AddEdge(DungeonEdge edge)
        {
            if (edge == null) return;

            edges.Add(edge);

            // 노드에 연결 정보 추가
            var fromNode = GetNode(edge.fromNodeId);
            var toNode = GetNode(edge.toNodeId);

            if (fromNode != null && !fromNode.outgoingConnections.Contains(edge.toNodeId))
            {
                fromNode.outgoingConnections.Add(edge.toNodeId);
            }

            if (toNode != null && !toNode.incomingConnections.Contains(edge.fromNodeId))
            {
                toNode.incomingConnections.Add(edge.fromNodeId);
            }
        }

        /// <summary>
        /// 두 노드 사이 엣지 추가 (간편 메서드)
        /// </summary>
        public void Connect(string fromId, string toId, EdgeType type = EdgeType.Normal)
        {
            AddEdge(new DungeonEdge(fromId, toId, type));
        }

        /// <summary>
        /// 두 노드 사이 엣지 존재 여부
        /// </summary>
        public bool HasEdge(string fromId, string toId)
        {
            return edges.Any(e => e.fromNodeId == fromId && e.toNodeId == toId);
        }


        // ====== 인접 노드 조회 ======

        /// <summary>
        /// 특정 노드에서 이동 가능한 다음 노드들 (outgoing)
        /// </summary>
        public List<DungeonNode> GetAdjacentNodes(string nodeId)
        {
            var node = GetNode(nodeId);
            if (node == null) return new List<DungeonNode>();

            return node.outgoingConnections
                .Select(id => GetNode(id))
                .Where(n => n != null)
                .ToList();
        }

        /// <summary>
        /// 특정 노드로 들어오는 이전 노드들 (incoming)
        /// </summary>
        public List<DungeonNode> GetPreviousNodes(string nodeId)
        {
            var node = GetNode(nodeId);
            if (node == null) return new List<DungeonNode>();

            return node.incomingConnections
                .Select(id => GetNode(id))
                .Where(n => n != null)
                .ToList();
        }

        /// <summary>
        /// 미방문 인접 노드들
        /// </summary>
        public List<DungeonNode> GetUnvisitedAdjacentNodes(string nodeId)
        {
            return GetAdjacentNodes(nodeId).Where(n => !n.isVisited).ToList();
        }


        // ====== 경로 탐색 ======

        /// <summary>
        /// 두 노드 간 최단 경로 (BFS)
        /// </summary>
        public List<DungeonNode> GetPath(string startId, string endId)
        {
            if (startId == endId)
                return new List<DungeonNode> { GetNode(startId) };

            var visited = new HashSet<string>();
            var queue = new Queue<List<string>>();
            queue.Enqueue(new List<string> { startId });

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var current = path.Last();

                if (current == endId)
                {
                    return path.Select(id => GetNode(id)).ToList();
                }

                if (visited.Contains(current)) continue;
                visited.Add(current);

                foreach (var nextId in GetNode(current)?.outgoingConnections ?? new List<string>())
                {
                    if (!visited.Contains(nextId))
                    {
                        var newPath = new List<string>(path) { nextId };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return new List<DungeonNode>(); // 경로 없음
        }

        /// <summary>
        /// Entry에서 Boss까지의 경로 존재 여부
        /// </summary>
        public bool HasPathToBoss()
        {
            return GetPath(entryNodeId, bossNodeId).Count > 0;
        }


        // ====== 그래프 검증 ======

        /// <summary>
        /// 그래프 유효성 검증
        /// </summary>
        public bool ValidateGraph(out List<string> errors)
        {
            errors = new List<string>();

            // 1. Entry 노드 존재
            if (EntryNode == null)
                errors.Add("Entry node not found");

            // 2. Boss 노드 존재
            if (BossNode == null)
                errors.Add("Boss node not found");

            // 3. Entry → Boss 경로 존재
            if (EntryNode != null && BossNode != null && !HasPathToBoss())
                errors.Add("No path from Entry to Boss");

            // 4. 모든 노드 Entry에서 도달 가능
            var reachable = GetAllReachableNodes(entryNodeId);
            var unreachable = nodes.Keys.Where(id => !reachable.Contains(id)).ToList();
            if (unreachable.Count > 0)
                errors.Add($"Unreachable nodes: {string.Join(", ", unreachable)}");

            // 5. 고립된 노드 없음 (연결 없는 노드)
            foreach (var node in nodes.Values)
            {
                if (node.nodeId != entryNodeId && node.incomingConnections.Count == 0)
                    errors.Add($"Node {node.nodeId} has no incoming connections");

                if (node.nodeId != bossNodeId && node.outgoingConnections.Count == 0)
                    errors.Add($"Node {node.nodeId} has no outgoing connections");
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// 특정 노드에서 도달 가능한 모든 노드 (DFS)
        /// </summary>
        public HashSet<string> GetAllReachableNodes(string startId)
        {
            var reachable = new HashSet<string>();
            var stack = new Stack<string>();
            stack.Push(startId);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (reachable.Contains(current)) continue;
                reachable.Add(current);

                var node = GetNode(current);
                if (node == null) continue;

                foreach (var nextId in node.outgoingConnections)
                {
                    if (!reachable.Contains(nextId))
                        stack.Push(nextId);
                }
            }

            return reachable;
        }


        // ====== 상태 관리 ======

        /// <summary>
        /// 현재 노드 변경
        /// </summary>
        public void SetCurrentNode(string nodeId)
        {
            currentNodeId = nodeId;
            var node = GetNode(nodeId);
            if (node != null)
            {
                node.Visit();
                // 인접 노드 공개
                foreach (var adjacent in GetAdjacentNodes(nodeId))
                {
                    adjacent.Reveal();
                }
            }
        }

        /// <summary>
        /// 그래프 상태 초기화 (새 런 시작 시)
        /// </summary>
        public void ResetState()
        {
            foreach (var node in nodes.Values)
            {
                node.isVisited = false;
                node.isRevealed = false;
                node.isCleared = false;
            }
            currentNodeId = entryNodeId;

            // Entry 노드 방문 처리
            if (EntryNode != null)
            {
                EntryNode.Visit();
            }
        }


        // ====== 통계 ======

        /// <summary>
        /// 방 타입별 개수 통계
        /// </summary>
        public Dictionary<RoomType, int> GetRoomTypeStats()
        {
            return nodes.Values
                .GroupBy(n => n.roomType)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// 분기점 수
        /// </summary>
        public int GetBranchPointCount()
        {
            return nodes.Values.Count(n => n.IsBranchPoint);
        }

        /// <summary>
        /// Entry에서 Boss까지 최단 경로 길이
        /// </summary>
        public int GetShortestPathLength()
        {
            return GetPath(entryNodeId, bossNodeId).Count;
        }


        // ====== 디버그 ======

        public override string ToString()
        {
            return $"[DungeonGraph] Stage:{stageNumber} Seed:{seed} " +
                   $"Nodes:{NodeCount} Edges:{EdgeCount} Floors:{totalFloors}";
        }

        /// <summary>
        /// 상세 정보 출력
        /// </summary>
        public string ToDetailedString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(ToString());
            sb.AppendLine($"Entry: {entryNodeId}, Boss: {bossNodeId}, Current: {currentNodeId}");
            sb.AppendLine("--- Nodes by Floor ---");

            for (int f = 0; f <= totalFloors; f++)
            {
                var floorNodes = GetNodesAtFloor(f);
                if (floorNodes.Count == 0) continue;

                sb.Append($"Floor {f}: ");
                sb.AppendLine(string.Join(", ", floorNodes.Select(n => $"{n.roomType}({n.nodeId})")));
            }

            sb.AppendLine("--- Room Type Stats ---");
            foreach (var stat in GetRoomTypeStats())
            {
                sb.AppendLine($"  {stat.Key}: {stat.Value}");
            }

            return sb.ToString();
        }
    }
}
