//////////////////////////////////////////////
/// <summary>
/// Filename: Iterator.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
/// </summary>
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Collections
{

    public interface IIterator<out T>
    {
        /// <summary>
        /// Checks if we have something next in the iterator
        /// </summary>
        /// <returns></returns>
        public bool HasNext();

        /// <summary>
        /// Increments the internal index to the next one.
        /// </summary>
        /// <returns></returns>
        public T Next();

        /// <summary>
        /// Fetches the current object
        /// </summary>
        /// <returns></returns>
        public T Current();
    }

    public sealed class Iterator<T> : IIterator<T>
    {
        public int Index { get => _index; }

        private readonly IEnumerable<T> _enumerable;
        private List<T> _toIterate;

        private int _index;

        public Iterator([NotNull] IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            this._enumerable = enumerable;
            this._toIterate = new List<T>(_enumerable);
            this._index = -1;
        }

        /// <summary>
        /// Checks if we have something next in the iterator
        /// </summary>
        /// <returns></returns>
        public bool HasNext() => (_index + 1) < _toIterate.Count;

        /// <summary>
        /// Increments the internal index to the next one.
        /// </summary>
        /// <returns></returns>
        public T Next() => _toIterate[++_index];

        /// <summary>
        /// Fetches the current object
        /// </summary>
        /// <returns></returns>
        public T Current() => _index == -1 ? default : _toIterate[_index];
    }

    public static class IteratorExtensions
    {

        public static Iterator<T> Iterator<T>(this IEnumerable<T> enumerable) => new Iterator<T>(enumerable);

    }

}