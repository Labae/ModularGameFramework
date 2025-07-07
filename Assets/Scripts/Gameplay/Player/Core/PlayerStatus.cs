using MarioGame.Core;
using MarioGame.Core.Entities;
using MarioGame.Core.Enums;
using MarioGame.Core.Reactive;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Components.Detections;
using MarioGame.Gameplay.Components.Locomotion;
using MarioGame.Gameplay.Components.Stats;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Extensions;
using MarioGame.Gameplay.Player.Components;
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
        private EntityHealth _health;
        [SerializeField]
        private PlayerMovement _movement;

        [SerializeField]
        private PlayerJump _jump;
       
        [SerializeField]
        private PlayerClimb _climb;
        
        [SerializeField]
        private GroundChecker _groundChecker;
        
        [SerializeField]
        private LadderChecker _ladderChecker;

        [Header("Observable States")] 
        [SerializeField] private ObservableProperty<int> _currentHealth = new(0);
        [SerializeField] private ObservableBool _isAlive = new();

        
        [Header("Checks")]
        [SerializeField]
        private ObservableBool _isGrounded = new();
        
        [SerializeField]
        private ObservableBool _isOnLadder = new();
        private ObservableBool _isAtLadderTop = new();
        private ObservableBool _isAtLadderBottom = new();
        
        [Header("Movement")]
        [SerializeField] private ObservableBool _isRising = new();
        [SerializeField] private ObservableBool _isFalling = new();
        [SerializeField] private ObservableBool _isMoving = new();
        [SerializeField] private ObservableBool _isClimbing = new();

        [SerializeField] private ObservableProperty<float> _horizontalVelocity = new(0f);
        [SerializeField] private ObservableProperty<float> _verticalVelocity = new(0f);
        [SerializeField] private ObservableProperty<float> _currentSpeed = new(0f);
        [SerializeField] private ObservableProperty<float> _climbSpeed = new(0f);

        public ObservableProperty<int> CurrentHealth => _currentHealth;
        public ObservableBool IsAlive => _isAlive;
        
        public ObservableBool IsGrounded => _isGrounded;
        public ObservableBool IsRising => _isRising;
        public ObservableBool IsFalling => _isFalling;
        public ObservableBool IsMoving => _isMoving;
        public ObservableBool IsClimbing => _isClimbing;
        
        public ObservableBool IsOnLadder => _isOnLadder;
        public ObservableBool IsAtLadderTop => _isAtLadderTop;
        public ObservableBool IsAtLadderBottom => _isAtLadderBottom;
        
        public ObservableProperty<float> HorizontalVelocity => _horizontalVelocity;
        public ObservableProperty<float> VerticalVelocity => _verticalVelocity;
        public ObservableProperty<float> CurrentSpeed => _currentSpeed;
        public ObservableProperty<float> ClimbSpeed => _climbSpeed;
        
        public int CurrentHealthValue => _currentHealth.Value;
        public bool IsAliveValue => _isAlive.Value;
        
        public bool IsGroundedValue => _isGrounded.Value;
        public bool IsRisingValue => !IsClimbingValue && _isRising.Value;
        public bool IsFallingValue => !IsClimbingValue && _isFalling.Value;
        public bool IsMovingValue => _isMoving.Value;
        public bool IsClimbingValue => _isClimbing.Value;
        
        public bool IsOnLadderValue => _isOnLadder.Value;
        public bool IsAtLadderTopValue => _isAtLadderTop.Value;
        public bool IsAtLadderBottomValue => _isAtLadderBottom.Value;
        
        public float HorizontalVelocityValue => _horizontalVelocity.Value;
        public float VerticalVelocityValue => _verticalVelocity.Value;
        public float CurrentSpeedValue => _currentSpeed.Value;
        public float ClimbSpeedValue => _climbSpeed.Value;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _movement ??= GetComponent<PlayerMovement>();
            _jump ??= GetComponent<PlayerJump>();
            _groundChecker ??= GetComponentInChildren<GroundChecker>();
            _ladderChecker ??= GetComponentInChildren<LadderChecker>();
            _climb ??= GetComponent<PlayerClimb>();
            _health ??= GetComponent<EntityHealth>();
            
            AssertIsNotNull(_movement, "PlayerMovement component required");
            AssertIsNotNull(_jump, "PlayerJump component required");
            AssertIsNotNull(_groundChecker, "GroundChecker component required");
            AssertIsNotNull(_ladderChecker, "LadderChecker component required");
            AssertIsNotNull(_climb, "PlayerClimb component required");
            AssertIsNotNull(_health, "EntityHealth component required");
        }

        protected override void UpdateStates()
        {
            base.UpdateStates();
            
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
        
            _isRising.Value = _jump.IsRising;
            _isFalling.Value = _jump.IsFalling;

            _isMoving.Value = _movement.IsMoving;
            _isClimbing.Value = _climb.IsClimbing;

            _isOnLadder.Value = _ladderChecker.IsOnLadder;
            _isAtLadderTop.Value = _ladderChecker.IsAtLadderTop;
            _isAtLadderBottom.Value = _ladderChecker.IsAtLadderBottom;
            
            _horizontalVelocity.Value = _movement.HorizontalSpeed;
            _verticalVelocity.Value = _jump.VerticalVelocity;
            _currentSpeed.Value = _movement.CurrentSpeed;
            _climbSpeed.Value = _climb.ClimbVelocity;
        }

        public override bool CanFire()
        {
            return CurrentStateValue.CanFire();
        }
    }
}