using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using FSM.Core.Utils;

namespace FSM.Core
{
    public class StateMachine : MonoBehaviour, IStateMachine
    {
        [Header("디버그 설정")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private string initialStateId;

        [Header("Inspector 헬퍼 (읽기 전용)")]
        [SerializeField] private StateMachineInspectorHelper inspectorHelper = new StateMachineInspectorHelper();

        private Dictionary<string, IState> states = new Dictionary<string, IState>();
        private List<ITransition> transitions = new List<ITransition>();
        private Dictionary<string, bool> eventTriggers = new Dictionary<string, bool>();

        private IState currentState;
        private CancellationTokenSource cancellationTokenSource;

        public string CurrentStateId => currentState?.Id ?? string.Empty;
        public IState CurrentState => currentState;
        public bool IsRunning { get; private set; }

        public IReadOnlyDictionary<string, IState> States => states;
        public IReadOnlyList<ITransition> Transitions => transitions;

        public event Action<string, string> OnStateChanged;
        public event Action<ITransition> OnTransitionStarted;
        public event Action<ITransition> OnTransitionCompleted;
        public event Action OnStarted;
        public event Action OnStopped;

        private void Awake()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        private void Update()
        {
            if (IsRunning)
            {
                ((IStateMachine)this).Update();
            }
        }

        void IStateMachine.Update()
        {
            if (currentState != null)
            {
                currentState.OnUpdate(Time.deltaTime);
                CheckTransitions();
            }

            // Inspector 헬퍼 업데이트 (에디터에서만)
            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                inspectorHelper.UpdateFromStateMachine(this);
            }
            #endif
        }

        private void CheckTransitions()
        {
            var availableTransitions = transitions
                .Where(t => t.IsEnabled && t.FromStateId == CurrentStateId)
                .OrderByDescending(t => t.Priority);

            foreach (var transition in availableTransitions)
            {
                if (transition.CanTransition())
                {
                    _ = TransitionToAsync(transition);
                    break;
                }
            }
        }

        public void AddState(IState state)
        {
            if (state == null || string.IsNullOrEmpty(state.Id)) return;

            if (states.ContainsKey(state.Id))
            {
                RemoveState(state.Id);
            }

            state.Initialize(state.Id, gameObject, this);
            states[state.Id] = state;

            if (enableDebugLog)
                Debug.Log($"[FSM] Added state: {state.Id}");
        }

        public void RemoveState(string stateId)
        {
            if (states.TryGetValue(stateId, out var state))
            {
                // ���� ���¶�� ���� ó��
                if (CurrentStateId == stateId && IsRunning)
                {
                    Stop();
                }

                // ���� ��ȯ ����
                var relatedTransitions = transitions
                    .Where(t => t.FromStateId == stateId || t.ToStateId == stateId)
                    .ToList();

                foreach (var transition in relatedTransitions)
                {
                    RemoveTransition(transition);
                }

                states.Remove(stateId);

                if (enableDebugLog)
                    Debug.Log($"[FSM] Removed state: {stateId}");
            }
        }

        public bool HasState(string stateId)
        {
            return states.ContainsKey(stateId);
        }

        public bool TryGetState(string stateId, out IState state)
        {
            return states.TryGetValue(stateId, out state);
        }

        public void AddTransition(ITransition transition)
        {
            if (transition == null) return;

            if (!HasState(transition.FromStateId) || !HasState(transition.ToStateId))
            {
                Debug.LogWarning($"[FSM] Cannot add transition: States {transition.FromStateId} or {transition.ToStateId} do not exist");
                return;
            }

            transitions.Add(transition);

            if (enableDebugLog)
                Debug.Log($"[FSM] Added transition: {transition.FromStateId} -> {transition.ToStateId}");
        }

        /// <summary>
        /// 이벤트 기반 전환 추가 (편의 메서드)
        /// </summary>
        public void AddTransition(string fromStateId, string toStateId, string eventId, int priority = 0)
        {
            var transition = new EventBasedTransition($"{fromStateId}_{toStateId}_{eventId}", fromStateId, toStateId, eventId, this, priority);
            AddTransition(transition);
        }

