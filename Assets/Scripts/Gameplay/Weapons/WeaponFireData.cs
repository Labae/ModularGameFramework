using System;
using MarioGame.Core.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Weapons
{
    /// <summary>
    /// 발사 데이터를 담는 구조체
    /// </summary>
    [Serializable]
    public struct WeaponFireData
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Timestamp;
        public float Speed;

        public WeaponFireData(Vector2 position, Vector2 direction, float timestamp,
            float speed)
        {
            Position = position;
            Direction = direction;
            Timestamp = timestamp;
            Speed = speed;
        }
    }
}