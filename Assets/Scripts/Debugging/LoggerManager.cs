using System;
using UnityEngine;

namespace Debugging
{
    public class LoggerManager : MonoBehaviour
    {
        [SerializeField]
        private bool _enableStateMachineLogs = true;
        [SerializeField]
        private bool _enableAnimatorLogs = true;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                _enableStateMachineLogs = !_enableStateMachineLogs;
                _enableAnimatorLogs = !_enableAnimatorLogs;
            }
            
            Logger.EnableStateMachineLogs = _enableStateMachineLogs;
            Logger.EnableAnimatorLogs = _enableStateMachineLogs;
        }
    }
}