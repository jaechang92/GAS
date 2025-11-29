using System.Collections.Generic;
using UnityEngine;

namespace GASPT.Gameplay.Level.Graph
{
    /// <summary>
    /// 던전 그래프의 노드 (방 정보)
    /// Slay the Spire 스타일의 층(Floor) 기반 그래프 구조에서 각 방을 나타냄
    /// </summary>
    [System.Serializable]
    public class DungeonNode
    {
        // ====== 기본 정보 ======

        /// <summary>
        /// 노드 고유 ID
        /// </summary>
        public string nodeId;

        /// <summary>
        /// 방 타입 (Combat, Elite, Shop, Rest, Treasure, Boss 등)
        /// </summary>
        public RoomType roomType;

        /// <summary>
        /// 연결된 RoomData (프리팹 선택용)
        /// </summary>
        public RoomData roomData;

        /// <summary>
        /// 생성된 Room 인스턴스 참조
        /// </summary>
        [System.NonSerialized]
        public Room roomInstance;


        // ====== 그래프 위치 ======

        /// <summary>
        /// 층 인덱스 (0 = Entry, Last = Boss)
        /// </summary>
        public int floor;

        /// <summary>
        /// 같은 층 내에서의 위치 인덱스 (0부터 시작)
        /// </summary>
        public int column;

        /// <summary>
        /// 미니맵 표시용 2D 위치
        /// </summary>
        public Vector2 minimapPosition;


        // ====== 연결 정보 ======

        /// <summary>
        /// 이 노드에서 나가는 연결 (다음 층으로)
        /// </summary>
        public List<string> outgoingConnections = new List<string>();

        /// <summary>
        /// 이 노드로 들어오는 연결 (이전 층에서)
        /// </summary>
        public List<string> incomingConnections = new List<string>();


        // ====== 상태 ======

        /// <summary>
        /// 플레이어가 방문했는지 여부
        /// </summary>
        public bool isVisited;

        /// <summary>
        /// 미니맵에 공개되었는지 여부 (인접 노드 방문 시 공개)
        /// </summary>
        public bool isRevealed;

        /// <summary>
        /// 방 클리어 여부
        /// </summary>
        public bool isCleared;


        // ====== 생성자 ======

        public DungeonNode()
        {
            nodeId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            outgoingConnections = new List<string>();
            incomingConnections = new List<string>();
        }

        public DungeonNode(string id, RoomType type, int floor, int column) : this()
        {
            this.nodeId = id;
            this.roomType = type;
            this.floor = floor;
            this.column = column;
            this.minimapPosition = new Vector2(column, floor);
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 이 노드가 분기점인지 확인 (나가는 연결이 2개 이상)
        /// </summary>
        public bool IsBranchPoint => outgoingConnections.Count > 1;

        /// <summary>
        /// 이 노드가 합류점인지 확인 (들어오는 연결이 2개 이상)
        /// </summary>
        public bool IsMergePoint => incomingConnections.Count > 1;

        /// <summary>
        /// 노드 방문 처리
        /// </summary>
        public void Visit()
        {
            isVisited = true;
            isRevealed = true;
        }

        /// <summary>
        /// 노드 공개 처리 (방문하지 않고 미니맵에만 표시)
        /// </summary>
        public void Reveal()
        {
            isRevealed = true;
        }

        /// <summary>
        /// 노드 클리어 처리
        /// </summary>
        public void Clear()
        {
            isCleared = true;
        }


        // ====== 디버그 ======

        public override string ToString()
        {
            return $"[Node:{nodeId}] {roomType} (F{floor}C{column}) " +
                   $"Out:{outgoingConnections.Count} In:{incomingConnections.Count} " +
                   $"Visited:{isVisited} Cleared:{isCleared}";
        }
    }
}
