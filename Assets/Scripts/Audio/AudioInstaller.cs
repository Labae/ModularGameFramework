using Reflex.Core;
using UnityEngine;

namespace MarioGame.Audio
{
    public class AudioInstaller : MonoBehaviour, IInstaller
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioManager _audioManagerPrefab;
        
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(_ =>
            {
                var instance = Instantiate(_audioManagerPrefab);
                instance.name = "AudioManager";
                return instance;
            }, typeof(AudioManager));
        }
    }
}