using System;
using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Detection;
using MarioGame.Level.LevelObjects.Ladders;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Detections
{
    /// <summary>
    /// 사다리 감지를 담당하는 컴포넌트
    /// 플레이어가 사다리 근처에 있는지, 사다리 위/아래 끝에 있는지 감지
    /// </summary>
    public class LadderChecker : CoreBehaviour, ILadderChecker
    {
        [Header("Ladder Detection Config")] [SerializeField]
        private LadderDetectionConfig _config;

        [Header("Debug")] [SerializeField] private bool _drawGizmos = true;

        // DI로 주입받는 서비스
        private ILadderDetector _ladderDetector;

        // 컴포넌트 캐시
        private Collider2D _collider2D;

        // 상태 관리
        private bool _isOnLadder;
        private bool _wasOnLadderLastFrame;
        private bool _isAtLadderTop;
        private bool _isAtLadderBottom;
        private LadderDetectionResult _lastResult;

        // 이벤트들
        public event Action OnLadderEnter;
        public event Action OnLadderExit;
        public event Action OnLadderTopReached;
        public event Action OnLadderBottomReached;

        // Public Properties
        public bool IsOnLadder => _isOnLadder;
        public bool IsAtLadderTop => _isAtLadderTop;
        public bool IsAtLadderBottom => _isAtLadderBottom;
        public Ladder CurrentLadder => _lastResult?.CurrentLadder;

        protected override void Awake()
        {
            base.Awake();
            CreateLadderDetector();
        }

        private void CreateLadderDetector()
        {
            // 서비스 생성 (생성자 주입)
            _ladderDetector = new LadderDetector(_debugLogger);
        }

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _collider2D ??= GetComponentInChildren<Collider2D>();
            _assertManager.AssertIsNotNull(_collider2D, "Collider2D component required");
        }

        private void FixedUpdate()
        {
            PerformLadderCheck();
            HandleLadderEvents();
        }

        private void PerformLadderCheck()
        {
            if (_collider2D == null || _ladderDetector == null)
            {
                return;
            }

            _config.Validate(); // 설정값 검증

            _lastResult = _ladderDetector.DetectLadder(transform, _collider2D, _config);

            _wasOnLadderLastFrame = _isOnLadder;
            _isOnLadder = _lastResult.IsOnLadder;
            _isAtLadderTop = _lastResult.IsAtLadderTop;
            _isAtLadderBottom = _lastResult.IsAtLadderBottom;
        }

        private void HandleLadderEvents()
        {
            // 사다리 진입/퇴장 이벤트
            if (_isOnLadder && !_wasOnLadderLastFrame)
            {
                OnLadderEnter?.Invoke();
            }
            else if (!_isOnLadder && _wasOnLadderLastFrame)
            {
                OnLadderExit?.Invoke();
            }

            // 사다리 끝점 도달 이벤트 (매 프레임 체크)
            if (_isOnLadder)
            {
                if (_isAtLadderTop)
                {
                    OnLadderTopReached?.Invoke();
                }

                if (_isAtLadderBottom)
                {
                    OnLadderBottomReached?.Invoke();
                }
            }
        }

        // 편의 메서드들
        public bool TryGetLadderAlignPosition(out float alignX)
        {
            alignX = 0f;
            return _lastResult?.TryGetLadderAlignPosition(out alignX) ?? false;
        }

        public bool TryGetLadderYLimits(out float minY, out float maxY)
        {
            minY = 0f;
            maxY = 0f;
            return _lastResult?.TryGetLadderYLimits(out minY, out maxY) ?? false;
        }

        // Public 인터페이스 - 외부에서 서비스에 직접 접근 가능
        public ILadderDetector LadderDetector => _ladderDetector;
        public LadderDetectionConfig Config => _config;
        public LadderDetectionResult LastResult => _lastResult;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _collider2D == null || _ladderDetector == null)
            {
                return;
            }

            var rayPositions = _ladderDetector.CalculateRayPositions(transform, _collider2D, _config);

            // 사다리 감지 Ray 그리기
            for (int i = 0; i < rayPositions.Length; i++)
            {
                var rayStart = rayPositions[i];

                // 오른쪽 Ray
                var rayEndRight = rayStart.AddX(_config.LadderCheckDistance);
                Gizmos.color = _isOnLadder ? Color.green : Color.red;
                Gizmos.DrawLine(rayStart.ToVector3(), rayEndRight.ToVector3());

                // 왼쪽 Ray
                var rayEndLeft = rayStart.AddX(-_config.LadderCheckDistance);
                Gizmos.DrawLine(rayStart.ToVector3(), rayEndLeft.ToVector3());

                Gizmos.DrawWireSphere(rayStart.ToVector3(), 0.02f);
            }

            // 현재 사다리 정보 표시
            if (_isOnLadder && CurrentLadder != null)
            {
                var center = transform.position.ToVector2() + _collider2D.offset;
                var checkHeight = _collider2D.bounds.size.y * 0.8f;

                // 사다리 중앙 X축 라인
                Gizmos.color = Color.cyan;
                var ladderX = CurrentLadder.CenterX;
                var ladderLineStart = new Vector3(ladderX, center.y - checkHeight * 0.5f, 0);
                var ladderLineEnd = new Vector3(ladderX, center.y + checkHeight * 0.5f, 0);
                Gizmos.DrawLine(ladderLineStart, ladderLineEnd);

                // 사다리 끝점 표시
                if (_isAtLadderTop)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(new Vector3(ladderX, CurrentLadder.TopPoint.y, 0), 0.1f);
                }

                if (_isAtLadderBottom)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(new Vector3(ladderX, CurrentLadder.BottomPoint.y, 0), 0.1f);
                }
            }
        }

        private void OnValidate()
        {
            _config.Validate();
        }
#endif
    }
}