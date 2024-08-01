///////////////////////////////////////////////////////
/// Filename: PageList.cs
/// Date: July 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;


namespace EppNet.Collections
{

    public class PageList<T> where T : IPageable, new()
    {
        public readonly int ItemsPerPage;

        protected readonly float _itemIndexToPageIndexMult;
        protected internal List<Page<T>> _pages;

        internal int _pageIndexWithAvaliability;
        private ReaderWriterLockSlim _lock;

        public PageList(int itemsPerPage)
        {
            this.ItemsPerPage = itemsPerPage;
            this._itemIndexToPageIndexMult = 1f / itemsPerPage;
            this._pages = new List<Page<T>>();
            this._pageIndexWithAvaliability = -1;
            this._lock = new();
        }

        public void DoOnActive(Action<T> action)
        {
            try
            {
                _lock.EnterReadLock();

                for (int i = 0; i < _pages.Count; i++)
                    _pages[i].DoOnActive(action);

            }
            finally { _lock.ExitReadLock(); }
        }

        public void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                for (int i = 0; i < _pages.Count; i++)
                {
                    Page<T> page = _pages[i];
                    page.ClearAll();
                }

                _pages.Clear();
            }
            finally { _lock.ExitWriteLock(); }
        }

        public int PurgeEmptyPages()
        {
            try
            {
                _lock.EnterWriteLock();
                Iterator<Page<T>> iterator = _pages.Iterator();
                int purged = 0;

                while (iterator.HasNext())
                {
                    Page<T> page = iterator.Next();

                    if (page.IsEmpty())
                    {
                        _pages.Remove(page);
                        purged++;
                    }
                }

                return purged;
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool TryAllocate(in long id, out T allocated)
        {
            int pageIndex = ItemIdToPageIndex(id);

            try
            {
                // We might have to allocate a page
                _lock.EnterUpgradeableReadLock();

                if ((pageIndex + 1) > _pages.Count)
                {

                    try
                    {
                        // We have to write data
                        _lock.EnterWriteLock();

                        // We don't have a page for this
                        int pagesToAllocate = (pageIndex + 1) - _pages.Count;

                        for (int i = 0; i < pagesToAllocate; i++)
                        {
                            Page<T> newPage = new(this, _pages.Count + i);
                            _pages.Add(newPage);
                        }
                    }
                    finally { _lock.ExitWriteLock(); }

                }

                // Fetch the appropriate page
                Page<T> page = _pages[pageIndex];
                return page.TryAllocate(id, out allocated);
            }
            finally { _lock.ExitUpgradeableReadLock();  }

        }

        public bool TryAllocate(out T allocated)
        {
            Page<T> page;

            try
            {
                // We might have to allocate a page.
                _lock.EnterUpgradeableReadLock();

                if (_pageIndexWithAvaliability != -1)
                    // We know that this page has availability
                    page = _pages[_pageIndexWithAvaliability];
                else
                {
                    try
                    {
                        _lock.EnterWriteLock();

                        // We don't have a page with availability
                        // Allocate one.
                        page = new Page<T>(this, _pages.Count);
                        _pages.Add(page);

                    }
                    finally { _lock.ExitWriteLock(); }

                }

                return page.TryAllocate(out allocated);
            }
            finally {  _lock.ExitUpgradeableReadLock(); }

        }

        public bool TryFree(T toFree)
        {
            Page<T> page = toFree.Page as Page<T>;
            return page.TryFree(toFree);
        }

        public bool IsAvailable(long id)
        {
            try
            {
                // This is a read-only operation
                _lock.EnterReadLock();

                int pageIndex = (int)Math.Floor(id * _itemIndexToPageIndexMult);

                if (pageIndex >= _pages.Count)
                    return true;

                Page<T> page = _pages[pageIndex];
                return page[Convert.ToInt32(id - page.StartIndex)].IsFree();
            }
            finally { _lock.ExitReadLock(); }
        }

        public bool TryGetById(long id, out T item)
        {
            try
            {
                _lock.EnterReadLock();
                int pageIndex = (int)Math.Floor(id * _itemIndexToPageIndexMult);

                if (pageIndex > _pages.Count)
                {
                    item = default;
                    return false;
                }

                Page<T> page = _pages[pageIndex];
                item = page[ItemIdToPageIndex(page, id)];
                return true;

            }
            finally { _lock.ExitReadLock(); }
        }

        public T Get(long id)
        {
            TryGetById(id, out T item);
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ItemIdToPageIndex(Page<T> page, long id)
        {
            long delta = id - page.StartIndex;
            return Convert.ToInt32(delta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ItemIdToPageIndex(long id) => (int)Math.Floor(id * _itemIndexToPageIndexMult);
    }

    public interface IPage
    {
        public int GetIndex();
    }

    public interface IPageable : IDisposable
    {
        public IPage Page { internal set; get; }
        public long ID { set; get; }

        public bool IsFree();
    }

    public class Page<T> : IPage where T : IPageable, new()
    {

        internal const int _primSize = 64;
        internal const float _multiplier = 1f / _primSize;

        public readonly PageList<T> List;
        public readonly int Index;
        public readonly int Size;

        public bool Empty { private set; get; }

        public readonly int StartIndex;

        /// <summary>
        /// Available index is relative to [0 -> <see cref="Size"/> - 1]
        /// </summary>
        public int AvailableIndex;

        public bool HasFree => AvailableIndex != -1;

        public Action<T> OnAllocate;
        public Action<T> OnFree;

        private readonly T[] _data;
        private readonly ulong[] _allocated;
        private readonly ReaderWriterLockSlim _lock;
        public T this[int index] => _data[index];

        internal Page([NotNull] PageList<T> list, int index)
        {
            Guard.AgainstNull(list);
            this.List = list;
            this.Index = index;
            this.Size = list.ItemsPerPage;
            this.StartIndex = Index * List.ItemsPerPage;
            this.AvailableIndex = 0;

            if (Size % _primSize != 0)
                throw new ArgumentOutOfRangeException("Size must be a multiple of 64!");

            this._data = new T[Size];

            for (int i = 0; i < Size; i++)
            {
                T item = new();
                item.Page = this;
                item.ID = StartIndex + i;
                _data[i] = item;
            }

            this._allocated = new ulong[Size / _primSize];
            this._lock = new();
        }

        public void DoOnActive(Action<T> action)
        {
            Guard.AgainstNull(action);
            try
            {
                _lock.EnterReadLock();

                if (Empty)
                    return;

                for (int i = 0; i < _allocated.Length; i++)
                {
                    ulong marker = _allocated[i];

                    // Nothing to iterate
                    if (marker == 0UL)
                        continue;

                    for (int bit = _primSize - 1; bit > -1; bit--)
                    {
                        if ((marker & (1UL << bit)) == 0)
                            continue;

                        // This bit is active
                        int normalized = _primSize - bit;
                        int index = StartIndex + (i * _primSize) + normalized;
                        T item = this[index];
                        action.Invoke(item);
                    }
                }

            } finally { _lock.ExitReadLock(); }
        }

        public void ClearAll()
        {
            for (int i = 0; i < Size; i++)
                TryFree(this[i]);
        }

        public bool TryAllocate(long id, out T allocated)
        {
            int index = (int) id - StartIndex;
            allocated = default;

            if (index >= Size || index < 0)
                return false;

            bool available = this[index].IsFree();

            if (available)
                _Internal_DoAllocate(index, out allocated);

            return available;
        }

        public bool TryAllocate(out T allocated)
        {

            if (AvailableIndex == -1)
            {
                allocated = default;
                return false;
            }

            _Internal_DoAllocate(AvailableIndex, out allocated);
            return true;
        }

        public bool TryFree(T item)
        {
            if (!item.IsFree())
            {
                int index = Convert.ToInt32(item.ID - StartIndex);

                item.Dispose();
                _Internal_UpdateBit(index, false);

                OnFree?.Invoke(item);

                // Check if empty
                IsEmpty();

                if (AvailableIndex == -1 || index < AvailableIndex)
                    Interlocked.Exchange(ref AvailableIndex, index);

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            try
            {
                _lock.EnterReadLock();

                for (int i = 0; i < _allocated.Length; i++)
                {
                    if (_allocated[i] != 0UL)
                    {
                        Empty = false;
                        return Empty;
                    }
                }

                Empty = true;
                return Empty;

            }
            finally { _lock.ExitReadLock(); }
        }

        public int GetIndex() => Index;

        public string GetFreeString()
        {
            StringBuilder builder = new();

            for (int l = 0; l < _allocated.Length; l++)
            {
                ulong marker = _allocated[l];

                for (int i = _primSize - 1; i > -1; i--)
                {
                    if ((marker & (1UL << i)) != 0)
                        builder.Append('1');
                    else
                        builder.Append('0');
                }

                if (l + 1 < _allocated.Length)
                    builder.AppendLine();
            }

            return builder.ToString();
        }

        protected void _Internal_CalculateNextFree()
        {
            try
            {
                _lock.EnterWriteLock();

                AvailableIndex = -1;
                int firstFree = -1;

                for (int i = 0; i < _allocated.Length; i++)
                {
                    ulong marker = _allocated[i];

                    if (marker == ulong.MaxValue)
                        continue;

                    #if NET7_0_OR_GREATER
                        int zeros = System.Numerics.BitOperations.LeadingZeroCount(marker);

                        if (zeros != 0)
                        {
                            firstFree = (i * _primSize) + (_primSize - zeros);
                            break;
                        }
                    #else
                        for (int bit = _primSize - 1; bit > -1; bit--)
                        {
                            if (((int) marker & (1UL << bit)) == 0)
                            {
                                firstFree = (i * _primSize) + (_primSize - bit);
                                break;
                            }
                        }
                    #endif
                }

                if (firstFree != -1)
                {
                    Interlocked.Exchange(ref AvailableIndex, firstFree);
                    if (List._pageIndexWithAvaliability == -1 || List._pageIndexWithAvaliability > Index)
                        Interlocked.Exchange(ref List._pageIndexWithAvaliability, Index);
                }

            } finally { _lock.ExitWriteLock(); }

        }

        protected void _Internal_DoAllocate(int index, out T allocated)
        {
            _Internal_UpdateBit(index, true);

            allocated = this[index];
            OnAllocate?.Invoke(allocated);

            // We just completed an allocation, we are not empty.
            Empty = false;

            _Internal_CalculateNextFree();
        }

        protected void _Internal_UpdateBit(int index, bool on)
        {
            try
            {
                _lock.EnterWriteLock();
                int longIndex = (int)Math.Floor(index * _multiplier);
                int bitIndex = index % _primSize;

                if (on)
                    _allocated[longIndex] |= (1UL << bitIndex);
                else
                    _allocated[longIndex] &= ~(1UL << bitIndex);

            } finally { _lock.ExitWriteLock(); }

        }

    }

}
