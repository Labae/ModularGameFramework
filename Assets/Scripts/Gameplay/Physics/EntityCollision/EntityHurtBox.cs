using MarioGame.Core;
using MarioGame.Gameplay.Combat.Data;
using MarioGame.Gameplay.Interfaces.Combat;
using UnityEngine;

namespace MarioGame.Gameplay.Physics.EntityCollision
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class EntityHurtBox : CoreBehaviour
    {
        [Header("Hurtbox Settings")]
        [SerializeField] private bool _isActive = true;
        [SerializeField] private float _damageMultiplier = 1.0f;
        [SerializeField] private string _hurtboxName = "Body";
        
        [Header("Special Properties")]
        [SerializeField] private bool _isCriticalHitZone = false;
        [SerializeField] private float _criticalMultiplier = 2.0f;
        
        private IDamageable _damageable;
        
        // 프로퍼티들
        public bool IsActive => _isActive;
        public string HurtboxName => _hurtboxName;
        public bool CanTakeDamage => _isActive && _damageable?.CanTakeDamage == true;
        
        protected override void CacheComponents()
        {
            base.CacheComponents();
            _damageable = GetComponentInParent<IDamageable>();
            AssertIsNotNull(_damageable, "IDamageable required in parent");
        }
        
        /// <summary>
        /// 외부에서 데미지 적용 (IDamageable 숨김)
        /// </summary>
        public bool TryApplyDamage(int baseDamage, Vector2 hitPoint, Vector2 damageDirection)
        {
            if (!CanTakeDamage) return false;
            
            // HurtBox별 데미지 계산
            int finalDamage = CalculateFinalDamage(baseDamage);
            
            var damageData = new DamageInfo
            {
                Damage = finalDamage,
                HitPoint = hitPoint,
                DamageDirection = damageDirection,
                WasCritical = _isCriticalHitZone
            };
            
            // IDamageable에 전달
            _damageable.TakeDamage(damageData);
            
            return true;
        }
        
        /// <summary>
        /// HurtBox별 데미지 계산
        /// </summary>
        private int CalculateFinalDamage(int baseDamage)
        {
            float multiplier = _damageMultiplier;
            
            // 크리티컬 히트 체크
            if (_isCriticalHitZone)
            {
                multiplier *= _criticalMultiplier;
                Log($"Critical hit on {_hurtboxName}! Damage: {baseDamage} -> {Mathf.RoundToInt(baseDamage * multiplier)}");
            }
            
            return Mathf.RoundToInt(baseDamage * multiplier);
        }
        
        /// <summary>
        /// HurtBox 활성화/비활성화 (애니메이션 이벤트 등에서 사용)
        /// </summary>
        public void SetActive(bool active)
        {
            _isActive = active;
        }
        
        /// <summary>
        /// 데미지 배율 동적 변경 (버프/디버프 등)
        /// </summary>
        public void SetDamageMultiplier(float multiplier)
        {
            _damageMultiplier = multiplier;
        }
    }
}