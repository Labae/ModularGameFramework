using MarioGame.Gameplay.Physics.ProjectileCollision;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;

namespace MarioGame.Gameplay.Interfaces.Combat
{
    public interface IInteractable
    {
        void OnInteract(ProjectileHitData hitData);
        bool CanInteract { get; }
    }
}