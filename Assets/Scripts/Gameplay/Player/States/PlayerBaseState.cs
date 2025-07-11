using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Player.Core;

namespace MarioGame.Gameplay.Player.States
{
    /// <summary>
    /// 모든 플레이어 상태의 베이스 클래스
    /// PlayerController와 필요한 컴포넌트
    /// </summary>
    public abstract class PlayerBaseState : BaseState<PlayerStateType>
    {
        protected readonly PlayerStatus _status;
        protected readonly PlayerStateContext _context;

        protected PlayerBaseState(StateMachine<PlayerStateType> stateMachine, 
            IDebugLogger logger, PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger)
        {
            _status = status;
            _context = context;
        }

        protected bool HasMovementInput()
        {
            return FloatUtility.IsInputActive(_context.InputProvider.MoveDirection);
        }

        protected bool CheckClimbTransition()
        {
            if (!_status.IsOnLadderValue)
            {
                return false;
            }
            
            var verticalInput = _context.InputProvider.VerticalInput;
            var processedInput =
                FloatUtility.RemoveDeadzone(verticalInput, _context.MovementConfig.ClimbConfig.ClimbInputDeadzone);

            if (!FloatUtility.IsInputActive(processedInput))
            {
                return false;
            }

            if (processedInput > 0 && _status.IsAtLadderTopValue)
            {
                return false;
            }

            if (processedInput < 0 && _status.IsAtLadderBottomValue)
            {
                return false;
            }
            
            return true;
        }
    }
}