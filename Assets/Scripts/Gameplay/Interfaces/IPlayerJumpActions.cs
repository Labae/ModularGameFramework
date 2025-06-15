namespace MarioGame.Gameplay.Interfaces
{
    public interface IPlayerJumpActions
    {
        bool HasPendingJump { get; }

        /// <summary>
        /// 점프 시도
        /// </summary>
        /// <param name="forceMultiplier"></param>
        /// <returns></returns>
        bool TryJump(float forceMultiplier = 1.0f);

        void RequestJump();

        void CutJump();

        void SetJumpInputHeld(bool held);
    }
}