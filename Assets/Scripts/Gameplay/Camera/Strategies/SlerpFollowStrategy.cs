using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Camera.Strategies
{
    public class SlerpFollowStrategy : ICameraFollowStrategy
    {
        public Vector3 CalculatePosition(Vector3 currentPosition, Vector3 targetPosition,
            ref Vector3 velocity, float deltaTime,
            CameraController cameraController)
        {
            return Vector3.Slerp(currentPosition, targetPosition, cameraController.FollowSpeed * deltaTime);
        }
    }
}