using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Animations;
using UnityEngine;

namespace MarioGame.Gameplay.Effects
{
    [RequireComponent(typeof(SpriteAnimator))]
    [DisallowMultipleComponent]
    public abstract class BaseEffect<TSpawnData> : PoolableObject
        where TSpawnData : EffectSpawnData
    {
        [Header("Audio")] [SerializeField] protected AudioClip _effectSound;
        [SerializeField] protected bool _playAudioOnSpawn = true;

        protected SpriteAnimator _animator;
        protected TSpawnData _effectData;
        protected bool _effectCompleted;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _animator = GetComponent<SpriteAnimator>();
            _assertManager.AssertIsNotNull(_animator, "SpriteAnimator required");
        }

        public void Initialize(TSpawnData spawnData)
        {
            _effectData = spawnData;
            
            transform.position = _effectData.Position;
            transform.rotation = _effectData.Rotation;
            transform.localScale = _effectData.Scale;

            OnInitialize(spawnData);
            PlayEffect();
        }
        
        protected abstract void OnInitialize(TSpawnData spawnData);

        /// <summary>
        /// 이펙트 재생 시작
        /// </summary>
        protected void PlayEffect()
        {
            _effectCompleted = false;

            // SpriteAnimator 이벤트 구독
            _animator.OnAnimationCompleted += OnEffectCompleted;

            _animator.Play();

            // 사운드 재생
            if (_playAudioOnSpawn && _effectSound != null)
            {
                PlayAudio();
            }

            // 하위 클래스 커스텀 로직
            OnEffectStarted();
        }

        protected virtual void PlayAudio()
        {
            if (_effectSound != null)
            {
                // AudioManager.Instance?.PlaySFXAtPosition(_effectSound, _spawnPosition);
                Debug.Log($"Playing effect sound: {_effectSound.name} at {position2D}");
            }
        }

        protected virtual void OnEffectStarted()
        {
        }

        private void OnEffectCompleted()
        {
            if (_effectCompleted) return; // 중복 호출 방지

            _effectCompleted = true;
            _animator.OnAnimationCompleted -= OnEffectCompleted;
            OnEffectFinished();
            ReturnToPool();
        }

        /// <summary>
        /// 이펙트 완료 시 추가 로직 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual void OnEffectFinished()
        {
        }

        /// <summary>
        /// 강제로 이펙트 완료
        /// </summary>
        public virtual void ForceComplete()
        {
            if (_effectCompleted) return;

            _animator.StopAnimation();
            OnEffectCompleted();
        }

        public override void OnReturnToPool()
        {
            if (_animator != null)
            {
                _animator.OnAnimationCompleted -= OnEffectCompleted;
                _animator.StopAnimation();
            }

            _effectCompleted = false;
            _effectData = null;
            
            base.OnReturnToPool();
        }
    }
}