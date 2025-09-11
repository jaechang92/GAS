// ===================================
// 파일: Assets/Scripts/Ability/Sample/SampleCharacter.cs
// ===================================
using UnityEngine;
using System.Collections.Generic;

namespace AbilitySystem
{
    /// <summary>
    /// 테스트용 캐릭터 구현체
    /// </summary>
    public class SampleCharacter : MonoBehaviour, IAbilityTarget
    {
        [Header("캐릭터 스탯")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private bool isInvulnerable = false;

        [Header("이펙트")]
        [SerializeField] private GameObject damageEffectPrefab;
        [SerializeField] private GameObject healEffectPrefab;

        // IAbilityTarget 구현
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool IsAlive => currentHealth > 0;
        public bool IsTargetable => IsAlive && !isInvulnerable;

        /// <summary>
        /// 캐릭터 초기화
        /// </summary>
        private void Start()
        {
            // 체력 및 컴포넌트 초기화
        }

        /// <summary>
        /// 데미지 받기 (IAbilityTarget 구현)
        /// </summary>
        public void TakeDamage(float damage, GameObject source)
        {
            // 데미지 처리 및 이펙트
        }

        /// <summary>
        /// 힐 받기 (IAbilityTarget 구현)
        /// </summary>
        public void Heal(float amount, GameObject source)
        {
            // 체력 회복 처리
        }

        /// <summary>
        /// 버프/디버프 적용 (IAbilityTarget 구현)
        /// </summary>
        public void ApplyEffect(string effectId, float duration, GameObject source)
        {
            // 상태 효과 적용
        }

        /// <summary>
        /// 캐릭터 사망 처리
        /// </summary>
        private void Die()
        {
            // 사망 처리 로직
        }

        /// <summary>
        /// 체력 바 업데이트
        /// </summary>
        private void UpdateHealthBar()
        {
            // UI 체력바 갱신
        }

        /// <summary>
        /// 무적 상태 설정
        /// </summary>
        public void SetInvulnerable(float duration)
        {
            // 일시적 무적 상태
        }
    }
}