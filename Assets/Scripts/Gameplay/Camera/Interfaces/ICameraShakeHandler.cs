using UnityEngine;

namespace MarioGame.Gameplay.Camera.Interfaces
{
    public interface ICameraShakeHandler
    {
        void SetShakeOffset(Vector2 shakeOffset);
    }
}