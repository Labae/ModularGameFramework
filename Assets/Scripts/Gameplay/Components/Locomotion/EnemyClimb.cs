using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
    public class EnemyClimb : EntityClimb
    {
        private Transform _transform;

        public EnemyClimb(IDebugLogger logger, ILadderChecker ladderChecker, Transform transform) 
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
            
            // Enemy도 사다리 중앙 정렬
            if (_ladderChecker.TryGetLadderAlignPosition(out float ladderCenterX))
            {
                var currentPos = _transform.position;
                _transform.position = new Vector3(ladderCenterX, currentPos.y, currentPos.z);
            }
        }
    }
}