using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Components
{
    [DisallowMultipleComponent]
    public sealed class PlayerJump : EntityJump, IPlayerJumpActions
    {
        private PlayerMovementConfig _config;

        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

        public bool CanExecuteJump => (_coyoteTimeCounter > 0 || IsGrounded);
        public bool HasPendingJump => _jumpBufferCounter > 0;
        
        public float CoyoteTimeLeft => _coyoteTimeCounter;
        public float JumpBufferLeft => _jumpBufferCounter;

        public void Initialize(PlayerMovementConfig config)
        {
            _config = config;
            AssertIsNotNull(_config, "PlayerMovementConfig required");
            
            _rigidbody2D.gravityScale = _config.GravityScale;
        }

        private void Update()
        {
            if (_config == null)
            {
                return;
            }
            
            UpdateTimers();
        }

        private void FixedUpdate()
        {
            if (_config == null)
            {
                return;
            }
            
            ApplyGravityModifiers(false);
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

            return true;
        }

        public void RequestJump()
        {
            if (_config == null)
            {
                return;
            }

            _jumpBufferCounter = _config.JumpBufferTime;
        }

        public override void CutJump()
        {
            if (_config == null || VerticalVelocity < -FloatUtility.VELOCITY_THRESHOLD)
            {
                return;
            }
            
            SetVerticalVelocity(VerticalVelocity * _config.VariableJumpMultiplier);
        }

        public override void ApplyGravityModifiers(bool jumpInputHeld)
        {
            if (_config == null)
            {
                return;
            }

            if (VerticalVelocity < -FloatUtility.VELOCITY_THRESHOLD)
            {
                var fallGravity = Physics2D.gravity.y * (_config.FallMultiplier - 1) * Time.fixedDeltaTime;
                AddVerticalVelocity(fallGravity);
            }
            else if (VerticalVelocity > FloatUtility.VELOCITY_THRESHOLD && !jumpInputHeld)
            {
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