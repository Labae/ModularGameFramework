using MarioGame.Core.Entities;
using MarioGame.Gameplay.Animations;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Effects
{
    /// <summary>
    /// 이펙트 생성을 위한 설정 데이터 베이스
    /// </summary>
    public abstract class EffectSpawnData
    {
        public SpriteAnimation EffectAnimation { get; set; }
        public Vector2 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector2 Scale { get; set; }

        public float Duration { get; set; } = -1f;
    }

    public class ProjectileHitEffectData : EffectSpawnData
    {
        public WeaponConfiguration WeaponConfig { get; set; }
        public Collider2D HitCollider { get; set; }
        public HitTargetType HitType { get; set; }
        public Vector2 HitNormal { get; set; }
        public bool IsPenetrateEffect { get; set; }
    }

    public class EntityDeathEffectData : EffectSpawnData
    {
        public Entity DeadEntity { get; set; }
        public float EntitySize { get; set; }
    }
}