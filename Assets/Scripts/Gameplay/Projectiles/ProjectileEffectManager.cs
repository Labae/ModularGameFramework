using System;
using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Projectiles.HitEffects;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles
{
    public class ProjectileEffectManager : IDisposable
    {
        public void CreateEffect<T>(Vector2 position, Vector2 normal, T prefab,
            HitTargetType hitType) where T : ProjectileEffect
        {
            var effect = GetPooledEffect<T>();
            if (effect != null)
            {
                effect.SetPosition(position);
                effect.SetNormal(normal);
                effect.SetHitType(hitType);
            }
        }

        private T GetPooledEffect<T>() where T : Component, IPoolable
        {
            if (!ObjectPoolManager.Instance.TryGetPool<T>(out var pool))
            {
                Debug.LogWarning($"Pool for {typeof(T).Name} not found");
                return null;
            }

            var effect = pool.Get();
            if (effect == null)
            {
                Debug.LogError($"Failed to get {typeof(T).Name} from pool");
            }

            return effect;
        }

        public void Reset()
        {
            // 필요한 리셋 로직
        }

        public void Dispose()
        {
            // 필요한 정리 로직
        }
    }
}