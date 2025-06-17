using UnityEngine;

namespace MarioGame.Core.Utilities
{
    public static class LayerUtility
    {
        public static readonly LayerMask GroundLayer = LayerMask.NameToLayer("Ground");
        public static readonly LayerMask EnemyLayer = LayerMask.NameToLayer("Enemy");
        public static readonly LayerMask PlayerLayer = LayerMask.NameToLayer("Player");
    }
}