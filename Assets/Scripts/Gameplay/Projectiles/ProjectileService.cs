using System;
using MarioGame.Audio.Interfaces;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Effects.Interface;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces.Combat;
using MarioGame.Gameplay.Interfaces.Projectiles;
using MarioGame.Gameplay.Physics.EntityCollision;
using MarioGame.Gameplay.Projectiles.Interface;
using MarioGame.Gameplay.Projectiles.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles
{
public class ProjectileService : IProjectileService
    {
        private readonly IDebugLogger _logger;
        private readonly IAudioManager _audioManager;
        private readonly IEffectFactory _effectFactory;
        private readonly IProjectileCollisionFactory _collisionFactory;
        
        // Unity 컴포넌트 참조
        private Transform _transform;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        private SpriteAnimator _spriteAnimator;
        
        // 런타임 상태
        private WeaponConfiguration _config;
        private Vector2 _direction;
        private Vector2 _startPosition;
        private Vector2 _previousPosition;
        private float _spawnTime;
        private bool _isActive;
        private int _remainingPenetrations;
        private readonly System.Collections.Generic.HashSet<Collider2D> _hitTargets = new();
        
        // 충돌 시스템
        private IProjectileCollisionBase _collisionChecker;
        
        // 프로퍼티
        public bool IsActive => _isActive;
        public WeaponConfiguration CurrentConfig => _config;
        public Vector2 Direction => _direction;
        public Vector2 Velocity => _rigidbody?.velocity ?? Vector2.zero;
        public float RemainingLifetime => _config != null ? 
            (_config.ProjectileLifetime - (Time.time - _spawnTime)) : 0f;
        
        // 이벤트
        public event Action<Vector2, Vector2> OnFired;
        public event Action<ProjectileHitData> OnHitTarget;
        public event Action<Vector2> OnDestroyed;
        public event Action<ProjectileHitData> OnPenetrated;
        public event Action OnDestroyRequested;

        public ProjectileService(IDebugLogger logger, IAudioManager audioManager, 
            IEffectFactory effectFactory, IProjectileCollisionFactory collisionFactory)
        {
            _logger = logger;
            _audioManager = audioManager;
            _effectFactory = effectFactory;
            _collisionFactory = collisionFactory;
        }

        public void Initialize(Transform transform, Rigidbody2D rigidbody, 
            SpriteRenderer spriteRenderer, SpriteAnimator animator)
        {
            _transform = transform;
            _rigidbody = rigidbody;
            _spriteRenderer = spriteRenderer;
            _spriteAnimator = animator;
            
            _logger?.Projectile("ProjectileService initialized");
        }

        public void Fire(Vector2 startPosition, Vector2 direction, WeaponConfiguration config)
        {
            if (config == null)
            {
                _logger?.Error("WeaponConfiguration is null!");
                return;
            }

            _config = config;
            _startPosition = startPosition;
            _previousPosition = startPosition;
            _direction = direction.normalized;
            _spawnTime = Time.time;
            _isActive = true;
            _remainingPenetrations = GetInitialPenetrations(config);

            // Transform 설정
            _transform.position = startPosition;

            // 시스템 설정
            SetupPhysics(config);
            SetupVisuals(config);
            SetupCollisionSystem(config);
            OnFireCustomization(config);

            OnFired?.Invoke(startPosition, _direction);
            _logger?.Projectile($"Projectile fired: {config.name}");
        }

        public void Update()
        {
            if (!_isActive || _config == null) return;

            // 수명 체크
            if (Time.time - _spawnTime >= _config.ProjectileLifetime)
            {
                OnLifetimeExpired();
                return;
            }

            // 회전 업데이트
            if (_config.RotateWithVelocity)
            {
                UpdateRotationWithVelocity();
            }

            // 타입별 특수 업데이트
            UpdateProjectileTypeSpecific();

            // 충돌 검사
            CheckCollisions();

            _previousPosition = _transform.position;
        }

        public void ProcessHit(ProjectileHitData hitData, HitTargetType hitType)
        {
            HandleHit(hitData, hitType);
            OnHitTarget?.Invoke(hitData);
        }

        public void DestroyProjectile()
        {
            if (!_isActive) return;

            _isActive = false;
            OnDestroyed?.Invoke(_transform.position);
            CreateDestroyEffect();
            OnDestroyRequested?.Invoke();
            
            _logger?.Projectile("Projectile destroyed");
        }

        public void Reset()
        {
            _isActive = false;
            _config = null;
            _remainingPenetrations = 0;
            _hitTargets.Clear();
            
            // 물리 초기화
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
                _rigidbody.gravityScale = 0f;
            }
            
            // 비주얼 초기화
            if (_transform != null)
            {
                _transform.rotation = Quaternion.identity;
            }
            
            if (_spriteRenderer != null)
            {
                var color = _spriteRenderer.color;
                color.a = 1f;
                _spriteRenderer.color = color;
            }
            
            _spriteAnimator?.StopAnimation();
        }

        private int GetInitialPenetrations(WeaponConfiguration config)
        {
            return config switch
            {
                PiercingWeaponConfig piercingConfig => piercingConfig.MaxPenetrations,
                HitscanWeaponConfig { CanPenetrate: true } hitscanConfig => hitscanConfig.MaxPenetrationTargets,
                _ => -1
            };
        }

        private void SetupPhysics(WeaponConfiguration config)
        {
            if (_rigidbody == null) return;
            
            _rigidbody.velocity = _direction * config.ProjectileSpeed;
            _rigidbody.gravityScale = config.UseGravity ? config.GravityScale : 0f;
            
            if (config.RotateWithVelocity)
            {
                UpdateRotationWithVelocity();
            }
        }

        private void SetupVisuals(WeaponConfiguration config)
        {
            if (_spriteRenderer == null) return;
            
            _spriteRenderer.flipX = _direction.x < 0;
            var color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
        }

        private void SetupCollisionSystem(WeaponConfiguration config)
        {
            _collisionChecker = _collisionFactory.CreateForProjectileType(config.ProjectileType);
            _hitTargets.Clear();
            
            _logger?.Projectile($"Collision system setup: {_collisionChecker?.GetType().Name}");
        }

        private void OnFireCustomization(WeaponConfiguration config)
        {
            SetupProjectileAnimation(config);
            
            switch (config)
            {
                case HitscanWeaponConfig hitscanConfig:
                    _logger?.Projectile($"Hitscan fired: Range={hitscanConfig.MaxRange}");
                    break;
                case PiercingWeaponConfig piercingConfig:
                    _logger?.Projectile($"Piercing fired: Max={piercingConfig.MaxPenetrations}");
                    break;
                case NormalWeaponConfig normalConfig:
                    _logger?.Projectile($"Normal fired: Bounce={normalConfig.CanBounce}");
                    break;
            }
        }

        private void SetupProjectileAnimation(WeaponConfiguration config)
        {
            if (_spriteAnimator == null) return;
            
            var animation = config.GetProjectileAnimationForType();
            if (animation != null && animation.IsValid)
            {
                _spriteAnimator.Play(animation, _direction);
                _logger?.Projectile($"Playing animation: {animation.name}");
            }
        }

        private void UpdateProjectileTypeSpecific()
        {
            switch (_config)
            {
                case HitscanWeaponConfig hitscanConfig:
                    CheckHitscanRange(hitscanConfig);
                    break;
                case PiercingWeaponConfig piercingConfig:
                    ApplyPiercingEffects(piercingConfig);
                    break;
            }
            
            UpdateFlyAnimation();
        }

        private void CheckHitscanRange(HitscanWeaponConfig config)
        {
            var distance = Vector2.Distance(_startPosition, _transform.position);
            if (distance >= config.MaxRange)
            {
                DestroyProjectile();
            }
        }

        private void ApplyPiercingEffects(PiercingWeaponConfig config)
        {
            var penetrationsUsed = config.MaxPenetrations - _remainingPenetrations;
            if (penetrationsUsed > 0)
            {
                var speedReduction = 1f - (penetrationsUsed * config.SpeedReductionPerHit);
                speedReduction = Mathf.Max(0.1f, speedReduction);
                
                var targetSpeed = config.ProjectileSpeed * speedReduction;
                if (_rigidbody.velocity.magnitude > targetSpeed)
                {
                    _rigidbody.velocity = _rigidbody.velocity.normalized * targetSpeed;
                }
            }
        }

        private void UpdateFlyAnimation()
        {
            if (Time.time - _spawnTime > 0.1f && _spriteAnimator != null)
            {
                var flyAnimation = _config.GetProjectileFlyAnimation();
                if (flyAnimation != _config.GetProjectileAnimation() && 
                    flyAnimation != null && !_spriteAnimator.IsPlaying)
                {
                    _spriteAnimator.Play(flyAnimation, _direction);
                }
            }
        }

        private void UpdateRotationWithVelocity()
        {
            if (_rigidbody?.velocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(_rigidbody.velocity.y, _rigidbody.velocity.x) * Mathf.Rad2Deg;
                _transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        private void OnLifetimeExpired()
        {
            _logger?.Projectile("Projectile lifetime expired");
            DestroyProjectile();
        }

        private void CheckCollisions()
        {
            if (_collisionChecker == null) return;

            var hits = PerformCollisionCheck();
            if (hits.Length > 0)
            {
                ProcessCollisionHits(hits);
            }
        }

        private ProjectileHitData[] PerformCollisionCheck()
        {
            var currentPos = (Vector2)_transform.position;
            var direction = Velocity.normalized;
            
            return _collisionChecker switch
            {
                IProjectilePhysicsCollision physicsCollision => 
                    physicsCollision.CheckPathCollision(_previousPosition, currentPos, _config),
                IProjectilePiercingCollision piercingCollision => 
                    piercingCollision.CheckPiercingPath(_previousPosition, currentPos, _config),
                IProjectileHitscanCollision hitscanCollision => 
                    hitscanCollision.CheckMultipleCollisions(currentPos, direction, _config),
                _ => Array.Empty<ProjectileHitData>()
            };
        }

        private void ProcessCollisionHits(ProjectileHitData[] hits)
        {
            bool shouldDestroy = false;
            
            foreach (var hit in hits)
            {
                if (!hit.IsValid || !_hitTargets.Add(hit.HitCollider)) continue;

                var hitType = _config.DetermineHitType(hit.HitCollider);
                ProcessHit(hit, hitType);
                
                if (CanPenetrate(hit))
                {
                    _remainingPenetrations--;
                    CreatePenetrateEffect(hit);
                    continue;
                }
                
                shouldDestroy = true;
                break;
            }
            
            if (shouldDestroy)
            {
                DestroyProjectile();
            }
        }

        private bool CanPenetrate(ProjectileHitData hitData)
        {
            var canPenetrate = _config switch
            {
                PiercingWeaponConfig _ => _remainingPenetrations > 0,
                HitscanWeaponConfig hitscanConfig => hitscanConfig.CanPenetrate && _remainingPenetrations > 0,
                _ => false
            };

            if (!canPenetrate) return false;

            var hitType = _config.DetermineHitType(hitData.HitCollider);
            return _config.CanPenetrateTarget(hitType);
        }

        private void HandleHit(ProjectileHitData hitData, HitTargetType hitType)
        {
            switch (hitType)
            {
                case HitTargetType.Entity:
                    TryHandleEntityHit(hitData);
                    break;
                case HitTargetType.Destructible:
                    TryHandleDestructibleHit(hitData);
                    break;
                case HitTargetType.Wall:
                    HandleWallHit(hitData);
                    break;
            }
            
            CreateHitEffect(hitData, hitType);
        }

        private void TryHandleEntityHit(ProjectileHitData hitData)
        {
            if (!_config.CanDamageTarget(hitData.HitCollider)) return;

            var hurtBox = hitData.HitCollider.GetComponent<EntityHurtBox>();
            if (hurtBox != null)
            {
                var damage = CalculateDamage(hitData);
                var direction = (hitData.HitPoint - (Vector2)_transform.position).normalized;
                hurtBox.TryApplyDamage(damage, hitData.HitPoint, direction);
            }
        }

        private void TryHandleDestructibleHit(ProjectileHitData hitData)
        {
            var destructible = hitData.HitCollider.GetComponent<IDestructible>();
            if (destructible != null)
            {
                var damage = CalculateDamage(hitData);
                destructible.Destroy(hitData.HitPoint, damage);
            }
        }

        private void HandleWallHit(ProjectileHitData hitData)
        {
            switch (_config)
            {
                case HitscanWeaponConfig hitscanConfig when hitscanConfig.IgnoreWalls:
                case PiercingWeaponConfig piercingConfig when piercingConfig.CanPenetrateWalls && _remainingPenetrations > 0:
                    return; // 관통하고 계속 진행
            }
        }

        private int CalculateDamage(ProjectileHitData hitData)
        {
            return _config switch
            {
                PiercingWeaponConfig piercingConfig => CalculatePiercingDamage(piercingConfig),
                HitscanWeaponConfig hitscanConfig => CalculateHitscanDamage(hitscanConfig, hitData),
                _ => _config.ProjectileDamage
            };
        }

        private int CalculatePiercingDamage(PiercingWeaponConfig config)
        {
            var penetrationsUsed = config.MaxPenetrations - _remainingPenetrations;
            var damage = config.ProjectileDamage - (penetrationsUsed * config.DamageReductionPerHit);
            return Mathf.Max(1, damage);
        }

        private int CalculateHitscanDamage(HitscanWeaponConfig config, ProjectileHitData hitData)
        {
            if (!config.HasDamageFalloff) return config.ProjectileDamage;
            
            var distance = Vector2.Distance(_startPosition, hitData.HitPoint);
            return config.CalculateDamageAtDistance(distance);
        }

        private void CreateHitEffect(ProjectileHitData hitData, HitTargetType hitType)
        {
            _effectFactory?.CreateProjectileHitEffect(hitData.HitPoint, _config, 
                hitData.HitCollider, hitType, hitData.HitNormal);
        }

        private void CreatePenetrateEffect(ProjectileHitData hitData)
        {
            _effectFactory?.CreatePenetrateEffect(hitData.HitPoint, _config, 
                hitData.HitCollider, hitData.HitNormal);
            OnPenetrated?.Invoke(hitData);
        }

        private void CreateDestroyEffect()
        {
            // 필요시 파괴 이펙트 구현
        }
    }
}