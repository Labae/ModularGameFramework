using System;
using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Interfaces.Projectiles;
using UnityEngine;

namespace MarioGame.Gameplay.Physics.ProjectileCollision.Core
{
    /// <summary>
    /// 3개 투사체 타입용 단순화된 충돌 검사 팩토리
    /// Normal, Piercing, Hitscan만 지원
    /// 캐시 제거로 단순화
    /// </summary>
    public static class ProjectileCollisionFactory
    {
        /// <summary>
        /// ProjectileType에 따른 충돌 검사기 생성
        /// 캐시 없이 매번 새로운 인스턴스 생성
        /// </summary>
        public static IProjectileCollisionBase CreateForProjectileType(ProjectileType type)
        {
            return type switch
            {
                ProjectileType.Normal => new ProjectilePhysicsCollision(),
                ProjectileType.Piercing => new ProjectilePiercingCollision(), 
                ProjectileType.Hitscan => new ProjectileHitscanCollision(),
                _ => new ProjectilePhysicsCollision() // 기본값
            };
        }

        /// <summary>
        /// Normal/Piercing용 물리 기반 충돌 검사기 생성
        /// </summary>
        public static IProjectilePhysicsCollision CreatePhysicsCollision()
        {
            return new ProjectilePhysicsCollision();
        }

        /// <summary>
        /// Piercing 전용 충돌 검사기 생성
        /// </summary>
        public static IProjectilePiercingCollision CreatePiercingCollision()
        {
            return new ProjectilePiercingCollision();
        }

        /// <summary>
        /// Hitscan 전용 충돌 검사기 생성
        /// </summary>
        public static IProjectileHitscanCollision CreateHitscanCollision()
        {
            return new ProjectileHitscanCollision();
        }

        /// <summary>
        /// WeaponConfiguration 타입에 따른 충돌 검사기 생성
        /// Pattern Matching으로 직접 WeaponConfig 타입 체크
        /// </summary>
        public static IProjectileCollisionBase CreateForWeaponConfig(Type weaponConfigType)
        {
            if (weaponConfigType == null)
            {
                Debug.LogWarning("WeaponConfiguration type is null, using default physics collision");
                return new ProjectilePhysicsCollision();
            }

            return weaponConfigType.Name switch
            {
                "NormalWeaponConfig" => new ProjectilePhysicsCollision(),
                "PiercingWeaponConfig" => new ProjectilePiercingCollision(),
                "HitscanWeaponConfig" => new ProjectileHitscanCollision(),
                _ => new ProjectilePhysicsCollision() // 기본값
            };
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터용 팩토리 상태 출력
        /// </summary>
        [UnityEditor.MenuItem("MarioGame/Debug/Log Collision Factory Info")]
        public static void LogFactoryInfo()
        {
            Debug.Log("ProjectileCollisionFactory Info:");
            Debug.Log("- Supported Types: Normal, Piercing, Hitscan");
            Debug.Log("- No caching (new instances created each time)");
            Debug.Log("- Normal -> ProjectilePhysicsCollision");
            Debug.Log("- Piercing -> ProjectilePiercingCollision");
            Debug.Log("- Hitscan -> ProjectileHitscanCollision");
        }

        /// <summary>
        /// 에디터용 모든 타입 테스트
        /// </summary>
        [UnityEditor.MenuItem("MarioGame/Debug/Test All Collision Types")]
        public static void TestAllTypes()
        {
            var types = new[] { ProjectileType.Normal, ProjectileType.Piercing, ProjectileType.Hitscan };
            
            foreach (var type in types)
            {
                var collision = CreateForProjectileType(type);
                Debug.Log($"{type} -> {collision.GetType().Name}");
            }
        }
#endif
    }
}