using System;
using Debugging;
using UnityEngine.Assertions;

namespace Core.StateMachine
{
    public abstract class BaseState<T> : IState<T> where T : Enum
    {
        private readonly Action<T> _changeChangeStateAction;

        public abstract T StateTypeType { get; }

        protected BaseState(Action<T> changeChangeStateAction)
        {
            _changeChangeStateAction = changeChangeStateAction;
            Assert.IsNotNull(_changeChangeStateAction, "ChangeState 함수가 null입니다.");
        }

        public virtual void OnEnter()
        {
            Logger.StateMachine($"State({StateTypeType}) 진입");
        }

        public virtual void OnUpdate()
        {
            CheckTransitions();
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnExit()
        {
            Logger.StateMachine($"State({StateTypeType}) 종료");
        }

        protected void ChangeState(T newStateType)
        {
            Logger.StateMachine($"State변경 요청: {StateTypeType} -> {newStateType}");
            _changeChangeStateAction?.Invoke(newStateType);
        }

        protected virtual void CheckTransitions()
        {
            
        }
    }
}