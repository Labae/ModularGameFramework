using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Physics.ProjectileCollision;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    public interface IProjectileBehaviour
    {
        ProjectileType Type { get; }

        bool CanPenetrate(Collider2D target);
        int CalculateDamage(Collider2D target, ProjectileHitData hitData);
        void ProcessHitEffects(ProjectileHitData hitData);
    }
}