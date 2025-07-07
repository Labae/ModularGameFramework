using System;
using MarioGame.Audio;
using MarioGame.Audio.Interfaces;
using MarioGame.Gameplay.Config.Item;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces.Pickups;
using MarioGame.Level.Enums;
using Reflex.Attributes;
using UnityEngine;

namespace MarioGame.Gameplay.Pickups
{
    public class WeaponPickup : Pickup, IPickupable
    {
        [SerializeField] private WeaponRegistry _registry;

        private WeaponType _currentWeaponType;
        private WeaponData _currentWeaponData;

        [Inject]
        private IAudioManager _audioManager;
        
        public WeaponConfiguration Config => _currentWeaponData.Configuration;

        public bool CanBePickedUp => true;
        public event Action OnPickup;
        
        public bool TryPickup(out Pickup pickup)
        {
            if (!CanBePickedUp)
            {
                pickup = null;
                return false;
            }

            pickup = this;
            OnPickup?.Invoke();
            return true;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (_ldtkFields.TryGetField("WeaponType", out var field))
            {
                if (Enum.TryParse(field.GetValueAsString(), out _currentWeaponType))
                {
                    _currentWeaponData = _registry.GetWeaponData(_currentWeaponType);
                }
                else
                {
                    _debugLogger.Error($"Failed to parse ItemType : {field.GetValueAsString()}");
                    return;
                }
            }
            else
            {
                _debugLogger.Error("Failed to get ItemType field");
                return;
            }
            
            _spriteRenderer.sprite = _currentWeaponData.WeaponSprite;
            OnPickup += HandlePickup;
        }

        protected override void HandleDestruction()
        {
            OnPickup -= HandlePickup;
            base.HandleDestruction();
        }

        private void HandlePickup()
        {
            _audioManager.PlaySFX(_currentWeaponData.PickupSFX);
            Destroy(gameObject);
        }
    }
}