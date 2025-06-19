using System;
using Core.StateMachine;
using Gameplay.MovementIntents;
using UnityEngine;

namespace Core
{
    public enum DummyState
    {
        Idle,
        Walk,
    }

    public class DummyIdleState : BaseState<DummyState>
    {
        private readonly IMovementIntentReceiver _movementIntentReceiver;
        public DummyIdleState(StateMachine<DummyState> stateMachine, IMovementIntentReceiver intentReceiver) : base(stateMachine)
        {
            _movementIntentReceiver = intentReceiver;
        }

        public override DummyState StateType => DummyState.Idle;

        public override void OnEnter()
        {
            _movementIntentReceiver.SetMovementIntent(MovementIntent.None);
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(DummyState.Walk);
            }
        }
    }
    
    public class DummyWalkState : BaseState<DummyState>
    {
        private readonly IMovementIntentReceiver _movementIntentReceiver;
        private MovementIntent _movementIntent;
        public DummyWalkState(StateMachine<DummyState> stateMachine, IMovementIntentReceiver intentReceiver) : base(stateMachine)
        {
            _movementIntentReceiver = intentReceiver;
        }

        public override DummyState StateType => DummyState.Walk;

        public override void OnEnter()
        {
            base.OnEnter();
            _movementIntent = new MovementIntent {
                HorizontalInput = 0f,
                Type = MovementType.Ground,
                SpeedMultiplier = 1.2f
            };
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeState(DummyState.Idle);
            }
            
            var horizontalInput = Input.GetAxisRaw("Horizontal");
            _movementIntent.HorizontalInput = horizontalInput;
            
            _movementIntentReceiver.SetMovementIntent(_movementIntent);
        }
    }
    
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class DummyEntity : MonoBehaviour
    {
        private StateMachine<DummyState> _stateMachine;
        private IMovementIntentReceiver _intentReceiver;

        [Header("Sprites")]
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite _walkSprite;

        private void Awake()
        {
            _spriteRenderer ??= GetComponent<SpriteRenderer>();
            _intentReceiver = GetComponent<IMovementIntentReceiver>();
            
            _stateMachine = new StateMachine<DummyState>();
            _stateMachine.AddStates(
                new DummyIdleState(_stateMachine, _intentReceiver),
                new DummyWalkState(_stateMachine, _intentReceiver)
                );
        }

        private void OnEnable()
        {
            _stateMachine.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            _stateMachine.OnStateChanged -= OnStateChanged;
        }

        private void Start()
        {
            _stateMachine.Start(DummyState.Idle);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
        
        private void OnStateChanged(DummyState previousState, DummyState newState)
        {
            if (newState == DummyState.Idle)
            {
                _spriteRenderer.sprite = _idleSprite;
            }
            else if (newState == DummyState.Walk)
            {
                _spriteRenderer.sprite = _walkSprite;
            }
        }
    }
}