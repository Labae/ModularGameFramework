using System;
using MarioGame.Core;
using MarioGame.Core.Enums;
using MarioGame.Gameplay.Extensions;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Misc;
using MarioGame.Gameplay.Player.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Components
{
    [DisallowMultipleComponent]
    public class PlayerWeapon : CoreBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;
        
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _firePosition;
        [SerializeField] private float _fireRate = 0.3f;

        private IInputProvider _inputProvider;
        private PlayerStatus _playerStatus;
        private float _lastFireTime;

        private HorizontalDirectionType _fireDirection;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _playerStatus = GetComponent<PlayerStatus>();
            AssertIsNotNull(_playerStatus, "PlayerStatus Required");
        }

        public void Initialize(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;
            _lastFireTime = 0f;

            _playerStatus.FaceDirection.OnValueChanged += OnFaceDirectionChanged;
            
            AssertIsNotNull(_inputProvider, "IInputProvider Required");
        }

        private void OnDestroy()
        {
            if (_playerStatus != null)
            {
                _playerStatus.FaceDirection.OnValueChanged -= OnFaceDirectionChanged;
            }
        }

        private void Update()
        {
            if (_inputProvider == null || _playerStatus == null)
            {
                return;
            }

            HandleFireInput();
        }

        private void HandleFireInput()
        {
            if (_inputProvider.FirePressed && CanFire())
            {
                Fire();
            }
        }
        
        private bool CanFire()
        {
            if (Time.time - _lastFireTime < _fireRate)
            {
                return false;
            }

            return _playerStatus.CurrentStateValue.CanFire();
        }

        private void Fire()
        {
            var firePos = _firePosition.position;
            
            var bulletObject =  Instantiate(_projectilePrefab, firePos, Quaternion.identity);
            if (bulletObject.TryGetComponent<PlayerBullet>(out var playerBullet))
            {
                playerBullet.Fire(_fireDirection);
            }
            else
            {
                Destroy(bulletObject);
            }
            _lastFireTime = Time.time;
        }

        private void OnFaceDirectionChanged(HorizontalDirectionType direction)
        {
            if (direction == HorizontalDirectionType.Right)
            {
                _pivot.localRotation = Quaternion.identity;
            }
            else
            {
                _pivot.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            _fireDirection = direction;
        }
    }
}