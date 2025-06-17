using System;

namespace MarioGame.Gameplay.Enums
{
    [Flags]
    public enum EntityMovementLockType
    {
        None = 0,
        PhysicsReaction
    }
}