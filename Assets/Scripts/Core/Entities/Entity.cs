using System;
using MarioGame.Core.Interfaces;
using MarioGame.Core.Utilities;
using UnityEngine;

namespace MarioGame.Core.Entities
{
    /// <summary>
    /// 게임 내 모든 오브젝트의 기본 클래스
    /// </summary>
    public abstract class Entity : CoreBehaviour, IDebugLogger
    {
        [Header("Entity Settings")] [SerializeField]
        protected string _entityId;
        [SerializeField] protected bool _isActive = true;

        [Header("Debug")] [SerializeField] protected bool _enableDebugLogs = false;

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
            DebugLog("Entity created:", EntityId);
        }

        protected void Start()
        {
            DebugLog("Entity starting:", EntityId);
            Initialize();
            DebugLog("Entity initialized:", EntityId);
        }

        protected virtual void LateUpdate()
        {
            transform.position = PixelPerfectUtility.SnapToPixelGrid(position2D);
        }

        protected virtual void OnDestroy()
        {
            DebugLog("Entity destroying:", EntityId);
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
            
            DebugLog("Entity active state changed:", EntityId);
            _isActive = active;
            gameObject.SetActive(active);
        }

        public virtual void DestroyEntity()
        {
            DebugLog("Entity destroy requested:", EntityId);
            HandleDestruction();
            Destroy(gameObject);
        }

        protected virtual void HandleDestruction()
        {
            DebugLog("Entity destruction handled:", EntityId);
            OnDestroyed?.Invoke(this);
        }

        #region Log Methods

        public bool EnableDebugLogs => _enableDebugLogs;

        /// <summary>
        /// Entity별 디버그 로그 출력
        /// </summary>
        /// <param name="messages"></param>
        public void DebugLog(params object[] messages)
        {
            if (_enableDebugLogs)
            {
                var fullMessage = new object[messages.Length + 1];
                fullMessage[0] = $"[[{GetType().Name}]:]";
                Array.Copy(messages, 0, fullMessage, 1, messages.Length);
                
                Log(fullMessage);
            }
        }
        
        /// <summary>
        /// Entity별 경고 로그 출력
        /// </summary>
        /// <param name="messages"></param>
        public void DebugLogWarning(params object[] messages)
        {
            if (_enableDebugLogs)
            {
                var fullMessage = new object[messages.Length + 1];
                fullMessage[0] = $"[[{GetType().Name}]:]";
                Array.Copy(messages, 0, fullMessage, 1, messages.Length);

                
                LogWarning(fullMessage);
            }
        }
        
        /// <summary>
        /// Entity별 에러 로그 출력
        /// </summary>
        /// <param name="messages"></param>
        public void DebugLogError(params object[] messages)
        {
            if (_enableDebugLogs)
            {
                var fullMessage = new object[messages.Length + 1];
                fullMessage[0] = $"[[{GetType().Name}]:]";
                Array.Copy(messages, 0, fullMessage, 1, messages.Length);

                LogError(fullMessage);
            }
        }
        
        #endregion

        #region Assert Methods

        protected override string GetAssertIdentifier()
        {
            return _entityId;
        }

        #endregion
    }
}