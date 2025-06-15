using MarioGame.Core.Entities;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.Core
{
    public class EnemyAnimator : EntityAnimator<EnemyStateType>
    {
        [Header("Enemy Status")]
        [SerializeField] private EnemyStatus _status;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _status = GetComponentInParent<EnemyStatus>();
            AssertIsNotNull(_status, "EnemyStatus required");
        }
        
        private void Start()
        {
            SetupEnemyStatusListeners();
        }

        private void OnDestroy()
        {
            _status.CurrentState.OnValueChanged -= OnStateChanged;
            _status.HorizontalVelocity.OnValueChanged -= UpdateDirection;
        }

        private void SetupEnemyStatusListeners()
        {
            if (_status == null)
            {
                LogError("Failed to setup enemy status listeners");
                return;
            }

            _status.CurrentState.OnValueChanged += OnStateChanged;
            _status.HorizontalVelocity.OnValueChanged += UpdateDirection;
        }
    }
}