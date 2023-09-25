///////////////////////////////////////////////////////
/// Filename: ObjectPool.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace EppNet.Objects
{

    public class ObjectPool<T> : IObjectPool where T : class, IPoolable
    {

        public static readonly int DefaultMaxCapacity = 200;
        public static readonly int DefaultCapacity = 100;

        protected Queue<T> _pool;
        
        public int MaxCapacity { private set; get; }
        public int Capacity { private set; get; }

        public ObjectPool() : this(DefaultCapacity, DefaultMaxCapacity) { }

        public ObjectPool(int capacity, int maxCapacity)
        {
            this.MaxCapacity = maxCapacity;
            this.Capacity = capacity;
            this._pool = new Queue<T>(capacity);
        }

        protected virtual T _MakeNew() => Activator.CreateInstance<T>();

        /// <summary>
        /// Sets the maximum capacity of the pool and deletes any extras that
        /// are above capacity.
        /// </summary>
        /// <param name="maxCapacity"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        public void SetMaxCapacity(int maxCapacity)
        {
            if (maxCapacity < 2)
                throw new ArgumentOutOfRangeException("[ObjectPool#SetMaxCapacity()] Must be at least 2!");

            this.MaxCapacity = maxCapacity;

            if (_pool.Count > MaxCapacity)
            {
                // Let's dequeue the extras over capacity.
                for (int i = 0; i < (_pool.Count - MaxCapacity); i++)
                    _pool.Dequeue();
            }
        }

        /// <summary>
        /// Sets the current capacity of the pool and updates the
        /// amount of objects within it to match it.
        /// </summary>
        /// <param name="capacity"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        public void SetCapacity(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException("[ObjectPool#SetCapacity()] Must be at least 1!");

            if (_pool.Count < capacity)
                // Fill up the queue to capacity
                for (int i = 0; i < (capacity - _pool.Count); i++)
                    _pool.Enqueue(_MakeNew());

            else if (_pool.Count > capacity)
                // Deque extras
                for (int i = 0; i < (_pool.Count - capacity); i++)
                    _pool.Dequeue();

        }

        /// <summary>
        /// Tries to return the <see cref="IPoolable"/> to the pool if:<br/>
        /// 1. The object isn't in the pool already; and,<br/>
        /// 2. The pool is below maximum capacity.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Whether or not the object was pooled.</returns>

        public bool TryReturnToPool(IPoolable obj)
        {
            if (_pool.Contains(obj as T))
                return false;

            if (_pool.Count < MaxCapacity)
            {
                if (obj.IsInitialized())
                    obj.Dispose();

                _pool.Enqueue(obj as T);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dequeues an object in the pool or creates a new one using <see cref="_MakeNew"/>
        /// </summary>
        /// <returns></returns>

        public IPoolable Get()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            return _MakeNew();
        }

    }

    public class UpdatePool : ObjectPool<Update>
    {

        public UpdatePool() : base() { }
        public UpdatePool(int capacity, int maxCapacity) : base(capacity, maxCapacity) { }

        protected override Update _MakeNew() => new();

    }

}
