///////////////////////////////////////////////////////
/// Filename: Register.cs
/// Date: September 13, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace EppNet.Registers
{

    public abstract class Register<TKey, BaseType> : IDisposable
    {

        protected internal IDictionary<TKey, IRegistrationBase> _lookupTable;

        public Register()
        {
            this._lookupTable = new Dictionary<TKey, IRegistrationBase>();
        }

        public bool Add<T>(TKey key) where T : BaseType
        {

            if (!_lookupTable.ContainsKey(key))
            {
                _lookupTable.Add(key, new Registration<T>());
                return true;
            }

            return false;
        }

        public IRegistrationBase Get(TKey key)
        {
            _lookupTable.TryGetValue(key, out IRegistrationBase registration);
            return registration;
        }

        public int CompileAll()
        {
            int compiled = 0;

            foreach (IRegistrationBase registration in _lookupTable.Values)
            {
                if (registration.Compile())
                    compiled++;
            }

            return compiled;
        }

        /// <summary>
        /// Disposes and unregisters all types.
        /// </summary>

        public void Dispose()
        {
            foreach (IRegistrationBase registration in _lookupTable.Values)
                registration.Dispose();

            _lookupTable.Clear();
        }

    }

}
