using System;
using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Level.LevelObjects;
using MarioGame.Level.LevelObjects.Ladders;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    /// <summary>
    /// 사다리 감지를 담당하는 컴포넌트
    /// 플레이어가 사다리 근처에 있는지, 사다리 위/아래 끝에 있는지 감지
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LadderChecker : CoreBehaviour
    {
        [Header("Ladder Detection")] [SerializeField, Min(0.1f)]
        private float _ladderCheckDistance = 0.2f;

        [SerializeField, Min(3f)] private int _ladderCheckRayCount = 3;
        [SerializeField] private LayerMask _ladderCheckLayerMask;

        [Header("Debug")] [SerializeField] private bool _drawGizmos = true;
        private RaycastHit2D[] _ladderCheckHits;

        private bool _isOnLadder;
        private bool _wasOnLadderLastFrame;
        private bool _isAtLadderTop;
        private bool _isAtLadderBottom;

        private Collider2D _collider2D;
        private Ladder _currentLadder;

        public event Action OnLadderEnter;
        public event Action OnLadderExit;
        public event Action OnLadderTopReached;
        public event Action OnLadderBottomReached;
        public bool IsOnLadder => _isOnLadder;
        public bool IsAtLadderTop => _isAtLadderTop;
        public bool IsAtLadderBottom => _isAtLadderBottom;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _collider2D = GetComponent<Collider2D>();
            AssertIsNotNull(_collider2D, "Collider2D Required");

            _ladderCheckHits = new RaycastHit2D[_ladderCheckRayCount + 1];
        }

        private void FixedUpdate()
        {
            PerformLadderCheck();
            HandleLadderEvents();
        }

        private void PerformLadderCheck()
        {
            if (_collider2D == null)
            {
                return;
            }

            var center = transform.position.ToVector2() + _collider2D.offset;
            var checkHeight = _collider2D.bounds.size.y * 0.8f;
            var startY = center.y - checkHeight * 0.5f;
            var endY = center.y + checkHeight * 0.5f;

            bool foundGround = false;
            Ladder detectedLadder = null;

            for (var i = 0; i <= _ladderCheckRayCount; i++)
            {
                var t = (float)i / _ladderCheckRayCount;
                var rayY = Mathf.Lerp(startY, endY, t);
                var rayStart = new Vector2(center.x, rayY);

                var hit = Physics2D.RaycastNonAlloc(rayStart, Vector3.down
                    , _ladderCheckHits, _ladderCheckDistance, _ladderCheckLayerMask);

                if (hit > 0 && _ladderCheckHits[i].collider.TryGetComponent<Ladder>(out var ladder))
                {
                    foundGround = true;
                    detectedLadder = ladder;
                    break;
                }
            }

            _wasOnLadderLastFrame = _isOnLadder;
            _isOnLadder = foundGround;

            if (_isOnLadder && detectedLadder != null)
            {
                _currentLadder = detectedLadder;

                _isAtLadderTop = _currentLadder.IsEntityAtTop(position2D);
                _isAtLadderBottom = _currentLadder.IsEntityAtBottom(position2D);
            }
            else
            {
                _currentLadder = null;
                _isAtLadderTop = false;
                _isAtLadderBottom = false;
            }
        }

        private void HandleLadderEvents()
        {
            if (_isOnLadder && !_wasOnLadderLastFrame)
            {
                OnLadderEnter?.Invoke();
            }
            else if (!_isOnLadder && _wasOnLadderLastFrame)
            {
                OnLadderExit?.Invoke();
            }

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

        public bool TryGetLadderAlignPosition(out float alignX)
        {
            if (_isOnLadder && _currentLadder != null)
            {
                alignX = _currentLadder.CenterX;
                return true;
            }

            alignX = 0.0f;
            return false;
        }

        public bool TryGetLadderYLimits(out float minY, out float maxY)
        {
            if (_isOnLadder && _currentLadder != null)
            {
                minY = _currentLadder.GetMinClimbY();
                maxY = _currentLadder.GetMaxClimbY();
                return true;
            }

            minY = 0.0f;
            maxY = 0.0f;
            return false;
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _collider2D == null)
            {
                return;
            }

            var center = transform.position.ToVector2() + _collider2D.offset;
            var checkHeight = _collider2D.bounds.size.y * 0.8f;
            var startY = center.y - checkHeight * 0.5f;
            var endY = center.y + checkHeight * 0.5f;

            // 사다리 감지 Ray 그리기
            for (var i = 0; i < _ladderCheckRayCount; i++)
            {
                var t = (float)i / (_ladderCheckRayCount - 1);
                var rayY = Mathf.Lerp(startY, endY, t);
                var rayStart = new Vector2(center.x, rayY);

                // 오른쪽 Ray
                var rayEndRight = rayStart.AddX(_ladderCheckDistance);
                Gizmos.color = _isOnLadder ? Color.green : Color.red;
                Gizmos.DrawLine(rayStart.ToVector3(), rayEndRight.ToVector3());

                // 왼쪽 Ray
                var rayEndLeft = rayStart.AddX(-_ladderCheckDistance);
                Gizmos.DrawLine(rayStart.ToVector3(), rayEndLeft.ToVector3());

                Gizmos.DrawWireSphere(rayStart.ToVector3(), 0.02f);
            }

            // 현재 사다리 정보 표시
            if (_isOnLadder && _currentLadder != null)
            {
                // 사다리 중앙 X축 라인
                Gizmos.color = Color.cyan;
                var ladderX = _currentLadder.CenterX;
                var ladderLineStart = new Vector3(ladderX, center.y - checkHeight * 0.5f, 0);
                var ladderLineEnd = new Vector3(ladderX, center.y + checkHeight * 0.5f, 0);
                Gizmos.DrawLine(ladderLineStart, ladderLineEnd);

                // 사다리 끝점 표시
                if (_isAtLadderTop)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(new Vector3(ladderX, _currentLadder.TopPoint.y, 0), 0.1f);
                }

                if (_isAtLadderBottom)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(new Vector3(ladderX, _currentLadder.BottomPoint.y, 0), 0.1f);
                }
            }
        }


#endif
    }
}