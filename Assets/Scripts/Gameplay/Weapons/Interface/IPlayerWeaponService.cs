using System;
using MarioGame.Core.Enums;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Pickups;

namespace MarioGame.Gameplay.Weapons.Interface
{
    public interface IPlayerWeaponService
    {
        WeaponConfiguration CurrentWeaponConfig { get; }
        float LastFireTime { get; }
        
        void HandlePickup(Pickup pickup);
        void UpdateWeaponDirection(HorizontalDirectionType faceDirection);
        
        event Action<WeaponFireData> OnWeaponFired;
        event Action<WeaponConfiguration> OnWeaponChanged;
    }
}