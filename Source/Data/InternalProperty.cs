///////////////////////////////////////////////////////
/// Filename: InternalProperty.cs
/// Date: February 16, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;

namespace EppNet.Data
{

    /// <summary>
    /// Wrapper of a property only settable by this library<br/>
    /// Created to get around interface restrictions
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class InternalProperty<T>
    {

        public T Value { private set; get; }

        public Action<T, T> OnChanged;

        public InternalProperty()
        {
            this.Value = default;
            this.OnChanged = null;
        }

        internal void Set(T newValue)
        {
            T current = Value;
            this.Value = newValue;

            OnChanged?.GlobalInvoke(newValue, current);
        }

    }

}
