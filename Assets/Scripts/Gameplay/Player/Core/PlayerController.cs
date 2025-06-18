using MarioGame.Core.Entities;
using MarioGame.Core.Extensions;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Camera.Events;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Config.Data;
using MarioGame.Gameplay.Config.Input;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Input;
using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Physics.EntityCollision;
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
        [SerializeField] private EntityData _entityData;
        [SerializeField] private PlayerMovementConfig _movementConfig;
        [SerializeField] private PlayerInputReader _playerInputReader;

        // StateMachine
        private StateMachine<PlayerStateType> _stateMachine;

        private PlayerStatus _status;
        private PlayerMovement _movement;
        private PlayerJump _playerJump;
        private PlayerClimb _playerClimb;
        private EntityBypass _entityBypass;
        private PlayerWeapon _playerWeapon;
        private EntityHealth _health;

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
            _health = GetComponent<EntityHealth>();
            AssertIsNotNull(_status, "PlayerStatus component required");
            AssertIsNotNull(_movement, "PlayerMovement component required");
            AssertIsNotNull(_playerJump, "PlayerJump component required");
            AssertIsNotNull(_playerClimb, "PlayerClimb component required");
            AssertIsNotNull(_entityBypass, "EntityBypass component required");
            AssertIsNotNull(_playerWeapon, "PlayerWeapon component required");
            AssertIsNotNull(_health, "EntityHealth component required");
        }

        public override void Initialize()
        {
            base.Initialize();
            _health.Initialize(_entityData);
            _movement.Initialize(_movementConfig);
            _playerJump.Initialize(_movementConfig);
            _playerClimb.Initialize(_movementConfig.ClimbConfig);
            _entityBypass.Initialize(_inputProvider);
            _playerWeapon.Initialize(_inputProvider);
            _stateMachine.Start(PlayerStateType.Idle);
            
            _health.OnDamageTaken += OnDamageTaken;
            
            CameraEventGenerator.SetTarget(this);
        }

        protected override void HandleDestruction()
        {
            _health.OnDamageTaken -= OnDamageTaken;
            _stateMachine.OnStateChanged -= OnPlayerStateChanged;
            _inputProvider.Dispose();
            base.HandleDestruction();
        }
        
        
        private void OnDamageTaken(DamageEventData obj)
        {
            if (obj.DamageInfo.Damage > 0 && obj.RemainingHealth > 0)
            {
                _stateMachine.ForceChangeState(PlayerStateType.Hurt);
            }

            if (obj.DamageInfo.WasCritical)
            {
                CameraEventGenerator.Shake(ShakeData.Punch( obj.DamageInfo.DamageDirection,0.4f, 0.25f));
            }
        }

        private void Update()
        {
            _inputProvider.UpdateInput();
            _stateMachine.Update();

// 왼쪽 넉백
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
            {
                TestKnockback(Vector2.left, "Left Knockback");
            }

// 오른쪽 넉백  
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                TestKnockback(Vector2.right, "Right Knockback");
            }

// 위쪽 넉백
            if (UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                TestKnockback(Vector2.up, "Upward Knockback");
            }
        }

        private void TestKnockback(Vector2 direction, string testName)
        {
            var hurtBox = GetComponentInChildren<EntityHurtBox>();
            var hitPoint = transform.position.ToVector2()+ direction * -0.5f; // 반대 방향에서 맞음

            hurtBox.TryApplyDamage(1, hitPoint, -direction.normalized);
            Debug.Log($"{testName} applied!");
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
                _playerClimb,
                _playerWeapon);

            _stateMachine.AddStates(
                new PlayerIdleState(_stateMachine, this, _status, context),
                new PlayerRunState(_stateMachine, this, _status, context),
                new PlayerJumpState(_stateMachine, this, _status, context),
                new PlayerFallState(_stateMachine, this, _status, context),
                new PlayerCrouchState(_stateMachine, this, _status, context),
                new PlayerClimbState(_stateMachine, this, _status, context),
                new PlayerIdleShoot(_stateMachine, this, _status, context),
                new PlayerHurtState(_stateMachine, this, _status, context)
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