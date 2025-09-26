using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSM.Core.Utils
{
    /// <summary>
    /// Dictionary를 Unity Inspector에서 확인할 수 있게 해주는 헬퍼 클래스
    /// </summary>
    [Serializable]
    public class DictionaryInspectorHelper<TKey, TValue>
    {
        [SerializeField] private List<KeyValuePair<TKey, TValue>> items = new List<KeyValuePair<TKey, TValue>>();

        [Serializable]
        public struct KeyValuePair<K, V>
        {
            public K key;
            public V value;

            public KeyValuePair(K k, V v)
            {
                key = k;
                value = v;
            }
        }

        /// <summary>
        /// Dictionary의 내용을 Inspector용 리스트로 업데이트
        /// </summary>
        public void UpdateFromDictionary(Dictionary<TKey, TValue> dictionary)
        {
            items.Clear();
            foreach (var kvp in dictionary)
            {
                items.Add(new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
            }
        }

        /// <summary>
        /// Inspector에서 보여줄 아이템 리스트
        /// </summary>
        public List<KeyValuePair<TKey, TValue>> Items => items;

        /// <summary>
        /// 딕셔너리 크기
        /// </summary>
        public int Count => items.Count;
    }

    /// <summary>
    /// StateMachine용 특화된 Inspector 헬퍼
    /// </summary>
    [Serializable]
    public class StateMachineInspectorHelper
    {
        [Header("상태 목록")]
        [SerializeField] private List<StateInfo> states = new List<StateInfo>();

        [Header("전환 목록")]
        [SerializeField] private List<TransitionInfo> transitions = new List<TransitionInfo>();

        [Header("현재 상태")]
        [SerializeField] private string currentStateId;
        [SerializeField] private bool isRunning;

        [Serializable]
        public struct StateInfo
        {
            public string id;
            public string typeName;
            public bool isActive;

            public StateInfo(string stateId, string type, bool active)
            {
                id = stateId;
                typeName = type;
                isActive = active;
            }
        }

        [Serializable]
        public struct TransitionInfo
        {
            public string fromState;
            public string toState;
            public bool isEnabled;
            public int priority;

            public TransitionInfo(string from, string to, bool enabled, int prio)
            {
                fromState = from;
                toState = to;
                isEnabled = enabled;
                priority = prio;
            }
        }

        /// <summary>
        /// StateMachine의 상태를 Inspector용으로 업데이트
        /// </summary>
        public void UpdateFromStateMachine(IStateMachine stateMachine)
        {
            // 상태 정보 업데이트
            states.Clear();
            foreach (var state in stateMachine.States)
            {
                bool isActive = stateMachine.CurrentStateId == state.Key;
                states.Add(new StateInfo(state.Key, state.Value.GetType().Name, isActive));
            }

            // 전환 정보 업데이트
            transitions.Clear();
            foreach (var transition in stateMachine.Transitions)
            {
                transitions.Add(new TransitionInfo(
                    transition.FromStateId,
                    transition.ToStateId,
                    transition.IsEnabled,
                    transition.Priority
                ));
            }

            // 현재 상태 정보 업데이트
            currentStateId = stateMachine.CurrentStateId;
            isRunning = stateMachine.IsRunning;
        }

        /// <summary>
        /// 상태 개수
        /// </summary>
        public int StateCount => states.Count;

        /// <summary>
        /// 전환 개수
        /// </summary>
        public int TransitionCount => transitions.Count;

        /// <summary>
        /// 현재 실행 중인지 여부
        /// </summary>
        public bool IsRunning => isRunning;

        /// <summary>
        /// 현재 상태 ID
        /// </summary>
        public string CurrentStateId => currentStateId;
    }
}