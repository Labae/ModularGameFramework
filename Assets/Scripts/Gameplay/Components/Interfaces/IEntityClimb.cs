using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IEntityClimb
    {
        bool IsClimbing { get; }
        float ClimbVelocity { get; }
        bool CanStartClimbing { get; }
        bool IsMovementLocked { get; }
        
        void Initialize(ClimbMovementConfig config);
        void UpdatePhysics(Rigidbody2D rigidbody);
        void StartClimbing();
        void StopClimbing();
        void ClimbUp(float multiplier = 1.0f);
        void ClimbDown(float multiplier = 1.0f);
        void JumpFromLadder(float jumpForceMultiplier = 1.0f);
        void ForceStopClimbing();
        void AddLock(EntityMovementLockType lockType);
        void RemoveLock(EntityMovementLockType lockType);
    }
}