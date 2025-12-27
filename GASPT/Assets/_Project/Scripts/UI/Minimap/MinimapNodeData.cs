using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Gameplay.Level.Graph;

namespace GASPT.UI.Minimap
{
    /// <summary>
    /// 미니맵 노드 상태
    /// </summary>
    public enum MinimapNodeState
    {
        /// <summary>
        /// 숨겨진 상태 (아직 발견되지 않음)
        /// </summary>
        Hidden,

        /// <summary>
        /// 발견됨 (인접 노드로 표시됨)
        /// </summary>
        Revealed,

        /// <summary>
        /// 방문하지 않음 (갈 수 있지만 아직 안 감)
        /// </summary>
        Unvisited,

        /// <summary>
        /// 방문함
        /// </summary>
        Visited,

        /// <summary>
        /// 현재 위치
        /// </summary>
        Current,

        /// <summary>
        /// 클리어 완료
        /// </summary>
        Cleared
    }


    /// <summary>
    /// 미니맵 노드 데이터
    /// 던전 노드를 미니맵에 표시하기 위한 데이터
    /// </summary>
    [System.Serializable]
    public struct MinimapNodeData
    {
        /// <summary>
        /// 노드 ID
        /// </summary>
        public string nodeId;

        /// <summary>
        /// 미니맵상 위치
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// 방 타입
        /// </summary>
        public RoomType roomType;

        /// <summary>
        /// 노드 상태
        /// </summary>
        public MinimapNodeState state;

        /// <summary>
        /// 층 번호
        /// </summary>
        public int floor;

        /// <summary>
        /// 열 번호
        /// </summary>
        public int column;

        /// <summary>
        /// 연결된 노드 ID 목록 (나가는 방향)
        /// </summary>
        public string[] outgoingConnections;

        /// <summary>
        /// 연결된 노드 ID 목록 (들어오는 방향)
        /// </summary>
        public string[] incomingConnections;


        /// <summary>
        /// DungeonNode로부터 MinimapNodeData 생성
        /// </summary>
        public static MinimapNodeData FromDungeonNode(DungeonNode node)
        {
            return new MinimapNodeData
            {
                nodeId = node.nodeId,
                position = node.minimapPosition,
                roomType = node.roomType,
                state = GetStateFromNode(node),
                floor = node.floor,
                column = node.column,
                outgoingConnections = node.outgoingConnections.ToArray(),
                incomingConnections = node.incomingConnections.ToArray()
            };
        }

        /// <summary>
        /// 노드 상태 결정
        /// </summary>
        private static MinimapNodeState GetStateFromNode(DungeonNode node)
        {
            if (node.isCleared)
                return MinimapNodeState.Cleared;

            if (node.isVisited)
                return MinimapNodeState.Visited;

            if (node.isRevealed)
                return MinimapNodeState.Revealed;

            return MinimapNodeState.Hidden;
        }

        /// <summary>
        /// 현재 노드로 설정
        /// </summary>
        public MinimapNodeData AsCurrent()
        {
            var copy = this;
            copy.state = MinimapNodeState.Current;
            return copy;
        }

        /// <summary>
        /// 상태 업데이트
        /// </summary>
        public MinimapNodeData WithState(MinimapNodeState newState)
        {
            var copy = this;
            copy.state = newState;
            return copy;
        }

        /// <summary>
        /// 선택 가능 여부 (이동 가능한 노드인지)
        /// </summary>
        public bool IsSelectable => state == MinimapNodeState.Revealed || state == MinimapNodeState.Unvisited;

        /// <summary>
        /// 표시 여부 (미니맵에 보이는지)
        /// </summary>
        public bool IsVisible => state != MinimapNodeState.Hidden;
    }


    /// <summary>
    /// 미니맵 엣지 데이터
    /// 노드 간 연결선을 표시하기 위한 데이터
    /// </summary>
    [System.Serializable]
    public struct MinimapEdgeData
    {
        /// <summary>
        /// 시작 노드 ID
        /// </summary>
        public string fromNodeId;

        /// <summary>
        /// 종료 노드 ID
        /// </summary>
        public string toNodeId;

        /// <summary>
        /// 시작 위치
        /// </summary>
        public Vector2 fromPosition;

        /// <summary>
        /// 종료 위치
        /// </summary>
        public Vector2 toPosition;

        /// <summary>
        /// 엣지 타입
        /// </summary>
        public EdgeType edgeType;

        /// <summary>
        /// 표시 여부 (양쪽 노드 중 하나라도 보이면 표시)
        /// </summary>
        public bool isVisible;

        /// <summary>
        /// 방문 여부 (실제로 이 경로를 통해 이동했는지)
        /// </summary>
        public bool isTraversed;


        /// <summary>
        /// DungeonEdge로부터 MinimapEdgeData 생성
        /// </summary>
        public static MinimapEdgeData FromDungeonEdge(DungeonEdge edge, DungeonGraph graph)
        {
            var fromNode = graph.GetNode(edge.fromNodeId);
            var toNode = graph.GetNode(edge.toNodeId);

            bool isVisible = (fromNode?.isRevealed ?? false) || (toNode?.isRevealed ?? false);
            bool isTraversed = (fromNode?.isVisited ?? false) && (toNode?.isVisited ?? false);

            return new MinimapEdgeData
            {
                fromNodeId = edge.fromNodeId,
                toNodeId = edge.toNodeId,
                fromPosition = fromNode?.minimapPosition ?? Vector2.zero,
                toPosition = toNode?.minimapPosition ?? Vector2.zero,
                edgeType = edge.edgeType,
                isVisible = isVisible,
                isTraversed = isTraversed
            };
        }
    }
}
