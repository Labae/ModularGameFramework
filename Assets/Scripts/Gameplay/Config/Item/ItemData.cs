using UnityEngine;

namespace MarioGame.Gameplay.Config.Item
{
    public abstract class ItemData : ScriptableObject
    {
        public string ItemName;
        public Sprite ItemSprite;
    }
}