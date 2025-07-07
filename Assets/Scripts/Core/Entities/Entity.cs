using System;
using MarioGame.Core.Interfaces;
using MarioGame.Core.Utilities;
using UnityEngine;

namespace MarioGame.Core.Entities
{
    /// <summary>
    /// 게임 내 모든 오브젝트의 기본 클래스
    /// </summary>
    public abstract class Entity : CoreBehaviour
    {
        [Header("Entity Settings")] [SerializeField]
        protected string _entityId;
        [SerializeField] protected bool _isActive = true;

        public string EntityId => string.IsNullOrEmpty(_entityId) ? name : _entityId;
        public bool IsActive => _isActive;

        public event System.Action<Entity> OnDestroyed;

        protected override void Awake()
        {
            base.Awake();

            if (string.IsNullOrEmpty(_entityId))
            {
                _entityId = string.Join(" ", GetType().Name, GetInstanceID());
            }
            _debugLogger.Entity($"Entity created: {EntityId}");
        }

        protected void Start()
        {
            Initialize();
        }

        protected virtual void LateUpdate()
        {
            transform.position = PixelPerfectUtility.SnapToPixelGrid(position2D);
        }

        protected virtual void OnDestroy()
        {
            _debugLogger.Entity($"Entity destroying: {EntityId}");
            HandleDestruction();
        }

        public virtual void Initialize()
        {
        }

        public virtual void SetActive(bool active)
        {
            if (_isActive == active)
            {
                return;
            }
            
            _debugLogger.Entity($"Entity active state changed: {EntityId}");
            _isActive = active;
            gameObject.SetActive(active);
        }

        public virtual void DestroyEntity()
        {
            _debugLogger.Entity($"Entity destroy requested: {EntityId}");
            HandleDestruction();
            Destroy(gameObject);
        }

        protected virtual void HandleDestruction()
        {
            _debugLogger.Entity($"Entity destruction handled: {EntityId}");
            OnDestroyed?.Invoke(this);
        }

        #region Assert Methods

        protected override string GetAssertIdentifier()
        {
            return _entityId;
        }

        #endregion
    }
}