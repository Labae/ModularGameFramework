using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.MovementIntents
{
    /// <summary>
    /// 움직임 의도를 나타내는 데이터 구조
    /// State에서 Movement로 전달되는 순수 데이터
    /// </summary>
    [System.Serializable]
    public struct MovementIntent
    {
        [Header("Input")]
        public float HorizontalInput;
        
        [Header("Modifiers")]
        public float SpeedMultiplier;
        public MovementType Type;
        public bool MaintainMomentum;
        public bool AllowDirectionChange;

        [Header("Air Controls")]
        public float AirControlAmount;

        public static MovementIntent None => new MovementIntent
        {
            HorizontalInput = 0f,
            SpeedMultiplier = 1f,
            Type = MovementType.Idle,
            MaintainMomentum = false,
            AllowDirectionChange = true,
            AirControlAmount = 0f,
        };
    }
}