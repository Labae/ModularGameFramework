using MarioGame.Core.Data;
using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Enemies.Core;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.States
{
    public class EnemyPatrolState : EnemyBaseState
    {
        private readonly PatrolData _patrolData;
        private MovementIntent _patrolIntent; 

        public override EnemyStateType StateType => EnemyStateType.Patrol;
        public EnemyPatrolState(StateMachine<EnemyStateType> stateMachine, IDebugLogger logger,
            EnemyStatus status, EnemyStateContext context, PatrolData data) : base(stateMachine, logger, status, context)
        {
            _patrolData = data;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            StateLog("Enemy entered patrol state");

            if (!_patrolData.HasPatrolPoint)
            {
                StateLogWarning("No patrol points available, switching to idle state");
                ChangeState(EnemyStateType.Idle);
                return;
            }
            
            _patrolIntent = MovementIntentFactory.CreateGroundMovement(_context.MovementConfig,
                0, _context.MovementConfig.PatrolMultiplier);
            StateLog($"Starting patrol to point: {_patrolData.CurrentPatrolPoint}");
        }

        public override void OnExit()
        {
            _patrolData.MoveToNext();
            StopMovement();
            StateLog("Enemy exited patrol state");
            base.OnExit();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!_patrolData.HasPatrolPoint)
            {
                return;
            }

            CheckTransitions();
        }
        
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            MoveTowardsTarget();
        }

        private void CheckTransitions()
        {
            var currentPosition = _status.position2D;
            var arrivalThreshold = _context.MovementConfig.ArrivalThreshold;

            if (_patrolData.HasReachedTargetX(currentPosition, arrivalThreshold))
            {
                StateLog($"Reached patrol point: {_patrolData.CurrentPatrolPoint}");
                ChangeState(EnemyStateType.Idle);
                return;
            }
        }

        private void MoveTowardsTarget()
        {
            var currentPosition = _status.position2D;
            var targetPosition = _patrolData.CurrentPatrolPoint;
            
            var direction = (targetPosition - currentPosition).normalized.x;
            _context.InputProvider.SetMoveDirection(direction);
            _patrolIntent.HorizontalInput = direction;
            _context.Movement.SetMovementIntent(_patrolIntent);
        }
    }
}