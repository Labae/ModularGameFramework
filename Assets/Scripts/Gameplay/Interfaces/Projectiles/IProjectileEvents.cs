using System;
using MarioGame.Gameplay.Physics.ProjectileCollision;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    public interface IProjectileEvents
    {
        event Action<Vector2, Vector2> OnFired;
        event Action<ProjectileHitData> OnHitTarget;
        event Action<Vector2> OnDestroyed;
        
        /// <summary>
        /// 관통 발생 시
        /// </summary>
        event Action<ProjectileHitData> OnPenetrated;
    }
}