using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Effects.DeathEffects;
using MarioGame.Gameplay.Effects.HitEffects;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Effects.Interface
{
    public interface IEffectFactory
    {
        ProjectileEffect CreateProjectileHitEffect(
            Vector2 position,
            WeaponConfiguration config,
            Collider2D hitCollider,
            HitTargetType hitTargetType,
            Vector2 normal = default
        );

        ProjectileEffect CreatePenetrateEffect(
            Vector2 position,
            WeaponConfiguration config,
            Collider2D hitCollider,
            Vector2 normal = default
        );

        EntityDeathEffect CreateEnemyDeathEffect(
            SpriteAnimation deathAnimation,
            Vector2 position,
            float entitySize = 1f
        );
    }
}