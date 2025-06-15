using System;
using MarioGame.Core.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace MarioGame.Core.StateMachine
{
    /// <summary>
    /// State Machine에서 사용할 상태의 기본 추상 클래스
    /// 공통 기능과 편의 메서드 제공
    /// </summary>
    /// <typeparam name="T">State Type Enum</typeparam>
    public abstract class BaseState<T> : IState<T> where T : System.Enum
    {
        protected readonly StateMachine<T> _stateMachine;
        protected readonly IDebugLogger _logger;
        protected readonly bool _enableStateLogs = false;

        /// <summary>
        /// 상태 타입
        /// </summary>
        public abstract T StateType { get; }

        /// <summary>
        /// BaseState 생성자
        /// </summary>
        /// <param name="stateMachine">상태를 관리하는 StateMachine</param>
        /// <param name="logger">상태를 소유한 Entity</param>
        protected BaseState(
            StateMachine<T> stateMachine,
            IDebugLogger logger)
        {
            _stateMachine = stateMachine;
            _logger = logger;
            
            Assert.IsNotNull(_stateMachine, "StateMachine is null");
            Assert.IsNotNull(_logger, "Owner is null");
            
            if (logger != null)
            {
                _enableStateLogs = _logger.EnableDebugLogs;
            }
        }

        public virtual void OnEnter()
        {
            StateLog("Entering state:", StateType);
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnExit()
        {
            StateLog("Exiting state:", StateType);
        }

        protected void ChangeState(T newStateType)
        {
            StateLog("Requesting state change:", StateType, "->", newStateType);
            _stateMachine?.ChangeState(newStateType);
        }

        #region Logging Methods
        
        /// <summary>
        /// State 전용 로그
        /// </summary>
        /// <param name="messages"></param>
        protected void StateLog(params object[] messages)
        {
            if (!_enableStateLogs || _logger == null)
            {
                return;
            }
            
            var fullMessage = new object[messages.Length + 1];
            fullMessage[0] = $"[{GetType().Name}]";
            Array.Copy(messages, 0, fullMessage, 
                1, messages.Length);
            _logger.DebugLog(fullMessage);
        }

        /// <summary>
        /// State 전용 경고 로그
        /// </summary>
        /// <param name="messages"></param>
        protected void StateLogWarning(params object[] messages)
        {
            if (!_enableStateLogs || _logger == null)
            {
                return;
            }
            
            var fullMessage = new object[messages.Length + 1];
            fullMessage[0] = $"[{GetType().Name}]";
            Array.Copy(messages, 0, fullMessage, 
                1, messages.Length);
            _logger.DebugLogWarning(fullMessage);
        }
        
        /// <summary>
        /// State 전용 에러 로그
        /// </summary>
        /// <param name="messages"></param>
        protected void StateLogError(params object[] messages)
        {
            if (!_enableStateLogs || _logger == null)
            {
                return;
            }
            
            var fullMessage = new object[messages.Length + 1];
            fullMessage[0] = $"[{GetType().Name}]";
            Array.Copy(messages, 0, fullMessage,
                1, messages.Length);
            _logger.DebugLogError(fullMessage);
        }
        
        #endregion
    }
}