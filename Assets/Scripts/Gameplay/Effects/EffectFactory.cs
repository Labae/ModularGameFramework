using MarioGame.Core.ObjectPooling;
using MarioGame.Core.ObjectPooling.Interface;
using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Effects.DeathEffects;
using MarioGame.Gameplay.Effects.HitEffects;
using MarioGame.Gameplay.Effects.Interface;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Effects
{
    /// <summary>
    /// 이펙트 생성을 담당하는 팩토리
    /// </summary>
    public sealed class EffectFactory : IEffectFactory
    {
        private readonly IObjectPoolManager _poolManager;
        private readonly ILogger _logger;

        public EffectFactory(IObjectPoolManager poolManager, ILogger logger)
        {
            _poolManager = poolManager;
            _logger = logger;
        }
        
        public ProjectileEffect CreateProjectileHitEffect(
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
        
        public ProjectileEffect CreatePenetrateEffect(
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

        public EntityDeathEffect CreateEnemyDeathEffect(
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

        public T CreateAndInitialize<T>(EffectSpawnData data)
            where T : PoolableObject
        {
            var effect = _poolManager.Get<T>();

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