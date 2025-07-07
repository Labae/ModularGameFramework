using MarioGame.Core.Entities;
using MarioGame.Core.Entities.Interfaces;
using MarioGame.Debugging.Core;
using MarioGame.Debugging.Interfaces;
using Reflex.Core;
using UnityEngine;
using ILogger = UnityEngine.ILogger;
using Logger = UnityEngine.Logger;

namespace MarioGame.Core
{
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(typeof(ILogger), 
                typeof(Logger));
            containerBuilder.AddSingleton(typeof(IAssertManager), 
                typeof(AssertManager));
            containerBuilder.AddSingleton(typeof(IEntityFactory), 
                typeof(EntityFactory));
        }
    }
}