using System;

namespace MarioGame.Gameplay.Enums
{
    /// <summary>
    /// 플레이어의 모든 상태를 정의하는 Enum
    /// StateMachine에서 사용
    /// </summary>
    public enum PlayerStateType 
    {
        /// <summary>
        /// 정지 상태 - 땅에 서있고 움직이지 않음
        /// </summary>
        Idle = 0,
        
        /// <summary>
        /// 달리기 상태 - 땅에서 좌우 이동
        /// </summary>
        Run,
        
        /// <summary>
        /// 점프 상태 - 상승 중 (Y 속도 > 0)
        /// </summary>
        Jump,
        
        /// <summary>
        /// 낙하 상태 - 하강 중 (Y 속도 < 0)
        /// </summary>
        Fall,
        
        /// <summary>
        /// 웅크림 상태
        /// </sumdary>
        Crouch,
        
        /// <summary>
        /// 오르기 상태
        /// </sumdary>
        Climb,
        
        /// <summary>
        /// 사망 상태 - 체력이 0이 되었을 때
        /// </summary>
        Dead,
    }
}