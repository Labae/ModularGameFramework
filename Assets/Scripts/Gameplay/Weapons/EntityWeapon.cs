using System;
using MarioGame.Audio;
using MarioGame.Audio.Interfaces;
using MarioGame.Core;
using MarioGame.Core.Entities;
using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling.Interface;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Interfaces.Weapon;
using MarioGame.Gameplay.Weapons.Interface;
using Reflex.Attributes;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    [DisallowMultipleComponent]
    public abstract class EntityWeapon<TStatus> : CoreBehaviour
        where TStatus : EntityStatus, IFireCondition
    {
        [Header("Weapon Configuration")] [SerializeField]
        protected WeaponConfiguration _weaponConfig;

        [Header("Fire Settings")] [SerializeField]
        private Transform _pivot;

        [SerializeField] private Transform _firePosition;

        // DI 서비스들
        [Inject] protected IAudioManager _audioManager;
        [Inject] protected IObjectPoolManager _poolManager;

        // 순수 C# 서비스
        protected IWeaponService _weaponService;
        protected TStatus _entityStatus;

        protected bool _isInitialized = false;

        public event Action OnFired;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _entityStatus = GetComponent<TStatus>();
            _assertManager.AssertIsNotNull(_entityStatus, $"{typeof(TStatus)} Required");
            _assertManager.AssertIsNotNull(_weaponConfig, "WeaponConfiguration Required");
            _assertManager.AssertIsNotNull(_pivot, "Pivot Transform Required");
            _assertManager.AssertIsNotNull(_firePosition, "Fire Position Transform Required");
        }

        protected override void Awake()
        {
            base.Awake();
            CreateWeaponService();
        }

        public virtual void Initialize(IInputProvider inputProvider)
        {
            if (_weaponConfig != null)
            {
                _weaponService.Initialize(_weaponConfig, _firePosition, _pivot);
                _weaponService.OnWeaponFired += HandleWeaponFired;
                _weaponService.OnWeaponChanged += HandleWeaponChanged;
            }

            _isInitialized = true;
            _debugLogger?.Entity("EntityWeapon initialized");
        }

        protected virtual void OnDestroy()
        {
            if (_weaponService != null)
            {
                _weaponService.OnWeaponFired -= HandleWeaponFired;
                _weaponService.OnWeaponChanged -= HandleWeaponChanged;
            }
        }

        protected virtual void CreateWeaponService()
        {
            _weaponService = new WeaponService(_debugLogger, _audioManager, _poolManager);
        }

        protected virtual void Update()
        {
            if (!_isInitialized || _entityStatus == null)
            {
                return;
            }

            _weaponService?.Update();
            UpdateWeaponDirection();

            if (ShouldFire())
            {
                _weaponService?.TryFire(position2D, _entityStatus);
            }
        }

        protected abstract void UpdateWeaponDirection();
        protected abstract bool ShouldFire();

        public virtual void ChangeWeaponConfiguration(WeaponConfiguration configuration)
        {
            _weaponService?.ChangeWeapon(configuration);
        }

        protected virtual void HandleWeaponFired(WeaponFireData fireData)
        {
            OnFired?.Invoke();
        }

        protected virtual void HandleWeaponChanged(WeaponConfiguration newConfig)
        {
            // 하위 클래스에서 필요시 오버라이드
        }
    }
}