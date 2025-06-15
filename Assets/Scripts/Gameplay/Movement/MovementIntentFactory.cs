using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enums;

namespace MarioGame.Gameplay.Movement
{
    /// <summary>
    /// Intent 생성을 담당하는 팩토리
    /// 설정 기반으로 일관된 Intent 생성
    /// </summary>
    public static class MovementIntentFactory
    {
        public static MovementIntent CreateIdle()
        {
            return new MovementIntent
            {
                HorizontalInput = 0,
                SpeedMultiplier = 1.0f,
                Type = MovementType.Idle,
                MaintainMomentum = false,
                AllowDirectionChange = true,
                AirControlAmount = 0f
            };
        }
        
        public static MovementIntent CreateGroundMovement(BaseMovementConfig config,
            float input, float speedMultiplier)
        {
            var processedInput = FloatUtility.RemoveDeadzone(input, config.InputDeadzone);
            return new MovementIntent
            {
                HorizontalInput = processedInput,
                SpeedMultiplier = speedMultiplier,
                Type = MovementType.Ground,
                MaintainMomentum = false,
                AllowDirectionChange = true,
                AirControlAmount = 0f
            };
        }        
        
        public static MovementIntent CreateAirControl(BaseMovementConfig config,
            float input, float speedMultiplier)
        {
            var processedInput = FloatUtility.RemoveDeadzone(input, config.InputDeadzone);
            return new MovementIntent
            {
                HorizontalInput = processedInput,
                SpeedMultiplier = speedMultiplier,
                Type = MovementType.Air,
                MaintainMomentum = true,
                AllowDirectionChange = true,
                AirControlAmount = config.AirControlMultiplier,
            };
        }
        
        public static MovementIntent CreateForced(float horizontalForce, float speedMultiplier)
        {
            return new MovementIntent
            {
                HorizontalInput = horizontalForce,
                SpeedMultiplier = speedMultiplier,
                Type = MovementType.Forced,
                MaintainMomentum = false,
                AllowDirectionChange = false,
                AirControlAmount = 0f,
            };
        }
    }
}