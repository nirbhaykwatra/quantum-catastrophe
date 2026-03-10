using System;
using QC.Utilities.ServiceLocation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QC.Utilities.EventBusSystem
{
    /// <summary>
    /// MonoBehaviour component that drives the event bus system by processing queued events each frame.
    /// Must be present in the scene for queued events to be dispatched.
    /// </summary>
    public class EventQueueProcessor : MonoBehaviour
    {
        /// <summary>
        /// Reference to the central event bus registry containing all channels.
        /// </summary>
        private EventBusRegistry m_eventBusRegistry;

        /// <summary>
        /// If true, this GameObject persists across scene loads to maintain event processing.
        /// </summary>
        [SerializeField] private bool m_dontDestroyOnLoad = true;

        /// <summary>
        /// If true, enables detailed trace logging for all event buses to aid debugging.
        /// </summary>
        [SerializeField] private bool m_enableTraceLogging = false;

        /// <summary>
        /// Initializes the processor by retrieving the event bus registry from the service locator
        /// and applying configuration settings.
        /// </summary>
        private void OnEnable()
        {
            // Unparent this GameObject so that it can persist across scene loads
            if (transform.parent != null) transform.SetParent(null);
            // Get the event bus registry from the global service locator
            m_eventBusRegistry = ServiceLocator.Global.Get<EventBusRegistry>();

            // Enable trace logging on all buses if configured
            if (m_enableTraceLogging)
            {
                foreach (EventBus bus in m_eventBusRegistry.GetAllBuses())
                {
                    bus.EnableTraceLogging = true;
                }
            }

            // Persist this processor across scene loads if configured
            if (m_dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Processes all queued events on all buses at the end of each frame.
        /// LateUpdate ensures events are processed after all regular Update logic completes.
        /// </summary>
        private void LateUpdate()
        {
            // Process the event queue for each registered bus
            foreach (EventBus bus in m_eventBusRegistry.GetAllBuses())
            {
                bus.ProcessQueue();
            }
        }
    }

    // Uncomment the following code if you want to automatically create an EventQueueProcessor instance before any scene loads

    /*
    /// <summary>
    /// Optional auto-initializer that creates an EventQueueProcessor before the first scene loads.
    /// Uncomment this class to ensure the event system is always active without manual scene setup.
    /// </summary>
    public static class EventQueueInitializer
    {
        /// <summary>
        /// Automatically creates an EventQueueProcessor GameObject before the first scene loads.
        /// This ensures the event system is ready to process events immediately on startup.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateEventProcessor()
        {
            GameObject go = new GameObject("EventQueueProcessor", typeof(EventQueueProcessor));
        }
    }
    */
}