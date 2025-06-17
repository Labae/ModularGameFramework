using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Combat
{
    /// <summary>
    /// 파괴 가능한 오브젝트 인터페이스
    /// </summary>
    public interface IDestructible
    {
        void Destroy(Vector2 hitPoint, int damage);
        bool IsDestructible { get; }
    }
}