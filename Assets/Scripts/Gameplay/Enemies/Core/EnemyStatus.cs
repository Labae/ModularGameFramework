using MarioGame.Core.Entities;
using MarioGame.Core.Enums;
using MarioGame.Core.Reactive;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Enemies.Components;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Player;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.Core
{
    public class EnemyStatus : EntityStatus<EnemyStateType>
    {
        [Header("Observed Components")]
        [SerializeField]
        private EntityHealth _health;
        [SerializeField]
        private EnemyMovement _movement;
        
        [SerializeField]
        private GroundChecker _groundChecker;
        
        [Header("Observable States")] 
        [SerializeField] private ObservableProperty<int> _currentHealth = new(0);
        [SerializeField] private ObservableBool _isAlive = new();

        [SerializeField]
        private ObservableBool _isGrounded = new();
        
        [SerializeField] private ObservableBool _isMoving = new();

        [SerializeField] private ObservableProperty<float> _horizontalVelocity = new(0f);
        [SerializeField] private ObservableProperty<float> _currentSpeed = new(0f);

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

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _movement = GetComponent<EnemyMovement>();
            _groundChecker = GetComponent<GroundChecker>();
            _health = GetComponent<EntityHealth>();
            
            AssertIsNotNull(_movement, "EnemyMovement component required");
            AssertIsNotNull(_groundChecker, "GroundChecker component required");
            AssertIsNotNull(_health, "EntityHealth component required");
        }
        
        protected override void UpdateStates()
        {
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