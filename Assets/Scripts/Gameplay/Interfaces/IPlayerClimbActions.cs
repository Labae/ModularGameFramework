namespace MarioGame.Gameplay.Interfaces
{
    public interface IPlayerClimbActions
    {
        bool IsClimbing { get; }
        float ClimbVelocity { get; }
        
        void StartClimbing();
        void StopClimbing();
        void ClimbUp(float multiplier = 1.0f);
        void ClimbDown(float multiplier = 1.0f);
        void JumpFromLadder(float jumpForceMultiplier = 1.0f);
    }
}