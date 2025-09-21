// ===================================
// 파일: Assets/Scripts/Ability/Integration/AbilityEventManager.cs
// ===================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티 시스템 전역 이벤트 관리자
    /// </summary>
    public class AbilityEventManager : MonoBehaviour
    {
        private static AbilityEventManager instance;
        public static AbilityEventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AbilityEventManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AbilityEventManager");
                        instance = go.AddComponent<AbilityEventManager>();
                    }
                }
                return instance;
            }
        }

        // 이벤트 정의
        public event Action<Ability> OnAbilityUsed;
        public event Action<Ability> OnAbilityCooldownStarted;
        public event Action<Ability> OnAbilityCooldownCompleted;
        public event Action<string, float> OnAbilityDamageDealt;
        public event Action<string, float> OnAbilityHealDealt;
        public event Action<string> OnAbilityBlocked;
        public event Action<string> OnAbilityInterrupted;

        /// <summary>
        /// 어빌리티 사용 이벤트 발생
        /// </summary>
        public void TriggerAbilityUsed(Ability ability)
        {
            // 어빌리티 사용 알림
        }

        /// <summary>
        /// 쿨다운 시작 이벤트 발생
        /// </summary>
        public void TriggerCooldownStarted(Ability ability)
        {
            // 쿨다운 시작 알림
        }

        /// <summary>
        /// 쿨다운 완료 이벤트 발생
        /// </summary>
        public void TriggerCooldownCompleted(Ability ability)
        {
            // 쿨다운 완료 알림
        }

        /// <summary>
        /// 데미지 발생 이벤트
        /// </summary>
        public void TriggerDamageDealt(string abilityId, float damage)
        {
            // 데미지 처리 알림
        }

        /// <summary>
        /// 힐 발생 이벤트
        /// </summary>
        public void TriggerHealDealt(string abilityId, float healAmount)
        {
            // 힐 처리 알림
        }

        /// <summary>
        /// 모든 이벤트 리스너 제거
        /// </summary>
        public void ClearAllListeners()
        {
            // 이벤트 정리
        }
    }
}