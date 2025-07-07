using Reflex.Attributes;
using UnityEngine;

namespace MarioGame.Debugging.Core
{
    public class LogManager : MonoBehaviour
    {
        [SerializeField] private bool _enableSystemLogs = true;
        [SerializeField] private bool _enablePlayerLogs = true;
        [SerializeField] private bool _enableEntityLogs = true;
        [SerializeField] private bool _enableStateMachineLogs = true;
        [SerializeField] private bool _enableAnimatorLogs = true;
        [SerializeField] private bool _enableAudioLogs = true;
        [SerializeField] private bool _enableCameraLogs = true;
        [SerializeField] private bool _enableEffectsLogs = true;
        [SerializeField] private bool _enableProjectileLogs = true;
        
        [Inject]
        private Interfaces.IDebugLogger _debugLogger;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                _enableSystemLogs = !_enableSystemLogs;
                _enablePlayerLogs = !_enablePlayerLogs;
                _enableEntityLogs = !_enableEntityLogs;
                _enableStateMachineLogs = !_enableStateMachineLogs;
                _enableAnimatorLogs = !_enableAnimatorLogs;
                _enableAudioLogs = !_enableAudioLogs;
                _enableCameraLogs = !_enableCameraLogs;
                _enableEffectsLogs = !_enableEffectsLogs;
                _enableProjectileLogs = !_enableProjectileLogs;
            }
            
            _debugLogger.EnableSystemLogs = _enableSystemLogs;
            _debugLogger.EnablePlayerLogs = _enablePlayerLogs;
            _debugLogger.EnableEntityLogs = _enableEntityLogs;
            _debugLogger.EnableStateMachineLogs = _enableStateMachineLogs;
            _debugLogger.EnableAnimatorLogs = _enableAnimatorLogs;
            _debugLogger.EnableAudioLogs = _enableAudioLogs;
            _debugLogger.EnableCameraLogs = _enableCameraLogs;
            _debugLogger.EnableEffectLogs = _enableEffectsLogs;
            _debugLogger.EnableProjectileLogs = _enableProjectileLogs;
        }
    }
}