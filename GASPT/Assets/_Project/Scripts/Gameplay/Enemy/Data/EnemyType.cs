using System;

namespace Enemy.Data
{
    /// <summary>
    /// 적 타입 분류
    /// </summary>
    [Serializable]
    public enum EnemyType
    {
        /// <summary>근접 공격형 적</summary>
        Melee = 0,

        /// <summary>원거리 공격형 적</summary>
        Ranged = 1,

        /// <summary>탱크형 적 (높은 체력, 느린 이동)</summary>
        Tank = 2,

        /// <summary>보스급 적</summary>
        Boss = 3
    }
}
