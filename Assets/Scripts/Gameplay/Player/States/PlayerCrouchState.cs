using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using MarioGame.Gameplay.Player.Core;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public override PlayerStateType StateType => PlayerStateType.Crouch;

        public PlayerCrouchState(StateMachine<PlayerStateType> stateMachine, IDebugLogger logger, PlayerStatus status,
            PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _context.IntentReceiver.SetMovementIntent(MovementIntent.None);
            StateLog("Player entered crouching state");
        }

        public override void OnExit()
        {
            base.OnExit();
            StateLog("Player exiting crouching state");
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }

        public override void OnFixedUpdate()
        {
        }

        private void CheckTransitions()
        {
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

            if (!_context.InputProvider.CrouchHeld)
            {
                ChangeState(PlayerStateType.Idle);
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