using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using MarioGame.Gameplay.Player.Core;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerJumpState : PlayerBaseState
    {
        private MovementIntent _movementIntent;
        public override PlayerStateType StateType => PlayerStateType.Jump;

        public PlayerJumpState(StateMachine<PlayerStateType> stateMachine, IDebugLogger logger, PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _movementIntent = MovementIntentFactory.CreateAirControl(_context.MovementConfig, 0, 0);
            StateLog("Player entered jumping state");
        }
        
        public override void OnExit()
        {
            base.OnExit();
            StateLog("Player exiting jumping state");
        }

        public override void OnUpdate()
        {
            HandleJumpInput();
            CheckTransitions();
        }

        public override void OnFixedUpdate()
        {
            UpdateAirMovement();
        }
        
        private void HandleJumpInput()
        {
            _context.JumpActions.SetJumpInputHeld(_context.InputProvider.JumpHeld);

            if (_context.InputProvider.JumpReleased)
            {
                _context.JumpActions.CutJump();
            }
        }

        private void CheckTransitions()
        {
            if (CheckClimbTransition())
            {
                ChangeState(PlayerStateType.Climb);
                return;
            }
            
            if (_status.IsFallingValue)
            {
                ChangeState(PlayerStateType.Fall);
                return;
            }

            if (_status.IsGroundedValue && _status.VerticalVelocityValue <= 0)
            {
                if (HasMovementInput())
                {
                    ChangeState(PlayerStateType.Run);
                }
                else
                {
                    ChangeState(PlayerStateType.Idle);
                }

                return;
            }
        }
        
        private void UpdateAirMovement()
        {
            var processedInput = FloatUtility.RemoveDeadzone(_context.InputProvider.MoveDirection);
            
            _movementIntent.HorizontalInput = processedInput;
            _movementIntent.SpeedMultiplier = _context.MovementConfig.RunMultiplier;
            
            _context.IntentReceiver.SetMovementIntent(_movementIntent);
        }
    }
}