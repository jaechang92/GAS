using System.Collections.Generic;
using UnityEngine;
using FSM.Core;

namespace GameFlow
{
    /// <summary>
    /// 게임 상태 전환 조건 기본 클래스
    /// </summary>
    public abstract class GameTransition : ITransition
    {
        public string Id { get; protected set; }
        public string FromStateId { get; protected set; }
        public string ToStateId { get; protected set; }
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; protected set; } = 0;

        protected GameFlowManager gameFlowManager;
        private List<ICondition> conditions = new List<ICondition>();

        public IReadOnlyList<ICondition> Conditions => conditions;

        // 이벤트
        public event System.Action<ITransition> OnTransitionTriggered;

        protected GameTransition(GameStateType from, GameStateType to, GameFlowManager manager, int priority = 0)
        {
            Id = $"{from}_{to}_{GetType().Name}";
            FromStateId = from.ToString();
            ToStateId = to.ToString();
            gameFlowManager = manager;
            Priority = priority;
        }

        public virtual void AddCondition(ICondition condition)
        {
            if (condition != null && !conditions.Contains(condition))
            {
                conditions.Add(condition);
            }
        }

        public virtual void RemoveCondition(ICondition condition)
        {
            conditions.Remove(condition);
        }

        public virtual bool CanTransition()
        {
            // 기본 조건들 확인
            foreach (var condition in conditions)
            {
                if (condition.IsEnabled && !condition.Evaluate(gameFlowManager?.gameObject, gameFlowManager?.GetComponent<StateMachine>()))
                {
                    return false;
                }
            }

            // 자식 클래스의 조건 확인
            return CheckTransitionCondition();
        }

        // 자식 클래스에서 구현할 추상 메서드
        protected abstract bool CheckTransitionCondition();

        // 전환 트리거 호출
        protected virtual void TriggerTransition()
        {
            OnTransitionTriggered?.Invoke(this);
        }
    }

    /// <summary>
    /// 이벤트 기반 전환
    /// </summary>
    public class EventTransition : GameTransition
    {
        private GameEventType triggerEvent;
        private bool eventTriggered = false;

        public EventTransition(GameStateType from, GameStateType to, GameEventType eventType,
                             GameFlowManager manager, int priority = 0)
            : base(from, to, manager, priority)
        {
            triggerEvent = eventType;

            // 이벤트 구독
            if (gameFlowManager != null)
            {
                gameFlowManager.OnEventTriggered += OnEventTriggered;
            }
        }

        private void OnEventTriggered(GameEventType eventType)
        {
            if (eventType == triggerEvent)
            {
                eventTriggered = true;
            }
        }

        protected override bool CheckTransitionCondition()
        {
            if (eventTriggered)
            {
                eventTriggered = false; // 한 번만 트리거되도록
                TriggerTransition();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 시간 기반 전환
    /// </summary>
    public class TimeTransition : GameTransition
    {
        private float duration;
        private float elapsedTime = 0f;
        private bool isStarted = false;

        public TimeTransition(GameStateType from, GameStateType to, float durationSeconds,
                            GameFlowManager manager, int priority = 0)
            : base(from, to, manager, priority)
        {
            duration = durationSeconds;
        }

        protected override bool CheckTransitionCondition()
        {
            if (!isStarted)
            {
                isStarted = true;
                elapsedTime = 0f;
            }

            elapsedTime += Time.deltaTime;
            if (elapsedTime >= duration)
            {
                TriggerTransition();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            isStarted = false;
            elapsedTime = 0f;
        }
    }

    /// <summary>
    /// 키 입력 기반 전환
    /// </summary>
    public class KeyInputTransition : GameTransition
    {
        private KeyCode triggerKey;

        public KeyInputTransition(GameStateType from, GameStateType to, KeyCode key,
                                GameFlowManager manager, int priority = 0)
            : base(from, to, manager, priority)
        {
            triggerKey = key;
        }

        protected override bool CheckTransitionCondition()
        {
            if (Input.GetKeyDown(triggerKey))
            {
                TriggerTransition();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 조건 기반 전환
    /// </summary>
    public class ConditionalTransition : GameTransition
    {
        private System.Func<bool> condition;

        public ConditionalTransition(GameStateType from, GameStateType to, System.Func<bool> conditionFunc,
                                   GameFlowManager manager, int priority = 0)
            : base(from, to, manager, priority)
        {
            condition = conditionFunc;
        }

        protected override bool CheckTransitionCondition()
        {
            if (condition?.Invoke() ?? false)
            {
                TriggerTransition();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 로딩 완료 전환 (특화된 전환)
    /// </summary>
    public class LoadingCompleteTransition : GameTransition
    {
        private bool loadingComplete = false;

        public LoadingCompleteTransition(GameFlowManager manager)
            : base(GameStateType.Loading, GameStateType.Ingame, manager, 10) // 높은 우선순위
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.OnEventTriggered += OnEventTriggered;
            }
        }

        private void OnEventTriggered(GameEventType eventType)
        {
            if (eventType == GameEventType.LoadComplete)
            {
                loadingComplete = true;
            }
        }

        protected override bool CheckTransitionCondition()
        {
            if (loadingComplete)
            {
                loadingComplete = false;
                TriggerTransition();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 게임 플로우 전환 설정 도우미 클래스
    /// </summary>
    public static class GameFlowTransitionSetup
    {
        /// <summary>
        /// 기본 게임 플로우 전환들을 설정
        /// </summary>
        public static void SetupDefaultTransitions(StateMachine stateMachine, GameFlowManager gameFlowManager)
        {
            // Main -> Loading (게임 시작)
            stateMachine.AddTransition(new EventTransition(
                GameStateType.Main, GameStateType.Loading,
                GameEventType.StartGame, gameFlowManager, 5));

            // Loading -> Ingame (로딩 완료)
            stateMachine.AddTransition(new LoadingCompleteTransition(gameFlowManager));

            // Ingame -> Pause (ESC 키 또는 일시정지 이벤트)
            stateMachine.AddTransition(new EventTransition(
                GameStateType.Ingame, GameStateType.Pause,
                GameEventType.PauseGame, gameFlowManager, 5));

            // Pause -> Ingame (게임 재개)
            stateMachine.AddTransition(new EventTransition(
                GameStateType.Pause, GameStateType.Ingame,
                GameEventType.ResumeGame, gameFlowManager, 5));

            // Ingame -> Menu (메뉴 열기)
            stateMachine.AddTransition(new EventTransition(
                GameStateType.Ingame, GameStateType.Menu,
                GameEventType.OpenMenu, gameFlowManager, 3));

            // Menu -> Ingame (메뉴 닫기)
            stateMachine.AddTransition(new EventTransition(
                GameStateType.Menu, GameStateType.Ingame,
                GameEventType.CloseMenu, gameFlowManager, 3));

            // Any State -> Lobby (로비로 이동)
            foreach (GameStateType state in System.Enum.GetValues(typeof(GameStateType)))
            {
                if (state != GameStateType.Lobby)
                {
                    stateMachine.AddTransition(new EventTransition(
                        state, GameStateType.Lobby,
                        GameEventType.GoToLobby, gameFlowManager, 8));
                }
            }

            // Any State -> Main (메인으로 이동)
            foreach (GameStateType state in System.Enum.GetValues(typeof(GameStateType)))
            {
                if (state != GameStateType.Main)
                {
                    stateMachine.AddTransition(new EventTransition(
                        state, GameStateType.Main,
                        GameEventType.GoToMain, gameFlowManager, 9));
                }
            }
        }
    }
}