using MarioGame.Core.Entities;
using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Locomotion
{
    public class EntityBypass : IEntityBypass
    {
        protected readonly IDebugLogger _logger;
        protected readonly Entity _entity;
        protected readonly IGroundChecker _groundChecker;
        protected readonly IInputProvider _inputProvider;
        
        protected EntityBypassConfig _config;
        protected bool _wasPressingDown = false;
        protected float _lastBypassTime = -1f;

        public bool EnableBypass 
        { 
            get => _config?.EnableBypass ?? false;
            set { if (_config != null) _config.EnableBypass = value; }
        }
        
        public bool CanBypass => EnableBypass && _groundChecker?.CanBypass == true && !IsOnCooldown;
        public bool IsOnCooldown => Time.time - _lastBypassTime < (_config?.BypassCooldown ?? 0f);

        public EntityBypass(IDebugLogger logger, Entity entity, IGroundChecker groundChecker, IInputProvider inputProvider)
        {
            _logger = logger;
            _entity = entity;
            _groundChecker = groundChecker;
            _inputProvider = inputProvider;
        }

        public virtual void Initialize(EntityBypassConfig config)
        {
            _config = config;
            _logger?.Entity($"EntityBypass initialized - Enabled: {_config?.EnableBypass}");
        }

        public virtual void Update()
        {
            if (!EnableBypass || _inputProvider == null)
            {
                return;
            }

            CheckBypassInput();
        }

        protected virtual void CheckBypassInput()
        {
            if (!CanBypass)
            {
                _wasPressingDown = false;
                return;
            }
            
            var verticalInput = _inputProvider.VerticalInput;
            if (!FloatUtility.IsInputActive(verticalInput))
            {
                _wasPressingDown = false;
                return;
            }

            var inputThreshold = _config?.InputThreshold ?? 0.5f;
            var isPressingDown = verticalInput < -inputThreshold;

            if (isPressingDown && !_wasPressingDown)
            {
                TryBypass(verticalInput);
            }

            _wasPressingDown = isPressingDown;
        }

        public virtual bool TryBypass(float verticalInput)
        {
            if (!CanBypass)
            {
                return false;
            }

            var success = _groundChecker.CurrentBypassable?.TryBypass(_entity) ?? false;
            
            if (success)
            {
                _lastBypassTime = Time.time;
                _logger?.Entity($"Bypass successful with input: {verticalInput}");
                OnBypassSuccess();
            }
            else
            {
                _logger?.Entity("Bypass failed");
                OnBypassFailed();
            }

            return success;
        }

        public virtual bool ForceBypass()
        {
            if (!EnableBypass || _groundChecker?.CanBypass != true)
            {
                return false;
            }

            var success = _groundChecker.CurrentBypassable?.TryBypass(_entity) ?? false;
            
            if (success)
            {
                _lastBypassTime = Time.time;
                _logger?.Entity("Force bypass successful");
                OnBypassSuccess();
            }

            return success;
        }

        // 하위 클래스에서 오버라이드할 이벤트 메서드들
        protected virtual void OnBypassSuccess() { }
        protected virtual void OnBypassFailed() { }
    }
}