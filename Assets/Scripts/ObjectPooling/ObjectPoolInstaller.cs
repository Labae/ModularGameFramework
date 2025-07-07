using MarioGame.Core.ObjectPooling;
using MarioGame.Core.ObjectPooling.Interface;
using Reflex.Core;
using UnityEngine;

namespace MarioGame.ObjectPooling
{
    public class ObjectPoolInstaller : MonoBehaviour, IInstaller
    {
        [Header("Pool Manager Settings")]
        [SerializeField] private ObjectPoolManager _poolManagerPrefab;
        
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(_ =>
            {
                var poolManagerInstance = Object.Instantiate(_poolManagerPrefab);
                poolManagerInstance.name = "ObjectPoolManager";
                return poolManagerInstance;
            }, typeof(IObjectPoolManager));
        }
    }
}