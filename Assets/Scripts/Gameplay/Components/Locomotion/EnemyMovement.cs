using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Config.Movement;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
 public class EnemyMovement : IntentBasedMovement
    {
        private EnemyMovementConfig _config;

        public EnemyMovement(IDebugLogger logger) : base(logger) { }

        public void Initialize(EnemyMovementConfig config)
        {
            _config = config;
            _logger?.Entity($"EnemyMovement initialized with speed: {_config?.BaseSpeed}");
        }

        protected override float GetBaseSpeed()
        {
            return _config?.BaseSpeed ?? 0f;
        }

        protected override void ApplyAcceleration(float inputDirection, float speedMultiplier)
        {
            if (_config == null)
            {
                _logger?.Error("EnemyMovementConfig not initialized");
                return;
            }
            
            var processedInput = FloatUtility.RemoveDeadzone(inputDirection, _config.InputDeadzone);
            var targetSpeed = processedInput * _config.BaseSpeed * speedMultiplier;
            var acceleration = _config.Acceleration;

            // Enemy 특화: 방향 전환시 약간의 가속 보너스
            if (FloatUtility.IsDirectionChanged(_currentHorizontalSpeed, processedInput))
            {
                acceleration *= 1.2f;
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
                _logger?.Error("EnemyMovementConfig not initialized");
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