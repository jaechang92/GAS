using UnityEngine;
using System;
using Core.Enums;

namespace Combat.Core
{
    /// <summary>
    /// 데미지 정보를 담는 구조체
    /// 공격 시 전달되는 모든 데미지 관련 데이터 포함
    /// </summary>
    [Serializable]
    public struct DamageData
    {
        [Header("기본 데미지 정보")]
        public float amount;                // 데미지 양
        public DamageType damageType;       // 데미지 타입

        [Header("추가 효과")]
        public Vector2 knockback;           // 넉백 벡터
        public bool canCritical;            // 크리티컬 가능 여부
        public float criticalMultiplier;    // 크리티컬 배율 (기본 1.5배)

        [Header("공격 정보")]
        public GameObject source;           // 공격 주체
        public GameObject instigator;       // 공격 실행자 (스킬의 경우 플레이어)
        public Vector3 hitPoint;            // 타격 지점
        public Vector3 hitNormal;           // 타격 방향

        [Header("상태 효과")]
        public float stunDuration;          // 스턴 시간
        public bool ignoreInvincibility;    // 무적 무시 여부

        /// <summary>
        /// 기본 데미지 데이터 생성
        /// </summary>
        public static DamageData Create(float amount, DamageType type, GameObject source)
        {
            return new DamageData
            {
                amount = amount,
                damageType = type,
                source = source,
                instigator = source,
                canCritical = false,
                criticalMultiplier = 1.5f,
                knockback = Vector2.zero,
                stunDuration = 0f,
                ignoreInvincibility = false
            };
        }

        /// <summary>
        /// 넉백 효과가 있는 데미지 생성
        /// </summary>
        public static DamageData CreateWithKnockback(float amount, DamageType type, GameObject source, Vector2 knockback)
        {
            var data = Create(amount, type, source);
            data.knockback = knockback;
            return data;
        }

        /// <summary>
        /// 크리티컬 가능한 데미지 생성
        /// </summary>
        public static DamageData CreateCritical(float amount, DamageType type, GameObject source, float critMultiplier = 1.5f)
        {
            var data = Create(amount, type, source);
            data.canCritical = true;
            data.criticalMultiplier = critMultiplier;
            return data;
        }

        /// <summary>
        /// 크리티컬 적용
        /// </summary>
        public void ApplyCritical()
        {
            if (canCritical)
            {
                amount *= criticalMultiplier;
            }
        }

        /// <summary>
        /// 데미지 배율 적용
        /// </summary>
        public void ApplyMultiplier(float multiplier)
        {
            amount *= multiplier;
        }
    }
}
