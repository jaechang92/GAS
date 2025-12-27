using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 방 생성기
    /// 스테이지별 방 배치 및 타입 결정
    /// </summary>
    public class RoomGenerator : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("방 생성 설정")]
        [SerializeField]
        [Tooltip("스테이지당 방 수")]
        private int roomsPerStage = 5;

        [SerializeField]
        [Tooltip("보스 방 위치 (1부터 시작)")]
        private int bossRoomPosition = 5;


        [Header("방 타입 확률")]
        [SerializeField]
        [Range(0f, 1f)]
        private float eliteRoomChance = 0.2f;

        [SerializeField]
        [Range(0f, 1f)]
        private float shopRoomChance = 0.15f;

        [SerializeField]
        [Range(0f, 1f)]
        private float treasureRoomChance = 0.1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float restRoomChance = 0.1f;


        // ====== 상태 ======

        private List<RoomType> generatedRooms = new List<RoomType>();
        private int currentStage = 1;


        // ====== 싱글톤 ======

        private static RoomGenerator instance;
        public static RoomGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<RoomGenerator>();
                }
                return instance;
            }
        }


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }


        // ====== 방 생성 ======

        /// <summary>
        /// 스테이지의 방 배치 생성
        /// </summary>
        public List<RoomType> GenerateStageRooms(int stage)
        {
            currentStage = stage;
            generatedRooms.Clear();

            for (int i = 0; i < roomsPerStage; i++)
            {
                int roomIndex = i + 1; // 1부터 시작
                RoomType roomType = DetermineRoomType(roomIndex, stage);
                generatedRooms.Add(roomType);
            }

            Debug.Log($"[RoomGenerator] 스테이지 {stage} 방 생성: {string.Join(", ", generatedRooms)}");

            return new List<RoomType>(generatedRooms);
        }

        /// <summary>
        /// 방 타입 결정
        /// </summary>
        private RoomType DetermineRoomType(int roomIndex, int stage)
        {
            // 첫 번째 방은 시작 방
            if (roomIndex == 1)
            {
                return RoomType.Start;
            }

            // 마지막 방 (보스 위치)은 보스 방
            if (roomIndex == bossRoomPosition)
            {
                return RoomType.Boss;
            }

            // 중간 방들은 확률 기반
            return DetermineRandomRoomType(roomIndex, stage);
        }

        /// <summary>
        /// 랜덤 방 타입 결정
        /// </summary>
        private RoomType DetermineRandomRoomType(int roomIndex, int stage)
        {
            float roll = Random.value;
            float cumulative = 0f;

            // 상점 (2~3번째 방에서 높은 확률)
            float shopBonus = (roomIndex == 2 || roomIndex == 3) ? 0.1f : 0f;
            cumulative += shopRoomChance + shopBonus;
            if (roll < cumulative)
            {
                // 상점은 스테이지당 1개만
                if (!generatedRooms.Contains(RoomType.Shop))
                    return RoomType.Shop;
            }

            // 휴식 (4번째 방, 보스 전에 높은 확률)
            float restBonus = (roomIndex == bossRoomPosition - 1) ? 0.2f : 0f;
            cumulative += restRoomChance + restBonus;
            if (roll < cumulative)
            {
                if (!generatedRooms.Contains(RoomType.Rest))
                    return RoomType.Rest;
            }

            // 보물
            cumulative += treasureRoomChance;
            if (roll < cumulative)
            {
                return RoomType.Treasure;
            }

            // 엘리트 (후반 스테이지에서 높은 확률)
            float eliteBonus = stage >= 3 ? 0.1f : 0f;
            cumulative += eliteRoomChance + eliteBonus;
            if (roll < cumulative)
            {
                return RoomType.Elite;
            }

            // 기본은 일반 전투 방
            return RoomType.Normal;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 특정 방 타입 개수 반환
        /// </summary>
        public int CountRoomType(RoomType type)
        {
            int count = 0;
            foreach (var room in generatedRooms)
            {
                if (room == type) count++;
            }
            return count;
        }

        /// <summary>
        /// 생성된 방 목록 반환
        /// </summary>
        public IReadOnlyList<RoomType> GetGeneratedRooms()
        {
            return generatedRooms.AsReadOnly();
        }

        /// <summary>
        /// 특정 인덱스의 방 타입 반환
        /// </summary>
        public RoomType GetRoomType(int index)
        {
            if (index < 0 || index >= generatedRooms.Count)
            {
                Debug.LogWarning($"[RoomGenerator] 유효하지 않은 방 인덱스: {index}");
                return RoomType.Normal;
            }

            return generatedRooms[index];
        }

        /// <summary>
        /// 보스 방 인덱스 반환 (0부터 시작)
        /// </summary>
        public int GetBossRoomIndex()
        {
            return bossRoomPosition - 1;
        }


        // ====== 디버그 ======

        [ContextMenu("테스트 생성")]
        private void DebugGenerateRooms()
        {
            GenerateStageRooms(1);
        }

        public string GetDebugInfo()
        {
            if (generatedRooms.Count == 0)
                return "방이 생성되지 않음";

            string info = $"스테이지 {currentStage} ({generatedRooms.Count}개 방):\n";

            for (int i = 0; i < generatedRooms.Count; i++)
            {
                info += $"  [{i + 1}] {generatedRooms[i]}\n";
            }

            return info;
        }
    }
}
