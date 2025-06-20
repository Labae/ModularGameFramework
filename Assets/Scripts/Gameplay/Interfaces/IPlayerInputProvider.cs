namespace Gameplay.Interfaces
{
    public interface IPlayerInputProvider
    {
        float HorizontalInput { get; }
        bool ShootPressed { get; }

        void Update();
    }
}