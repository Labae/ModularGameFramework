using System;
using MarioGame.Core.Extensions;
using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
    public abstract class IntentBasedMovement : IIntentBasedMovement
    {
        protected readonly IDebugLogger _logger;
        protected float _currentHorizontalSpeed;
        protected EntityMovementLockType _lockType;
        protected MovementIntent _currentIntent = MovementIntent.None;

        public MovementIntent CurrentIntent => _currentIntent;
        public float HorizontalSpeed => _currentHorizontalSpeed;
        public float CurrentSpeed => Mathf.Abs(_currentHorizontalSpeed);
        public bool IsMoving => !FloatUtility.IsVelocityZero(_currentHorizontalSpeed);
        public bool IsMovementLocked => _lockType != EntityMovementLockType.None;

        protected IntentBasedMovement(IDebugLogger logger)
        {
            _logger = logger;
        }

        public virtual void SetMovementIntent(MovementIntent intent)
        {
            _currentIntent = intent;
        }

        public virtual void Update()
        {
            if (IsMovementLocked) return;

            switch (_currentIntent.Type)
            {
                case MovementType.Idle:
                    ExecuteIdle();
                    break;
                case MovementType.Ground:
                    ExecuteGroundMovement();
                    break;
                case MovementType.Air:
                    ExecuteAirMovement();
                    break;
                case MovementType.Forced:
                    ExecuteForcedMovement();
                    break;
                case MovementType.Disabled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void ExecuteIdle()
        {
            ApplyAcceleration(0, _currentIntent.SpeedMultiplier);
        }

        protected virtual void ExecuteGroundMovement()
        {
            ApplyAcceleration(_currentIntent.HorizontalInput, _currentIntent.SpeedMultiplier);
        }

        protected virtual void ExecuteAirMovement()
        {
            ApplyAirControl(_currentIntent.HorizontalInput, _currentIntent.AirControlAmount,
                _currentIntent.SpeedMultiplier);
        }

        protected virtual void ExecuteForcedMovement()
        {
            var targetVelocity = _currentIntent.HorizontalInput * GetBaseSpeed() * _currentIntent.SpeedMultiplier;
            SetHorizontalVelocity(targetVelocity);
        }

        protected abstract float GetBaseSpeed();
        protected abstract void ApplyAcceleration(float inputDirection, float speedMultiplier);
        protected abstract void ApplyAirControl(float inputDirection, float controlAmount, float speedMultiplier);

        protected virtual void SetHorizontalVelocity(float velocity)
        {
            if (IsMovementLocked) return;
            _currentHorizontalSpeed = velocity;
        }

        public virtual void AddLock(EntityMovementLockType lockType)
        {
            _lockType |= lockType;
            _logger?.Entity($"Movement lock added: {lockType}");
        }

        public virtual void RemoveLock(EntityMovementLockType lockType)
        {
            _lockType &= ~lockType;
            if (!IsMovementLocked)
            {
                Stop();
            }

            _logger?.Entity($"Movement lock removed: {lockType}");
        }

        public virtual void Stop()
        {
            _currentHorizontalSpeed = 0f;
            _currentIntent = MovementIntent.None;
        }

        public virtual void UpdatePhysics(Rigidbody2D rigidbody)
        {
            if (rigidbody == null) return;
            rigidbody.velocity = rigidbody.velocity.WithX(_currentHorizontalSpeed);
        }
    }
}