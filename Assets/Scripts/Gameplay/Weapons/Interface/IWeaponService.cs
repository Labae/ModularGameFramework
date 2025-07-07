using System;
using MarioGame.Core.Interfaces;
using MarioGame.Gameplay.Config.Weapon;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons.Interface
{
    public interface IWeaponService
    {
        WeaponConfiguration CurrentConfig { get; }
        Vector2 CurrentDirection { get; }
        
        void Initialize(WeaponConfiguration config, Transform firePosition, Transform pivot);
        void Update();
        void SetDirection(Vector2 direction);
        void ChangeWeapon(WeaponConfiguration newConfig);
        bool TryFire(Vector2 entityPosition, IFireCondition fireCondition);
        
        event Action<WeaponFireData> OnWeaponFired;
        event Action<WeaponConfiguration> OnWeaponChanged;
    }
}