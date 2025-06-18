using MarioGame.Gameplay.Config.Weapon;
using UnityEngine;

namespace MarioGame.Gameplay.Config.Item
{
    [CreateAssetMenu(menuName = "MarioGame/Gameplay/Item/" + nameof(WeaponData)
        , fileName = nameof(WeaponData))]
    public class WeaponData : ScriptableObject
    {
        public string WeawponName;
        public Sprite WeaponSprite;
        
        public WeaponConfiguration Configuration;
        public AudioClip PickupSFX;
    }
}