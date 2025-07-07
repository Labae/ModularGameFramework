using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Enemies.Core;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.States
{
    public class EnemyIdleState : EnemyBaseState
    {
        private float _idleTimer;
        public override EnemyStateType StateType => EnemyStateType.Idle;

        public EnemyIdleState(StateMachine<EnemyStateType> stateMachine, IDebugLogger logger, EnemyStatus status, EnemyStateContext context) : base(stateMachine, logger, status, context)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            
            var intent = MovementIntentFactory.CreateIdle();
            _context.Movement.SetMovementIntent(intent);

            _idleTimer = _context.MovementConfig.GetRandomIdleTime();
            
            StateLog("Enemy entered idle state");
        }

        public override void OnUpdate()
        {
            _idleTimer -= Time.deltaTime;
            CheckTransitions();
        }

        public override void OnExit()
        {
            _idleTimer = 0f;
            StateLog("Enemy exited idle state");
            base.OnExit();
        }

        private void CheckTransitions()
        {
            if (_idleTimer < 0)
            {
                _stateMachine.ChangeState(EnemyStateType.Patrol);
                return;
            }
        }
    }
}