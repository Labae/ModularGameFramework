using System.Text;
using UnityEngine;

namespace MarioGame.Debugging.Core
{
    public class DebugLogger : Interfaces.IDebugLogger
    {
        public bool EnableSystemLogs { get; set; } = true;
        public bool EnableEntityLogs { get; set; } = true;
        public bool EnablePlayerLogs { get; set; } = true;
        public bool EnableStateMachineLogs { get; set; } = true;
        public bool EnableAnimatorLogs { get; set; } = true;
        public bool EnableAudioLogs { get; set; } = true;
        public bool EnableCameraLogs { get; set; } = true;
        public bool EnableEffectLogs { get; set; } = true;
        public bool EnableProjectileLogs { get; set; } = true;

        public void Warning(string message, Object context = null)
        {
            Debug.LogWarning($"[Warning] {message}", context);
        }

        public void Error(string message, Object context = null)
        {
            Debug.LogWarning($"[Error] {message}", context);
        }
        
        public void System(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=red>[SYSTEM]</color> {message}", context);
#endif
        }
        
        public void Entity(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"[ENTITY] {message}", context);
#endif
        }

        public void Player(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=blue>[PLAYER]</color> {message}", context);
#endif
        }

        public void StateMachine(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=green>[STATE MACHINE]</color> {message}", context);
#endif
        }

        public void Animator(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=yellow>[ANIMATOR]</color> {message}", context);
#endif
        }

        public void Audio(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=cyan>[Audio]</color> {message}", context);
#endif
        }
        
        public void Camera(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=green>[CAMERA]</color> {message}", context);
#endif
        }

        public void Effect(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=pink>[EFFECT]</color> {message}", context);
#endif
        }

        public void Projectile(string message, Object context = null)
        {
#if UNITY_EDITOR
            Debug.Log($"[PROJECTILE] {message}", context);
#endif 
        }
    }
}