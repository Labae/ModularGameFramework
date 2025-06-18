using System.Collections.Generic;
using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Effects.HitEffects
{
    /// <summary>
    /// WeaponConfiguration 기반으로 동작하는 범용 히트 이펙트 클래스
    /// abstract 제거 - 하나의 클래스가 모든 히트 이펙트 처리
    /// WeaponConfig에 따라 다른 SpriteAnimation 자동 선택
    /// </summary>
    public sealed class ProjectileEffect : BaseEffect<ProjectileHitEffectData>
    {
        [Header("Effect Settings")]
        [SerializeField] private bool _rotateToNormal = true;
        [SerializeField] private bool _randomizeRotation = false;
        [SerializeField] private Vector2 _randomRotationRange = new Vector2(-15f, 15f);

        private Vector2 _hitNormal;
        
        // 런타임 상태
        private HitTargetType _hitType = HitTargetType.Unknown;
        private EntityClassification _entityClassification = EntityClassification.Unknown;
        private ProjectileType _projectileType = ProjectileType.Normal;
        private WeaponConfiguration _weaponConfig;

        /// <summary>
        /// Entity 분류 (적/아군/중립 구분용)
        /// </summary>
        private enum EntityClassification
        {
            Unknown,
            Enemy,
            Friendly,
            Neutral
        }
        
        protected override void OnInitialize(ProjectileHitEffectData spawnData)
        {
            _weaponConfig = spawnData.WeaponConfig;
            _hitType = spawnData.HitType;
            _hitNormal = spawnData.HitNormal;
            _projectileType = _weaponConfig.ProjectileType;

            ApplyRotationFromHitNormal();

            if (_hitType == HitTargetType.Entity)
            {
                SetEntityClassification(spawnData.HitCollider, _weaponConfig);
            }

            if (spawnData.IsPenetrateEffect)
            {
                SetupPenetrateEffect();
            }
            else
            {
                SetupNormalHitEffect();
            }
        }

        private void ApplyRotationFromHitNormal()
        {
            if (_rotateToNormal && _hitNormal != Vector2.zero)
            {
                var angle = Mathf.Atan2(_hitNormal.y, _hitNormal.x) * Mathf.Rad2Deg;
                if (_randomizeRotation)
                {
                    angle += Random.Range(_randomRotationRange.x, _randomRotationRange.y);
                }
                
                transform.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
        
        private void SetupNormalHitEffect()
        {
            if (TrySetupWeaponConfigAnimation())
            {
                return;
            }
            
            LogWarning($"No animation found for {_hitType}");
        }

        private void SetupPenetrateEffect()
        {
            var penetrateAnimation = _weaponConfig.GetPenetrateAnimation();
            if (penetrateAnimation != null && penetrateAnimation.IsValid)
            {
                _animator.SetAnimation(penetrateAnimation);
                return;
            }
            
            SetupNormalHitEffect();
            Log("Using fallback animation for penetrate effect");
        }
        

        /// <summary>
        /// Entity 타입의 경우 세부 분류 설정 (WeaponConfig 기반)
        /// </summary>
        public void SetEntityClassification(Collider2D hitCollider, WeaponConfiguration weaponConfig)
        {
            if (_hitType != HitTargetType.Entity || weaponConfig == null)
            {
                _entityClassification = EntityClassification.Unknown;
                return;
            }

            var layer = hitCollider.gameObject.layer;
            var layerMask = 1 << layer;

            if ((weaponConfig.EnemyLayers & layerMask) != 0)
            {
                _entityClassification = EntityClassification.Enemy;
            }
            else if ((weaponConfig.FriendlyLayers & layerMask) != 0)
            {
                _entityClassification = EntityClassification.Friendly;
            }
            else if ((weaponConfig.NeutralLayers & layerMask) != 0)
            {
                _entityClassification = EntityClassification.Neutral;
            }
            else
            {
                _entityClassification = EntityClassification.Unknown;
            }
        }
        
        /// <summary>
        /// 관통 이펙트 재생
        /// </summary>
        public void PlayPenetrateEffect()
        {
            if (_weaponConfig != null)
            {
                var penetrateAnimation = _weaponConfig.GetPenetrateAnimation();
        
                if (penetrateAnimation != null && penetrateAnimation.IsValid)
                {
                    // 관통 전용 애니메이션 재생
                    _animator.Play(penetrateAnimation, GetEffectDirection());
                    Log($"Playing penetrate animation: {penetrateAnimation.name} for {_hitType} ({_projectileType})");
                    return;
                }
            }
    
            // Fallback: 일반 히트 애니메이션 사용
            ConfigureSpriteAnimatorForHitType();
            Log($"Playing fallback animation for penetrate effect");
        }

        private Vector2? GetEffectDirection()
        {
            return _hitNormal == Vector2.zero ? null : _hitNormal;
        }

        /// <summary>
        /// 히트 타입과 ProjectileType에 따라 SpriteAnimator 설정
        /// 우선순위: WeaponConfig SpriteAnimation > Fallback 스프라이트 리스트
        /// </summary>
        private void ConfigureSpriteAnimatorForHitType()
        {
            // 1순위: WeaponConfig의 SpriteAnimation 사용
            if (_weaponConfig != null && TrySetupWeaponConfigAnimation())
            {
                return;
            }
            LogWarning($"No animation or sprites found for {_hitType} with ProjectileType {_projectileType}");
        }

        /// <summary>
        /// WeaponConfig의 SpriteAnimation 재생 시도
        /// </summary>
        private bool TrySetupWeaponConfigAnimation()
        {
            SpriteAnimation spriteAnim = _weaponConfig.GetHitAnimation(_hitType);

            if (spriteAnim != null && spriteAnim.IsValid)
            {
                // SpriteAnimation 기반 재생
                _animator.SetAnimation(spriteAnim);
                Log($"Set WeaponConfig animation: {spriteAnim.name} for {_hitType} ({_projectileType})");
                return true;
            }

            return false;
        }

        protected override void OnEffectStarted()
        {
            base.OnEffectStarted();
            
            // 히트 타입에 따른 추가 로직
            switch (_hitType)
            {
                case HitTargetType.Entity:
                    OnEntityHitEffect();
                    break;
                case HitTargetType.Wall:
                    OnWallHitEffect();
                    break;
                case HitTargetType.Destructible:
                    OnDestructibleHitEffect();
                    break;
            }

            // ProjectileType별 특수 이펙트
            OnProjectileTypeEffect();
        }

        /// <summary>
        /// Entity 히트 이펙트 (세부 분류 고려)
        /// </summary>
        private void OnEntityHitEffect()
        {
            // 기본 Entity 히트 처리
            switch (_entityClassification)
            {
                case EntityClassification.Enemy:
                    OnEnemyEntityEffect();
                    break;
                case EntityClassification.Friendly:
                    OnFriendlyEntityEffect();
                    break;
                case EntityClassification.Neutral:
                    OnNeutralEntityEffect();
                    break;
            }
        }

        /// <summary>
        /// ProjectileType별 특수 이펙트 처리
        /// </summary>
        private void OnProjectileTypeEffect()
        {
            switch (_projectileType)
            {
                case ProjectileType.Explosive:
                    OnExplosiveEffect();
                    break;
                case ProjectileType.Laser:
                    OnLaserEffect();
                    break;
                case ProjectileType.Magic:
                    OnMagicEffect();
                    break;
                case ProjectileType.Plasma:
                    OnPlasmaEffect();
                    break;
            }
        }

        // 확장 가능한 이펙트 메서드들
        private void OnEnemyEntityEffect() { }
        private void OnFriendlyEntityEffect() { }
        private void OnNeutralEntityEffect() { }
        private void OnWallHitEffect() { }
        private void OnDestructibleHitEffect() { }
        
        // ProjectileType별 이펙트 메서드들
        private void OnExplosiveEffect() 
        {
            // 폭발 이펙트 - 파티클, 화면 흔들림 등
        }
        
        private void OnLaserEffect() 
        {
            // 레이저 이펙트 - 번쩍임, 스파크 등
        }
        
        private void OnMagicEffect() 
        {
            // 마법 이펙트 - 반짝임, 파티클 등
        }

        private void OnPlasmaEffect() 
        {
            // 플라즈마 이펙트 - 전기, 에너지 등
        }

        public override void OnReturnToPool()
        {
            _hitType = HitTargetType.Unknown;
            _entityClassification = EntityClassification.Unknown;
            _projectileType = ProjectileType.Normal;
            _weaponConfig = null;
            base.OnReturnToPool();
        }
    }
}