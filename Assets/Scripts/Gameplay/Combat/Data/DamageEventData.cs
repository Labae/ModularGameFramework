using System;

namespace MarioGame.Gameplay.Combat.Data
{
    [Serializable]
    public struct DamageEventData
    {
        public DamageInfo DamageInfo;
        public int RemainingHealth;
    }
}