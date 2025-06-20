using System;
using Gameplay.Enums;
using Gameplay.MovementIntents;
using UnityEngine;

namespace Gameplay.Player.States
{
    public class PlayerIdleState : PlayerBaseState
    {
        public override PlayerStateType StateTypeType => PlayerStateType.Idle;

        public PlayerIdleState(Action<PlayerStateType> changeStateAction, 
            PlayerStateContext context) : base(changeStateAction, context)
        {
        }

        public override void OnEnter()
        {
            _context.MovementIntentReceiver.SetMovementIntent(MovementIntent.None);
        }


        protected override void CheckTransitions()
        {
            if (IsMovementInput())
            {
                ChangeState(PlayerStateType.Run);
                return;
            }

            if (CanShoot())
            {
                ChangeState(PlayerStateType.IdleShoot);
                return;
            }
        }
    }
}