using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Gameplay.Level.Generation;
using Random = UnityEngine.Random;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// RoomManager - 던전 로딩 관련 기능
    /// DungeonConfig 기반 동적 Room 로딩 지원
    /// </summary>
    public partial class RoomManager
    {
        // ====== 던전 로딩 ======

        /// <summary>
        /// DungeonConfig 기반으로 던전 로드 (그래프 기반 생성)
        /// 참고: 일반적으로 DungeonGenerator + LoadDungeonGraph를 사용하는 것을 권장
        /// </summary>
        public void LoadDungeon(DungeonConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[RoomManager] DungeonConfig가 null입니다!");
                return;
            }

            currentDungeon = config;

            // 기존 방들 정리
            ClearRooms();

            // Room Container 생성
            CreateRoomContainer();

            // 그래프 기반 던전 생성
            GenerateProceduralDungeon(config.generationRules, config.roomDataPool, config.roomTemplatePrefab);

            // 모든 방 이벤트 구독
            SubscribeRoomEvents();
        }

        /// <summary>
        /// Room Container 생성
        /// </summary>
        private void CreateRoomContainer()
        {
            if (roomContainer != null)
            {
                Destroy(roomContainer.gameObject);
            }

            GameObject containerObj = new GameObject("RoomContainer");
            roomContainer = containerObj.transform;
            roomContainer.SetParent(transform); // RoomManager의 자식으로
        }

        /// <summary>
        /// Prefab 방식: Room Prefab 로드
        /// </summary>
        private void LoadRoomsFromPrefabs(Room[] prefabs)
        {
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError("[RoomManager] roomPrefabs가 비어있습니다!");
                return;
            }

            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                {
                    Debug.LogWarning("[RoomManager] null Prefab을 건너뜁니다.");
                    continue;
                }

                // Prefab 인스턴스화
                Room room = Instantiate(prefab, roomContainer);
                room.gameObject.SetActive(false); // 초기 비활성화
                rooms.Add(room);
            }
        }

        /// <summary>
        /// Data 방식: RoomData로 Room 동적 생성
        /// </summary>
        private void GenerateRoomsFromData(RoomData[] dataList, Room templatePrefab)
        {
            if (dataList == null || dataList.Length == 0)
            {
                Debug.LogError("[RoomManager] roomDataList가 비어있습니다!");
                return;
            }

            if (templatePrefab == null)
            {
                Debug.LogError("[RoomManager] roomTemplatePrefab이 없습니다!");
                return;
            }

            foreach (var data in dataList)
            {
                if (data == null)
                {
                    Debug.LogWarning("[RoomManager] null RoomData를 건너뜁니다.");
                    continue;
                }

                // 템플릿 Prefab으로 Room 생성
                Room room = Instantiate(templatePrefab, roomContainer);
                room.gameObject.SetActive(false);
                room.name = $"Room_{data.roomName}";

                // RoomData 주입 (Room.Initialize 호출)
                room.Initialize(data);

                rooms.Add(room);
            }
        }

        /// <summary>
        /// Procedural 방식: 룰 기반 랜덤 던전 생성
        /// </summary>
        private void GenerateProceduralDungeon(RoomGenerationRules rules, RoomData[] pool, Room templatePrefab)
        {
            if (rules == null)
            {
                Debug.LogError("[RoomManager] generationRules가 없습니다!");
                return;
            }

            if (pool == null || pool.Length == 0)
            {
                Debug.LogError("[RoomManager] roomDataPool이 비어있습니다!");
                return;
            }

            if (templatePrefab == null)
            {
                Debug.LogError("[RoomManager] roomTemplatePrefab이 없습니다!");
                return;
            }

            // 방 개수 결정
            int roomCount = Random.Range(rules.minRooms, rules.maxRooms + 1);

            // 방 생성
            for (int i = 0; i < roomCount; i++)
            {
                // 진행도 계산 (0~1)
                float progress = roomCount > 1 ? (float)i / (roomCount - 1) : 0f;

                // RoomData 선택
                RoomData selectedData = SelectRoomDataForProcedural(pool, i, roomCount, progress, rules);

                if (selectedData == null)
                {
                    Debug.LogWarning($"[RoomManager] {i}번째 방의 RoomData 선택 실패!");
                    continue;
                }

                // Room 생성
                Room room = Instantiate(templatePrefab, roomContainer);
                room.gameObject.SetActive(false);
                room.name = $"Room_{i:D2}_{selectedData.roomName}";

                // RoomData 주입
                room.Initialize(selectedData);

                rooms.Add(room);
            }

            // 보스 방 추가
            if (rules.includeBossRoom)
            {
                AddProceduralBossRoom(pool, templatePrefab);
            }
        }

        /// <summary>
        /// Procedural용 RoomData 선택
        /// </summary>
        private RoomData SelectRoomDataForProcedural(RoomData[] pool, int index, int totalRooms, float progress, RoomGenerationRules rules)
        {
            // 첫 번째 방은 쉬운 방
            if (index == 0 && rules.firstRoomAlwaysEasy)
            {
                var easyRooms = System.Array.FindAll(pool, r => r.difficulty <= 2);
                return easyRooms.Length > 0 ? easyRooms[Random.Range(0, easyRooms.Length)] : pool[Random.Range(0, pool.Length)];
            }

            // 난이도 기반 선택
            int targetDifficulty = rules.GetDifficultyForProgress(progress);

            // 난이도가 가장 가까운 RoomData 찾기
            RoomData closestRoom = null;
            int minDiff = int.MaxValue;

            foreach (var data in pool)
            {
                // 보스 방은 제외
                if (data.roomType == RoomType.Boss) continue;

                int diff = Mathf.Abs(data.difficulty - targetDifficulty);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestRoom = data;
                }
            }

            return closestRoom ?? pool[Random.Range(0, pool.Length)];
        }

        /// <summary>
        /// Procedural 보스 방 추가
        /// </summary>
        private void AddProceduralBossRoom(RoomData[] pool, Room templatePrefab)
        {
            // 보스 방 RoomData 찾기
            var bossRooms = System.Array.FindAll(pool, r => r.roomType == RoomType.Boss);

            if (bossRooms.Length == 0)
            {
                Debug.LogWarning("[RoomManager] roomDataPool에 보스 방이 없습니다!");
                return;
            }

            RoomData bossData = bossRooms[Random.Range(0, bossRooms.Length)];

            // 보스 방 생성
            Room bossRoom = Instantiate(templatePrefab, roomContainer);
            bossRoom.gameObject.SetActive(false);
            bossRoom.name = $"Room_Boss_{bossData.roomName}";
            bossRoom.Initialize(bossData);

            rooms.Add(bossRoom);
        }

        /// <summary>
        /// 기존 방들 정리
        /// </summary>
        private void ClearRooms()
        {
            // 이벤트 구독 해제
            foreach (var room in rooms)
            {
                if (room != null)
                {
                    room.OnRoomEnter -= OnRoomEnter;
                    room.OnRoomClear -= OnRoomClear;
                    room.OnRoomFail -= OnRoomFail;
                }
            }

            rooms.Clear();

            // Room Container 제거
            if (roomContainer != null)
            {
                Destroy(roomContainer.gameObject);
                roomContainer = null;
            }
        }

        /// <summary>
        /// 모든 방 이벤트 구독
        /// </summary>
        private void SubscribeRoomEvents()
        {
            foreach (var room in rooms)
            {
                room.OnRoomEnter += OnRoomEnter;
                room.OnRoomClear += OnRoomClear;
                room.OnRoomFail += OnRoomFail;
            }
        }
    }
}
