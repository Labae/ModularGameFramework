using System;
using Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gameplay.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : CoreBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        [SerializeField] private float _distance = 1.0f;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            Assert.IsNotNull(_rigidbody2D, "Rigidbody가 없습니다");
        }
        
        public void Fire(float fireForce)
        {
            _rigidbody2D.AddForce(transform.right * fireForce, ForceMode2D.Impulse);
        }

        private void FixedUpdate()
        {
            if (Physics2D.Raycast(transform.position, 
                    transform.right, 
                    _distance, 
                    1 << LayerMask.NameToLayer("Ground")))
            {
                Destroy(gameObject);
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * _distance);
        }
#endif
    }
}