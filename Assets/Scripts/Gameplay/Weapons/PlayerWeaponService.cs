using MarioGame.Audio.Interfaces;
using MarioGame.Core.Enums;
using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling.Interface;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Pickups;
using MarioGame.Gameplay.Weapons.Interface;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    public class PlayerWeaponService : WeaponService, IPlayerWeaponService
    {
        private float _lastFireTime;

        public WeaponConfiguration CurrentWeaponConfig => CurrentConfig;
        public float LastFireTime => _lastFireTime;

        public PlayerWeaponService(IDebugLogger logger, IAudioManager audioManager, IObjectPoolManager poolManager)
            : base(logger, audioManager, poolManager)
        {
        }

        public void HandlePickup(Pickup pickup)
        {
            if (pickup is WeaponPickup weaponPickup)
            {
                ChangeWeapon(weaponPickup.Config);
            }
        }

        public void UpdateWeaponDirection(HorizontalDirectionType faceDirection)
        {
            var direction = faceDirection == HorizontalDirectionType.Right ? Vector2.right : Vector2.left;
            SetDirection(direction);
        }

        public override bool TryFire(Vector2 entityPosition, IFireCondition fireCondition)
        {
            var result = base.TryFire(entityPosition, fireCondition);
            if (result)
            {
                _lastFireTime = Time.time;
            }
            return result;
        }
    }
}