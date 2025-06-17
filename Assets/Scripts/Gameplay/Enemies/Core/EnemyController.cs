using System.Collections.Generic;
using MarioGame.Core.Data;
using MarioGame.Core.Entities;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Components;
using MarioGame.Gameplay.Config.Data;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enemies.Components;
using MarioGame.Gameplay.Enemies.States;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Input;
using MarioGame.Level.LDtkImport;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.Core
{
    [RequireComponent(typeof(EnemyStatus))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EntityHealth))]
    [DisallowMultipleComponent]
    public class EnemyController : Entity
    {
        [SerializeField] private EntityData _data;
        [SerializeField] private EnemyMovementConfig _config;
        
        // StateMachine
        private StateMachine<EnemyStateType> _stateMachine;

        private EnemyStatus _status;
        private EntityHealth _health;
        private EnemyMovement _movement;

        private AIInputProvider _inputProvider;
        
        private LDtkImportedEnemy _importedEnemy;
        
        protected override void Awake()
        {
            base.Awake();
            SetupStateMachine();
        }

        protected override void CacheComponents()
        {
            base.CacheComponents();
            
            _status = GetComponent<EnemyStatus>();
            _inputProvider = new AIInputProvider();
            _movement = GetComponent<EnemyMovement>();
            _health = GetComponent<EntityHealth>();
            _importedEnemy = GetComponent<LDtkImportedEnemy>();
            AssertIsNotNull(_status, "EnemyStatus component required");
            AssertIsNotNull(_movement, "EnemyMovement component required");
            AssertIsNotNull(_importedEnemy, "LDtkFields component required");
            AssertIsNotNull(_health, "EntityHealth component required");
        }
        
        public override void Initialize()
        {
            base.Initialize();
            _movement.Initialize(_config);
            _health.Initialize(_data);
            _stateMachine.Start(EnemyStateType.Idle);
        }
        
        protected override void HandleDestruction()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= OnEnemyStateChanged;
            }
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
            var patrolData = _importedEnemy.PatrolData;
            AssertIsNotNull(patrolData, "PatrolData component required");
            
            patrolData.SetPatrolType(_config.PatrolType);
            
            _stateMachine = new StateMachine<EnemyStateType>(this);
            var context = new EnemyStateContext(_config, _inputProvider, _movement);
            _stateMachine.AddStates(
                new EnemyIdleState(_stateMachine, this, _status, context),
                new EnemyPatrolState(_stateMachine, this, _status, context, patrolData)
            );

            _stateMachine.OnStateChanged += OnEnemyStateChanged;
            DebugLog("StateMachine setup completed");
        }

       
        
        private void OnEnemyStateChanged(EnemyStateType newState, EnemyStateType oldState)
        {
            _status.SetCurrentState(newState);
        }
    }
}