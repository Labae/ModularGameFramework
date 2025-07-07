using UnityEngine;

namespace MarioGame.Debugging.Interfaces
{
    public interface IDebugLogger
    {
        bool EnableSystemLogs { get; set; }
        public bool EnableEntityLogs { get; set; }
        public bool EnablePlayerLogs { get; set; }
        bool EnableStateMachineLogs { get; set; }
        bool EnableAnimatorLogs { get; set; }
        bool EnableAudioLogs { get; set; }
        bool EnableCameraLogs { get; set; }
        bool EnableEffectLogs { get; set; }
        public bool EnableProjectileLogs { get; set; }

        void Warning(string message, Object context = null);
        void Error(string message, Object context = null);
        void System(string message, Object context = null);
        void Entity(string message, Object context = null);
        void Player(string message, Object context = null);
        void StateMachine(string message, Object context = null);
        void Animator(string message, Object context = null);
        void Audio(string message, Object context = null);
        void Camera(string message, Object context = null);
        void Effect(string message, Object context = null);
        void Projectile(string message, Object context = null);
    }
}