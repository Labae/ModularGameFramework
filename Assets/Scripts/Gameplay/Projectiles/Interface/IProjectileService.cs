using System;
using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Projectiles.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles.Interface
{
    public interface IProjectileService
    {
        bool IsActive { get; }
        WeaponConfiguration CurrentConfig { get; }
        Vector2 Direction { get; }
        Vector2 Velocity { get; }
        float RemainingLifetime { get; }
        
        void Initialize(Transform transform, Rigidbody2D rigidbody, SpriteRenderer spriteRenderer, SpriteAnimator animator);
        void Fire(Vector2 startPosition, Vector2 direction, WeaponConfiguration config);
        void Update();
        void ProcessHit(ProjectileHitData hitData, HitTargetType hitType);
        void DestroyProjectile();
        void Reset();
        
        event Action<Vector2, Vector2> OnFired;
        event Action<ProjectileHitData> OnHitTarget;
        event Action<Vector2> OnDestroyed;
        event Action<ProjectileHitData> OnPenetrated;
        event Action OnDestroyRequested;
    }
}