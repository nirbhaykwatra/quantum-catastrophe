using QC.Utilities.EventBusSystem;
using UnityEngine;

namespace QC.Utilities.ServiceLocation
{
    /// <summary>
    /// Global service locator bootstrapper that persists across scene loads.
    /// Configures the service container as global and registers core services like the EventBusRegistry.
    /// </summary>
    [AddComponentMenu("QC/ServiceLocator/ServiceLocator Global")]
    public class ServiceLocatorGlobal : Bootstrapper
    {
        /// <summary>
        /// Whether this GameObject should persist across scene loads using DontDestroyOnLoad.
        /// </summary>
        [SerializeField] private bool m_dontDestroyOnLoad = true;

        /// <summary>
        /// Bootstraps the global service locator.
        /// Configures the container as global and registers the EventBusRegistry service.
        /// </summary>
        protected override void Bootstrap()
        {
            // Unparent this GameObject so that it can persist across scene loads
            if (transform.parent != null) transform.SetParent(null);
            // Configure this container as the global service locator instance
            Container.ConfigureAsGlobal(m_dontDestroyOnLoad);

            // Register the event bus registry for global event communication
            Container.Register(new EventBusRegistry());
        }
    }
}