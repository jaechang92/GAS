using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Extensions;
using GASPT.Gameplay.Level.Graph;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// RoomManager - 그래프 기반 던전 관리 + 방 조회 + 디버그
    /// </summary>
    public partial class RoomManager
    {
        // ====== 그래프 기반 던전 관리 ======

        /// <summary>
        /// 던전 그래프 로드
        /// </summary>
        public void LoadDungeonGraph(DungeonGraph graph, Dictionary<string, Room> roomMap)
        {
            if (graph == null)
            {
                Debug.LogError("[RoomManager] DungeonGraph가 null입니다!");
                return;
            }

            // 기존 방 정리
            ClearRooms();

            // 그래프 설정
            dungeonGraph = graph;
            nodeRoomMap = roomMap ?? new Dictionary<string, Room>();

            // rooms 리스트도 업데이트 (하위 호환성)
            rooms.Clear();
            rooms.AddRange(nodeRoomMap.Values);
        }

        /// <summary>
        /// 특정 노드로 이동 (그래프 기반)
        /// </summary>
        public async Awaitable MoveToNodeAsync(DungeonNode targetNode)
        {
            if (targetNode == null)
            {
                Debug.LogError("[RoomManager] 대상 노드가 null입니다!");
                return;
            }

            if (dungeonGraph == null)
            {
                Debug.LogError("[RoomManager] 던전 그래프가 로드되지 않았습니다!");
                return;
            }

            // 대상 Room 가져오기
            if (!nodeRoomMap.TryGetValue(targetNode.nodeId, out var targetRoom))
            {
                Debug.LogError($"[RoomManager] 노드 {targetNode.nodeId}에 대응하는 Room이 없습니다!");
                return;
            }

            // 현재 방 비활성화
            if (currentRoom != null)
            {
                currentRoom.gameObject.SetActive(false);
            }

            // 그래프 상태 업데이트
            dungeonGraph.SetCurrentNode(targetNode.nodeId);

            // 새 방 활성화
            currentRoom = targetRoom;
            currentRoom.gameObject.SetActive(true);

            // 이벤트 발생
            OnRoomChanged?.Invoke(currentRoom);
            OnNodeChanged?.Invoke(targetNode);

            // 인접 노드 공개
            var adjacentNodes = dungeonGraph.GetAdjacentNodes(targetNode.nodeId);
            foreach (var adj in adjacentNodes)
            {
                adj.Reveal();
            }
            OnAdjacentNodesRevealed?.Invoke(adjacentNodes);

            // 방 진입 실행
            await currentRoom.EnterRoomAsync();
        }

        /// <summary>
        /// 노드 ID로 이동
        /// </summary>
        public async Awaitable MoveToNodeAsync(string nodeId)
        {
            var node = dungeonGraph?.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogError($"[RoomManager] 노드 {nodeId}를 찾을 수 없습니다!");
                return;
            }
            await MoveToNodeAsync(node);
        }

        /// <summary>
        /// 이동 가능한 다음 노드들 조회
        /// </summary>
        public List<DungeonNode> GetAvailableNextNodes()
        {
            if (dungeonGraph == null || dungeonGraph.currentNodeId == null)
            {
                return new List<DungeonNode>();
            }

            return dungeonGraph.GetAdjacentNodes(dungeonGraph.currentNodeId);
        }

        /// <summary>
        /// 미방문 다음 노드들 조회
        /// </summary>
        public List<DungeonNode> GetUnvisitedNextNodes()
        {
            if (dungeonGraph == null || dungeonGraph.currentNodeId == null)
            {
                return new List<DungeonNode>();
            }

            return dungeonGraph.GetUnvisitedAdjacentNodes(dungeonGraph.currentNodeId);
        }

        /// <summary>
        /// 그래프 기반 던전 시작 (Entry 노드로 이동)
        /// </summary>
        public async Awaitable StartGraphDungeonAsync()
        {
            if (dungeonGraph == null)
            {
                Debug.LogError("[RoomManager] 던전 그래프가 로드되지 않았습니다!");
                return;
            }

            // Entry 노드로 이동
            var entryNode = dungeonGraph.EntryNode;
            if (entryNode == null)
            {
                Debug.LogError("[RoomManager] Entry 노드를 찾을 수 없습니다!");
                return;
            }

            await MoveToNodeAsync(entryNode);
        }

        /// <summary>
        /// 노드 ID로 Room 가져오기
        /// </summary>
        public Room GetRoomByNodeId(string nodeId)
        {
            if (nodeRoomMap.TryGetValue(nodeId, out var room))
            {
                return room;
            }
            return null;
        }


        // ====== 방 조회 ======

        /// <summary>
        /// 특정 인덱스의 방 가져오기
        /// </summary>
        public Room GetRoom(int index)
        {
            if (index < 0 || index >= rooms.Count)
            {
                Debug.LogWarning($"[RoomManager] 잘못된 방 인덱스: {index}");
                return null;
            }

            return rooms[index];
        }

        /// <summary>
        /// 모든 방 가져오기
        /// </summary>
        public List<Room> GetAllRooms()
        {
            return new List<Room>(rooms);
        }


        // ====== 디버그 ======

        [ContextMenu("Print Room List")]
        private void PrintRoomList()
        {
            Debug.Log($"=== Room Manager ===");
            Debug.Log($"Total Rooms: {rooms.Count}");
            Debug.Log($"Current Room: {(currentRoom != null ? currentRoom.name : "None")} ({currentRoomIndex + 1}/{rooms.Count})");
            Debug.Log("Rooms:");

            for (int i = 0; i < rooms.Count; i++)
            {
                string status = (i == currentRoomIndex) ? "[CURRENT]" : "";
                string cleared = rooms[i].IsCleared ? "[CLEARED]" : "";
                Debug.Log($"  [{i}] {rooms[i].name} {status} {cleared}");
            }

            Debug.Log("====================");
        }

        [ContextMenu("Refresh Rooms")]
        private void DebugRefreshRooms()
        {
            RefreshRooms();
        }

        [ContextMenu("Start Dungeon (Test)")]
        private void TestStartDungeon()
        {
            if (Application.isPlaying)
            {
                StartDungeonAsync().Forget();
            }
            else
            {
                Debug.LogWarning("[RoomManager] Play 모드에서만 실행 가능합니다.");
            }
        }

        [ContextMenu("Move To Next Room (Test)")]
        private void TestMoveToNextRoom()
        {
            if (Application.isPlaying)
            {
                MoveToNextRoomAsync().Forget();
            }
            else
            {
                Debug.LogWarning("[RoomManager] Play 모드에서만 실행 가능합니다.");
            }
        }
    }
}
