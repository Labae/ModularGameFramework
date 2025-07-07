using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Projectiles.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    /// <summary>
    /// 관통 투사체 충돌 검사 인터페이스
    /// Piercing 타입에서 사용
    /// 물리 기반 + 관통 특화 기능
    /// </summary>
    public interface IProjectilePiercingCollision : IProjectileCollisionBase
    {
        /// <summary>
        /// 관통 대응 다중 충돌 검사
        /// 관통 횟수와 관통 가능 타겟 고려
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="direction">방향</param>
        /// <param name="weaponConfig">Piercing 무기 설정</param>
        /// <returns>관통 순서대로 정렬된 충돌 결과</returns>
        ProjectileHitData[] CheckMultipleCollisions(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig);

        /// <summary>
        /// 관통 가능 여부 체크
        /// 타겟 타입과 남은 관통 횟수 고려
        /// </summary>
        /// <param name="hitData">충돌 데이터</param>
        /// <param name="remainingPenetrations">남은 관통 횟수</param>
        /// <param name="weaponConfig">무기 설정</param>
        /// <returns>관통 가능 여부</returns>
        bool CanPenetrateTarget(ProjectileHitData hitData, int remainingPenetrations, WeaponConfiguration weaponConfig);

        /// <summary>
        /// 관통 후 궤도 계산
        /// 관통 시 투사체 방향/속도 변화 계산
        /// </summary>
        /// <param name="currentDirection">현재 방향</param>
        /// <param name="currentVelocity">현재 속도</param>
        /// <param name="hitData">충돌 데이터</param>
        /// <param name="weaponConfig">무기 설정</param>
        /// <returns>관통 후 (방향, 속도)</returns>
        (Vector2 newDirection, float newSpeed) CalculatePostPenetrationTrajectory(
            Vector2 currentDirection, 
            float currentVelocity, 
            ProjectileHitData hitData, 
            WeaponConfiguration weaponConfig);

        /// <summary>
        /// 경로 기반 관통 충돌 검사
        /// 이전 위치에서 현재 위치까지의 경로상 모든 관통 처리
        /// </summary>
        /// <param name="previousPosition">이전 위치</param>
        /// <param name="currentPosition">현재 위치</param>
        /// <param name="weaponConfig">무기 설정</param>
        /// <returns>경로상 모든 충돌 (관통 순서)</returns>
        ProjectileHitData[] CheckPiercingPath(Vector2 previousPosition, Vector2 currentPosition, WeaponConfiguration weaponConfig);
    }
}