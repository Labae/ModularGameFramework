using System;
using Core.StateMachine;
using Gameplay.Enums;
using Gameplay.MovementIntents;
using UnityEngine;

namespace Gameplay.Player.States
{
    public class PlayerWalkState : PlayerBaseState
    {
        private MovementIntent _movementIntent;
        public PlayerWalkState(Action<PlayerStateType> changeStateAction,
            PlayerStateContext context) : base(changeStateAction, context)
        {
        }

        public override PlayerStateType StateTypeType => PlayerStateType.Run;

        public override void OnEnter()
        {
            base.OnEnter();
            _movementIntent = new MovementIntent {
                HorizontalInput = 0f,
                Type = MovementType.Ground,
                SpeedMultiplier = 1.2f
            };
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            _movementIntent.HorizontalInput = _context.InputProvider.HorizontalInput;
            _context.MovementIntentReceiver.SetMovementIntent(_movementIntent);
        }

        protected override void CheckTransitions()
        {
            if (!IsMovementInput())
            {
                ChangeState(PlayerStateType.Idle);
                return;
            }
            
            if (CanShoot())
            {
                ChangeState(PlayerStateType.IdleShoot);
            }
        }
    }
}