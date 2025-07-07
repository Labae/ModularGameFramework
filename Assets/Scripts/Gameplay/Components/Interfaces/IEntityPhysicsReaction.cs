using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Config.Reactions;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Interfaces
{
    public interface IEntityPhysicsReaction
    {
        bool HasActiveReaction(PhysicsReactionType reactionType);
        bool HasAnyActiveReaction { get; }

        event Action OnKnockbackEnded;
        event Action OnStunEnded;

        void Initialize(EntityPhysicsReactionConfig config);
        void UpdatePhysics(Rigidbody2D rigidbody);
        UniTask ApplyKnockbackAsync(KnockbackData knockbackData, CancellationToken cancellationToken = default);
        UniTask ApplyStunAsync(float duration, CancellationToken cancellationToken = default);
        void StopReaction(PhysicsReactionType reactionType);
        void StopAllReactions();
    }
}