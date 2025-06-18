using System;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Pickups;

namespace MarioGame.Gameplay.Interfaces.Pickups
{
    public interface IPickupable
    {
        bool CanBePickedUp { get; }
        event Action OnPickup;
        bool TryPickup(out Pickup pickup);
    }
}