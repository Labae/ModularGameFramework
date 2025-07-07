using UnityEngine;

namespace MarioGame.Gameplay.Config.Reactions
{
    [CreateAssetMenu(menuName = "MarioGame/Entity/" + nameof(EntityHitReactionConfig),
        fileName = "New " + nameof(EntityHitReactionConfig))]
    public class EntityHitReactionConfig : ScriptableObject
    {
        [Header("Normal Flash Effect")]
        public Color FlashColor = Color.white;
        public float FlashDuration = 0.1f;
        public int FlashCount = 3;
        
        [Header("Critical Flash Effect")]
        public Color CriticalFlashColor = Color.red;
        public float CriticalFlashDuration = 0.15f;
        public int CriticalFlashCount = 5;

        public void Validate()
        {
            FlashDuration = Mathf.Max(0.01f, FlashDuration);
            FlashCount = Mathf.Max(1, FlashCount);
            CriticalFlashDuration = Mathf.Max(0.01f, CriticalFlashDuration);
            CriticalFlashCount = Mathf.Max(1, CriticalFlashCount);
        }
    }
}