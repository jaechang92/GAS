// ===================================
// 파일: Assets/Scripts/Ability/Core/AbilityCooldown.cs
// ===================================
using System;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티 쿨다운 관리 클래스
    /// </summary>
    [Serializable]
    public class AbilityCooldown
    {
        private float cooldownDuration;
        private float remainingTime;
        private bool isOnCooldown;
        private float lastUsedTime;

        // 이벤트
        public event Action OnCooldownStarted;
        public event Action OnCooldownCompleted;
        public event Action<float> OnCooldownUpdated;

        // 프로퍼티
        public float Duration => cooldownDuration;
        public float RemainingTime => remainingTime;
        public bool IsOnCooldown => isOnCooldown;
        public float Progress => cooldownDuration > 0 ? 1f - (remainingTime / cooldownDuration) : 1f;

        /// <summary>
        /// 쿨다운 초기화
        /// </summary>
        public void Initialize(float duration)
        {
            // 쿨다운 시간 설정
        }

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        public void StartCooldown()
        {
            // 쿨다운 타이머 시작
        }

        /// <summary>
        /// 쿨다운 시작 (커스텀 시간)
        /// </summary>
        public void StartCooldown(float customDuration)
        {
            // 특정 시간으로 쿨다운 시작
        }

        /// <summary>
        /// 쿨다운 업데이트 (매 프레임 호출)
        /// </summary>
        public void Update(float deltaTime)
        {
            // 남은 시간 감소 및 완료 체크
        }

        /// <summary>
        /// 쿨다운 즉시 완료
        /// </summary>
        public void CompleteCooldown()
        {
            // 쿨다운 강제 완료
        }

        /// <summary>
        /// 쿨다운 리셋
        /// </summary>
        public void Reset()
        {
            // 쿨다운 상태 초기화
        }

        /// <summary>
        /// 쿨다운 감소 (아이템/버프 효과)
        /// </summary>
        public void ReduceCooldown(float amount)
        {
            // 남은 쿨다운 시간 감소
        }

        /// <summary>
        /// 쿨다운 비율 감소 (0~1)
        /// </summary>
        public void ReduceCooldownByPercent(float percent)
        {
            // 퍼센트 기반 쿨다운 감소
        }
    }
}