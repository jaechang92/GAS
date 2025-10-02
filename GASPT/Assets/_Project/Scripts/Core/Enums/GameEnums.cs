namespace Core.Enums
{
    /// <summary>
    /// 데미지 타입 정의
    /// 전체 게임 시스템에서 공통으로 사용
    /// </summary>
    public enum DamageType
    {
        Physical,       // 물리 데미지
        Magical,        // 마법 데미지
        True,           // 고정 데미지 (방어 무시)
        Environmental,  // 환경 데미지 (함정, 낙하 등)
        Healing,        // 힐링 (음수 데미지로 처리 가능)
        Shield,         // 실드 부여
        Poison,         // 독 데미지 (DoT)
        Fire,           // 화염 데미지
        Ice,            // 빙결 데미지
        Lightning,      // 번개 데미지
        Dark,           // 암흑 데미지
        Holy,           // 신성 데미지
        Custom          // 커스텀 (확장용)
    }

    /// <summary>
    /// 리소스 타입 정의
    /// 캐릭터 리소스 관리용
    /// </summary>
    public enum ResourceType
    {
        Health,         // 체력
        Mana,           // 마나
        Stamina,        // 스태미나
        Energy,         // 에너지
        Rage,           // 분노
        Focus,          // 집중력
        Soul,           // 영혼 (Skul 스타일)
        Bone,           // 뼈 (Skul 화폐)
        Custom          // 커스텀
    }

    /// <summary>
    /// 상태 이상 타입
    /// </summary>
    public enum StatusEffectType
    {
        None,           // 없음
        Stun,           // 기절
        Slow,           // 둔화
        Root,           // 속박
        Silence,        // 침묵 (스킬 사용 불가)
        Blind,          // 실명
        Burn,           // 화상
        Freeze,         // 빙결
        Poison,         // 중독
        Bleed,          // 출혈
        Knockback,      // 넉백
        Airborne,       // 공중에 뜸
        Invincible,     // 무적
        Invisible,      // 투명
        Custom          // 커스텀
    }

    /// <summary>
    /// 팀 정의
    /// </summary>
    public enum Team
    {
        Neutral,        // 중립
        Player,         // 플레이어
        Enemy,          // 적
        Ally,           // 아군
        Environment     // 환경 (함정 등)
    }

    /// <summary>
    /// 난이도
    /// </summary>
    public enum Difficulty
    {
        Easy,           // 쉬움
        Normal,         // 보통
        Hard,           // 어려움
        Expert,         // 전문가
        Master,         // 마스터
        Nightmare       // 악몽
    }

    /// <summary>
    /// 레어리티 (아이템/스컬 등급)
    /// </summary>
    public enum Rarity
    {
        Common,         // 일반
        Uncommon,       // 고급
        Rare,           // 희귀
        Epic,           // 영웅
        Legendary,      // 전설
        Mythic,         // 신화
        Unique          // 유니크
    }

    /// <summary>
    /// 방향 (4방향)
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// 방향 (8방향)
    /// </summary>
    public enum Direction8
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }
}
