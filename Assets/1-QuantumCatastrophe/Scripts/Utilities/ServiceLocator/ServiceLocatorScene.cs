using UnityEngine;

namespace QC.Utilities.ServiceLocation
{
    /// <summary>
    /// Scene-specific service locator bootstrapper that is destroyed when the scene unloads.
    /// Use this for services that should only exist within a particular scene's lifetime.
    /// </summary>
    [AddComponentMenu("SCP/ServiceLocator/ServiceLocator Scene")]
    public class ServiceLocatorScene : Bootstrapper
    {
        /// <summary>
        /// Bootstraps the scene-specific service locator.
        /// Configures the container to be scoped to the current scene only.
        /// </summary>
        protected override void Bootstrap()
        {
            // Configure this container as scene-scoped (will be destroyed on scene unload)
            Container.ConfigureForScene();
        }
    }
}