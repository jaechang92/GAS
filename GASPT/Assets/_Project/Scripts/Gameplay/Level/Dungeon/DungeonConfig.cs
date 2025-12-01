using UnityEngine;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 던전 설정 데이터
    /// Room 기반 + Procedural 경로 생성 방식
    /// - Room: 미리 디자인된 RoomData 풀에서 선택
    /// - 경로: GraphBuilder가 절차적으로 생성
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


        // ====== 경로 생성 설정 ======

        [Header("경로 생성 (Procedural)")]
        [Tooltip("그래프 생성 규칙 (층 수, 분기 확률, 특수 방 배치 등)")]
        public RoomGenerationRules generationRules;


        // ====== Room 설정 ======

        [Header("Room 설정")]
        [Tooltip("사용 가능한 RoomData 풀 (타입별로 선택됨)")]
        public RoomData[] roomDataPool;

        [Tooltip("Room 템플릿 Prefab (동적 Room 생성용)")]
        public Room roomTemplatePrefab;


        // ====== 유효성 검증 ======

        private void OnValidate()
        {
            if (generationRules == null)
            {
                Debug.LogWarning($"[DungeonConfig] {dungeonName}: generationRules가 설정되지 않았습니다!");
            }

            if (roomDataPool == null || roomDataPool.Length == 0)
            {
                Debug.LogWarning($"[DungeonConfig] {dungeonName}: roomDataPool이 비어있습니다!");
            }

            if (roomTemplatePrefab == null)
            {
                Debug.LogWarning($"[DungeonConfig] {dungeonName}: roomTemplatePrefab이 설정되지 않았습니다!");
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 설정이 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            return generationRules != null
                && roomDataPool != null
                && roomDataPool.Length > 0
                && roomTemplatePrefab != null;
        }


        // ====== 디버그 정보 ======

        public override string ToString()
        {
            return $"[DungeonConfig] {dungeonName} (Lv.{recommendedLevel})";
        }
    }
}
