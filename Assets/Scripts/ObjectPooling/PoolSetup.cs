using System;
using UnityEngine;

namespace MarioGame.Core.ObjectPooling
{
    [Serializable]
    public class PoolSetup
    {
        [Header("Pool Configuration")]
        public PoolableObject Prefab;
        
        [Header("Pool Settings")]
        [Range(1, 50)]
        public int InitialPoolSize;
        
        [Range(10, 500)]
        public int MaxPoolSize;
        
        public bool AutoExpand = true;

        public PoolSetup(PoolableObject prefab, 
            int initialPoolSize,
            int maxPoolSize, bool autoExpand)
        {
            Prefab = prefab;
            InitialPoolSize = initialPoolSize;
            MaxPoolSize = maxPoolSize;
            AutoExpand = autoExpand;
        }
        
        public bool IsValid()
        {
            if (Prefab == null)
            {
                return false;
            }

            return true;
        }
    }
}