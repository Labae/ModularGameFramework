using System;
using MarioGame.Gameplay.Config.Detection;
using MarioGame.Level.Interfaces;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IGroundChecker
    {
        bool IsGrounded { get; }
        bool CanBypass { get; }
        bool HasBypassableBelow { get; }
        IBypassable CurrentBypassable { get; }
        
        event Action OnGroundEnter;
        event Action OnGroundExit;
        
        IGroundDetector GroundDetector { get; }
        GroundDetectionConfig Config { get; }
        GroundDetectionResult LastResult { get; }
    }
}