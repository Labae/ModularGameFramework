using System.Collections;
using System.Collections.Generic;
using MarioGame.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Misc
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class BulletHit : CoreBehaviour
    {
        private SpriteRenderer _renderer;

        [SerializeField] private List<Sprite> _sprites = new();
        [SerializeField] private float _frameRate = 12.0f;
        
        private WaitForSeconds _frameRateWait;
        
        private bool _isPlaying = false;
        private Coroutine _animationCoroutine;
        private int _currentFrame;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.enabled = false;
            _frameRateWait = new WaitForSeconds(1f / _frameRate);
            AssertIsNotNull(_renderer, "SpriteRenderer required");
        }

        public void Play(Vector2 position, Vector2 direction)
        {
            transform.position = position;
            
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Deg2Rad;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            PlayAnimation();
        }

        private void PlayAnimation()
        {
            if (_isPlaying)
            {
                StopAnimation();
            }

            _currentFrame = 0;
            _isPlaying = true;
            _renderer.enabled = true;
            _animationCoroutine = StartCoroutine(AnimationCoroutine());
        }

        private IEnumerator AnimationCoroutine()
        {
            do
            {
                if (_currentFrame >= _sprites.Count)
                {
                    break;
                }

                if (_sprites[_currentFrame] != null)
                {
                    _renderer.sprite = _sprites[_currentFrame];
                }
                _currentFrame++;
                yield return _frameRateWait;
            } while (_currentFrame < _sprites.Count);

            OnAnimationCompleted();
        }

        private void StopAnimation()
        {
            StopAllCoroutines();
            _animationCoroutine = null;
            _isPlaying = false;
            Destroy(gameObject);
        }
        
        private void OnAnimationCompleted()
        {
            _animationCoroutine = null;
            _isPlaying = false;
            Destroy(gameObject);
        }
    }
}