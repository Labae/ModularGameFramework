using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarioGame.Core.Reactive
{
    [System.Serializable]
    public class ObservableProperty<T> 
    {
        [SerializeField] private T _value;
        
        public event Action<T> OnValueChanged;

        /// <summary>
        /// (newValue, oldValue)
        /// </summary>
        public event Action<T, T> OnValueChangedWithPreviousValue;

        public T Value
        {
            get => _value;
            set => SetValue(value);
        }

        public ObservableProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        public void SetValue(T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(_value, newValue))
            {
                return;
            }
            
            var oldValue = _value;
            _value = newValue;
            
            OnValueChanged?.Invoke(newValue);
            OnValueChangedWithPreviousValue?.Invoke(newValue, oldValue);
        }
        
        public static implicit operator T(ObservableProperty<T> property) => property.Value;
    }
}