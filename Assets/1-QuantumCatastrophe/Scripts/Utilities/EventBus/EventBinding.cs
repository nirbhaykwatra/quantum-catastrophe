using System;

namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Internal interface for event bindings that allows the EventBus to invoke callbacks.
    /// Supports both parameterized and parameterless callback variants.
    /// </summary>
    /// <typeparam name="T">The event type this binding handles.</typeparam>
    internal interface IEventBinding<T>
    {
        /// <summary>
        /// Callback invoked with the event data when the event is raised.
        /// </summary>
        public Action<T> OnEvent { get; set; }

        /// <summary>
        /// Callback invoked without parameters when the event is raised.
        /// Useful when the listener doesn't need the event data.
        /// </summary>
        public Action OnEventNoArgs { get; set; }
    }
    
    /// <summary>
    /// Represents a binding between an event type and callback actions.
    /// Supports both parameterized callbacks (with event data) and parameterless callbacks.
    /// Multiple callbacks can be added to a single binding.
    /// </summary>
    /// <typeparam name="T">The event type this binding handles.</typeparam>
    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        /// <summary>
        /// Action invoked with event data. Defaults to an empty action.
        /// </summary>
        private Action<T> m_onEvent = _ => { };

        /// <summary>
        /// Action invoked without parameters. Defaults to an empty action.
        /// </summary>
        private Action m_onEventNoArgs = () => { };

        /// <summary>
        /// Explicit interface implementation for parameterized event callback.
        /// </summary>
        Action<T> IEventBinding<T>.OnEvent
        {
            get => m_onEvent;
            set => m_onEvent = value;
        }

        /// <summary>
        /// Explicit interface implementation for parameterless event callback.
        /// </summary>
        Action IEventBinding<T>.OnEventNoArgs
        {
            get => m_onEventNoArgs;
            set => m_onEventNoArgs = value;
        }

        /// <summary>
        /// Creates a binding with a parameterized callback.
        /// </summary>
        /// <param name="onEvent">Callback to invoke with event data.</param>
        public EventBinding(Action<T> onEvent) => m_onEvent = onEvent;

        /// <summary>
        /// Creates a binding with a parameterless callback.
        /// </summary>
        /// <param name="onEventNoArgs">Callback to invoke without parameters.</param>
        public EventBinding(Action onEventNoArgs) => m_onEventNoArgs = onEventNoArgs;

        /// <summary>
        /// Adds a parameterized callback to this binding.
        /// </summary>
        /// <param name="onEvent">Callback to add.</param>
        public void Add(Action<T> onEvent) => m_onEvent += onEvent;

        /// <summary>
        /// Removes a parameterized callback from this binding.
        /// </summary>
        /// <param name="onEvent">Callback to remove.</param>
        public void Remove(Action<T> onEvent) => m_onEvent -= onEvent;

        /// <summary>
        /// Adds a parameterless callback to this binding.
        /// </summary>
        /// <param name="onEventNoArgs">Callback to add.</param>
        public void Add(Action onEventNoArgs) => m_onEventNoArgs += onEventNoArgs;

        /// <summary>
        /// Removes a parameterless callback from this binding.
        /// </summary>
        /// <param name="onEventNoArgs">Callback to remove.</param>
        public void Remove(Action onEventNoArgs) => m_onEventNoArgs -= onEventNoArgs;
    }
}