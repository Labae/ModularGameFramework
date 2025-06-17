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
            var direction = (playerPos - baseFirePosition).normalized;
            var raycastHit = Physics2D.Raycast(baseFirePosition, direction, distance, _config.WallLayers);

            if (raycastHit.collider != null)
            {
                // 실제 경로상의 벽 충돌 지점
                var hitPoint = raycastHit.point;
                var offsetFromWall = direction * _config.SafeFireDistance;
                return hitPoint + offsetFromWall;
            }
            
            return baseFirePosition;
        }
    }
}