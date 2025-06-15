using System;
using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Core.Utilities;
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
        
        public float HorizontalSpeed => _currentHorizontalSpeed;
        public float CurrentSpeed => Mathf.Abs(_currentHorizontalSpeed);
        public bool IsMoving => !FloatUtility.IsVelocityZero(_currentHorizontalSpeed);

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.freezeRotation = true;
            _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            AssertIsNotNull(_rigidbody2D, "Rigidbody2D required");
        }

        public virtual void SetHorizontalVelocity(float velocity)
        {
            _currentHorizontalSpeed = velocity;
            _rigidbody2D.velocity = _rigidbody2D.velocity.WithX(velocity);
        }

        public virtual void ModifyHorizontalVelocity(float multiplier)
        {
            SetHorizontalVelocity(_currentHorizontalSpeed * multiplier);
        }
        
        public abstract void ApplyAcceleration(float inputDirection, float speedMultiplier = 1);
        public abstract void ApplyAirControl(float inputDirection, float controlAmount, float speedMultiplier = 1);

        public virtual void Stop()
        {
            _currentHorizontalSpeed = 0;
            SetHorizontalVelocity(0);
        }

        public virtual void ApplyKnockback(float force)
        {
            var knockbackVelocity = Mathf.Sign(transform.position.x) * force;
            SetHorizontalVelocity(knockbackVelocity);
        }
    }
}