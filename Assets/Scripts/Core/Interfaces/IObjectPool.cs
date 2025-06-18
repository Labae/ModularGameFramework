using System;
using MarioGame.Core.ObjectPooling;
using UnityEngine;

namespace MarioGame.Core.Interfaces
{
    public interface IObjectPool
    {
        Type PoolType { get; }
        
        int PooledCount { get; }
        int ActiveCount { get; }
        int TotalCount { get; }
        
        void ReturnToPool(IPoolable poolable);

        void ReturnAll();

        void Clear();
        
        void Resize(int newSize);
    }

    public interface IObjectPool<T> : IObjectPool where T : PoolableObject
    {
        T Get();
        void Return(T poolable);
    }
}