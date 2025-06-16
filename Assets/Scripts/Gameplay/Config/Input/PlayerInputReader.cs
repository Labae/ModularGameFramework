using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarioGame.Gameplay.Config.Input
{
    [CreateAssetMenu(fileName = nameof(PlayerInputReader),
        menuName = "MarioGame/Gameplay/Input/" + nameof(PlayerInputReader))]
    public class PlayerInputReader : InputReader, InputControls.IPlayerActions
    {
        [field: SerializeField] public float MoveDirection { get; private set; }

        [field: SerializeField] public float VerticalInput { get; private set; }

        [field: SerializeField] public bool JumpPressed { get; private set; }

        [field: SerializeField] public bool JumpHeld { get; private set; }

        [field: SerializeField] public bool JumpReleased { get; private set; }

        [field: SerializeField] public bool CrouchHeld { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            _inputControls.Player.SetCallbacks(this);
        }

        public override void EnableAllControls()
        {
            _inputControls.Player.Enable();
        }

        public override void DisableAllControls()
        {
            _inputControls.Player.Disable();
        }

        public void ResetFrameInputs()
        {
            JumpPressed = false;
            JumpReleased = false;
        }

        public void OnHorizontal(InputAction.CallbackContext context)
        {
            MoveDirection = context.ReadValue<float>();
        }

        public void OnVertical(InputAction.CallbackContext context)
        {
            VerticalInput = context.ReadValue<float>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                JumpPressed = true;
            }

            JumpHeld = context.ReadValueAsButton();

            if (context.canceled)
            {
                JumpReleased = true;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            CrouchHeld = context.ReadValueAsButton();
        }
    }
}