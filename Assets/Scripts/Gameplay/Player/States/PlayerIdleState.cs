using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using MarioGame.Gameplay.Player.Core;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerIdleState : PlayerBaseState
    {
        public override PlayerStateType StateType => PlayerStateType.Idle;

        public PlayerIdleState(StateMachine<PlayerStateType> stateMachine, IDebugLogger logger, PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }


        public override void OnEnter()
        {
            base.OnEnter();

            var intent = MovementIntentFactory.CreateIdle();
            _context.IntentReceiver.SetMovementIntent(intent);
            StateLog("Player entered idle state");
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }

        public override void OnFixedUpdate()
        {
        }

        public override void OnExit()
        {
            base.OnExit();
            StateLog("Player exited idle state");
        }

        private void CheckTransitions()
        {
            if (CheckClimbTransition())
            {
                ChangeState(PlayerStateType.Climb);
                return;
            }
            
            if (!_status.IsGroundedValue)
            {
                if (_status.IsRisingValue)
                {
                    ChangeState(PlayerStateType.Jump);
                }
                else if (_status.IsFallingValue)
                {
                    ChangeState(PlayerStateType.Fall);
                }
            }

            if (_context.InputProvider.JumpPressed)
            {
                if (_context.JumpActions.TryJump())
                {
                    ChangeState(PlayerStateType.Jump);
                    return;
                }
            }

            if (_context.InputProvider.CrouchHeld)
            {
                ChangeState(PlayerStateType.Crouch);
                return;
            }
            
            if (HasMovementInput())
            {
                ChangeState(PlayerStateType.Run);
                return;
            }
        }
    }
}