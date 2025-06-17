using System;
using MarioGame.Gameplay.Combat.Data;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        bool CanTakeDamage { get; }

        event Action<int> OnHealed;
        event Action<DamageEventData> OnDamageTaken;
        event Action OnDeath;
        
        void TakeDamage(DamageInfo damageInfo);
    }
}