using System;
using Core;
using Gameplay.Interfaces;
using Gameplay.MovementIntents;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gameplay.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class EntityMovement : CoreBehaviour, IMovementIntentReceiver
    {
        private Rigidbody2D _rigidbody2D;
        public MovementIntent CurrentIntent { get; private set; }

        [SerializeField] private float _horizontalVelocity = 0f;
        [SerializeField] private float _baseSpeed = 1f;
        [SerializeField] private float _acceleration = 1;
        [SerializeField] private float _deceleration = 1;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            Assert.IsNotNull(_rigidbody2D, "Rigidbody2D가 Null입니다.");
            _rigidbody2D.freezeRotation = true;
            _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void SetMovementIntent(MovementIntent intent)
        {
            CurrentIntent = intent;
        }

        private void FixedUpdate()
        {
            switch (CurrentIntent.Type)
            {
                case MovementType.Idle:
                    ExecuteIdle();
                    break;
                case MovementType.Ground:
                    ExecuteGround();
                    break;
                case MovementType.Air:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void ExecuteIdle()
        {
            ApplyAcceleration(0, CurrentIntent.SpeedMultiplier);
        }

        private void ExecuteGround()
        {
            ApplyAcceleration(CurrentIntent.HorizontalInput, CurrentIntent.SpeedMultiplier);
        }

        private void SetHorizontalVelocity(float velocity)
        {
            _horizontalVelocity = velocity;
            var currentVelocity = _rigidbody2D.velocity;
            var velocityDifference = _horizontalVelocity - currentVelocity.x;
            _rigidbody2D.AddForce(Vector2.right * velocityDifference);
        }

        private void ApplyAcceleration(float direction, float speedMultiplier = 1f)
        {
            var targetSpeed = direction * _baseSpeed * speedMultiplier;
            var finalSpeed = 0f;
            if (direction == 0)
            {
                finalSpeed = Mathf.MoveTowards(_horizontalVelocity, 0, _deceleration * Time.fixedDeltaTime);
            }
            else
            {
                finalSpeed = Mathf.MoveTowards(_horizontalVelocity, targetSpeed, _acceleration * Time.fixedDeltaTime);
            }
            SetHorizontalVelocity(finalSpeed);
        }
    }
}