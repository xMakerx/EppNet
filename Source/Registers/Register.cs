///////////////////////////////////////////////////////
/// Filename: Register.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace EppNet.Registers
{

    public abstract class Register<TKey, BaseType> : ICompilable, IDisposable
    {

        protected internal IDictionary<TKey, IRegistration> _lookupTable;
        protected internal Dictionary<Type, TKey> _type2Keys;
        protected bool _compiled;

        public Register()
        {
            this._lookupTable = new Dictionary<TKey, IRegistration>();
            this._type2Keys = new Dictionary<Type, TKey>();
            this._compiled = false;
        }

        /// <summary>
        /// Checks if the given key can be added to the internal lookup table.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        public virtual bool IsValidKey(TKey key) => !_lookupTable.ContainsKey(key);

        public bool Add<T>(TKey key) where T : BaseType => Add(key, typeof(T));

        public bool Add(TKey key, Type type)
        {
            if (!typeof(BaseType).IsAssignableFrom(type))
                throw new ArgumentException($"Type {type.Name} is not supported.");

            if (IsValidKey(key))
            {
                Registration r = new Registration(type);
                _lookupTable.Add(key, r);
                _type2Keys.Add(type, key);
                return true;
            }

            return false;
        }

        public bool Add(TKey key, Registration r)
        {
            if (!typeof(BaseType).IsAssignableFrom(r.Type))
                throw new ArgumentException($"Type {r.Type} is not supported.");

            if (IsValidKey(key))
            {
                _lookupTable.Add(key, r);
                _type2Keys.Add(r.Type, key);
                return true;
            }

            return false;
        }

        public virtual IRegistration Get(TKey key)
        {
            _lookupTable.TryGetValue(key, out IRegistration registration);
            return registration;
        }

        public virtual IRegistration Get(Type type)
        {
            _type2Keys.TryGetValue(type, out TKey key);

            if (key == null)
                return null;

            return _lookupTable[key];
        }

        public virtual bool Compile()
        {
            if (_compiled)
                return false;

            foreach (IRegistration registration in _lookupTable.Values)
                registration.Compile();

            _compiled = true;
            return _compiled;
        }

        public bool IsCompiled() => _compiled;

        /// <summary>
        /// Disposes and unregisters all types.
        /// </summary>

        public virtual void Dispose()
        {
            foreach (IRegistration registration in _lookupTable.Values)
                registration.Dispose();

            _lookupTable.Clear();
        }

    }

}
