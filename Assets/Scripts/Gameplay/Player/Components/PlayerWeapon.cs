using Core;
using Gameplay.Interfaces;
using Gameplay.Projectiles;
using UnityEngine;

namespace Gameplay.Player.Components
{
    public class PlayerWeapon : CoreBehaviour, IPlayerWeaponActions
    {
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField, Min(0.1f)] private float _fireForce = 20f;
        [SerializeField, Min(0.1f)] private float _fireRate = 0.2f;

        [SerializeField, Min(0.1f)] private float _shootToIdleDelay = 1.0f;

        public float ShootToIdleDelay => _shootToIdleDelay;

        private float _lastShootTime;

        public bool CanFire()
        {
            return Time.time - _lastShootTime >= _fireRate;
        }

        public void Shoot()
        {
            var bulletObject = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
            var bullet = bulletObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Fire(_fireForce);
                _lastShootTime = Time.time;
            }
            else
            {
                Destroy(bulletObject);
            }
        }
    }
}