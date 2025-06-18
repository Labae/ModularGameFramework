using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Effects.DeathEffects;
using MarioGame.Gameplay.Effects.HitEffects;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Effects
{
    /// <summary>
    /// 이펙트 생성을 담당하는 팩토리
    /// </summary>
    public static class EffectFactory
    {
        public static ProjectileEffect CreateProjectileHitEffect(
            Vector2 position,
            WeaponConfiguration config,
            Collider2D hitCollider,
            HitTargetType hitTargetType,
            Vector2 normal = default
        )
        {
            var data = new ProjectileHitEffectData
            {
                EffectAnimation = config.GetHitAnimation(hitTargetType),
                Position = position,
                WeaponConfig = config,
                HitCollider = hitCollider,
                HitType = hitTargetType,
                IsPenetrateEffect = false,
            };

            return CreateAndInitialize<ProjectileEffect>(data);
        }
        
        public static ProjectileEffect CreatePenetrateEffect(
            Vector2 position,
            WeaponConfiguration config,
            Collider2D hitCollider,
            Vector2 normal = default
        )
        {
            var data = new ProjectileHitEffectData
            {
                EffectAnimation = config.GetPenetrateAnimation(),
                Position = position,
                WeaponConfig = config,
                HitCollider = hitCollider,
                HitType = config.DetermineHitType(hitCollider),
                HitNormal = normal,
                IsPenetrateEffect = true,
            };

            return CreateAndInitialize<ProjectileEffect>(data);
        }

        public static EntityDeathEffect CreateEnemyDeathEffect(
            SpriteAnimation deathAnimation,
            Vector2 position,
            float entitySize = 1f
        )
        {
            var data = new EntityDeathEffectData
            {
                EffectAnimation = deathAnimation,
                Position = position,
                EntitySize = entitySize,
            };
            
            return CreateAndInitialize<EntityDeathEffect>(data);
        }

        public static T CreateAndInitialize<T>(EffectSpawnData data)
            where T : PoolableObject
        {
            var effect = ObjectPoolManager.Instance.Get<T>();

            if (effect is ProjectileEffect projectileEffect && data is ProjectileHitEffectData hitEffectData)
            {
                projectileEffect.transform.position = hitEffectData.Position;
                projectileEffect.transform.rotation = hitEffectData.Rotation;
                projectileEffect.transform.localScale = hitEffectData.Scale;
                projectileEffect.Initialize(hitEffectData);
            }
            else if (effect is EntityDeathEffect deathEffect && data is EntityDeathEffectData entityDeathEffectData)
            {
                deathEffect.transform.position = entityDeathEffectData.Position;
                deathEffect.transform.rotation = entityDeathEffectData.Rotation;
                deathEffect.transform.localScale = entityDeathEffectData.Scale;
                deathEffect.Initialize(entityDeathEffectData);
            }
            
            return effect;
        }
    }
}