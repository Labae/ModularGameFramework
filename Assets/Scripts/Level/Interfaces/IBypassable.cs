using MarioGame.Core.Entities;

namespace MarioGame.Level.Interfaces
{
    public interface IBypassable
    {
        bool IsBypassing { get; }
        
        bool CanBypass { get; }
        bool TryBypass(Entity entity);
    }
}