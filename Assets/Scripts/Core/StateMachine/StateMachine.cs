using System;
using System.Collections.Generic;
using Debugging;
using UnityEngine.Assertions;

namespace Core.StateMachine
{
    /// <summary>
    /// Generic State Machine 클래스
    /// Enum을 사용하여 상태를 관리함.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StateMachine<T> where T : Enum
    {
        private readonly Dictionary<T, IState<T>> _stateTypeToStateMap;

        private T _currentStateType;
        private IState<T> _currentState;

        /// <summary>
        /// [NewStateType, OldStateType]
        /// </summary>
        public event Action<T, T> OnStateChanged;

        public StateMachine()
        {
            _stateTypeToStateMap = new();
        }

        public void Start(T stateType)
        {
            Assert.IsTrue(_stateTypeToStateMap.ContainsKey(stateType), 
                $"존재하지 않는 State({stateType})로는 시작할 수 없습니다.");

            if (_currentState != null)
            {
                Logger.Warning($"이미 StateMachine이 시작되었습니다. State({stateType})로 변경합니다.");
                ChangeState(stateType);
                return;
            }
            
            ChangeState(stateType);
        }

        /// <summary>
        /// 상태를 추가합니다.
        /// </summary>
        /// <param name="state"></param>
        public void AddState(IState<T> state)
        {
            Assert.IsNotNull(state, "추가하려고 하는 state가 Null입니다.");
            
            var stateType = state.StateTypeType;
            if (_stateTypeToStateMap.ContainsKey(stateType))
            {
                Logger.Warning($"이미 추가하려는 State({stateType})가 존재합니다.");
            }
            
            // 새로운거면 추가 or 기존에 있던거면 교체
            _stateTypeToStateMap[stateType] = state;
            Logger.StateMachine($"State({stateType})가 추가되었습니다.");
        }

        /// <summary>
        /// 여러 상태를 한번에 추가
        /// </summary>
        /// <param name="states"></param>
        public void AddStates(params IState<T>[] states)
        {
            foreach (var state in states)
            {
                AddState(state);
            }
        }

        /// <summary>
        /// 상태 변경
        /// </summary>
        /// <param name="newStateType"></param>
        public void ChangeState(T newStateType)
        {
            // 같은 상태로 변경은 무시
            if (_currentState != null 
                && _currentState.StateTypeType.Equals(newStateType))
            {
                Logger.StateMachine($"이미 {newStateType}상태입니다. 변경 요청을 무시합니다.");
                return;
            }
            
            Assert.IsTrue(_stateTypeToStateMap.ContainsKey(newStateType), 
                $"존재하지 않는 State({newStateType})로는 변경할 수 없습니다.");

            var previousStateType = default(T);
            if (_currentState != null)
            {
                previousStateType = _currentState.StateTypeType;
            }
            
            _currentState?.OnExit();
            
            _currentState = _stateTypeToStateMap[newStateType];
            _currentStateType = newStateType;
            
            Logger.StateMachine($"State 변경: {previousStateType} -> {_currentStateType}");
            
            _currentState?.OnEnter();
            OnStateChanged?.Invoke(previousStateType, _currentStateType);
        }

        public void Update()
        {
            _currentState?.OnUpdate();
        }

        public void FixedUpdate()
        {
            _currentState?.OnFixedUpdate();
        }
    }
}