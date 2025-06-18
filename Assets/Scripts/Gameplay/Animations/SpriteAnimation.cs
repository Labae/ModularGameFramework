using System.Collections.Generic;
using UnityEngine;

namespace MarioGame.Gameplay.Animations
{
    /// <summary>
    /// 스프라이트 애니메이션 데이터를 담는 ScriptableObject
    /// WeaponConfiguration에서 사용하여 완전한 데이터 드리븐 애니메이션 시스템 구현
    /// </summary>
    [CreateAssetMenu(menuName = "MarioGame/Animation/SpriteAnimation", fileName = "New Sprite Animation")]
    public class SpriteAnimation : ScriptableObject
    {
        [Header("Animation Data")]
        [Tooltip("애니메이션에 사용할 스프라이트 리스트")]
        public List<Sprite> sprites = new();
        
        [Tooltip("초당 프레임 수")]
        [Range(1f, 60f)]
        public float frameRate = 12f;
        
        [Tooltip("애니메이션 반복 여부")]
        public bool loop = false;
        
        [Tooltip("애니메이션 자동 시작 여부")]
        public bool autoStart = true;

        [Header("Audio")] public AudioClip AnimationSFX;
        
        [Header("Visual Effects")]
        [Tooltip("랜덤 스케일 적용 여부")]
        public bool randomScale = false;
        
        [Tooltip("랜덤 스케일 범위")]
        public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        
        [Tooltip("랜덤 회전 적용 여부")]
        public bool randomRotation = false;
        
        [Tooltip("랜덤 회전 범위 (도)")]
        public Vector2 rotationRange = new Vector2(0f, 360f);

        [Header("Fade Out Settings")]
        [Tooltip("애니메이션 종료 시 페이드 아웃 여부")]
        public bool fadeOut = false;
        
        [Tooltip("페이드 아웃 지속 시간")]
        [Range(0.1f, 2f)]
        public float fadeOutDuration = 0.3f;

        [Header("Direction Settings")]
        [Tooltip("방향에 따른 회전 적용 여부")]
        public bool rotateWithDirection = false;
        
        [Tooltip("기본 회전 오프셋 (도)")]
        public float rotationOffset = 0f;

        /// <summary>
        /// 애니메이션 유효성 검사
        /// </summary>
        public bool IsValid => sprites != null && sprites.Count > 0;

        /// <summary>
        /// 총 애니메이션 시간 계산
        /// </summary>
        public float Duration => IsValid ? sprites.Count / frameRate : 0f;

        /// <summary>
        /// SpriteAnimator에 애니메이션 적용
        /// </summary>
        public void ApplyToAnimator(SpriteAnimator animator, Vector2? direction = null)
        {
            if (!IsValid || animator == null)
            {
                Debug.LogWarning($"Cannot apply invalid animation: {name}");
                return;
            }

            // 애니메이션 설정
            animator.SetFrameRate(frameRate);
            animator.SetLoop(loop);
            animator.SetFadeOut(fadeOut, fadeOutDuration);
            animator.SetRandomEffects(randomScale, scaleRange, randomRotation, rotationRange);

            // 방향 설정
            if (direction.HasValue && rotateWithDirection)
            {
                var angle = Mathf.Atan2(direction.Value.y, direction.Value.x) * Mathf.Rad2Deg + rotationOffset;
                animator.SetRotation(angle);
            }
        }

        /// <summary>
        /// 새로운 SpriteAnimation 인스턴스 생성 (런타임 수정용)
        /// </summary>
        public SpriteAnimation CreateRuntimeCopy()
        {
            var copy = CreateInstance<SpriteAnimation>();
            copy.sprites = new List<Sprite>(sprites);
            copy.frameRate = frameRate;
            copy.loop = loop;
            copy.autoStart = autoStart;
            copy.randomScale = randomScale;
            copy.scaleRange = scaleRange;
            copy.randomRotation = randomRotation;
            copy.rotationRange = rotationRange;
            copy.fadeOut = fadeOut;
            copy.fadeOutDuration = fadeOutDuration;
            copy.rotateWithDirection = rotateWithDirection;
            copy.rotationOffset = rotationOffset;
            return copy;
        }

