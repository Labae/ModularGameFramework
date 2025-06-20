using System;
using Gameplay.Enums;
using Gameplay.MovementIntents;
using UnityEngine;

namespace Gameplay.Player.States
{
    public class PlayerShootState : PlayerBaseState
    {
        private float _lastShootTime;
        
        public override PlayerStateType StateTypeType => PlayerStateType.IdleShoot;

        public PlayerShootState(Action<PlayerStateType> changeStateAction,
            PlayerStateContext context) : base(changeStateAction, context)
        {
        }

        public override void OnEnter()
        {
            _context.MovementIntentReceiver.SetMovementIntent(MovementIntent.None);
            
            // 총알을 발사한다.
            _lastShootTime = Time.time;
            _context.WeaponActions.Shoot();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if (CanShoot())
            {
                _context.WeaponActions.Shoot();
                _lastShootTime = Time.time;
                return;
            }
        }

        protected override void CheckTransitions()
        {
            if (IsMovementInput())
            {
                ChangeState(PlayerStateType.Run);
                return;
            }
            
            if (Time.time - _lastShootTime >= _context.WeaponActions.ShootToIdleDelay)
            {
                ChangeState(PlayerStateType.Idle);
                return;
            }
        }
    }
}