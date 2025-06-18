using System;
using MarioGame.Audio;
using MarioGame.Core;
using MarioGame.Core.Entities;
using MarioGame.Core.Interfaces;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Interfaces.Weapon;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    [DisallowMultipleComponent]
    public abstract class EntityWeapon<TStatus> : CoreBehaviour
        where TStatus : EntityStatus, IFireCondition
    {
        [Header("Weapon Configuration")] [SerializeField]
        protected WeaponConfiguration _weaponConfig;
        
        [Header("Fire Settings")]
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _firePosition;

        protected IWeaponDirectionController _directionController;
        protected IWeaponFireController _fireController;
        protected IWeaponEffectManager _effectManager;

        protected TStatus _entityStatus;

        protected bool _isInitialized = false;
        
        public event Action OnFired;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _entityStatus = GetComponent<TStatus>();
            AssertIsNotNull(_entityStatus, $"{typeof(TStatus)} Required");
            AssertIsNotNull(_weaponConfig, "WeaponConfiguration Required");
        }

        public virtual void Initialize(IInputProvider inputProvider)
        {
            InitializeWeaponComponents();
        }

        protected virtual void OnDestroy()
        {
            _directionController?.Dispose();
            _effectManager?.Dispose();
        }

        /// <summary>
        /// 공통 무기 컴포넌트 초기화
        /// </summary>
        protected virtual void InitializeWeaponComponents()
        {
            _directionController = CreateDirectionController();
            _fireController = CreateFireController();
            _effectManager = CreateEffectManager();
            
            // 컴포넌트들 초기화
            _directionController?.Initialize();
            _fireController?.Initialize();
            
            _isInitialized = true;
        }

        protected virtual IWeaponEffectManager CreateEffectManager()
        {
            return new WeaponEffectManager(_weaponConfig);
        }

        /// <summary>
        /// 방향 컨트롤러 생성 (하위 클래스에서 오버라이드 가능)
        /// </summary>
        protected virtual IWeaponDirectionController CreateDirectionController()
        {
            return new WeaponDirectionController(_pivot);
        }

        /// <summary>
        /// 발사 컨트롤러 생성 (하위 클래스에서 오버라이드 가능)
        /// </summary>
        protected virtual IWeaponFireController CreateFireController()
        {
            return new WeaponFireController(_weaponConfig, _firePosition);
        }

        protected virtual void Update()
        {
            if (!_isInitialized || _entityStatus == null)
            {
                return;
            }
            
            UpdateWeaponDirection();
            
            if (ShouldFire() && CanFire())
            {
                Fire();
            }
        }
        
        protected abstract void UpdateWeaponDirection();

        protected virtual void Fire()
        {
            if (_fireController == null || _directionController == null)
            {
                return;
            }

            var fireData = _fireController.CreateFireData(_directionController.CurrentDirection,
                position2D);
            _fireController.Fire(fireData);

            OnFireExecuted(fireData);
        }

        protected virtual void OnFireExecuted(WeaponFireData fireData)
        {
            _effectManager.PlayFireEffect(fireData);
            OnFired?.Invoke();
        }

        public virtual void SetWeaponDirection(Vector2 direction)
        {
            _directionController.SetDirection(direction);
        }

        protected void ChangeWeaponConfiguration(WeaponConfiguration configuration)
        {
            _weaponConfig = configuration;

            if (_isInitialized)
            {
                _fireController = CreateFireController();
                _directionController = CreateDirectionController();
                _effectManager = CreateEffectManager();

                _fireController?.Initialize();
                _directionController?.Initialize();
            }
            
            AudioManager.Instance.PlaySFX(_weaponConfig.EquipSound);
        }

        protected virtual bool CanFire()
        {
            return _entityStatus.CanFire();
        }
        
        protected abstract bool ShouldFire();
    }
}