        /// <summary>
        /// 애니메이션 설정을 다른 SpriteAnimation에서 복사
        /// </summary>
        public void CopySettingsFrom(SpriteAnimation other)
        {
            if (other == null) return;

            frameRate = other.frameRate;
            loop = other.loop;
            autoStart = other.autoStart;
            randomScale = other.randomScale;
            scaleRange = other.scaleRange;
            randomRotation = other.randomRotation;
            rotationRange = other.rotationRange;
            fadeOut = other.fadeOut;
            fadeOutDuration = other.fadeOutDuration;
            rotateWithDirection = other.rotateWithDirection;
            rotationOffset = other.rotationOffset;
        }

        /// <summary>
        /// 특정 프레임의 스프라이트 가져오기
        /// </summary>
        public Sprite GetSpriteAtFrame(int frameIndex)
        {
            if (!IsValid || frameIndex < 0 || frameIndex >= sprites.Count)
                return null;
            
            return sprites[frameIndex];
        }

        /// <summary>
        /// 스프라이트 추가
        /// </summary>
        public void AddSprite(Sprite sprite)
        {
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        /// <summary>
        /// 스프라이트 제거
        /// </summary>
        public void RemoveSprite(Sprite sprite)
        {
            sprites.Remove(sprite);
        }

        /// <summary>
        /// 모든 스프라이트 클리어
        /// </summary>
        public void ClearSprites()
        {
            sprites.Clear();
        }

#if UNITY_EDITOR
        [Header("Editor Tools")]
        [Space(10)]
        [Tooltip("에디터에서 애니메이션 미리보기")]
        public bool previewInEditor = false;

        /// <summary>
        /// 에디터에서 애니메이션 정보 표시
        /// </summary>
        private void OnValidate()
        {
            // 프레임 레이트 검증
            frameRate = Mathf.Clamp(frameRate, 1f, 60f);
            
            // 페이드 아웃 시간 검증
            fadeOutDuration = Mathf.Clamp(fadeOutDuration, 0.1f, 2f);
            
            // 스케일 범위 검증
            if (scaleRange.x > scaleRange.y)
            {
                scaleRange = new Vector2(scaleRange.y, scaleRange.x);
            }
            
            // 회전 범위 검증 (0-360도 범위로 제한)
            rotationRange.x = Mathf.Clamp(rotationRange.x, 0f, 360f);
            rotationRange.y = Mathf.Clamp(rotationRange.y, 0f, 360f);
        }

        /// <summary>
        /// 에디터용 디버그 정보 출력
        /// </summary>
        [ContextMenu("Print Animation Info")]
        public void PrintAnimationInfo()
        {
            Debug.Log($"SpriteAnimation '{name}' Info:\n" +
                     $"- Sprites: {sprites.Count}\n" +
                     $"- Frame Rate: {frameRate} fps\n" +
                     $"- Duration: {Duration:F2}s\n" +
                     $"- Loop: {loop}\n" +
                     $"- Fade Out: {fadeOut} ({fadeOutDuration}s)\n" +
                     $"- Random Scale: {randomScale} ({scaleRange})\n" +
                     $"- Random Rotation: {randomRotation} ({rotationRange})");
        }

        /// <summary>
        /// 에디터용 애니메이션 유효성 검사
        /// </summary>
        [ContextMenu("Validate Animation")]
        public void ValidateAnimation()
        {
            var issues = new List<string>();

            if (sprites.Count == 0)
                issues.Add("No sprites assigned");

            if (frameRate <= 0)
                issues.Add("Invalid frame rate");

            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] == null)
                    issues.Add($"Null sprite at index {i}");
            }

            if (issues.Count > 0)
            {
                Debug.LogWarning($"SpriteAnimation '{name}' has issues:\n- " + string.Join("\n- ", issues));
            }
            else
            {
                Debug.Log($"SpriteAnimation '{name}' is valid!");
            }
        }
#endif
    }
}