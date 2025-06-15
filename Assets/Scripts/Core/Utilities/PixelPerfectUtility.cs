using UnityEngine;

namespace MarioGame.Core.Utilities
{
    public static class PixelPerfectUtility
    {
        public static Vector3 SnapToPixelGrid(Vector2 position, float pixelsPerUnit = 16f)
        {
            return new Vector3(
                Mathf.Round(position.x * pixelsPerUnit) / pixelsPerUnit,
                Mathf.Round(position.y * pixelsPerUnit) / pixelsPerUnit,
                0.0f
            );
        }
    }
}