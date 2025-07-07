using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Movement;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
  public class PlayerJump : EntityJump
    {
        private PlayerMovementConfig _config;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

        public bool CanExecuteJump => (_coyoteTimeCounter > 0 || IsGrounded);
        public bool HasPendingJump => _jumpBufferCounter > 0;
        public float CoyoteTimeLeft => _coyoteTimeCounter;
        public float JumpBufferLeft => _jumpBufferCounter;

        public PlayerJump(IDebugLogger logger, IGroundChecker groundChecker) 
            : base(logger, groundChecker) { }

        public void Initialize(PlayerMovementConfig config)
        {
            _config = config;
            _logger?.StateMachine($"PlayerJump initialized with jump force: {_config?.JumpForce}");
        }

        public override void Update()
        {
            if (_config == null) return;
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            if (IsGrounded)
            {
                _coyoteTimeCounter = _config.CoyoteTime;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }

            if (_jumpBufferCounter > 0)
            {
                _jumpBufferCounter -= Time.deltaTime;
            }
        }

        public override bool TryJump(float forceMultiplier = 1.0f)
        {
            if (_config == null || !CanExecuteJump)
            {
                return false;
            }
            
            var jumpVelocity = forceMultiplier * _config.JumpForce;
            SetVerticalVelocity(jumpVelocity);

            _coyoteTimeCounter = 0;
            _jumpBufferCounter = 0;

            _logger?.StateMachine($"Player jumped with velocity: {jumpVelocity}");
            return true;
        }

        public void RequestJump()
        {
            if (_config == null) return;
            _jumpBufferCounter = _config.JumpBufferTime;
        }

        public override void CutJump()
        {
            if (_config == null || VerticalVelocity < -FloatUtility.VELOCITY_THRESHOLD)
            {
                return;
            }
            
            SetVerticalVelocity(VerticalVelocity * _config.VariableJumpMultiplier);
            _logger?.StateMachine("Jump cut applied");
        }

        public override void ApplyGravityModifiers(bool jumpInputHeld)
        {
            if (_config == null) return;

            if (VerticalVelocity < -FloatUtility.VELOCITY_THRESHOLD)
            {
                // 낙하 중: 더 빠른 낙하
                var fallGravity = Physics2D.gravity.y * (_config.FallMultiplier - 1) * Time.fixedDeltaTime;
                AddVerticalVelocity(fallGravity);
            }
            else if (VerticalVelocity > FloatUtility.VELOCITY_THRESHOLD && !jumpInputHeld)
            {
                // 상승 중 + 점프 버튼 안 누름: 약한 점프
                var lowJumpGravity = Physics2D.gravity.y * (_config.LowJumpMultiplier - 1) * Time.fixedDeltaTime;
                AddVerticalVelocity(lowJumpGravity);
            }
        }

        public void SetJumpInputHeld(bool held)
        {
            ApplyGravityModifiers(held);
        }
    }
}