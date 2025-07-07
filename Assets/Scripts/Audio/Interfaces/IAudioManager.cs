using UnityEngine;

namespace MarioGame.Audio.Interfaces
{
    public interface IAudioManager
    {
        void PlaySFX(AudioClip clip, float volume = 1.0f, float pitch = 1.0f);
        void PlaySFX3D(AudioClip clip, Vector3 position,  float volume = 1.0f, float pitch = 1.0f);
        void PlayMusic(AudioClip clip, float volume = 1.0f, float pitch = 1.0f);
        void StopMusic();
    }
}