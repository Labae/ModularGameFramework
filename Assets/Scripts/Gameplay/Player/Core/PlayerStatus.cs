using MarioGame.Core;
using MarioGame.Core.Entities;
using MarioGame.Core.Reactive;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Core
{
    /// <summary>
    /// 플레이어의 모든 상태를 추적하고 관리하는 컴포넌트
    /// 다른 컴포넌트들의 변화를 감지해서 상태 업데이트
    /// </summary>
    public class PlayerStatus : EntityStatus<PlayerStateType>
    {
        [Header("Observed Components")]
        [SerializeField]
        private PlayerMovement _movement;

        [SerializeField]
        private PlayerJump _jump;
        
        [SerializeField]
        private GroundChecker _groundChecker;

        [Header("Observable States")] 
        [SerializeField]
        private ObservableBool _isGrounded = new();
        
        [SerializeField] private ObservableBool _isRising = new();
        [SerializeField] private ObservableBool _isFalling = new();
        [SerializeField] private ObservableBool _isMoving = new();

        [SerializeField] private ObservableProperty<float> _horizontalVelocity = new(0f);
        [SerializeField] private ObservableProperty<float> _verticalVelocity = new(0f);
        [SerializeField] private ObservableProperty<float> _currentSpeed = new(0f);

        
        public ObservableBool IsGrounded => _isGrounded;
        public ObservableBool IsRising => _isRising;
        public ObservableBool IsFalling => _isFalling;
        public ObservableBool IsMoving => _isMoving;
        
        public ObservableProperty<float> HorizontalVelocity => _horizontalVelocity;
        public ObservableProperty<float> VerticalVelocity => _verticalVelocity;
        public ObservableProperty<float> CurrentSpeed => _currentSpeed;
        
        public bool IsGroundedValue => _isGrounded.Value;
        public bool IsRisingValue => _isRising.Value;
        public bool IsFallingValue => _isFalling.Value;
        public bool IsMovingValue => _isMoving.Value;
        
        public float HorizontalVelocityValue => _horizontalVelocity.Value;
        public float VerticalVelocityValue => _verticalVelocity.Value;
        public float CurrentSpeedValue => _currentSpeed.Value;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _movement = GetComponent<PlayerMovement>();
            _jump = GetComponent<PlayerJump>();
            _groundChecker = GetComponent<GroundChecker>();
            
            AssertIsNotNull(_movement, "PlayerMovement component required");
            AssertIsNotNull(_jump, "PlayerJump component required");
            AssertIsNotNull(_groundChecker, "GroundChecker component required");
        }

        protected override void UpdateStates()
        {
            base.UpdateStates();
            _isGrounded.Value = _groundChecker.IsGrounded;
        
            _isRising.Value = _jump.IsRising;
            _isFalling.Value = _jump.IsFalling;

            _isMoving.Value = _movement.IsMoving;
            
            _horizontalVelocity.Value = _movement.HorizontalSpeed;
            _verticalVelocity.Value = _jump.VerticalVelocity;
            _currentSpeed.Value = _movement.CurrentSpeed;
        }
    }
}