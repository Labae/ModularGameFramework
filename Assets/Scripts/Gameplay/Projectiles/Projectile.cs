using System;
using MarioGame.Audio.Interfaces;
using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Effects.Interface;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces.Projectiles;
using MarioGame.Gameplay.Projectiles.Interface;
using MarioGame.Gameplay.Projectiles.ProjectileCollision.Core;
using Reflex.Attributes;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(SpriteAnimator))]
    [DisallowMultipleComponent]
    public class Projectile : PoolableObject, IProjectileLifecycle, IProjectileEvents
    {
        // DI 서비스들
        [Inject] private IEffectFactory _effectFactory;
        [Inject] private IProjectileCollisionFactory _collisionFactory;

        // 컴포넌트 캐시
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;
        private SpriteAnimator _spriteAnimator;

        // 순수 C# 서비스
        private IProjectileService _projectileService;

        // IProjectileLifecycle & IProjectileEvents 구현
        public bool IsActive => _projectileService?.IsActive ?? false;

        public event Action<Vector2, Vector2> OnFired
        {
            add => _projectileService.OnFired += value;
            remove => _projectileService.OnFired -= value;
        }

        public event Action<ProjectileHitData> OnHitTarget
        {
            add => _projectileService.OnHitTarget += value;
            remove => _projectileService.OnHitTarget -= value;
        }

        public event Action<Vector2> OnDestroyed
        {
            add => _projectileService.OnDestroyed += value;
            remove => _projectileService.OnDestroyed -= value;
        }

        public event Action<ProjectileHitData> OnPenetrated
        {
            add => _projectileService.OnPenetrated += value;
            remove => _projectileService.OnPenetrated -= value;
        }

        protected override void Awake()
        {
            base.Awake();
            CreateProjectileService();
        }

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteAnimator = GetComponent<SpriteAnimator>();

            _assertManager.AssertIsNotNull(_rigidbody2D, "Rigidbody2D required");
            _assertManager.AssertIsNotNull(_spriteRenderer, "SpriteRenderer required");
            _assertManager.AssertIsNotNull(_spriteAnimator, "SpriteAnimator required");
        }

        private void CreateProjectileService()
        {
            _projectileService = new ProjectileService(_debugLogger,
                GetComponent<IAudioManager>(), _effectFactory, _collisionFactory);

            _projectileService.Initialize(transform, _rigidbody2D, _spriteRenderer, _spriteAnimator);
            _projectileService.OnDestroyRequested += () => ReturnToPool();
        }

        private void FixedUpdate()
        {
            _projectileService?.Update();
        }

        // IProjectileLifecycle 구현 - 서비스로 전달
        public void Fire(Vector2 startPosition, Vector2 direction, WeaponConfiguration config)
        {
            _projectileService?.Fire(startPosition, direction, config);
        }

        public void UpdateProjectile()
        {
            _projectileService?.Update();
        }

        public void ProcessHit(ProjectileHitData hitData, HitTargetType hitType)
        {
            _projectileService?.ProcessHit(hitData, hitType);
        }

        public void DestroyProjectile()
        {
            _projectileService?.DestroyProjectile();
        }

        public override void OnSpawnFromPool()
        {
            base.OnSpawnFromPool();
            _projectileService?.Reset();
        }

        public override void OnReturnToPool()
        {
            _projectileService?.Reset();
            base.OnReturnToPool();
        }
    }
}