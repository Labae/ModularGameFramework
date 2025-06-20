using System;
using System.Collections;
using Core;
using UnityEngine;

namespace Animations
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : CoreBehaviour
    {
        [SerializeField] private SpriteAnimation _defaultAnimation;
        
        private SpriteAnimation _currentAnimation;

        private SpriteRenderer _spriteRenderer;
        
        private bool _isPlaying;
        
        private Coroutine _animationCoroutine;
        private WaitForSeconds _animationFrameWait;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (_defaultAnimation == null)
            {
                return;
            }

            if (_defaultAnimation.AutoStart && _defaultAnimation.IsValid())
            {
                PlayAnimation(_defaultAnimation);
            }
        }

        private void PlayAnimation(SpriteAnimation requestedAnimation)
        {
            _currentAnimation = requestedAnimation;
            _animationFrameWait = new WaitForSeconds(1.0f / _currentAnimation.FrameRate);

            if (_isPlaying)
            {
                StopAnimation();
            }

            _isPlaying = true;
            _spriteRenderer.enabled = true;
            _animationCoroutine = StartCoroutine(AnimationCoroutine());
        }

        private void StopAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            _isPlaying = false;
            _spriteRenderer.enabled = false;
        }

        private IEnumerator AnimationCoroutine()
        {
            do
            {
                for (int i = 0; i < _currentAnimation.TotalFrames; i++)
                {
                    if (_currentAnimation.SpriteFrames[i] != null)
                    {
                        _spriteRenderer.sprite = _currentAnimation.SpriteFrames[i];
                    }
                    
                    if (i < _currentAnimation.TotalFrames - 1)
                    {
                        yield return _animationFrameWait;
                    }
                }

            } while (_currentAnimation.Loop);
        }
    }
}