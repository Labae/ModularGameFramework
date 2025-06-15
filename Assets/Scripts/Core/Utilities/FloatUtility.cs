using UnityEngine;

namespace MarioGame.Core.Utilities
{
    public static class FloatUtility
    {
        #region Constants

        public const float INPUT_DEADZONE = 0.01f;
        
        public const float VELOCITY_THRESHOLD = 0.1f;

        public const float POSITION_THRESHOLD = 0.01f;

        public const float EPSILON = 1e-6f;

        #endregion
        
        #region Input Processing

        public static bool IsInDeadzone(float input, float deadzone = INPUT_DEADZONE)
        {
            return Mathf.Abs(input) < deadzone;
        }

        public static float RemoveDeadzone(float input, float deadzone = INPUT_DEADZONE)
        {
            if (IsInDeadzone(input, deadzone))
            {
                return 0;
            }
            
            var sign = Mathf.Sign(input);
            var abs = Mathf.Abs(input);
            
            var normalized =  (abs - deadzone) / (1f - deadzone);
            return sign * Mathf.Clamp01(normalized);
        }

        public static bool IsInputActive(float input, float deadzone = INPUT_DEADZONE)
        {
            return !IsInDeadzone(input, deadzone);
        }
            
        #endregion

        #region Value Comparison

        public static bool IsApproximately(float a, float b, float threshold = EPSILON)
        {
            return Mathf.Abs(a - b) < threshold;
        }

        public static bool IsApproximatelyZero(float value, float threshold = EPSILON)
        {
            return Mathf.Abs(value) < threshold;
        }

        public static bool IsVelocityZero(float velocity)
        {
            return Mathf.Abs(velocity) < VELOCITY_THRESHOLD;
        }

        public static bool IsVelocityZero(Vector2 velocity)
        {
            return velocity.sqrMagnitude < VELOCITY_THRESHOLD * VELOCITY_THRESHOLD;
        }
        
        public static bool IsVelocityZero(Vector3 velocity, float threshold = VELOCITY_THRESHOLD)
        {
            return velocity.sqrMagnitude < threshold * threshold;
        }

        #endregion

        #region Movement

        public static bool IsDirectionChanged(float currentDirection, float newDirection)
        {
            if (IsInDeadzone(currentDirection) && IsInDeadzone(newDirection))
            {
                return false;
            }

            if (IsInDeadzone(currentDirection) || IsInDeadzone(newDirection))
            {
                return true;
            }

            return (currentDirection * newDirection) < 0;
        }

        #endregion

        #region Lerp

        public static bool IsCloseToTarget(float current, float target, float threshold = EPSILON)
        {
            return Mathf.Abs(current - target) < threshold;
        }

        #endregion
        
        #region Range & Clamping

        public static bool IsInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static int ToSignWithDeadzone(float value, float deadzone = INPUT_DEADZONE)
        {
            if (IsInDeadzone(value, deadzone))
            {
                return 0;
            }

            return (int)Mathf.Sign(value);
        }

        #endregion
    }
}