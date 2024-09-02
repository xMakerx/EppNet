///////////////////////////////////////////////////////
/// Filename: SortedListExtensions.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Collections.Generic;

namespace EppNet.Utilities
{

    public static class SortedListExtensions
    {

        public static TKey GetKeyAtIndex<TKey, TValue>(this SortedList<TKey, TValue> list, int index)
        {
            if (index < 0 || index >= list.Count)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            return list.Keys[index];
        }

        public static TValue GetValueAtIndex<TKey, TValue>(this SortedList<TKey, TValue> list, int index)
        {
            if (index < 0 || index >= list.Count)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            return list.Values[index];
        }

    }

}