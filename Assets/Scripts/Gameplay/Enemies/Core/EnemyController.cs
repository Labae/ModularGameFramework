using System.Collections.Generic;
using LDtkUnity;
using MarioGame.Core.Data;
using MarioGame.Core.Entities;
using MarioGame.Core.StateMachine;
using MarioGame.Gameplay.Config.Movement;
using MarioGame.Gameplay.Enemies.States;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Input;
using UnityEngine;

namespace MarioGame.Gameplay.Enemies.Core
{
    [RequireComponent(typeof(EnemyStatus))]
    [RequireComponent(typeof(EnemyMovement))]
    [DisallowMultipleComponent]
    public class EnemyController : Entity
    {
        // StateMachine
        private StateMachine<EnemyStateType> _stateMachine;

        private EnemyStatus _status;

        [SerializeField] private EnemyMovementConfig _config;
        private EnemyMovement _movement;

        private AIInputProvider _inputProvider;
        
        private LDtkFields _ldtkFields;
        
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
            _ldtkFields = GetComponent<LDtkFields>();
            AssertIsNotNull(_status, "EnemyStatus component required");
            AssertIsNotNull(_movement, "EnemyMovement component required");
            AssertIsNotNull(_movement, "LDtkFields component required");
        }
        
        public override void Initialize()
        {
            base.Initialize();
            _movement.Initialize(_config);
            
            _stateMachine.Start(EnemyStateType.Idle);
        }
        
        protected override void HandleDestruction()
        {
            _stateMachine.OnStateChanged -= OnEnemyStateChanged;
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
            _stateMachine = new StateMachine<EnemyStateType>(this);
            var context = new EnemyStateContext(_config, _inputProvider, _movement);
            _stateMachine.AddStates(
                new EnemyIdleState(_stateMachine, this, _status, context),
                new EnemyPatrolState(_stateMachine, this, _status, context, GetPatrolPointsFromLDtk())
            );

            _stateMachine.OnStateChanged += OnEnemyStateChanged;
            DebugLog("StateMachine setup completed");
        }

        private PatrolData GetPatrolPointsFromLDtk()
        {
            var points = new List<Vector2>();
            if (!_ldtkFields.TryGetField("patrol", out var field))
            {
                DebugLogError("Failed to get patrol field");
                return null;
            }

            if (!field.TryGetArray(out var elements))
            {
                DebugLogError("Failed to get array elements");
                return null;
            }
            
            
            foreach (var element in elements)
            {
                points.Add(element.GetPoint());
            }

            points.Add(position2D);
            return new PatrolData(points.ToArray(), _config.PatrolType);
        }
        
        private void OnEnemyStateChanged(EnemyStateType newState, EnemyStateType oldState)
        {
            _status.SetCurrentState(newState);
        }
    }
}