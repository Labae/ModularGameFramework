using System;
using MarioGame.Gameplay.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarioGame.Gameplay.Config.Data
{
    [Serializable]
    public class ShakeData
    {
        [Header("Basic Settings")] public float Intensity = 1f;
        public float Duration = 0.5f;
        public ShakeType ShakeType = ShakeType.Random;

        [Header("Frequency")] public float Frequency = 5f;

        [Header("Damping")] public AnimationCurve Damping = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Direction (for Punch)")] public Vector2 PunchDirection = Vector2.right;

        [Header("Perlin Settings")] public float PerlinStrength = 1f;
        public Vector2 PerlinOffset = Vector2.zero;

        public static ShakeData Quick(float intensity, float duration)
            => new ShakeData { Intensity = intensity, Duration = duration, ShakeType = ShakeType.Random };


        public static ShakeData Explosion(float intensity, float duration)
            => new ShakeData { Intensity = intensity, Duration = duration, ShakeType = ShakeType.Explosion };


        public static ShakeData Punch(Vector2 direction, float intensity, float duration)
            => new ShakeData
                { Intensity = intensity, Duration = duration, ShakeType = ShakeType.Punch, PunchDirection = direction };
    }

    internal class ShakeInstance
    {
        public ShakeData Data;
        public float StartTime;
        public float CurrentTime;

        public bool IsActive;
        public int Priority;
        
        public float Progress => CurrentTime / Data.Duration;
        public bool IsComplete => CurrentTime >= Data.Duration;

        public Vector3 GetCurrentOffset()
        {
            if (!IsActive || IsComplete)
            {
                return Vector3.zero;
            }
            
            var dampingFactor = Data.Damping.Evaluate(Progress);
            var currentIntensity = Data.Intensity * dampingFactor;

            return Data.ShakeType switch
            {
                ShakeType.Random => GetRandomOffset(currentIntensity),
                ShakeType.Horizontal => GetHorizontalOffset(currentIntensity),
                ShakeType.Vertical => GetVerticalOffset(currentIntensity),
                ShakeType.Perlin => GetPerlinOffset(currentIntensity),
                ShakeType.Punch => GetPunchOffset(currentIntensity, dampingFactor),
                ShakeType.Explosion => GetExplosionOffset(currentIntensity),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Vector3 GetRandomOffset(float currentIntensity)
        {
            var x = (Mathf.Sin(CurrentTime * Data.Frequency) + Random.Range(-0.5f, 0.5f)) * currentIntensity;
            var y = (Mathf.Cos(CurrentTime * Data.Frequency) + Random.Range(-0.5f, 0.5f)) * currentIntensity;
            return new Vector3(x, y, 0f);
        }

        private Vector3 GetHorizontalOffset(float currentIntensity)
        {
            var x = (Mathf.Sin(CurrentTime * Data.Frequency) + Random.Range(-0.5f, 0.5f)) * currentIntensity;
            return new Vector3(x, 0f, 0f);
        }
        
        private Vector3 GetVerticalOffset(float currentIntensity)
        {
            var y = (Mathf.Cos(CurrentTime * Data.Frequency) + Random.Range(-0.5f, 0.5f)) * currentIntensity;
            return new Vector3(0f, y, 0f);
        }

        private Vector3 GetPerlinOffset(float currentIntensity)
        {
            var x = (Mathf.PerlinNoise(CurrentTime * Data.PerlinStrength + Data.PerlinOffset.x, 0f) - 0.5f) * 2f * currentIntensity;
            var y = (Mathf.PerlinNoise(CurrentTime * Data.PerlinStrength + Data.PerlinOffset.y, 100f) - 0.5f) * 2f * currentIntensity;
            return new Vector3(x, y, 0f);
        }

        private Vector3 GetPunchOffset(float currentIntensity, float  dampingFactor)
        {
            var punchIntensity = currentIntensity * (1f - Progress);
            return new Vector3(
                Data.PunchDirection.x * punchIntensity * dampingFactor, 
                Data.PunchDirection.y * punchIntensity * dampingFactor
                , 0f);
        }

        private Vector3 GetExplosionOffset(float currentIntensity)
        {
            var explosionFactor = Mathf.Pow((1f - Progress), 2f);
            var x = Random.Range(-1f, 1f) * currentIntensity * explosionFactor;
            var y = Random.Range(-1f, 1f) * currentIntensity * explosionFactor;
            return new Vector3(x, y, 0f);
        }
    }
}