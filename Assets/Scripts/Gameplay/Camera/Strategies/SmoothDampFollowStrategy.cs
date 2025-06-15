using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Camera.Strategies
{
    public class SmoothDampFollowStrategy : ICameraFollowStrategy
    {
        public Vector3 CalculatePosition(Vector3 currentPosition, Vector3 targetPosition,
            ref Vector3 velocity, float deltaTime,
            CameraController cameraController)
        {
            return Vector3.SmoothDamp(currentPosition, targetPosition, 
                ref velocity,
                cameraController.SmoothTime, cameraController.MaxSpeed, deltaTime);
        }
    }
}