using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Gameplay.Level.Graph;

namespace GASPT.Gameplay.Level.Generation
{
    /// <summary>
    /// 방 배치 클래스
    /// DungeonGraph를 기반으로 실제 Room 인스턴스를 생성하고 배치
    /// </summary>
    public class RoomPlacer
    {
        // ====== 설정 ======

        private RoomData[] roomDataPool;
        private Room roomTemplatePrefab;
        private Transform roomContainer;


        // ====== 캐시 ======

        private Dictionary<RoomType, List<RoomData>> roomDataByType;
        private Dictionary<string, Room> roomInstances;


        // ====== 생성자 ======

        public RoomPlacer(RoomData[] pool, Room templatePrefab)
        {
            this.roomDataPool = pool;
            this.roomTemplatePrefab = templatePrefab;
            this.roomInstances = new Dictionary<string, Room>();

            // 타입별 RoomData 캐시 생성
            CacheRoomDataByType();
        }

        private void CacheRoomDataByType()
        {
            roomDataByType = new Dictionary<RoomType, List<RoomData>>();

            if (roomDataPool == null) return;

            foreach (var data in roomDataPool)
            {
                if (data == null) continue;

                if (!roomDataByType.ContainsKey(data.roomType))
                {
                    roomDataByType[data.roomType] = new List<RoomData>();
                }
                roomDataByType[data.roomType].Add(data);
            }

            Debug.Log($"[RoomPlacer] RoomData 캐시 생성 완료: {roomDataByType.Count}개 타입");
        }


        // ====== 메인 메서드 ======

        /// <summary>
        /// 그래프 기반으로 Room 인스턴스 생성
        /// </summary>
        public Dictionary<string, Room> PlaceRooms(DungeonGraph graph, Transform container)
        {
            if (graph == null)
            {
                Debug.LogError("[RoomPlacer] DungeonGraph가 null입니다!");
                return new Dictionary<string, Room>();
            }

            roomContainer = container;
            roomInstances.Clear();

            Debug.Log($"[RoomPlacer] Room 배치 시작 - {graph.NodeCount}개 노드");

            // 각 노드에 대해 Room 생성
            foreach (var node in graph.nodes.Values)
            {
                var room = CreateRoomForNode(node);
                if (room != null)
                {
                    roomInstances[node.nodeId] = room;
                    node.roomInstance = room;
                }
            }

            // Portal 연결 설정
            ConfigureAllPortals(graph);

            Debug.Log($"[RoomPlacer] Room 배치 완료 - {roomInstances.Count}개 생성됨");

            return roomInstances;
        }


        // ====== Room 생성 ======

        /// <summary>
        /// 노드에 대응하는 Room 생성
        /// </summary>
        private Room CreateRoomForNode(DungeonNode node)
        {
            // RoomData 선택
            var roomData = SelectRoomData(node.roomType, node.floor);
            if (roomData == null)
            {
                Debug.LogWarning($"[RoomPlacer] Node {node.nodeId}에 적합한 RoomData를 찾을 수 없습니다. 타입: {node.roomType}");
                // 기본 Normal 타입으로 폴백
                roomData = SelectRoomData(RoomType.Normal, node.floor);
            }

            // Room 인스턴스 생성
            var room = InstantiateRoom(node, roomData);
            return room;
        }

        /// <summary>
        /// 노드 타입과 난이도에 맞는 RoomData 선택
        /// </summary>
        private RoomData SelectRoomData(RoomType type, int floor)
        {
            // 타입에 맞는 RoomData 목록 가져오기
            if (!roomDataByType.TryGetValue(type, out var candidates) || candidates.Count == 0)
            {
                // 타입이 없으면 Normal로 폴백
                if (type != RoomType.Normal && roomDataByType.TryGetValue(RoomType.Normal, out var normalCandidates))
                {
                    candidates = normalCandidates;
                }
                else
                {
                    return null;
                }
            }

            if (candidates.Count == 0) return null;

            // 층에 따른 난이도 계산 (1~10)
            int targetDifficulty = Mathf.Clamp(floor + 1, 1, 10);

            // 난이도가 가장 가까운 RoomData 선택 (약간의 랜덤성 추가)
            var sorted = candidates
                .OrderBy(d => Mathf.Abs(d.difficulty - targetDifficulty))
                .ThenBy(d => SeedManager.Value()) // 같은 난이도면 랜덤
                .ToList();

            // 상위 3개 중 랜덤 선택 (다양성 확보)
            int selectIndex = SeedManager.Range(0, Mathf.Min(3, sorted.Count));
            return sorted[selectIndex];
        }

