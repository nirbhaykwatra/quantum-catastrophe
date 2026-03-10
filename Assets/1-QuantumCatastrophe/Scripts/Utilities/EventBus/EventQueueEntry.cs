namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Non-generic interface for queued event entries, allowing polymorphic storage and processing
    /// of events with different type parameters.
    /// </summary>
    public interface IEventQueueEntry
    {
        /// <summary>
        /// Dispatches the queued event to all subscribers on the specified event bus.
        /// </summary>
        /// <param name="eventBus">The bus to dispatch the event on</param>
        void Dispatch(EventBus eventBus);

        /// <summary>
        /// Returns this entry to its object pool for reuse, clearing stored data.
        /// </summary>
        void ReturnToPool();

        /// <summary>
        /// Gets the trace information associated with this event for debugging purposes.
        /// </summary>
        EventTrace Trace { get; }
    }

    /// <summary>
    /// Generic implementation of a queued event entry that wraps an event with its trace information.
    /// Used internally by the event bus queue system to defer event dispatching.
    /// Instances are pooled to reduce allocations.
    /// </summary>
    /// <typeparam name="T">The specific event type being queued</typeparam>
    internal class EventQueueEntry<T> : IEventQueueEntry where T : IEvent
    {
        /// <summary>
        /// The actual event data to be dispatched.
        /// </summary>
        public T Event;

        /// <summary>
        /// Trace information for debugging event origins and flow.
        /// </summary>
        public EventTrace Trace { get; set; }

        /// <summary>
        /// Dispatches the stored event to all subscribers, including the trace information.
        /// </summary>
        /// <param name="eventBus">The bus to dispatch the event on</param>
        public void Dispatch(EventBus eventBus)
        {
            eventBus.RaiseWithTrace(Event, Trace);
        }

        /// <summary>
        /// Clears the event data and returns this entry to the object pool for reuse.
        /// This helps reduce garbage collection pressure from frequent event queuing.
        /// </summary>
        public void ReturnToPool()
        {
            Event = default;
            Trace = EventTrace.Empty;
            EventQueueEntryPool<T>.Return(this);
        }
    }
}