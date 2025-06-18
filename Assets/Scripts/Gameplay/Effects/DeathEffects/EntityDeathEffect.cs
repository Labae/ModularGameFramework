using System.Collections;
using UnityEngine;

namespace MarioGame.Gameplay.Effects.DeathEffects
{
    public class EntityDeathEffect : BaseEffect<EntityDeathEffectData>
    {
        [Header("Death Effect Settings")] [SerializeField]
        private bool _useScaleAnimation = true;
        [SerializeField] private AnimationCurve _scaleCurve 
            = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.5f);
        [SerializeField] private float _scaleAnimationDuration = 0.2f;
        
        protected override void OnInitialize(EntityDeathEffectData spawnData)
        {
            _effectData = spawnData;

            SetupEffectScale();
            SetupExplosionAnimation();
        }

        private void SetupEffectScale()
        {
            var effectScale = Mathf.Clamp(_effectData.EntitySize * 0.8f, 0.5f, 3f);
            transform.localScale = Vector3.one * effectScale;
        }
        
        private void SetupExplosionAnimation()
        {
            if (_effectData.EffectAnimation != null && _effectData.EffectAnimation.IsValid)
            {
                _animator.SetAnimation(_effectData.EffectAnimation);
            }
        }

        protected override void OnEffectStarted()
        {
            base.OnEffectStarted();

            if (_useScaleAnimation)
            {
                StartCoroutine(PlayScaleAnimation());
            }
        }

        private IEnumerator PlayScaleAnimation()
        {
            var originScale = transform.localScale;
            var elapsed = 0f;

            while (elapsed < _scaleAnimationDuration)
            {
                elapsed += Time.deltaTime;
                var scaleMultiplier = _scaleCurve.Evaluate(elapsed / _scaleAnimationDuration);
                transform.localScale = originScale * scaleMultiplier;
                yield return null;
            }
            
            transform.localScale = originScale;
        }

        public override void OnReturnToPool()
        {
            StopAllCoroutines();
            transform.localScale = Vector3.one;
            base.OnReturnToPool();
        }
    }
}