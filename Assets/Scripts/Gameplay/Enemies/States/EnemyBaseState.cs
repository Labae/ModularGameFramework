using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Enemies.Core;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.MovementIntents;

namespace MarioGame.Gameplay.Enemies.States
{
    public abstract class EnemyBaseState : BaseState<EnemyStateType>
    {
        protected readonly EnemyStatus _status;
        protected readonly EnemyStateContext _context;
        
        protected EnemyBaseState(StateMachine<EnemyStateType> stateMachine,
            IDebugLogger logger, EnemyStatus status, EnemyStateContext context) : base(stateMachine, logger)
        {
            _status = status;
            _context = context;
        }

        protected void StopMovement()
        {
                _context.InputProvider
                    .SetMoveDirection(0);

                var idleIntent = MovementIntentFactory.CreateIdle();
                _context.IntentReceiver.SetMovementIntent(idleIntent);
        }
    }
}