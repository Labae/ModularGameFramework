using System.Collections;
using MarioGame.Core;
using MarioGame.Gameplay.Combat.Data;
using UnityEngine;

namespace MarioGame.Gameplay.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class EntityHitEffect : CoreBehaviour
    {
        [Header("Flash Effect Settings")]
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private int _flashCount = 3;
        
        [SerializeField] private Color _criticalFlashColor = Color.red;
        [SerializeField] private float _criticalFlashDuration = 0.15f;
        [SerializeField] private int _criticalFlashCount = 5;
        
        private SpriteRenderer _renderer;
        private EntityHealth _health;
        
        private Color _originalColor;
        private Coroutine _currentEffectCoroutine;
        
        private WaitForSeconds _flashWait;
        private WaitForSeconds _criticalFlashWait;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _renderer = GetComponent<SpriteRenderer>();
            AssertIsNotNull(_renderer, "SpriteRenderer required");

            _health = GetComponentInParent<EntityHealth>();
            AssertIsNotNull(_health, "EntityHealth required");

            _flashWait = new WaitForSeconds(_flashDuration * 0.5f);
            _criticalFlashWait = new WaitForSeconds(_criticalFlashDuration * 0.5f);
            _originalColor = _renderer.color;
        }

        private void Start()
        {
            _health.OnDamageTaken += OnDamageTaken;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDamageTaken -= OnDamageTaken;
            }
        }
        
        private void OnDamageTaken(DamageEventData eventData)
        {
            if (eventData.DamageInfo.Damage <= 0 || eventData.RemainingHealth <= 0)
            {
                return;
            }

            PlayHitEffect(eventData.DamageInfo.WasCritical);
        }

        private void PlayHitEffect(bool isCritical)
        {
            if (_currentEffectCoroutine != null)
            {
                StopCoroutine(_currentEffectCoroutine);
                ResetEffects();
            }
            
            _currentEffectCoroutine = StartCoroutine(FlashEffect(isCritical));
        }

        private IEnumerator FlashEffect(bool isCritical)
        {
            int count = isCritical ? _criticalFlashCount : _flashCount;
            Color color = isCritical ? _criticalFlashColor : _flashColor;
            
            for (int i = 0; i < count; i++)
            {
                _renderer.color = color;
                yield return isCritical ? _criticalFlashWait : _flashWait;
                
                _renderer.color = _originalColor;
                yield return isCritical ? _criticalFlashWait : _flashWait;
            }
            
            ResetEffects();
            _currentEffectCoroutine = null;
        }
        
        private void ResetEffects()
        {
            _renderer.color = _originalColor;
        }
    }
}