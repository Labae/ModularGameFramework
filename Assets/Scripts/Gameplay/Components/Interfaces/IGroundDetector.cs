using MarioGame.Gameplay.Config.Detection;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IGroundDetector
    {
        GroundDetectionResult DetectGround(Transform transform, Collider2D collider, GroundDetectionConfig config);
        Vector2[] CalculateRayPositions(Transform transform, Collider2D collider, GroundDetectionConfig config);
    }
}