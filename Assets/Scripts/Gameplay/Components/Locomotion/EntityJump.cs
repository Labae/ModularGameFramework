using MarioGame.Core.Extensions;
using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
  public abstract class EntityJump : IEntityJump
    {
        protected readonly IDebugLogger _logger;
        protected readonly IGroundChecker _groundChecker;
        protected EntityMovementLockType _lockType;
        protected float _currentVerticalVelocity;

        public float VerticalVelocity => _currentVerticalVelocity;
        public bool IsGrounded => _groundChecker?.IsGrounded ?? false;
        public bool IsFalling => VerticalVelocity < -FloatUtility.VELOCITY_THRESHOLD;
        public bool IsRising => VerticalVelocity > FloatUtility.VELOCITY_THRESHOLD;
        public bool IsMovementLocked => _lockType != EntityMovementLockType.None;

        protected EntityJump(IDebugLogger logger, IGroundChecker groundChecker)
        {
            _logger = logger;
            _groundChecker = groundChecker;
        }

        public virtual void Update()
        {
            // 하위 클래스에서 구현 (타이머 업데이트 등)
        }

        public virtual void UpdatePhysics(Rigidbody2D rigidbody)
        {
            if (rigidbody == null) return;
            
            // 현재 수직 속도 동기화
            _currentVerticalVelocity = rigidbody.velocity.y;
            
            // 수직 속도가 변경되었다면 적용
            rigidbody.velocity = rigidbody.velocity.WithY(_currentVerticalVelocity);
        }

        public virtual void SetVerticalVelocity(float velocity)
        {
            if (IsMovementLocked) return;
            _currentVerticalVelocity = velocity;
        }

        public virtual void AddVerticalVelocity(float velocity)
        {
            if (IsMovementLocked) return;
            _currentVerticalVelocity += velocity;
        }

        public virtual void AddLock(EntityMovementLockType lockType)
        {
            _lockType |= lockType;
            _logger?.StateMachine($"Jump lock added: {lockType}");
        }

        public virtual void RemoveLock(EntityMovementLockType lockType)
        {
            _lockType &= ~lockType;
            _logger?.StateMachine($"Jump lock removed: {lockType}");
        }

        public abstract bool TryJump(float forceMultiplier = 1.0f);
        public abstract void CutJump();
        public abstract void ApplyGravityModifiers(bool jumpInputHeld);
    }
}