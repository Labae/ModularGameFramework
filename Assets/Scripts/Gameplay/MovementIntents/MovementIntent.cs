using System;

namespace Gameplay.MovementIntents
{
    public enum MovementType
    {
        Idle,
        Ground,
        Air,
    }
    
    [Serializable]
    public struct MovementIntent
    {
        public float HorizontalInput;

        public MovementType Type;
        public float SpeedMultiplier;

        public static MovementIntent None => new MovementIntent
        {
            HorizontalInput = 0,
            Type = MovementType.Idle,
            SpeedMultiplier = 1
        };
    }
}