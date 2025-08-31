using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.TagSystem;

namespace GAS.EffectSystem
{
    /// <summary>
    /// GameplayEffect를 수신하고 처리할 수 있는 객체를 정의하는 인터페이스
    /// </summary>
    public interface IEffectReceiver
    {
        /// <summary>
        /// 활성화된 Effect 인스턴스 목록
        /// </summary>
        IReadOnlyList<EffectInstance> ActiveEffects { get; }

        /// <summary>
        /// Effect 적용 가능 여부 확인
        /// </summary>
        /// <param name="effect">확인할 Effect</param>
        /// <param name="context">Effect 컨텍스트</param>
        /// <returns>적용 가능 여부</returns>
        bool CanReceiveEffect(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Effect 적용
        /// </summary>
        /// <param name="effect">적용할 Effect</param>
        /// <param name="context">Effect 컨텍스트</param>
        /// <returns>생성된 Effect 인스턴스 (실패시 null)</returns>
        EffectInstance ApplyEffect(GameplayEffect effect, EffectContext context);

        /// <summary>
        /// Effect 제거
        /// </summary>
        /// <param name="instance">제거할 Effect 인스턴스</param>
        /// <returns>제거 성공 여부</returns>
        bool RemoveEffect(EffectInstance instance);

        /// <summary>
        /// Effect ID로 제거
        /// </summary>
        /// <param name="effectId">제거할 Effect ID</param>
        /// <returns>제거된 Effect 개수</returns>
        int RemoveEffectById(string effectId);

        /// <summary>
        /// 특정 Source의 모든 Effect 제거
        /// </summary>
        /// <param name="source">Effect source</param>
        /// <returns>제거된 Effect 개수</returns>
        int RemoveEffectsFromSource(object source);

        /// <summary>
        /// 모든 Effect 제거
        /// </summary>
        void RemoveAllEffects();

        /// <summary>
        /// 특정 ID의 활성 Effect 인스턴스 가져오기
        /// </summary>
        /// <param name="effectId">찾을 Effect ID</param>
        /// <returns>Effect 인스턴스 목록</returns>
        List<EffectInstance> GetActiveEffectsByID(string effectId);

        /// <summary>
        /// 특정 태그를 가진 Effect 인스턴스 가져오기
        /// </summary>
        /// <param name="tag">찾을 태그</param>
        /// <returns>Effect 인스턴스 목록</returns>
        List<EffectInstance> GetActiveEffectsByTag(GameplayTag tag);

        /// <summary>
        /// Effect 스택 수 가져오기
        /// </summary>
        /// <param name="effectId">Effect ID</param>
        /// <returns>스택 수</returns>
        int GetEffectStackCount(string effectId);

        /// <summary>
        /// Effect 적용 시 발생하는 이벤트
        /// </summary>
        event Action<EffectInstance> OnEffectApplied;

        /// <summary>
        /// Effect 제거 시 발생하는 이벤트
        /// </summary>
        event Action<EffectInstance> OnEffectRemoved;

        /// <summary>
        /// Effect 스택 변경 시 발생하는 이벤트
        /// </summary>
        event Action<EffectInstance, int> OnEffectStackChanged;

        /// <summary>
        /// Effect 만료 시 발생하는 이벤트
        /// </summary>
        event Action<EffectInstance> OnEffectExpired;
    }

    /// <summary>
    /// 활성화된 GameplayEffect의 런타임 인스턴스
    /// </summary>
    public class EffectInstance
    {
        #region Fields

        private readonly string instanceId;
        private readonly GameplayEffect sourceEffect;
        private readonly EffectContext context;
        private readonly float startTime;
        private float remainingDuration;
        private float nextPeriodicTime;
        private int currentStack;
        private int periodicExecutionCount;
        private bool isExpired;
        private List<Guid> appliedModifierIds;
        private GameObject visualEffect;

        #endregion

        #region Properties

        /// <summary>
        /// 인스턴스 고유 ID
        /// </summary>
        public string InstanceId => instanceId;

        /// <summary>
        /// 원본 Effect
        /// </summary>
        public GameplayEffect SourceEffect => sourceEffect;

        /// <summary>
        /// Effect 컨텍스트
        /// </summary>
        public EffectContext Context => context;

        /// <summary>
        /// 시작 시간
        /// </summary>
        public float StartTime => startTime;

