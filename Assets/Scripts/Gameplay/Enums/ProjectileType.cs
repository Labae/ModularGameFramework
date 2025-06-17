namespace MarioGame.Gameplay.Enums
{
    public enum ProjectileType
    {
        /// <summary>
        /// 직선 투사체
        /// Raycast
        /// </summary>
        Normal,
        
        /// <summary>
        /// 관통 투사체
        /// Raycast
        /// </summary>
        Piercing,
        
        /// <summary>
        /// 즉시 명중
        /// 거리 무관
        /// </summary>
        Hitscan,
        
        /// <summary>
        /// 폭발형 투사체
        /// Overlay 충돌 검사
        /// </summary>
        Explosive,
        
        /// <summary>
        /// 수류탄 (궤도 + 폭발)
        /// Hybrid충돌 검사
        /// </summary>
        Grenade,
        
        /// <summary>
        /// 튕기는 투사체
        /// Hybrid
        /// </summary>
        Bouncing,
        
        /// <summary>
        /// 유도 미사일
        /// Hybrid 충돌 검사
        /// </summary>
        Homing,
        
        /// <summary>
        /// 도탄
        /// Hybrid 충돌 검사
        /// </summary>
        Ricochet,
        
        /// <summary>
        /// 레이저 빔
        /// Raycast 충돌 검사(Multiple)
        /// 즉시 명중
        /// </summary>
        Laser,
        
        /// <summary>
        /// 플라즈마 볼
        /// Hybrid 충돌 검사
        /// </summary>
        Plasma,
        
        /// <summary>
        /// 마법 투사체
        /// Overlap 충돌 검사
        /// </summary>
        Magic,
    }
}