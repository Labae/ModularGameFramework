using MarioGame.Core.Enums;
using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Input
{
    public class AIInputProvider : IInputProvider
    {
        public float MoveDirection { get; private set; }
        public float VerticalInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleased { get; private set; }
        public bool CrouchHeld { get; private set; }

        private bool _wantsToJump;
        private float _desiredMoveDirection;
        
        public void UpdateInput()
        {
            MoveDirection = _desiredMoveDirection;
            JumpPressed = _wantsToJump;
            JumpHeld = _wantsToJump;
        }

        public void ResetFrameInputs()
        {
            _wantsToJump = false;
        }

        /// <summary>
        /// AI가 이동하고 싶은 방향 설정
        /// </summary>
        /// <param name="moveDirection"></param>
        public void SetMoveDirection(float moveDirection)
        {
            _desiredMoveDirection = Mathf.Clamp(moveDirection, -1f, 1f);
        }

        /// <summary>
        /// AI가 점프하고 싶음을 설정
        /// </summary>
        public void RequestJump()
        {
            _wantsToJump = true;
        }

        public void StopAllInputs()
        {
            _wantsToJump = false;
        }

        public void Dispose()
        {
            
        }
    }
}