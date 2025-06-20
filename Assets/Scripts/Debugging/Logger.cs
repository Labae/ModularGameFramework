using UnityEngine;

namespace Debugging
{
    public static class Logger
    {
        public static bool EnableStateMachineLogs = true;
        public static bool EnableAnimatorLogs = true;

        public static void Warning(string message, Object context = null)
        {
            Debug.LogWarning($"[WARNING] {message}", context);
        }
        
        public static void Error(string message, Object context = null)
        {
            Debug.LogError($"[ERROR] {message}", context);
        }

        public static void StateMachine(string message, Object context = null)
        {
#if UNITY_EDITOR
            if (EnableStateMachineLogs)
            {
                Debug.Log($"<color=green>[STATE MACHINE]</color> {message}", context);
            }
#endif
        }
        
        public static void Animator(string message, Object context = null)
        {
#if UNITY_EDITOR
            if (EnableAnimatorLogs)
            {
                Debug.Log($"<color=yellow>[ANIMATOR]</color> {message}", context);
            }
#endif
        }
    }
}
