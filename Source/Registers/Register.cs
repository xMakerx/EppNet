///////////////////////////////////////////////////////
/// Filename: Register.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

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

        public virtual bool Add(TKey key, IRegistration r)
        {
            Type regType = r.GetRegisteredType();

            if (!typeof(BaseType).IsAssignableFrom(regType))
                throw new ArgumentException($"Type {regType} is not supported.");

            if (IsValidKey(key))
            {
                _lookupTable.Add(key, r);
                _type2Keys.Add(regType, key);
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

        public virtual TKey GetKeyFromValue(IRegistration registration)
        {

            foreach (KeyValuePair<TKey, IRegistration> pair in _lookupTable)
            {
                if (ReferenceEquals(pair.Value, registration))
                    return pair.Key;
            }

            return default;
        }

        public virtual CompilationResult Compile()
        {

            if (_compiled)
                return new();

            int compiledCount = 0;

            try
            {
                foreach (IRegistration registration in _lookupTable.Values)
                {
                    registration.Compile();
                    compiledCount++;
                }

                _compiled = true;
            }
            catch (Exception ex)
            {
                return new(false, compiledCount, ex);
            }

            return new(_compiled, compiledCount, null);
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
