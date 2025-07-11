using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enums;

namespace MarioGame.Gameplay.MovementIntents
{
    /// <summary>
    /// Intent 생성을 담당하는 팩토리
    /// 설정 기반으로 일관된 Intent 생성
    /// </summary>
    public static class MovementIntentFactory
    {
        public static MovementIntents.MovementIntent CreateIdle()
        {
            return new MovementIntents.MovementIntent
            {
                HorizontalInput = 0,
                SpeedMultiplier = 1.0f,
                Type = MovementType.Idle,
                MaintainMomentum = false,
                AllowDirectionChange = true,
                AirControlAmount = 0f
            };
        }
        
        public static MovementIntents.MovementIntent CreateGroundMovement(BaseMovementConfig config,
            float input, float speedMultiplier)
        {
            var processedInput = FloatUtility.RemoveDeadzone(input, config.InputDeadzone);
            return new MovementIntents.MovementIntent
            {
                HorizontalInput = processedInput,
                SpeedMultiplier = speedMultiplier,
                Type = MovementType.Ground,
                MaintainMomentum = false,
                AllowDirectionChange = true,
                AirControlAmount = 0f
            };
        }        
        
        public static MovementIntents.MovementIntent CreateAirControl(BaseMovementConfig config,
            float input, float speedMultiplier)
        {
            var processedInput = FloatUtility.RemoveDeadzone(input, config.InputDeadzone);
            return new MovementIntents.MovementIntent
            {
                HorizontalInput = processedInput,
                SpeedMultiplier = speedMultiplier,
                Type = MovementType.Air,
                MaintainMomentum = true,
                AllowDirectionChange = true,
                AirControlAmount = config.AirControlMultiplier,
            };
        }
        
        public static MovementIntents.MovementIntent CreateForced(float horizontalForce, float speedMultiplier)
        {
            return new MovementIntents.MovementIntent
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