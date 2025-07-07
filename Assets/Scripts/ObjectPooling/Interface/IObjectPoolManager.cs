namespace MarioGame.Core.ObjectPooling.Interface
{
    public interface IObjectPoolManager
    {
        IObjectPool<T> CreatePool<T>(T prefab, int initialSize = -1, int maxSize = -1, bool? autoExpand = null)
            where T : PoolableObject;

        T Get<T>() where T : PoolableObject;
        void Return<T>(T pooledObject) where T : PoolableObject;
        bool TryGetPool<T>(out IObjectPool<T> result) where T : PoolableObject;

        bool TryGetPool<T>(T poolable, out IObjectPool<T> result)
            where T : PoolableObject;

        void ClearAllPools();
    }
}