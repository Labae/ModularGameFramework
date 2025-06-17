using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Weapon
{
    /// <summary>
    /// 관통 투사체용 WeaponConfiguration
    /// 적이나 오브젝트를 관통할 수 있는 투사체
    /// </summary>
    [CreateAssetMenu(menuName = "MarioGame/Weapon/Piercing Weapon Config", fileName = "New Piercing Weapon")]
    public class PiercingWeaponConfig : WeaponConfiguration
    {
        [Header("Piercing Settings")]
        [Tooltip("최대 관통 횟수")]
        public int MaxPenetrations = 3;
        
        [Tooltip("관통할 때마다 데미지 감소량")]
        public int DamageReductionPerHit = 0;
        
        [Tooltip("관통할 때마다 속도 감소율 (0~1)")]
        [Range(0f, 1f)]
        public float SpeedReductionPerHit = 0.1f;
        
        [Tooltip("벽을 관통할 수 있는지 여부")]
        public bool CanPenetrateWalls = false;

        void Reset()
        {
            // 기본값 설정
            ProjectileType = ProjectileType.Piercing;
            WeaponName = "Piercing Weapon";
            ProjectileSpeed = 20f;
            ProjectileDamage = 2;
            ProjectileLifetime = 4f;
            UseGravity = false;
            RotateWithVelocity = true;
            MaxPenetrations = 3;
        }

        /// <summary>
        /// 관통 투사체의 관통 가능 여부 판별
        /// </summary>
        public override bool CanPenetrateTarget(HitTargetType targetType)
        {
            return targetType switch
            {
                HitTargetType.Entity => true, // 엔티티는 항상 관통 가능
                HitTargetType.Destructible => true, // 파괴 가능한 오브젝트도 관통
                HitTargetType.Wall => CanPenetrateWalls, // 벽은 설정에 따라
                _ => false
            };
        }
    }
}