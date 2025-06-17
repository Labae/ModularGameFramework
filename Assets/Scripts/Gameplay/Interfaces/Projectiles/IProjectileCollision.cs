using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Physics.ProjectileCollision.Core;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    /// <summary>
    /// 물리 기반 투사체 충돌 검사 인터페이스
    /// Normal, Piercing 타입에서 사용
    /// 실제 물리적으로 날아가는 투사체의 충돌 검사
    /// </summary>
    public interface IProjectilePhysicsCollision : IProjectileCollisionBase
    {
        /// <summary>
        /// 다중 충돌 검사 (관통 지원)
        /// WeaponConfiguration을 직접 받아서 처리
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="direction">방향</param>
        /// <param name="weaponConfig">무기 설정</param>
        /// <returns>충돌 결과 배열</returns>
        ProjectileHitData[] CheckMultipleCollisions(Vector2 origin, Vector2 direction, WeaponConfiguration weaponConfig);

        /// <summary>
        /// 경로 기반 충돌 검사 (이전 위치 → 현재 위치)
        /// 빠른 투사체의 tunneling 방지용
        /// </summary>
        /// <param name="previousPosition">이전 위치</param>
        /// <param name="currentPosition">현재 위치</param>
        /// <param name="weaponConfig">무기 설정</param>
        /// <returns>충돌 결과</returns>
        ProjectileHitData[] CheckPathCollision(Vector2 previousPosition, Vector2 currentPosition, WeaponConfiguration weaponConfig);

        /// <summary>
        /// 반사/튕김 방향 계산
        /// Normal 타입의 CanBounce 기능용
        /// </summary>
        /// <param name="incomingDirection">입사 방향</param>
        /// <param name="hitNormal">충돌 표면 법선</param>
        /// <returns>반사 방향</returns>
        Vector2 CalculateReflectionDirection(Vector2 incomingDirection, Vector2 hitNormal);
    }
}