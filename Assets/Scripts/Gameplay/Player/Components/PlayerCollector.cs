using System;
using MarioGame.Core;
using MarioGame.Gameplay.Interfaces.Pickups;
using MarioGame.Gameplay.Pickups;
using UnityEngine;

namespace MarioGame.Gameplay.Player.Components
{
    [DisallowMultipleComponent]
    public class PlayerCollector : CoreBehaviour
    {
        [SerializeField] private LayerMask _pickupLayer = -1;
        [SerializeField] private float _radius;

        private readonly Collider2D[] _hitColliders = new Collider2D[10];
        
        public event Action<Pickup> OnPickup; 

        private void FixedUpdate()
        {
            int size = Physics2D.OverlapCircleNonAlloc(position2D, _radius, _hitColliders, _pickupLayer);
            for (int i = 0; i < size; i++)
            {
                if (!_hitColliders[i].TryGetComponent<IPickupable>(out var weaponPickup))
                {
                    continue;
                }
                
                if (weaponPickup.TryPickup(out var pickup))
                {
                    OnPickup?.Invoke(pickup);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
#endif
    }
}