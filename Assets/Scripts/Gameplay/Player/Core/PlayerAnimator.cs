using MarioGame.Core.Entities;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Core
{
    public class PlayerAnimator : EntityAnimator<PlayerStateType>
    {
        [Header("Player Status")]
        [SerializeField] private PlayerStatus _playerStatus;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _playerStatus = GetComponentInParent<PlayerStatus>();
            AssertIsNotNull(_playerStatus, "PlayerStatus required");
        }

        private void Start()
        {
            SetupPlayerStatusListeners();
        }

        private void OnDestroy()
        {
            _playerStatus.CurrentState.OnValueChanged -= OnStateChanged;
            _playerStatus.HorizontalVelocity.OnValueChanged -= UpdateDirection;
            _playerStatus.ClimbSpeed.OnValueChanged -= UpdateClimbAnimationSpeed;
            _playerStatus.IsClimbing.OnBecameFalse -= OnIsClimbingBecameFalse;
        }

        private void SetupPlayerStatusListeners()
        {
            if (_playerStatus == null)
            {
                LogError("Failed to setup player status listeners");
                return;
            }

            _playerStatus.CurrentState.OnValueChanged += OnStateChanged;
            _playerStatus.HorizontalVelocity.OnValueChanged += UpdateDirection;
            _playerStatus.ClimbSpeed.OnValueChanged += UpdateClimbAnimationSpeed;
            _playerStatus.IsClimbing.OnBecameFalse += OnIsClimbingBecameFalse;
        }
        
        private void OnIsClimbingBecameFalse()
        {
            SetAnimationSpeed(_defaultAnimationSpeed);
        }
        
        private void UpdateClimbAnimationSpeed(float speed)
        {
            if (!_playerStatus.IsClimbingValue)
            {
                return;
            }
            
            if (FloatUtility.IsVelocityZero(speed))
            {
                SetAnimationSpeed(0);
            }
            else
            {
                SetAnimationSpeed(_defaultAnimationSpeed);
            }
        }
    }
}