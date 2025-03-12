/////////////////////////////////////////////
/// Filename: EquatableSet.cs
/// Date: March 12, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace EppNet.SourceGen.Models
{

    public class EquatableHashSet<T> : HashSet<T>, IEquatable<EquatableHashSet<T>>
    {

        public EquatableHashSet() : base() { }

        public EquatableHashSet(HashSet<T> source) : base(source) { }

        public bool Equals(EquatableHashSet<T> other)
        {
            if (other is null || Count != other.Count)
                return false;

            return SetEquals(other);
        }

        public override bool Equals(object obj) =>
            obj is not null &&
            obj is EquatableHashSet<T> other &&
            Equals(other);

        public override int GetHashCode() =>
            this.Count == 0 ? 0 :
            this.Select(item => item?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);

        public static bool operator ==(EquatableHashSet<T> set1, EquatableHashSet<T> set2) =>
            ReferenceEquals(set1, set2) ||
            set1 is not null &&
            set2 is not null &&
            set1.Equals(set2);

        public static bool operator !=(EquatableHashSet<T> set1, EquatableHashSet<T> set2) =>
            !(set1 == set2);
    }

}
