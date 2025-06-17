using System;
using UnityEngine;

namespace MarioGame.Gameplay.Combat.Data
{
    [Serializable]
    public struct KnockbackData
    {
        public Vector2 Direction;
        public float Force;
        public float Duration;
        public AnimationCurve DistanceFalloff;
    }
}