using System;
using MarioGame.Audio;
using MarioGame.Core;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Config.Data;
using MarioGame.Gameplay.Interfaces.Combat;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    public class EntityHealth : CoreBehaviour, IDamageable
    {
        private EntityData _data;

        private float _lastDamageTime;
        private int _currentHealth;

        public bool IsAlive => _currentHealth > 0;
        public bool CanTakeDamage => _data.CanTakeDamage && IsAlive && !IsInvincible;

        public event Action<int> OnHealed;
        public event Action<DamageEventData> OnDamageTaken;
        public event Action OnDeath;
        
        public int CurrentHealth => _currentHealth;
        public bool IsInvincible => Time.time - _lastDamageTime < _data.InvincibilityDuration;

        public void Initialize(EntityData data)
        {
            _data = data;
            AssertIsNotNull(_data, "EntityData required");

            if (!_data.HasHealth)
            {
                LogWarning("Entity has no health system.");
                return;
            }

            _currentHealth = _data.MaxHealth;
            _lastDamageTime = _data.InvincibilityDuration;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_data == null)
            {
                AssertIsNotNull(_data, "EntityData required");
                return;
            }
            
            if (!CanTakeDamage)
            {
                Log($"Cannot take damage: Alive:{IsAlive}, Invincible={IsInvincible}");
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damageInfo.Damage);
            _lastDamageTime = Time.time;

            var eventData = new DamageEventData
            {
                DamageInfo = damageInfo,
                RemainingHealth = _currentHealth,
            };

            if (IsAlive)
            {
                if (damageInfo.WasCritical)
                {
                    if (_data.CriticalHitSound != null)
                    {
                        AudioManager.Instance.PlaySFX3D(_data.CriticalHitSound, damageInfo.HitPoint);
                    }
                }
                else
                {
                    if (_data.HitSound != null)
                    {
                        AudioManager.Instance.PlaySFX3D(_data.HitSound, damageInfo.HitPoint);
                    }
                }
                OnDamageTaken?.Invoke(eventData);
            }
            else
            {
                AudioManager.Instance.PlaySFX3D(_data.DeathSound, damageInfo.HitPoint);
                OnDeath?.Invoke();
                Log($"Entity has died");
            }
        }

        public void Heal(int amount)
        {
            if (_data == null)
            {
                AssertIsNotNull(_data, "EntityData required");
                return;
            }
            
            if (!IsAlive)
            {
                return;
            }

            _currentHealth = Mathf.Min(_currentHealth + amount, _data.MaxHealth);
            OnHealed?.Invoke(amount);
        }

        public void Kill()
        {
            if (!IsAlive)
            {
                return;
            }

            _currentHealth = 0;
            OnDeath?.Invoke();
        }
    }
}