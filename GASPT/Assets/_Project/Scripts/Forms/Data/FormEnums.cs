namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 폼의 속성 타입
    /// 각 타입은 고유한 플레이 스타일과 스킬 특성을 가짐
    /// </summary>
    public enum FormType
    {
        /// <summary>기본 - 밸런스형</summary>
        Basic = 0,

        /// <summary>화염 마법사 - 범위 공격, 높은 데미지</summary>
        Flame = 1,

        /// <summary>얼음 마법사 - CC 특화, 안전한 원거리</summary>
        Frost = 2,

        /// <summary>번개 마법사 - 빠른 공격, 높은 기동성</summary>
        Thunder = 3,

        /// <summary>암흑 마법사 - 흡혈, 고위험 고보상</summary>
        Dark = 4,

        /// <summary>마법사 - 원거리 마법 공격</summary>
        Mage = 5,

        /// <summary>전사 - 근접 전투</summary>
        Warrior = 6,

        /// <summary>암살자 - 빠른 공격, 회피</summary>
        Assassin = 7,

        /// <summary>탱커 - 방어 중심</summary>
        Tank = 8
    }

    /// <summary>
    /// 폼의 등급 (각성 단계)
    /// 동일 폼 획득 시 등급 상승
    /// </summary>
    public enum FormRarity
    {
        /// <summary>일반 등급 - 기본 상태</summary>
        Common = 0,

        /// <summary>레어 등급 - 1회 각성</summary>
        Rare = 1,

        /// <summary>유니크 등급 - 2회 각성</summary>
        Unique = 2,

        /// <summary>레전더리 등급 - 최종 각성</summary>
        Legendary = 3
    }
}
