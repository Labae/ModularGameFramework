using MarioGame.Core.Interfaces;
using MarioGame.Gameplay.Weapons;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Weapon
{
    public interface IWeaponFireController
    {
        bool CanFire(IFireCondition fireCondition);
        WeaponFireData CreateFireData(Vector2 direction, Vector2 entityPosition);
        void Fire(WeaponFireData fireData);
        void Initialize();
    }
}