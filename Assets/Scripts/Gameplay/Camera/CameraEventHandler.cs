using System;
using System.Collections.Generic;
using EventBus.Core;
using MarioGame.Core;
using MarioGame.Gameplay.Camera.Events;
using MarioGame.Gameplay.Config.Data;
using UnityEngine;

namespace MarioGame.Gameplay.Camera
{
    [RequireComponent(typeof(CameraController))]
    [DisallowMultipleComponent]
    public class CameraEventHandler : CoreBehaviour
    {
        [Header("Camera Shake")] [SerializeField]
        private bool _enableShake = true;

        private int _maxShakeCount = 3;

        private readonly List<ShakeInstance> _activeShakes = new();
        private readonly Queue<ShakeInstance> _shakePool = new();
        private Vector3 _basePosition;

        private CameraController _cameraController;

        private EventBinding<CameraEvent> _cameraEventBinding;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _cameraController = GetComponent<CameraController>();
            _cameraEventBinding = new EventBinding<CameraEvent>(HandleCameraEvent);
            EventBus<CameraEvent>.Register(_cameraEventBinding);
            
            AssertIsNotNull(_cameraController, "Camera required");
        }

        private void OnDestroy()
        {
            EventBus<CameraEvent>.Deregister(_cameraEventBinding);
        }

        private void Update()
        {
            HandleShakeLifecycle();
            var currentShakeOffset = CalculateCurrentShakeOffset();
            _cameraController.SetShakeOffset(currentShakeOffset);
        }

        private void HandleShakeLifecycle()
        {
            for (int i = _activeShakes.Count - 1; i >= 0; i--)
            {
                _activeShakes[i].CurrentTime += Time.deltaTime;
                if (_activeShakes[i].IsComplete)
                {
                    ReturnShakeToPool(_activeShakes[i]);
                    _activeShakes.RemoveAt(i);
                }
            }
        }

        private void HandleCameraEvent(CameraEvent evt)
        {
            switch (evt.Type)
            {
                case CameraEventType.SetTarget:
                    _cameraController.SetTarget(evt.Target);
                    break;
                case CameraEventType.Shake:
                    StartShake(evt.ShakeData, evt.Priority);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartShake(ShakeData shakeData, int priority = 0)
        {
            if (!_enableShake || shakeData == null)
            {
                return;
            }

            var shakeInstance = GetShakeInstance();
            shakeInstance.Data = shakeData;
            shakeInstance.Priority = priority;
            shakeInstance.StartTime = Time.time;
            shakeInstance.CurrentTime = 0f;
            shakeInstance.IsActive = true;

            _activeShakes.Add(shakeInstance);
            _activeShakes.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            while (_activeShakes.Count > _maxShakeCount)
            {
                var lowest = _activeShakes[^1];
                _activeShakes.RemoveAt(_activeShakes.Count - 1);
                ReturnShakeToPool(lowest);
            }
        }

        public void StopAllShakes()
        {
            foreach (var shakeInstance in _activeShakes)
            {
                ReturnShakeToPool(shakeInstance);
            }

            _activeShakes.Clear();
            _cameraController.SetShakeOffset(Vector3.zero);
        }

        private ShakeInstance GetShakeInstance()
        {
            if (_shakePool.Count > 0)
            {
                return _shakePool.Dequeue();
            }

            return new ShakeInstance();
        }

        private void ReturnShakeToPool(ShakeInstance shakeInstance)
        {
            shakeInstance.IsActive = false;
            shakeInstance.Data = null;
            _shakePool.Enqueue(shakeInstance);
        }

        private Vector3 CalculateCurrentShakeOffset()
        {
            var offset = Vector3.zero;
            foreach (var shakeInstance in _activeShakes)
            {
                offset += shakeInstance.GetCurrentOffset();
            }

            return offset;
        }
    }
}