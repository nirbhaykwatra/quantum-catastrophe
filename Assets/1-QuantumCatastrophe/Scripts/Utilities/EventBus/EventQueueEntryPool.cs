using System.Collections;
using System.Collections.Generic;

namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Generic object pool for EventQueueEntry instances to reduce allocations during event queuing.
    /// Each event type T has its own independent pool to ensure type safety and optimal memory reuse.
    /// </summary>
    /// <typeparam name="T">The event type this pool manages entries for</typeparam>
    internal static class EventQueueEntryPool<T> where T : IEvent
    {
        // Pre-allocated stack with initial capacity of 32 to handle typical burst scenarios
        private static readonly Stack<EventQueueEntry<T>> s_pool = new(32);

        /// <summary>
        /// Retrieves an EventQueueEntry from the pool, or creates a new one if the pool is empty.
        /// </summary>
        /// <returns>A reusable EventQueueEntry instance ready for use</returns>
        public static EventQueueEntry<T> Rent() => s_pool.Count > 0 ? s_pool.Pop() : new EventQueueEntry<T>();

        /// <summary>
        /// Returns an EventQueueEntry to the pool after clearing its data.
        /// The entry can then be reused for future events, reducing GC pressure.
        /// </summary>
        /// <param name="entry">The entry to return to the pool</param>
        public static void Return(EventQueueEntry<T> entry)
        {
            // Clear the entry data before returning to pool to prevent reference leaks
            entry.Event = default;
            entry.Trace = EventTrace.Empty;
            s_pool.Push(entry);
        }
    }
}