namespace Gameplay.MovementIntents
{
    public interface IMovementIntentReceiver
    {
        MovementIntent CurrentIntent { get; }

        void SetMovementIntent(MovementIntent intent);
    }
}