using System;
using System.Collections.Generic;
using System.Linq;
using QC.Utilities.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QC.Utilities.ServiceLocation
{
    /// <summary>
    /// Implements the Service Locator pattern for dependency injection in Unity.
    /// Provides a hierarchical service lookup system with global, scene, and local scopes.
    /// Services registered in child locators take precedence over parent locators.
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        #region Private Fields
        
        /// <summary>
        /// The singleton global service locator instance.
        /// </summary>
        private static ServiceLocator s_global;

        /// <summary>
        /// Dictionary mapping Unity scenes to their respective scene-scoped service locators.
        /// </summary>
        private static Dictionary<Scene, ServiceLocator> s_sceneContainers;

        /// <summary>
        /// Temporary list used for iterating through scene root GameObjects to avoid allocations.
        /// </summary>
        private static List<GameObject> s_tmpSceneGameObjects;

        /// <summary>
        /// The internal service manager that stores and retrieves registered services.
        /// </summary>
        private readonly ServiceManager m_services = new();

        // Constants providing service locator naming conventions
        private const string k_GlobalServiceLocatorName = "Service Locator [Global]";
        private const string k_SceneServiceLocatorName = "Service Locator [Scene]";
        
        #endregion

        #region Configuration Methods
        
        /// <summary>
        /// Configures this ServiceLocator instance as the global singleton.
        /// </summary>
        /// <param name="dontDestroyOnLoad">If true, marks the GameObject to persist across scene loads.</param>
        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            // Warn if already configured as global
            if (s_global == this)
            {
                Debug.LogWarning("ServiceLocator.ConfigureAsGlobal: Already configured as global.", this);
            }
            // Error if another instance is already global
            else if (s_global != null)
            {
                Debug.LogError(
                    $"ServiceLocator.ConfigureAsGlobal: Another ServiceLocator is already configured as global. Global: {gameObject.name}",
                    this);
            }
            // Otherwise set as global service locator instance
            else
            {
                s_global = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Configures this ServiceLocator instance as the scene-scoped locator for its scene.
        /// Each scene can only have one scene-scoped ServiceLocator.
        /// </summary>
        internal void ConfigureForScene()
        {
            // Get scene associated with the scene-scoped ServiceLocator GameObject
            Scene scene = gameObject.scene;

            // Error if scene already has a ServiceLocator
            if (s_sceneContainers.ContainsKey(scene))
            {
                Debug.LogError($"ServiceLocator.ConfigureForScene: Scene {scene.name} already has a ServiceLocator.",
                    this);
                return;
            }

            // If scene does not already have a ServiceLocator, register it
            s_sceneContainers.Add(scene, this);
        }
        
        #endregion

        #region Service Getters and Registration Methods

        /// <summary>
        /// Gets the global ServiceLocator instance. Lazy-loads the global locator on first access.
        /// The global locator acts as the fallback for all service lookups.
        /// </summary>
        public static ServiceLocator Global
        {
            get
            {
                // Return cached global instance if it exists
                if (s_global != null) return s_global;

                // Try to find an existing global bootstrapper
                if (FindFirstObjectByType<ServiceLocatorGlobal>() is { } found)
                {
                    // If an existing bootstrapper is found, execute it and return the created global ServiceLocator
                    found.BootstrapOnDemand();
                    return s_global;
                }

                // Otherwise, create a new global service locator GameObject
                GameObject container = new(k_GlobalServiceLocatorName, typeof(ServiceLocator));
                // Add bootstrapper component and execute it to create the global ServiceLocator
                container.AddComponent<ServiceLocatorGlobal>().BootstrapOnDemand();

                return s_global;
            }
        }


        /// <summary>
        /// Gets the most appropriate ServiceLocator for the given MonoBehaviour.
        /// Searches in order: local (in hierarchy), scene-scoped, then global.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to find a ServiceLocator for.</param>
        /// <returns>The nearest ServiceLocator in the hierarchy, scene, or global fallback.</returns>
        public static ServiceLocator For(MonoBehaviour monoBehaviour)
        {
            // If no service locator is found in the hierarchy, check the scene. If it is still not found, then return the global instance.
            return monoBehaviour.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(monoBehaviour) ?? Global;
        }

        /// <summary>
        /// Gets the scene-scoped ServiceLocator for the scene containing the given MonoBehaviour.
        /// If no scene locator exists, returns the global locator.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour whose scene to search.</param>
        /// <returns>The scene-scoped ServiceLocator, or global if none exists.</returns>
        public static ServiceLocator ForSceneOf(MonoBehaviour monoBehaviour)
        {
            // Get the scene associated with the MonoBehaviour
            Scene scene = monoBehaviour.gameObject.scene;

            // 1. Check if the scene already has a registered container and that the container is not the object itself.
            if (s_sceneContainers.TryGetValue(scene, out ServiceLocator container) && container != monoBehaviour)
            {
                // If the container is registered and is not the object itself, return it.
                return container;
            }

            // 2. Search for ServiceLocatorScene bootstrapper in scene root GameObjects
            
            // Get a list of root GameObjects in the scene
            s_tmpSceneGameObjects.Clear();
            scene.GetRootGameObjects(s_tmpSceneGameObjects);

            // Iterate through root GameObjects and find ServiceLocatorScene bootstrappers
            foreach (GameObject gameObject in s_tmpSceneGameObjects
                         .Where(go => go.GetComponent<ServiceLocatorScene>() != null))
            {
                // If a ServiceLocatorScene bootstrapper is found, execute it and return the created ServiceLocator
                if (gameObject.TryGetComponent(out ServiceLocatorScene bootstrapper) &&
                    bootstrapper.Container != monoBehaviour)
                {
                    bootstrapper.BootstrapOnDemand();
                    return bootstrapper.Container;
                }
            }

            // Fall back to global if no scene locator found
            return Global;
        }

        /// <summary>
        /// Registers a service of type T with this ServiceLocator.
        /// </summary>
        /// <typeparam name="T">The type to register the service as.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <returns>This ServiceLocator for method chaining.</returns>
        public ServiceLocator Register<T>(T service)
        {
            m_services.Register(service);
            return this;
        }

        /// <summary>
        /// Registers a service with a specific type with this ServiceLocator.
        /// </summary>
        /// <param name="type">The type to register the service as.</param>
        /// <param name="service">The service instance to register.</param>
        /// <returns>This ServiceLocator for method chaining.</returns>
        public ServiceLocator Register(Type type, object service)
        {
            m_services.Register(type, service);
            return this;
        }

        /// <summary>
        /// Gets a service of type T from this ServiceLocator or parent locators.
        /// Searches up the hierarchy if the service is not found locally.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <param name="service">Output parameter that receives the service instance.</param>
        /// <returns>This ServiceLocator for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the service is not found in any locator.</exception>
        public ServiceLocator Get<T>(out T service) where T : class
        {
            // Try to get service from local service manager first
            if (TryGetService(out service)) return this;

            // If that is not found, search parent locators in the hierarchy
            if (TryGetNextInHierarchy(out ServiceLocator container))
            {
                container.Get(out service);
                return this;
            }
            
            // If service is still not found, throw an exception
            throw new ArgumentException($"ServiceLocator.Get: No service of type {typeof(T).FullName} registered.",
                nameof(T));
        }

        /// <summary>
        /// Gets a service of type T from this ServiceLocator or parent locators.
        /// Searches up the hierarchy if the service is not found locally.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The service instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the service is not found in any locator.</exception>
        public T Get<T>() where T : class
        {
            // Try to get service from local service manager first
            Type type = typeof(T);
            T service = null;

            if (TryGetService(type, out service)) return service;

            // If that is not found, search parent locators in the hierarchy
            if (TryGetNextInHierarchy(out ServiceLocator container))
            {
                return container.Get<T>();
            }
            
            // If service is still not found, throw an exception
            throw new ArgumentException($"Could not resolve type '{typeof(T).FullName}'.");
        }

        /// <summary>
        /// Attempts to retrieve a service of type T from the local service manager.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <param name="service">Output parameter that receives the service instance if found.</param>
        /// <returns>True if the service was found; otherwise, false.</returns>
        private bool TryGetService<T>(out T service) where T : class
        {
            return m_services.TryGet(out service);
        }

        /// <summary>
        /// Attempts to retrieve a service of a specific type from the local service manager.
        /// </summary>
        /// <typeparam name="T">The type to cast the service to.</typeparam>
        /// <param name="type">The type of service to retrieve (unused in current implementation).</param>
        /// <param name="service">Output parameter that receives the service instance if found.</param>
        /// <returns>True if the service was found; otherwise, false.</returns>
        private bool TryGetService<T>(Type type, out T service) where T : class
        {
            return m_services.TryGet(out service);
        }

        /// <summary>
        /// Attempts to find the next ServiceLocator in the hierarchy chain.
        /// Searches parent transforms, then scene-scoped locators.
        /// </summary>
        /// <param name="container">Output parameter that receives the next ServiceLocator if found.</param>
        /// <returns>True if a parent locator was found; false if this is the global locator.</returns>
        private bool TryGetNextInHierarchy(out ServiceLocator container)
        {
            // Check if this is the global locator
            if (this == s_global)
            {
                // If so, null the container and return false to indicate that no parent locator was found
                container = null;
                return false;
            }

            // Search parent hierarchy, then fall back to scene locator
            container = transform.parent.OrNull()?.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(this);
            return container != null;
        }
        
        #endregion

        /// <summary>
        /// Unity lifecycle method called when this GameObject is destroyed.
        /// Cleans up references in static containers to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            // Clear global reference if this is the global locator
            if (s_global == this)
            {
                s_global = null;
            }
            // Remove from scene containers if this is a scene locator
            else if (s_sceneContainers.ContainsValue(this)) s_sceneContainers.Remove(gameObject.scene);
        }

        /// <summary>
        /// Resets all static fields when entering Play mode or when domain reload occurs.
        /// This ensures clean state between play sessions in the Unity Editor.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_global = null;
            s_sceneContainers = new Dictionary<Scene, ServiceLocator>();
            s_tmpSceneGameObjects = new List<GameObject>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor menu item that creates a new global ServiceLocator in the scene.
        /// Accessible via GameObject > Service Locators > Add Global Service Locator.
        /// </summary>
        [MenuItem("GameObject/Service Locators/Add Global Service Locator")]
        private static void AddGlobal()
        {
            GameObject go = new(k_GlobalServiceLocatorName, typeof(ServiceLocatorGlobal));
        }

        /// <summary>
        /// Unity Editor menu item that creates a new scene-scoped ServiceLocator in the scene.
        /// Accessible via GameObject > Service Locators > Add Scene Service Locator.
        /// </summary>
        [MenuItem("GameObject/Service Locators/Add Scene Service Locator")]
        private static void AddScene()
        {
            GameObject go = new(k_SceneServiceLocatorName, typeof(ServiceLocatorScene));
        }
#endif
    }
}