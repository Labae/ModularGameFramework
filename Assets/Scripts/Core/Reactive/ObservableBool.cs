using System;

namespace MarioGame.Core.Reactive
{
    [System.Serializable]
    public class ObservableBool : ObservableProperty<bool>
    {
        public event Action OnBecameTrue;
        public event Action OnBecameFalse;

        public ObservableBool(bool initialValue = false) : base(initialValue)
        {
            OnValueChangedWithPreviousValue += HandleBoolChange;
        }

        private void HandleBoolChange(bool newValue, bool oldValue)
        {
            if (!oldValue && newValue)
            {
                OnBecameTrue?.Invoke();
            }
            else if (oldValue && !newValue)
            {
                OnBecameFalse?.Invoke();
            }
        }
    }
}