using System;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Config.Data;
using MarioGame.Gameplay.Interfaces.Combat;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IEntityHealth : IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsAlive { get; }
        bool CanTakeDamage { get; }
        bool IsInvincible { get; }
        
        event Action<int> OnHealed;
        event Action<DamageEventData> OnDamageTaken;
        event Action OnDeath;
        
        void Initialize(EntityData data);
        void Heal(int amount);
        void Kill();
    }
}