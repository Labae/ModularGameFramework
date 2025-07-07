using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Physics.ProjectileCollision;
using MarioGame.Gameplay.Projectiles.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    public interface IProjectileLifecycle
    {
        bool IsActive { get; }

        void Fire(Vector2 startPosition, Vector2 direction, WeaponConfiguration config);
        void UpdateProjectile();
        void ProcessHit(ProjectileHitData hitData, HitTargetType hitType);
        void DestroyProjectile();
    }
}