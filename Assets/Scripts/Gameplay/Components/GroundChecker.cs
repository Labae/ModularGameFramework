using System;
using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Level.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    /// <summary>
    /// 땅 감지를 담당하는 컴포넌트
    /// 여러 개의 Raycast를 사용하여 정확한 땅 감지 제공
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GroundChecker : CoreBehaviour
    {
        [SerializeField, Min(0.0f)] private float _groundCheckDistance = 0.1f;
        [SerializeField, Min(3)] private int _groundCheckRayCount = 5;
        [SerializeField] private float _groundCheckYOffset = 0.0f;

        [SerializeField, Min(0.1f)]
        private float _groundCheckWidth = 0.8f;

        [SerializeField] private LayerMask _groundLayerMask;

        [Header("Bypass System")] [SerializeField]
        private bool _enableBypasss = true;
        private IBypassable _currentBypassable;

        [Header("Debug")] [SerializeField] private bool _drawGizmos = true;
        private RaycastHit2D[] _groundCheckHits;
        
        private bool _isGrounded;
        private bool _wasGroundedLastFrame;
        
        private Collider2D _collider2D;

        public event Action OnGroundEnter;
        public event Action OnGroundExit;

        public bool IsGrounded => _isGrounded;
        public bool CanBypass => _enableBypasss && _currentBypassable != null;
        public bool HasBypassableBelow => _currentBypassable != null;
        public IBypassable CurrentBypassable => _currentBypassable;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _collider2D = GetComponent<Collider2D>();
            AssertIsNotNull(_collider2D, "Collider2D component required");

            _groundCheckHits = new RaycastHit2D[_groundCheckRayCount+1];
        }

        private void FixedUpdate()
        {
            PerformGroundCheck();
            HandleGroundEvents();
        }

        private void PerformGroundCheck()
        {
            if (_collider2D == null)
            {
                return;
            }

            var center = transform.position.ToVector2() + _collider2D.offset;
            var bottom = center.AddY(-_collider2D.bounds.size.y * 0.5f + _groundCheckYOffset);

            var checkWidth = _collider2D.bounds.size.x * _groundCheckWidth;
            var startX = bottom.x - checkWidth * 0.5f;
            var endX = bottom.x + checkWidth * 0.5f;

            _currentBypassable = null;
            bool foundGround = false;

            for (var i = 0; i <= _groundCheckRayCount; i++)
            {
                var t = (float)i / _groundCheckRayCount;
                var rayX = Mathf.Lerp(startX, endX, t);
                var rayStart = new Vector2(rayX, bottom.y);

                var hitSize = Physics2D.RaycastNonAlloc(rayStart, Vector3.down
                    , _groundCheckHits, _groundCheckDistance, _groundLayerMask);
                
                if (hitSize > 0)
                {
                    foundGround = true;

                    FindBypass(hitSize);
                    break;
                }
            }

            _wasGroundedLastFrame = _isGrounded;
            _isGrounded = foundGround;
        }

        private void FindBypass(int hitSize)
        {
            if (!_enableBypasss || _currentBypassable != null) return;
           
            for (var i = 0; i < hitSize; i++)
            {
                var bypass = _groundCheckHits[i].collider.GetComponent<IBypassable>();
                if (bypass == null)
                {
                    continue;
                }
                
                _currentBypassable = bypass;
                break;
            }
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _collider2D == null)
            {
                return;
            }
            
            var center = transform.position.ToVector2() + _collider2D.offset;
            var bottom = center.AddY(-_collider2D.bounds.size.y * 0.5f + _groundCheckYOffset);
            
            var checkWidth = _collider2D.bounds.size.x * _groundCheckWidth;
            var startX = bottom.x - checkWidth * 0.5f;
            var endX = bottom.x + checkWidth * 0.5f;
            
            for (var i = 0; i <= _groundCheckRayCount; i++)
            {
                var t = (float)i / _groundCheckRayCount;
                var rayX = Mathf.Lerp(startX, endX, t);
                var rayStart = new Vector2(rayX, bottom.y);
                var rayEnd = rayStart.AddY(-_groundCheckDistance);
                bool hasHit = Application.isPlaying && _groundCheckHits != null
                    && i < _groundCheckHits.Length && _groundCheckHits[i].collider != null;
                
                Gizmos.color = hasHit ? Color.green : (_isGrounded ? Color.yellow : Color.red);
                Gizmos.DrawLine(rayStart.ToVector3(), rayEnd.ToVector3());
                
                Gizmos.DrawWireSphere(rayStart.ToVector3(), 0.02f);
            }

            Gizmos.color = Color.cyan;
            var rangeStart = new Vector2(startX, bottom.y);
            var rangeEnd = new Vector2(endX, bottom.y);
            Gizmos.DrawLine(rangeStart.ToVector3(), rangeEnd.ToVector3());
        }

        private void OnValidate()
        {
            _groundCheckDistance = Mathf.Max(0.01f, _groundCheckDistance);
            _groundCheckRayCount = Mathf.Max(3, _groundCheckRayCount);
            _groundCheckWidth = Mathf.Clamp(_groundCheckWidth, 0.1f, 2);
        }
#endif
    }
}