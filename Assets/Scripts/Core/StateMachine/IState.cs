using System;

namespace Core.StateMachine
{
    /// <summary>
    /// StateMachine에서 사용할 상태의 기본 인터페이스
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 상태 진입 시 호출
        /// </summary>
        void OnEnter();
        
        /// <summary>
        /// 매 프레임 호출
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// 고정 프레임 호출
        /// </summary>
        void OnFixedUpdate();
        
        /// <summary>
        /// 상태 종료시 호출
        /// </summary>
        void OnExit();
    }

    /// <summary>
    /// Generic State 인터페이스입
    /// <typeparam name="T">State Type</typeparam>
    /// </summary>
    public interface IState<T> : IState where T : Enum
    {
        /// <summary>
        /// 상태 타
        /// </summary>
        T StateType { get; }
    }
}
