using Core.StateMachine;
using Gameplay.Enums;
using Gameplay.Interfaces;
using Gameplay.Player.Components;
using Gameplay.Player.States;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gameplay.Player.Core
{
    [DisallowMultipleComponent]
    public class Player : MonoBehaviour
    {
        private StateMachine<PlayerStateType> _stateMachine;
        private IMovementIntentReceiver _intentReceiver;

        [SerializeField]
        private PlayerAnimator _animator;
        private IPlayerWeaponActions _weaponActions;
        private IPlayerInputProvider _inputProvider;

        private void Awake()
        {
            _intentReceiver = GetComponent<IMovementIntentReceiver>();
            _animator ??= GetComponentInChildren<PlayerAnimator>();
            _weaponActions = GetComponentInChildren<IPlayerWeaponActions>();
            _inputProvider = new PlayerInputHandler();

            Assert.IsNotNull(_intentReceiver, "Failed to get IMovementIntentReceiver");
            Assert.IsNotNull(_animator, "Failed to get PlayerAnimator from children");
            Assert.IsNotNull(_weaponActions, "Failed to get IPlayerWeaponActions  from children");

            var stateContext = new PlayerStateContext(_inputProvider, _intentReceiver, _weaponActions);
            
            _stateMachine = new StateMachine<PlayerStateType>();
            _stateMachine.AddStates(
                new PlayerIdleState(_stateMachine.ChangeState, stateContext),
                new PlayerWalkState(_stateMachine.ChangeState, stateContext),
                new PlayerShootState(_stateMachine.ChangeState, stateContext)
                );
        }

        private void Start()
        {
            _animator.Initialize(_stateMachine);
            _stateMachine.Start(PlayerStateType.Idle);
        }

        private void Update()
        {
            _inputProvider.Update();
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
    }
}