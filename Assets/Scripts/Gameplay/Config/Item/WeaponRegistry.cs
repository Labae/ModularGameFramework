using System;
using System.Collections.Generic;
using MarioGame.Level.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Item
{
    [CreateAssetMenu(menuName = "MarioGame/Gameplay/Registry/" + nameof(WeaponRegistry)
        , fileName = nameof(WeaponRegistry))]
    public class WeaponRegistry : ScriptableObject
    {
        [Serializable]
        public class WeaponEntry
        {
            public WeaponType Type;
            public WeaponData Data;
        }
        
             
        [SerializeField] private WeaponEntry[] _weaponEntries;
        private Dictionary<WeaponType, WeaponData> _weaponDataMap;
        
        private void OnEnable()
        {
            BuildItemDataMap();
        }

        private void BuildItemDataMap()
        {
            _weaponDataMap = new Dictionary<WeaponType, WeaponData>();
            if (_weaponEntries == null)
            {
                return;
            }
            
            foreach (var entry in _weaponEntries)
            {
                _weaponDataMap[entry.Type] = entry.Data;
            }
        }

        public WeaponData GetWeaponData(WeaponType itemType)
        {
            if (_weaponDataMap == null)
            {
                BuildItemDataMap();
            }
            
            return _weaponDataMap.GetValueOrDefault(itemType);
        }
    }
}