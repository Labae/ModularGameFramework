using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Movement;

namespace MarioGame.Gameplay.Components.Locomotion
{
    public class EnemyJump : EntityJump
    {
        private EnemyMovementConfig _config;

        public EnemyJump(IDebugLogger logger, IGroundChecker groundChecker) 
            : base(logger, groundChecker) { }

        public void Initialize(EnemyMovementConfig config)
        {
            _config = config;
            _logger?.StateMachine($"EnemyJump initialized");
        }

        public override bool TryJump(float forceMultiplier = 1.0f)
        {
            if (_config == null || !IsGrounded)
            {
                return false;
            }
            
            // Enemy는 단순한 점프 (고정된 힘)
            var jumpVelocity = 10f * forceMultiplier; // 기본값
            SetVerticalVelocity(jumpVelocity);

            _logger?.StateMachine($"Enemy jumped with velocity: {jumpVelocity}");
            return true;
        }

        public override void CutJump()
        {
            // Enemy는 점프 컷 없음
        }

        public override void ApplyGravityModifiers(bool jumpInputHeld)
        {
            // Enemy는 기본 중력만 사용
        }
    }
}