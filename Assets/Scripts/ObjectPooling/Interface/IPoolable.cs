using MarioGame.Core.Interfaces;

namespace MarioGame.Core.ObjectPooling.Interface
{
    public interface IPoolable
    {
        bool IsPooled { get; set; }
        IObjectPool OwnerPool { get; set; }
        
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}