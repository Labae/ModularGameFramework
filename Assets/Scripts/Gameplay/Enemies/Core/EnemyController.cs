using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MarioGame.Audio.Interfaces;
using MarioGame.Core.Entities;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Camera.Events;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Components.Detections;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Components.Locomotion;
using MarioGame.Gameplay.Components.Reactions;
using MarioGame.Gameplay.Components.Stats;
using MarioGame.Gameplay.Config.Data;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Config.Reactions;
using MarioGame.Gameplay.Effects.Interface;
using MarioGame.Gameplay.Enemies.States;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Input;
using MarioGame.Level.LDtkImport;
using Reflex.Attributes;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.Core
{
    [RequireComponent(typeof(EnemyStatus))]
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class EnemyController : Entity
    {
        [Header("Enemy Config")] [SerializeField]
        private EntityData _data;

        [SerializeField] private EnemyMovementConfig _movementConfig;
        [SerializeField] private ClimbMovementConfig _climbConfig;
        [SerializeField] private EntityBypassConfig _bypassConfig = new EntityBypassConfig();
        [SerializeField] private EntityHitReactionConfig _hitEffectConfig;
        [SerializeField] private EntityPhysicsReactionConfig _physicsReactionConfig;

        // DI 서비스들
        [Inject] private IEffectFactory _effectFactory;
        [Inject] private IAudioManager _audioManager;

        // 순수 C# 시스템들
        private IEntityHealth _health;
        private IIntentBasedMovement _movement;
        private IEntityJump _jump;
        private IEntityClimb _climb;
        private IEntityBypass _bypass;
        private IEntityHitReaction _hitReaction;
        private IEntityPhysicsReaction _physicsReaction;

        // MonoBehaviour 컴포넌트들
        private EnemyStatus _status;
        private Rigidbody2D _rigidbody2D;
        private GroundChecker _groundChecker;
        private LadderChecker _ladderChecker;
        private SpriteRenderer _spriteRenderer;
        private LDtkImportedEnemy _importedEnemy;

        // 기타
        private StateMachine<EnemyStateType> _stateMachine;
        private AIInputProvider _inputProvider;
        private CancellationTokenSource _destroyCancellationSource;

        protected override void CacheComponents()
        {
            base.CacheComponents();

            _status = GetComponent<EnemyStatus>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _groundChecker = GetComponent<GroundChecker>();
            _ladderChecker = GetComponent<LadderChecker>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _importedEnemy = GetComponent<LDtkImportedEnemy>();

            _assertManager.AssertIsNotNull(_status, "EnemyStatus component required");
            _assertManager.AssertIsNotNull(_rigidbody2D, "Rigidbody2D component required");
            _assertManager.AssertIsNotNull(_groundChecker, "GroundChecker component required");
            _assertManager.AssertIsNotNull(_ladderChecker, "LadderChecker component required");
            _assertManager.AssertIsNotNull(_spriteRenderer, "SpriteRenderer component required");
            _assertManager.AssertIsNotNull(_importedEnemy, "LDtkImportedEnemy component required");
        }

        protected override void Awake()
        {
            base.Awake();
            _destroyCancellationSource = new CancellationTokenSource();
            CreateServices();
            SetupStateMachine();
        }

        private void CreateServices()
        {
            // InputProvider 생성
            _inputProvider = new AIInputProvider();

            // Health 시스템 생성
            _health = new EntityHealth(_audioManager, _debugLogger, _assertManager);

            // Movement 시스템 생성
            _movement = new EnemyMovement(_debugLogger);

            // Jump 시스템 생성 (Enemy용)
            _jump = new EnemyJump(_debugLogger, _groundChecker);

            // Climb 시스템 생성
            _climb = new EnemyClimb(_debugLogger, _ladderChecker, transform);

            // Bypass 시스템 생성
            _bypass = new EntityBypass(_debugLogger, this, _groundChecker, _inputProvider);

            // Hit Effect 시스템 생성
            _hitReaction = new EntityHitReaction(_debugLogger);

            // Physics Reaction 시스템 생성
            _physicsReaction = new EntityPhysicsReaction(_debugLogger, _movement, _groundChecker, _jump, _climb);
        }

        public override void Initialize()
        {
            base.Initialize();

            // 모든 시스템 초기화
            _health.Initialize(_data);
            (_movement as EnemyMovement)?.Initialize(_movementConfig);
            (_jump as EnemyJump)?.Initialize(_movementConfig);
            _climb.Initialize(_climbConfig);
            _bypass.Initialize(_bypassConfig);
            _hitReaction.Initialize(_hitEffectConfig, _spriteRenderer.color);
            _physicsReaction.Initialize(_physicsReactionConfig);

            // 이벤트 구독
            _health.OnDeath += HandleDeath;
            _health.OnDamageTaken += OnDamageTaken;

            // EnemyStatus에 의존성 주입
            _status.InjectDependencies(_health, _movement, _groundChecker);

            // StateMachine 시작
            _stateMachine.Start(EnemyStateType.Idle);
        }

        protected override void HandleDestruction()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= OnEnemyStateChanged;
            }

            if (_health != null)
            {
                _health.OnDeath -= HandleDeath;
                _health.OnDamageTaken -= OnDamageTaken;
            }

            _inputProvider?.Dispose();
            _destroyCancellationSource?.Cancel();
            _destroyCancellationSource?.Dispose();

            base.HandleDestruction();
        }

        private void Update()
        {
            _inputProvider.UpdateInput();
            _stateMachine.Update();

            // 시스템 업데이트
            _bypass?.Update();
        }

        private void FixedUpdate()
        {
            // Movement 업데이트
            _movement?.Update();

            // Jump 업데이트
            _jump?.Update();

            // 물리 업데이트
            _movement?.UpdatePhysics(_rigidbody2D);
            _jump?.UpdatePhysics(_rigidbody2D);
            _climb?.UpdatePhysics(_rigidbody2D);
            _physicsReaction?.UpdatePhysics(_rigidbody2D);

            _stateMachine.FixedUpdate();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            _inputProvider.ResetFrameInputs();

            // Hit Effect 색상 업데이트
            if (_hitReaction != null && _spriteRenderer != null)
            {
                _spriteRenderer.color = _hitReaction.CurrentColor;
            }
        }

        private void SetupStateMachine()
        {
            var patrolData = _importedEnemy.PatrolData;
            _assertManager.AssertIsNotNull(patrolData, "PatrolData component required");

            patrolData.SetPatrolType(_movementConfig.PatrolType);

            _stateMachine = new StateMachine<EnemyStateType>(_debugLogger, _assertManager);
            var context = new EnemyStateContext(_movementConfig, _inputProvider, _movement);
            _stateMachine.AddStates(
                new EnemyIdleState(_stateMachine, _debugLogger, _status, context),
                new EnemyPatrolState(_stateMachine, _debugLogger, _status, context, patrolData)
            );

            _stateMachine.OnStateChanged += OnEnemyStateChanged;
            _debugLogger.Entity("StateMachine setup completed");
        }

        private void HandleDeath()
        {
            if (_data.DeathEffectAnimation != null && _effectFactory != null)
            {
                _effectFactory.CreateEnemyDeathEffect(_data.DeathEffectAnimation, transform.position,
                    transform.localScale.x);
            }
            
            CameraEventGenerator.Shake(ShakeData.Explosion(0.4f, 0.25f));
            DestroyEntity();
        }

        private void OnDamageTaken(DamageEventData eventData)
        {
            // Hit Effect 재생
            _hitReaction?.PlayHitEffectAsync(eventData.DamageInfo.WasCritical,
                _destroyCancellationSource.Token).Forget();

            // Physics Reaction 처리
            if (eventData.DamageInfo.WasCritical && _physicsReaction != null)
            {
                var knockbackData = _physicsReactionConfig.CriticalKnockbackData;
                knockbackData.Direction = -eventData.DamageInfo.DamageDirection;
                    
                _physicsReaction.ApplyKnockbackAsync(knockbackData, _destroyCancellationSource.Token).Forget();
            }
        }

        private void OnEnemyStateChanged(EnemyStateType newState, EnemyStateType oldState)
        {
            _status.SetCurrentState(newState);
        }

        // Public 인터페이스들
        public IEntityHealth Health => _health;
        public IIntentBasedMovement Movement => _movement;
        public IEntityJump Jump => _jump;
        public IEntityClimb Climb => _climb;
        public IEntityBypass Bypass => _bypass;
        public IEntityHitReaction HitReaction => _hitReaction;
        public IEntityPhysicsReaction PhysicsReaction => _physicsReaction;
    }
}