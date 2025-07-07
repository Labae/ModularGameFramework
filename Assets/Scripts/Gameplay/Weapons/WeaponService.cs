using System;
using MarioGame.Audio.Interfaces;
using MarioGame.Core.Extensions;
using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling.Interface;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Projectiles;
using MarioGame.Gameplay.Weapons.Interface;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    public class WeaponService : IWeaponService
    {
        private readonly IDebugLogger _logger;
        private readonly IAudioManager _audioManager;
        private readonly IObjectPoolManager _poolManager;

        private WeaponConfiguration _config;
        private Transform _firePosition;
        private Transform _pivot;
        private Vector2 _currentDirection = Vector2.right;
        private float _lastFireTime;

        public WeaponConfiguration CurrentConfig => _config;
        public Vector2 CurrentDirection => _currentDirection;

        public event Action<WeaponFireData> OnWeaponFired;
        public event Action<WeaponConfiguration> OnWeaponChanged;

        public WeaponService(IDebugLogger logger, IAudioManager audioManager, IObjectPoolManager poolManager)
        {
            _logger = logger;
            _audioManager = audioManager;
            _poolManager = poolManager;
        }

        public void Initialize(WeaponConfiguration config, Transform firePosition, Transform pivot)
        {
            _config = config;
            _firePosition = firePosition;
            _pivot = pivot;

            if (_config?.EquipSound != null)
            {
                _audioManager?.PlaySFX(_config.EquipSound);
            }

            _logger?.Entity($"WeaponService initialized with: {_config?.name}");
        }

        public void Update()
        {
            // 필요시 무기별 업데이트 로직
        }

        public void SetDirection(Vector2 direction)
        {
            _currentDirection = direction;
            UpdatePivotRotation();
        }

        public void ChangeWeapon(WeaponConfiguration newConfig)
        {
            if (newConfig == null)
            {
                _logger?.Warning("Attempted to change to null weapon config");
                return;
            }

            _config = newConfig;

            if (_config.EquipSound != null)
            {
                _audioManager?.PlaySFX(_config.EquipSound);
            }

            OnWeaponChanged?.Invoke(_config);
            _logger?.Entity($"Weapon changed to: {_config.name}");
        }

        public virtual bool TryFire(Vector2 entityPosition, IFireCondition fireCondition)
        {
            // 발사 가능 조건 체크
            if (_config == null ||
                Time.time - _lastFireTime < _config.FireRate ||
                !fireCondition.CanFire())
            {
                return false;
            }

            var fireData = CreateFireData(entityPosition);
            ExecuteFire(fireData);
            return true;
        }

        private WeaponFireData CreateFireData(Vector2 entityPosition)
        {
            var firePosition = GetSafeFirePosition(entityPosition);

            return new WeaponFireData
            {
                Position = firePosition,
                Direction = _currentDirection,
                Timestamp = Time.time,
                Speed = _config.ProjectileSpeed
            };
        }

        private void ExecuteFire(WeaponFireData fireData)
        {
            _lastFireTime = fireData.Timestamp;

            // 투사체 생성
            if (_poolManager.TryGetPool<Projectile>(out var pool))
            {
                var projectile = pool.Get();
                projectile?.Fire(fireData.Position, fireData.Direction, _config);
            }

            // 사운드 재생
            if (_config.FireSound != null)
            {
                _audioManager?.PlaySFX(_config.FireSound);
            }

            // 이벤트 발생
            OnWeaponFired?.Invoke(fireData);

            _logger?.Entity($"Weapon fired: {_config.name}");
        }

        private Vector2 GetSafeFirePosition(Vector2 entityPosition)
        {
            var baseFirePosition = _firePosition.position;
            var playerPos = new Vector2(entityPosition.x, baseFirePosition.y);
            var distance = Vector2.Distance(playerPos, baseFirePosition);
            var direction = ((Vector2)baseFirePosition - playerPos).normalized;

            // Raycast로 경로상 벽 체크
            var raycastHit = Physics2D.Raycast(playerPos, direction, distance, _config.GetCombatLayerMask());

            if (raycastHit.collider != null)
            {
                // 벽이 있다면 안전 위치 계산
                var safePosition = raycastHit.point - (direction * _config.SafeFireDistance);
                return safePosition;
            }

            return baseFirePosition;
        }

        private void UpdatePivotRotation()
        {
            if (_pivot == null) return;

            if (_currentDirection.x > 0)
            {
                _pivot.localRotation = Quaternion.identity;
            }
            else if (_currentDirection.x < 0)
            {
                _pivot.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
    }
}