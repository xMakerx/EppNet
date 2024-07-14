///////////////////////////////////////////////////////
/// Filename: ServiceManager.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace EppNet.Services
{

    public class ServiceManager : INodeDescendant
    {

        public NetworkNode Node { get; }

        /// <summary>
        /// Generic HashSet storage because services aren't going to be
        /// added and removed during the lifetime of any delegate app. <br/>Services
        /// are expected to be added when the NetworkNode is instantiated and
        /// when the application is about to close. No need for threading.
        /// </summary>
        protected HashSet<Service> _services;

        public ServiceManager(NetworkNode node)
        {
            this.Node = node;
            this._services = new();
        }

        public bool TryAddService<T>() where T : Service
        {
            T existing = GetService<T>();

            if (existing != null)
                return false;

            try
            {
                T created = (T)Activator.CreateInstance(typeof(T), Node);
                return _services.Add(created);
            } catch (Exception ex)
            {
                Node.HandleException(ex);
            }

            return false;
        }

        /// <summary>
        /// Tries to add the specified <see cref="Service"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns>Whether it was added or not</returns>

        [MemberNotNull]
        public bool TryAddService<T>(T service) where T : Service
        {
            if (service == null)
                return false;

            return _services.Add(service);
        }

        /// <summary>
        /// Fetches the specified <see cref="Service"/> by type<br/>
        /// Returns null if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if trying to fetch a Service of type <see cref="Service"/> rather than a derived type.</exception>

        public T GetService<T>() where T : Service
        {
            Type type = typeof(T);

            if (type == typeof(Service))
                throw new InvalidOperationException("Must specify a derived type of \"Service\"!");

            T result = null;
            foreach (Service service in _services)
            {
                if (service is T tService)
                {
                    result = tService;
                    break;
                }
            }

            return result;
        }

    }

}