        /// <summary>
        /// 남은 지속 시간
        /// </summary>
        public float RemainingDuration
        {
            get => remainingDuration;
            set => remainingDuration = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 다음 주기 실행 시간
        /// </summary>
        public float NextPeriodicTime
        {
            get => nextPeriodicTime;
            set => nextPeriodicTime = value;
        }

        /// <summary>
        /// 현재 스택 수
        /// </summary>
        public int CurrentStack
        {
            get => currentStack;
            set => currentStack = Mathf.Max(1, value);
        }

        /// <summary>
        /// 주기적 실행 횟수
        /// </summary>
        public int PeriodicExecutionCount => periodicExecutionCount;

        /// <summary>
        /// 만료 여부
        /// </summary>
        public bool IsExpired
        {
            get => isExpired;
            set => isExpired = value;
        }

        /// <summary>
        /// 무한 지속 여부
        /// </summary>
        public bool IsInfinite => sourceEffect.DurationPolicy == EffectDurationPolicy.Infinite;

        /// <summary>
        /// 주기적 Effect 여부
        /// </summary>
        public bool IsPeriodic => sourceEffect.Type == EffectType.Periodic;

        /// <summary>
        /// 적용된 Modifier ID 목록
        /// </summary>
        public List<Guid> AppliedModifierIds => appliedModifierIds;

        /// <summary>
        /// 시각 효과 GameObject
        /// </summary>
        public GameObject VisualEffect
        {
            get => visualEffect;
            set => visualEffect = value;
        }

        /// <summary>
        /// 경과 시간
        /// </summary>
        public float ElapsedTime => Time.time - startTime;

        /// <summary>
        /// 진행률 (0~1)
        /// </summary>
        public float Progress
        {
            get
            {
                if (IsInfinite) return 0f;
                if (sourceEffect.Duration <= 0f) return 1f;
                return 1f - (remainingDuration / sourceEffect.Duration);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// EffectInstance 생성자
        /// </summary>
        public EffectInstance(GameplayEffect effect, EffectContext context)
        {
            instanceId = Guid.NewGuid().ToString();
            sourceEffect = effect;
            this.context = context.Clone();
            startTime = Time.time;

            // Duration 설정
            if (effect.DurationPolicy == EffectDurationPolicy.HasDuration)
            {
                remainingDuration = effect.Duration;
            }
            else if (effect.DurationPolicy == EffectDurationPolicy.Infinite)
            {
                remainingDuration = float.MaxValue;
            }
            else
            {
                remainingDuration = 0f;
            }

            // Periodic 설정
            if (effect.Type == EffectType.Periodic)
            {
                nextPeriodicTime = startTime + effect.Period;
            }

            currentStack = 1;
            periodicExecutionCount = 0;
            isExpired = false;
            appliedModifierIds = new List<Guid>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 주기 실행 카운트 증가
        /// </summary>
        public void IncrementPeriodicCount()
        {
            periodicExecutionCount++;
        }

        /// <summary>
        /// 스택 추가
        /// </summary>
        public void AddStack(int amount = 1)
        {
            currentStack += amount;
            currentStack = Mathf.Min(currentStack, sourceEffect.MaxStackCount);
        }

        /// <summary>
        /// Duration 갱신
        /// </summary>
        public void RefreshDuration()
        {
            if (sourceEffect.DurationPolicy == EffectDurationPolicy.HasDuration)
            {
                remainingDuration = sourceEffect.Duration;
            }
        }

        /// <summary>
        /// Periodic 타이머 리셋
        /// </summary>
        public void ResetPeriodicTimer()
        {
            if (sourceEffect.Type == EffectType.Periodic)
            {
                nextPeriodicTime = Time.time + sourceEffect.Period;
            }
        }

        /// <summary>
        /// Modifier ID 추가
        /// </summary>
        public void AddModifierId(Guid modifierId)
        {
            if (!appliedModifierIds.Contains(modifierId))
            {
                appliedModifierIds.Add(modifierId);
            }
        }

        /// <summary>
        /// Modifier ID 제거
        /// </summary>
        public bool RemoveModifierId(Guid modifierId)
        {
            return appliedModifierIds.Remove(modifierId);
        }

        /// <summary>
        /// 모든 Modifier ID 제거
        /// </summary>
        public void ClearModifierIds()
        {
            appliedModifierIds.Clear();
        }

        /// <summary>
        /// 인스턴스 정보 문자열
        /// </summary>
        public override string ToString()
        {
            return $"EffectInstance [{sourceEffect.EffectName}] " +
                   $"Stack: {currentStack}, " +
                   $"Duration: {remainingDuration:F1}s, " +
                   $"Expired: {isExpired}";
        }

        #endregion
    }
}

// 파일 위치: Assets/Scripts/GAS/EffectSystem/Base/IEffectReceiver.cs
// 기존 IEffectComponent.cs 파일을 이 내용으로 교체