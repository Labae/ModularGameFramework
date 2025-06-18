using System;

namespace EventBus.Core
{
    /// <summary>
    /// A generic class that implements the IEventBinding interface to handle event bindings.
    /// Provides support for both typed and parameterless event handlers.
    /// </summary>
    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        Action<T> IEventBinding<T>.OnEvent
        {
            get => _onEvent;
            set => _onEvent = value;
        }

        Action IEventBinding<T>.OnEventNoArgs
        {
            get => _onEventNoArgs;
            set => _onEventNoArgs = value;
        }

        private Action<T> _onEvent = _ => { };
        private Action _onEventNoArgs = () => { };

        public EventBinding(Action<T> onEvent) => _onEvent = onEvent;
        public EventBinding(Action onEventNoArgs) => _onEventNoArgs = onEventNoArgs;

        public void Add(Action<T> onEvent) => _onEvent += onEvent;
        public void Remove(Action<T> onEvent) => _onEvent -= onEvent;
        
        public void Add(Action onEvent) => _onEventNoArgs += onEvent;
        public void Remove(Action onEvent) => _onEventNoArgs -= onEvent;
    }
}