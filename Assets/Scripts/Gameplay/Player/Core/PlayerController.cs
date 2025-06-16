using MarioGame.Core.Entities;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Config.Input;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Input;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Player.Components;
using MarioGame.Gameplay.Player.States;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Core
{
    [RequireComponent(typeof(PlayerStatus))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerJump))]
    [DisallowMultipleComponent]
    public class PlayerController : Entity
    {
        // StateMachine
        private StateMachine<PlayerStateType> _stateMachine;

        private PlayerStatus _status;

        [SerializeField] private PlayerMovementConfig _movementConfig;
        private PlayerMovement _movement;
        private PlayerJump _playerJump;
        private PlayerClimb _playerClimb;
        private EntityBypass _entityBypass;
        private PlayerWeapon _playerWeapon;

        [SerializeField] private PlayerInputReader _playerInputReader;
        private IInputProvider _inputProvider;

        protected override void Awake()
        {
            base.Awake();
            SetupStateMachine();
        }

        protected override void CacheComponents()
        {
            base.CacheComponents();

            AssertIsNotNull(_playerInputReader, "PlayerInputReader required");

            _status = GetComponent<PlayerStatus>();
            _inputProvider = new PlayerInputProvider(_playerInputReader);
            _movement = GetComponent<PlayerMovement>();
            _playerJump = GetComponent<PlayerJump>();
            _playerClimb = GetComponent<PlayerClimb>();
            _entityBypass = GetComponent<EntityBypass>();
            _playerWeapon = GetComponent<PlayerWeapon>();
            AssertIsNotNull(_status, "PlayerStatus component required");
            AssertIsNotNull(_movement, "PlayerMovement component required");
            AssertIsNotNull(_playerJump, "PlayerJump component required");
            AssertIsNotNull(_playerClimb, "PlayerClimb component required");
            AssertIsNotNull(_entityBypass, "EntityBypass component required");
            AssertIsNotNull(_playerWeapon, "PlayerWeapon component required");
        }

        public override void Initialize()
        {
            base.Initialize();
            _movement.Initialize(_movementConfig);
            _playerJump.Initialize(_movementConfig);
            _playerClimb.Initialize(_movementConfig.ClimbConfig);
            _entityBypass.Initialize(_inputProvider);
            _playerWeapon.Initialize(_inputProvider);
            _stateMachine.Start(PlayerStateType.Idle);
        }

        protected override void HandleDestruction()
        {
            _stateMachine.OnStateChanged -= OnPlayerStateChanged;
            _inputProvider.Dispose();
            base.HandleDestruction();
        }

        private void Update()
        {
            _inputProvider.UpdateInput();
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            _inputProvider.ResetFrameInputs();
        }

        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine<PlayerStateType>(this);
            var context = new PlayerStateContext(
                _movementConfig,
                _inputProvider,
                _movement,
                _playerJump,
                _playerClimb);

            _stateMachine.AddStates(
                new PlayerIdleState(_stateMachine, this, _status, context),
                new PlayerRunState(_stateMachine, this, _status, context),
                new PlayerJumpState(_stateMachine, this, _status, context),
                new PlayerFallState(_stateMachine, this, _status, context),
                new PlayerCrouchState(_stateMachine, this, _status, context),
                new PlayerClimbState(_stateMachine, this, _status, context)
            );

            _stateMachine.OnStateChanged += OnPlayerStateChanged;
            DebugLog("StateMachine setup completed");
        }

        private void OnPlayerStateChanged(PlayerStateType newState, PlayerStateType oldState)
        {
            _status.SetCurrentState(newState);
        }
    }
}