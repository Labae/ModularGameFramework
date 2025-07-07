using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Config.Movement;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
public class PlayerMovement : IntentBasedMovement
    {
        private PlayerMovementConfig _config;

        public PlayerMovement(IDebugLogger logger) : base(logger) { }

        public void Initialize(PlayerMovementConfig config)
        {
            _config = config;
            _logger?.Entity($"PlayerMovement initialized with speed: {_config?.BaseSpeed}");
        }

        protected override float GetBaseSpeed()
        {
            return _config?.BaseSpeed ?? 0f;
        }

        protected override void ApplyAcceleration(float inputDirection, float speedMultiplier)
        {
            if (_config == null)
            {
                _logger?.Error("PlayerMovementConfig not initialized");
                return;
            }
            
            var processedInput = FloatUtility.RemoveDeadzone(inputDirection, _config.InputDeadzone);
            var targetSpeed = processedInput * _config.BaseSpeed * speedMultiplier;
            var acceleration = _config.Acceleration;
            
            // Player 특화: 방향 전환시 더 빠른 가속
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
        }

        protected override void ApplyAirControl(float inputDirection, float controlAmount, float speedMultiplier)
        {
            if (_config == null)
            {
                _logger?.Error("PlayerMovementConfig not initialized");
                return;
            }
            
            var processedInput = FloatUtility.RemoveDeadzone(inputDirection, _config.InputDeadzone);
            var targetSpeed = processedInput * _config.BaseSpeed * speedMultiplier;
            var acceleration = _config.Acceleration * controlAmount;
            
            if (FloatUtility.IsInputActive(processedInput))
            {
                _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 
                    targetSpeed, acceleration * Time.fixedDeltaTime);
            }
        }
    }
}