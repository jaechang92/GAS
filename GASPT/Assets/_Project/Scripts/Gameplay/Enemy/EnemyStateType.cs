namespace Enemy
{
    /// <summary>
    /// 적 상태 타입
    /// </summary>
    public enum EnemyStateType
    {
        /// <summary>대기 상태</summary>
        Idle,

        /// <summary>정찰 상태</summary>
        Patrol,

        /// <summary>추적 상태</summary>
        Chase,

        /// <summary>공격 상태</summary>
        Attack,

        /// <summary>피격 상태</summary>
        Hit,

        /// <summary>사망 상태</summary>
        Death
    }
}
