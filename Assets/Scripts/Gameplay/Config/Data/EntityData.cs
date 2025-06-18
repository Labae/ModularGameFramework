using MarioGame.Gameplay.Animations;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Data
{
    [CreateAssetMenu(menuName = "MarioGame/Entity/" + nameof(EntityData), 
        fileName = "New " + nameof(EntityData))]
    public class EntityData : ScriptableObject
    {
        [Header("Basic Info")] public string EntityName;
        [TextArea]
        public string Description;

        [Min(1)]
        public int MaxHealth = 100;
        public bool HasHealth = true;

        [Min(0f)]
        public float InvincibilityDuration = 0.2f;
        public bool CanTakeDamage = true;
        
        public SpriteAnimation DeathEffectAnimation;

        [Header("Audio")] 
        public AudioClip HitSound;
        public AudioClip CriticalHitSound;
        public AudioClip DeathSound;
    }
}