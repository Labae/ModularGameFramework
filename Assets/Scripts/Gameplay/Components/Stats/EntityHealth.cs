using System;
using MarioGame.Audio.Interfaces;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Data;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Stats
{
    public class EntityHealth : IEntityHealth
    {
        private readonly IAudioManager _audioManager;
        private readonly IDebugLogger _logger;
        private readonly IAssertManager _assertManager;
        
        private EntityData _data;
        private float _lastDamageTime;
        private int _currentHealth;

        public bool IsAlive => _currentHealth > 0;
        public bool CanTakeDamage => _data?.CanTakeDamage == true && IsAlive && !IsInvincible;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _data?.MaxHealth ?? 0;
        public bool IsInvincible => Time.time - _lastDamageTime < (_data?.InvincibilityDuration ?? 0);

        public event Action<int> OnHealed;
        public event Action<DamageEventData> OnDamageTaken;
        public event Action OnDeath;

        public EntityHealth(IAudioManager audioManager, IDebugLogger logger, IAssertManager assertManager)
        {
            _audioManager = audioManager;
            _logger = logger;
            _assertManager = assertManager;
        }
        
        public void Initialize(EntityData data)
        {
            _data = data;
            
            if (_data == null)
            {
                _assertManager?.AssertIsNotNull(_data, "EntityData required");
                return;
            }

            if (!_data.HasHealth)
            {
                _logger?.Warning("Entity has no health system.");
                return;
            }

            _currentHealth = _data.MaxHealth;
            _lastDamageTime = -_data.InvincibilityDuration; // 시작 시 무적 상태가 아니도록
            
            _logger?.Entity($"Health initialized: {_currentHealth}/{_data.MaxHealth}");
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_data == null)
            {
                _assertManager?.AssertIsNotNull(_data, "EntityData required");
                return;
            }
            
            if (!CanTakeDamage)
            {
                _logger?.Entity($"Cannot take damage: Alive:{IsAlive}, Invincible={IsInvincible}");
                return;
            }

            var oldHealth = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - damageInfo.Damage);
            _lastDamageTime = Time.time;

            var eventData = new DamageEventData
            {
                DamageInfo = damageInfo,
                RemainingHealth = _currentHealth,
            };

            _logger?.Entity($"Took damage: {damageInfo.Damage}, Health: {_currentHealth}/{_data.MaxHealth}");

            if (IsAlive)
            {
                // 크리티컬 히트 사운드
                if (damageInfo.WasCritical && _data.CriticalHitSound != null)
                {
                    _audioManager?.PlaySFX3D(_data.CriticalHitSound, damageInfo.HitPoint);
                }
                // 일반 히트 사운드
                else if (_data.HitSound != null)
                {
                    _audioManager?.PlaySFX3D(_data.HitSound, damageInfo.HitPoint);
                }
                
                OnDamageTaken?.Invoke(eventData);
            }
            else
            {
                // 죽음 사운드
                if (_data.DeathSound != null)
                {
                    _audioManager?.PlaySFX3D(_data.DeathSound, damageInfo.HitPoint);
                }
                
                OnDeath?.Invoke();
                _logger?.Entity("Entity has died");
            }
        }

        public void Heal(int amount)
        {
            if (_data == null)
            {
                _assertManager?.AssertIsNotNull(_data, "EntityData required");
                return;
            }
            
            if (!IsAlive || amount <= 0)
            {
                return;
            }

            var oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + amount, _data.MaxHealth);
            
            if (_currentHealth > oldHealth)
            {
                OnHealed?.Invoke(amount);
                _logger?.Entity($"Healed: {amount}, Health: {_currentHealth}/{_data.MaxHealth}");
            }
        }

        public void Kill()
        {
            if (!IsAlive)
            {
                return;
            }

            _currentHealth = 0;
            OnDeath?.Invoke();
            _logger?.Entity("Entity was killed");
        }
        
        public void SetHealth(int health)
        {
            if (_data == null) return;
            
            _currentHealth = Mathf.Clamp(health, 0, _data.MaxHealth);
        }

        public void RestoreToFull()
        {
            if (_data == null) return;
            
            var oldHealth = _currentHealth;
            _currentHealth = _data.MaxHealth;
            
            if (_currentHealth > oldHealth)
            {
                OnHealed?.Invoke(_currentHealth - oldHealth);
                _logger?.Entity($"Health restored to full: {_currentHealth}");
            }
        }

        public float GetHealthPercentage()
        {
            if (_data == null) return 0f;
            return (float)_currentHealth / _data.MaxHealth;
        }
    }
}