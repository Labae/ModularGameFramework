using UnityEngine;

namespace MarioGame.Core.Extensions
{
    public static class VectorExtensions
    {
        #region Vector2 Extensions

        public static Vector2 WithX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }

        public static Vector2 WithY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, y);
        }

        public static Vector2 AddX(this Vector2 vector, float value)
        {
            return new Vector2(vector.x + value, vector.y);
        }
        
        public static Vector2 AddY(this Vector2 vector, float value)
        {
            return new Vector2(vector.x, vector.y + value);
        }

        public static Vector2 FlipX(this Vector2 vector)
        {
            return new Vector2(-vector.x, vector.y);
        }
        
        public static Vector2 FlipY(this Vector2 vector)
        {
            return new Vector2(vector.x, -vector.y);
        }
        
        public static Vector2 Flip(this Vector2 vector)
        {
            return new Vector2(-vector.x, -vector.y);
        }

        public static bool IsNearZero(this Vector2 vector, float threshold = 0.001f)
        {
            return vector.sqrMagnitude < threshold * threshold;
        }

        public static bool Approximately(this Vector2 vector2, Vector2 other, float threshold = 0.001f)
        {
            return (vector2 - other).sqrMagnitude < threshold * threshold;
        }
        
        public static Vector3 ToVector3(this Vector2 vector2)
        {
            return new Vector3(vector2.x, vector2.y, 0f);
        }
        
        #endregion
        
        #region Vector3 Extensions

        public static Vector2 ToVector2(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }
        
        #endregion
    }
}