///////////////////////////////////////////////////////
/// Filename: Iterator.cs
/// Date: September 10, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Collections
{

    public ref struct Iterator<T>
    {

        public int Index { private set; get; }

        private ImmutableArray<T> _elements;

        public Iterator([NotNull] IEnumerable<T> collection)
        {
            Guard.AgainstNull(collection);
            this.Index = -1;
            this._elements = collection.ToImmutableArray();
        }

        /// <summary>
        /// Increments the internal index to the next one.
        /// </summary>
        /// <returns></returns>

        public T Next()
            => _elements[++Index];

        /// <summary>
        /// Checks if we have something next in the iterator
        /// </summary>
        /// <returns></returns>

        public bool HasNext()
            => (Index + 1) < _elements.Length;

        /// <summary>
        /// Fetches the current object
        /// </summary>
        /// <returns></returns>

        public T Current()
            => (Index == -1)
            ? default
            : _elements[Index];
    }

    public static class IteratorExtensions
    {

        public static Iterator<T> Iterator<T>(this IEnumerable<T> enumerable)
            => new Iterator<T>(enumerable);

    }

}