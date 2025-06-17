using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Weapon
{
    /// <summary>
    /// 즉시 명중 투사체용 WeaponConfiguration
    /// 레이저, 스나이퍼 등 즉시 명중하는 무기
    /// </summary>
    [CreateAssetMenu(menuName = "MarioGame/Weapon/Hitscan Weapon Config", fileName = "New Hitscan Weapon")]
    public class HitscanWeaponConfig : WeaponConfiguration
    {
        [Header("Hitscan Settings")]
        [Tooltip("최대 사거리")]
        public float MaxRange = 100f;
        
        [Tooltip("벽을 무시하고 관통할 수 있는지")]
        public bool IgnoreWalls = false;
        
        [Tooltip("관통 가능 여부")]
        public bool CanPenetrate = false;
        
        [Tooltip("최대 관통 대상 수")]
        public int MaxPenetrationTargets = 1;
        
        [Tooltip("거리에 따른 데미지 감소 여부")]
        public bool HasDamageFalloff = false;
        
        [Tooltip("데미지 감소 시작 거리")]
        public float DamageFalloffStartDistance = 30f;
        
        [Tooltip("최소 데미지 배율 (0~1)")]
        [Range(0f, 1f)]
        public float MinDamageMultiplier = 0.5f;

        void Reset()
        {
            // 기본값 설정
            ProjectileType = ProjectileType.Hitscan;
            WeaponName = "Hitscan Weapon";
            ProjectileSpeed = 1000f; // 매우 빠른 속도 (즉시 명중)
            ProjectileDamage = 3;
            ProjectileLifetime = 0.1f; // 매우 짧은 수명
            UseGravity = false;
            RotateWithVelocity = true;
            MaxRange = 100f;
        }

        /// <summary>
        /// Hitscan 투사체의 관통 가능 여부 판별
        /// </summary>
        public override bool CanPenetrateTarget(HitTargetType targetType)
        {
            if (!CanPenetrate) return false;
            
            return targetType switch
            {
                HitTargetType.Entity => true, // 엔티티는 관통 가능
                HitTargetType.Destructible => true, // 파괴 가능한 오브젝트도 관통
                HitTargetType.Wall => IgnoreWalls, // 벽은 IgnoreWalls 설정에 따라
                _ => false
            };
        }

        /// <summary>
        /// 거리에 따른 데미지 계산
        /// </summary>
        public int CalculateDamageAtDistance(float distance)
        {
            if (!HasDamageFalloff || distance <= DamageFalloffStartDistance)
            {
                return ProjectileDamage;
            }

            // 거리에 따른 선형 감소
            var falloffRatio = (distance - DamageFalloffStartDistance) / (MaxRange - DamageFalloffStartDistance);
            var damageMultiplier = Mathf.Lerp(1f, MinDamageMultiplier, falloffRatio);
            
            return Mathf.RoundToInt(ProjectileDamage * damageMultiplier);
        }
    }
}