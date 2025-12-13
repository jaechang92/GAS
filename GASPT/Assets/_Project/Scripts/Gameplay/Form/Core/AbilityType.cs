namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 어빌리티 타입 Enum
    /// 각 스킬의 종류를 정의
    /// </summary>
    public enum AbilityType
    {
        None,              // 없음 (빈 슬롯)

        // ===== 마법사 스킬 =====
        MagicMissile,      // 마법 미사일 (기본 공격)
        Fireball,          // 화염구 (폭발 AOE)
        IceBlast,          // 빙결 (범위 공격 + 슬로우)
        LightningBolt,     // 번개 (관통 공격)
        Teleport,          // 순간이동
        Shield,            // 보호막 (무적)

        // ===== 전사 스킬 =====
        SwordSlash,        // 검 베기 (기본 공격)
        Charge,            // 돌진 (전방 돌진 + 대미지)
        ShieldBash,        // 방패 강타 (스턴)
        WarCry,            // 함성 (공격력 버프)

        // ===== 암살자 스킬 =====
        DaggerStrike,      // 단검 공격 (기본 공격)
        ShadowStrike,      // 그림자 일격 (순간이동 + 공격)
        Backstab,          // 백스탭 (후방 크리티컬)
        SmokeScreen,       // 연막 (짧은 무적)

        // ===== 공용 스킬 =====
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
