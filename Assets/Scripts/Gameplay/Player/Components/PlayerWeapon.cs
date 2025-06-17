using MarioGame.Core.Enums;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Interfaces.Weapon;
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
        private float _lastFireTime;

        public override void Initialize(IInputProvider inputProvider)
        {
            _inputHandler = new WeaponInputHandler(inputProvider);
            AssertIsNotNull(inputProvider, "IInputProvider Required");
            base.Initialize(inputProvider);
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
            base.OnFireExecuted(fireData);
        }

        protected override bool CanFire()
        {
            return base.CanFire() && Time.time - _lastFireTime > _weaponConfig.FireRate;
        }
    }
}