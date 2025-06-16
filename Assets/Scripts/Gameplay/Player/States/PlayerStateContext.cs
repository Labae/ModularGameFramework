using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Interfaces;

namespace MarioGame.Gameplay.Player.States
{
    public readonly struct PlayerStateContext
    {
        public PlayerMovementConfig MovementConfig { get; }
        public IInputProvider InputProvider { get; }
        public IMovementIntentReceiver IntentReceiver { get; }
        public IPlayerJumpActions JumpActions { get; }
        public IPlayerClimbActions ClimbActions { get; }

        public PlayerStateContext
        (
            PlayerMovementConfig movementConfig,
            IInputProvider inputProvider,
            IMovementIntentReceiver intentReceiver,
            IPlayerJumpActions jumpActions,
            IPlayerClimbActions climbActions)
        {
            MovementConfig = movementConfig;
            InputProvider = inputProvider;
            IntentReceiver = intentReceiver;
            JumpActions = jumpActions;
            ClimbActions = climbActions;
        }
    }
}