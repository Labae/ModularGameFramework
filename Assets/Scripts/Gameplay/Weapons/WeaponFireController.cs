using MarioGame.Core.Extensions;
using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces.Weapon;
using MarioGame.Gameplay.Projectiles;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    /// <summary>
    /// 무기 발사 제어
    /// </summary>
    public class WeaponFireController : IWeaponFireController
    {
        private readonly WeaponConfiguration _config;
        private readonly Transform _firePosition;
        private float _lastFireTime;

        public WeaponFireController(WeaponConfiguration config, Transform firePosition)
        {
            _config = config;
            _firePosition = firePosition;
        }

        public void Initialize()
        {
            if (_config?.ProjectilePrefab != null)
            {
                var concreteProjectile = _config.ProjectilePrefab.GetComponent<Projectile>();
                ObjectPoolManager.Instance.CreatePool(concreteProjectile);
            }
        }

        public bool CanFire(IFireCondition fireCondition)
        {
            if (Time.time - _lastFireTime < _config.FireRate)
            {
                return false;
            }

            return fireCondition.CanFire();
        }

        public WeaponFireData CreateFireData(Vector2 direction, Vector2 entityPosition)
        {
            return new WeaponFireData
            {
                Position = GetSafeFirePosition(entityPosition),
                Direction = direction,
                Timestamp = Time.time
            };
        }

        public void Fire(WeaponFireData fireData)
        {
            if (ObjectPoolManager.Instance.TryGetPool(_config.ProjectilePrefab, out var pool))
            {
                var bullet = pool.Get();
                bullet?.Fire(fireData.Position, fireData.Direction, _config);
            }
            
            _lastFireTime = fireData.Timestamp;
        }

        private Vector2 GetSafeFirePosition(Vector2 entityPosition)
        {
            var baseFirePosition = _firePosition.position.ToVector2();
            var playerPos = entityPosition.WithY(_firePosition.position.y); 
            var distance = Vector2.Distance(playerPos, baseFirePosition);
            var direction = (baseFirePosition - playerPos).normalized;
    
            // 1단계: Raycast로 경로상 벽 체크
            var raycastHit = Physics2D.Raycast(playerPos, direction, distance, _config.GetCombatLayerMask());
    
            if (raycastHit.collider != null)
            {
                // 벽이 있다면 안전 위치 계산
                var safePosition = raycastHit.point - (direction * _config.SafeFireDistance);
                return safePosition;
            }
    
            return baseFirePosition;
        }

        private Vector2 AdjustPositionForOverlap(Vector2 position, Collider2D overlap, Vector2 direction)
        {
            // 겹치는 콜라이더에서 가장 가까운 점 찾기
            var closestPoint = overlap.ClosestPoint(position);
            var adjustDirection = (position - closestPoint).normalized;
    
            // 추가로 밀어내기
            return position + adjustDirection * _config.SafeFireDistance;
        }
    }
}