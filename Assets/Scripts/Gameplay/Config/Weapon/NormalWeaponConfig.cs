using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Weapon
{
    /// <summary>
    /// 일반 물리 투사체용 WeaponConfiguration
    /// 기본적인 물리 기반 투사체 (총알, 화살 등)
    /// </summary>
    [CreateAssetMenu(menuName = "MarioGame/Weapon/Normal Weapon Config", fileName = "New Normal Weapon")]
    public class NormalWeaponConfig : WeaponConfiguration
    {
        [Header("Normal Projectile Settings")]
        [Tooltip("투사체가 튕길 수 있는지 여부")]
        public bool CanBounce = false;
        
        [Tooltip("최대 튕김 횟수")]
        public int MaxBounces = 1;
        
        [Tooltip("튕김 시 속도 유지율 (0~1)")]
        [Range(0f, 1f)]
        public float BounceVelocityRetention = 0.8f;

        void Reset()
        {
            // 기본값 설정
            ProjectileType = ProjectileType.Normal;
            WeaponName = "Normal Weapon";
            ProjectileSpeed = 15f;
            ProjectileDamage = 1;
            ProjectileLifetime = 3f;
            UseGravity = false;
            RotateWithVelocity = true;
        }

        /// <summary>
        /// 일반 투사체는 기본적으로 관통 불가
        /// </summary>
        public override bool CanPenetrateTarget(HitTargetType targetType)
        {
            return false; // 일반 투사체는 관통 불가
        }
    }
}