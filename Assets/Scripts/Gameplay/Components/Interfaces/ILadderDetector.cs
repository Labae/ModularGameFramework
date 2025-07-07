using MarioGame.Gameplay.Config.Detection;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface ILadderDetector
    {
        LadderDetectionResult DetectLadder(Transform transform, Collider2D collider, LadderDetectionConfig config);
        Vector2[] CalculateRayPositions(Transform transform, Collider2D collider, LadderDetectionConfig config);
    }
}