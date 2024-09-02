///////////////////////////////////////////////////////
/// Filename: LockQueue.cs
/// Date: September 27, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Data
{

    public class LockQueue<T> : Queue<T>
    {

        protected object _lock;

        public LockQueue() : base()
        {
            _lock = new object();
        }

        public LockQueue(int capacity) : base(capacity)
        {
            _lock = new object();
        }

        public LockQueue(IEnumerable<T> items) : base(items)
        {
            _lock = new object();
        }

        public new void Enqueue(T item)
        {
            lock (_lock)
                base.Enqueue(item);
        }

        public virtual bool TryEnqueue(T item)
        {
            lock (_lock)
            {
                bool contains = base.Contains(item);

                if (!contains)
                    base.Enqueue(item);

                return !contains;
            }
        }

        public new T Dequeue()
        {
            lock (_lock)
                return base.Dequeue();
        }

        public new bool TryDequeue([MaybeNullWhen(false)] out T item)
        {
            lock (_lock)
                return base.TryDequeue(out item);
        }

        public new T Peek()
        {
            lock (_lock)
                return base.Peek();
        }

        public new bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            lock (_lock)
                return base.TryPeek(out result);
        }

        public new bool Contains(T item)
        {
            lock (_lock)
                return base.Contains(item);
        }

        public new T[] ToArray()
        {
            lock (_lock)
                return base.ToArray();
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
                base.CopyTo(array, arrayIndex);
        }

        public new void Clear()
        {
            lock (_lock)
                base.Clear();
        }

        public virtual List<T> FlushQueue()
        {
            lock (_lock)
            {
                List<T> list = new List<T>();

                for (int i = 0; i < Count; i++)
                    list.Add(base.Dequeue());

                return list;
            }
        }

    }

}
