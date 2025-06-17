using System;
using MarioGame.Core.Entities;
using MarioGame.Core.Enums;
using UnityEngine;

namespace MarioGame.Gameplay.Interfaces.Weapon
{
    public interface IWeaponDirectionController : IDisposable
    {
        Vector2 CurrentDirection { get; }
        void Initialize();
        void SetDirection(Vector2 zero);
    }
}