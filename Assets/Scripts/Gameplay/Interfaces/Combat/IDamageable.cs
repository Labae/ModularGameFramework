using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Combat
{
    public interface IDamageable
    {
        void TakeDamage(int damage, Vector2 hitPoint, Vector2 hitDirection);
        bool IsAlive { get; }
    }
}