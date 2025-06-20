using System;
using Core.StateMachine;
using Core.Utilities;
using Gameplay.Enums;

namespace Gameplay.Player.States
{
    public abstract class PlayerBaseState : BaseState<PlayerStateType>
    {
        protected readonly PlayerStateContext _context;

        protected PlayerBaseState(Action<PlayerStateType> changeStateAction,
            PlayerStateContext context) : base(changeStateAction)
        {
            _context = context;
        }

        protected bool IsMovementInput()
        {
            var horizontalInput = _context.InputProvider.HorizontalInput;
            return FloatUtility.IsInputActive(horizontalInput);
        }

        protected bool CanShoot()
        {
            return _context.InputProvider.ShootPressed && _context.WeaponActions.CanFire();
        }
    }
}