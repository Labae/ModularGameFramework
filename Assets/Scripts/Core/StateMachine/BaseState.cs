using System;
using Debugging;
using UnityEngine.Assertions;

namespace Core.StateMachine
{
    public abstract class BaseState<T> : IState<T> where T : Enum
    {
        private readonly StateMachine<T> _stateMachine;
        
        public abstract T StateType { get; }

        protected BaseState(StateMachine<T> stateMachine)
        {
            _stateMachine = stateMachine;
            Assert.IsNotNull(_stateMachine, "StateMachine이 null입니다.");
        }
        
        public virtual void OnEnter()
        {
            Logger.StateMachine($"State({StateType}) 진입");
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnExit()
        {
            Logger.StateMachine($"State({StateType}) 종료");
        }

        protected void ChangeState(T newStateType)
        {
            Logger.StateMachine($"State변경 요청: {StateType} -> {newStateType}");
            _stateMachine.ChangeState(newStateType);
        }
    }
}