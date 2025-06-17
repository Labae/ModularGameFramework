using System;
using System.Collections.Generic;
using System.Linq;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces.Projectiles;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Physics.ProjectileCollision
{
    /// <summary>
    /// 물리 기반 관통 투사체 충돌 검사 구현체
    /// Piercing 타입에서 사용
    /// 실제로 날아가면서 경로상 충돌 검사 + 관통 처리
    /// </summary>
    public class ProjectilePiercingCollision : IProjectilePiercingCollision
    {
        /// <summary>
        /// 경로 기반 관통 충돌 검사 (메인 메서드)
        /// 이전 위치에서 현재 위치까지의 경로상 모든 관통 가능 타겟 검사
        /// </summary>
        public ProjectileHitData[] CheckMultipleCollisions(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            // 관통형은 경로 기반 검사를 사용해야 함
            // 하지만 인터페이스 호환성을 위해 짧은 경로로 변환
            var endPoint = origin + direction * 0.1f; // 아주 짧은 거리
            return CheckPiercingPath(origin, endPoint, weaponConfig);
        }

        /// <summary>
        /// 경로 기반 관통 충돌 검사 (실제 구현)
        /// 한 프레임 이동 경로상에서 모든 관통 가능 타겟 검사
        /// </summary>
        public ProjectileHitData[] CheckPiercingPath(Vector2 previousPosition, Vector2 currentPosition, WeaponConfiguration weaponConfig)
        {
            if (weaponConfig == null)
            {
                Debug.LogWarning("WeaponConfiguration is null");
                return Array.Empty<ProjectileHitData>();
            }

            if (!(weaponConfig is PiercingWeaponConfig piercingConfig))
            {
                Debug.LogWarning("WeaponConfiguration is not PiercingWeaponConfig");
                return Array.Empty<ProjectileHitData>();
            }

            var pathVector = currentPosition - previousPosition;
            var pathDistance = pathVector.magnitude;
            
            // 경로가 너무 짧으면 스킵
            if (pathDistance < 0.01f)
            {
                return Array.Empty<ProjectileHitData>();
            }

            var direction = pathVector / pathDistance;
            var targetLayers = CreateTargetLayerMask(weaponConfig);

            // 경로상 모든 충돌 감지
            var allHits = Physics2D.RaycastAll(previousPosition, direction, pathDistance, targetLayers);
            
            if (allHits.Length == 0)
            {
                return Array.Empty<ProjectileHitData>();
            }

            // 거리순 정렬 및 관통 처리
            return ProcessPiercingHits(allHits, previousPosition, currentPosition, weaponConfig, 
                pathVector);
        }

        /// <summary>
        /// 관통 히트 처리
        /// 관통 가능한 타겟들을 순서대로 처리
        /// </summary>
        private ProjectileHitData[] ProcessPiercingHits(
            RaycastHit2D[] allHits, 
            Vector2 previousPosition, 
            Vector2 currentPosition,
            WeaponConfiguration weaponConfig, 
            Vector2 pathVector)
        {
            var piercingConfig = (PiercingWeaponConfig)weaponConfig;
            var validHits = new List<ProjectileHitData>();
            
            // 거리순 정렬
            var sortedHits = allHits
                .Where(hit => hit.collider != null && IsHitOnPath(previousPosition, currentPosition, hit.point))
                .OrderBy(hit => hit.distance)
                .ToArray();

            int remainingPenetrations = piercingConfig.MaxPenetrations;

            foreach (var hit in sortedHits)
            {
                var hitData = ProjectileHitData.FromPhysicsPath(previousPosition, currentPosition, hit, weaponConfig, pathVector);
                
                // 유효한 히트인지 검증
                if (!IsValidHit(hitData))
                {
                    continue;
                }

                validHits.Add(hitData);

                // 관통 가능한지 체크
                if (!CanPenetrateTarget(hitData, remainingPenetrations, weaponConfig))
                {
                    // 관통 불가능하면 여기서 멈춤
                    hitData.ShouldStopProjectile = true;
                    break;
                }

                // 관통 성공
                remainingPenetrations--;
                hitData.CanPenetrate = true;
                hitData.ShouldStopProjectile = false;

                // 관통 횟수 소진되면 다음 충돌에서 정지
                if (remainingPenetrations <= 0)
                {
                    // 다음 충돌이 있다면 그것에서 정지
                    var nextHitIndex = Array.IndexOf(sortedHits, hit) + 1;
                    if (nextHitIndex < sortedHits.Length)
                    {
                        var nextHit = sortedHits[nextHitIndex];
                        var nextHitData = ProjectileHitData.FromPhysicsPath(previousPosition, currentPosition, nextHit, weaponConfig, pathVector);
                        nextHitData.ShouldStopProjectile = true;
                        validHits.Add(nextHitData);
                    }
                    break;
                }
            }

            return validHits.ToArray();
        }

        /// <summary>
        /// 관통 가능 여부 체크
        /// 타겟 타입과 남은 관통 횟수 고려
        /// </summary>
        public bool CanPenetrateTarget(ProjectileHitData hitData, int remainingPenetrations, WeaponConfiguration weaponConfig)
        {
            if (remainingPenetrations <= 0)
            {
                return false;
            }

            if (weaponConfig is not PiercingWeaponConfig piercingConfig)
            {
                return false;
            }

            // WeaponConfig의 관통 가능 여부 체크 사용
            var hitType = weaponConfig.DetermineHitType(hitData.HitCollider);
            return weaponConfig.CanPenetrateTarget(hitType);
        }

        /// <summary>
        /// 관통 후 궤도 계산
        /// 관통 시 투사체 방향/속도 변화 계산
        /// </summary>
        public (Vector2 newDirection, float newSpeed) CalculatePostPenetrationTrajectory(
            Vector2 currentDirection, 
            float currentVelocity, 
            ProjectileHitData hitData, 
            WeaponConfiguration weaponConfig)
        {
            if (!(weaponConfig is PiercingWeaponConfig piercingConfig))
            {
                return (currentDirection, currentVelocity);
            }

            // 방향은 일반적으로 유지 (관통이므로)
            var newDirection = currentDirection;

            // 속도는 관통 시 감소
            var speedReduction = 1f - piercingConfig.SpeedReductionPerHit;
            var newSpeed = currentVelocity * speedReduction;

            // 최소 속도 보장 (10%)
            newSpeed = Mathf.Max(newSpeed, currentVelocity * 0.1f);

            return (newDirection, newSpeed);
        }

        /// <summary>
        /// WeaponConfiguration에서 타겟 레이어 마스크 생성
        /// </summary>
        private LayerMask CreateTargetLayerMask(WeaponConfiguration weaponConfig)
        {
            LayerMask targetLayers = weaponConfig.EnemyLayers | weaponConfig.WallLayers | weaponConfig.DestructibleLayers;
            
            if (weaponConfig.AllowFriendlyFire)
            {
                targetLayers |= weaponConfig.FriendlyLayers;
            }
            
            if (weaponConfig.DamageNeutralTargets)
            {
                targetLayers |= weaponConfig.NeutralLayers;
            }

            return targetLayers;
        }

        /// <summary>
        /// 충돌 지점이 실제 이동 경로상에 있는지 검증
        /// </summary>
        private bool IsHitOnPath(Vector2 startPos, Vector2 endPos, Vector2 hitPoint)
        {
            var pathVector = endPos - startPos;
            var hitVector = hitPoint - startPos;
            
            // 방향 일치 검사 (내적 > 0.9f로 엄격하게)
            var dot = Vector2.Dot(pathVector.normalized, hitVector.normalized);
            if (dot < 0.9f) return false;
            
            // 거리 검사 (경로 길이를 초과하지 않아야 함)
            return hitVector.sqrMagnitude <= pathVector.sqrMagnitude;
        }

        /// <summary>
        /// 유효한 충돌인지 검증
        /// </summary>
        private bool IsValidHit(ProjectileHitData hitData)
        {
            if (hitData.Equals(ProjectileHitData.Empty) || !hitData.IsValid) return false;
            if (!hitData.HitCollider) return false;
            if (!hitData.HitCollider.gameObject.activeInHierarchy) return false;
            
            return true;
        }
    }
}