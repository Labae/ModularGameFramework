using MarioGame.Core;
using MarioGame.Core.Entities;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Camera.Interfaces;
using MarioGame.Gameplay.Camera.Strategies;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
        public class CameraController : CoreBehaviour, ICameraShakeHandler
    {
        [Header("Target Following")] 
        [SerializeField] private Entity _target;
        [SerializeField] private Vector3 _followOffset = new Vector3(0f, 2f, -10.0f);

        [Header("Movement Settings")] 
        [SerializeField] private FollowMode _followMode = FollowMode.SmoothDamp;
        [SerializeField] private float _followSpeed = 5.0f;
        [SerializeField] private float _smoothTime = 0.3f;
        [SerializeField] private float _maxSpeed = 20.0f;

        [Header("Deadzone (Look Ahead)")] 
        [SerializeField] private bool _useDeadzone = true;
        [SerializeField] private Vector2 _deadzoneSize = new Vector2(2f, 1f);
        [SerializeField] private float _lookAheadDistance = 3f;
        [SerializeField] private float _lookAheadSpeed = 2f;

        [Header("Prediction")] 
        [SerializeField] private bool _usePrediction = true;
        [SerializeField] private float _predictionTime = 0.5f;
        [SerializeField] private float _predictionWeight = 0.3f;
        
        [Header("Camera Shake")]
        [SerializeField] private bool _enableShake = true;
        [SerializeField] private int _maxShakeCount = 3;
        
        [Header("Debug")] 
        [SerializeField] private bool _drawGizmos = true;

        private UnityEngine.Camera _camera;
        private Vector3 _currentVelocity;
        private Vector3 _currentLookAhead;
        private Vector3 _targetVelocity;
        private Vector3 _lastTargetPosition;
        private ICameraFollowStrategy _followStrategy;
        private Vector3 _smoothPosition;
        private float _pixelSize;
        private Vector3 _shakeOffset;
        private Vector3 _totalOffset;

        private CameraEventHandler _eventHandler;

        public bool IsFollowingTarget => _target != null;
        public float FollowSpeed => _followSpeed;
        public float SmoothTime => _smoothTime;
        public float MaxSpeed => _maxSpeed;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _camera = GetComponent<UnityEngine.Camera>();
            _assertManager.AssertIsNotNull(_camera, "Camera required");
        }

        private void Start()
        {
            InitializeFollowStrategy();
            InitializeEventHandler();
            SnapToTarget();
        }

        private void InitializeEventHandler()
        {
            _eventHandler = new CameraEventHandler(this, _debugLogger, _enableShake, _maxShakeCount);
        }

        private void Update()
        {
            _eventHandler?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _eventHandler?.Dispose();
        }

        private void InitializeFollowStrategy()
        {
            _followStrategy = _followMode switch
            {
                FollowMode.Lerp => new LerpFollowStrategy(),
                FollowMode.SmoothDamp => new SmoothDampFollowStrategy(),
                FollowMode.Slerp => new SlerpFollowStrategy(),
                _ => new SmoothDampFollowStrategy(),
            };
        }

        private void FixedUpdate()
        {
            if (!IsFollowingTarget) return;

            UpdateTargetVelocity();
            UpdateLookAhead();
            UpdateCameraPosition();
        }

        private void SnapToTarget()
        {
            if (!IsFollowingTarget) return;

            var desiredPosition = CalculateDesiredPosition();
            transform.position = desiredPosition;
            _lastTargetPosition = desiredPosition;
            _smoothPosition = desiredPosition;
            
            _currentVelocity = Vector3.zero;
            _currentLookAhead = Vector3.zero;

            _debugLogger?.Camera("Camera snapped to target");
        }

        public void SetTarget(Entity newTarget)
        {
            if (_target == newTarget) return;

            _target = newTarget;
            if (_target != null)
            {
                _lastTargetPosition = _target.position3D;
                _targetVelocity = Vector3.zero;
                _debugLogger?.Camera($"Camera target set to: {_target.name}");
                SnapToTarget();
            }
            else
            {
                _debugLogger?.Camera("Camera target cleared");
            }
        }

        private void SetFollowMode(FollowMode followMode)
        {
            if (_followMode == followMode) return;

            _followMode = followMode;
            InitializeFollowStrategy();
            _debugLogger?.Camera($"Follow mode set to: {_followMode}");
        }

        public void SetShakeOffset(Vector2 shakeOffset)
        {
            _shakeOffset = shakeOffset;
            UpdateTotalOffset();
        }

        private void UpdateTotalOffset()
        {
            _totalOffset = _shakeOffset;
        }

        private Vector3 CalculateDesiredPosition()
        {
            if (!IsFollowingTarget) return position3D;

            var basePosition = (_target.position3D + _followOffset);
            if (_useDeadzone)
            {
                basePosition += _currentLookAhead;
            }

            if (_usePrediction && !FloatUtility.IsVelocityZero(_targetVelocity, 0.001f))
            {
                var prediction = _targetVelocity * (_predictionTime * _predictionWeight);
                basePosition += prediction;
            }

            return basePosition;
        }

        private void UpdateTargetVelocity()
        {
            if (!IsFollowingTarget) return;

            _targetVelocity = (_target.position3D - _lastTargetPosition) / Time.fixedDeltaTime;
            _lastTargetPosition = _target.position3D;
        }

        private void UpdateLookAhead()
        {
            if (!_useDeadzone || !IsFollowingTarget) return;

            var targetLookAhead = Vector3.zero;
            if (!FloatUtility.IsVelocityZero(_targetVelocity, 0.001f))
            {
                var direction = _targetVelocity.normalized;
                targetLookAhead = direction * _lookAheadSpeed;
            }

            _currentLookAhead = Vector3.Lerp(_currentLookAhead, targetLookAhead, _lookAheadSpeed * Time.fixedDeltaTime);
        }

        private void UpdateCameraPosition()
        {
            var desiredPosition = CalculateDesiredPosition();
            _smoothPosition = _followStrategy.CalculatePosition(position3D, desiredPosition, 
                ref _currentVelocity, Time.fixedDeltaTime, this);

            transform.position = _smoothPosition + _totalOffset;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _target == null) return;

            // Draw deadzone
            if (_useDeadzone)
            {
                Gizmos.color = Color.yellow;
                var deadzoneCenter = _target.position3D + _followOffset;
                Gizmos.DrawWireCube(deadzoneCenter, _deadzoneSize);

                // Draw look ahead
                if (_currentLookAhead.magnitude > 0.1f)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(deadzoneCenter, deadzoneCenter + _currentLookAhead);
                    Gizmos.DrawWireSphere(deadzoneCenter + _currentLookAhead, 0.5f);
                }
            }

            // Draw prediction
            if (_usePrediction && Application.isPlaying && _targetVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.magenta;
                var predictionPos = _target.position3D + _targetVelocity * _predictionTime * _predictionWeight;
                Gizmos.DrawLine(_target.position3D, predictionPos);
                Gizmos.DrawWireSphere(predictionPos, 0.3f);
            }

            // Draw target and camera positions
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_target.position3D, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        private void OnValidate()
        {
            _followSpeed = Mathf.Max(0.1f, _followSpeed);
            _smoothTime = Mathf.Max(0.01f, _smoothTime);
            _maxSpeed = Mathf.Max(1f, _maxSpeed);
            _predictionTime = Mathf.Max(0f, _predictionTime);
            _predictionWeight = Mathf.Clamp01(_predictionWeight);
            _lookAheadDistance = Mathf.Max(0f, _lookAheadDistance);
            _lookAheadSpeed = Mathf.Max(0.1f, _lookAheadSpeed);
        }
#endif
    }
}