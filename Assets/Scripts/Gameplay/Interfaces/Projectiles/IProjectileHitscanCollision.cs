using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    /// <summary>
    /// 즉시 명중 투사체 충돌 검사 인터페이스
    /// Hitscan 타입에서 사용
    /// 즉시 레이캐스트로 충돌 검사하는 무기용
    /// </summary>
    public interface IProjectileHitscanCollision : IProjectileCollisionBase
    {
        /// <summary>
        /// 즉시 명중 검사 (레이캐스트 기반)
        /// MaxRange까지 즉시 충돌 검사
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="direction">방향</param>
        /// <param name="weaponConfig">Hitscan 무기 설정</param>
        /// <returns>충돌 결과 배열 (관통 지원)</returns>
        ProjectileHitData[] CheckMultipleCollisions(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig);

        /// <summary>
        /// 즉시 단일 충돌 검사
        /// 가장 가까운 충돌만 반환
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="direction">방향</param>
        /// <param name="weaponConfig">Hitscan 무기 설정</param>
        /// <returns>첫 번째 충돌 결과</returns>
        ProjectileHitData CheckInstantHit(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig);

        /// <summary>
        /// 최대 사거리 내 모든 충돌 검사
        /// IgnoreWalls 설정 고려
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="direction">방향</param>
        /// <param name="maxRange">최대 사거리</param>
        /// <param name="weaponConfig">Hitscan 무기 설정</param>
        /// <returns>사거리 내 모든 충돌 결과</returns>
        ProjectileHitData[] CheckRangedHits(Vector2 origin, Vector2 direction, float maxRange, WeaponConfiguration weaponConfig);

        /// <summary>
        /// 레이저 빔 형태 연속 충돌 검사
        /// 레이저 타입 무기용
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="direction">방향</param>
        /// <param name="weaponConfig">무기 설정</param>
        /// <returns>빔 경로상 모든 충돌</returns>
        ProjectileHitData[] CheckBeamCollision(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig);
    }
}