using LDtkUnity;
using MarioGame.Core.Entities;
using UnityEngine;

namespace MarioGame.Gameplay.Pickups
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public abstract class Pickup : Entity
    {
        protected SpriteRenderer _spriteRenderer;
        protected LDtkFields _ldtkFields;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _ldtkFields = GetComponent<LDtkFields>();
            _assertManager.AssertIsNotNull(_spriteRenderer, "SpriteRenderer required");
            _assertManager.AssertIsNotNull(_ldtkFields, "LDtkFields required");
        }
    }
}