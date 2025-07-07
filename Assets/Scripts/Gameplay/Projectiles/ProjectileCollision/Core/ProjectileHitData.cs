using System;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces.Combat;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles.ProjectileCollision.Core
{
    /// <summary>
    /// 투사체 충돌 결과 데이터를 담는 구조체
    /// 기존 코드 기반 + 필요한 기능만 추가
    /// </summary>
    [Serializable]
    public struct ProjectileHitData : IEquatable<ProjectileHitData>
    {
        public bool HasHit;
        public Vector2 HitPoint;
        public Vector2 HitNormal;
        public Collider2D HitCollider;
        public float Distance;
        public int LayerHit;
        public bool IsPriority;
        
        // 추가된 정보들
        public HitTargetType Type;
        public float HitAngle; // 입사각 (반사 계산용)
        public Vector2 HitDirection; // 충돌 방향
        public Component HitComponent; // 충돌한 컴포넌트 (IDamageable 등)

        // 물리 정보 (Piercing에서 필요)
        public Vector2 HitVelocity; // 충돌 시 투사체 속도
        public float HitForce; // 충돌 힘
        public bool ShouldStopProjectile;
        public bool CanPenetrate;

        public bool IsValid => HasHit && HitCollider != null;
        public bool IsWallHit => Type == HitTargetType.Wall;
        public bool IsEntityHit => Type == HitTargetType.Entity;
        public bool IsDestructibleHit => Type == HitTargetType.Destructible;

        // 충돌 각도 계산 (반사 등에 사용)
        public float CalculateReflectionAngle(Vector2 incomingDirection)
        {
            if (HitNormal == Vector2.zero) return 0f;
            return Vector2.Angle(incomingDirection, Vector2.Reflect(incomingDirection, HitNormal));
        }

        // 반사 방향 계산
        public Vector2 CalculateReflectionDirection(Vector2 incomingDirection)
        {
            if (HitNormal == Vector2.zero) return incomingDirection;
            return Vector2.Reflect(incomingDirection, HitNormal);
        }

        public static ProjectileHitData Empty => new ProjectileHitData
        {
            HasHit = false,
            HitPoint = Vector2.zero,
            HitNormal = Vector2.zero,
            HitCollider = null,
            Distance = float.MaxValue,
            LayerHit = -1,
            IsPriority = false,
            Type = HitTargetType.None,
            HitAngle = 0f,
            HitDirection = Vector2.zero,
            HitComponent = null,
            HitVelocity = Vector2.zero,
            HitForce = 0f
        };

        /// <summary>
        /// RaycastHit2D에서 ProjectileHitData 생성 (WeaponConfig 기반 타입 판별)
        /// </summary>
        public static ProjectileHitData FromRaycastHit(RaycastHit2D hit, 
            WeaponConfiguration weaponConfig = null, Vector2 projectileDirection = default, Vector2 projectileVelocity = default)
        {
            var hitData = new ProjectileHitData
            {
                HasHit = hit.collider != null,
                HitPoint = hit.point,
                HitNormal = hit.normal,
                HitCollider = hit.collider,
                Distance = hit.distance,
                LayerHit = hit.collider?.gameObject.layer ?? -1,
                IsPriority = false,
                HitDirection = projectileDirection,
                HitVelocity = projectileVelocity,
                HitForce = projectileVelocity.magnitude
            };

            // 충돌 타입 판별
            if (hit.collider != null)
            {
                if (weaponConfig != null)
                {
                    // WeaponConfiguration 기반 정확한 타입 판별
                    hitData.Type = weaponConfig.DetermineHitType(hit.collider);
                    hitData.IsPriority = IsPriorityTarget(hit.collider, weaponConfig);
                }
                else
                {
                    // 기본 레이어 이름 기반 판별 (fallback)
                    hitData.Type = DetermineHitTypeByLayer(hit.collider);
                }
                
                hitData.HitComponent = GetRelevantComponent(hit.collider);
                
                if (projectileDirection != Vector2.zero && hit.normal != Vector2.zero)
                {
                    hitData.HitAngle = Vector2.Angle(projectileDirection, hit.normal);
                }
            }

            return hitData;
        }

        /// <summary>
        /// 물리 경로 추적에서 ProjectileHitData 생성 (Piercing용)
        /// </summary>
        public static ProjectileHitData FromPhysicsPath(Vector2 previousPos, Vector2 currentPos, 
            RaycastHit2D hit, WeaponConfiguration weaponConfig = null, Vector2 velocity = default)
        {
            var direction = (currentPos - previousPos).normalized;
            var hitData = FromRaycastHit(hit, weaponConfig, direction, velocity);
            
            // 물리 경로 추적 특수 처리
            if (hitData.HasHit)
            {
                // 실제 히트 포인트가 경로 상에 있는지 확인
                var pathDirection = currentPos - previousPos;
                var hitDirection = hitData.HitPoint - previousPos;
                
                // 경로를 벗어난 히트는 무효화 (터널링 방지)
                if (Vector2.Dot(pathDirection.normalized, hitDirection.normalized) < 0.8f)
                {
                    return Empty;
                }
            }
            
            return hitData;
        }

        /// <summary>
        /// OverlapCollider2D에서 ProjectileHitData 생성
        /// </summary>
        public static ProjectileHitData FromOverlapHit(Collider2D collider, Vector2 hitPoint, 
            WeaponConfiguration weaponConfig = null, Vector2 projectileDirection = default)
        {
            var hitData = new ProjectileHitData
            {
                HasHit = collider != null,
                HitPoint = hitPoint,
                HitNormal = (hitPoint - (Vector2)collider.bounds.center).normalized, // 근사치
                HitCollider = collider,
                Distance = 0f, // Overlap은 거리가 0
                LayerHit = collider?.gameObject.layer ?? -1,
                IsPriority = false,
                HitDirection = projectileDirection,
                HitComponent = GetRelevantComponent(collider),
                HitVelocity = Vector2.zero, // Overlap은 속도 없음
                HitForce = 0f
            };

            if (collider != null)
            {
                if (weaponConfig != null)
                {
                    hitData.Type = weaponConfig.DetermineHitType(collider);
                    hitData.IsPriority = IsPriorityTarget(collider, weaponConfig);
                }
                else
                {
                    hitData.Type = DetermineHitTypeByLayer(collider);
                }
            }

            return hitData;
        }

        /// <summary>
        /// 우선순위 타겟인지 판별 (WeaponConfig 기반)
        /// </summary>
        private static bool IsPriorityTarget(Collider2D collider, WeaponConfiguration weaponConfig)
        {
            if (collider == null || weaponConfig == null) return false;
            
            var layer = collider.gameObject.layer;
            var layerMask = 1 << layer;
            
            // 적군 레이어가 우선순위
            return (weaponConfig.EnemyLayers & layerMask) != 0;
        }

        /// <summary>
        /// 레이어 이름 기반 타입 판별 (WeaponConfig 없을 때 fallback)
        /// </summary>
        private static HitTargetType DetermineHitTypeByLayer(Collider2D collider)
        {
            if (collider == null) return HitTargetType.None;

            var layer = collider.gameObject.layer;
            var layerName = LayerMask.LayerToName(layer);

            return layerName.ToLower() switch
            {
                "player" or "enemy" or "npc" => HitTargetType.Entity, // 모든 생명체
                "ground" or "wall" or "environment" => HitTargetType.Wall,
                "destructible" => HitTargetType.Destructible,
                _ => HitTargetType.Unknown
            };
        }

        /// <summary>
        /// 관련 컴포넌트 찾기
        /// </summary>
        private static Component GetRelevantComponent(Collider2D collider)
        {
            if (collider == null) return null;

            // 데미지를 받을 수 있는 컴포넌트 찾기
            var damageable = collider.GetComponent<IDamageable>();
            if (damageable != null) return damageable as Component;

            // 다른 상호작용 가능한 컴포넌트들
            var interactable = collider.GetComponent<IInteractable>();
            if (interactable != null) return interactable as Component;

            return collider;
        }

        // IEquatable 구현
        public bool Equals(ProjectileHitData other)
        {
            return HasHit == other.HasHit &&
                   HitCollider == other.HitCollider &&
                   Mathf.Approximately(Distance, other.Distance);
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectileHitData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HasHit, HitCollider, Distance);
        }

        public override string ToString()
        {
            if (!IsValid) return "ProjectileHitData: Invalid";
            
            return $"ProjectileHitData: {Type} at {HitPoint}, Distance: {Distance:F2}, " +
                   $"Layer: {LayerMask.LayerToName(LayerHit)}, Priority: {IsPriority}, Force: {HitForce:F1}";
        }
    }
}