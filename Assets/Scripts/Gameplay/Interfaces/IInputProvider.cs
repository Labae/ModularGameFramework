using System;

namespace MarioGame.Gameplay.Interfaces
{
    /// <summary>
    /// 입력 제공자 인터페이스
    /// Player Input과 AI Input 모두 이 인터페이스를 구현
    /// </summary>
    public interface IInputProvider: IDisposable
    {
        /// <summary>
        /// 이동 방향
        /// </summary>
        float MoveDirection { get; }
        
        /// <summary>
        /// 점프 버튼이 눌렸는지
        /// </summary>
        bool JumpPressed { get; }
        
        /// <summary>
        /// 점프 버튼이 눌리고 있는지
        /// </summary>
        bool JumpHeld { get; }
        
        /// <summary>
        /// 점프 버튼을 뗐는지
        /// </summary>
        bool JumpReleased { get; }

        /// <summary>
        /// 웅크리기 버튼을 누르고 있는지
        /// </summary>
        bool CrouchHeld { get; }

        /// <summary>
        /// 입력 업데이트
        /// </summary>
        void UpdateInput();

        /// <summary>
        /// 입력 초기화
        /// </summary>
        void ResetFrameInputs();
    }
}