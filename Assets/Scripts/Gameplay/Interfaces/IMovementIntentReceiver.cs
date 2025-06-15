using MarioGame.Gameplay.Movement;

namespace MarioGame.Gameplay.Interfaces
{
    public interface IMovementIntentReceiver
    {
        /// <summary>
        /// 현재 움직임 의도 조회
        /// </summary>
        MovementIntent CurrentIntent { get; }
        
        /// <summary>
        /// 움직임 의도 설정
        /// </summary>
        /// <param name="intent"></param>
        void SetMovementIntent(MovementIntent intent);
    }
}