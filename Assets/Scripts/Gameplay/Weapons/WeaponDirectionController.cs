using MarioGame.Core.Utilities;
using MarioGame.Gameplay.Interfaces.Weapon;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    /// <summary>
    /// 무기 방향 제어
    /// </summary>
    public class WeaponDirectionController : IWeaponDirectionController
    {
        private readonly Transform _pivot;

        public Vector2 CurrentDirection { get; private set; }
        
        public WeaponDirectionController(Transform pivot)
        {
            _pivot = pivot;
        }
        
        public void Initialize()
        {
            CurrentDirection = Vector2.zero;
            UpdatePivotRotation();
        }

        public void Dispose()
        {
        }

        public void SetDirection(Vector2 direction)
        {
            CurrentDirection = direction;
            UpdatePivotRotation();
        }

        private void UpdatePivotRotation()
        {
            if (_pivot == null) return;

            if (CurrentDirection.x > FloatUtility.EPSILON)
            {
                _pivot.localRotation = Quaternion.identity;
            }
            else if (CurrentDirection.x < -FloatUtility.EPSILON)
            {
                _pivot.localRotation = Quaternion.Euler(0f, 180f, 0f);

            }
        }
    }
}