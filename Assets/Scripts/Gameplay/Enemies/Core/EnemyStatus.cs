using MarioGame.Core.Entities;
using MarioGame.Core.Enums;
using MarioGame.Core.Reactive;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.Core
{
    public class EnemyStatus : EntityStatus<EnemyStateType>
    {
        [Header("Observable States")] [SerializeField]
        private ObservableProperty<int> _currentHealth = new(0);

        [SerializeField] private ObservableBool _isAlive = new();
        [SerializeField] private ObservableBool _isGrounded = new();
        [SerializeField] private ObservableBool _isMoving = new();
        [SerializeField] private ObservableProperty<float> _horizontalVelocity = new(0f);
        [SerializeField] private ObservableProperty<float> _currentSpeed = new(0f);

        // 의존성들 (수동 주입)
        private IEntityHealth _health;
        private IIntentBasedMovement _movement;
        private IGroundChecker _groundChecker;

        // Public Properties
        public ObservableProperty<int> CurrentHealth => _currentHealth;
        public ObservableBool IsAlive => _isAlive;
        public ObservableBool IsGrounded => _isGrounded;
        public ObservableBool IsMoving => _isMoving;
        public ObservableProperty<float> HorizontalVelocity => _horizontalVelocity;
        public ObservableProperty<float> CurrentSpeed => _currentSpeed;

        public int CurrentHealthValue => _currentHealth.Value;
        public bool IsAliveValue => _isAlive.Value;
        public bool IsGroundedValue => _isGrounded.Value;
        public bool IsMovingValue => _isMoving.Value;
        public float CurrentSpeedValue => _currentSpeed.Value;
        public float HorizontalVelocityValue => _horizontalVelocity.Value;

        /// <summary>
        /// EnemyController에서 의존성을 수동으로 주입
        /// </summary>
        public void InjectDependencies(IEntityHealth health, IIntentBasedMovement movement,
            IGroundChecker groundChecker)
        {
            _health = health;
            _movement = movement;
            _groundChecker = groundChecker;
        }

        protected override void UpdateStates()
        {
            if (_health == null || _movement == null || _groundChecker == null) return;

            _isAlive.Value = _health.IsAlive;
            _currentHealth.Value = _health.CurrentHealth;

            if (HorizontalVelocityValue > FloatUtility.VELOCITY_THRESHOLD)
            {
                _faceDirection.Value = HorizontalDirectionType.Right;
            }
            else if (HorizontalVelocityValue < -FloatUtility.VELOCITY_THRESHOLD)
            {
                _faceDirection.Value = HorizontalDirectionType.Left;
            }

            _isGrounded.Value = _groundChecker.IsGrounded;
            _isMoving.Value = _movement.IsMoving;
            _horizontalVelocity.Value = _movement.HorizontalSpeed;
            _currentSpeed.Value = _movement.CurrentSpeed;
        }

        public override bool CanFire()
        {
            return false;
        }
    }
}