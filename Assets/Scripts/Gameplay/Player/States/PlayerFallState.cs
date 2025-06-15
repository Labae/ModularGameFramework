using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Movement;
using MarioGame.Gameplay.Player.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerFallState : PlayerBaseState
    {
        private MovementIntent _movementIntent;
        private float _fallTime;
        public override PlayerStateType StateType => PlayerStateType.Fall;
        
        public PlayerFallState(StateMachine<PlayerStateType> stateMachine, IDebugLogger logger, PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            _movementIntent = MovementIntentFactory.CreateAirControl(_context.MovementConfig, 0, 0);
            _fallTime = 0.0f;
            StateLog("Player entered falling state");
        }
        
        public override void OnExit()
        {
            base.OnExit();
            StateLog("Player exiting falling state");
        }

        public override void OnUpdate()
        {
            _fallTime += Time.deltaTime;
            HandleJumpInput();
            CheckTransitions();
        }

        public override void OnFixedUpdate()
        {
            UpdateAirMovement();
        }
        
        private void HandleJumpInput()
        {
            // 코요테 타임/점프 버퍼 체크
            if (_context.InputProvider.JumpPressed)
            {
                _context.JumpActions.RequestJump();

                if (_context.JumpActions.TryJump())
                {
                    ChangeState(PlayerStateType.Jump);
                    return;
                }
            }
        }

        private void CheckTransitions()
        {
            if (_status.IsRisingValue)
            {
                ChangeState(PlayerStateType.Jump);
                return;
            }

            if (_status.IsGroundedValue)
            {
                if (HasMovementInput())
                {
                    ChangeState(PlayerStateType.Run);
                }
                else
                {
                    ChangeState(PlayerStateType.Idle);
                }   
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