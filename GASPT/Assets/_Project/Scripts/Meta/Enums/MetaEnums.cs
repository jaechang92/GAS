namespace GASPT.Meta
{
    /// <summary>
    /// 메타 재화 타입
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>
        /// Bone - 일반 재화 (적 처치, 상자에서 획득)
        /// </summary>
        Bone,

        /// <summary>
        /// Soul - 희귀 재화 (보스 처치, 스테이지 클리어)
        /// </summary>
        Soul
    }

    /// <summary>
    /// 영구 업그레이드 타입
    /// </summary>
    public enum UpgradeType
    {
        /// <summary>
        /// 최대 체력 증가
        /// </summary>
        MaxHP,

        /// <summary>
        /// 공격력 증가 (%)
        /// </summary>
        Attack,

        /// <summary>
        /// 방어력 증가 (받는 피해 감소 %)
        /// </summary>
        Defense,

        /// <summary>
        /// 이동속도 증가 (%)
        /// </summary>
        MoveSpeed,

        /// <summary>
        /// 골드 획득량 증가 (%)
        /// </summary>
        GoldBonus,

        /// <summary>
        /// 경험치 획득량 증가 (%)
        /// </summary>
        ExpBonus,

        /// <summary>
        /// 시작 골드 추가
        /// </summary>
        StartGold,

        /// <summary>
        /// 추가 대시 횟수
        /// </summary>
        ExtraDash,

        /// <summary>
        /// 런당 부활 횟수
        /// </summary>
        Revive
    }

    /// <summary>
    /// 업적 카테고리
    /// </summary>
    public enum AchievementCategory
    {
        /// <summary>
        /// 진행 업적 (적 처치, 스테이지 도달 등)
        /// </summary>
        Progress,

        /// <summary>
        /// 도전 업적 (노데미지, 시간 제한 등)
        /// </summary>
        Challenge,

        /// <summary>
        /// 수집 업적 (폼, 아이템 수집)
        /// </summary>
        Collection
    }

    /// <summary>
    /// 해금 가능한 콘텐츠 타입
    /// </summary>
    public enum UnlockableType
    {
        /// <summary>
        /// 폼 해금
        /// </summary>
        Form,

        /// <summary>
        /// 아이템 해금 (드롭 풀에 추가)
        /// </summary>
        Item,

        /// <summary>
        /// 스킬 해금
        /// </summary>
        Skill
    }
}
