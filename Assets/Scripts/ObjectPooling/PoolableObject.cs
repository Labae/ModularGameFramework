using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling.Interface;
using Reflex.Attributes;
using UnityEngine;

namespace MarioGame.Core.ObjectPooling
{
    public abstract class PoolableObject : CoreBehaviour, IPoolable
    {
        [Header("Pooling")] [SerializeField] private bool _isPooled;

        [Inject]
        private IObjectPoolManager _poolManager;
        
        public bool IsPooled
        {
            get => _isPooled;
            set => _isPooled = value;
        }
        public IObjectPool OwnerPool { get; set; }
        public virtual void OnSpawnFromPool()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnReturnToPool()
        {
            gameObject.SetActive(false);
        }

        protected void ReturnToPool()
        {
            if (_isPooled && OwnerPool != null)
            {
                OwnerPool.ReturnToPool(this);
            }
            else if (_isPooled)
            {
                _poolManager.Return(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}