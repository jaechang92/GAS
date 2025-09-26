using System;
using System.Threading;
using UnityEngine;
using GAS.Core;
using System.Linq;
using FSM.Utils;

namespace FSM.Core.Integration
{
    public interface IAbilityState : IState
    {
        string AbilityId { get; }
        IAbilitySystem AbilitySystem { get; }
    }

    public class AbilityState : State, IAbilityState
    {
        public string AbilityId { get; private set; }
        public IAbilitySystem AbilitySystem { get; private set; }

        private bool abilityExecuted;

        public AbilityState(string abilityId)
        {
            AbilityId = abilityId;
        }

        public override void Initialize(string id, GameObject owner, IStateMachine stateMachine)
        {
            base.Initialize(id, owner, stateMachine);
            AbilitySystem = owner.GetComponent<IAbilitySystem>();
        }

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            if (AbilitySystem != null && !string.IsNullOrEmpty(AbilityId))
            {
                abilityExecuted = AbilitySystem.TryUseAbility(AbilityId);
                if (!abilityExecuted)
                {
                    Debug.LogWarning($"[AbilityState] Failed to execute ability: {AbilityId}");
                }
            }
            await Awaitable.NextFrameAsync();
        }

        protected override Awaitable OnExitState(CancellationToken cancellationToken)
        {
            if (abilityExecuted && AbilitySystem != null)
            {
                AbilitySystem.CancelAbility(AbilityId);
            }
            return AwaitableHelper.CompletedTask;
        }
    }

    public class AbilityCondition : Condition
    {
        private string abilityId;
        private bool checkCanUse;
        private bool checkCooldown;

        public AbilityCondition(string id, string abilityId, bool checkCanUse = true, bool checkCooldown = false)
        {
            Id = id;
            this.abilityId = abilityId;
            this.checkCanUse = checkCanUse;
            this.checkCooldown = checkCooldown;
        }

        public override bool Evaluate(GameObject owner, IStateMachine stateMachine)
        {
            var abilitySystem = owner?.GetComponent<IAbilitySystem>();
            if (abilitySystem == null) return false;

            if (checkCanUse)
            {
                return abilitySystem.CanUseAbility(abilityId);
            }

            if (checkCooldown && abilitySystem.TryGetAbility(abilityId, out var ability))
            {
                if (ability is GAS.Core.Ability concreteAbility)
                {
                    return concreteAbility.IsOnCooldown;
                }
            }

            return abilitySystem.HasAbility(abilityId);
        }
    }

    public class ResourceCondition : Condition
    {
        public enum ComparisonType
        {
            GreaterThan,
            LessThan,
            Equal,
            GreaterThanOrEqual,
            LessThanOrEqual
        }

        private string resourceType;
        private float threshold;
        private ComparisonType comparison;
        private bool usePercentage;

        public ResourceCondition(string id, string resourceType, float threshold,
            ComparisonType comparison = ComparisonType.GreaterThanOrEqual, bool usePercentage = false)
        {
            Id = id;
            this.resourceType = resourceType;
            this.threshold = threshold;
            this.comparison = comparison;
            this.usePercentage = usePercentage;
        }

        public override bool Evaluate(GameObject owner, IStateMachine stateMachine)
        {
            var abilitySystem = owner?.GetComponent<IAbilitySystem>();
            if (abilitySystem == null || !abilitySystem.HasResource(resourceType)) return false;

            var currentValue = abilitySystem.GetResource(resourceType);

            if (usePercentage)
            {
                var maxValue = abilitySystem.GetMaxResource(resourceType);
                currentValue = maxValue > 0 ? (currentValue / maxValue) * 100f : 0f;
            }

            return comparison switch
            {
                ComparisonType.GreaterThan => currentValue > threshold,
                ComparisonType.LessThan => currentValue < threshold,
                ComparisonType.Equal => Mathf.Approximately(currentValue, threshold),
                ComparisonType.GreaterThanOrEqual => currentValue >= threshold,
                ComparisonType.LessThanOrEqual => currentValue <= threshold,
                _ => false
            };
        }
    }

    public class StateBasedAbilitySystem : MonoBehaviour
    {
        [SerializeField] private StateMachine stateMachine;
        [SerializeField] private GAS.Core.AbilitySystem abilitySystem;

        [Header("상태 기반 어빌리티 제한")]
        [SerializeField] private StateAbilityRestriction[] restrictions;

        private void Awake()
        {
            if (stateMachine == null) stateMachine = GetComponent<StateMachine>();
            if (abilitySystem == null) abilitySystem = GetComponent<AbilitySystem>();
        }

        private void Start()
        {
            if (abilitySystem != null)
            {
                abilitySystem.OnAbilityUsed += OnAbilityUsed;
            }
        }

        private void OnDestroy()
        {
            if (abilitySystem != null)
            {
                abilitySystem.OnAbilityUsed -= OnAbilityUsed;
            }
        }

        private void OnAbilityUsed(string abilityId)
        {
            foreach (var restriction in restrictions)
            {
                if (restriction.abilityId == abilityId &&
                    restriction.allowedStates.Contains(stateMachine.CurrentStateId))
                {
                    if (!string.IsNullOrEmpty(restriction.transitionToState))
                    {
                        stateMachine.TryTransitionTo(restriction.transitionToState);
                    }
                    break;
                }
            }
        }

        public bool CanUseAbilityInCurrentState(string abilityId)
        {
            if (stateMachine == null) return true;

            foreach (var restriction in restrictions)
            {
                if (restriction.abilityId == abilityId)
                {
                    return restriction.allowedStates.Contains(stateMachine.CurrentStateId);
                }
            }

            return true;
        }

        [System.Serializable]
        public class StateAbilityRestriction
        {
            public string abilityId;
            public string[] allowedStates;
            public string transitionToState;
        }
    }

    public static class FSMAbilityExtensions
    {
        public static void AddAbilityState(this IStateMachine stateMachine, string stateId, string abilityId)
        {
            var abilityState = new AbilityState(abilityId);
            stateMachine.AddState(abilityState);
        }

        public static void AddAbilityTransition(this IStateMachine stateMachine,
            string fromStateId, string toStateId, string requiredAbilityId,
            int priority = 0, bool checkCanUse = true)
        {
            var transition = new Transition($"{fromStateId}_to_{toStateId}_ability", fromStateId, toStateId, priority);
            var condition = new AbilityCondition($"ability_{requiredAbilityId}", requiredAbilityId, checkCanUse);

            transition.AddCondition(condition);
            stateMachine.AddTransition(transition);
        }

        public static void AddResourceTransition(this IStateMachine stateMachine,
            string fromStateId, string toStateId, string resourceType, float threshold,
            ResourceCondition.ComparisonType comparison = ResourceCondition.ComparisonType.LessThanOrEqual,
            int priority = 0)
        {
            var transition = new Transition($"{fromStateId}_to_{toStateId}_resource", fromStateId, toStateId, priority);
            var condition = new ResourceCondition($"resource_{resourceType}", resourceType, threshold, comparison);

            transition.AddCondition(condition);
            stateMachine.AddTransition(transition);
        }
    }
}
