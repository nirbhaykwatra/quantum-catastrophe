using System;
using System.Collections.Generic;
using UnityEngine;

namespace QC.Utilities.ServiceLocation
{
    /// <summary>
    /// Manages registration and retrieval of services using a type-based dictionary.
    /// Provides a simple dependency injection container for game services.
    /// </summary>
    public class ServiceManager
    {
        /// <summary>
        /// Dictionary mapping service types to their instances.
        /// </summary>
        private readonly Dictionary<Type, object> m_services = new();

        /// <summary>
        /// Gets all registered service instances.
        /// </summary>
        public IEnumerable<object> RegisteredServices => m_services.Values;

        /// <summary>
        /// Attempts to retrieve a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <param name="service">The retrieved service instance, or null if not found.</param>
        /// <returns>True if the service was found, false otherwise.</returns>
        public bool TryGet<T>(out T service) where T : class
        {
            // Attempt to retrieve the service from the dictionary
            
            // Cache type for performance
            Type type = typeof(T);
            
            // Check if the service exists
            if (m_services.TryGetValue(type, out object obj))
            {
                // Cast the retrieved object to the requested type and return
                service = obj as T;
                return true;
            }
            
            // If the service was not found, return false
            service = null;
            return false;
        }
        
        /// <summary>
        /// Retrieves a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The service instance.</returns>
        /// <exception cref="ArgumentException">Thrown when no service of the specified type is registered.</exception>
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            
            // Attempt to retrieve the service from the dictionary
            if (m_services.TryGetValue(type, out object service))
            {
                // If found, cast the retrieved object to the requested type and return
                return service as T;
            }
            
            // If the service was not found, throw an exception
            throw new ArgumentException($"ServiceManager.Get: No service of type {type.FullName} registered.", nameof(T));
        } 
        
        /// <summary>
        /// Registers a service instance with its type.
        /// </summary>
        /// <typeparam name="T">The type to register the service as.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <returns>This ServiceManager instance for method chaining.</returns>
        public ServiceManager Register<T>(T service)
        {
            Type type = typeof(T);

            // Attempt to add the service, log error if it already exists
            if (!m_services.TryAdd(type, service))
            {
                Debug.LogError($"ServiceManager.Register: Failed to register service of type {type.FullName}. Service already exists.");
            }
            
            // Return this instance for method chaining
            return this;
        }

        /// <summary>
        /// Registers a service instance with a specified type.
        /// Useful when you need to register a service as a base type or interface.
        /// </summary>
        /// <param name="type">The type to register the service as.</param>
        /// <param name="service">The service instance to register.</param>
        /// <returns>This ServiceManager instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the service instance is not of the specified type.</exception>
        public ServiceManager Register(Type type, object service)
        {
            // Validate that the service is actually an instance of the specified type
            if (!type.IsInstanceOfType(service))
            {
                throw new ArgumentException($"Type of service does not match type of service interface", nameof(service));
            }

            // Attempt to add the service, log error if it already exists
            if (!m_services.TryAdd(type, service))
            {
                Debug.LogError($"ServiceManager.Register: Failed to register service of type {type.FullName}. Service already exists.");
            }
            
            // Return this instance for method chaining
            return this;
        }
    }
}