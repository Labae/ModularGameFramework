namespace MarioGame.Core.Interfaces
{
    public interface IPoolable
    {
        bool IsPooled { get; set; }
        IObjectPool OwnerPool { get; set; }
        
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}