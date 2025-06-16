using MarioGame.Gameplay.Config.Input;
using MarioGame.Gameplay.Interfaces;
using UnityEngine;

namespace MarioGame.Gameplay.Input
{
    /// <summary>
    /// 플레이어 입력 제공자
    /// 키보드입력을 처리
    /// </summary>
    [System.Serializable]
    public sealed class PlayerInputProvider : IInputProvider
    {
        [field: SerializeField]
        public float MoveDirection { get; private set; }

        [field: SerializeField]
        public float VerticalInput { get; private set; }

        [field: SerializeField]
        public bool JumpPressed { get; private set; }
        
        [field: SerializeField]
        public bool JumpHeld { get; private set; }
        
        [field: SerializeField]
        public bool JumpReleased { get; private set; }

        [field: SerializeField]
        public bool CrouchHeld { get; private set; }

        private readonly PlayerInputReader _playerInputReader;
        
        public PlayerInputProvider(PlayerInputReader inputReader)
        {
            _playerInputReader = inputReader;
            _playerInputReader.EnableAllControls();
        }
        
        public void UpdateInput()
        {
            MoveDirection = _playerInputReader.MoveDirection;
            VerticalInput = _playerInputReader.VerticalInput;

            JumpPressed = _playerInputReader.JumpPressed;
            JumpHeld = _playerInputReader.JumpHeld;
            JumpReleased = _playerInputReader.JumpReleased;
            CrouchHeld = _playerInputReader.CrouchHeld;
        }

        public void ResetFrameInputs()
        {
            _playerInputReader.ResetFrameInputs();
        }

        public void Dispose()
        {
            _playerInputReader.DisableAllControls();
        }
    }
}