using System;
using System.Collections.Generic;
using MarioGame.Core.Interfaces;
using UnityEngine;

namespace MarioGame.Core.ObjectPooling
{
    public class ObjectPoolManager : CoreBehaviour
    {
        private static ObjectPoolManager _instance;

        public static ObjectPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ObjectPoolManager");
                    _instance = go.AddComponent<ObjectPoolManager>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        private readonly Dictionary<Type, IObjectPool> _pools = new();

        [Header("Pool Settings")] [SerializeField]
        private int _defaultInitialSize = 10;

        [SerializeField] private int _defaultMaxSize = 100;
        [SerializeField] private bool _defaultAutoExpand = true;

        protected override void Awake()
        {
            base.Awake();
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public IObjectPool<T> CreatePool<T>(T prefab, int initialSize = -1, int maxSize = -1, bool? autoExpand = null)
            where T : Component, IPoolable
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var cachedPool))
            {
                return cachedPool as IObjectPool<T>;
            }

            int poolInitialSize = initialSize >= 0 ? initialSize : _defaultInitialSize;
            int poolMaxSize = maxSize >= 0 ? maxSize : _defaultMaxSize;
            bool poolAutoExpand = autoExpand ?? _defaultAutoExpand;

            var pool = new ObjectPool<T>(prefab, poolInitialSize, poolMaxSize, poolAutoExpand);
            _pools[type] = pool;

            Log($"Create ObjectPool for {type.Name}");
            return pool;
        }

        public T Get<T>() where T : Component, IPoolable
        {
            var type = typeof(T);

            if (_pools.TryGetValue(type, out var cachedPool) && cachedPool is IObjectPool<T> pool)
            {
                return pool.Get();
            }

            LogError($"No pool found for type {type.Name}");
            return null;
        }

        public void Return<T>(T pooledObject) where T : Component, IPoolable
        {
            var type = typeof(T);

            if (_pools.TryGetValue(type, out var cachedPool) && cachedPool is ObjectPool<T> pool)
            {
                pool.Return(pooledObject);
            }

            LogError($"No pool found for type {type.Name}");
        }

        public bool TryGetPool<T>(out IObjectPool<T> result) where T : Component, IPoolable
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var cachedPool) && cachedPool is IObjectPool<T> pool)
            {
                result = pool;
                return true;
            }

            LogError($"No pool found for type {type.Name}");
            result = null;
            return false;
        }

        public bool TryGetPool<T>(T poolable, out IObjectPool<T> result)
            where T : Component, IPoolable
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var cachedPool) && cachedPool is IObjectPool<T> pool)
            {
                result = pool;
                return true;
            }

            LogError($"No pool found for type {type.Name}");
            result = null;
            return false;
        }

        public void ClearAllPools()
        {
            foreach (var kvp in _pools)
            {
                kvp.Value.Clear();
            }

            _pools.Clear();
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}