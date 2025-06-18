using System;
using System.Collections;
using System.Collections.Generic;
using MarioGame.Core;
using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class EntityPhysicsReaction : CoreBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        private EntityMovement _entityMovement;
        private EntityJump _entityJump;
        private EntityClimb _entityClimb;

        private EntityHealth _entityHealth;

        private readonly Dictionary<PhysicsReactionType, Coroutine> _activeReactions = new();

        [SerializeField] private KnockbackData _cirticalKnockbackData;

        public event Action OnKnockbackEnded;
        public event Action OnStunEnded;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _entityMovement = GetComponent<EntityMovement>();
            _entityJump = GetComponent<EntityJump>();
            _entityClimb = GetComponent<EntityClimb>();
            _entityHealth = GetComponent<EntityHealth>();
            AssertIsNotNull(_rigidbody2D, "Rigidbody2D required");
            AssertIsNotNull(_entityHealth, "EntityHealth required");
        }

        private void Start()
        {
            _entityHealth.OnDamageTaken += OnDamageTaken;
        }

        private void OnDestroy()
        {
            if (_entityHealth != null)
            {
                _entityHealth.OnDamageTaken -= OnDamageTaken;
            }
        }

        #region Callbacks

        private void OnDamageTaken(DamageEventData eventData)
        {
            if (eventData.DamageInfo.WasCritical)
            {
                _cirticalKnockbackData.Direction = -eventData.DamageInfo.DamageDirection;
                ApplyKnockback(_cirticalKnockbackData);
            }
        }

        #endregion

        #region Knockback

        public void ApplyKnockback(KnockbackData knockbackData)
        {
            ForceInterruptAllActions();

            StopReaction(PhysicsReactionType.Knockback);

            if (knockbackData.Force <= FloatUtility.VELOCITY_THRESHOLD)
            {
                return;
            }

            var coroutine = StartCoroutine(KnockbackCoroutine(knockbackData));
            _activeReactions[PhysicsReactionType.Knockback] = coroutine;
        }

        private IEnumerator KnockbackCoroutine(KnockbackData knockbackData)
        {
            LockAllMovement();

            var direction = knockbackData.Direction.normalized;
            var elapsedTime = 0f;

            while (elapsedTime < knockbackData.Duration)
            {
                var ratio = elapsedTime / knockbackData.Duration;
                var fallOff = knockbackData.DistanceFalloff.Evaluate(ratio);
                var force = knockbackData.Force * fallOff;
                var velocity = direction * force;

                _rigidbody2D.velocity = velocity;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _rigidbody2D.velocity = Vector2.zero;
            UnlockAllMovement();
            _activeReactions.Remove(PhysicsReactionType.Knockback);

            OnKnockbackEnded?.Invoke();
        }

        #endregion

        #region Force Interruption

        private void ForceInterruptAllActions()
        {
            if (_entityClimb != null && _entityClimb.IsClimbing)
            {
                _entityClimb.StopClimbing();
            }

            if (_entityJump != null && (_entityJump.IsRising || !_entityJump.IsGrounded))
            {
                _entityJump.CutJump();
            }

            if (_entityMovement != null && _entityMovement.IsMoving)
            {
                _entityMovement.Stop();
            }
        }

        private void LockAllMovement()
        {
            _entityMovement?.AddLock(EntityMovementLockType.PhysicsReaction);
            _entityJump?.AddLock(EntityMovementLockType.PhysicsReaction);
            _entityClimb?.AddLock(EntityMovementLockType.PhysicsReaction);
        }

        private void UnlockAllMovement()
        {
            _entityMovement?.RemoveLock(EntityMovementLockType.PhysicsReaction);
            _entityJump?.RemoveLock(EntityMovementLockType.PhysicsReaction);
            _entityClimb?.RemoveLock(EntityMovementLockType.PhysicsReaction);
        }

        #endregion

        private void StopReaction(PhysicsReactionType reactionType)
        {
            if (_activeReactions.TryGetValue(reactionType, out var coroutine))
            {
                StopCoroutine(coroutine);
                _activeReactions.Remove(reactionType);
                CleanupReaction(reactionType);
            }
        }

        private void StopAllReaction()
        {
            var reactionTypes = _activeReactions.Keys;
            foreach (var reactionType in reactionTypes)
            {
                StopReaction(reactionType);
            }
        }

        private void CleanupReaction(PhysicsReactionType reactionType)
        {
            switch (reactionType)
            {
                case PhysicsReactionType.Knockback:
                    UnlockAllMovement();
                    OnKnockbackEnded?.Invoke();
                    break;
                case PhysicsReactionType.Stun:
                    UnlockAllMovement();
                    OnStunEnded?.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reactionType), reactionType, null);
            }
        }
    }
}