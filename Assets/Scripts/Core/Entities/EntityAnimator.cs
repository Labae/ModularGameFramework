using System;
using System.Collections.Generic;
using MarioGame.Core.Enums;
using MarioGame.Core.Utilities;
using UnityEngine;

namespace MarioGame.Core.Entities
{
    /// <summary>
    /// 모든 엔티티의 애니메이션을 관리하는 베이스 클래스
    /// State변화나 Status 변화에 반응해서 애니메이션 업데이트
    /// </summary>
    public abstract class EntityAnimator<TStateType> : CoreBehaviour
        where TStateType : Enum
    {
        [Header("Animation Components")]
        [SerializeField] protected Animator _animator;
        [SerializeField] protected SpriteRenderer _spriteRenderer;

        [Header("Sprite Direction Settings")]
        [SerializeField]
        private HorizontalDirectionType _defaultFaceDirection = HorizontalDirectionType.Right;
        [SerializeField]
        protected bool _flipOnDirectionChanged = true;

        [Header("Animation Settings")] 

        [SerializeField] protected float _defaultAnimationSpeed = 1.0f;
        [SerializeField] protected string _animationPrefix = "";

        protected Dictionary<TStateType, string> _stateAnimationMap = new();
        protected Dictionary<TStateType, int> _stateAnimationHashMap = new();
        
        [SerializeField]
        protected TStateType _currentState;
        
        protected HorizontalDirectionType _currentFaceDireciton = HorizontalDirectionType.Right;

        protected override void Awake()
        {
            base.Awake();
            SetupStateAnimationMappings();
            CacheAnimationHashes();
        }

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            AssertIsNotNull(_animator, "Animator required");
            AssertIsNotNull(_spriteRenderer, "SpriteRenderer required");
            _currentFaceDireciton = _defaultFaceDirection;
        }

        protected virtual void SetupStateAnimationMappings()
        {
            var stateValues = Enum.GetValues(typeof(TStateType));
            foreach (TStateType state in stateValues)
            {
                var animationName = _animationPrefix + state;
                _stateAnimationMap[state] = animationName;
                
                Log($"Auto-mapped: {state} -> {animationName}");
            }
        }

        protected virtual void CacheAnimationHashes()
        {
            foreach (var kvp in _stateAnimationMap)
            {
                _stateAnimationHashMap[kvp.Key] = Animator.StringToHash(kvp.Value);
            }
        }

        protected void MapStateToAnimation(TStateType stateType, string animationName)
        {
            _stateAnimationMap[stateType] = animationName;
        }

        protected virtual void OnStateChanged(TStateType newState)
        {
            _currentState = newState;

            if (_stateAnimationHashMap.TryGetValue(newState, out var animationHash))
            {
                _animator.Play(animationHash);
                Log($"Playing animation for state: {newState}");
            }
            else
            {
                LogWarning($"No animation mapped for state: {newState}");
            }
        }

        public virtual void PlayAnimation(string animationName)
        {
            if (_animator == null)
            {
                return;
            }
            
            _animator.Play(animationName);
        }
        
        public virtual void PlayAnimation(int animationHash)
        {
            if (_animator == null)
            {
                return;
            }
            
            _animator.Play(animationHash);
        }

        public virtual void SetDirection(HorizontalDirectionType directionType)
        {
            if (!_flipOnDirectionChanged || _spriteRenderer == null)
            {
                return;
            }

            if (_currentFaceDireciton == directionType)
            {
                return;
            }
            
            _currentFaceDireciton = directionType;

            var facingRight = (_currentFaceDireciton == HorizontalDirectionType.Right);
            
            if (_defaultFaceDirection == HorizontalDirectionType.Right)
            {
                _spriteRenderer.flipX = !facingRight;
            }
            else
            {
                _spriteRenderer.flipX = facingRight;
            }
        }

        public virtual void UpdateDirection(HorizontalDirectionType directionType)
        {
            SetDirection(directionType);
        }

        public virtual void SetAnimationSpeed(float speed)
        {
            if (_animator == null)
            {
                return;
            }
            
            _animator.speed = speed;
        }
    }
}