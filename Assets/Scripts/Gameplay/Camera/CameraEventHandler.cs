using System;
using System.Collections.Generic;
using EventBus.Core;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Camera.Events;
using MarioGame.Gameplay.Camera.Interfaces;
using MarioGame.Gameplay.Config.Data;
using UnityEngine;

namespace MarioGame.Gameplay.Camera
{
    public class CameraEventHandler : IDisposable
    {
        [Header("Camera Shake Settings")]
        private readonly bool _enableShake;
        private readonly int _maxShakeCount;

        private readonly List<ShakeInstance> _activeShakes = new();
        private readonly Queue<ShakeInstance> _shakePool = new();

        // DI를 통해 받는 의존성들
        private readonly ICameraShakeHandler _cameraShakeHandler;
        private readonly IDebugLogger _logger;
        
        private readonly EventBinding<CameraEvent> _cameraEventBinding;

        // 생성자를 통한 의존성 주입
        public CameraEventHandler(ICameraShakeHandler cameraShakeHandler, IDebugLogger logger, bool enableShake = true, int maxShakeCount = 3)
        {
            _cameraShakeHandler = cameraShakeHandler;
            _logger = logger;
            _enableShake = enableShake;
            _maxShakeCount = maxShakeCount;
            
            // EventBus 등록
            _cameraEventBinding = new EventBinding<CameraEvent>(HandleCameraEvent);
            EventBus<CameraEvent>.Register(_cameraEventBinding);
        }

        public void Update(float deltaTime)
        {
            HandleShakeLifecycle(deltaTime);
            var currentShakeOffset = CalculateCurrentShakeOffset();
            _cameraShakeHandler?.SetShakeOffset(currentShakeOffset);
        }

        public void Dispose()
        {
            EventBus<CameraEvent>.Deregister(_cameraEventBinding);
        }

        private void HandleShakeLifecycle(float deltaTime)
        {
            for (int i = _activeShakes.Count - 1; i >= 0; i--)
            {
                _activeShakes[i].CurrentTime += deltaTime;
                if (_activeShakes[i].IsComplete)
                {
                    ReturnShakeToPool(_activeShakes[i]);
                    _activeShakes.RemoveAt(i);
                }
            }
        }

        // EventBus를 통한 이벤트 처리
        private void HandleCameraEvent(CameraEvent evt)
        {
            switch (evt.Type)
            {
                case CameraEventType.SetTarget:
                    // CameraController가 직접 처리하도록 이벤트 전달
                    if (_cameraShakeHandler is CameraController cameraController)
                    {
                        cameraController.SetTarget(evt.Target);
                    }
                    break;
                case CameraEventType.Shake:
                    StartShake(evt.ShakeData, evt.Priority);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StartShake(ShakeData shakeData, int priority = 0)
        {
            if (!_enableShake || shakeData == null) return;

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

            _logger?.Camera($"Camera shake started with priority {priority}");
        }

        public void StopAllShakes()
        {
            foreach (var shakeInstance in _activeShakes)
            {
                ReturnShakeToPool(shakeInstance);
            }

            _activeShakes.Clear();
            _cameraShakeHandler?.SetShakeOffset(Vector3.zero);
            _logger?.Camera("All camera shakes stopped");
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