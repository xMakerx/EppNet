///////////////////////////////////////////////////////
/// Filename: IDataHolder.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Data
{

    /// <summary>
    /// Handy interface that allows external code to associate data with a
    /// specific instance
    /// </summary>

    public interface IDataHolder : IDisposable
    {
        /// <summary>
        /// Global data dictionary. I considered using something with weak references but those
        /// just hide poor memory management.
        /// <br></br>Just make sure you call dispose!
        /// </summary>
        internal static readonly Dictionary<IDataHolder, Dictionary<string, object>> _globalDataDict = new();

        /// <summary>
        /// Tries to get all data associated with a particular <see cref="IDataHolder"/><br></br>
        /// </summary>
        /// <param name="dataHolder"></param>
        /// <returns>Whether or not an internal dictionary was cleared<br>
        /// </br><b>NOTE:</b> Returns false if no custom data was set.</returns>

        public static bool DeleteAllData([NotNull] IDataHolder dataHolder)
        {
            if (dataHolder == null)
                return false;

            return _globalDataDict.Remove(dataHolder);
        }

        /// <summary>
        /// Tries to associate a value with a key<br/>
        /// Key mustn't be null or empty.<br></br>
        /// <b>NOTE:</b> Uses <see cref="GetOrCreateData"/>, so will only fail for invalid keys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Whether or not the key was set to the value</returns>
        /// <exception cref="ArgumentException">Key cannot be empty or null</exception>

        public bool Set(string key, object value)
        {
            var data = GetOrCreateData();

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be empty or null!");

            if (TryGetValue(key, out object existing) && existing == value)
                // Data is already set
                return false;

            data[key] = value;
            return true;
        }

        /// <summary>
        /// Fetches a value by the key without any checks<br/>
        /// Can result in an error if the key doesn't exist.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="KeyNotFoundException">The specified key could not be found</exception>
        /// <returns></returns>

        public object Get(string key) => GetOrCreateData()[key];

        /// <summary>
        /// Tries to remove the specified key from the internal data dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Whether or not it was deleted</returns>

        public bool Remove(string key) => GetOrCreateData().Remove(key);

        /// <summary>
        /// Tries to get an object by key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Whether or not the key was valid</returns>

        public bool TryGetValue(string key, out object value)
        {
            value = null;

            var data = GetOrCreateData();

            if (data == null)
                return false;

            return data.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets or creates an internal dictionary for data holding
        /// </summary>
        /// <returns>A dictionary</returns>

        public Dictionary<string, object> GetOrCreateData()
        {

            Dictionary<string, object> data;

            if (!_globalDataDict.TryGetValue(this, out data))
            {
                data = new(StringComparer.Ordinal);
                _globalDataDict.Add(this, data);
            }

            return data;
        }

    }

    public static class IDataHolderExtensions
    {

        /// <summary>
        /// Tries to associate a value with a key<br/>
        /// Key mustn't be null or empty.<br></br>
        /// <b>NOTE:</b> Uses <see cref="GetOrCreateData"/>, so will only fail for invalid keys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Whether or not the key was set to the value</returns>
        /// <exception cref="ArgumentException">Key cannot be empty or null</exception>
        public static bool Set<T>(this T holder, string key, object value) where T : IDataHolder => holder.Set(key, value);

        /// <summary>
        /// Fetches a value by the key without any checks<br/>
        /// Can result in an error if the key doesn't exist.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="KeyNotFoundException">The specified key could not be found</exception>
        /// <returns></returns>
        public static object Get<T>(this T holder, string key) where T : IDataHolder => holder.Get(key);

        /// <summary>
        /// Tries to remove the specified key from the internal data dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Whether or not it was deleted</returns>

        public static bool Remove<T>(this T holder, string key) where T : IDataHolder => holder.Remove(key);

        /// <summary>
        /// Tries to get an object by key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Whether or not the key was valid</returns>

        public static bool TryGetValue<T>(this T holder, string key, out object value) where T : IDataHolder => holder.TryGetValue(key, out value);

        /// <summary>
        /// Gets or creates an internal dictionary for data holding
        /// </summary>
        /// <returns>A dictionary</returns>

        public static Dictionary<string, object> GetOrCreateData<T>(this T holder) where T : IDataHolder => holder.GetOrCreateData();


    }

}
