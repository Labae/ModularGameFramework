using UnityEngine;

namespace MarioGame.Gameplay.Config.Movement
{
    [CreateAssetMenu(menuName = "MarioGame/Gameplay/Movement/" + nameof(PlayerMovementConfig)
        , fileName = nameof(PlayerMovementConfig))]
    public class PlayerMovementConfig : BaseMovementConfig
    {
        [Header("Advanced Jump")]
        [Tooltip("코요테 타임 (절변에서 점프 가능한 시간)")]
        [Range(0.05f, 0.3f)]
        public float CoyoteTime = 0.1f;
        
        [Tooltip("점프 버퍼 시간 (착지 전 점프 입력 유지)")]
        [Range(0.05f, 0.3f)]
        public float JumpBufferTime = 0.1f;
        
        [Tooltip("가변 점프 배율 (점프 키 놓았을 떄)")]
        [Range(0.1f, 0.8f)]
        public float VariableJumpMultiplier = 0.5f;

        [Header("Physics")]
        [Tooltip("떨어지는 속도 배율")]
        [Range(1.0f, 5.0f)]
        public float FallMultiplier = 2.5f;
        
        [Tooltip("짧은 점프 시 중력 배율")]
        [Range(1.0f, 5.0f)]
        public float LowJumpMultiplier = 2f;
    }
}