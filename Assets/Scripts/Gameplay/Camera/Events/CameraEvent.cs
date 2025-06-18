using EventBus.Core;
using MarioGame.Core.Entities;
using MarioGame.Gameplay.Config.Data;

namespace MarioGame.Gameplay.Camera.Events
{
    public enum CameraEventType
    {
        SetTarget,
        Shake,
    }
    
    public struct CameraEvent : IEvent
    {
        public CameraEventType Type;
        public int Priority;
        public Entity Target;
        public ShakeData ShakeData;
    }

    public static class CameraEventGenerator
    {
        public static void SetTarget(Entity entity)
        {
            GenerateEvent(CameraEventType.SetTarget, entity);
        }

        public static void Shake(ShakeData shakeData, int priority = 0)
        {
            GenerateEvent(CameraEventType.Shake, shakeData: shakeData, priority: priority);
        }

        private static void GenerateEvent(CameraEventType type, Entity entity = null, ShakeData shakeData = null, int priority = 0)
        {
            EventBus<CameraEvent>.Raise(new CameraEvent
            {
                Type = type,
                Priority = priority,
                Target = entity,
                ShakeData = shakeData,
            });
        }
    }
}