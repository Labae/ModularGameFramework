using System;
using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Detection;
using MarioGame.Level.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Detections
{
    /// <summary>
    /// 땅 감지를 담당하는 컴포넌트
    /// 여러 개의 Raycast를 사용하여 정확한 땅 감지 제공
    /// </summary>
    public class GroundChecker : CoreBehaviour, IGroundChecker
    {
        [Header("Ground Detection Config")] [SerializeField]
        private GroundDetectionConfig _config;

        [Header("Debug")] [SerializeField] private bool _drawGizmos = true;

        // DI로 주입받는 서비스
        private IGroundDetector _groundDetector;

        // 컴포넌트 캐시
        private Collider2D _collider2D;

        // 상태 관리
        private bool _isGrounded;
        private bool _wasGroundedLastFrame;
        private GroundDetectionResult _lastResult;

        // 이벤트들
        public event Action OnGroundEnter;
        public event Action OnGroundExit;

        // Public Properties
        public bool IsGrounded => _isGrounded;
        public bool CanBypass => _config.EnableBypass && _lastResult?.CurrentBypassable != null;
        public bool HasBypassableBelow => _lastResult?.CurrentBypassable != null;
        public IBypassable CurrentBypassable => _lastResult?.CurrentBypassable;

        protected override void Awake()
        {
            base.Awake();
            CreateGroundDetector();
        }

        private void CreateGroundDetector()
        {
            // 서비스 생성 (생성자 주입)
            _groundDetector = new GroundDetector(_debugLogger);
        }

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _collider2D ??= GetComponentInChildren<Collider2D>();
            _assertManager.AssertIsNotNull(_collider2D, "Collider2D component required");
        }

        private void FixedUpdate()
        {
            PerformGroundCheck();
            HandleGroundEvents();
        }

        private void PerformGroundCheck()
        {
            if (_collider2D == null || _groundDetector == null)
            {
                return;
            }

            _config.Validate(); // 설정값 검증

            _lastResult = _groundDetector.DetectGround(transform, _collider2D, _config);

            _wasGroundedLastFrame = _isGrounded;
            _isGrounded = _lastResult.IsGrounded;
        }

        private void HandleGroundEvents()
        {
            if (_isGrounded && !_wasGroundedLastFrame)
            {
                OnGroundEnter?.Invoke();
            }
            else if (!_isGrounded && _wasGroundedLastFrame)
            {
                OnGroundExit?.Invoke();
            }
        }

        // Public 인터페이스 - 외부에서 서비스에 직접 접근 가능
        public IGroundDetector GroundDetector => _groundDetector;
        public GroundDetectionConfig Config => _config;
        public GroundDetectionResult LastResult => _lastResult;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _collider2D == null || _groundDetector == null)
            {
                return;
            }

            var rayPositions = _groundDetector.CalculateRayPositions(transform, _collider2D, _config);

            for (int i = 0; i < rayPositions.Length; i++)
            {
                var rayStart = rayPositions[i];
                var rayEnd = rayStart.AddY(-_config.GroundCheckDistance);

                // 히트 체크 (플레이 중일 때만)
                bool hasHit = Application.isPlaying && _lastResult?.Hits != null
                                                    && i < _lastResult.Hits.Length &&
                                                    _lastResult.Hits[i].collider != null;

                Gizmos.color = hasHit ? Color.green : (_isGrounded ? Color.yellow : Color.red);
                Gizmos.DrawLine(rayStart.ToVector3(), rayEnd.ToVector3());
                Gizmos.DrawWireSphere(rayStart.ToVector3(), 0.02f);
            }

            // 체크 범위 표시
            if (rayPositions.Length >= 2)
            {
                Gizmos.color = Color.cyan;
                var rangeStart = rayPositions[0];
                var rangeEnd = rayPositions[rayPositions.Length - 1];
                Gizmos.DrawLine(rangeStart.ToVector3(), rangeEnd.ToVector3());
            }
        }

        private void OnValidate()
        {
            _config.Validate();
        }
#endif
    }
}