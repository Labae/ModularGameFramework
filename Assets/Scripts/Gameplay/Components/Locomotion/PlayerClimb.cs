using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
    public class PlayerClimb : EntityClimb
    {
        private Transform _transform; // Entity에서 주입받음

        public PlayerClimb(IDebugLogger logger, ILadderChecker ladderChecker, Transform transform) 
            : base(logger, ladderChecker)
        {
            _transform = transform;
        }

        protected override Vector2 GetCurrentPosition()
        {
            return _transform?.position ?? Vector2.zero;
        }

        public override void StartClimbing()
        {
            base.StartClimbing();
            
            // Player 특화: 사다리 중앙으로 자동 정렬
            if (_ladderChecker.TryGetLadderAlignPosition(out float ladderCenterX))
            {
                var currentPos = _transform.position;
                _transform.position = new Vector3(ladderCenterX, currentPos.y, currentPos.z);
                _logger?.Entity($"Aligned to ladder center: {ladderCenterX}");
            }
        }

        protected override void ApplyClimbMovement(float climbSpeed)
        {
            // Player 특화: 사다리 중앙 유지
            if (_ladderChecker.TryGetLadderAlignPosition(out float ladderCenterX))
            {
                var currentPos = _transform.position;
                _transform.position = new Vector3(ladderCenterX, currentPos.y, currentPos.z);
            }

            base.ApplyClimbMovement(climbSpeed);
        }

        protected override void OnClimbingStarted()
        {
            base.OnClimbingStarted();
            // Player 특화 로직 (사운드, 이펙트 등)
        }

        protected override void OnJumpedFromLadder(float jumpForceMultiplier)
        {
            base.OnJumpedFromLadder(jumpForceMultiplier);
            // Player 특화 로직 (사운드, 이펙트 등)
        }
    }
}