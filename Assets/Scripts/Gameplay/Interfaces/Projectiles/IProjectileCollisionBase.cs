namespace MarioGame.Gameplay.Interfaces.Projectiles
{
    /// <summary>
    /// 모든 투사체 충돌 검사 시스템의 공통 베이스 마커 인터페이스
    /// 
    /// 목적:
    /// - ProjectileCollisionFactory의 캐시 시스템에서 공통 타입으로 사용
    /// - 다양한 충돌 검사 인터페이스들의 공통 부모 역할
    /// - 실제 메서드는 포함하지 않음 (마커 역할만)
    /// 
    /// 사용법:
    /// - IProjectileCollision, IProjectilePhysicsCollision 등이 이를 상속
    /// - Factory에서 IProjectileCollisionBase로 캐시 관리
    /// - 실제 사용 시 구체적인 인터페이스로 캐스팅
    /// </summary>
    public interface IProjectileCollisionBase
    {
        // 마커 인터페이스 - 메서드 없음
        // 모든 충돌 검사 시스템의 공통 식별자 역할
    }
}