using MarioGame.Core;
using UnityEngine;

namespace MarioGame.Audio
{
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public sealed class PooledAudioSource : CoreBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        
        public bool IsPlaying => _audioSource.isPlaying;
        public bool IsAvailable=> !_audioSource.isPlaying;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _audioSource ??= GetComponent<AudioSource>();
            _assertManager.AssertIsNotNull(_audioSource, "AudioSource component required");
        }

        public void PlayAudio(AudioRequest request)
        {
            _audioSource.clip = request.Clip;
            _audioSource.volume = request.Volume;
            _audioSource.pitch = request.Pitch;
            _audioSource.loop = request.Loop;
            _audioSource.outputAudioMixerGroup = request.MixerGroup;

            if (request.is3D)
            {
                _audioSource.spatialBlend = 1f;
                transform.position = request.Position;
            }
            else
            {
                _audioSource.spatialBlend = 0f;
            }
            
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
        }
    }
}