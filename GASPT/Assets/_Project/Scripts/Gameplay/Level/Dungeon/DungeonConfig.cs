using UnityEngine;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 던전 설정 데이터
    /// - Prefab 방식: 미리 디자인된 Room Prefab 사용
    /// - Data 방식: RoomData로 Room 동적 생성
    /// - Procedural 방식: 룰 기반 자동 생성
    /// </summary>
    [CreateAssetMenu(fileName = "DungeonConfig", menuName = "GASPT/Level/Dungeon Config")]
    public class DungeonConfig : ScriptableObject
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("던전 이름")]
        public string dungeonName = "New Dungeon";

        [Tooltip("권장 레벨")]
        [Range(1, 50)]
        public int recommendedLevel = 1;

        [Tooltip("던전 설명")]
        [TextArea(3, 5)]
        public string description = "";


        // ====== 생성 방식 ======

        [Header("생성 방식")]
        [Tooltip("Room 생성 방식")]
        public DungeonGenerationType generationType = DungeonGenerationType.Prefab;


        // ====== Prefab 방식 ======

        [Header("Prefab 방식 (generationType = Prefab)")]
        [Tooltip("미리 디자인된 Room Prefab 배열 (순서대로 로드됨)")]
        public Room[] roomPrefabs;


        // ====== Data 방식 ======

        [Header("Data 방식 (generationType = Data)")]
        [Tooltip("RoomData 배열 (동적으로 Room 생성)")]
        public RoomData[] roomDataList;

        [Tooltip("Room 템플릿 Prefab (빈 Room)")]
        public Room roomTemplatePrefab;


        // ====== Procedural 방식 ======

        [Header("Procedural 방식 (generationType = Procedural)")]
        [Tooltip("생성 룰")]
        public RoomGenerationRules generationRules;

        [Tooltip("사용 가능한 RoomData 풀")]
        public RoomData[] roomDataPool;


        // ====== 유효성 검증 ======

        private void OnValidate()
        {
            switch (generationType)
            {
                case DungeonGenerationType.Prefab:
                    if (roomPrefabs == null || roomPrefabs.Length == 0)
                    {
                        Debug.LogWarning($"[DungeonConfig] {dungeonName}: Prefab 방식인데 roomPrefabs가 비어있습니다!");
                    }
                    break;

                case DungeonGenerationType.Data:
                    if (roomDataList == null || roomDataList.Length == 0)
                    {
                        Debug.LogWarning($"[DungeonConfig] {dungeonName}: Data 방식인데 roomDataList가 비어있습니다!");
                    }
                    if (roomTemplatePrefab == null)
                    {
                        Debug.LogWarning($"[DungeonConfig] {dungeonName}: Data 방식인데 roomTemplatePrefab이 없습니다!");
                    }
                    break;

                case DungeonGenerationType.Procedural:
                    if (roomDataPool == null || roomDataPool.Length == 0)
                    {
                        Debug.LogWarning($"[DungeonConfig] {dungeonName}: Procedural 방식인데 roomDataPool이 비어있습니다!");
                    }
                    if (generationRules == null)
                    {
                        Debug.LogWarning($"[DungeonConfig] {dungeonName}: Procedural 방식인데 generationRules가 없습니다!");
                    }
                    break;
            }
        }


        // ====== 디버그 정보 ======

        public override string ToString()
        {
            return $"[DungeonConfig] {dungeonName} (Lv.{recommendedLevel}) - {generationType} 방식";
        }
    }


    // ====== 열거형 ======

    /// <summary>
    /// 던전 생성 방식
    /// </summary>
    public enum DungeonGenerationType
    {
        Prefab,         // Prefab 기반 (고정 레이아웃)
        Data,           // Data 기반 (동적 생성)
        Procedural      // Procedural 생성 (랜덤)
    }
}
