using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using MarioGame.Gameplay.Player.Core;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerClimbState : PlayerBaseState
    {
        public override PlayerStateType StateType => PlayerStateType.Climb;

        public PlayerClimbState(StateMachine<PlayerStateType> stateMachine, IDebugLogger logger,
            PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            StateLog("Player entered climb state");
            
            _context.ClimbActions.StartClimbing();
            _context.IntentReceiver.SetMovementIntent(MovementIntent.None);
        }

        public override void OnExit()
        {
            StateLog("Player exited climb state");
            _context.ClimbActions.StopClimbing();
            base.OnExit();
        }

        public override void OnUpdate()
        {
            HandleClimbInput();
            CheckTransitions();
        }
        
        private void HandleClimbInput()
        {
            if (_context.InputProvider.JumpPressed)
            {
                HandleJumpFromLadder();
                return;
            }

            var verticalInput = _context.InputProvider.VerticalInput;
            var processedInput =
                FloatUtility.RemoveDeadzone(verticalInput, _context.MovementConfig.ClimbConfig.ClimbInputDeadzone);

            if (!FloatUtility.IsInputActive(processedInput))
            {
                _context.ClimbActions.ClimbUp(0);
                return;
            }
            
            if (processedInput > _context.MovementConfig.ClimbConfig.ClimbInputDeadzone)
            {
                if (!_status.IsAtLadderTopValue)
                {
                    _context.ClimbActions.ClimbUp();
                }
                else
                {
                    ExitLadderToTop();
                }
            }
            else if (processedInput < -_context.MovementConfig.ClimbConfig.ClimbInputDeadzone)
            {
                if (!_status.IsAtLadderBottomValue)
                {
                    _context.ClimbActions.ClimbDown();
                }
                else
                {
                    ExitLadderToBottom();
                }
            }
        }

        private void HandleJumpFromLadder()
        {
            StateLog("Jumping from ladder");

            _context.ClimbActions.JumpFromLadder();

            ChangeState(PlayerStateType.Jump);
        }

        private void ExitLadderToTop()
        {
            ChangeState(PlayerStateType.Idle);
        }
        
        private void ExitLadderToBottom()
        {
            if (_status.IsGroundedValue)
            {
                ChangeState(PlayerStateType.Idle);
            }
            else
            {
                ChangeState(PlayerStateType.Fall);
            }
        }
        
        private void CheckTransitions()
        {
            if (!_status.IsOnLadderValue)
            {
                ChangeState(PlayerStateType.Fall);
                return;
            }
            
            var verticalInput = _context.InputProvider.VerticalInput;
            var processedInput =
                FloatUtility.RemoveDeadzone(verticalInput, _context.MovementConfig.ClimbConfig.ClimbInputDeadzone);

            if (_status.IsAtLadderTopValue && processedInput > 0)
            {
                ExitLadderToTop();
                return;
            }
            
            if (_status.IsAtLadderBottomValue && processedInput < 0)
            {
                ExitLadderToBottom();
                return;
            }
        }
    }
}