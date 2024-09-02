///////////////////////////////////////////////////////
/// Filename: Register.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

namespace EppNet.Registers
{

    public abstract class Register<TKey, BaseType> : ICompilable, IDisposable
        where TKey : INumber<TKey>, IEquatable<TKey>
    {

        /// <summary>
        /// The total number of registrations
        /// </summary>
        public int Registrations { get => _lookupTable.Count; }

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

        public bool IsValidKey(TKey key)
            => !_lookupTable.ContainsKey(key);

        /// <summary>
        /// Registers the specified type with a key equal to the next available index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public virtual bool TryRegister<T>() where T : BaseType
        {

            // Ensure it isn't already registered
            if (_type2Keys.ContainsKey(typeof(T)))
                return false;

            TKey key;
            int offset = 0;

            do
            {
                key = TKey.CreateChecked(_lookupTable.Count + offset++);
            } while (!IsValidKey(key));

            return _Internal_TryRegister<T>(key);
        }

        public virtual bool TryGetNew<T>(out T instance) where T : BaseType
        {
            instance = default;
            IRegistration registration = Get(typeof(T));

            if (registration != null)
            {
                instance = (T) registration.NewInstance();
                return true;
            }

            return false;
        }

        public virtual IRegistration Get(TKey key)
        {
            _lookupTable.TryGetValue(key, out IRegistration registration);
            return registration;
        }

        public virtual IRegistration Get<T>() where T : BaseType
            => Get(typeof(T));

        public virtual IRegistration Get(Type type)
        {
            if (!_type2Keys.TryGetValue(type, out TKey key))
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

        public bool IsCompiled()
            => _compiled;

        /// <summary>
        /// Disposes and unregisters all types.
        /// </summary>

        public virtual void Dispose()
        {
            foreach (IRegistration registration in _lookupTable.Values)
                registration.Dispose();

            _lookupTable.Clear();
        }

        protected virtual bool _Internal_TryRegister<T>(TKey key) where T : BaseType
        {
            Type type = typeof(T);

            if (!IsValidKey(key))
                throw new ArgumentOutOfRangeException($"Failed to register Type {type.Name} because provided key {key} is unavailable!");

            Registration r = new Registration(type);
            _lookupTable.Add(key, r);
            _type2Keys.Add(type, key);
            return true;
        }

        protected virtual bool _Internal_TryRegister(TKey key, IRegistration r)
        {
            Type regType = r.GetRegisteredType();

            if (!typeof(BaseType).IsAssignableFrom(regType))
                throw new ArgumentException($"Type {regType} is not supported.");

            if (!IsValidKey(key))
                throw new ArgumentOutOfRangeException($"Failed to register Type {regType.Name} because provided key {key} is unavailable!");

            _lookupTable.Add(key, r);
            _type2Keys.Add(regType, key);
            return true;
        }

    }

}
