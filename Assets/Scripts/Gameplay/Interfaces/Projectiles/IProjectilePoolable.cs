using MarioGame.Core.Interfaces;

namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    public interface IProjectilePoolable : IPoolable
    {
        void ResetProjectile();

        void ApplyConfiguration();
    }
}