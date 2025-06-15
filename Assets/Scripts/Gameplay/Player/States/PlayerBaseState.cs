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
    }
}