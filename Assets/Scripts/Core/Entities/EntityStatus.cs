using System;
using MarioGame.Core.Enums;
using MarioGame.Core.Interfaces;
using MarioGame.Core.Reactive;
using UnityEngine;

namespace MarioGame.Core.Entities
{
    public abstract class EntityStatus : CoreBehaviour, IFireCondition
    {
        [SerializeField]
        protected ObservableProperty<HorizontalDirectionType> _faceDirection;
        
        public ObservableProperty<HorizontalDirectionType> FaceDirection => _faceDirection;
        public HorizontalDirectionType FaceDirectionValue => _faceDirection.Value;
        
        private void Start()
        {
            SetupEventListeners();
        }
        
        protected virtual void SetupEventListeners()
        {

        }
        
        private void Update()
        {
            UpdateStates();
        }
        
        protected virtual void  UpdateStates()
        {
            
        }
        
        public abstract bool CanFire();
    }
    
    [DisallowMultipleComponent]
    public abstract class EntityStatus<T> : EntityStatus  where T : Enum
    {
        [Header("State Information")] [SerializeField]
        protected ObservableProperty<T> _currentState;
        public ObservableProperty<T> CurrentState => _currentState;
        public T CurrentStateValue => _currentState.Value;
        
        
        public void SetCurrentState(T newState)
        {
            _currentState.Value = newState;
        }
    }
}