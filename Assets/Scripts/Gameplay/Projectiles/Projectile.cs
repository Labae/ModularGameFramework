using System;
using System.Collections.Generic;
using MarioGame.Core.Animations;
using MarioGame.Core.Interfaces;
using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces.Combat;
using MarioGame.Gameplay.Interfaces.Projectiles;
using MarioGame.Gameplay.Physics.EntityCollision;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles
{
    /// <summary>
    /// WeaponConfiguration 중심으로 리팩토링된 범용 투사체 클래스
    /// 모든 설정을 WeaponConfiguration에서 받아서 동작
    /// PlayerBullet, EnemyBullet 등의 하위 클래스 불필요
    /// SpriteAnimation 기반 비주얼 시스템 지원
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(SpriteAnimator))]
    [DisallowMultipleComponent]
    public class Projectile : PoolableObject, IProjectileLifecycle, IProjectileEvents
    {
        [Header("Runtime Data Only - No Configuration Here!")]
        [SerializeField] private WeaponConfiguration _currentWeaponConfig;
        
        // 컴포넌트 캐시
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;
        private SpriteAnimator _spriteAnimator;
        
        // 런타임 상태
        private Vector2 _direction;
        private Vector2 _startPosition;
        private Vector2 _previousPosition;
        private float _spawnTime;
        private bool _isActive;
        private int _remainingPenetrations;
        private float _currentLifetime;
        
        // 충돌 관련
        private IProjectileCollisionBase _collisionChecker;
        private readonly HashSet<Collider2D> _hitTargets = new();
        
        // 이벤트 (IProjectileEvents 인터페이스와 일치)
        public event Action<Vector2, Vector2> OnFired;
        public event Action<ProjectileHitData> OnHitTarget;
        public event Action<Vector2> OnDestroyed;
        public event Action<ProjectileHitData> OnPenetrated;

        // 프로퍼티
        public bool IsActive => _isActive;
        public WeaponConfiguration CurrentWeaponConfig => _currentWeaponConfig;
        public Vector2 Direction => _direction;
        public Vector2 Velocity => _rigidbody2D.velocity;
        public float RemainingLifetime => _currentWeaponConfig != null ? 
            (_currentWeaponConfig.ProjectileLifetime - (Time.time - _spawnTime)) : 0f;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            CacheComponents();
        }

        private void FixedUpdate()
        {
            UpdateProjectile();
        }

        #endregion

        #region Component Management

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteAnimator = GetComponent<SpriteAnimator>();
            
            AssertIsNotNull(_rigidbody2D, "Rigidbody2D required");
            AssertIsNotNull(_spriteRenderer, "SpriteRenderer required");
            AssertIsNotNull(_spriteAnimator, "SpriteAnimator required");
        }

        #endregion

        #region IProjectileLifecycle Implementation

        /// <summary>
        /// WeaponConfiguration을 받아서 투사체 발사
        /// 모든 설정이 WeaponConfiguration에서 온다
        /// </summary>
        public void Fire(Vector2 startPosition, Vector2 direction, WeaponConfiguration weaponConfig)
        {
            if (weaponConfig == null)
            {
                LogError("WeaponConfiguration is null!");
                return;
            }

            // WeaponConfiguration 저장 및 초기 설정
            _currentWeaponConfig = weaponConfig;
            _startPosition = startPosition;
            _previousPosition = startPosition;
            _direction = direction.normalized;
            _spawnTime = Time.time;
            _isActive = true;
            _remainingPenetrations = weaponConfig is PiercingWeaponConfig piercingWeaponConfig
                ? piercingWeaponConfig.MaxPenetrations
                : -1;

            // Transform 설정
            transform.position = startPosition;

            // Physics 설정 (WeaponConfig 기반)
            SetupPhysics(weaponConfig);
            
            // Visual 설정 (WeaponConfig 기반)
            SetupVisuals(weaponConfig);
            
            // Collision 설정 (WeaponConfig 기반)
            SetupCollisionSystem(weaponConfig);
            
            // Audio 재생 (WeaponConfig 기반)
            PlayFireSound(weaponConfig, startPosition);
            
            // 커스터마이징 (WeaponConfig의 ProjectileType 기반)
            OnFireCustomization(weaponConfig);
            
            // 이벤트 발생 (IProjectileEvents와 일치)
            OnFired?.Invoke(startPosition, _direction);
        }

        /// <summary>
        /// WeaponConfiguration 기반 물리 설정
        /// </summary>
        private void SetupPhysics(WeaponConfiguration weaponConfig)
        {
            // 속도 설정
            _rigidbody2D.velocity = _direction * weaponConfig.ProjectileSpeed;
            
            // 중력 설정
            if (weaponConfig.UseGravity)
            {
                _rigidbody2D.gravityScale = weaponConfig.GravityScale;
            }
            else
            {
                _rigidbody2D.gravityScale = 0f;
            }
            
            // 회전 설정
            if (weaponConfig.RotateWithVelocity)
            {
                UpdateRotationWithVelocity();
            }
        }

        /// <summary>
        /// WeaponConfiguration 기반 비주얼 설정
        /// </summary>
        private void SetupVisuals(WeaponConfiguration weaponConfig)
        {
            // 스프라이트 방향 설정
            _spriteRenderer.flipX = _direction.x < 0;
            
            // 투명도 초기화
            var color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
        }

        /// <summary>
        /// WeaponConfiguration 기반 충돌 시스템 설정
        /// </summary>
        private void SetupCollisionSystem(WeaponConfiguration weaponConfig)
        {
            // Collision Checker 초기화 (WeaponConfig 타입 기반)
            InitializeCollisionChecker(weaponConfig);
            
            // Hit Targets 초기화
            _hitTargets.Clear();
        }

        /// <summary>
        /// WeaponConfiguration 타입별 발사 시 커스터마이징
        /// Pattern Matching으로 실제 하위 클래스별 특수 설정
        /// </summary>
        private void OnFireCustomization(WeaponConfiguration weaponConfig)
        {
            // 투사체 애니메이션 설정 (WeaponConfig 기반)
            SetupProjectileAnimation(weaponConfig);

            // Pattern Matching으로 실제 하위 클래스별 특수 설정
            switch (weaponConfig)
            {
                case HitscanWeaponConfig hitscanConfig:
                    // 즉시 명중 - 매우 빠른 속도와 짧은 수명
                    _rigidbody2D.velocity = _direction * hitscanConfig.ProjectileSpeed;
                    Log($"Hitscan fired: MaxRange={hitscanConfig.MaxRange}, IgnoreWalls={hitscanConfig.IgnoreWalls}");
                    break;
                    
                case PiercingWeaponConfig piercingConfig:
                    // 관통 투사체 - 관통 횟수 설정
                    _remainingPenetrations = piercingConfig.MaxPenetrations;
                    Log($"Piercing fired: MaxPenetrations={piercingConfig.MaxPenetrations}, CanPenetrateWalls={piercingConfig.CanPenetrateWalls}");
                    break;
                    
                case NormalWeaponConfig normalConfig:
                    // 일반 물리 투사체 - 기본 설정
                    Log($"Normal fired: CanBounce={normalConfig.CanBounce}, MaxBounces={normalConfig.MaxBounces}");
                    break;
                    
                default:
                    LogWarning($"Unknown weapon config type: {weaponConfig.GetType().Name}");
                    break;
            }
        }

        /// <summary>
        /// WeaponConfiguration 기반 투사체 애니메이션 설정
        /// </summary>
        private void SetupProjectileAnimation(WeaponConfiguration weaponConfig)
        {
            // 기본 투사체 애니메이션 가져오기
            var projectileAnimation = weaponConfig.GetProjectileAnimationForType();
            
            if (projectileAnimation != null && projectileAnimation.IsValid)
            {
                // 방향을 고려한 애니메이션 재생
                _spriteAnimator.Play(projectileAnimation, _direction);
                
                Log($"Playing projectile animation: {projectileAnimation.name} for {weaponConfig.ProjectileType}");
            }
            else
            {
                LogWarning($"No valid projectile animation found for {weaponConfig.ProjectileType}");
                
                // 기본 스프라이트 설정 (fallback)
                if (_spriteRenderer.sprite == null)
                {
                    // 기본 스프라이트가 없다면 에러
                    LogError("No sprite assigned to projectile SpriteRenderer!");
                }
            }
            
            // 트레일 애니메이션이 있다면 별도 처리 (추후 구현)
            var trailAnimation = weaponConfig.GetProjectileTrailAnimation();
            if (trailAnimation != null)
            {
                // TODO: 트레일 이펙트 시스템 구현
                Log($"Trail animation available: {trailAnimation.name}");
            }
        }

        /// <summary>
        /// 최적화된 충돌 검사기 생성 (WeaponConfig 타입 기반)
        /// Factory를 통해 적절한 구현체 생성
        /// </summary>
        private void InitializeCollisionChecker(WeaponConfiguration weaponConfig)
        {
            // Factory를 통해 WeaponConfig 타입에 맞는 충돌 검사기 생성
            _collisionChecker = ProjectileCollisionFactory.CreateForProjectileType(weaponConfig.ProjectileType);
            
            Log($"Collision checker initialized: {_collisionChecker.GetType().Name} for {weaponConfig.ProjectileType}");
        }
        

        #endregion

        #region Update Logic

        /// <summary>
        /// 투사체 업데이트 (WeaponConfig 기반)
        /// </summary>
        public void UpdateProjectile()
        {
            if (!_isActive || _currentWeaponConfig == null)
                return;

            // 수명 체크 (WeaponConfig 기반)
            if (Time.time - _spawnTime >= _currentWeaponConfig.ProjectileLifetime)
            {
                OnLifetimeExpired();
                return;
            }

            // 회전 업데이트
            if (_currentWeaponConfig.RotateWithVelocity)
            {
                UpdateRotationWithVelocity();
            }

            // ProjectileType별 특수 업데이트
            UpdateProjectileTypeSpecific();

            // 충돌 검사
            CheckCollisions();

            _previousPosition = position2D;
        }

        /// <summary>
        /// WeaponConfiguration 타입별 특수 업데이트 로직
        /// Pattern Matching으로 실제 하위 클래스별 분기 처리
        /// </summary>
        private void UpdateProjectileTypeSpecific()
        {
            switch (_currentWeaponConfig)
            {
                case HitscanWeaponConfig hitscanConfig:
                    // Hitscan은 즉시 처리되므로 특별한 업데이트 불필요
                    // 하지만 MaxRange 체크는 할 수 있음
                    CheckHitscanRange(hitscanConfig);
                    break;
                    
                case PiercingWeaponConfig piercingConfig:
                    // 관통 투사체는 속도 감소 적용
                    ApplyPiercingEffects(piercingConfig);
                    break;
                    
                case NormalWeaponConfig normalConfig:
                    // 일반 투사체는 튕김 처리 (추후 구현)
                    break;
                    
                default:
                    break;
            }

            // 비행 중 애니메이션으로 전환 (모든 타입 공통)
            UpdateFlyAnimation();
        }

        /// <summary>
        /// Hitscan 최대 사거리 체크
        /// </summary>
        private void CheckHitscanRange(HitscanWeaponConfig hitscanConfig)
        {
            var travelDistance = Vector2.Distance(_startPosition, position2D);
            if (travelDistance >= hitscanConfig.MaxRange)
            {
                Log($"Hitscan reached max range: {hitscanConfig.MaxRange}");
                DestroyProjectile();
            }
        }

        /// <summary>
        /// 관통 투사체 효과 적용
        /// </summary>
        private void ApplyPiercingEffects(PiercingWeaponConfig piercingConfig)
        {
            // 관통한 횟수에 따른 속도 감소 적용
            var penetrationsUsed = piercingConfig.MaxPenetrations - _remainingPenetrations;
            if (penetrationsUsed > 0)
            {
                var speedReduction = 1f - (penetrationsUsed * piercingConfig.SpeedReductionPerHit);
                speedReduction = Mathf.Max(0.1f, speedReduction); // 최소 10% 속도 유지
                
                var currentSpeed = _rigidbody2D.velocity.magnitude;
                var targetSpeed = piercingConfig.ProjectileSpeed * speedReduction;
                
                if (currentSpeed > targetSpeed)
                {
                    _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * targetSpeed;
                }
            }
        }

        /// <summary>
        /// 비행 중 애니메이션으로 전환 (필요시)
        /// </summary>
        private void UpdateFlyAnimation()
        {
            // 발사 후 일정 시간이 지나면 비행 애니메이션으로 전환
            if (Time.time - _spawnTime > 0.1f) // 0.1초 후
            {
                var flyAnimation = _currentWeaponConfig.GetProjectileFlyAnimation();
                
                // 비행 애니메이션이 기본 애니메이션과 다르고, 아직 전환하지 않았다면
                if (flyAnimation != _currentWeaponConfig.GetProjectileAnimation() && 
                    flyAnimation != null && 
                    !_spriteAnimator.IsPlaying)
                {
                    _spriteAnimator.Play(flyAnimation, _direction);
                    Log($"Switched to fly animation: {flyAnimation.name}");
                }
            }
        }

        /// <summary>
        /// 속도에 따른 회전 업데이트
        /// </summary>
        private void UpdateRotationWithVelocity()
        {
            if (_rigidbody2D.velocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(_rigidbody2D.velocity.y, _rigidbody2D.velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        /// <summary>
        /// 수명 만료 시 처리
        /// </summary>
        private void OnLifetimeExpired()
        {
            Log("Projectile lifetime expired");
            DestroyProjectile();
        }

        #endregion

        #region Collision System

        /// <summary>
        /// 충돌 검사 (WeaponConfig 기반)
        /// 새로운 인터페이스 시스템 사용
        /// </summary>
        private void CheckCollisions()
        {
            if (_collisionChecker == null)
                return;

            var currentPos = position2D;
            var direction = Velocity.normalized;
            
            // 타입별 충돌 검사 수행
            var allHits = PerformCollisionCheck(currentPos, direction);
            
            if (allHits.Length <= 0)
                return;

            ProcessCollisionHits(allHits);
        }

        /// <summary>
        /// 충돌 검사 수행 (타입별 분기)
        /// </summary>
        private ProjectileHitData[] PerformCollisionCheck(Vector2 origin, Vector2 direction)
        {
            // 타입별로 적절한 메서드 호출
            return _collisionChecker switch
            {
                IProjectilePhysicsCollision physicsCollision => 
                    physicsCollision.CheckPathCollision(_previousPosition, origin, _currentWeaponConfig),
                
                IProjectilePiercingCollision piercingCollision => 
                    piercingCollision.CheckPiercingPath(_previousPosition, origin, _currentWeaponConfig),
                
                IProjectileHitscanCollision hitscanCollision => 
                    hitscanCollision.CheckMultipleCollisions(origin, direction, _currentWeaponConfig),
                
                _ => Array.Empty<ProjectileHitData>()
            };
        }

        /// <summary>
        /// 충돌 히트 처리 (직접 처리 방식으로 변경)
        /// CollisionHandler 대신 직접 처리
        /// </summary>
        private void ProcessCollisionHits(ProjectileHitData[] hits)
        {
            bool shouldDestroy = false;
            
            foreach (var hit in hits)
            {
                if (!hit.IsValid) continue;
                
                // 이미 충돌한 타겟은 건너뛰기 (중복 방지)
                if (!_hitTargets.Add(hit.HitCollider))
                {
                    continue;
                }

                // 히트 타입 결정
                var hitType = _currentWeaponConfig.DetermineHitType(hit.HitCollider);
                
                // 히트 처리
                ProcessHit(hit, hitType);
                
                // 관통 가능한지 체크
                if (CanPenetrate(hit))
                {
                    // 관통 성공 - 카운터 감소 및 이펙트 생성
                    _remainingPenetrations--;
                    CreatePenetrateEffect(hit);
                    Log($"Penetrated {hitType} target: {hit.HitCollider.name} (remaining: {_remainingPenetrations})");
                    continue;
                }
                
                shouldDestroy = true;
                break; // 관통 불가능하면 파괴
            }
            
            if (shouldDestroy)
            {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// ProjectileHitData 기반 관통 가능 여부 체크
        /// Pattern Matching으로 실제 하위 클래스별 관통 로직 처리
        /// </summary>
        public bool CanPenetrate(ProjectileHitData hitData)
        {
            // Pattern Matching을 사용한 관통 가능 여부 체크
            var canPenetrate = _currentWeaponConfig switch
            {
                PiercingWeaponConfig _ => _remainingPenetrations > 0,
                HitscanWeaponConfig hitscanConfig => hitscanConfig.CanPenetrate && _remainingPenetrations > 0,
                NormalWeaponConfig _ => false, // 일반 투사체는 관통 불가
                _ => false
            };

            if (!canPenetrate) return false;

            // 히트 타입 결정 후 관통 가능 여부 체크
            var hitType = _currentWeaponConfig.DetermineHitType(hitData.HitCollider);
            return _currentWeaponConfig.CanPenetrateTarget(hitType);
        }

        /// <summary>
        /// 실제 히트 처리 - WeaponConfig와 ProjectileType 기반
        /// </summary>
        public void ProcessHit(ProjectileHitData hitData, HitTargetType hitType)
        {
            // 히트 처리 로직
            HandleHit(hitData, hitType);
            
            // 이벤트 발생 (IProjectileEvents와 일치)
            OnHitTarget?.Invoke(hitData);
        }

        /// <summary>
        /// WeaponConfig와 ProjectileType 기반 히트 처리
        /// </summary>
        private void HandleHit(ProjectileHitData hitData, HitTargetType hitType)
        {
            switch (hitType)
            {
                case HitTargetType.Entity:
                    if (TryHandleEntityHit(hitData))
                    {
                        CreateHitEffect(hitData, hitType);
                    }
                    break;
                case HitTargetType.Destructible:
                    if (TryHandleDestructibleHit(hitData))
                    {
                        CreateHitEffect(hitData, hitType);
                    }

                    break;
                case HitTargetType.Wall:
                    HandleWallHit(hitData);
                    CreateHitEffect(hitData, hitType);
                    break;
                default:
                    HandleUnknownHit(hitData);
                    break;
            }
        }

        /// <summary>
        /// WeaponConfiguration 타입별 데미지 계산
        /// Pattern Matching으로 실제 하위 클래스별 데미지 로직 처리
        /// </summary>
        private int CalculateDamage(ProjectileHitData hitData)
        {
            return _currentWeaponConfig switch
            {
                PiercingWeaponConfig piercingConfig => CalculatePiercingDamage(piercingConfig),
                HitscanWeaponConfig hitscanConfig => CalculateHitscanDamage(hitscanConfig, hitData),
                NormalWeaponConfig normalConfig => normalConfig.ProjectileDamage,
                _ => _currentWeaponConfig.ProjectileDamage
            };
        }

        /// <summary>
        /// 관통 투사체의 데미지 계산 (관통별 감소)
        /// </summary>
        private int CalculatePiercingDamage(PiercingWeaponConfig piercingConfig)
        {
            var baseDamage = piercingConfig.ProjectileDamage;
            var penetrationsUsed = piercingConfig.MaxPenetrations - _remainingPenetrations;
            
            // 관통할 때마다 데미지 감소
            var finalDamage = baseDamage - (penetrationsUsed * piercingConfig.DamageReductionPerHit);
            return Mathf.Max(1, finalDamage); // 최소 1 데미지
        }

        /// <summary>
        /// Hitscan 투사체의 데미지 계산 (거리별 감소)
        /// </summary>
        private int CalculateHitscanDamage(HitscanWeaponConfig hitscanConfig, ProjectileHitData hitData)
        {
            if (!hitscanConfig.HasDamageFalloff)
            {
                return hitscanConfig.ProjectileDamage;
            }

            // 거리 계산
            var distance = Vector2.Distance(_startPosition, hitData.HitPoint);
            return hitscanConfig.CalculateDamageAtDistance(distance);
        }

        /// <summary>
        /// 엔티티 충돌 처리 (WeaponConfig의 적/아군 구분 + 타입별 데미지 적용)
        /// </summary>
        private bool TryHandleEntityHit(ProjectileHitData hitData)
        {
            var entity = hitData.HitCollider;
            
            // WeaponConfiguration에서 데미지 여부 판단
            if (!_currentWeaponConfig.CanDamageTarget(entity))
            {
                var classification = _currentWeaponConfig.GetTargetClassification(entity);
                Log($"Hit {classification} entity: {entity.name} (no damage - friendly fire disabled)");
                return false;
            }
            
            var hurtBox = entity.GetComponent<EntityHurtBox>();
            if (hurtBox != null)
            {
                // Pattern Matching으로 타입별 데미지 계산
                var finalDamage = CalculateDamage(hitData);
                var damageDirection = (hitData.HitPoint - position2D).normalized;
                
                bool success = hurtBox.TryApplyDamage(finalDamage, hitData.HitPoint, damageDirection);
                var classification = _currentWeaponConfig.GetTargetClassification(entity);
                Log($"Dealt {finalDamage} damage to {classification} entity: {entity.name}");
                return success;
            }
            else
            {
                Log($"Hit entity: {entity.name} (no EntityHurtBox component)");
                return false;
            }
        }

        /// <summary>
        /// 파괴 가능한 오브젝트 충돌 처리 (WeaponConfig 타입별 데미지 사용)
        /// </summary>
        private bool TryHandleDestructibleHit(ProjectileHitData hitData)
        {
            var destructible = hitData.HitCollider.GetComponent<IDestructible>();
            if (destructible != null)
            {
                // Pattern Matching으로 타입별 데미지 계산
                var finalDamage = CalculateDamage(hitData);
                destructible.Destroy(hitData.HitPoint, finalDamage);
                Log($"Destroyed {hitData.HitCollider.name} with {finalDamage} damage");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 벽 충돌 처리 (WeaponConfig 타입별 특수 처리)
        /// </summary>
        private void HandleWallHit(ProjectileHitData hitData)
        {
            // Pattern Matching으로 타입별 벽 충돌 처리
            switch (_currentWeaponConfig)
            {
                case HitscanWeaponConfig hitscanConfig when hitscanConfig.IgnoreWalls:
                    Log($"Hitscan ignoring wall: {hitData.HitCollider.name}");
                    return; // 벽 무시하고 계속 진행
                    
                case PiercingWeaponConfig piercingConfig when piercingConfig.CanPenetrateWalls && _remainingPenetrations > 0:
                    Log($"Piercing through wall: {hitData.HitCollider.name}");
                    // 관통 카운터는 ProcessCollisionHits에서 이미 감소됨
                    return; // 벽 관통하고 계속 진행
                    
                case NormalWeaponConfig normalConfig when normalConfig.CanBounce:
                    // TODO: 튕김 로직 구현
                    Log($"Normal projectile bouncing off wall: {hitData.HitCollider.name}");
                    break; // 일단 파괴 (튕김 로직은 추후 구현)
                    
                default:
                    Log($"Hit wall: {hitData.HitCollider.name}");
                    break; // 일반적으로 파괴됨
            }
        }

        private void HandleUnknownHit(ProjectileHitData hitData)
        {
            Log($"Hit Unknown: {hitData.HitCollider.name}");
        }
        
        #endregion

        #region Effects & Audio

        /// <summary>
        /// 히트 이펙트 생성 (WeaponConfig 기반)
        /// </summary>
        private void CreateHitEffect(ProjectileHitData hitData, HitTargetType hitType)
        {
            if (_currentWeaponConfig.HitEffectPrefab != null)
            {
                var effect = GetPooledEffect(_currentWeaponConfig.HitEffectPrefab);
                if (effect != null)
                {
                    // WeaponConfig와 함께 이펙트 초기화
                    effect.Initialize(_currentWeaponConfig, hitData.HitCollider, hitType);
                    effect.SetPosition(hitData.HitPoint);
                    effect.SetNormal(hitData.HitNormal);
                }
            }

            // 히트 사운드는 이펙트에서 WeaponConfig 기반으로 재생됨
            // 별도 사운드 재생 불필요
        }

        /// <summary>
        /// 관통 이펙트 생성 (WeaponConfig 기반)
        /// </summary>
        private void CreatePenetrateEffect(ProjectileHitData hitData)
        {
            if (_currentWeaponConfig.PenetrateEffectPrefab != null)
            {
                var effect = GetPooledEffect(_currentWeaponConfig.PenetrateEffectPrefab);
                if (effect != null)
                {
                    // 실제 타겟 타입으로 관통 이펙트 초기화
                    var hitType = _currentWeaponConfig.DetermineHitType(hitData.HitCollider);
                    effect.Initialize(_currentWeaponConfig, hitData.HitCollider, hitType);
                    effect.SetPosition(hitData.HitPoint);
                    effect.SetNormal(hitData.HitNormal);
                    
                    Log($"Created penetrate effect for {hitType} target: {hitData.HitCollider.name}");
                }
            }
            
            // 관통 이벤트 발생 (IProjectileEvents와 일치)
            OnPenetrated?.Invoke(hitData);
        }

        /// <summary>
        /// 풀에서 이펙트 가져오기
        /// </summary>
        private T GetPooledEffect<T>(T prefab) where T : Component, IPoolable
        {
            if (!ObjectPoolManager.Instance.TryGetPool<T>(out var pool))
            {
                LogWarning($"Pool for {typeof(T).Name} not found");
                return null;
            }

            var effect = pool.Get();
            if (effect == null)
            {
                LogError($"Failed to get {typeof(T).Name} from pool");
            }

            return effect;
        }

        #endregion

        #region Audio

        /// <summary>
        /// 발사 사운드 재생 (WeaponConfig 기반)
        /// </summary>
        private void PlayFireSound(WeaponConfiguration weaponConfig, Vector2 position)
        {
            if (weaponConfig.FireSound != null)
            {
                // AudioManager.Instance?.PlaySFX(weaponConfig.FireSound, position);
                Log($"Playing fire sound: {weaponConfig.FireSound.name}");
            }
        }

        #endregion

        #region Destruction

        /// <summary>
        /// 투사체 파괴
        /// </summary>
        public void DestroyProjectile()
        {
            if (!_isActive)
                return;

            _isActive = false;
            
            // 파괴 이벤트 (IProjectileEvents와 일치)
            OnDestroyed?.Invoke(position2D);
            
            // 파괴 이펙트 (선택사항)
            CreateDestroyEffect();
            
            // 풀로 반환
            ReturnToPool();
        }

        /// <summary>
        /// 파괴 이펙트 생성 (선택사항)
        /// </summary>
        private void CreateDestroyEffect()
        {
            // ProjectileType별 특별한 파괴 이펙트가 필요하다면 여기서 구현
        }

        #endregion

        #region Object Pooling

        public override void OnSpawnFromPool()
        {
            base.OnSpawnFromPool();
            
            // 상태 초기화
            _isActive = false;
            _currentWeaponConfig = null;
            _remainingPenetrations = 0;
            _hitTargets.Clear();
            
            // 물리 초기화
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
            _rigidbody2D.gravityScale = 0f;
            
            // 비주얼 초기화
            transform.rotation = Quaternion.identity;
            var color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
            
            // 애니메이터 초기화
            if (_spriteAnimator != null)
            {
                _spriteAnimator.StopAnimation();
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            // 이벤트 정리 (메모리 누수 방지)
            OnFired = null;
            OnHitTarget = null;
            OnDestroyed = null;
            OnPenetrated = null;
            
            // 상태 정리
            _hitTargets.Clear();
            _currentWeaponConfig = null;
            
            // 애니메이터 정리
            if (_spriteAnimator != null)
            {
                _spriteAnimator.StopAnimation();
            }
        }

        #endregion
    }
}