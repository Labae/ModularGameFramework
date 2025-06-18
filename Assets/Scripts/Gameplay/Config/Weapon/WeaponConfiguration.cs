using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Effects.HitEffects;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Projectiles;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Weapon
{
    /// <summary>
    /// WeaponConfiguration 기반 적/아군 구분 시스템
    /// ProjectileType 추가로 완전한 데이터 드리븐 시스템
    /// </summary>
    public abstract class WeaponConfiguration : ScriptableObject
    {
        [Header("Basic Info")]
        public string WeaponName;
        public Sprite WeaponIcon;

        [Header("Projectile Type & Settings")]
        [Tooltip("투사체 동작 방식 - 충돌 검사 방식이 자동으로 결정됨")]
        public ProjectileType ProjectileType = ProjectileType.Normal;
        
        public Projectile ProjectilePrefab; 
        public float ProjectileSpeed = 15f;
        public int ProjectileDamage = 1;
        public float ProjectileLifetime = 3f;

        [Header("Projectile Animations")]
        [Tooltip("투사체 기본 애니메이션 (필수)")]
        public SpriteAnimation ProjectileAnimation;
        
        [Tooltip("투사체 비행 중 애니메이션 (선택사항 - 없으면 기본 애니메이션 사용)")]
        public SpriteAnimation ProjectileFlyAnimation;
        
        [Tooltip("투사체 꼬리/트레일 애니메이션 (선택사항)")]
        public SpriteAnimation ProjectileTrailAnimation;
        
        [Tooltip("투사체 충전/차징 애니메이션 (선택사항)")]
        public SpriteAnimation ProjectileChargeAnimation;

        [Header("Fire Settings")]
        public AudioClip FireSound;
        public float FireRate = 0.3f;
        public float SafeFireDistance = 0.2f;

        [Header("Target Classification")]
        [Tooltip("적군 레이어 - 데미지를 주는 대상")]
        public LayerMask EnemyLayers = -1;
        
        [Tooltip("아군 레이어 - 데미지를 주지 않는 대상")]
        public LayerMask FriendlyLayers = -1;
        
        [Tooltip("벽/장애물 레이어")]
        public LayerMask WallLayers = -1;
        
        [Tooltip("파괴 가능한 오브젝트 레이어")]
        public LayerMask DestructibleLayers = -1;
        
        [Tooltip("중립 레이어 - 설정에 따라 데미지 여부 결정")]
        public LayerMask NeutralLayers = 0;

        [Header("Effects & Audio")]
        public GameObject MuzzleFlashPrefab;

        [Header("Hit Animations")]
        [Tooltip("Entity 히트 시 재생할 애니메이션")]
        public SpriteAnimation EntityHitAnimation;
        
        [Tooltip("벽 히트 시 재생할 애니메이션")]
        public SpriteAnimation WallHitAnimation;
        
        [Tooltip("파괴 가능한 오브젝트 히트 시 재생할 애니메이션")]
        public SpriteAnimation DestructibleHitAnimation;
        
        [Tooltip("관통 시 재생할 애니메이션")]
        public SpriteAnimation PenetrateAnimation;

        [Header("ProjectileType Specific Animations")]
        [Tooltip("이 부분은 하위 클래스에서 필요시 오버라이드")]

        [Header("Advanced")]
        public bool UseGravity = false;
        public float GravityScale = 1f;
        public bool RotateWithVelocity = true;
        
        [Header("Friendly Fire")]
        [Tooltip("아군에게도 데미지를 줄지 여부")]
        public bool AllowFriendlyFire = false;
        
        [Tooltip("중립 대상에게 데미지를 줄지 여부")]
        public bool DamageNeutralTargets = true;

        /// <summary>
        /// 히트 타입 결정 (간소화된 분류)
        /// </summary>
        public HitTargetType DetermineHitType(Collider2D hitCollider)
        {
            var layer = hitCollider.gameObject.layer;
            var layerMask = 1 << layer;

            // Entity 체크 (모든 생명체) - Enemy, Friendly, Neutral 모두 Entity로 분류
            if ((EnemyLayers & layerMask) != 0 || 
                (FriendlyLayers & layerMask) != 0 || 
                (NeutralLayers & layerMask) != 0)
            {
                return HitTargetType.Entity;
            }
            
            // 파괴 가능한 오브젝트
            if ((DestructibleLayers & layerMask) != 0)
            {
                return HitTargetType.Destructible;
            }
            
            // 벽/장애물
            if ((WallLayers & layerMask) != 0)
            {
                return HitTargetType.Wall;
            }

            return HitTargetType.Unknown;
        }

        /// <summary>
        /// 데미지를 줄 수 있는 대상인지 확인
        /// </summary>
        public bool CanDamageTarget(Collider2D hitCollider)
        {
            var layer = hitCollider.gameObject.layer;
            var layerMask = 1 << layer;

            // 적군은 항상 데미지
            if ((EnemyLayers & layerMask) != 0)
            {
                return true;
            }
            
            // 아군은 설정에 따라
            if ((FriendlyLayers & layerMask) != 0)
            {
                return AllowFriendlyFire;
            }
            
            // 중립은 설정에 따라
            if ((NeutralLayers & layerMask) != 0)
            {
                return DamageNeutralTargets;
            }
            
            // 파괴 가능한 오브젝트는 항상 데미지
            if ((DestructibleLayers & layerMask) != 0)
            {
                return true;
            }

            return false; // 기본적으로 데미지 없음
        }

        /// <summary>
        /// 관통 가능한 타겟인지 확인
        /// </summary>
        public virtual bool CanPenetrateTarget(HitTargetType targetType)
        {
            return false;
        }

        public virtual LayerMask GetCombatLayerMask()
        {
            return WallLayers | EnemyLayers | DestructibleLayers;
        } 

        /// <summary>
        /// 타겟의 분류 정보 반환 (디버그/로그용)
        /// </summary>
        public string GetTargetClassification(Collider2D hitCollider)
        {
            var layer = hitCollider.gameObject.layer;
            var layerMask = 1 << layer;

            if ((EnemyLayers & layerMask) != 0) return "Enemy";
            if ((FriendlyLayers & layerMask) != 0) return "Friendly";
            if ((NeutralLayers & layerMask) != 0) return "Neutral";
            if ((DestructibleLayers & layerMask) != 0) return "Destructible";
            if ((WallLayers & layerMask) != 0) return "Wall";
            
            return "Unknown";
        }

        /// <summary>
        /// 히트 타입과 ProjectileType에 맞는 SpriteAnimation 반환
        /// 우선순위: ProjectileType 전용 > 기본 히트 타입
        /// </summary>
        public SpriteAnimation GetHitAnimation(HitTargetType hitType)
        {
            // 1순위: ProjectileType별 전용 애니메이션
            var projectileSpecificAnimation = GetProjectileTypeAnimation(hitType);
            if (projectileSpecificAnimation != null)
            {
                return projectileSpecificAnimation;
            }

            // 2순위: 기본 히트 타입별 애니메이션
            return hitType switch
            {
                HitTargetType.Entity => EntityHitAnimation,
                HitTargetType.Wall => WallHitAnimation,
                HitTargetType.Destructible => DestructibleHitAnimation,
                _ => EntityHitAnimation // 기본값
            };
        }

        /// <summary>
        /// 관통 시 사용할 애니메이션 반환
        /// </summary>
        public SpriteAnimation GetPenetrateAnimation()
        {
            return PenetrateAnimation ?? EntityHitAnimation; // 기본값으로 Entity 히트 애니메이션 사용
        }

        /// <summary>
        /// ProjectileType별 전용 애니메이션 반환 (하위 클래스에서 오버라이드)
        /// </summary>
        protected virtual SpriteAnimation GetProjectileTypeAnimation(HitTargetType hitType)
        {
            return null; // 기본적으로 없음, 하위 클래스에서 구현
        }

        /// <summary>
        /// 투사체 기본 애니메이션 반환 (발사 시 사용)
        /// </summary>
        public SpriteAnimation GetProjectileAnimation()
        {
            return ProjectileAnimation;
        }

        /// <summary>
        /// 투사체 비행 애니메이션 반환 (비행 중 사용, 없으면 기본 애니메이션)
        /// </summary>
        public SpriteAnimation GetProjectileFlyAnimation()
        {
            return ProjectileFlyAnimation ?? ProjectileAnimation;
        }

        /// <summary>
        /// 투사체 트레일 애니메이션 반환 (선택사항)
        /// </summary>
        public SpriteAnimation GetProjectileTrailAnimation()
        {
            return ProjectileTrailAnimation;
        }

        /// <summary>
        /// 투사체 차징 애니메이션 반환 (선택사항)
        /// </summary>
        public SpriteAnimation GetProjectileChargeAnimation()
        {
            return ProjectileChargeAnimation;
        }

        /// <summary>
        /// ProjectileType에 맞는 투사체 애니메이션 반환 (발사 시 사용)
        /// 하위 클래스에서 필요시 오버라이드
        /// </summary>
        public virtual SpriteAnimation GetProjectileAnimationForType()
        {
            return ProjectileAnimation; // 기본값
        }
    }
}