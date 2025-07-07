using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IEntityJump
    {
        float VerticalVelocity { get; }
        bool IsFalling { get; }
        bool IsRising { get; }
        bool IsMovementLocked { get; }
        
        void Update();
        void UpdatePhysics(Rigidbody2D rigidbody);
        void SetVerticalVelocity(float velocity);
        void AddVerticalVelocity(float velocity);
        void AddLock(EntityMovementLockType lockType);
        void RemoveLock(EntityMovementLockType lockType);
        bool TryJump(float forceMultiplier = 1.0f);
        void CutJump();
        void ApplyGravityModifiers(bool jumpInputHeld);
    }
}