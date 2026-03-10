using System;
using System.Collections.Generic;
using UnityEngine;

namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Type-safe event bus that supports both immediate and queued event dispatch.
    /// Events can be raised immediately or enqueued for later processing.
    /// </summary>
    public class EventBus
    {
        /// <summary>
        /// Dictionary mapping event types to their registered event bindings.
        /// Each event type has a HashSet of bindings that will be invoked when the event is raised.
        /// </summary>
        private readonly Dictionary<Type, object> m_bindings = new();

        /// <summary>
        /// Queue for events that should be processed later via ProcessQueue().
        /// Allows for batched or deferred event processing.
        /// </summary>
        private readonly Queue<IEventQueueEntry> m_eventQueue = new();

        /// <summary>
        /// When enabled, logs trace information about event dispatch including stack traces.
        /// Useful for debugging event flow.
        /// </summary>
        public bool EnableTraceLogging { get; set; }

        #region Registration Methods

        /// <summary>
        /// Gets or creates the HashSet of bindings for a specific event type.
        /// Creates a new HashSet if one doesn't exist for the given event type.
        /// </summary>
        /// <typeparam name="T">The event type to get bindings for.</typeparam>
        /// <returns>The HashSet of bindings for the specified event type.</returns>
        private HashSet<IEventBinding<T>> GetBindings<T>() where T : IEvent
        {
            Type key = typeof(T);
            // Return existing bindings if they exist
            if (m_bindings.TryGetValue(key, out object existing))
            {
                return (HashSet<IEventBinding<T>>)existing;
            }

            // Create new binding set for this event type
            HashSet<IEventBinding<T>> set = new();
            m_bindings[key] = set;
            return set;
        }

        /// <summary>
        /// Registers an event binding to listen for events of type T.
        /// </summary>
        /// <typeparam name="T">The event type to listen for.</typeparam>
        /// <param name="binding">The event binding to register.</param>
        public void Register<T>(EventBinding<T> binding) where T : IEvent => GetBindings<T>().Add(binding);

        /// <summary>
        /// Deregisters an event binding so it will no longer receive events of type T.
        /// </summary>
        /// <typeparam name="T">The event type to stop listening for.</typeparam>
        /// <param name="binding">The event binding to deregister.</param>
        public void Deregister<T>(EventBinding<T> binding) where T : IEvent => GetBindings<T>().Remove(binding);

        #endregion
        
        #region Immediate Dispatch Methods

        /// <summary>
        /// Raises an event immediately, invoking all registered bindings.
        /// Captures a stack trace for debugging if trace logging is enabled.
        /// </summary>
        /// <typeparam name="T">The event type to raise.</typeparam>
        /// <param name="event">The event data to dispatch.</param>
        public void Raise<T>(T @event) where T : IEvent
        {
            // Capture stack trace for debugging
            EventTrace trace = EventTrace.Capture();
            RaiseWithTrace(@event, trace);
        }

        /// <summary>
        /// Raises an event immediately with an explicit trace.
        /// Invokes all registered bindings for the event type.
        /// </summary>
        /// <typeparam name="T">The event type to raise.</typeparam>
        /// <param name="event">The event data to dispatch.</param>
        /// <param name="trace">The trace information for debugging.</param>
        public void RaiseWithTrace<T>(T @event, EventTrace trace) where T : IEvent
        {
            // Log trace information if enabled
            if (EnableTraceLogging && trace.IsValid)
            {
                Debug.Log($"[{this}] {typeof(T).Name} dispatched {trace}");
            }

            // Invoke all registered bindings for this event type
            foreach (IEventBinding<T> binding in GetBindings<T>())
            {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }

        #endregion
        
        #region Queued Dispatch Methods

        /// <summary>
        /// Enqueues an event for later processing via ProcessQueue().
        /// Uses an object pool to reduce allocations.
        /// </summary>
        /// <typeparam name="T">The event type to enqueue.</typeparam>
        /// <param name="event">The event data to queue.</param>
        public void Enqueue<T>(T @event) where T : IEvent
        {
            // Rent a queue entry from the pool to reduce allocations
            EventQueueEntry<T> entry = EventQueueEntryPool<T>.Rent();
            entry.Event = @event;

            // Capture stack trace for debugging
            EventTrace trace = EventTrace.Capture();
            entry.Trace = trace;

            m_eventQueue.Enqueue(entry);
        }

        /// <summary>
        /// Processes all queued events, dispatching them immediately.
        /// Returns queue entries to the pool after dispatch.
        /// </summary>
        public void ProcessQueue()
        {
            // Process all queued events
            while (m_eventQueue.Count > 0)
            {
                IEventQueueEntry entry = m_eventQueue.Dequeue();
                entry.Dispatch(this);
                entry.ReturnToPool();
            }
        }

        /// <summary>
        /// Processes up to maxCount queued events.
        /// Useful for spreading event processing across multiple frames.
        /// </summary>
        /// <param name="maxCount">Maximum number of events to process.</param>
        public void ProcessQueue(int maxCount)
        {
            int processed = 0;
            while (m_eventQueue.Count > 0 && processed < maxCount)
            {
                IEventQueueEntry entry = m_eventQueue.Dequeue();
                entry.Dispatch(this);
                entry.ReturnToPool();
                processed++;
            }
        }

        /// <summary>
        /// Gets the number of events currently in the queue.
        /// </summary>
        public int QueuedCount => m_eventQueue.Count;

        #endregion
        
        #region Clearing Methods

        /// <summary>
        /// Clears all queued events without dispatching them.
        /// Returns all queue entries to their respective pools.
        /// </summary>
        public void ClearQueue()
        {
            while (m_eventQueue.Count > 0)
            {
                m_eventQueue.Dequeue().ReturnToPool();
            }
        }

        /// <summary>
        /// Clears all bindings for a specific event type.
        /// After calling this, no listeners will receive events of type T until they re-register.
        /// </summary>
        /// <typeparam name="T">The event type to clear bindings for.</typeparam>
        public void ClearBinding<T>() where T : IEvent
        {
            Debug.Log($"Clearing {typeof(T).Name} bindings...");
            if (m_bindings.TryGetValue(typeof(T), out object set))
            {
                ((HashSet<IEventBinding<T>>)set).Clear();
            }
        }

        /// <summary>
        /// Clears all event bindings and queued events.
        /// This completely resets the event bus to its initial state.
        /// </summary>
        public void ClearAllBindingsAndQueues()
        {
            Debug.Log($"Clearing all event bus bindings and queues...");
            m_bindings.Clear();
            ClearQueue();
        }

        #endregion
    }
}