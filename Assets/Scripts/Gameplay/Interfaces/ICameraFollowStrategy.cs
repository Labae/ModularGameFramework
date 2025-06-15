using MarioGame.Gameplay.Camera;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces
{
    public interface ICameraFollowStrategy
    {
        Vector3 CalculatePosition(Vector3 currentPosition,
            Vector3 targetPosition, ref Vector3 velocity,
            float deltaTime, CameraController cameraController);
    }
}