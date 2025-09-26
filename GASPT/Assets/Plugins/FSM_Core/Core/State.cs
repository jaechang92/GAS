using FSM.Utils;
using System;
using System.Threading;
using UnityEngine;

namespace FSM.Core
{
    public abstract class State : IState
    {
        protected GameObject owner;
        protected IStateMachine stateMachine;

        public string Id { get; private set; }
        public virtual string Name => Id;
        public bool IsActive { get; private set; }
        public GameObject Owner => owner;

        public event Action<IState> OnEntered;
        public event Action<IState> OnExited;

        public virtual void Initialize(string id, GameObject owner, IStateMachine stateMachine)
        {
            this.Id = id;
            this.owner = owner;
            this.stateMachine = stateMachine;
        }

        public virtual async Awaitable OnEnter(CancellationToken cancellationToken = default)
        {
            IsActive = true;
            await OnEnterState(cancellationToken);
            OnEntered?.Invoke(this);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            if (IsActive)
            {
                OnUpdateState(deltaTime);
            }
        }

        public virtual async Awaitable OnExit(CancellationToken cancellationToken = default)
        {
            IsActive = false;
            await OnExitState(cancellationToken);
            OnExited?.Invoke(this);
        }

        protected virtual Awaitable OnEnterState(CancellationToken cancellationToken) => AwaitableHelper.CompletedTask;
        protected virtual void OnUpdateState(float deltaTime) { }
        protected virtual Awaitable OnExitState(CancellationToken cancellationToken) => AwaitableHelper.CompletedTask;
    }

    public class SimpleState : State
    {
        private Action onEnterAction;
        private Action<float> onUpdateAction;
        private Action onExitAction;

        public SimpleState(Action onEnter = null, Action<float> onUpdate = null, Action onExit = null)
        {
            onEnterAction = onEnter;
            onUpdateAction = onUpdate;
            onExitAction = onExit;
        }

        protected override Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            onEnterAction?.Invoke();
            return AwaitableHelper.CompletedTask;
        }

        protected override void OnUpdateState(float deltaTime)
        {
            onUpdateAction?.Invoke(deltaTime);
        }

        protected override Awaitable OnExitState(CancellationToken cancellationToken)
        {
            onExitAction?.Invoke();
            return AwaitableHelper.CompletedTask;
        }
    }
}