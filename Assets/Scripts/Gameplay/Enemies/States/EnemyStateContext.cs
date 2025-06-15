using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Input;
using MarioGame.Gameplay.Interfaces;

namespace MarioGame.Gameplay.Enemies.States
{
    public readonly struct EnemyStateContext
    {
        public EnemyMovementConfig MovementConfig { get; }
        public AIInputProvider InputProvider { get; }
        public IMovementIntentReceiver IntentReceiver { get; }

        public EnemyStateContext(
            EnemyMovementConfig movementConfig,
            AIInputProvider inputProvider,
            IMovementIntentReceiver intentReceiver)
        {
            MovementConfig = movementConfig;
            InputProvider = inputProvider;
            IntentReceiver = intentReceiver;
        }
    }
}