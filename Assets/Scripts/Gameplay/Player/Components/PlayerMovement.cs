using System;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.MovementIntents;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Components
{
    /// <summary>
    /// 플레이어 전용 수평 이동 구현
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerMovement : EntityMovement, IMovementIntentReceiver
    {
        private PlayerMovementConfig _config;
        
        [field: SerializeField]
        public MovementIntent CurrentIntent { get; private set; }

        public void Initialize(PlayerMovementConfig config)
        {
            _config = config;
            AssertIsNotNull(_config, "PlayerMovementConfig required");
        }
        
        public void SetMovementIntent(MovementIntent intent)
        {
            CurrentIntent = intent;
        }

        private void FixedUpdate()
        {
            if (_config == null)
            {
                return;
            }

            switch (CurrentIntent.Type)
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
        
        private void ExecuteIdle()
        {
            ApplyAcceleration(0, CurrentIntent.SpeedMultiplier);
        }
        
        private void ExecuteGroundMovement()
        {
            ApplyAcceleration(CurrentIntent.HorizontalInput, CurrentIntent.SpeedMultiplier);
        }

        private void ExecuteAirMovement()
        {
            ApplyAirControl(CurrentIntent.HorizontalInput, CurrentIntent.AirControlAmount, CurrentIntent.SpeedMultiplier);
        }

        private void ExecuteForcedMovement()
        {
            var targetVelocity = CurrentIntent.HorizontalInput * _config.BaseSpeed * CurrentIntent.SpeedMultiplier;
            SetHorizontalVelocity(targetVelocity);
        }

        public override void ApplyAcceleration(float inputDirection, float speedMultiplier = 1)
        {
            if (_config == null)
            {
                LogError("PlayerMovementConfig not initialized");
                return;
            }
            
            var processedInput = FloatUtility.RemoveDeadzone(inputDirection, _config.InputDeadzone);
            var targetSpeed = processedInput * _config.BaseSpeed * speedMultiplier;
            var acceleration = _config.Acceleration;
            
            if (FloatUtility.IsDirectionChanged(_currentHorizontalSpeed, inputDirection))
            {
                acceleration *= 1.5f;
            }
            
            if (FloatUtility.IsInputActive(processedInput))
            {
                _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 
                    targetSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 
                    0, _config.Deceleration * Time.fixedDeltaTime);
            }
            
            SetHorizontalVelocity(_currentHorizontalSpeed);
        }

        public override void ApplyAirControl(float inputDirection, float controlAmount, float speedMultiplier = 1)
        {
            if (_config == null)
            {
                LogError("PlayerMovementConfig not initialized");
                return;
            }
            
            var processedInput = FloatUtility.RemoveDeadzone(inputDirection, _config.InputDeadzone);
            var targetSpeed = processedInput * _config.BaseSpeed * speedMultiplier;
            var acceleration = _config.Acceleration * controlAmount;
            
            if (FloatUtility.IsInputActive(processedInput))
            {
                _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 
                    targetSpeed, acceleration * Time.fixedDeltaTime);
                
                SetHorizontalVelocity(_currentHorizontalSpeed);
            }
        }

        public override void Stop()
        {
            base.Stop();
            CurrentIntent = MovementIntent.None;
        }
    }
}