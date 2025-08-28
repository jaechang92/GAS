// 파일 위치: Assets/Scripts/GAS/AttributeSystem/AttributeModifier.cs
using System;
using UnityEngine;
using GAS.Core;

namespace GAS.AttributeSystem
{
    /// <summary>
    /// 속성을 수정하는 수정자 클래스
    /// </summary>
    [Serializable]
    public class AttributeModifier
    {
        #region Serialized Fields
        [SerializeField] private string modifierName;
        [SerializeField] private GASConstants.AttributeType targetAttribute;
        [SerializeField] private GASConstants.ModifierOperation operation;
        [SerializeField] private float value;
        [SerializeField] private GASConstants.ModifierPriority priority;
        [SerializeField] private bool isTemporary;
        [SerializeField] private float duration;
        #endregion

        #region Private Fields
        private object source;
        private float timeRemaining;
        private bool isActive = true;
        private Guid uniqueId;
        #endregion

        #region Properties
        /// <summary>
        /// 수정자 이름
        /// </summary>
        public string ModifierName
        {
            get => modifierName;
            set => modifierName = value;
        }

        /// <summary>
        /// 대상 속성 타입
        /// </summary>
        public GASConstants.AttributeType TargetAttribute
        {
            get => targetAttribute;
            set => targetAttribute = value;
        }

        /// <summary>
        /// 수정 연산 타입
        /// </summary>
        public GASConstants.ModifierOperation Operation
        {
            get => operation;
            set => operation = value;
        }

        /// <summary>
        /// 수정값
        /// </summary>
        public float Value
        {
            get => value;
            set => this.value = value;
        }

        /// <summary>
        /// 적용 우선순위
        /// </summary>
        public GASConstants.ModifierPriority Priority
        {
            get => priority;
            set => priority = value;
        }

        /// <summary>
        /// 우선순위 숫자값
        /// </summary>
        public int PriorityValue => (int)priority;

        /// <summary>
        /// 임시 수정자 여부
        /// </summary>
        public bool IsTemporary
        {
            get => isTemporary;
            set => isTemporary = value;
        }

        /// <summary>
        /// 지속 시간
        /// </summary>
        public float Duration
        {
            get => duration;
            set
            {
                duration = value;
                timeRemaining = value;
            }
        }

        /// <summary>
        /// 남은 시간
        /// </summary>
        public float TimeRemaining
        {
            get => timeRemaining;
            set => timeRemaining = value;
        }

        /// <summary>
        /// 수정자 소스 (효과, 장비 등)
        /// </summary>
        public object Source
        {
            get => source;
            set => source = value;
        }

        /// <summary>
        /// 활성 상태
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        /// <summary>
        /// 만료 여부
        /// </summary>
        public bool IsExpired => isTemporary && timeRemaining <= 0;

        /// <summary>
        /// 고유 ID
        /// </summary>
        public Guid UniqueId => uniqueId;

