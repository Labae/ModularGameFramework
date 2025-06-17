using System;
using System.Linq;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces.Projectiles;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Physics.ProjectileCollision
{
    /// <summary>
    /// 물리 기반 투사체 충돌 검사 구현체
    /// Normal 타입에서 사용
    /// 실제 물리적으로 날아가는 투사체의 충돌 검사
    /// </summary>
    public class ProjectilePhysicsCollision : IProjectilePhysicsCollision
    {
        /// <summary>
        /// 다중 충돌 검사 (일반적으로 단일 충돌)
        /// Normal 타입은 기본적으로 관통하지 않음
        /// </summary>
        public ProjectileHitData[] CheckMultipleCollisions(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (weaponConfig == null)
            {
                Debug.LogWarning("WeaponConfiguration is null");
                return Array.Empty<ProjectileHitData>();
            }

            // 타겟 레이어 마스크 생성
            var targetLayers = CreateTargetLayerMask(weaponConfig);
            var maxDistance = CalculateMaxDistance(weaponConfig);

            // 레이캐스트 실행
            var hit = Physics2D.Raycast(origin, direction, maxDistance, targetLayers);
            
            if (!hit.collider)
            {
                return Array.Empty<ProjectileHitData>();
            }

            // 단일 히트 데이터 생성
            var hitData = ProjectileHitData.FromRaycastHit(hit, weaponConfig, direction);
            return new[] { hitData };
        }

        /// <summary>
        /// 경로 기반 충돌 검사 (tunneling 방지)
        /// 이전 위치에서 현재 위치까지의 선분 충돌 검사
        /// </summary>
        public ProjectileHitData[] CheckPathCollision(Vector2 previousPosition, Vector2 currentPosition, WeaponConfiguration weaponConfig)
        {
            if (weaponConfig == null)
            {
                return Array.Empty<ProjectileHitData>();
            }

            var direction = (currentPosition - previousPosition).normalized;
            var distance = Vector2.Distance(previousPosition, currentPosition);
            var targetLayers = CreateTargetLayerMask(weaponConfig);

            // 경로상 충돌 검사
            var hit = Physics2D.Raycast(previousPosition, direction, distance, targetLayers);
            
            if (!hit.collider)
            {
                return Array.Empty<ProjectileHitData>();
            }

            // 경로 검증 - 실제 경로상에 있는지 확인
            var hitDirection = hit.point - previousPosition;
            var pathDirection = currentPosition - previousPosition;
            
            // 같은 방향인지 확인 (내적 > 0.8f)
            if (Vector2.Dot(hitDirection.normalized, pathDirection.normalized) < 0.8f)
            {
                return Array.Empty<ProjectileHitData>();
            }

            return new[]
            {
                ProjectileHitData.FromPhysicsPath(previousPosition, currentPosition, hit, weaponConfig, pathDirection)
            };
        }

        /// <summary>
        /// 반사/튕김 방향 계산
        /// Normal 타입의 CanBounce 기능용
        /// </summary>
        public Vector2 CalculateReflectionDirection(Vector2 incomingDirection, Vector2 hitNormal)
        {
            if (hitNormal == Vector2.zero)
            {
                return incomingDirection; // 법선이 없으면 원래 방향 유지
            }

            return Vector2.Reflect(incomingDirection, hitNormal);
        }

        /// <summary>
        /// WeaponConfiguration에서 타겟 레이어 마스크 생성
        /// </summary>
        private LayerMask CreateTargetLayerMask(WeaponConfiguration weaponConfig)
        {
            LayerMask targetLayers = weaponConfig.EnemyLayers | weaponConfig.WallLayers | weaponConfig.DestructibleLayers;
            
            // 아군 충돌 체크 (데미지는 별도 처리)
            if (weaponConfig.AllowFriendlyFire)
            {
                targetLayers |= weaponConfig.FriendlyLayers;
            }
            
            // 중립 충돌 체크
            if (weaponConfig.DamageNeutralTargets)
            {
                targetLayers |= weaponConfig.NeutralLayers;
            }

            return targetLayers;
        }

        /// <summary>
        /// 최대 거리 계산
        /// </summary>
        private float CalculateMaxDistance(WeaponConfiguration weaponConfig)
        {
            // 물리 투사체는 속도 * 수명으로 최대 거리 계산
            return weaponConfig.ProjectileSpeed * weaponConfig.ProjectileLifetime;
        }

        /// <summary>
        /// 우선순위 타겟 정렬
        /// 적군을 우선으로 정렬
        /// </summary>
        private ProjectileHitData[] SortByPriority(ProjectileHitData[] hits, WeaponConfiguration weaponConfig)
        {
            return hits.OrderBy(hit => 
            {
                var layer = hit.HitCollider.gameObject.layer;
                var layerMask = 1 << layer;
                
                // 적군이 우선순위 0 (가장 높음)
                if ((weaponConfig.EnemyLayers & layerMask) != 0) return 0;
                // 파괴 가능한 오브젝트 우선순위 1
                if ((weaponConfig.DestructibleLayers & layerMask) != 0) return 1;
                // 벽이 우선순위 2 (가장 낮음)
                return 2;
            })
            .ThenBy(hit => hit.Distance) // 같은 우선순위에서는 거리순
            .ToArray();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터용 디버그 레이 그리기
        /// </summary>
        public void DrawDebugRay(Vector2 origin, Vector2 direction, float distance, Color color, float duration = 1f)
        {
            Debug.DrawRay(origin, direction * distance, color, duration);
        }

        /// <summary>
        /// 에디터용 충돌 검사 시각화
        /// </summary>
        public void VisualizeCollisionCheck(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (weaponConfig == null) return;

            var maxDistance = CalculateMaxDistance(weaponConfig);
            var targetLayers = CreateTargetLayerMask(weaponConfig);
            
            var hit = Physics2D.Raycast(origin, direction, maxDistance, targetLayers);
            
            if (hit.collider)
            {
                // 충돌 지점까지는 빨간색
                Debug.DrawRay(origin, direction * hit.distance, Color.red, 2f);
                // 충돌 지점에서 법선 벡터는 파란색
                Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.blue, 2f);
            }
            else
            {
                // 충돌 없으면 녹색
                Debug.DrawRay(origin, direction * maxDistance, Color.green, 1f);
            }
        }
#endif
    }
}