namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 시스템 관련 상수 정의
    /// </summary>
    public static class InventoryConstants
    {
        // ====== 인벤토리 용량 ======

        /// <summary>
        /// 기본 인벤토리 크기
        /// </summary>
        public const int DefaultCapacity = 30;

        /// <summary>
        /// 최소 인벤토리 크기
        /// </summary>
        public const int MinCapacity = 10;

        /// <summary>
        /// 최대 인벤토리 크기
        /// </summary>
        public const int MaxCapacity = 100;

        /// <summary>
        /// 용량 확장 단위
        /// </summary>
        public const int ExpandUnit = 10;


        // ====== 퀵슬롯 ======

        /// <summary>
        /// 퀵슬롯 개수
        /// </summary>
        public const int QuickSlotCount = 5;


        // ====== 스택 ======

        /// <summary>
        /// 기본 최대 스택 수
        /// </summary>
        public const int DefaultMaxStack = 99;


        // ====== 드롭 ======

        /// <summary>
        /// 드롭 아이템 최대 유지 시간 (초)
        /// </summary>
        public const float DroppedItemLifetime = 300f;

        /// <summary>
        /// 드롭 아이템 자석 범위 (m)
        /// </summary>
        public const float MagnetRange = 2f;

        /// <summary>
        /// 드롭 아이템 픽업 범위 (m)
        /// </summary>
        public const float PickupRange = 1.5f;


        // ====== 아이템 상호작용 ======

        /// <summary>
        /// 쿨다운 최소 시간 (초)
        /// </summary>
        public const float MinCooldown = 0.1f;

        /// <summary>
        /// 장비 내구도 감소량 (피격당)
        /// </summary>
        public const int DurabilityLossOnHit = 1;


        // ====== UI 관련 ======

        /// <summary>
        /// 인벤토리 그리드 열 개수
        /// </summary>
        public const int GridColumns = 6;

        /// <summary>
        /// 슬롯 크기 (px)
        /// </summary>
        public const float SlotSize = 64f;

        /// <summary>
        /// 슬롯 간격 (px)
        /// </summary>
        public const float SlotSpacing = 4f;
    }
}