        /// <summary>
        /// 남은 시간 비율 (0~1)
        /// </summary>
        public float TimeRemainingRatio
        {
            get
            {
                if (!isTemporary || duration <= 0) return 1f;
                return Mathf.Clamp01(timeRemaining / duration);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// 기본 생성자
        /// </summary>
        public AttributeModifier()
        {
            modifierName = "New Modifier";
            operation = GASConstants.ModifierOperation.Add;
            priority = GASConstants.ModifierPriority.Normal;
            uniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// 영구 수정자 생성
        /// </summary>
        public AttributeModifier(string name, GASConstants.AttributeType target,
            GASConstants.ModifierOperation op, float val)
        {
            modifierName = name;
            targetAttribute = target;
            operation = op;
            value = val;
            priority = GASConstants.ModifierPriority.Normal;
            isTemporary = false;
            uniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// 임시 수정자 생성
        /// </summary>
        public AttributeModifier(string name, GASConstants.AttributeType target,
            GASConstants.ModifierOperation op, float val, float dur)
        {
            modifierName = name;
            targetAttribute = target;
            operation = op;
            value = val;
            priority = GASConstants.ModifierPriority.Normal;
            isTemporary = true;
            duration = dur;
            timeRemaining = dur;
            uniqueId = Guid.NewGuid();
        }

        /// <summary>
        /// 우선순위 지정 생성자
        /// </summary>
        public AttributeModifier(string name, GASConstants.AttributeType target,
            GASConstants.ModifierOperation op, float val, GASConstants.ModifierPriority prio)
        {
            modifierName = name;
            targetAttribute = target;
            operation = op;
            value = val;
            priority = prio;
            isTemporary = false;
            uniqueId = Guid.NewGuid();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 시간 업데이트 (임시 수정자용)
        /// </summary>
        public void UpdateTime(float deltaTime)
        {
            if (!isTemporary || !isActive) return;

            timeRemaining -= deltaTime;

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                isActive = false;
            }
        }

        /// <summary>
        /// 지속시간 갱신
        /// </summary>
        public void RefreshDuration()
        {
            if (isTemporary)
            {
                timeRemaining = duration;
                isActive = true;
            }
        }

        /// <summary>
        /// 지속시간 연장
        /// </summary>
        public void ExtendDuration(float additionalTime)
        {
            if (isTemporary)
            {
                timeRemaining += additionalTime;
                isActive = true;
            }
        }

        /// <summary>
        /// 수정자 활성화
        /// </summary>
        public void Activate()
        {
            isActive = true;
        }

        /// <summary>
        /// 수정자 비활성화
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
        }

        /// <summary>
        /// 수정자 복사
        /// </summary>
        public AttributeModifier Clone()
        {
            return new AttributeModifier
            {
                modifierName = modifierName,
                targetAttribute = targetAttribute,
                operation = operation,
                value = value,
                priority = priority,
                isTemporary = isTemporary,
                duration = duration,
                timeRemaining = timeRemaining,
                source = source,
                isActive = isActive,
                uniqueId = Guid.NewGuid() // 새로운 ID 생성
            };
        }

        /// <summary>
        /// 계산된 값 가져오기 (기본값에 적용)
        /// </summary>
        public float CalculateValue(float baseValue)
        {
            if (!isActive) return baseValue;

            switch (operation)
            {
                case GASConstants.ModifierOperation.Add:
                    return baseValue + value;

                case GASConstants.ModifierOperation.Subtract:
                    return baseValue - value;

                case GASConstants.ModifierOperation.Multiply:
                    return baseValue * value;

                case GASConstants.ModifierOperation.Divide:
                    return value != 0 ? baseValue / value : baseValue;

                case GASConstants.ModifierOperation.Override:
                    return value;

                case GASConstants.ModifierOperation.AddPercent:
                    return baseValue * (1f + value / 100f);

                case GASConstants.ModifierOperation.SubtractPercent:
                    return baseValue * (1f - value / 100f);

                default:
                    return baseValue;
            }
        }

        /// <summary>
        /// 동일한 수정자인지 확인
        /// </summary>
        public bool Equals(AttributeModifier other)
        {
            if (other == null) return false;
            return uniqueId == other.uniqueId;
        }

        /// <summary>
        /// 스택 가능한지 확인
        /// </summary>
        public bool CanStackWith(AttributeModifier other)
        {
            if (other == null) return false;

            return modifierName == other.modifierName &&
                   targetAttribute == other.targetAttribute &&
                   operation == other.operation &&
                   priority == other.priority &&
                   source == other.source;
        }

        /// <summary>
        /// 다른 수정자와 스택
        /// </summary>
        public void StackWith(AttributeModifier other)
        {
            if (!CanStackWith(other)) return;

            // 값 합산
            value += other.value;

            // 지속시간 처리
            if (isTemporary && other.isTemporary)
            {
                // 더 긴 지속시간 사용
                if (other.duration > duration)
                {
                    duration = other.duration;
                }

                // 남은 시간은 더 긴 것 사용
                if (other.timeRemaining > timeRemaining)
                {
                    timeRemaining = other.timeRemaining;
                }
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 디버그용 문자열
        /// </summary>
        public override string ToString()
        {
            string operationStr = GetOperationString();
            string timeStr = isTemporary ? $" ({timeRemaining:F1}s/{duration:F1}s)" : "";
            string activeStr = isActive ? "" : " [Inactive]";

            return $"{modifierName}: {targetAttribute} {operationStr} [{priority}]{timeStr}{activeStr}";
        }

        private string GetOperationString()
        {
            switch (operation)
            {
                case GASConstants.ModifierOperation.Add:
                    return $"+{value:F2}";
                case GASConstants.ModifierOperation.Subtract:
                    return $"-{value:F2}";
                case GASConstants.ModifierOperation.Multiply:
                    return $"x{value:F2}";
                case GASConstants.ModifierOperation.Divide:
                    return $"/{value:F2}";
                case GASConstants.ModifierOperation.Override:
                    return $"={value:F2}";
                case GASConstants.ModifierOperation.AddPercent:
                    return $"+{value:F1}%";
                case GASConstants.ModifierOperation.SubtractPercent:
                    return $"-{value:F1}%";
                default:
                    return value.ToString("F2");
            }
        }

        /// <summary>
        /// Inspector 표시용 이름
        /// </summary>
        public string GetDisplayName()
        {
            return $"{modifierName} ({GetOperationString()})";
        }
        #endregion

        #region Static Factory Methods
        /// <summary>
        /// 플랫 보너스 수정자 생성
        /// </summary>
        public static AttributeModifier CreateFlatBonus(string name, GASConstants.AttributeType target, float value)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.Add, value);
        }

        /// <summary>
        /// 퍼센트 보너스 수정자 생성
        /// </summary>
        public static AttributeModifier CreatePercentBonus(string name, GASConstants.AttributeType target, float percent)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.AddPercent, percent);
        }

        /// <summary>
        /// 플랫 감소 수정자 생성
        /// </summary>
        public static AttributeModifier CreateFlatPenalty(string name, GASConstants.AttributeType target, float value)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.Subtract, value);
        }

        /// <summary>
        /// 퍼센트 감소 수정자 생성
        /// </summary>
        public static AttributeModifier CreatePercentPenalty(string name, GASConstants.AttributeType target, float percent)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.SubtractPercent, percent);
        }

        /// <summary>
        /// 덮어쓰기 수정자 생성
        /// </summary>
        public static AttributeModifier CreateOverride(string name, GASConstants.AttributeType target, float value)
        {
            return new AttributeModifier(name, target, GASConstants.ModifierOperation.Override, value,
                GASConstants.ModifierPriority.Override);
        }
        #endregion
    }
}