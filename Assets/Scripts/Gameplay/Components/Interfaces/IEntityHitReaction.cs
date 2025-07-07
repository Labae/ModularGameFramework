using System.Threading;
using Cysharp.Threading.Tasks;
using MarioGame.Gameplay.Config.Reactions;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IEntityHitReaction
    {
        bool IsEffectActive { get; }
        Color CurrentColor { get; }
        void Initialize(EntityHitReactionConfig config, Color originalColor);
        UniTask PlayHitEffectAsync(bool isCritical, CancellationToken cancellationToken = default);
        void StopEffect();
    }
}