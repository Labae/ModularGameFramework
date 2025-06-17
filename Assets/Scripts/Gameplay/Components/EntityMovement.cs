using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    /// <summary>
    /// Entity의 이동을 담당하는 컴포넌트
    /// Player, Enemy 등 모든 Entity에서 재사용 가능
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public abstract class EntityMovement : CoreBehaviour
    {
        protected Rigidbody2D _rigidbody2D;
        protected float _currentHorizontalSpeed;
        
        [SerializeField]
        protected EntityMovementLockType _lockType;
        
        public float HorizontalSpeed => _currentHorizontalSpeed;
        public float CurrentSpeed => Mathf.Abs(_currentHorizontalSpeed);
        public bool IsMoving => !FloatUtility.IsVelocityZero(_currentHorizontalSpeed);
        public bool IsMovementLocked => _lockType != EntityMovementLockType.None;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.freezeRotation = true;
            _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            AssertIsNotNull(_rigidbody2D, "Rigidbody2D required");
        }

        protected void SetHorizontalVelocity(float velocity)
        {
            if (IsMovementLocked)
            {
                return;
            }
            
            _currentHorizontalSpeed = velocity;
            _rigidbody2D.velocity = _rigidbody2D.velocity.WithX(velocity);
        }

        protected void ModifyHorizontalVelocity(float multiplier)
        {
            if (IsMovementLocked)
            {
                return;
            }
            
            SetHorizontalVelocity(_currentHorizontalSpeed * multiplier);
        }
        
        public void AddLock(EntityMovementLockType lockType)
        {
            _lockType |= lockType;
        }

        public void RemoveLock(EntityMovementLockType lockType)
        {
            _lockType &= ~lockType;
            if (!IsMovementLocked)
            {
                Stop();
            }
        }
        
        public abstract void ApplyAcceleration(float inputDirection, float speedMultiplier = 1);
        public abstract void ApplyAirControl(float inputDirection, float controlAmount, float speedMultiplier = 1);

        public virtual void Stop()
        {
            _currentHorizontalSpeed = 0;
            SetHorizontalVelocity(0);
        }


    }
}