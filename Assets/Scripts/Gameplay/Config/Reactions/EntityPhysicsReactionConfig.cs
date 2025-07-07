using MarioGame.Gameplay.Combat.Data;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Reactions
{
    [CreateAssetMenu(menuName = "MarioGame/Entity/" + nameof(EntityPhysicsReactionConfig),
        fileName = "New " + nameof(EntityPhysicsReactionConfig))]
    public class EntityPhysicsReactionConfig : ScriptableObject
    {
        [Header("Critical Knockback")]
        public KnockbackData CriticalKnockbackData = new KnockbackData();
        
        [Header("Reaction Settings")]
        public bool EnableKnockback = true;
        public bool EnableStun = true;
        public float MinKnockbackForce = 0.1f;
    }
}