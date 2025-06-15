using MarioGame.Core.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarioGame.Gameplay.Config.Movement
{
    [CreateAssetMenu(menuName = "MarioGame/Gameplay/Movement/" + nameof(EnemyMovementConfig)
        , fileName = nameof(EnemyMovementConfig))]
    public class EnemyMovementConfig : BaseMovementConfig
    {
        [Header("Idle Settings")] [Tooltip("기본 대기 시간")]
        public float DefaultIdleTime = 2.0f;

        [Tooltip("랜덤 대기 시간 범위(+-)")]
        public float IdleTimeVariation = 0.5f;
        
        [Header("Patrol Settings")] 
        
        public PatrolData.PatrolType PatrolType;
        
        [Tooltip("Patrol 속도 배율")]
        [Range(0.5f, 1.5f)]
        public float PatrolMultiplier = 1.0f;
        
        [Tooltip("패트롤 시 회전 속도(방향 전환)")]
        [Range(1.0f, 10.0f)]
        public float TurnSpeed = 5.0f;

        [Tooltip("목표점에 도달했다고 판단하는 거리")] [Range(0.1f, 1.0f)]
        public float ArrivalThreshold = 0.3f;

        public float GetRandomIdleTime()
        {
            return Random.Range(DefaultIdleTime - IdleTimeVariation, DefaultIdleTime + IdleTimeVariation);
        }
    }
}