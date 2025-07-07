using MarioGame.Core.Extensions;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Detection;
using MarioGame.Level.LevelObjects.Ladders;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Detections
{
    public class GroundDetector : IGroundDetector
    {
        private readonly IDebugLogger _logger;
        private RaycastHit2D[] _reusableHits;

        public GroundDetector(IDebugLogger logger)
        {
            _logger = logger;
        }
        
        public GroundDetectionResult DetectGround(Transform transform, Collider2D collider, GroundDetectionConfig config)
        {
            if (collider == null)
            {
                _logger?.Warning("Collider2D is null, cannot perform ground detection");
                return new GroundDetectionResult { IsGrounded = false };
            }

            // 레이 배열 크기 확인 및 재할당
            if (_reusableHits == null || _reusableHits.Length < config.GroundCheckRayCount + 1)
            {
                _reusableHits = new RaycastHit2D[config.GroundCheckRayCount + 1];
            }

            var rayPositions = CalculateRayPositions(transform, collider, config);
            var result = new GroundDetectionResult
            {
                RayPositions = rayPositions,
                Hits = new RaycastHit2D[rayPositions.Length]
            };

            LadderTopPlatformBypass foundBypassable = null;
            bool foundGround = false;

            // 각 레이로 검사
            for (int i = 0; i < rayPositions.Length; i++)
            {
                var rayStart = rayPositions[i];
                var hitCount = Physics2D.RaycastNonAlloc(
                    rayStart, 
                    Vector2.down, 
                    _reusableHits, 
                    config.GroundCheckDistance, 
                    config.GroundLayerMask);

                if (hitCount <= 0) continue;
                
                foundGround = true;
                result.Hits[i] = _reusableHits[0]; // 가장 가까운 히트 저장

                // Bypass 검사
                if (config.EnableBypass)
                {
                    foundBypassable = FindBypassInHits(_reusableHits, hitCount);
                }

                // 첫 번째 히트에서 멈춤 (성능 최적화)
                break;
            }

            result.IsGrounded = foundGround;
            result.CurrentBypassable = foundBypassable;

            return result;
        }

        public Vector2[] CalculateRayPositions(Transform transform, Collider2D collider, GroundDetectionConfig config)
        {
            var center = transform.position.ToVector2() + collider.offset;
            var bottom = center.AddY(-collider.bounds.size.y * 0.5f + config.GroundCheckYOffset);

            var checkWidth = collider.bounds.size.x * config.GroundCheckWidth;
            var startX = bottom.x - checkWidth * 0.5f;
            var endX = bottom.x + checkWidth * 0.5f;

            var rayPositions = new Vector2[config.GroundCheckRayCount + 1];
            
            for (int i = 0; i <= config.GroundCheckRayCount; i++)
            {
                var t = (float)i / config.GroundCheckRayCount;
                var rayX = Mathf.Lerp(startX, endX, t);
                rayPositions[i] = new Vector2(rayX, bottom.y);
            }

            return rayPositions;
        }
        
        private LadderTopPlatformBypass FindBypassInHits(RaycastHit2D[] hits, int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                var bypass = hits[i].collider.GetComponent<LadderTopPlatformBypass>();
                if (bypass != null)
                {
                    return bypass;
                }
            }
            return null;
        }
    }
}