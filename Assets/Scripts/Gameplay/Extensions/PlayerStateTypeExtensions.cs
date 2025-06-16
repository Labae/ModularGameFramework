using MarioGame.Gameplay.Enums;
using MarioGame.Gameplay.Player;

namespace MarioGame.Gameplay.Extensions
{
    /// <summary>
    /// PlayerStateType 확장 Method
    /// </summary>
    public static class PlayerStateTypeExtensions
    {
        
        /// <summary>
        /// 땅에 있는 상태인지 확인
        /// </summary>
        /// <param name="state">확인할 상태</param>
        /// <returns>땅에 있는 상태이면 true</returns>
        public static bool IsGrounded(this PlayerStateType state)
        {
            return state switch
            {
                PlayerStateType.Idle => true,
                PlayerStateType.Run => true,
                PlayerStateType.Crouch => true,
                _ => false,
            };
        }

        /// <summary>
        /// 공중에 있는 상태인지 확인
        /// </summary>
        /// <param name="state">확인할 상태</param>
        /// <returns>공중에 있는 상태이면 true</returns>
        public static bool IsAir(this PlayerStateType state)
        {
            return state switch
            {
                PlayerStateType.Jump => true,
                PlayerStateType.Fall => true,
                _ => false,
            };
        }
        
        /// <summary>
        /// 이동 가능한 상태인지 확인
        /// </summary>
        /// <param name="state">확인할 상태</param>
        /// <returns>이동 가능한 상태이면 true</returns>
        public static bool CanMove(this PlayerStateType state)
        {
            return state switch
            {
                PlayerStateType.Idle => true,
                PlayerStateType.Run => true,
                PlayerStateType.Jump => true,
                PlayerStateType.Fall => true,
                _ => false,
            };
        }
        
        /// <summary>
        /// 점프 가능한 상태인지 확인
        /// </summary>
        /// <param name="state">확인할 상태</param>
        /// <returns>점프 가능한 상태이면 true</returns>
        public static bool CanJump(this PlayerStateType state)
        {
            return state switch
            {
                PlayerStateType.Idle => true,
                PlayerStateType.Run => true,
                _ => false,
            };
        }

        public static bool CanFire(this PlayerStateType state)
        {
            return state switch
            {
                PlayerStateType.Idle => true,
                PlayerStateType.Run => true,
                PlayerStateType.Crouch => true,
                _ => false,
            };
        }

        /// <summary>
        /// 상태 전환이 유효한지 확인
        /// </summary>
        /// <param name="from">현재 상태</param>
        /// <param name="to">전환하려는 상태</param>
        /// <returns>유효한 전환이면 true</returns>
        public static bool IsValidTransition(this PlayerStateType from, PlayerStateType to)
        {
            // 같은 상태로는 전환 불가
            if (from == to)
            {
                return false;
            }

            // 사망 상태에서는 다른 상태로 전환 불가
            if (from == PlayerStateType.Dead)
            {
                return false;
            }

            // 특수 전환 규칙들
            return (from, to) switch
            {
                // 공중에서 땅 상태로 직접 전환 불가능 (Landing을 거쳐야 함) 
                (PlayerStateType.Jump, PlayerStateType.Idle) => false,
                (PlayerStateType.Jump, PlayerStateType.Run) => false,
                (PlayerStateType.Fall, PlayerStateType.Idle) => false,
                (PlayerStateType.Fall, PlayerStateType.Run) => false,
                
                // 나머지는 모두 유효
                _ => true
            };
        }
    }
}