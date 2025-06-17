using System.Collections.Generic;
using MarioGame.Core.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Projectiles.HitEffects
{
    /// <summary>
    /// WeaponConfiguration 기반으로 동작하는 범용 히트 이펙트 클래스
    /// abstract 제거 - 하나의 클래스가 모든 히트 이펙트 처리
    /// WeaponConfig에 따라 다른 SpriteAnimation 자동 선택
    /// </summary>
    public class ProjectileEffect : BaseProjectileEffect
    {
        [Header("Fallback Audio (WeaponConfig 없을 때)")]
        [SerializeField] private AudioClip _entityHitSound;      // Entity용 (기존 Enemy + Player 통합)
        [SerializeField] private AudioClip _wallHitSound;
        [SerializeField] private AudioClip _destructibleHitSound;
        [SerializeField] private AudioClip _destroySound;

        [Header("Fallback Sprites (WeaponConfig 없을 때)")]
        [SerializeField] private List<Sprite> _entityHitSprites = new();      // Entity용 통합
        [SerializeField] private List<Sprite> _wallHitSprites = new();
        [SerializeField] private List<Sprite> _destructibleHitSprites = new();
        [SerializeField] private List<Sprite> _destroySprites = new();

        [Header("Entity Specific Fallback (Optional)")]
        [Tooltip("특정 Entity 타입별 차별화가 필요한 경우 사용")]
        [SerializeField] private AudioClip _enemySpecificSound;      // 적 전용 (선택사항)
        [SerializeField] private AudioClip _friendlySpecificSound;   // 아군 전용 (선택사항)
        [SerializeField] private List<Sprite> _enemySpecificSprites = new();   // 적 전용 스프라이트
        [SerializeField] private List<Sprite> _friendlySpecificSprites = new(); // 아군 전용 스프라이트

        [Header("ProjectileType Specific Fallback (Advanced)")]
        [Tooltip("ProjectileType별 특수 이펙트 (선택사항)")]
        [SerializeField] private List<Sprite> _explosiveHitSprites = new();    // 폭발형
        [SerializeField] private List<Sprite> _laserHitSprites = new();        // 레이저
        [SerializeField] private List<Sprite> _magicHitSprites = new();        // 마법
        [SerializeField] private AudioClip _explosiveHitSound;
        [SerializeField] private AudioClip _laserHitSound;
        [SerializeField] private AudioClip _magicHitSound;

        // 런타임 상태
        private HitTargetType _hitType = HitTargetType.Unknown;
        private bool _isDestroyVariant = false;
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

        /// <summary>
        /// WeaponConfiguration과 함께 히트 이펙트 초기화
        /// 모든 설정이 WeaponConfig에서 결정됨 (우선순위: WeaponConfig > Fallback)
        /// </summary>
        public virtual void Initialize(WeaponConfiguration weaponConfig, Collider2D hitCollider, HitTargetType hitType)
        {
            _weaponConfig = weaponConfig;
            _hitType = hitType;
            _projectileType = weaponConfig?.ProjectileType ?? ProjectileType.Normal;
            
            // Entity 타입인 경우 세부 분류 설정
            if (_hitType == HitTargetType.Entity && weaponConfig != null)
            {
                SetEntityClassification(hitCollider, weaponConfig);
            }
            
            // 애니메이션 설정 (WeaponConfig 우선, Fallback 차순)
            ConfigureSpriteAnimatorForHitType();
        }

        /// <summary>
        /// 충돌 타입 설정 (기존 호환성 - WeaponConfig 없이 사용할 때)
        /// </summary>
        public virtual void SetHitType(HitTargetType hitType)
        {
            _hitType = hitType;
            ConfigureSpriteAnimatorForHitType();
        }

        /// <summary>
        /// Entity 타입의 경우 세부 분류 설정 (WeaponConfig 기반)
        /// </summary>
        public virtual void SetEntityClassification(Collider2D hitCollider, WeaponConfiguration weaponConfig)
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

            // 분류에 따라 애니메이션 재설정
            ConfigureSpriteAnimatorForHitType();
        }
        
        /// <summary>
        /// 관통 이펙트 재생
        /// </summary>
        public virtual void PlayPenetrateEffect()
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

        /// <summary>
        /// 파괴 변형 이펙트 재생
        /// </summary>
        public virtual void PlayDestroyVariant()
        {
            _isDestroyVariant = true;
            ConfigureSpriteAnimatorForHitType();
        }

        /// <summary>
        /// 히트 타입과 ProjectileType에 따라 SpriteAnimator 설정
        /// 우선순위: WeaponConfig SpriteAnimation > Fallback 스프라이트 리스트
        /// </summary>
        private void ConfigureSpriteAnimatorForHitType()
        {
            // 1순위: WeaponConfig의 SpriteAnimation 사용
            if (_weaponConfig != null && TryPlayWeaponConfigAnimation())
            {
                return;
            }

            // 2순위: Fallback 스프라이트 리스트 사용
            var sprites = GetFallbackSpritesForHitType();
            if (sprites != null && sprites.Count > 0)
            {
                _animator.Play(sprites);
                Log($"Playing fallback sprite animation for {_hitType}");
            }
            else
            {
                LogWarning($"No animation or sprites found for {_hitType} with ProjectileType {_projectileType}");
            }
        }

        /// <summary>
        /// WeaponConfig의 SpriteAnimation 재생 시도
        /// </summary>
        private bool TryPlayWeaponConfigAnimation()
        {
            SpriteAnimation animation = null;

            if (_isDestroyVariant)
            {
                // 파괴 변형은 일반 히트 애니메이션 사용 (특별한 파괴 애니메이션은 없음)
                animation = _weaponConfig.GetHitAnimation(_hitType);
            }
            else
            {
                // 일반 히트 애니메이션
                animation = _weaponConfig.GetHitAnimation(_hitType);
            }

            if (animation != null && animation.IsValid)
            {
                // SpriteAnimation 기반 재생
                _animator.Play(animation, GetEffectDirection());
                Log($"Playing WeaponConfig animation: {animation.name} for {_hitType} ({_projectileType})");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Fallback 스프라이트 리스트 반환 (WeaponConfig 없을 때)
        /// </summary>
        private List<Sprite> GetFallbackSpritesForHitType()
        {
            if (_isDestroyVariant)
            {
                return _destroySprites;
            }

            // 1순위: ProjectileType별 특수 스프라이트
            var projectileSpecificSprites = GetProjectileTypeSprites();
            if (projectileSpecificSprites != null && projectileSpecificSprites.Count > 0)
            {
                return projectileSpecificSprites;
            }

            // 2순위: 기본 히트 타입별 스프라이트
            return _hitType switch
            {
                HitTargetType.Entity => GetEntitySprites(),
                HitTargetType.Wall => _wallHitSprites,
                HitTargetType.Destructible => _destructibleHitSprites,
                _ => null
            };
        }

        /// <summary>
        /// ProjectileType별 특수 스프라이트 반환 (Fallback)
        /// </summary>
        private List<Sprite> GetProjectileTypeSprites()
        {
            return _projectileType switch
            {
                ProjectileType.Explosive when _explosiveHitSprites.Count > 0 => _explosiveHitSprites,
                ProjectileType.Laser when _laserHitSprites.Count > 0 => _laserHitSprites,
                ProjectileType.Magic when _magicHitSprites.Count > 0 => _magicHitSprites,
                _ => null
            };
        }

        /// <summary>
        /// Entity 타입에 맞는 스프라이트 반환 (세부 분류 고려, Fallback)
        /// </summary>
        private List<Sprite> GetEntitySprites()
        {
            // 특정 Entity 타입별 스프라이트가 있으면 우선 사용
            var specificSprites = _entityClassification switch
            {
                EntityClassification.Enemy when _enemySpecificSprites.Count > 0 => _enemySpecificSprites,
                EntityClassification.Friendly when _friendlySpecificSprites.Count > 0 => _friendlySpecificSprites,
                _ => null
            };

            // 특정 스프라이트가 있으면 사용, 없으면 기본 Entity 스프라이트 사용
            return specificSprites ?? _entityHitSprites;
        }

        protected override void PlayAudio()
        {
            AudioClip soundToPlay = null;

            if (_isDestroyVariant)
            {
                soundToPlay = _destroySound;
            }
            else
            {
                // 1순위: WeaponConfig의 사운드 사용
                if (_weaponConfig != null && _weaponConfig.HitSound != null)
                {
                    soundToPlay = _weaponConfig.HitSound;
                    Debug.Log($"Playing WeaponConfig hit sound: {soundToPlay.name} for {_hitType} ({_projectileType})");
                }
                else
                {
                    // 2순위: Fallback 사운드 사용
                    // ProjectileType별 특수 사운드 우선
                    soundToPlay = GetProjectileTypeSound();
                    
                    // 기본 히트 타입별 사운드
                    if (soundToPlay == null)
                    {
                        soundToPlay = _hitType switch
                        {
                            HitTargetType.Entity => GetEntitySound(),
                            HitTargetType.Wall => _wallHitSound,
                            HitTargetType.Destructible => _destructibleHitSound,
                            _ => _effectSound
                        };
                    }
                    
                    if (soundToPlay != null)
                    {
                        Debug.Log($"Playing fallback hit sound: {soundToPlay.name} for {_hitType} ({_entityClassification}) ProjectileType: {_projectileType}");
                    }
                }
            }

            if (soundToPlay != null)
            {
                // AudioManager.Instance?.PlaySFXAtPosition(soundToPlay, _spawnPosition);
            }
            else
            {
                LogWarning($"No audio found for {_hitType} with ProjectileType {_projectileType}");
            }
        }

        /// <summary>
        /// ProjectileType별 특수 사운드 반환 (Fallback)
        /// </summary>
        private AudioClip GetProjectileTypeSound()
        {
            return _projectileType switch
            {
                ProjectileType.Explosive when _explosiveHitSound != null => _explosiveHitSound,
                ProjectileType.Laser when _laserHitSound != null => _laserHitSound,
                ProjectileType.Magic when _magicHitSound != null => _magicHitSound,
                _ => null
            };
        }

        /// <summary>
        /// Entity 타입에 맞는 사운드 반환 (세부 분류 고려, Fallback)
        /// </summary>
        private AudioClip GetEntitySound()
        {
            // 특정 Entity 타입별 사운드가 있으면 우선 사용
            var specificSound = _entityClassification switch
            {
                EntityClassification.Enemy when _enemySpecificSound != null => _enemySpecificSound,
                EntityClassification.Friendly when _friendlySpecificSound != null => _friendlySpecificSound,
                _ => null
            };

            // 특정 사운드가 있으면 사용, 없으면 기본 Entity 사운드 사용
            return specificSound ?? _entityHitSound;
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
        protected virtual void OnEntityHitEffect()
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
        protected virtual void OnProjectileTypeEffect()
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
        protected virtual void OnEnemyEntityEffect() { }
        protected virtual void OnFriendlyEntityEffect() { }
        protected virtual void OnNeutralEntityEffect() { }
        protected virtual void OnWallHitEffect() { }
        protected virtual void OnDestructibleHitEffect() { }
        
        // ProjectileType별 이펙트 메서드들
        protected virtual void OnExplosiveEffect() 
        {
            // 폭발 이펙트 - 파티클, 화면 흔들림 등
        }
        
        protected virtual void OnLaserEffect() 
        {
            // 레이저 이펙트 - 번쩍임, 스파크 등
        }
        
        protected virtual void OnMagicEffect() 
        {
            // 마법 이펙트 - 반짝임, 파티클 등
        }
        
        protected virtual void OnPlasmaEffect() 
        {
            // 플라즈마 이펙트 - 전기, 에너지 등
        }

        public override void OnReturnToPool()
        {
            _hitType = HitTargetType.Unknown;
            _entityClassification = EntityClassification.Unknown;
            _isDestroyVariant = false;
            _projectileType = ProjectileType.Normal;
            _weaponConfig = null;
            base.OnReturnToPool();
        }
    }
}