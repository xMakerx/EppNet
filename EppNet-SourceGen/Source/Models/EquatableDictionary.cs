/////////////////////////////////////////////
/// Filename: EquatableDictionary.cs
/// Date: March 11, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace EppNet.SourceGen.Models
{

    public class EquatableDictionary<TKey, TValue> : Dictionary<TKey, TValue>,
        IEquatable<EquatableDictionary<TKey, TValue>>
    {
        public EquatableDictionary() : base() { }

        public EquatableDictionary(Dictionary<TKey, TValue> source) : base(source) { }

        public bool Equals(EquatableDictionary<TKey, TValue> other)
        {
            if (other is null || Count != other.Count)
                return false;

            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                if (!other.TryGetValue(kvp.Key, out TValue otherValue))
                    return false;

                if (!EqualityComparer<TValue>.Default.Equals(kvp.Value, otherValue))
                    return false;
            }

            // Lengths and key-value pairs are identical
            return true;
        }

        public override bool Equals(object obj) =>
            Equals(obj as EquatableDictionary<TKey, TValue>);

        public override int GetHashCode() =>
            this.Count == 0 ? 0 :
            this.Select(item => item.GetHashCode())
            .Aggregate((x, y) => x ^ y);

        public static bool operator ==(EquatableDictionary<TKey, TValue> a, EquatableDictionary<TKey, TValue> b) =>
            ReferenceEquals(a, b) || a is not null && b is not null && a.Equals(b);

        public static bool operator !=(EquatableDictionary<TKey, TValue> a, EquatableDictionary<TKey, TValue> b) =>
            !(a == b);
    }

}
