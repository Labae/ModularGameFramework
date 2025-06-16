using System;
using MarioGame.Core.Enums;
using MarioGame.Core.Reactive;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarioGame.Core.Entities
{
    [DisallowMultipleComponent]
    public abstract class EntityStatus<T> : CoreBehaviour where T : Enum
    {
        [Header("State Information")] [SerializeField]
        protected ObservableProperty<T> _currentState;
        public ObservableProperty<T> CurrentState => _currentState;
        public T CurrentStateValue => _currentState.Value;
        
        [SerializeField]
        protected ObservableProperty<HorizontalDirectionType> _faceDirection;
        
        public ObservableProperty<HorizontalDirectionType> FaceDirection => _faceDirection;
        public HorizontalDirectionType FaceDirectionValue => _faceDirection.Value;
        
        private void Start()
        {
            SetupEventListeners();
        }
        
        private void Update()
        {
            UpdateStates();
        }

        protected void OnDestroy()
        {
            
        }

        protected virtual void SetupEventListeners()
        {

        }
        
        public void SetCurrentState(T newState)
        {
            _currentState.Value = newState;
        }

        protected virtual void  UpdateStates()
        {
            
        }
    }
}