namespace MarioGame.Core.StateMachine
{
    /// <summary>
    /// State Machine에서 사용할 상태의 기본 인터페이스
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
        /// 상태 종료 시 호출
        /// </summary>
        void OnExit();
    }

    /// <summary>
    /// Generic State 인터페이스 (Enum 타입 지원)
    /// </summary>
    /// <typeparam name="T">State Type Enum</typeparam>
    public interface IState<T> : IState where T : System.Enum
    {
        /// <summary>
        /// 상태 타입 (Enum 값)
        /// </summary>
        T StateType { get; }
    }
}