        /// <summary>
        /// 이벤트 트리거
        /// </summary>
        public void TriggerEvent(string eventId)
        {
            eventTriggers[eventId] = true;
        }

        /// <summary>
        /// 이벤트 상태 확인
        /// </summary>
        public bool IsEventTriggered(string eventId)
        {
            return eventTriggers.TryGetValue(eventId, out bool triggered) && triggered;
        }

        /// <summary>
        /// 이벤트 소비 (한 번 확인 후 리셋)
        /// </summary>
        public bool ConsumeEvent(string eventId)
        {
            if (eventTriggers.TryGetValue(eventId, out bool triggered) && triggered)
            {
                eventTriggers[eventId] = false;
                return true;
            }
            return false;
        }

        public void RemoveTransition(ITransition transition)
        {
            if (transitions.Remove(transition))
            {
                if (enableDebugLog)
                    Debug.Log($"[FSM] Removed transition: {transition.FromStateId} -> {transition.ToStateId}");
            }
        }

        public bool CanTransitionTo(string stateId)
        {
            if (!HasState(stateId) || !IsRunning) return false;

            return transitions.Any(t =>
                t.IsEnabled &&
                t.FromStateId == CurrentStateId &&
                t.ToStateId == stateId &&
                t.CanTransition());
        }

        public bool TryTransitionTo(string stateId)
        {
            if (!CanTransitionTo(stateId)) return false;

            var transition = transitions.First(t =>
                t.FromStateId == CurrentStateId &&
                t.ToStateId == stateId);

            _ = TransitionToAsync(transition);
            return true;
        }

        public void ForceTransitionTo(string stateId)
        {
            if (!HasState(stateId))
            {
                Debug.LogWarning($"[FSM] Cannot force transition to {stateId}: State does not exist");
                return;
            }

            _ = ChangeStateAsync(stateId);
        }

        public void StartStateMachine(string initialStateId = null)
        {
            if (IsRunning) return;

            var targetStateId = initialStateId ?? this.initialStateId;
            if (string.IsNullOrEmpty(targetStateId))
            {
                targetStateId = states.Keys.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(targetStateId))
            {
                Debug.LogWarning("[FSM] Cannot start: No states available");
                return;
            }

            IsRunning = true;
            _ = ChangeStateAsync(targetStateId);
            OnStarted?.Invoke();

            if (enableDebugLog)
                Debug.Log($"[FSM] Started with initial state: {targetStateId}");
        }

        public void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            _ = ExitCurrentStateAsync();
            OnStopped?.Invoke();

            if (enableDebugLog)
                Debug.Log("[FSM] Stopped");
        }

        private async Awaitable TransitionToAsync(ITransition transition)
        {
            OnTransitionStarted?.Invoke(transition);

            var fromStateId = CurrentStateId;
            await ChangeStateAsync(transition.ToStateId);

            OnTransitionCompleted?.Invoke(transition);
            //transition.OnTransitionTriggered?.Invoke(transition);

            if (enableDebugLog)
                Debug.Log($"[FSM] Transition completed: {fromStateId} -> {transition.ToStateId}");
        }

        private async Task ChangeStateAsync(string newStateId)
        {
            if (!states.TryGetValue(newStateId, out var newState))
            {
                Debug.LogWarning($"[FSM] State {newStateId} not found");
                return;
            }

            var previousStateId = CurrentStateId;

            // ���� ���� ����
            await ExitCurrentStateAsync();

            // �� ���� ����
            currentState = newState;
            try
            {
                await currentState.OnEnter(cancellationTokenSource.Token);
                OnStateChanged?.Invoke(previousStateId, newStateId);

                if (enableDebugLog)
                    Debug.Log($"[FSM] State changed: {previousStateId} -> {newStateId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[FSM] Error entering state {newStateId}: {e.Message}");
                currentState = null;
            }
        }

        private async Task ExitCurrentStateAsync()
        {
            if (currentState != null)
            {
                try
                {
                    await currentState.OnExit(cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FSM] Error exiting state {currentState.Id}: {e.Message}");
                }
                finally
                {
                    currentState = null;
                }
            }
        }

        private void OnDestroy()
        {
            Stop();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}