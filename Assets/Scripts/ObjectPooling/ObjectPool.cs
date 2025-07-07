using System;
using System.Collections.Generic;
using MarioGame.Core.ObjectPooling;
using MarioGame.Core.ObjectPooling.Interface;
using MarioGame.Debugging.Interfaces;
using Reflex.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MarioGame.ObjectPooling
{
    public class ObjectPool<T> : IObjectPool<T> where T : PoolableObject
    {
        private readonly Queue<T> _pool = new();
        private readonly HashSet<T> _activeObjects = new();
        private readonly Transform _poolParent;

        private readonly T _prefab;
        private readonly int _initialSize;
        private readonly int _maxSize;
        private readonly bool _autoExpand;

        public Type PoolType => typeof(T);
        public int PooledCount => _pool.Count;
        public int ActiveCount => _activeObjects.Count;
        public int TotalCount => PooledCount + ActiveCount;
        public bool IsFull => TotalCount >= _maxSize;

        [Inject] private IDebugLogger _debugLogger;

        public ObjectPool(T prefab, int initialSize = 10,
            int maxSize = 100, bool autoExpand = true,
            Transform parent = null)
        {
            _prefab = prefab;
            if (_prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            _initialSize = initialSize;
            _maxSize = maxSize;
            _autoExpand = autoExpand;

            if (parent == null)
            {
                var poolObject = new GameObject($"Pool_{typeof(T).Name}");
                parent = poolObject.transform;
                Object.DontDestroyOnLoad(poolObject);
            }

            _poolParent = parent;

            PrewarmPool();
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                var newObject = CreateNewObject();
                newObject.OnReturnToPool();
                _pool.Enqueue(newObject);
            }
        }

        private T CreateNewObject()
        {
            var newObject = Object.Instantiate(_prefab, _poolParent);
            newObject.IsPooled = true;
            newObject.OwnerPool = this;
            newObject.gameObject.SetActive(false);
            return newObject;
        }

        public T Get()
        {
            T pooledObject;

            if (_pool.Count > 0)
            {
                pooledObject = _pool.Dequeue();
            }
            else if (_autoExpand && !IsFull)
            {
                pooledObject = CreateNewObject();
                _debugLogger.System($"ObjectPool<{typeof(T).Name}> expanded. Total: {TotalCount + 1}");
            }
            else
            {
                _debugLogger.Warning($"ObjectPool<{typeof(T).Name}> is full! Returning null.");
                return null;
            }

            _activeObjects.Add(pooledObject);

            pooledObject.gameObject.SetActive(true);
            pooledObject.OnSpawnFromPool();
            return pooledObject;
        }

        public void Return(T pooledObject)
        {
            if (pooledObject == null)
            {
                _debugLogger.Warning($"Trying to return null object to pool");
                return;
            }

            if (!pooledObject.IsPooled)
            {
                _debugLogger.Warning($"Trying to return non-pooled object to pool: {pooledObject.name}");
                return;
            }

            if (!_activeObjects.Contains(pooledObject))
            {
                _debugLogger.Warning(
                    $"Trying to return object that's not from this object pool: ObjectPool<{typeof(T).Name}>");
                return;
            }

            _activeObjects.Remove(pooledObject);

            pooledObject.OnReturnToPool();
            pooledObject.gameObject.SetActive(false);
            pooledObject.transform.SetParent(_poolParent);

            _pool.Enqueue(pooledObject);
        }

        public void ReturnToPool(IPoolable poolable)
        {
            if (poolable is T typeObject)
            {
                Return(typeObject);
            }
            else
            {
                _debugLogger.Error(
                    $"Cannot return object of type {poolable.GetType().Name} to pool of type {typeof(T).Name}");
            }
        }

        public void ReturnAll()
        {
            var activeList = new List<T>(_activeObjects);
            foreach (var obj in activeList)
            {
                Return(obj);
            }

            _debugLogger.System($"ObjectPool<{typeof(T).Name}> returned all {activeList.Count} active objects");
        }

        public void Clear()
        {
            ReturnAll();

            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }

            _activeObjects.Clear();
            _debugLogger.System($"ObjectPool<{typeof(T).Name}> cleared completely");
        }

        public void Resize(int newSize)
        {
            newSize = Mathf.Max(0, newSize);
            while (_pool.Count > newSize)
            {
                var obj = _pool.Dequeue();
                Object.Destroy(obj);
            }

            while (_pool.Count < newSize && TotalCount < _maxSize)
            {
                var newObject = CreateNewObject();
                newObject.OnReturnToPool();
                _pool.Enqueue(newObject);
            }

            _debugLogger.System($"ObjectPool<{typeof(T).Name}> resized to {_pool.Count} pooled objects");
        }
    }
}