using System;
using System.Collections.Generic;
using MarioGame.Core.Entities;
using MarioGame.Core.Interfaces;
using UnityEngine.Assertions;

namespace MarioGame.Core.StateMachine
{
    /// <summary>
    /// Generic State Machine 클래스
    /// Enum 타입을 사용하여 상태를 관리
    /// </summary>
    /// <typeparam name="T">State Type Enum</typeparam>
    public class StateMachine<T> where T : Enum
    {
        private readonly Dictionary<T, IState<T>> _states;
        private readonly IDebugLogger _logger;
        
        private IState<T> _currentState;
        private T _currentStateType;
        private bool _enableStateMachineLogs = false;

        public IState<T> CurrentState => _currentState;
        public T CurrentStateType => _currentStateType;
        
        public event Action<T, T> OnStateChanged; 

        /// <summary>
        /// StateMachine 생성자
        /// </summary>
        /// <param name="logger">StateMachine을 소유한 Entity</param>
        public StateMachine(IDebugLogger logger)
        {
            _states = new Dictionary<T, IState<T>>();
            _logger = logger;
            Assert.IsNotNull(_logger, "StateMachine owner cannot be null.");

            if (logger != null)
            {
                _enableStateMachineLogs = _logger.EnableDebugLogs;
            }
        }
        
        /// <summary>
        /// 상태 추가
        /// </summary>
        /// <param name="state">추가할 상태</param>
        public void AddState(IState<T> state)
        {
            Assert.IsNotNull(state, "Cannot add null state to StateMachine");
            var stateType = state.StateType;

            if (_states.ContainsKey(stateType))
            {
                StateMachineLogWarning("State already exists, replacing:", stateType);
            }
            _states[stateType] = state;
            StateMachineLog("State added:", stateType);
        }

        /// <summary>
        /// 여러 상태를 한번에 추가
        /// </summary>
        /// <param name="states">추가할 상태들</param>
        public void AddStates(params IState<T>[] states)
        {
            foreach (var state in states)
            {
                AddState(state);
            }
        }

        /// <summary>
        /// 상태 제거
        /// </summary>
        /// <param name="stateType">제거할 상태 타입</param>
        /// <returns></returns>
        public bool RemoveState(T stateType)
        {
            if (_states.ContainsKey(stateType))
            {
                if (_currentState != null && _currentState.StateType.Equals(stateType))
                {
                    StateMachineLogWarning("Removing current state:", stateType);
                    _currentState?.OnExit();
                    _currentState = null;
                }
                _states.Remove(stateType);
                StateMachineLog("State removed:", stateType);
                return true;
            }

            StateMachineLogWarning("State not found for removal:", stateType);
            return false;
        }

        /// <summary>
        /// 초기 상태 설정
        /// </summary>
        /// <param name="stateType">시작할 상태 타입</param>
        public void Start(T stateType)
        {
            Assert.IsTrue(_states.ContainsKey(stateType), $"Cannot start with non-existing state: {stateType}");

            if (_currentState != null)
            {
                StateMachineLogWarning("StateMachine already started, changing to:", stateType);
                ChangeState(stateType);
                return;
            }

            ChangeState(stateType);
        }

        
        /// <summary>
        /// 상태 변경
        /// </summary>
        /// <param name="newStateType"></param>
        public void ChangeState(T newStateType)
        {
            // 같은 상태로 변경하려는 경우 무시
            if (_currentState != null && _currentState.StateType.Equals(newStateType))
            {
                StateMachineLog("Already in state:", newStateType, "- ignoring change request");
                return;
            }
            
            Assert.IsTrue(_states.ContainsKey(newStateType), $"Cannot change state with non-existing state: {newStateType}");
            T previousStateType = default;
            if (_currentState != null)
            {
                previousStateType = _currentState.StateType;
            }
            
            // 이전 상태 종료
            _currentState?.OnExit();

            // 새 상태로 전환
            _currentState = _states[newStateType];
            _currentStateType = newStateType;
            
            StateMachineLog("State changed:", previousStateType, "->", newStateType);

            // 새 상태 진입
            _currentState?.OnEnter();
            OnStateChanged?.Invoke(newStateType, previousStateType);
        }

        /// <summary>
        /// 강제 상태 변경 (같은 상태여도 다시 Enter 호출)
        /// </summary>
        /// <param name="newStateType">변경할 상태 타입</param>
        public void ForceChangeState(T newStateType)
        {
            Assert.IsTrue(_states.ContainsKey(newStateType), $"Cannot change state with non-existing state: {newStateType}");
            T previousStateType = default;
            if (_currentState != null)
            {
                previousStateType = _currentState.StateType;
            }
            
            // 이전 상태 종료
            _currentState?.OnExit();

            // 새 상태로 전환
            _currentState = _states[newStateType];
            _currentStateType = newStateType;
            
            StateMachineLog("State changed:", previousStateType, "->", newStateType);

            // 새 상태 진입
            _currentState?.OnEnter();
            OnStateChanged?.Invoke(newStateType, previousStateType);
        }

        /// <summary>
        /// Update 호출
        /// </summary>
        public void Update()
        {
            _currentState?.OnUpdate();
        }

        /// <summary>
        /// FixedUpdate 호출
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.OnFixedUpdate();
        }

        /// <summary>
        /// 특정 상태가 존재하는지 확인
        /// </summary>
        /// <param name="stateType">확인할 상태 타입</param>
        /// <returns>상태 존재 여부</returns>
        public bool HasState(T stateType)
        {
            return _states.ContainsKey(stateType);
        }

        /// <summary>
        /// 특정 상태 인스턴스 가져오기
        /// </summary>
        /// <param name="stateType">가져올 상태 타입</param>
        /// <returns>상태 인스턴스(없으면 null)</returns>
        public IState<T> GetState(T stateType)
        {
            _states.TryGetValue(stateType, out var state);
            return state;
        }

        public TState GetState<TState>(T stateType) where TState : class, IState<T>
        {
            return GetState(stateType) as TState;
        }

        public void Stop()
        {
            if (_currentState != null)
            {
                StateMachineLog("StateMachine stopping, exiting state:", _currentStateType);
                _currentState.OnExit();
                _currentState = null;
            }
        }
        
        public void Clear()
        {
            Stop();
            _states.Clear();
            StateMachineLog("StateMachine cleared");
        }

        #region Logging Method

        private void StateMachineLog(params object[] messages)
        {
            if (!_enableStateMachineLogs || _logger == null)
            {
                return;
            }

            var fullMessage = new object[messages.Length + 1];
            fullMessage[0] = "[StateMachine]";
            Array.Copy(messages, 0, fullMessage, 1, messages.Length);
            _logger.DebugLog(fullMessage);
        }
        
        private void StateMachineLogWarning(params object[] messages)
        {
            if (!_enableStateMachineLogs || _logger == null)
            {
                return;
            }

            var fullMessage = new object[messages.Length + 1];
            fullMessage[0] = "[StateMachine]";
            Array.Copy(messages, 0, fullMessage, 1, messages.Length);
            _logger.DebugLogWarning(fullMessage);
        }
        
        private void StateMachineLogError(params object[] messages)
        {
            if (!_enableStateMachineLogs || _logger == null)
            {
                return;
            }

            var fullMessage = new object[messages.Length + 1];
            fullMessage[0] = "[StateMachine]";
            Array.Copy(messages, 0, fullMessage, 1, messages.Length);
            _logger.DebugLogError(fullMessage);
        }

        #endregion
    }
}