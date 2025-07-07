using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Input;
using MarioGame.Gameplay.Interfaces;

namespace MarioGame.Gameplay.Enemies.States
{
    public readonly struct EnemyStateContext
    {
        public EnemyMovementConfig MovementConfig { get; }
        public AIInputProvider InputProvider { get; }
        public IIntentBasedMovement Movement { get; }

        public EnemyStateContext(
            EnemyMovementConfig movementConfig,
            AIInputProvider inputProvider,
            IIntentBasedMovement movement)
        {
            MovementConfig = movementConfig;
            InputProvider = inputProvider;
            Movement = movement;
        }
    }
}