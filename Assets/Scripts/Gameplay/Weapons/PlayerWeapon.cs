using System;
using MarioGame.Core.Enums;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Pickups;
using MarioGame.Gameplay.Player.Components;
using MarioGame.Gameplay.Player.Core;
using MarioGame.Gameplay.Weapons.Interface;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    /// <summary>
    /// 책임을 분리한 개선된 플레이어 무기 시스템
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerWeapon : EntityWeapon<PlayerStatus>, IPlayerWeaponService
    {
        [Header("Player Weapon Settings")] [SerializeField]
        private WeaponConfiguration defaultWeaponConfig;

        private PlayerCollector _collector;
        private IInputProvider _inputProvider;
        private PlayerWeaponService _playerWeaponService;

        // IPlayerWeaponService 구현
        public WeaponConfiguration CurrentWeaponConfig => _playerWeaponService?.CurrentWeaponConfig;
        public float LastFireTime => _playerWeaponService?.LastFireTime ?? 0f;

        public event Action<WeaponFireData> OnWeaponFired
        {
            add => _playerWeaponService.OnWeaponFired += value;
            remove => _playerWeaponService.OnWeaponFired -= value;
        }

        public event Action<WeaponConfiguration> OnWeaponChanged
        {
            add => _playerWeaponService.OnWeaponChanged += value;
            remove => _playerWeaponService.OnWeaponChanged -= value;
        }

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _collector = GetComponentInChildren<PlayerCollector>();
            _assertManager.AssertIsNotNull(_collector, "PlayerCollector Required");
        }

        protected override void CreateWeaponService()
        {
            _playerWeaponService = new PlayerWeaponService(_debugLogger, _audioManager, _poolManager);
            _weaponService = _playerWeaponService;
        }

        public override void Initialize(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;

            // 컬렉터 이벤트 연결
            _collector.OnPickup += _playerWeaponService.HandlePickup;

            // 기본 무기 설정
            if (defaultWeaponConfig != null)
            {
                ChangeWeaponConfiguration(defaultWeaponConfig);
            }

            base.Initialize(inputProvider);
        }

        protected override void OnDestroy()
        {
            if (_collector != null && _playerWeaponService != null)
            {
                _collector.OnPickup -= _playerWeaponService.HandlePickup;
            }

            base.OnDestroy();
        }

        protected override void UpdateWeaponDirection()
        {
            _playerWeaponService?.UpdateWeaponDirection(_entityStatus.FaceDirectionValue);
        }

        protected override bool ShouldFire()
        {
            return _inputProvider?.FirePressed ?? false;
        }

        // IPlayerWeaponService 단순 전달
        public void HandlePickup(Pickup pickup) => _playerWeaponService?.HandlePickup(pickup);

        public void UpdateWeaponDirection(HorizontalDirectionType faceDirection) =>
            _playerWeaponService?.UpdateWeaponDirection(faceDirection);
    }
}