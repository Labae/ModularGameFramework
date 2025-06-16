using System;
using MarioGame.Core.Entities;
using MarioGame.Core.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Misc
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class PlayerBullet : Entity
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        [SerializeField] private float _fireForce = 5.0f;
        [SerializeField] private float _lifeTime = 5.0f;
        
        [Header("Collision Settings")]
        [SerializeField] private LayerMask _collisionMask;

        [SerializeField] private GameObject _hitPrefab;
        
        private Vector2 _direction;
        private float _spawnTime;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _renderer = GetComponentInChildren<SpriteRenderer>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            if (Time.time - _spawnTime >= _lifeTime)
            {
                Destroy(gameObject);
            }
        }

        public void Fire(HorizontalDirectionType directionType)
        {
            _spawnTime = Time.time;
            bool isRight = directionType == HorizontalDirectionType.Right;

            var force = Vector2.right * _fireForce;
            if (!isRight)
            {
                _direction = Vector2.left;
                force = Vector2.left * _fireForce;
                _renderer.flipX = true;
            }
            else
            {
                _direction = Vector2.right;
                _renderer.flipX = false;
            }

            _rigidbody2D.AddForce(force, ForceMode2D.Impulse);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((1 << other.gameObject.layer & _collisionMask) == 0)
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                return;
            }

            ProcessHit(other);
        }

        private void ProcessHit(Collider2D other)
        {
            var hitPosition = transform.position;
            CreateHitEffect(hitPosition);
            Destroy(gameObject);
        }

        private void CreateHitEffect(Vector3 hitPosition)
        {
            var hitEffect = Instantiate(_hitPrefab, hitPosition, Quaternion.identity);
            if (hitEffect.TryGetComponent<BulletHit>(out var bulletHit))
            {
                bulletHit.Play(hitPosition, _direction);
            }
            else
            {
                Destroy(hitEffect);
            }
        }
    }
}