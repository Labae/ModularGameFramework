using System;
using MarioGame.Core;
using MarioGame.Core.Entities;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    [RequireComponent(typeof(Entity))]
    [RequireComponent(typeof(GroundChecker))]
    [DisallowMultipleComponent]
    public class EntityBypass : CoreBehaviour
    {
        [Header("Bypass Settings")] [SerializeField]
        private bool _enableBypass = true;

        private Entity _entity;
        private GroundChecker _groundChecker;
        private IInputProvider _inputProvider;

        private bool _wasPressingDown = false;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _entity = GetComponent<Entity>();
            _groundChecker = GetComponent<GroundChecker>();

            AssertIsNotNull(_entity, "Entity required");
            AssertIsNotNull(_groundChecker, "GroundChecker required");
        }

        public void Initialize(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;
            AssertIsNotNull(_inputProvider, "IInputProvider required");
        }

        private void Update()
        {
            if (!_enableBypass || _inputProvider == null)
            {
                return;
            }

            CheckBypassInput();
        }

        private void CheckBypassInput()
        {
            var verticalInput = _inputProvider.VerticalInput;
            if (!FloatUtility.IsInputActive(verticalInput))
            {
                return;
            }

            var isPressingDown = verticalInput < -FloatUtility.INPUT_DEADZONE;

            if (isPressingDown && !_wasPressingDown)
            {
                TryBypass(verticalInput);
            }

            _wasPressingDown = isPressingDown;
        }

        private void TryBypass(float verticalInput)
        {
            if (!_groundChecker.CanBypass)
            {
                return;
            }

            var success = _groundChecker.CurrentBypassable.TryBypass(_entity);
            if (success)
            {
                Log("Bypass success");
            }
            else
            {
                Log("Bypass failed");
            }
        }

        public bool ForceBypass()
        {
            if (!_enableBypass || !_groundChecker.CanBypass)
            {
                return false;
            }

            return _groundChecker.CurrentBypassable.TryBypass(_entity);
        }
    }
}