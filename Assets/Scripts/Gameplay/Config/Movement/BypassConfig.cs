using UnityEngine;

namespace MarioGame.Gameplay.Config.Movement
{
    [CreateAssetMenu(menuName = "MarioGame/Gameplay/Movement/" + nameof(EntityBypassConfig)
        , fileName = nameof(EntityBypassConfig))]
    public class EntityBypassConfig : ScriptableObject
    {
        [SerializeField] public bool EnableBypass = true;
        [SerializeField, Range(0.1f, 1f)] public float InputThreshold = 0.5f;
        [SerializeField] public float BypassCooldown = 0.2f;
    }
}