using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarioGame.Gameplay.Combat.Data
{
    [Serializable]
    public struct DamageInfo
    {
        public int Damage;
        public Vector2 HitPoint;
        [FormerlySerializedAs("HitNormal")] public Vector2 DamageDirection;
        public bool WasCritical;

        public DamageInfo(int damage, Vector2 hitPoint, Vector2 damageDirection, bool isCritical = false)
        {
            Damage = damage;
            HitPoint = hitPoint;
            DamageDirection = damageDirection;
            WasCritical = isCritical;
        }
    }
}