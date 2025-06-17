using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using MarioGame.Gameplay.Player.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerIdleShoot : PlayerBaseState
    {
        public override PlayerStateType StateType => PlayerStateType.IdleShoot;
        
        private float _lastFireTime;
        private readonly float _shootIdleTimeout = 2.0f;
        
        public PlayerIdleShoot(StateMachine<PlayerStateType> stateMachine,
            IDebugLogger logger, PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }
        
        public override void OnEnter()
        {
            base.OnEnter();

            var intent = MovementIntentFactory.CreateIdle();
            _context.IntentReceiver.SetMovementIntent(intent);

            _lastFireTime = Time.time;

            _context.Weapon.OnFired += OnFired;
            
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
            _context.Weapon.OnFired -= OnFired;
            base.OnExit();
            StateLog("Player exited idle state");
        }
        
        private void OnFired()
        {
            _lastFireTime = Time.time;
        }
        
        private void CheckTransitions()
        {
            if (CheckClimbTransition())
            {
                ChangeState(PlayerStateType.Climb);
                return;
            }
            
            if (Time.time - _lastFireTime > _shootIdleTimeout)
            {
                ChangeState(PlayerStateType.Idle);
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