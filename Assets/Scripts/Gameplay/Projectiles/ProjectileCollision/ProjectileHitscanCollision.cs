using System;
using System.Collections.Generic;
using System.Linq;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces.Projectiles;
using MarioGame.Gameplay.Projectiles.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles.ProjectileCollision
{
    /// <summary>
    /// 즉시 명중 투사체 충돌 검사 구현체
    /// Hitscan 타입에서 사용
    /// 즉시 레이캐스트로 충돌 검사하는 무기용
    /// </summary>
    public class ProjectileHitscanCollision : IProjectileHitscanCollision
    {
        /// <summary>
        /// 즉시 명중 다중 충돌 검사
        /// MaxRange까지 모든 충돌 검사 (관통 지원)
        /// </summary>
        public ProjectileHitData[] CheckMultipleCollisions(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (weaponConfig == null)
            {
                Debug.LogWarning("WeaponConfiguration is null");
                return Array.Empty<ProjectileHitData>();
            }

            if (!(weaponConfig is HitscanWeaponConfig hitscanConfig))
            {
                Debug.LogWarning("WeaponConfiguration is not HitscanWeaponConfig");
                return Array.Empty<ProjectileHitData>();
            }

            // 관통 지원 여부에 따라 다른 검사 방식 사용
            if (hitscanConfig.CanPenetrate)
            {
                return CheckPenetratingHits(origin, direction, hitscanConfig);
            }
            else
            {
                var singleHit = CheckInstantHit(origin, direction, weaponConfig);
                return singleHit.IsValid ? new[] { singleHit } : Array.Empty<ProjectileHitData>();
            }
        }

        /// <summary>
        /// 즉시 단일 충돌 검사
        /// 가장 가까운 충돌만 반환
        /// </summary>
        public ProjectileHitData CheckInstantHit(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (weaponConfig is not HitscanWeaponConfig hitscanConfig)
            {
                return ProjectileHitData.Empty;
            }

            var targetLayers = CreateTargetLayerMask(hitscanConfig);
            var hit = Physics2D.Raycast(origin, direction, hitscanConfig.MaxRange, targetLayers);
            
            if (!hit.collider)
            {
                return ProjectileHitData.Empty;
            }

            return ProjectileHitData.FromRaycastHit(hit, weaponConfig, direction);
        }

        /// <summary>
        /// 최대 사거리 내 모든 충돌 검사
        /// IgnoreWalls 설정 고려
        /// </summary>
        public ProjectileHitData[] CheckRangedHits(Vector2 origin, Vector2 direction, float maxRange, WeaponConfiguration weaponConfig)
        {
            if (!(weaponConfig is HitscanWeaponConfig hitscanConfig))
            {
                return Array.Empty<ProjectileHitData>();
            }

            var targetLayers = CreateTargetLayerMask(hitscanConfig);
            var hits = Physics2D.RaycastAll(origin, direction, maxRange, targetLayers);
            
            if (hits.Length == 0)
            {
                return Array.Empty<ProjectileHitData>();
            }

            // 거리순 정렬 후 변환
            return hits
                .OrderBy(hit => hit.distance)
                .Select(hit => ProjectileHitData.FromRaycastHit(hit, weaponConfig, direction))
                .Where(hitData => IsValidTarget(hitData, hitscanConfig))
                .ToArray();
        }

        /// <summary>
        /// 레이저 빔 형태 연속 충돌 검사
        /// 레이저 타입 무기용
        /// </summary>
        public ProjectileHitData[] CheckBeamCollision(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (!(weaponConfig is HitscanWeaponConfig hitscanConfig))
            {
                return Array.Empty<ProjectileHitData>();
            }

            // 레이저는 MaxRange까지 모든 타겟에 지속적으로 데미지
            var hits = new List<ProjectileHitData>();
            var targetLayers = CreateTargetLayerMask(hitscanConfig);
            
            // 빔 경로상 모든 충돌 감지
            var allHits = Physics2D.RaycastAll(origin, direction, hitscanConfig.MaxRange, targetLayers);
            
            foreach (var hit in allHits.OrderBy(h => h.distance))
            {
                var hitData = ProjectileHitData.FromRaycastHit(hit, weaponConfig, direction);
                
                if (IsValidTarget(hitData, hitscanConfig))
                {
                    hits.Add(hitData);
                }

                // 벽에 막히면 중단 (IgnoreWalls가 false인 경우)
                if (!hitscanConfig.IgnoreWalls && IsWallHit(hitData, hitscanConfig))
                {
                    break;
                }
            }

            return hits.ToArray();
        }

        /// <summary>
        /// 관통 히트 검사
        /// </summary>
        private ProjectileHitData[] CheckPenetratingHits(Vector2 origin, Vector2 direction, HitscanWeaponConfig hitscanConfig)
        {
            var hits = new List<ProjectileHitData>();
            var targetLayers = CreateTargetLayerMask(hitscanConfig);
            
            var currentOrigin = origin;
            var remainingRange = hitscanConfig.MaxRange;
            var remainingPenetrations = hitscanConfig.MaxPenetrationTargets;

            while (remainingPenetrations > 0 && remainingRange > 0.1f)
            {
                var hit = Physics2D.Raycast(currentOrigin, direction, remainingRange, targetLayers);
                
                if (!hit.collider)
                {
                    break; // 더 이상 충돌 없음
                }

                var hitData = ProjectileHitData.FromRaycastHit(hit, hitscanConfig, direction);
                
                if (IsValidTarget(hitData, hitscanConfig))
                {
                    hits.Add(hitData);
                    remainingPenetrations--;
                }

                // 벽에 막히면 중단 (IgnoreWalls가 false인 경우)
                if (!hitscanConfig.IgnoreWalls && IsWallHit(hitData, hitscanConfig))
                {
                    break;
                }

                // 다음 레이캐스트 준비
                currentOrigin = hit.point + direction * 0.01f;
                remainingRange -= hit.distance + 0.01f;
            }

            return hits.ToArray();
        }

        /// <summary>
        /// 유효한 타겟인지 검사
        /// </summary>
        private bool IsValidTarget(ProjectileHitData hitData, HitscanWeaponConfig hitscanConfig)
        {
            if (!hitData.IsValid)
            {
                return false;
            }

            // 거리 체크
            if (hitData.Distance > hitscanConfig.MaxRange)
            {
                return false;
            }

            // 벽 무시 설정 체크
            if (hitscanConfig.IgnoreWalls && IsWallHit(hitData, hitscanConfig))
            {
                return false; // 벽 무시 시 벽은 유효하지 않은 타겟
            }

            return true;
        }

        /// <summary>
        /// 벽 충돌인지 확인
        /// </summary>
        private bool IsWallHit(ProjectileHitData hitData, HitscanWeaponConfig hitscanConfig)
        {
            var layer = hitData.HitCollider.gameObject.layer;
            var layerMask = 1 << layer;
            
            return (hitscanConfig.WallLayers & layerMask) != 0;
        }

        /// <summary>
        /// WeaponConfiguration에서 타겟 레이어 마스크 생성
        /// IgnoreWalls 설정 고려
        /// </summary>
        private LayerMask CreateTargetLayerMask(HitscanWeaponConfig hitscanConfig)
        {
            LayerMask targetLayers = hitscanConfig.EnemyLayers | hitscanConfig.DestructibleLayers;
            
            // 벽 포함 여부 (IgnoreWalls 설정 반대)
            if (!hitscanConfig.IgnoreWalls)
            {
                targetLayers |= hitscanConfig.WallLayers;
            }
            
            if (hitscanConfig.AllowFriendlyFire)
            {
                targetLayers |= hitscanConfig.FriendlyLayers;
            }
            
            if (hitscanConfig.DamageNeutralTargets)
            {
                targetLayers |= hitscanConfig.NeutralLayers;
            }

            return targetLayers;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터용 Hitscan 시각화
        /// </summary>
        public void VisualizeHitscan(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (!(weaponConfig is HitscanWeaponConfig hitscanConfig)) return;

            var hits = CheckMultipleCollisions(origin, direction, weaponConfig);
            
            // 전체 사거리를 녹색 선으로 표시
            Debug.DrawRay(origin, direction * hitscanConfig.MaxRange, Color.green, 1f);
            
            // 각 히트 지점을 빨간색으로 표시
            foreach (var hit in hits)
            {
                Debug.DrawRay(hit.HitPoint, Vector2.up * 0.2f, Color.red, 2f);
                Debug.DrawRay(hit.HitPoint, Vector2.right * 0.2f, Color.red, 2f);
            }
        }

        /// <summary>
        /// 에디터용 레이저 빔 시각화
        /// </summary>
        public void VisualizeBeam(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (!(weaponConfig is HitscanWeaponConfig hitscanConfig)) return;

            var hits = CheckBeamCollision(origin, direction, weaponConfig);
            
            // 빔을 두꺼운 파란색 선으로 표시
            var beamEnd = origin + direction * hitscanConfig.MaxRange;
            Debug.DrawLine(origin, beamEnd, Color.cyan, 2f);
            
            // 빔에 맞은 모든 타겟 표시
            foreach (var hit in hits)
            {
                // 타겟을 마젠타색 X로 표시
                var offset = Vector2.one * 0.15f;
                Debug.DrawLine(hit.HitPoint - offset, hit.HitPoint + offset, Color.magenta, 2f);
                Debug.DrawLine(hit.HitPoint + new Vector2(-offset.x, offset.y), hit.HitPoint + new Vector2(offset.x, -offset.y), Color.magenta, 2f);
            }
        }

        /// <summary>
        /// 에디터용 Hitscan 정보 로그
        /// </summary>
        public void LogHitscanInfo(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (!(weaponConfig is HitscanWeaponConfig hitscanConfig)) return;

            var hits = CheckMultipleCollisions(origin, direction, weaponConfig);
            
            Debug.Log($"Hitscan Check: {hits.Length} hits found within {hitscanConfig.MaxRange}m range");
            Debug.Log($"Settings: IgnoreWalls={hitscanConfig.IgnoreWalls}, CanPenetrate={hitscanConfig.CanPenetrate}");
            
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                var damage = hitscanConfig.CalculateDamageAtDistance(hit.Distance);
                Debug.Log($"Hit {i}: {hit.HitCollider.name} at {hit.Distance:F2}m (damage: {damage})");
            }
        }

        /// <summary>
        /// 에디터용 관통 경로 시각화
        /// </summary>
        public void VisualizePenetration(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (!(weaponConfig is HitscanWeaponConfig hitscanConfig) || !hitscanConfig.CanPenetrate) return;

            var hits = CheckPenetratingHits(origin, direction, hitscanConfig);
            
            var currentPos = origin;
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                
                // 각 관통 구간을 다른 색으로 표시
                var color = Color.Lerp(Color.yellow, Color.red, (float)i / hits.Length);
                Debug.DrawLine(currentPos, hit.HitPoint, color, 2f);
                
                // 관통 지점 번호 표시 (디버그용)
                Debug.DrawRay(hit.HitPoint, Vector2.up * 0.3f, Color.white, 2f);
                
                currentPos = hit.HitPoint + direction * 0.02f;
            }
            
            // 남은 사거리를 회색으로 표시
            var finalEnd = origin + direction * hitscanConfig.MaxRange;
            Debug.DrawLine(currentPos, finalEnd, Color.gray, 1f);
        }
#endif
    }
}