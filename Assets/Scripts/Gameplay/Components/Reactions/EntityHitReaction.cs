using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MarioGame.Debugging.Interfaces;
using MarioGame.Gameplay.Components.Interfaces;
using MarioGame.Gameplay.Config.Reactions;
using UnityEngine;

namespace MarioGame.Gameplay.Components.Reactions
{
    public class EntityHitReaction : IEntityHitReaction
    {
        private readonly IDebugLogger _logger;
        private EntityHitReactionConfig _config;
        private Color _originalColor;
        private Color _currentColor;
        private bool _isEffectActive;
        private CancellationTokenSource _effectCancellationSource;

        public bool IsEffectActive => _isEffectActive;
        public Color CurrentColor => _currentColor;

        public EntityHitReaction(IDebugLogger logger)
        {
            _logger = logger;
        }

        public void Initialize(EntityHitReactionConfig config, Color originalColor)
        {
            _config = config;
            _config?.Validate();
            _originalColor = originalColor;
            _currentColor = originalColor;
            _logger?.Entity("HitEffect initialized");
        }

        public async UniTask PlayHitEffectAsync(bool isCritical, CancellationToken cancellationToken = default)
        {
            if (_config == null) return;

            // 기존 이펙트 중단
            StopEffect();

            // 새 이펙트 시작
            _isEffectActive = true;
            _effectCancellationSource = new CancellationTokenSource();

            // 외부 취소 토큰과 내부 취소 토큰 결합
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _effectCancellationSource.Token).Token;

            try
            {
                var flashColor = isCritical ? _config.CriticalFlashColor : _config.FlashColor;
                var flashDuration = isCritical ? _config.CriticalFlashDuration : _config.FlashDuration;
                var flashCount = isCritical ? _config.CriticalFlashCount : _config.FlashCount;
                var flashInterval = (int)(flashDuration * 0.5f * 1000); // milliseconds

                _logger?.Entity($"Hit effect started - Critical: {isCritical}, Flashes: {flashCount}");

                for (int i = 0; i < flashCount; i++)
                {
                    // 플래시 색상으로 변경
                    _currentColor = flashColor;
                    await UniTask.Delay(flashInterval, cancellationToken: combinedToken);

                    // 원본 색상으로 복원
                    _currentColor = _originalColor;
                    await UniTask.Delay(flashInterval, cancellationToken: combinedToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.Entity("Hit effect cancelled");
            }
            finally
            {
                // 이펙트 종료 정리
                _currentColor = _originalColor;
                _isEffectActive = false;
                _effectCancellationSource?.Dispose();
                _effectCancellationSource = null;
            }
        }

        public void StopEffect()
        {
            if (_isEffectActive)
            {
                _effectCancellationSource?.Cancel();
                _logger?.Entity("Hit effect stopped");
            }
        }
    }
}