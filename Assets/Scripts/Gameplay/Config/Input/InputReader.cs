using UnityEngine;

namespace MarioGame.Gameplay.Config.Input
{
    public abstract class InputReader : ScriptableObject
    {
        protected InputControls _inputControls;

        protected virtual void OnEnable()
        {
            _inputControls = new InputControls();
        }

        protected void OnDisable()
        {
            DisableAllControls();
        }

        public abstract void EnableAllControls();
        public abstract void DisableAllControls();
    }
}