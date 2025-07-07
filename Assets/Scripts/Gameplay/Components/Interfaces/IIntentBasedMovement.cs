using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IIntentBasedMovement : IMovementIntentReceiver
    {
        float HorizontalSpeed { get; }
        float CurrentSpeed { get; }
        bool IsMoving { get; }
        bool IsMovementLocked { get; }
        
        void Update(); 
        void UpdatePhysics(Rigidbody2D rigidbody);
        void Stop();
        void AddLock(EntityMovementLockType lockType);
        void RemoveLock(EntityMovementLockType lockType);
    }
}