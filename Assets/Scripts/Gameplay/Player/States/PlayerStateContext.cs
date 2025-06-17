using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Player.Components;

namespace MarioGame.Gameplay.Player.States
{
    public readonly struct PlayerStateContext
    {
        public PlayerMovementConfig MovementConfig { get; }
        public IInputProvider InputProvider { get; }
        public IMovementIntentReceiver IntentReceiver { get; }
        public IPlayerJumpActions JumpActions { get; }
        public IPlayerClimbActions ClimbActions { get; }
        public PlayerWeapon Weapon { get; }

        public PlayerStateContext
        (
            PlayerMovementConfig movementConfig,
            IInputProvider inputProvider,
            IMovementIntentReceiver intentReceiver,
            IPlayerJumpActions jumpActions,
            IPlayerClimbActions climbActions,
            PlayerWeapon weapon
            )
        {
            MovementConfig = movementConfig;
            InputProvider = inputProvider;
            IntentReceiver = intentReceiver;
            JumpActions = jumpActions;
            ClimbActions = climbActions;
            Weapon = weapon;
        }
    }
}