using MarioGame.Gameplay.MovementIntents;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Movement
{
    
    public abstract class BaseMovementConfig : ScriptableObject
    {
        [Header("Base Movement")] 
        [Tooltip("기본 이동속도")]
        public float BaseSpeed = 8.0f;

        [Tooltip("가속도 (높을소록 빠르게 가속)")]
        public float Acceleration = 30.0f;
        [Tooltip("감속도 (높을소록 빠르게 멈춤)")]
        public float Deceleration = 40.0f;
        
        [Header("Speed Multiplier")]
        
        [Tooltip("달리기 속도 배율")]
        [Range(1.0f, 3.0f)]
        public float RunMultiplier = 1.0f;
        
        [Header("Air Control")]
        [Tooltip("공중에서 조작감 (1.0 = 지상과 동일, 0.5 = 절반)")]
        public float AirControlMultiplier = 0.6f;
        
        [Header("Jump Settings")]
        [Tooltip("점프 높이")]
        public float JumpForce = 15.0f;
        
        [Tooltip("전체 중력 스케일")]
        [Range(1.0f, 5.0f)]
        public float GravityScale = 3.0f;
        
        [Header("Input Settings")]
        [Tooltip("입력 데드존")]
        [Range(0.01f, 0.1f)]
        public float InputDeadzone = 0.01f;

        public virtual MovementIntent CreateIdle()
        {
            return MovementIntentFactory.CreateIdle();
        }

        public virtual MovementIntent CreateGroundMovement(float input)
        {
            return MovementIntentFactory.CreateGroundMovement(this, input, RunMultiplier);
        }
        
        public virtual MovementIntent CreateAirControl(float input)
        {
            return MovementIntentFactory.CreateAirControl(this, input, RunMultiplier);
        }
    }
}