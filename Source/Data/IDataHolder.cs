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

        private static readonly Dictionary<IDataHolder, Dictionary<string, object>> _data = new();

        /// <summary>
        /// Tries to get all data associated with a particular <see cref="IDataHolder"/>
        /// </summary>
        /// <param name="dataHolder"></param>
        /// <returns>Whether or not the internal dictionary was cleared</returns>

        public static bool DeleteAllData([NotNull] IDataHolder dataHolder)
        {
            if (dataHolder == null)
                return false;

            return _data.Remove(dataHolder);
        }

        bool Set(string key, object value)
        {
            var data = GetData();

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

        object Get(string key) => GetData()[key];

        bool Remove(string key) => GetData().Remove(key);

        bool TryGetValue(string key, out object value)
        {
            value = null;

            Dictionary<string, object> data = GetData();

            if (data == null)
                return false;

            return data.TryGetValue(key, out value);
        }

        Dictionary<string, object> GetData()
        {

            Dictionary<string, object> data;

            if (!_data.TryGetValue(this, out data))
            {
                data = new(StringComparer.Ordinal);
                _data.Add(this, data);
            }

            return data;
        }

        new void Dispose() => DeleteAllData(this);
    }

}
