using QC.Utilities.Extensions;
using UnityEngine;

namespace QC.Utilities.ServiceLocation
{
    /// <summary>
    /// Abstract base class for bootstrapping service registration with the ServiceLocator.
    /// Ensures services are registered once during initialization through the Bootstrap method.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private ServiceLocator m_container;

        /// <summary>
        /// Gets the ServiceLocator component, caching it on first access.
        /// </summary>
        internal ServiceLocator Container => m_container.OrNull() ?? (m_container = GetComponent<ServiceLocator>());

        /// <summary>
        /// Tracks whether the bootstrapper has already executed to prevent duplicate initialization.
        /// </summary>
        private bool m_hasBeenBootstrapped;

        /// <summary>
        /// Unity lifecycle method that triggers bootstrapping on scene load.
        /// </summary>
        private void Awake() => BootstrapOnDemand();

        /// <summary>
        /// Initiates the bootstrap process if it hasn't already been executed.
        /// Can be called manually to ensure services are registered before normal Awake timing.
        /// </summary>
        public void BootstrapOnDemand()
        {
            // If the bootstrapper has already executed, do nothing
            if (m_hasBeenBootstrapped) return;
            
            // Otherwise, execute the bootstrap process and set the flag to true to prevent duplicate execution
            m_hasBeenBootstrapped = true;
            Bootstrap();
        }

        /// <summary>
        /// Override this method to register services with the ServiceLocator.
        /// Called once during the bootstrapping process.
        /// </summary>
        protected abstract void Bootstrap();
    }
}