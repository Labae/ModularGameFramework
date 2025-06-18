using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MarioGame.Audio
{
    [Serializable]
    public struct AudioRequest
    {
        public AudioClip Clip;
        public Vector2 Position;
        public bool is3D;
        public float Volume;
        public float Pitch;
        public bool Loop;

        public AudioMixerGroup MixerGroup;

        public static AudioRequest Create2D(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            return new AudioRequest
            {
                Clip = clip,
                Volume = volume,
                Pitch = pitch,
                Loop = false,
                is3D = false,
                Position = Vector3.zero,
            };
        }

        public static AudioRequest Create3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            return new AudioRequest
            {
                Clip = clip,
                Volume = volume,
                Pitch = pitch,
                Loop = false,
                is3D = true,
                Position = position,
            };
        }
    }
}