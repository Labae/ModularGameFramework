using System.Collections.Generic;
using UnityEngine;

namespace EventBus.Core
{
    public class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> _bindings = new();
        private static readonly HashSet<IEventBinding<T>> _toRemove = new();
        
        private static bool _isRaising = false;

        public static void Register(EventBinding<T> binding) => _bindings.Add(binding);

        public static void Deregister(EventBinding<T> binding)
        {
            if (_isRaising)
            {
                _toRemove.Add(binding);
            }
            else
            {
                _bindings.Remove(binding);
            }
        } 

        public static void Raise(T @event)
        {
            _isRaising = true;
            foreach (var binding in _bindings)
            {
                if (!_toRemove.Contains(binding))
                {
                    binding.OnEvent.Invoke(@event);
                    binding.OnEventNoArgs.Invoke();   
                }
            }
            
            _isRaising = false;

            foreach (var binding in _toRemove)
            {
                _bindings.Remove(binding);
            }
            _toRemove.Clear();
        }

        private static void Clear()
        {
            Debug.Log($"Clearing {typeof(T).Name} bindings");
        }
    }
}