using UnityEngine;

namespace MarioGame.Gameplay.Config.Movement
{
    [CreateAssetMenu(menuName = "MarioGame/Gameplay/Movement/" + nameof(ClimbMovementConfig)
        , fileName = nameof(ClimbMovementConfig))]
    public class ClimbMovementConfig : ScriptableObject
    {
        [Header("Climb Speed Settings")] [Tooltip("기본 사다리 올라가기/내려가기 속도")] [Range(1.0f, 10.0f)]
        public float ClimbSpeed = 4.0f;

        [Header("Jump Settings")] [Tooltip("사다리에서 점프할 때의 힘")] [Range(5.0f, 20.0f)]
        public float JumpFromLadderForce = 12.0f;

        [Tooltip("사다리 점프 힘 배율")] 
        [Range(1.0f, 2.0f)]
        public float JumpMultiplier = 1.3f;

        [Header("Phyiscs Settings")] [Tooltip("사다리 타는 동안 드래그")] [Range(0.0f, 20.0f)]
        public float ClimbDrag = 8.0f;
                
        [Header("Input Settings")]
        [Tooltip("입력 데드존")]
        [Range(0.01f, 0.1f)]
        public float ClimbInputDeadzone = 0.01f;
    }
}