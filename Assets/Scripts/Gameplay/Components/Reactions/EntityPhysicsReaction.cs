using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MarioGame.Core;
using MarioGame.Core.Utilities;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Components.Stats;
using MarioGame.Gameplay.Config.Reactions;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Reactions
{
    public class EntityPhysicsReaction : IEntityPhysicsReaction
    {
        private readonly IDebugLogger _logger;
        private readonly IIntentBasedMovement _movement;
        private readonly IGroundChecker _groundChecker;
        private readonly IEntityJump _jump;
        private readonly IEntityClimb _climb;

        private EntityPhysicsReactionConfig _config;
        private readonly Dictionary<PhysicsReactionType, CancellationTokenSource> _activeReactions = new();
        private Vector2 _currentVelocity;

        public bool HasAnyActiveReaction => _activeReactions.Count > 0;

        public event Action OnKnockbackEnded;
        public event Action OnStunEnded;

        public EntityPhysicsReaction(IDebugLogger logger, IIntentBasedMovement movement,
            IGroundChecker groundChecker,
            IEntityJump jump, IEntityClimb climb)
        {
            _logger = logger;
            _movement = movement;
            _groundChecker = groundChecker;
            _jump = jump;
            _climb = climb;
        }

        public void Initialize(EntityPhysicsReactionConfig config)
        {
            _config = config;
            _logger?.StateMachine("PhysicsReaction initialized");
        }

        public bool HasActiveReaction(PhysicsReactionType reactionType)
        {
            return _activeReactions.ContainsKey(reactionType);
        }

        public void UpdatePhysics(Rigidbody2D rigidbody)
        {
            if (rigidbody == null) return;

            // 물리 반응 중일 때만 velocity 직접 제어
            if (HasAnyActiveReaction)
            {
                rigidbody.velocity = _currentVelocity;
            }
        }

        public async UniTask ApplyKnockbackAsync(KnockbackData knockbackData,
            CancellationToken cancellationToken = default)
        {
            if (_config?.EnableKnockback != true || knockbackData.Force <= (_config?.MinKnockbackForce ?? 0.1f))
            {
                return;
            }

            // 모든 동작 강제 중단
            ForceInterruptAllActions();

            // 기존 넉백 중단
            StopReaction(PhysicsReactionType.Knockback);

            // 새 넉백 시작
            var reactionCancellation = new CancellationTokenSource();
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                reactionCancellation.Token).Token;

            _activeReactions[PhysicsReactionType.Knockback] = reactionCancellation;

            try
            {
                await ExecuteKnockbackAsync(knockbackData, combinedToken);
            }
            catch (OperationCanceledException)
            {
                _logger?.StateMachine("Knockback cancelled");
            }
            finally
            {
                CleanupReaction(PhysicsReactionType.Knockback);
            }
        }

        private async UniTask ExecuteKnockbackAsync(KnockbackData knockbackData, CancellationToken cancellationToken)
        {
            LockAllMovement();

            var direction = knockbackData.Direction.normalized;
            var duration = knockbackData.Duration;
            var elapsedTime = 0f;

            _logger?.StateMachine($"Knockback started - Force: {knockbackData.Force}, Duration: {duration}");

            while (elapsedTime < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var ratio = elapsedTime / duration;
                var fallOff = knockbackData.DistanceFalloff.Evaluate(ratio);
                var force = knockbackData.Force * fallOff;
                _currentVelocity = direction * force;

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            _currentVelocity = Vector2.zero;
            _logger?.StateMachine("Knockback completed");
        }

        public async UniTask ApplyStunAsync(float duration, CancellationToken cancellationToken = default)
        {
            if (_config?.EnableStun != true || duration <= 0f)
            {
                return;
            }

            // 기존 스턴 중단
            StopReaction(PhysicsReactionType.Stun);

            // 새 스턴 시작
            var reactionCancellation = new CancellationTokenSource();
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                reactionCancellation.Token).Token;

            _activeReactions[PhysicsReactionType.Stun] = reactionCancellation;

            try
            {
                await ExecuteStunAsync(duration, combinedToken);
            }
            catch (OperationCanceledException)
            {
                _logger?.StateMachine("Stun cancelled");
            }
            finally
            {
                CleanupReaction(PhysicsReactionType.Stun);
            }
        }

        private async UniTask ExecuteStunAsync(float duration, CancellationToken cancellationToken)
        {
            LockAllMovement();
            _currentVelocity = Vector2.zero;

            _logger?.StateMachine($"Stun started - Duration: {duration}");

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken);

            _logger?.StateMachine("Stun completed");
        }

        private void ForceInterruptAllActions()
        {
            if (_climb?.IsClimbing == true)
            {
                _climb.StopClimbing();
            }

            if (_jump?.IsRising == true || _groundChecker?.IsGrounded == false)
            {
                _jump.CutJump();
            }

            if (_movement?.IsMoving == true)
            {
                _movement.Stop();
            }
        }

        private void LockAllMovement()
        {
            _movement?.AddLock(EntityMovementLockType.PhysicsReaction);
            _jump?.AddLock(EntityMovementLockType.PhysicsReaction);
            _climb?.AddLock(EntityMovementLockType.PhysicsReaction);
        }

        private void UnlockAllMovement()
        {
            _movement?.RemoveLock(EntityMovementLockType.PhysicsReaction);
            _jump?.RemoveLock(EntityMovementLockType.PhysicsReaction);
            _climb?.RemoveLock(EntityMovementLockType.PhysicsReaction);
        }

        public void StopReaction(PhysicsReactionType reactionType)
        {
            if (_activeReactions.TryGetValue(reactionType, out var cancellationSource))
            {
                cancellationSource.Cancel();
                // CleanupReaction은 UniTask의 finally에서 호출됨
            }
        }

        public void StopAllReactions()
        {
            var reactionTypes = new List<PhysicsReactionType>(_activeReactions.Keys);
            foreach (var reactionType in reactionTypes)
            {
                StopReaction(reactionType);
            }
        }

        private void CleanupReaction(PhysicsReactionType reactionType)
        {
            if (_activeReactions.TryGetValue(reactionType, out var cancellationSource))
            {
                cancellationSource.Dispose();
                _activeReactions.Remove(reactionType);
            }

            // 모든 반응이 끝났으면 움직임 잠금 해제
            if (!HasAnyActiveReaction)
            {
                UnlockAllMovement();
                _currentVelocity = Vector2.zero;
            }

            // 이벤트 발생
            switch (reactionType)
            {
                case PhysicsReactionType.Knockback:
                    OnKnockbackEnded?.Invoke();
                    break;
                case PhysicsReactionType.Stun:
                    OnStunEnded?.Invoke();
                    break;
            }

            _logger?.StateMachine($"Physics reaction cleanup: {reactionType}");
        }

        // Health 이벤트 연동용
        public async UniTask OnDamageTakenAsync(DamageEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (_config == null || !eventData.DamageInfo.WasCritical) return;

            var knockbackData = _config.CriticalKnockbackData;
            knockbackData.Direction = -eventData.DamageInfo.DamageDirection;

            await ApplyKnockbackAsync(knockbackData, cancellationToken);
        }
    }
}