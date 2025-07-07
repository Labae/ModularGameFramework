using System.Collections;
using System.Collections.Generic;
using MarioGame.Audio.Interfaces;
using MarioGame.Core;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Audio;

namespace MarioGame.Audio
{
    public class AudioManager : CoreBehaviour, IAudioManager
    {
        [Header("Settings")] 
        [SerializeField] private int _maxPoolSize = 10;
        [SerializeField] private PooledAudioSource _pooledAudioPrefab;
        [SerializeField]
        private AudioMixer _audioMixer;
        
        [Header("Mixer Groups")] 
        [SerializeField] private AudioMixerGroup _musicMixerGroup;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;

        [Inject] private Debugging.Interfaces.IDebugLogger _debugLogger;
        
        private Queue<PooledAudioSource> _availableAudioSources = new();
        private List<PooledAudioSource> _allAudioSources = new();
        private PooledAudioSource _currentMusicSource;
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            InitializeAudioPool();
        }

        private void InitializeAudioPool()
        {
            for (int i = 0; i < _maxPoolSize; i++)
            {
                CreatePooledAudioSource();
            }
        }

        private PooledAudioSource CreatePooledAudioSource()
        {
            var audioObj = Instantiate(_pooledAudioPrefab.gameObject, transform);
            if (audioObj.TryGetComponent<PooledAudioSource>(out var audioSource))
            {
                _allAudioSources.Add(audioSource);
                _availableAudioSources.Enqueue(audioSource);
            }

            return audioSource;
        }

        private PooledAudioSource GetPooledAudioSource()
        {
            if (_availableAudioSources.Count > 0)
            {
                return _availableAudioSources.Dequeue();
            }

            foreach (var source in _allAudioSources)
            {
                if (source.IsAvailable)
                {
                    return source;
                }
            }
            
            _debugLogger.Warning("All audio sources are available. Creating new audio source.");
            return CreatePooledAudioSource();
        }

        private void ReturnToPool(PooledAudioSource audioSource)
        {
            if (!_allAudioSources.Contains(audioSource))
            {
                _availableAudioSources.Enqueue(audioSource);
            }
        }

        public void PlaySFX(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
            {
                return;
            }
            
            var request = AudioRequest.Create2D(clip, volume, pitch);
            request.MixerGroup = _sfxMixerGroup;
            
            var audioSource = GetPooledAudioSource();
            audioSource.PlayAudio(request);

            StartCoroutine(ReturnToPoolWhenFinished(audioSource));
        }
        
        public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
            {
                return;
            }
            
            var request = AudioRequest.Create3D(clip, position, volume, pitch);
            request.MixerGroup = _sfxMixerGroup;
            
            var audioSource = GetPooledAudioSource();
            audioSource.PlayAudio(request);

            StartCoroutine(ReturnToPoolWhenFinished(audioSource));
        }

        public void PlayMusic(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
            {
                return;
            }

            if (_currentMusicSource != null)
            {
                _currentMusicSource.Stop();
                ReturnToPool(_currentMusicSource);
            }
            
            var request = AudioRequest.Create2D(clip, volume, pitch);
            request.Loop = true;
            request.MixerGroup = _musicMixerGroup;
            
            _currentMusicSource = GetPooledAudioSource();
            _currentMusicSource.PlayAudio(request);
        }

        public void StopMusic()
        {
            if (_currentMusicSource != null)
            {
                _currentMusicSource.Stop();
                ReturnToPool(_currentMusicSource);
                _currentMusicSource = null;
            }
        }

        private IEnumerator ReturnToPoolWhenFinished(PooledAudioSource audioSource)
        {
            yield return new WaitWhile(() => audioSource.IsPlaying);
            ReturnToPool(audioSource);
        }
    }
}