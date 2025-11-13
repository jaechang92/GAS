namespace GASPT.Form
{
    /// <summary>
    /// 어빌리티 타입 Enum
    /// 각 스킬의 종류를 정의
    /// </summary>
    public enum AbilityType
    {
        None,              // 없음 (빈 슬롯)

        // 기본 공격
        MagicMissile,      // 마법 미사일 (기본 공격)

        // 원거리 공격
        Fireball,          // 화염구 (폭발 AOE)
        IceBlast,          // 빙결 (범위 공격 + 슬로우)
        LightningBolt,     // 번개 (관통 공격)

        // 유틸리티
        Teleport,          // 순간이동
        Shield,            // 보호막 (무적)

        // 미래 확장용
        ShadowStrike,      // 암살자용 스킬
        Charge,            // 전사용 돌진
        Heal               // 회복 스킬
    }

    /// <summary>
    /// 스킬 아이템 희귀도
    /// UI 색상 및 드롭률에 영향
    /// </summary>
    public enum SkillRarity
    {
        Common,            // 일반 (흰색)
        Rare,              // 희귀 (파란색)
        Epic,              // 영웅 (보라색)
        Legendary          // 전설 (주황색)
    }
}
