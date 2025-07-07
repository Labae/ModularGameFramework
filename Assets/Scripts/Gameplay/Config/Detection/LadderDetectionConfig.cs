using MarioGame.Level.LevelObjects.Ladders;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Detection
{
    [CreateAssetMenu(menuName = "MarioGame/Entity/" + nameof(LadderDetectionConfig),
        fileName = "New " + nameof(LadderDetectionConfig))]
    public class LadderDetectionConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] public float LadderCheckDistance = 0.2f;
        [SerializeField, Min(3)] public int LadderCheckRayCount = 3;
        [SerializeField] public LayerMask LadderCheckLayerMask;

        public void Validate()
        {
            LadderCheckDistance = Mathf.Max(0.1f, LadderCheckDistance);
            LadderCheckRayCount = Mathf.Max(3, LadderCheckRayCount);
        }
    }
    
    public class LadderDetectionResult
    {
        public bool IsOnLadder { get; set; }
        public bool IsAtLadderTop { get; set; }
        public bool IsAtLadderBottom { get; set; }
        public Ladder CurrentLadder { get; set; }
        public RaycastHit2D[] Hits { get; set; }
        public Vector2[] RayPositions { get; set; }

        public bool CanClimb => IsOnLadder && CurrentLadder != null;
        
        public bool TryGetLadderAlignPosition(out float alignX)
        {
            if (CanClimb)
            {
                alignX = CurrentLadder.CenterX;
                return true;
            }
            alignX = 0f;
            return false;
        }

        public bool TryGetLadderYLimits(out float minY, out float maxY)
        {
            if (CanClimb)
            {
                minY = CurrentLadder.GetMinClimbY();
                maxY = CurrentLadder.GetMaxClimbY();
                return true;
            }
            minY = 0f;
            maxY = 0f;
            return false;
        }
    }
}