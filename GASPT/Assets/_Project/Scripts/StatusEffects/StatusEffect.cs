using System;
using Core.Enums;
using UnityEngine;

namespace GASPT.StatusEffects
{
    /// <summary>
    /// 상태 이상 효과 기본 클래스
    /// 버프/디버프 및 DoT 효과 관리
    /// </summary>
    public class StatusEffect
    {
        // ====== 기본 정보 ======

        /// <summary>
        /// 효과 타입
        /// </summary>
        public StatusEffectType EffectType { get; private set; }

        /// <summary>
        /// 효과 이름 (표시용)
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// 효과 설명
        /// </summary>
        public string Description { get; private set; }


        // ====== 효과 수치 ======

        /// <summary>
        /// 효과 값 (공격력 +10, 방어력 -5 등)
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// 지속 시간 (초)
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// 틱 간격 (DoT의 경우, 초)
        /// 0이면 즉시 적용 효과
        /// </summary>
        public float TickInterval { get; private set; }


        // ====== 상태 관리 ======

        /// <summary>
        /// 활성 상태
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// 남은 시간 (초)
        /// </summary>
        public float RemainingTime { get; private set; }

        /// <summary>
        /// 중첩 횟수
        /// </summary>
        public int StackCount { get; private set; }

        /// <summary>
        /// 최대 중첩 수
        /// </summary>
        public int MaxStack { get; private set; }

        /// <summary>
        /// 다음 틱까지 남은 시간
        /// </summary>
        private float nextTickTime;


        // ====== 대상 ======

        /// <summary>
        /// 효과가 적용된 대상 (GameObject)
        /// </summary>
        public GameObject Target { get; private set; }


        // ====== 이벤트 ======

        /// <summary>
        /// 효과 적용 시 이벤트
        /// 매개변수: (StatusEffect)
        /// </summary>
        public event Action<StatusEffect> OnApplied;

        /// <summary>
        /// 효과 제거 시 이벤트
        /// 매개변수: (StatusEffect)
        /// </summary>
        public event Action<StatusEffect> OnRemoved;

        /// <summary>
        /// 효과 틱 발생 시 이벤트 (DoT)
        /// 매개변수: (StatusEffect, tickDamage)
        /// </summary>
        public event Action<StatusEffect, float> OnTick;


        // ====== 생성자 ======

        /// <summary>
        /// StatusEffect 생성자
        /// </summary>
        public StatusEffect(
            StatusEffectType effectType,
            string displayName,
            string description,
            float value,
            float duration,
            float tickInterval = 0f,
            int maxStack = 1)
        {
            EffectType = effectType;
            DisplayName = displayName;
            Description = description;
            Value = value;
            Duration = duration;
            TickInterval = tickInterval;
            MaxStack = maxStack;

            IsActive = false;
            RemainingTime = 0f;
            StackCount = 0;
            nextTickTime = 0f;
        }


        // ====== 효과 적용 ======

        /// <summary>
        /// 효과 적용 시작
        /// </summary>
        /// <param name="target">효과를 받을 대상</param>
        public void Apply(GameObject target)
        {
            Target = target;
            IsActive = true;
            RemainingTime = Duration;
            nextTickTime = TickInterval;

            // 중첩 추가
            StackCount = Mathf.Min(StackCount + 1, MaxStack);

            Debug.Log($"[StatusEffect] {DisplayName} 적용 - 대상: {target.name}, 스택: {StackCount}");

            OnApplied?.Invoke(this);
        }


        // ====== 업데이트 ======

        /// <summary>
        /// 매 프레임 업데이트
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        /// <returns>true면 계속 활성, false면 제거 필요</returns>
        public bool Update(float deltaTime)
        {
            if (!IsActive) return false;

            // 남은 시간 감소
            RemainingTime -= deltaTime;

            // DoT 틱 처리
            if (TickInterval > 0f)
            {
                nextTickTime -= deltaTime;

                if (nextTickTime <= 0f)
                {
                    // 틱 발생
                    Tick();
                    nextTickTime = TickInterval;
                }
            }

            // 지속 시간 종료 확인
            if (RemainingTime <= 0f)
            {
                Remove();
                return false;
            }

            return true;
        }


        // ====== 틱 처리 (DoT) ======

        /// <summary>
        /// DoT 틱 발생
        /// </summary>
        private void Tick()
        {
            float tickValue = Value * StackCount;

            Debug.Log($"[StatusEffect] {DisplayName} 틱 발생 - 값: {tickValue}");

            OnTick?.Invoke(this, tickValue);
        }


        // ====== 효과 제거 ======

        /// <summary>
        /// 효과 제거
        /// </summary>
        public void Remove()
        {
            if (!IsActive) return;

            IsActive = false;
            RemainingTime = 0f;
            StackCount = 0;

            Debug.Log($"[StatusEffect] {DisplayName} 제거 - 대상: {Target?.name}");

            OnRemoved?.Invoke(this);
        }


        // ====== 중첩 관리 ======

        /// <summary>
        /// 효과 중첩 (같은 효과 재적용 시)
        /// </summary>
        public void Stack()
        {
            if (StackCount < MaxStack)
            {
                StackCount++;
                Debug.Log($"[StatusEffect] {DisplayName} 중첩 증가 - 현재 스택: {StackCount}");
            }

            // 지속 시간 갱신 (Refresh)
            RemainingTime = Duration;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 효과 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"{DisplayName} (Lv.{StackCount}) - {RemainingTime:F1}s";
        }
    }
}
