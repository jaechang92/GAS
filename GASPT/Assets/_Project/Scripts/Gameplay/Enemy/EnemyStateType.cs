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

        /// <summary>추적 상태 (구 Chase)</summary>
        Trace,

        /// <summary>공격 상태</summary>
        Attack,

        /// <summary>피격 상태</summary>
        Hit,

        /// <summary>사망 상태</summary>
        Death,

        /// <summary>하위 호환성을 위한 Chase (Trace의 별칭)</summary>
        [System.Obsolete("Chase는 Trace로 변경되었습니다. Trace를 사용하세요.", false)]
        Chase = Trace
    }
}
