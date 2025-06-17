using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarioGame.Core.Animations
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class SpriteAnimator : CoreBehaviour
    {
        private SpriteRenderer _renderer;

        [Header("Animation Settings")]
        [SerializeField] private List<Sprite> _sprites = new();
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
        
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private Coroutine _fadeOutCoroutine;
        private Coroutine _animationCoroutine;
        private int _currentFrame;
        private int _loopCount = 0;

        public event Action OnAnimationStarted;
        public event Action OnAnimationCompleted;
        public event Action OnAnimationLooped;
        public event Action<int> OnFrameChanged;
        
        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        public int CurrentFrame => _currentFrame;
        public int TotalFrames => _sprites.Count;
        
        public float Progress => TotalFrames > 0 ? (float)_currentFrame / TotalFrames : 0f;
        public int LoopCount => _loopCount;
        public bool IsLastFrame => _currentFrame >= TotalFrames;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.enabled = false;
            _frameRateWait = new WaitForSeconds(1f / _frameRate);
            AssertIsNotNull(_renderer, "SpriteRenderer required");
        }

        private void Start()
        {
            if (_autoStart && TotalFrames > 0)
            {
                PlayAnimation();
            }
        }

        #region SpriteAnimation ScriptableObject 지원 메서드들

        /// <summary>
        /// SpriteAnimation ScriptableObject를 사용한 애니메이션 재생
        /// WeaponConfig에서 호출하는 주요 메서드
        /// </summary>
        public void Play(SpriteAnimation spriteAnimation, Vector2? direction = null)
        {
            if (spriteAnimation == null || !spriteAnimation.IsValid)
            {
                Debug.LogWarning("Invalid SpriteAnimation provided");
                return;
            }

            // SpriteAnimation 설정 적용
            spriteAnimation.ApplyToAnimator(this, direction);
        }

        /// <summary>
        /// 프레임 레이트 설정
        /// </summary>
        public void SetFrameRate(float frameRate)
        {
            _frameRate = Mathf.Max(1f, frameRate);
            _frameRateWait = new WaitForSeconds(1f / _frameRate);
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

        #endregion

        #region 기존 메서드들 (호환성 유지)

        public void Play(List<Sprite> sprites)
        {
            _sprites = sprites;
            ApplyRandomEffects();
            PlayAnimation();
        }

        public void Play(Vector2? direction)
        {
            if (direction.HasValue)
            {
                SetDirection(direction.Value);
            }

            ApplyRandomEffects();
            PlayAnimation();
        }

        private void SetDirection(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return;
            }
            
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        #endregion
        
        private void ApplyRandomEffects()
        {
            if (_randomScale)
            {
                var randomScale = Random.Range(_scaleRange.x, _scaleRange.y);
                transform.localScale = Vector3.one * randomScale;
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
        }

        private IEnumerator AnimationCoroutine()
        {
            do
            {
                for (_currentFrame = 0; _currentFrame < TotalFrames; _currentFrame++)
                {
                    while (_isPaused)
                    {
                        yield return null;
                    }
                    
                    if (_sprites[_currentFrame] != null)
                    {
                        _renderer.sprite = _sprites[_currentFrame];
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
            OnAnimationCompleted?.Invoke();
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
}