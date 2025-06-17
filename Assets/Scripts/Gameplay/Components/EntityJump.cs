using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Core.Utilities;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public abstract class EntityJump : CoreBehaviour
    {
        protected Rigidbody2D _rigidbody2D;
        [SerializeField]
        protected GroundChecker _groundChecker;
        
        public float VerticalVelocity { get; private set; }
        public bool IsGrounded => _groundChecker.IsGrounded;
        public bool IsFalling => _rigidbody2D.velocity.y < -FloatUtility.VELOCITY_THRESHOLD;
        public bool IsRising => VerticalVelocity > FloatUtility.VELOCITY_THRESHOLD;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _groundChecker = GetComponentInChildren<GroundChecker>();
            AssertIsNotNull(_rigidbody2D, "Rigidbody2D required");
            AssertIsNotNull(_groundChecker, "GroundChecker required");
        }

        public virtual void SetVerticalVelocity(float velocity)
        {
            VerticalVelocity = velocity;
            _rigidbody2D.velocity = _rigidbody2D.velocity.WithY(VerticalVelocity);
        }

        public virtual void AddVerticalVelocity(float velocity)
        {
            VerticalVelocity = velocity;
            _rigidbody2D.velocity = _rigidbody2D.velocity.AddY(VerticalVelocity);
        }

        public abstract bool TryJump(float forceMultiplier = 1.0f);
        public abstract void CutJump();
        public abstract void ApplyGravityModifiers(bool jumpInputHeld);
    }
}