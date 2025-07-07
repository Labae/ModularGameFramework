using MarioGame.Core.Entities.Interfaces;
using MarioGame.Debugging.Interfaces;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;

namespace MarioGame.Core.Entities
{
    public class EntityFactory : IEntityFactory
    {
        private readonly IDebugLogger _debugLogger;

        public EntityFactory(IDebugLogger debugLogger)
        {
            _debugLogger = debugLogger;
        }
        
        public T CreateEntity<T>(T prefab, Vector3 position, Quaternion rotation = default) where T : Entity
        {
            if (prefab == null)
            {
                _debugLogger.Error("Cannot create an entity with a null prefab");
                return null;
            }
            
            var instance = Object.Instantiate(prefab, position, rotation);
            InjectDependencies(instance);
            
            _debugLogger.System($"Created an entity: {typeof(T).Name} at position: {position} and rotation: {rotation}");
            return instance;
        }

        public T CreateEntity<T>(T prefab, Transform parent, Vector3 localPosition = default) where T : Entity
        {
            if (prefab == null)
            {
                _debugLogger.Error("Cannot create an entity with a null prefab");
                return null;
            }
            
            var instance = Object.Instantiate(prefab, parent);
            instance.transform.localPosition = localPosition;
            InjectDependencies(instance);
            
            _debugLogger.System($"Created an entity: {typeof(T).Name} under parent: {parent?.name ?? "null"}");
            return instance;
        }

        public T CreateEntity<T>(T prefab) where T : Entity
        {
            return CreateEntity(prefab, Vector3.zero, Quaternion.identity);
        }
        
        private void InjectDependencies<T>(T instance) where T : Entity
        {
            if (instance == null)
            {
                return;
            }

            if (!instance.gameObject.scene.IsValid())
            {
                _debugLogger.Warning($"Invalid scene for entity: {typeof(T).Name}");
            }
            else
            {
                var container = instance.gameObject.scene.GetSceneContainer();
                if (container == null)
                {
                    _debugLogger.Warning($"No scene container found for entity: {typeof(T).Name}");
                }
                else
                {
                    GameObjectInjector.InjectObject(instance.gameObject, container);
                }
            }
        }
    }
}