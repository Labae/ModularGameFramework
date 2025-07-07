using MarioGame.Gameplay.Effects;
using MarioGame.Gameplay.Effects.Interface;
using Reflex.Core;
using UnityEngine;

namespace MarioGame.Gameplay
{
    public class GameplayInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(typeof(EffectFactory), typeof(IEffectFactory));
        }
    }
}