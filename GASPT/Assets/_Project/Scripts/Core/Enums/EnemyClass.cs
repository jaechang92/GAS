namespace GASPT.Core.Enums
{
    /// <summary>
    /// 적 클래스 분류 (전투 스타일/행동 패턴 기준)
    /// 어떤 Enemy 컴포넌트를 사용할지 결정
    /// </summary>
    public enum EnemyClass
    {
        /// <summary>
        /// 근접 공격 적 (BasicMeleeEnemy)
        /// 플레이어에게 접근하여 근접 공격
        /// </summary>
        BasicMelee,

        /// <summary>
        /// 원거리 공격 적 (RangedEnemy)
        /// 일정 거리에서 투사체 발사
        /// </summary>
        Ranged,

        /// <summary>
        /// 비행 적 (FlyingEnemy)
        /// 공중에서 이동하며 공격
        /// </summary>
        Flying,

        /// <summary>
        /// 엘리트 적 (EliteEnemy)
        /// 특수 패턴과 강화된 능력
        /// </summary>
        Elite,

        /// <summary>
        /// 보스 적 (BossEnemy)
        /// 다단계 전투 및 복잡한 패턴
        /// </summary>
        Boss
    }
}
