using MarioGame.Core.Interfaces;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Player.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Player.States
{
    public class PlayerHurtState : PlayerBaseState
    {
        private float _lastHurtTime;
        
        public override PlayerStateType StateType => PlayerStateType.Hurt;
        
        public PlayerHurtState(StateMachine<PlayerStateType> stateMachine, IDebugLogger logger,
            PlayerStatus status, PlayerStateContext context) : base(stateMachine, logger, status, context)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _lastHurtTime = Time.time;
        }

        public override void OnUpdate()
        {
            CheckTransitions();
        }

        private void CheckTransitions()
        {
            if (Time.time - _lastHurtTime >= 1f)
            {
                StateLog($"{nameof(PlayerHurtState)} hurt : {Time.time - _lastHurtTime}");
                ChangeState(PlayerStateType.Idle);
            }
        }
    }
}