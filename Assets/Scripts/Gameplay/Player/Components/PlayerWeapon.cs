using MarioGame.Audio;
using MarioGame.Core.Enums;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Interfaces.Weapon;
using MarioGame.Gameplay.Pickups;
using MarioGame.Gameplay.Player.Core;
using MarioGame.Gameplay.Weapons;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Components
{
    /// <summary>
    /// 책임을 분리한 개선된 플레이어 무기 시스템
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerWeapon : EntityWeapon<PlayerStatus>
    {
        private IWeaponInputHandler _inputHandler;
        private PlayerCollector _collector;
        private float _lastFireTime;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _collector = GetComponentInChildren<PlayerCollector>();
            AssertIsNotNull(_collector, "PlayerCollector Required");
        }

        public override void Initialize(IInputProvider inputProvider)
        {
            _inputHandler = new WeaponInputHandler(inputProvider);
            AssertIsNotNull(inputProvider, "IInputProvider Required");
            _collector.OnPickup += HandlePickup;
            base.Initialize(inputProvider);
        }

        protected override void OnDestroy()
        {
            _collector.OnPickup -= HandlePickup;
            base.OnDestroy();
        }

        protected override void UpdateWeaponDirection()
        {
            var direction = _entityStatus.FaceDirectionValue == HorizontalDirectionType.Right ? Vector2.right : Vector2.left;
            _directionController.SetDirection(direction);
        }

        protected override bool ShouldFire()
        {
            return _inputHandler.ShouldFire();
        }

        protected override void OnFireExecuted(WeaponFireData fireData)
        {
            _lastFireTime = Time.time;
            AudioManager.Instance.PlaySFX(_weaponConfig.FireSound);
            base.OnFireExecuted(fireData);
        }

        private void HandlePickup(Pickup pickup)
        {
            if (pickup is WeaponPickup weaponPickup)
            {
                ChangeWeaponConfiguration(weaponPickup.Config);
            }
        }
        
        protected override bool CanFire()
        {
            return base.CanFire() && Time.time - _lastFireTime > _weaponConfig.FireRate;
        }
    }
}