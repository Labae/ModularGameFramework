using UnityEngine;

namespace Animations
{
    [CreateAssetMenu(fileName = "New Sprite Animation", menuName = "Animations/Sprite Animation")]
    public class SpriteAnimation : ScriptableObject
    {
        [Header("Animation Data")]
        public Sprite[] SpriteFrames;

        [Range(1f, 60f)]
        public float FrameRate = 12;

        public bool Loop = false;

        public bool AutoStart = true;
        public int TotalFrames => SpriteFrames?.Length ?? 0;

        public bool IsValid()
        {
            return SpriteFrames != null && SpriteFrames.Length > 0;
        }
    }
}