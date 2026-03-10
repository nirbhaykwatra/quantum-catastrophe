using System;
using System.Collections.Generic;

namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// Enumeration of available event bus channels for backwards compatibility and enum-based access.
    /// </summary>
    public enum BusChannel { Global, UI, Simulation, Audio }

    /// <summary>
    /// Central registry that manages and provides access to all event bus channels.
    /// Supports both type-safe generic access and legacy enum-based access patterns.
    /// </summary>
    public class EventBusRegistry
    {
        // Strongly-typed bus instances for each channel
        private GlobalEventBus m_globalBus;
        private UIEventBus m_uiBus;
        private SimulationEventBus m_gameplayBus;
        private AudioEventBus m_audioBus;

        // Cached array of all buses for efficient iteration without allocation
        private EventBus[] m_allBuses;

        // Lookup tables for fast bus retrieval by type or channel
        private readonly Dictionary<Type, EventBus> m_busesByType = new();
        private readonly Dictionary<BusChannel, EventBus> m_busesByChannel = new();

        /// <summary>
        /// Initializes the registry and creates all event bus channels.
        /// </summary>
        public EventBusRegistry()
        {
            // Pre-create all channels with typed buses
            m_globalBus = new GlobalEventBus();
            m_uiBus = new UIEventBus();
            m_gameplayBus = new SimulationEventBus();
            m_audioBus = new AudioEventBus();

            // Store by channel for enum-based access
            m_busesByChannel[BusChannel.Global] = m_globalBus;
            m_busesByChannel[BusChannel.UI] = m_uiBus;
            m_busesByChannel[BusChannel.Simulation] = m_gameplayBus;
            m_busesByChannel[BusChannel.Audio] = m_audioBus;

            // Store by type for generic Get<T>() access
            m_busesByType[typeof(GlobalEventBus)] = m_globalBus;
            m_busesByType[typeof(UIEventBus)] = m_uiBus;
            m_busesByType[typeof(SimulationEventBus)] = m_gameplayBus;
            m_busesByType[typeof(AudioEventBus)] = m_audioBus;

            // Cache array for efficient iteration
            m_allBuses = new EventBus[] { m_globalBus, m_uiBus, m_gameplayBus, m_audioBus };
        }
        
        /// <summary>
        /// Retrieves a strongly-typed event bus by its concrete type.
        /// </summary>
        /// <typeparam name="T">The specific EventBus type to retrieve (e.g., GlobalEventBus, UIEventBus)</typeparam>
        /// <returns>The requested event bus instance</returns>
        /// <exception cref="InvalidOperationException">Thrown when no bus is registered for the specified type</exception>
        public T Get<T>() where T : EventBus
        {
            if (m_busesByType.TryGetValue(typeof(T), out EventBus bus))
            {
                return (T)bus;
            }
            throw new InvalidOperationException($"No bus registered for type {typeof(T).Name}");
        }
        
        /// <summary>
        /// Retrieves all event buses as a read-only span for efficient iteration without allocations.
        /// Useful for operations that need to process all buses (e.g., clearing, updating).
        /// </summary>
        /// <returns>A read-only span containing all registered event buses</returns>
        public ReadOnlySpan<EventBus> GetAllBuses()
        {
            return m_allBuses;
        }

        /// <summary>
        /// Retrieves an event bus by its channel enum value.
        /// Provided for backwards compatibility with legacy enum-based access patterns.
        /// </summary>
        /// <param name="channel">The channel enum identifying which bus to retrieve</param>
        /// <returns>The event bus associated with the specified channel</returns>
        public EventBus Get(BusChannel channel) => m_busesByChannel[channel];

        /// <summary>
        /// Clears all event bindings and queued events from all buses in the registry.
        /// Use this when resetting the event system (e.g., during scene transitions or cleanup).
        /// </summary>
        public void ClearAllBindingsAndQueues()
        {
            m_globalBus.ClearAllBindingsAndQueues();
            m_uiBus.ClearAllBindingsAndQueues();
            m_gameplayBus.ClearAllBindingsAndQueues();
            m_audioBus.ClearAllBindingsAndQueues();
        }
    }
}