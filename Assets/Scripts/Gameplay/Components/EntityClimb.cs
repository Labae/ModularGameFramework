using System;
using MarioGame.Core;
using MarioGame.Core.Extensions;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Config.Movement;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class EntityClimb : CoreBehaviour
    {
        private ClimbMovementConfig _climbConfig;

        private Rigidbody2D _rigidbody2D;
        [SerializeField]
        private LadderChecker _ladderChecker;

        protected bool _isClimbing;
        protected float _climbVelocity;

        protected float _originalGravityScale;
        protected float _originalDrag;

        public bool IsClimbing => _isClimbing;
        public float ClimbVelocity => _climbVelocity;

        public bool CanStartClimbing => !_isClimbing && _ladderChecker != null && _ladderChecker.IsOnLadder
                                        && _climbConfig != null;

        public void Initialize(ClimbMovementConfig config)
        {
            _climbConfig = config;
            
            AssertIsNotNull(_climbConfig, "ClimbConfig required");
            
            _originalGravityScale = _rigidbody2D.gravityScale;
            _originalDrag = _rigidbody2D.drag;
        }
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _ladderChecker = GetComponentInChildren<LadderChecker>();
            
            AssertIsNotNull(_rigidbody2D, "rigidbody2D required");
            AssertIsNotNull(_ladderChecker, "_ladderChecker required");
        }

        public virtual void StartClimbing()
        {
            if (_isClimbing || _climbConfig == null)
            {
                return;
            }

            _isClimbing = true;
            _climbVelocity = 0.0f;

            if (_ladderChecker.TryGetLadderAlignPosition(out float ladderCenterX))
            {
                transform.position = position2D.WithX(ladderCenterX);
            }
            
            _rigidbody2D.gravityScale = 0.0f;
            _rigidbody2D.drag = _climbConfig.ClimbDrag;
            _rigidbody2D.velocity = Vector2.zero;

            OnClimbingStarted();
        }

        public virtual void StopClimbing()
        {
            if (!_isClimbing || _climbConfig == null)
            {
                return;
            }

            _isClimbing = false;
            _climbVelocity = 0.0f;
            
            _rigidbody2D.gravityScale = _originalGravityScale;
            _rigidbody2D.drag = _originalDrag;
            
            OnClimbingStopped();
        }

        public virtual void ClimbUp(float multiplier = 1.0f)
        {
            if (!_isClimbing || _climbConfig == null)
            {
                return;
            }
            
            var climbSpeed = _climbConfig.ClimbSpeed * multiplier;
            _climbVelocity = climbSpeed;
            ApplyClimbMovement(climbSpeed);
        }
        
        public virtual void ClimbDown(float multiplier = 1.0f)
        {
            if (!_isClimbing || _climbConfig == null)
            {
                return;
            }
            
            var climbSpeed = _climbConfig.ClimbSpeed * multiplier;
            _climbVelocity = -climbSpeed;
            ApplyClimbMovement(-climbSpeed);
        }

        private void ApplyClimbMovement(float climbSpeed)
        {
            if (_ladderChecker.TryGetLadderAlignPosition(out float ladderCenterX))
            {
                transform.position = position2D.WithX(ladderCenterX);
            }

            if (_ladderChecker.TryGetLadderYLimits(out float minY, out float maxY))
            {
                var targetY = position2D.y + (climbSpeed * Time.fixedDeltaTime);
                if (climbSpeed > FloatUtility.VELOCITY_THRESHOLD)
                {
                    targetY = Mathf.Min(targetY, maxY);
                }
                else if (climbSpeed < -FloatUtility.VELOCITY_THRESHOLD)
                {
                    targetY = Mathf.Max(targetY, minY);
                }
                
                var newVelocity = Vector2.up * (targetY - position2D.y) / Time.fixedDeltaTime;
                _rigidbody2D.velocity = newVelocity;
            }
            else
            {
                _rigidbody2D.velocity = Vector2.up * climbSpeed;
            }
        }

        public virtual void JumpFromLadder(float jumpForceMultiplier = 1.0f)
        {
            if (!_isClimbing || _climbConfig == null)
            {
                return;
            }
            
            StopClimbing();
            
            var jumpForce = _climbConfig.JumpFromLadderForce * jumpForceMultiplier;
            _rigidbody2D.velocity = _rigidbody2D.velocity.WithY(jumpForce);

            OnJumpedFromLadder(jumpForceMultiplier);
        }

        public virtual void ForceStopClimbing()
        {
            if (!_isClimbing || _climbConfig == null)
            {
                return;
            }
            
            StopClimbing();
            _rigidbody2D.velocity = Vector2.zero;
        }

        protected virtual void OnClimbingStarted()
        {
            
        }
        
        protected virtual void OnClimbingStopped()
        {
            
        }
        
        protected virtual void OnJumpedFromLadder(float jumpForceMultiplier = 1.0f)
        {
            
        }
    }
}