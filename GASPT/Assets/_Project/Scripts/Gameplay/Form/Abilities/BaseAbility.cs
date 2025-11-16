using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GASPT.Form
{
    /// <summary>
    /// 모든 Ability의 기본 클래스
    /// 쿨다운 로직 및 공통 기능 제공
    /// </summary>
    public abstract class BaseAbility : IAbility
    {
        // ====== IAbility 구현 ======

        /// <summary>
        /// Ability 이름 (자식 클래스에서 오버라이드)
        /// </summary>
        public abstract string AbilityName { get; }

        /// <summary>
        /// 쿨다운 시간 (초 단위, 자식 클래스에서 오버라이드)
        /// </summary>
        public abstract float Cooldown { get; }

        /// <summary>
        /// Ability 실행 (자식 클래스에서 구현)
        /// </summary>
        public abstract Task ExecuteAsync(GameObject caster, CancellationToken token);


        // ====== 쿨다운 관리 ======

        /// <summary>
        /// 마지막 사용 시간
        /// </summary>
        protected float lastUsedTime;

        /// <summary>
        /// 쿨다운 체크
        /// </summary>
        /// <returns>사용 가능하면 true, 쿨다운 중이면 false</returns>
        protected bool CheckCooldown()
        {
            if (Time.time - lastUsedTime < Cooldown)
            {
                Debug.Log($"[{AbilityName}] 쿨다운 중... (남은 시간: {Cooldown - (Time.time - lastUsedTime):F1}초)");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 쿨다운 시작 (Ability 사용 후 호출)
        /// </summary>
        protected void StartCooldown()
        {
            lastUsedTime = Time.time;
        }

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float RemainingCooldown => Mathf.Max(0f, Cooldown - (Time.time - lastUsedTime));

        /// <summary>
        /// 사용 가능 여부
        /// </summary>
        public bool IsReady => Time.time - lastUsedTime >= Cooldown;
    }
}
