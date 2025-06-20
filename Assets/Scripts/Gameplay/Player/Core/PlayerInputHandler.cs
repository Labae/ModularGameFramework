using Gameplay.Interfaces;
using UnityEngine;

namespace Gameplay.Player.Core
{
    public class PlayerInputHandler : IPlayerInputProvider
    {
        public float HorizontalInput { get; private set; }
        public bool ShootPressed { get; private set; }

        public void Update()
        {
            HorizontalInput = Input.GetAxisRaw("Horizontal");
            ShootPressed = Input.GetKey(KeyCode.Z);
        }
    }
}