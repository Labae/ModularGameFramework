using Gameplay.Interfaces;
using Gameplay.MovementIntents;

namespace Gameplay.Player.States
{
    public readonly struct PlayerStateContext
    {
        public IPlayerInputProvider InputProvider { get; }
        public IMovementIntentReceiver MovementIntentReceiver { get; }
        public IPlayerWeaponActions WeaponActions { get; }

        public PlayerStateContext(
            IPlayerInputProvider inputProvider,
            IMovementIntentReceiver movementIntentReceiver,
            IPlayerWeaponActions weaponActions)
        {
            InputProvider = inputProvider;
            MovementIntentReceiver = movementIntentReceiver;
            WeaponActions = weaponActions;
        }
    }
}