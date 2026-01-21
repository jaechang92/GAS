using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Utilities;
using GASPT.Data;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 모든 Ability의 기본 클래스
    /// 쿨다운 로직 및 공통 기능 제공
    /// AbilityData를 통한 데이터 드리븐 방식 지원
    /// </summary>
    public abstract class BaseAbility : IAbility
    {
        // ====== 데이터 참조 ======

        /// <summary>
        /// 어빌리티 데이터 (ScriptableObject)
        /// null인 경우 하드코딩된 기본값 사용
        /// </summary>
        protected AbilityData abilityData;


        // ====== IAbility 구현 ======

        /// <summary>
        /// Ability 이름 (데이터 우선, 없으면 자식 클래스 값)
        /// </summary>
        public virtual string AbilityName => abilityData != null ? abilityData.abilityName : GetDefaultAbilityName();

        /// <summary>
        /// 쿨다운 시간 (데이터 우선, 없으면 자식 클래스 값)
        /// </summary>
        public virtual float Cooldown => abilityData != null ? abilityData.cooldown : GetDefaultCooldown();

        /// <summary>
        /// 기본 데미지 (데이터 우선)
        /// </summary>
        public virtual int BaseDamage => abilityData != null ? abilityData.baseDamage : GetDefaultBaseDamage();

        /// <summary>
        /// 기본 범위 (데이터 우선)
        /// </summary>
        public virtual float BaseRange => abilityData != null ? abilityData.baseRange : GetDefaultBaseRange();

        /// <summary>
        /// 마나 소모량 (데이터 우선)
        /// </summary>
        public virtual int ManaCost => abilityData != null ? abilityData.manaCost : GetDefaultManaCost();

        /// <summary>
        /// Ability 실행 (자식 클래스에서 구현)
        /// </summary>
        public abstract Task ExecuteAsync(GameObject caster, CancellationToken token);


        // ====== 기본값 (자식 클래스에서 오버라이드) ======

        protected virtual string GetDefaultAbilityName() => "Unknown Ability";
        protected virtual float GetDefaultCooldown() => 1f;
        protected virtual int GetDefaultBaseDamage() => 0;
        protected virtual float GetDefaultBaseRange() => 0f;
        protected virtual int GetDefaultManaCost() => 0;


        // ====== 생성자 ======

        /// <summary>
        /// 기본 생성자 (하드코딩된 값 사용)
        /// </summary>
        protected BaseAbility()
        {
            abilityData = null;
        }

        /// <summary>
        /// 데이터 기반 생성자
        /// </summary>
        /// <param name="data">어빌리티 데이터</param>
        protected BaseAbility(AbilityData data)
        {
            abilityData = data;
        }


        // ====== 데이터 설정 ======

        /// <summary>
        /// 어빌리티 데이터 설정 (런타임에서 데이터 변경)
        /// </summary>
        /// <param name="data">어빌리티 데이터</param>
        public void SetAbilityData(AbilityData data)
        {
            abilityData = data;
        }


        // ====== 쿨다운 관리 (Cooldown struct 사용) ======

        /// <summary>
        /// 쿨다운 타이머
        /// </summary>
        protected Cooldown cooldownTimer;

        /// <summary>
        /// 쿨다운 체크
        /// </summary>
        /// <returns>사용 가능하면 true, 쿨다운 중이면 false</returns>
        protected bool CheckCooldown()
        {
            // Duration 동기화
            cooldownTimer.Duration = Cooldown;

            if (cooldownTimer.IsOnCooldown)
            {
                Debug.Log($"[{AbilityName}] 쿨다운 중... (남은 시간: {cooldownTimer.RemainingTime:F1}초)");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 쿨다운 시작 (Ability 사용 후 호출)
        /// </summary>
        protected void StartCooldown()
        {
            cooldownTimer.Duration = Cooldown;
            cooldownTimer.Start();
        }

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float RemainingCooldown => cooldownTimer.RemainingTime;

        /// <summary>
        /// 사용 가능 여부
        /// </summary>
        public bool IsReady => cooldownTimer.IsReady;

        /// <summary>
        /// 쿨다운 진행률 (0~1, UI용)
        /// </summary>
        public float CooldownProgress => cooldownTimer.Progress;
    }
}
