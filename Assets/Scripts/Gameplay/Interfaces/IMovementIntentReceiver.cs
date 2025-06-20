using Gameplay.MovementIntents;

namespace Gameplay.Interfaces
{
    public interface IMovementIntentReceiver
    {
        MovementIntent CurrentIntent { get; }

        void SetMovementIntent(MovementIntent intent);
    }
}