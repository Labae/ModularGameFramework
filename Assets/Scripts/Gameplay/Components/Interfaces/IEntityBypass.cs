using MarioGame.Gameplay.Config.Movement;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IEntityBypass
    {
        bool EnableBypass { get; set; }
        bool CanBypass { get; }
        bool IsOnCooldown { get; }
        
        void Initialize(EntityBypassConfig config);
        void Update(); // Entity에서 호출할 Update
        bool TryBypass(float verticalInput);
        bool ForceBypass();
    }
}