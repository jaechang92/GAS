using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace FSM.Core
{
    public interface IState
    {
        string Id { get; }
        string Name { get; }
        bool IsActive { get; }
        GameObject Owner { get; }

        Awaitable OnEnter(CancellationToken cancellationToken = default);
        void OnUpdate(float deltaTime);
        Awaitable OnExit(CancellationToken cancellationToken = default);

        void Initialize(string id, GameObject owner, IStateMachine stateMachine);

        event Action<IState> OnEntered;
        event Action<IState> OnExited;
    }
}