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
        private EnemyMovement _movement;
        
        [SerializeField]
        private GroundChecker _groundChecker;
        
        [Header("Observable States")] 
        [SerializeField]
        private ObservableBool _isGrounded = new();
        
        [SerializeField] private ObservableBool _isMoving = new();

        [SerializeField] private ObservableProperty<float> _horizontalVelocity = new(0f);
        [SerializeField] private ObservableProperty<float> _currentSpeed = new(0f);

        public ObservableBool IsGrounded => _isGrounded;
        public ObservableBool IsMoving => _isMoving;
        public ObservableProperty<float> HorizontalVelocity => _horizontalVelocity;
        public ObservableProperty<float> CurrentSpeed => _currentSpeed;
        public bool IsGroundedValue => _isGrounded.Value;
        public bool IsMovingValue => _isMoving.Value;
        public float CurrentSpeedValue => _currentSpeed.Value;
        public float HorizontalVelocityValue => _horizontalVelocity.Value;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _movement = GetComponent<EnemyMovement>();
            _groundChecker = GetComponent<GroundChecker>();
            
            AssertIsNotNull(_movement, "EnemyMovement component required");
            AssertIsNotNull(_groundChecker, "GroundChecker component required");
        }
        
        protected override void UpdateStates()
        {
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