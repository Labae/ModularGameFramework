using System;
using MarioGame.Gameplay.Config.Detection;
using MarioGame.Level.LevelObjects.Ladders;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface ILadderChecker
    {
        bool IsOnLadder { get; }
        bool IsAtLadderTop { get; }
        bool IsAtLadderBottom { get; }
        Ladder CurrentLadder { get; }
        
        event Action OnLadderEnter;
        event Action OnLadderExit;
        event Action OnLadderTopReached;
        event Action OnLadderBottomReached;
        
        bool TryGetLadderAlignPosition(out float alignX);
        bool TryGetLadderYLimits(out float minY, out float maxY);
        
        ILadderDetector LadderDetector { get; }
        LadderDetectionConfig Config { get; }
        LadderDetectionResult LastResult { get; }
    }
}