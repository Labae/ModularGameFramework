using System;
using MarioGame.Gameplay.Config.Item;
using MarioGame.Level.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Pickups
{
    public class ItemPickup : Pickup
    {
        [SerializeField] private ItemRegistry _registry;

        private ItemType _currentItemType;
        private ItemData _currentItemData;

        public override void Initialize()
        {
            base.Initialize();

            if (_ldtkFields.TryGetField("ItemType", out var field))
            {
                if (Enum.TryParse(field.GetValueAsString(), out _currentItemType))
                {
                    _currentItemData = _registry.GetItemData(_currentItemType);
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
            
            _spriteRenderer.sprite = _currentItemData.ItemSprite;
        }
    }
}