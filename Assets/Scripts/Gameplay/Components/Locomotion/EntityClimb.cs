using MarioGame.Core.Extensions;
using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
    public class EntityClimb : IEntityClimb
    {
        protected readonly IDebugLogger _logger;
        protected readonly ILadderChecker _ladderChecker;
        protected ClimbMovementConfig _climbConfig;

        protected EntityMovementLockType _lockType;
        protected bool _isClimbing;
        protected float _climbVelocity;
        protected float _originalGravityScale;
        protected float _originalDrag;

        // Rigidbody 상태 추적용
        protected Vector2 _currentVelocity;

        public bool IsClimbing => _isClimbing;
        public float ClimbVelocity => _climbVelocity;
        public bool CanStartClimbing => !_isClimbing && _ladderChecker?.IsOnLadder == true && _climbConfig != null;
        public bool IsMovementLocked => _lockType != EntityMovementLockType.None;

        public EntityClimb(IDebugLogger logger, ILadderChecker ladderChecker)
        {
            _logger = logger;
            _ladderChecker = ladderChecker;
        }

        public virtual void Initialize(ClimbMovementConfig config)
        {
            _climbConfig = config;
            _logger?.StateMachine($"Climb initialized with speed: {_climbConfig?.ClimbSpeed}");
        }

        public virtual void UpdatePhysics(Rigidbody2D rigidbody)
        {
            if (rigidbody == null) return;

            // 처음 초기화시 원본 값 저장
            if (_originalGravityScale == 0 && _originalDrag == 0)
            {
                _originalGravityScale = rigidbody.gravityScale;
                _originalDrag = rigidbody.drag;
            }

            // 현재 속도 적용
            rigidbody.velocity = _currentVelocity;
        }

        public virtual void StartClimbing()
        {
            if (_isClimbing || _climbConfig == null)
            {
                return;
            }

            _isClimbing = true;
            _climbVelocity = 0.0f;
            _currentVelocity = Vector2.zero;

            _logger?.StateMachine("Started climbing");
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

            _logger?.StateMachine("Stopped climbing");
            OnClimbingStopped();
        }

        public virtual void ClimbUp(float multiplier = 1.0f)
        {
            if (IsMovementLocked || !_isClimbing || _climbConfig == null)
            {
                return;
            }

            var climbSpeed = _climbConfig.ClimbSpeed * multiplier;
            _climbVelocity = climbSpeed;
            ApplyClimbMovement(climbSpeed);
        }

        public virtual void ClimbDown(float multiplier = 1.0f)
        {
            if (IsMovementLocked || !_isClimbing || _climbConfig == null)
            {
                return;
            }

            var climbSpeed = _climbConfig.ClimbSpeed * multiplier;
            _climbVelocity = -climbSpeed;
            ApplyClimbMovement(-climbSpeed);
        }

        protected virtual void ApplyClimbMovement(float climbSpeed)
        {
            // Y축 제한 체크
            if (_ladderChecker.TryGetLadderYLimits(out float minY, out float maxY))
            {
                // 현재 위치 기준으로 다음 프레임 위치 계산 (임시)
                var currentY = GetCurrentPosition().y;
                var targetY = currentY + (climbSpeed * Time.fixedDeltaTime);

                if (climbSpeed > FloatUtility.VELOCITY_THRESHOLD)
                {
                    targetY = Mathf.Min(targetY, maxY);
                }
                else if (climbSpeed < -FloatUtility.VELOCITY_THRESHOLD)
                {
                    targetY = Mathf.Max(targetY, minY);
                }

                var actualClimbSpeed = (targetY - currentY) / Time.fixedDeltaTime;
                _currentVelocity = Vector2.up * actualClimbSpeed;
            }
            else
            {
                _currentVelocity = Vector2.up * climbSpeed;
            }
        }

        public virtual void JumpFromLadder(float jumpForceMultiplier = 1.0f)
        {
            if (IsMovementLocked || !_isClimbing || _climbConfig == null)
            {
                return;
            }

            StopClimbing();

            var jumpForce = _climbConfig.JumpFromLadderForce * jumpForceMultiplier;
            _currentVelocity = _currentVelocity.WithY(jumpForce);

            _logger?.Entity($"Jumped from ladder with force: {jumpForce}");
            OnJumpedFromLadder(jumpForceMultiplier);
        }

        public virtual void ForceStopClimbing()
        {
            if (!_isClimbing || _climbConfig == null)
            {
                return;
            }

            StopClimbing();
            _currentVelocity = Vector2.zero;
            _logger?.Entity("Force stopped climbing");
        }

        public virtual void AddLock(EntityMovementLockType lockType)
        {
            _lockType |= lockType;
            _logger?.Entity($"Climb lock added: {lockType}");
        }

        public virtual void RemoveLock(EntityMovementLockType lockType)
        {
            _lockType &= ~lockType;
            _logger?.Entity($"Climb lock removed: {lockType}");
        }

        // 하위 클래스에서 오버라이드할 이벤트 메서드들
        protected virtual void OnClimbingStarted()
        {
        }

        protected virtual void OnClimbingStopped()
        {
        }

        protected virtual void OnJumpedFromLadder(float jumpForceMultiplier)
        {
        }

        // Transform 위치를 가져오는 메서드 (Entity에서 제공받아야 함)
        protected virtual Vector2 GetCurrentPosition()
        {
            // 실제로는 Entity에서 transform.position을 전달받아야 함
            return Vector2.zero;
        }
    }
}