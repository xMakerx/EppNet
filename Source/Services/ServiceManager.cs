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

        public NetworkNode Node { get => _node; }

        internal NetworkNode _node;

        /// <summary>
        /// Generic List storage because services aren't going to be
        /// added and removed during the lifetime of any delegate app. <br/>Services
        /// are expected to be added when the NetworkNode is instantiated and
        /// when the application is about to close. No need for threading.
        /// </summary>
        protected List<Service> _services;

        internal ServiceManager()
        {
            this._node = null;
            this._services = new();
        }

        public ServiceManager([NotNull] NetworkNode node) : this()
        {
            this._node = node;
        }

        /// <summary>
        /// Calls <see cref="Service.Start"/> on every service
        /// </summary>

        public void Start()
        {
            foreach (Service service in _services)
                service.Start();
        }
        
        /// <summary>
        /// Calls <see cref="Service.Update"/> on every service
        /// </summary>

        public void Tick()
        {
            foreach (Service service in _services)
                service.Update();
        }

        /// <summary>
        /// Calls <see cref="Service.Stop"/> on every service
        /// </summary>

        public void Stop()
        {
            foreach (Service service in _services)
                service.Stop();
        }

        public bool TryAddService<T>(out T created) where T : Service
        {
            created = null;
            T existing = GetService<T>();

            if (existing != null)
                return false;

            try
            {
                created = (T)Activator.CreateInstance(typeof(T), Node);
                _services.Add(created);
                return true;
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

        public bool TryAddService<T>([NotNull] T service) where T : Service
        {
            if (service == null)
                return false;

            if (service.Node != null)
                throw new InvalidOperationException("Service is already associated with a different NetworkNode!");

            _services.Add(service);
            return true;
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
            {
                InvalidOperationException exp = new("Must specify a derived type of \"Service\"!");

                if (Node == null)
                    throw exp;
                else
                    Node.HandleException(exp);

                return null;
            }

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
