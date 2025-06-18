using System;
using System.Collections.Generic;
using MarioGame.Level.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Item
{
    [CreateAssetMenu(menuName = "MarioGame/Gameplay/Registry/" + nameof(ItemRegistry)
        , fileName = nameof(ItemRegistry))]
    public class ItemRegistry : ScriptableObject
    {
        [Serializable]
        public class ItemEntry
        {
            public ItemType Type;
            public ItemData Data;
        }
        
        [SerializeField] private ItemEntry[] itemEntries;
        private Dictionary<ItemType, ItemData> _itemDataMap;
        
        private void OnEnable()
        {
            BuildItemDataMap();
        }

        private void BuildItemDataMap()
        {
            _itemDataMap = new Dictionary<ItemType, ItemData>();
            if (itemEntries == null)
            {
                return;
            }
            
            foreach (var entry in itemEntries)
            {
                _itemDataMap[entry.Type] = entry.Data;
            }
        }

        public ItemData GetItemData(ItemType itemType)
        {
            if (_itemDataMap == null)
            {
                BuildItemDataMap();
            }
            
            return _itemDataMap.GetValueOrDefault(itemType);
        }
    }
}