        /// <summary>
        /// Room 프리팹 인스턴스화
        /// </summary>
        private Room InstantiateRoom(DungeonNode node, RoomData data)
        {
            if (roomTemplatePrefab == null)
            {
                Debug.LogError("[RoomPlacer] roomTemplatePrefab이 null입니다!");
                return null;
            }

            // Room 인스턴스 생성
            var roomObj = Object.Instantiate(roomTemplatePrefab, roomContainer);
            roomObj.name = $"Room_{node.nodeId}_{node.roomType}";
            roomObj.gameObject.SetActive(false); // 초기 비활성화

            // Room 컴포넌트 가져오기
            var room = roomObj.GetComponent<Room>();
            if (room == null)
            {
                Debug.LogError($"[RoomPlacer] {roomObj.name}에 Room 컴포넌트가 없습니다!");
                Object.Destroy(roomObj.gameObject);
                return null;
            }

            // RoomData 초기화
            if (data != null)
            {
                room.Initialize(data);
            }

            // 노드 정보 저장 (Room에 확장 필드가 필요할 수 있음)
            node.roomData = data;

            Debug.Log($"[RoomPlacer] Room 생성: {roomObj.name} (Data: {data?.roomName ?? "None"})");

            return room;
        }


        // ====== Portal 설정 ======

        /// <summary>
        /// 모든 Room의 Portal 연결 설정
        /// </summary>
        private void ConfigureAllPortals(DungeonGraph graph)
        {
            Debug.Log("[RoomPlacer] Portal 연결 설정 시작...");

            foreach (var node in graph.nodes.Values)
            {
                if (!roomInstances.TryGetValue(node.nodeId, out var room)) continue;

                ConfigurePortals(room, node, graph);
            }

            Debug.Log("[RoomPlacer] Portal 연결 설정 완료");
        }

        /// <summary>
        /// 개별 Room의 Portal 설정
        /// </summary>
        private void ConfigurePortals(Room room, DungeonNode node, DungeonGraph graph)
        {
            // Room의 Portal 컴포넌트들 가져오기
            var portals = room.GetComponentsInChildren<Portal>(true);

            if (portals == null || portals.Length == 0)
            {
                Debug.LogWarning($"[RoomPlacer] {room.name}에 Portal이 없습니다!");
                return;
            }

            // 나가는 연결 (다음 방)
            var outgoingNodes = graph.GetAdjacentNodes(node.nodeId);

            // 들어오는 연결 (이전 방)
            var incomingNodes = graph.GetPreviousNodes(node.nodeId);

            int portalIndex = 0;

            // 나가는 연결용 포탈 설정
            foreach (var targetNode in outgoingNodes)
            {
                if (portalIndex >= portals.Length) break;

                var portal = portals[portalIndex];
                ConfigureSinglePortal(portal, targetNode, isForward: true);
                portalIndex++;
            }

            // 들어오는 연결용 포탈 설정 (뒤로 가기)
            // 선택적: 뒤로 가기 기능이 필요한 경우
            // foreach (var sourceNode in incomingNodes) { ... }

            Debug.Log($"[RoomPlacer] {room.name}: {outgoingNodes.Count}개 출구 Portal 설정");
        }

        /// <summary>
        /// 개별 Portal 설정
        /// </summary>
        private void ConfigureSinglePortal(Portal portal, DungeonNode targetNode, bool isForward)
        {
            // Portal의 연결 대상 설정
            // 참고: Portal 클래스에 DungeonNode 관련 필드 추가 필요 (Phase 4에서 구현)

            // 현재는 Portal의 기본 타입 설정만
            // portal.SetTargetNode(targetNode); // Phase 4에서 구현

            Debug.Log($"[RoomPlacer] Portal 설정: → {targetNode.nodeId} ({targetNode.roomType})");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 특정 노드의 Room 인스턴스 가져오기
        /// </summary>
        public Room GetRoomInstance(string nodeId)
        {
            return roomInstances.TryGetValue(nodeId, out var room) ? room : null;
        }

        /// <summary>
        /// 모든 Room 인스턴스 가져오기
        /// </summary>
        public Dictionary<string, Room> GetAllRoomInstances()
        {
            return new Dictionary<string, Room>(roomInstances);
        }

        /// <summary>
        /// 모든 Room 정리
        /// </summary>
        public void ClearAllRooms()
        {
            foreach (var room in roomInstances.Values)
            {
                if (room != null)
                {
                    Object.Destroy(room.gameObject);
                }
            }
            roomInstances.Clear();

            Debug.Log("[RoomPlacer] 모든 Room 정리 완료");
        }
    }
}
