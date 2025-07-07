using System;
using System.Collections;
using MarioGame.Audio;
using MarioGame.Audio.Interfaces;
using MarioGame.Core;
using MarioGame.Debugging.Interfaces;
using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarioGame.Gameplay.Animations
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public class SpriteAnimator : CoreBehaviour
    {
        private SpriteRenderer _renderer;

        [Header("Animation Settings")]
        [SerializeField] private SpriteAnimation _currentSpriteAnimation;
        [SerializeField] private float _frameRate = 12.0f;
        [SerializeField] private bool _loop = false;
        [SerializeField] private bool _autoStart = false;

        [Header("Visual Settings")] 
        [SerializeField] private bool _randomScale = false;
        [SerializeField] private Vector2 _scaleRange = new Vector2(0.8f, 1.2f);
        [SerializeField] private bool _randomRotation = false;
        [SerializeField] private Vector2 _rotationRange = new Vector2(0f, 360f);
        [SerializeField] private bool _fadeOut = false;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        
        private WaitForSeconds _frameRateWait;
        
        [Inject]
        private IAudioManager _audioManager;
        
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private Coroutine _fadeOutCoroutine;
        private Coroutine _animationCoroutine;
        private int _currentFrame;
        private int _loopCount = 0;
        private float _scaleMultiplier = 1f;

        public event Action OnAnimationStarted;
        public event Action OnAnimationCompleted;
        public event Action OnAnimationLooped;
        public event Action<int> OnFrameChanged;
        
        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        public int CurrentFrame => _currentFrame;
        public int TotalFrames => _currentSpriteAnimation.sprites.Count;
        
        public float Progress => TotalFrames > 0 ? (float)_currentFrame / TotalFrames : 0f;
        public int LoopCount => _loopCount;
        public bool IsLastFrame => _currentFrame >= TotalFrames;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.enabled = false;
            _frameRateWait = new WaitForSeconds(1f / _frameRate);
            _assertManager.AssertIsNotNull(_renderer, "SpriteRenderer required");
        }

        private void Start()
        {
            if (_autoStart && TotalFrames > 0)
            {
                PlayAnimation();
            }
        }

        #region SpriteAnimation ScriptableObject 지원 메서드들

        public void Play(SpriteAnimation spriteAnimation, Vector2? direction = null)
        {
            if (spriteAnimation == null || !spriteAnimation.IsValid)
            {
                Debug.LogWarning("Invalid SpriteAnimation provided");
                return;
            }
            _currentSpriteAnimation = spriteAnimation;

            // SpriteAnimation 설정 적용
            spriteAnimation.ApplyToAnimator(this, direction);
            PlayAnimation();
        }
        
        public void Play(Vector2? direction = null)
        {
            if (_currentSpriteAnimation == null || !_currentSpriteAnimation.IsValid)
            {
                Debug.LogWarning("Invalid SpriteAnimation provided");
                return;
            }

            // SpriteAnimation 설정 적용
            _currentSpriteAnimation.ApplyToAnimator(this, direction);
            PlayAnimation();
        }

        /// <summary>
        /// 프레임 레이트 설정
        /// </summary>
        public void SetFrameRate(float frameRate)
        {
            _frameRate = Mathf.Max(1f, frameRate);
            _frameRateWait = new WaitForSeconds(1f / _frameRate);
        }
        
        public void SetAnimation(SpriteAnimation spriteAnim)
        {
            _currentSpriteAnimation = spriteAnim;
        }

        /// <summary>
        /// 루프 설정
        /// </summary>
        public void SetLoop(bool loop)
        {
            _loop = loop;
        }

        /// <summary>
        /// 페이드 아웃 설정
        /// </summary>
        public void SetFadeOut(bool fadeOut, float duration = 0.3f)
        {
            _fadeOut = fadeOut;
            _fadeOutDuration = Mathf.Max(0.1f, duration);
        }

        /// <summary>
        /// 랜덤 효과 설정
        /// </summary>
        public void SetRandomEffects(bool randomScale, Vector2 scaleRange, bool randomRotation, Vector2 rotationRange)
        {
            _randomScale = randomScale;
            _scaleRange = scaleRange;
            _randomRotation = randomRotation;
            _rotationRange = rotationRange;
        }

        /// <summary>
        /// 회전 설정
        /// </summary>
        public void SetRotation(float angle)
        {
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        public void SetScaleMultiplier(float scaleMultiplier)
        {
            _scaleMultiplier = scaleMultiplier;
        }

        #endregion

        private void ApplyRandomEffects()
        {
            if (_randomScale)
            {
                var randomScale = Random.Range(_scaleRange.x, _scaleRange.y);
                transform.localScale = Vector3.one * (randomScale * _scaleMultiplier);
            }

            if (_randomRotation)
            {
                var randomRotation = Random.Range(_rotationRange.x, _rotationRange.y);
                transform.localRotation = Quaternion.AngleAxis(randomRotation, Vector3.forward);
            }
        }

        private void PlayAnimation()
        {
            if (_isPlaying)
            {
                StopAnimation();
            }

            ResetAnimation();

            _isPlaying = true;
            _isPaused = false;
            _renderer.enabled = true;
            ApplyRandomEffects();
            _animationCoroutine = StartCoroutine(AnimationCoroutine());
            OnAnimationStarted?.Invoke();
        }

        public void PauseAnimation()
        {
            if (_isPlaying && !_isPaused)
            {
                _isPaused = true;
            }
        }

        public void ResumeAnimation()
        {
            if (_isPlaying && _isPaused)
            {
                _isPaused = false;
            }
        }

        public void ResetAnimation()
        {
            _currentFrame = 0;
            _loopCount = 0;

            if (_renderer != null)
            {
                var color = _renderer.color;
                color.a = 1f;
                _renderer.color = color;
            }
        }
        
        public void StopAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            if (_fadeOutCoroutine != null)
            {
                StopCoroutine(_fadeOutCoroutine);
                _fadeOutCoroutine = null;
            }
            
            _isPlaying = false;
            _isPaused = false;
            _renderer.enabled = false;
            _scaleMultiplier = 1f;
        }

        private IEnumerator AnimationCoroutine()
        {
            if (_currentSpriteAnimation.AnimationSFX != null)
            {
                _audioManager.PlaySFX3D(_currentSpriteAnimation.AnimationSFX, position3D);
            }

            do
            {
                for (_currentFrame = 0; _currentFrame < TotalFrames; _currentFrame++)
                {
                    while (_isPaused)
                    {
                        yield return null;
                    }
                    
                    if (_currentSpriteAnimation.sprites[_currentFrame] != null)
                    {
                        _renderer.sprite = _currentSpriteAnimation.sprites[_currentFrame];
                        OnFrameChanged?.Invoke(_currentFrame);
                    }

                    if (_currentFrame < TotalFrames - 1)
                    {
                        yield return _frameRateWait;
                    }
                }

                if (_loop)
                {
                    _loopCount++;
                    OnAnimationLooped?.Invoke();
                    
                    yield return _frameRateWait;
                }
                
            } while (_loop);

            HandleAnimationComplete();
        }
        
        private void HandleAnimationComplete()
        {
            _animationCoroutine = null;
            _isPlaying = false;
            _isPaused = false;
            

            if (_fadeOut)
            {
                _fadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
            }
            else
            {
                HandleCompletion();
            }
        }

        private IEnumerator FadeOutCoroutine()
        {
            var elapsedTime = 0f;
            Color originalColor = _renderer.color;

            while (elapsedTime < _fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / _fadeOutDuration);
                var color = originalColor;
                color.a = alpha;
                _renderer.color = color;
                
                yield return null;
            }
            
            _fadeOutCoroutine = null;
            HandleCompletion();
        }

        private void HandleCompletion()
        {
            _renderer.enabled = false;
            _scaleMultiplier = 1f;
            OnAnimationCompleted?.Invoke();
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
}