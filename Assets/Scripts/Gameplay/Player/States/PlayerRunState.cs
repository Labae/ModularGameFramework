using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using MarioGame.Gameplay.Player.Core;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerRunState : PlayerBaseState
    {
        private MovementIntent _movementIntent;
        public override PlayerStateType StateType => PlayerStateType.Run;

        public PlayerRunState(StateMachine<PlayerStateType> stateMachine, IDebugLogger logger, PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            _movementIntent = MovementIntentFactory.CreateGroundMovement(_context.MovementConfig, 0, 0);

            StateLog("Player entered running state");
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }

        public override void OnFixedUpdate()
        {
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            var processedInput = FloatUtility.RemoveDeadzone(_context.InputProvider.MoveDirection);

            _movementIntent.HorizontalInput = processedInput;
            _movementIntent.SpeedMultiplier =_context.MovementConfig.RunMultiplier;
            _context.IntentReceiver.SetMovementIntent(_movementIntent);
        }

        public override void OnExit()
        {
            base.OnExit();
            StateLog("Player exited running state");
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

            if (!HasMovementInput())
            {
                ChangeState(PlayerStateType.Idle);
            }
        }
    }
}