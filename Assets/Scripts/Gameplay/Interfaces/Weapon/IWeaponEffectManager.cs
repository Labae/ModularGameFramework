using System;
using MarioGame.Gameplay.Projectiles;
using MarioGame.Gameplay.Projectiles.HitEffects;
using MarioGame.Gameplay.Weapons;

namespace MarioGame.Gameplay.Interfaces.Weapon
{
    public interface IWeaponEffectManager : IDisposable
    {
        void PlayFireEffect(WeaponFireData fireData);
        void Initialize();
    }
}