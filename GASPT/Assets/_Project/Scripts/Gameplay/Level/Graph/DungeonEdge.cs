namespace GASPT.Gameplay.Level.Graph
{
    /// <summary>
    /// 던전 그래프의 엣지 (방 연결 정보)
    /// 두 노드 사이의 연결을 나타냄
    /// </summary>
    [System.Serializable]
    public class DungeonEdge
    {
        // ====== 연결 정보 ======

        /// <summary>
        /// 출발 노드 ID
        /// </summary>
        public string fromNodeId;

        /// <summary>
        /// 도착 노드 ID
        /// </summary>
        public string toNodeId;

        /// <summary>
        /// 엣지 타입
        /// </summary>
        public EdgeType edgeType;


        // ====== 생성자 ======

        public DungeonEdge()
        {
            edgeType = EdgeType.Normal;
        }

        public DungeonEdge(string from, string to, EdgeType type = EdgeType.Normal)
        {
            this.fromNodeId = from;
            this.toNodeId = to;
            this.edgeType = type;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 양방향 이동 가능 여부
        /// </summary>
        public bool IsBidirectional => edgeType == EdgeType.Normal;

        /// <summary>
        /// 비밀 통로 여부
        /// </summary>
        public bool IsSecret => edgeType == EdgeType.Secret;


        // ====== 디버그 ======

        public override string ToString()
        {
            string arrow = edgeType == EdgeType.OneWay ? "->" : "<->";
            return $"[Edge] {fromNodeId} {arrow} {toNodeId} ({edgeType})";
        }
    }


    /// <summary>
    /// 엣지 타입 열거형
    /// </summary>
    public enum EdgeType
    {
        /// <summary>
        /// 일반 연결 (양방향 이동 가능)
        /// </summary>
        Normal,

        /// <summary>
        /// 비밀 통로 (특별한 조건으로 발견)
        /// </summary>
        Secret,

        /// <summary>
        /// 잠긴 통로 (이동 불가)
        /// </summary>

        Locked,

        /// <summary>
        /// 일방통행 (되돌아갈 수 없음)
        /// </summary>
        OneWay
    }
}
