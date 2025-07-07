using MarioGame.Level.Interfaces;
using MarioGame.Level.LevelObjects.Ladders;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Detection
{
    [CreateAssetMenu(menuName = "MarioGame/Entity/" + nameof(GroundDetectionConfig),
        fileName = "New " + nameof(GroundDetectionConfig))]
    public class GroundDetectionConfig : ScriptableObject
    {
        [SerializeField, Min(0.0f)] public float GroundCheckDistance = 0.1f;
        [SerializeField, Min(3)] public int GroundCheckRayCount = 5;
        [SerializeField] public float GroundCheckYOffset = 0.0f;
        [SerializeField, Min(0.1f)] public float GroundCheckWidth = 0.8f;
        [SerializeField] public LayerMask GroundLayerMask;
        [SerializeField] public bool EnableBypass = true;

        public void Validate()
        {
            GroundCheckDistance = Mathf.Max(0.01f, GroundCheckDistance);
            GroundCheckRayCount = Mathf.Max(3, GroundCheckRayCount);
            GroundCheckWidth = Mathf.Clamp(GroundCheckWidth, 0.1f, 2f);
        }
    }
    
    public class GroundDetectionResult
    {
        public bool IsGrounded { get; set; }
        public LadderTopPlatformBypass CurrentBypassable { get; set; }
        public RaycastHit2D[] Hits { get; set; }
        public Vector2[] RayPositions { get; set; }

        public bool CanBypass => CurrentBypassable != null;
        public bool HasBypassableBelow => CurrentBypassable != null;
        public IBypassable Bypassable => CurrentBypassable;
    }
}