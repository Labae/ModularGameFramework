using System;
using System.Collections.Generic;
using Core;
using Core.StateMachine;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gameplay.Components
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class EntityAnimator<T> : CoreBehaviour where T : Enum
    {
        private SpriteRenderer _spriteRenderer;
        protected Animator _animator;

        private T _currentStateType;

        [SerializeField] private string _animationPrefix;
        
        private Dictionary<T, int> _stateAnimationHashMap = new();

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            Assert.IsNotNull(_spriteRenderer, "Failed to get SpriteRenderer");
            Assert.IsNotNull(_animator, "Failed to get Animator");
        }

        public void Initialize(StateMachine<T> stateMachine)
        {
            CacheAnimationHashes();
            
            stateMachine.OnStateChanged += OnStateChanged;
        }

        private void CacheAnimationHashes()
        {
            var stateValues = Enum.GetValues(typeof(T));
            foreach (T state in stateValues)
            {
                var animationName = _animationPrefix + state;
                _stateAnimationHashMap[state] = Animator.StringToHash(animationName);
                Debugging.Logger.Animator($"자동으로 {state} -> {animationName} 매핑되었습니다.");
            }
        }

        protected virtual void OnStateChanged(T oldState, T newState)
        {
            _currentStateType = newState;
            PlayAnimation(newState);
        }

        protected void PlayAnimation(T stateType)
        {
            Assert.IsTrue(_stateAnimationHashMap.TryGetValue(stateType, out var animationHash),
                $"{stateType}이 매핑되지 않았습니다.");
            _animator.Play(animationHash);
        }
    }
}