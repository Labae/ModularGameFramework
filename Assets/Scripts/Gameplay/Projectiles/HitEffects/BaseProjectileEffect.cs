using MarioGame.Core.Animations;
using MarioGame.Core.ObjectPooling;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles.HitEffects
{
    /// <summary>
    /// 개선된 투사체 히트 이펙트 베이스 클래스
    /// - 타입별 다양한 이펙트 지원
    /// - 방향성 있는 이펙트
    /// - 사운드 통합
    /// </summary>
       [RequireComponent(typeof(SpriteAnimator))]
    [DisallowMultipleComponent]
    public abstract class BaseProjectileEffect : PoolableObject
    {
        [Header("Effect Settings")]
        [SerializeField] protected bool _rotateToNormal = true;
        [SerializeField] protected bool _randomizeRotation = false;
        [SerializeField] protected Vector2 _randomRotationRange = new Vector2(-15f, 15f);

        [Header("Audio")]
        [SerializeField] protected AudioClip _effectSound;
        [SerializeField] protected bool _playAudioOnSpawn = true;

        protected SpriteAnimator _animator;
        protected Vector2 _spawnPosition;
        protected Vector2 _hitNormal;
        protected bool _effectCompleted;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _animator = GetComponent<SpriteAnimator>();
            AssertIsNotNull(_animator, "SpriteAnimator required");
        }

        /// <summary>
        /// 이펙트 위치 설정
        /// </summary>
        public virtual void SetPosition(Vector2 spawnPosition)
        {
            _spawnPosition = spawnPosition;
            transform.position = spawnPosition;
        }

        /// <summary>
        /// 충돌 표면의 법선 벡터 설정
        /// </summary>
        public virtual void SetNormal(Vector2 normal)
        {
            _hitNormal = normal.normalized;
            ApplyRotationFromNormal();
        }

        /// <summary>
        /// 법선 벡터에 따른 회전 적용
        /// </summary>
        protected virtual void ApplyRotationFromNormal()
        {
            if (!_rotateToNormal || _hitNormal == Vector2.zero) return;

            float angle = Mathf.Atan2(_hitNormal.y, _hitNormal.x) * Mathf.Rad2Deg;
            
            if (_randomizeRotation)
            {
                float randomOffset = UnityEngine.Random.Range(_randomRotationRange.x, _randomRotationRange.y);
                angle += randomOffset;
            }

            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        /// <summary>
        /// 이펙트 재생 시작
        /// </summary>
        protected virtual void PlayEffect()
        {
            _effectCompleted = false;

            // SpriteAnimator 이벤트 구독
            _animator.OnAnimationCompleted += OnEffectCompleted;

            // 방향 설정 (필요한 경우)
            Vector2? direction = GetEffectDirection();
            _animator.Play(direction);

            // 사운드 재생
            if (_playAudioOnSpawn && _effectSound != null)
            {
                PlayAudio();
            }

            // 하위 클래스 커스텀 로직
            OnEffectStarted();
        }

        /// <summary>
        /// 이펙트 방향 반환 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual Vector2? GetEffectDirection()
        {
            return _hitNormal != Vector2.zero ? _hitNormal : null;
        }

        /// <summary>
        /// 이펙트 시작 시 호출되는 가상 메서드
        /// </summary>
        protected virtual void OnEffectStarted() { }

        /// <summary>
        /// 이펙트 완료 시 호출
        /// </summary>
        protected virtual void OnEffectCompleted()
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
        protected virtual void OnEffectFinished() { }

        /// <summary>
        /// 강제로 이펙트 완료
        /// </summary>
        public virtual void ForceComplete()
        {
            if (_effectCompleted) return;

            _animator.StopAnimation();
            OnEffectCompleted();
        }

        /// <summary>
        /// 오디오 재생
        /// </summary>
        protected virtual void PlayAudio()
        {
            if (_effectSound != null)
            {
                // AudioManager.Instance?.PlaySFXAtPosition(_effectSound, _spawnPosition);
                Debug.Log($"Playing effect sound: {_effectSound.name} at {_spawnPosition}");
            }
        }

        public override void OnSpawnFromPool()
        {
            base.OnSpawnFromPool();
            PlayEffect();
        }

        public override void OnReturnToPool()
        {
            if (_animator != null)
            {
                _animator.OnAnimationCompleted -= OnEffectCompleted;
                _animator.StopAnimation();
            }
            
            _effectCompleted = false;
            _hitNormal = Vector2.zero;
            
            base.OnReturnToPool();
        }
    }
}