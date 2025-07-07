using UnityEngine;

namespace MarioGame.Core.Entities.Interfaces
{
    public interface IEntityFactory
    {
        T CreateEntity<T>(T prefab, Vector3 position, Quaternion rotation = default) where T : Entity;
        T CreateEntity<T>(T prefab, Transform parent, Vector3 localPosition = default) where T : Entity;
        T CreateEntity<T>(T prefab) where T : Entity;
    }
}