/////////////////////////////////////////////
/// Filename: DictionaryExtensions.cs
/// Date: September 26, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EppNet.Utilities
{

    public static class DictionaryExtensions
    {

        /// <summary>
        /// Executes the specified <see cref="Action"/> on the given dictionary if the
        /// key exists. Better than vanilla implementation because it requires a single lookup rather than
        /// two or potentially more.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <returns>Whether or not the key existed (implies action execution)</returns>
        /// <remarks>Dictionary modification of any kind will invalidate the reference used for the value.
        /// <br/>Leave modifications for the END of the action.</remarks>

        public static bool ExecuteIfExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Action<TKey, TValue> action)
        {

            ref TValue valOrNull = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);

            if (!Unsafe.IsNullRef(ref valOrNull))
            {
                action?.Invoke(key, valOrNull);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary if it hasn't been added already. Better than 
        /// vanilla implementation because it requires a single lookup rather than 
        /// two or potentially more.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Whether or not the entry was added</returns>

        public static bool TryAddEntry<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            ref TValue valOrNew = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out bool exists);

            if (!exists)
                valOrNew = value;

            return !exists;
        }

    }

}
