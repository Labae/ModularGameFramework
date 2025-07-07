using MarioGame.Core.Extensions;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Detection;
using MarioGame.Level.LevelObjects.Ladders;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Detections
{
public class LadderDetector : ILadderDetector
    {
        private readonly IDebugLogger _logger;
        private RaycastHit2D[] _reusableHits;

        public LadderDetector(IDebugLogger logger)
        {
            _logger = logger;
        }

        public LadderDetectionResult DetectLadder(Transform transform, Collider2D collider, LadderDetectionConfig config)
        {
            if (collider == null)
            {
                _logger?.Warning("Collider2D is null, cannot perform ladder detection");
                return new LadderDetectionResult { IsOnLadder = false };
            }

            // 레이 배열 크기 확인 및 재할당
            if (_reusableHits == null || _reusableHits.Length < config.LadderCheckRayCount + 1)
            {
                _reusableHits = new RaycastHit2D[config.LadderCheckRayCount + 1];
            }

            var rayPositions = CalculateRayPositions(transform, collider, config);
            var result = new LadderDetectionResult
            {
                RayPositions = rayPositions,
                Hits = new RaycastHit2D[rayPositions.Length]
            };

            Ladder detectedLadder = null;
            bool foundLadder = false;

            // 각 레이로 사다리 검사 (양방향)
            for (int i = 0; i < rayPositions.Length; i++)
            {
                var rayStart = rayPositions[i];
                
                // 오른쪽으로 레이캐스트
                var rightHitCount = Physics2D.RaycastNonAlloc(
                    rayStart, 
                    Vector2.right, 
                    _reusableHits, 
                    config.LadderCheckDistance, 
                    config.LadderCheckLayerMask);

                if (rightHitCount > 0)
                {
                    detectedLadder = FindLadderInHits(_reusableHits, rightHitCount);
                    if (detectedLadder != null)
                    {
                        foundLadder = true;
                        result.Hits[i] = _reusableHits[0];
                        break;
                    }
                }

                // 왼쪽으로 레이캐스트
                var leftHitCount = Physics2D.RaycastNonAlloc(
                    rayStart, 
                    Vector2.left, 
                    _reusableHits, 
                    config.LadderCheckDistance, 
                    config.LadderCheckLayerMask);

                if (leftHitCount <= 0) continue;
                
                detectedLadder = FindLadderInHits(_reusableHits, leftHitCount);
                if (detectedLadder == null) continue;
                
                foundLadder = true;
                result.Hits[i] = _reusableHits[0];
                break;
            }

            result.IsOnLadder = foundLadder;
            result.CurrentLadder = detectedLadder;

            // 사다리 위치 정보 계산
            if (foundLadder && detectedLadder != null)
            {
                var entityPosition = transform.position.ToVector2();
                result.IsAtLadderTop = detectedLadder.IsEntityAtTop(entityPosition);
                result.IsAtLadderBottom = detectedLadder.IsEntityAtBottom(entityPosition);
            }

            return result;
        }

        public Vector2[] CalculateRayPositions(Transform transform, Collider2D collider, LadderDetectionConfig config)
        {
            var center = transform.position.ToVector2() + collider.offset;
            var checkHeight = collider.bounds.size.y * 0.8f;
            var startY = center.y - checkHeight * 0.5f;
            var endY = center.y + checkHeight * 0.5f;

            var rayPositions = new Vector2[config.LadderCheckRayCount];
            
            for (int i = 0; i < config.LadderCheckRayCount; i++)
            {
                var t = (float)i / (config.LadderCheckRayCount - 1);
                var rayY = Mathf.Lerp(startY, endY, t);
                rayPositions[i] = new Vector2(center.x, rayY);
            }

            return rayPositions;
        }

        private Ladder FindLadderInHits(RaycastHit2D[] hits, int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].collider.TryGetComponent<Ladder>(out var ladder))
                {
                    return ladder;
                }
            }
            return null;
        }
    }
}