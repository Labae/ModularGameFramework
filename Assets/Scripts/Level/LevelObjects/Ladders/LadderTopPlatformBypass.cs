using System;
using System.Collections;
using MarioGame.Core;
using MarioGame.Core.Entities;
using MarioGame.Core.Utilities;
using MarioGame.Level.Interfaces;
using UnityEngine;

namespace MarioGame.Level.LevelObjects.Ladders
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PlatformEffector2D))]
    [DisallowMultipleComponent]
    public class LadderTopPlatformBypass : CoreBehaviour, IBypassable
    {
        [Header("Bypass Settings")] [SerializeField]
        private float _maxBypassDistance = 5.0f;

        private PlatformEffector2D _platformEffector;
        private BoxCollider2D _platformCollider;
        private bool _isBypassing = false;

        public bool IsBypassing => _isBypassing;
        public bool CanBypass => !_isBypassing && _platformCollider != null;

        private WaitForSeconds _bypassDurationWait;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _platformCollider = GetComponent<BoxCollider2D>();
            _platformEffector = GetComponent<PlatformEffector2D>();

            AssertIsNotNull(_platformCollider, "_platformCollider required");
            AssertIsNotNull(_platformEffector, "_platformEffector required");
        }

        public bool TryBypass(Entity entity)
        {
            if (!CanBypass || entity == null)
            {
                return false;
            }

            var entityCollider = entity.GetComponentInChildren<Collider2D>();
            if (entityCollider == null)
            {
                LogWarning("Failed to get collider from entity");
                return false;
            }

            _isBypassing = true;
            StartCoroutine(BypassCoroutine(entity, entityCollider));
            return true;
        }

        private IEnumerator BypassCoroutine(Entity entity, Collider2D entityCollider)
        {
            var platformBottom = _platformCollider.bounds.min.y;
            var platformTop = _platformCollider.bounds.max.y;

            entity.transform.position += Vector3.down * 0.1f;
            var initEntityPosition = entity.position2D;

            Physics2D.IgnoreCollision(entityCollider, _platformCollider, true);

            bool hasEnteredPlatform = false;

            while (true)
            {
                if (entity == null || entityCollider == null)
                {
                    break;
                }
                
                var distanceFromStart = Vector3.Distance(entity.position2D, initEntityPosition);

                if (distanceFromStart >= _maxBypassDistance)
                {
                    break;
                }
                
                var entityTop = entityCollider.bounds.max.y;
                var entityBottom = entityCollider.bounds.min.y;

                if (!hasEnteredPlatform && entityTop < platformTop && entityBottom > platformBottom)
                {
                    hasEnteredPlatform = true;
                }

                if (hasEnteredPlatform && entityBottom > platformTop)
                {
                    break;
                }

                if (entityTop < platformBottom)
                {
                    break;
                }

                yield return null;
            }

            Physics2D.IgnoreCollision(entityCollider, _platformCollider, false);

            _isBypassing = false;
        }

        public void ForceStopBypass(Entity entity)
        {
            if (!_isBypassing || entity == null)
            {
                return;
            }

            var entityCollider = entity.GetComponentInChildren<Collider2D>();
            if (entityCollider == null)
            {
                LogWarning("Failed to get collider from entity");
                return;
            }

            Physics2D.IgnoreCollision(entityCollider, _platformCollider, false);
            StopAllCoroutines();
            _isBypassing = false;
        }
    }
}