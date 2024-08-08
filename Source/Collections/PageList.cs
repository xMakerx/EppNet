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

    public class PageList<T> : IDisposable where T : Pageable, new()
    {
        public readonly int ItemsPerPage;
        public Action<T> OnAllocate;
        public Action<T> OnFree;

        public readonly float ItemIndexToPageIndexMult;
        public List<Page<T>> Pages { protected set; get; }

        public int PageIndexWithAvailability { get => _pageIndexWithAvailability; }

        internal int _pageIndexWithAvailability;
        private ReaderWriterLockSlim _lock;

        public PageList(int itemsPerPage)
        {
            this.ItemsPerPage = itemsPerPage;
            this.ItemIndexToPageIndexMult = 1f / itemsPerPage;
            this.Pages = new List<Page<T>>();
            this._pageIndexWithAvailability = -1;
            this._lock = new();
        }

        public void DoOnActive(Action<T> action)
        {
            try
            {
                _lock.EnterReadLock();

                for (int i = 0; i < Pages.Count; i++)
                    Pages[i].DoOnActive(action);

            }
            finally { _lock.ExitReadLock(); }
        }

        public void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                for (int i = 0; i < Pages.Count; i++)
                {
                    Page<T> page = Pages[i];
                    page.ClearAll();
                }

                Pages.Clear();
            }
            finally { _lock.ExitWriteLock(); }
        }

        public int PurgeEmptyPages()
        {
            try
            {
                _lock.EnterWriteLock();
                Iterator<Page<T>> iterator = Pages.Iterator();
                int purged = 0;

                while (iterator.HasNext())
                {
                    Page<T> page = iterator.Next();

                    if (page.IsEmpty())
                    {
                        Pages.Remove(page);
                        purged++;
                    }
                }

                return purged;
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool TryAllocate(in long id, out T allocated) => TryAllocate((ulong)id, out allocated);

        public bool TryAllocate(in ulong id, out T allocated)
        {
            int pageIndex = (int) (id * ItemIndexToPageIndexMult);

            try
            {
                // We might have to allocate a page
                _lock.EnterUpgradeableReadLock();

                if ((pageIndex + 1) > Pages.Count)
                {

                    try
                    {
                        // We have to write data
                        _lock.EnterWriteLock();

                        // We don't have a page for this
                        int pagesToAllocate = (pageIndex + 1) - Pages.Count;

                        for (int i = 0; i < pagesToAllocate; i++)
                        {
                            Page<T> newPage = new(this, Pages.Count + i);
                            Pages.Add(newPage);
                        }
                    }
                    finally { _lock.ExitWriteLock(); }

                }

                // Fetch the appropriate page
                Page<T> page = Pages[pageIndex];
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

                if (PageIndexWithAvailability != -1)
                    // We know that this page has availability
                    page = Pages[PageIndexWithAvailability];
                else
                {
                    try
                    {
                        _lock.EnterWriteLock();

                        // We don't have a page with availability
                        // Allocate one.
                        page = new Page<T>(this, Pages.Count);
                        Pages.Add(page);

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

        public bool TryFree(ulong id)
        {
            if (TryGetById(id, out T item))
                return TryFree(item);

            return false;
        }

        public bool TryFree(long id) => TryFree((ulong)id);

        public bool IsAvailable(long id) => IsAvailable((ulong)id);

        public bool IsAvailable(ulong id)
        {
            try
            {
                // This is a read-only operation
                _lock.EnterReadLock();

                int pageIndex = (int) (id * ItemIndexToPageIndexMult);

                if (pageIndex >= Pages.Count)
                    return true;

                Page<T> page = Pages[pageIndex];
                return page[(int) (id - (ulong) page.StartIndex)].IsFree();
            }
            finally { _lock.ExitReadLock(); }
        }

        public bool TryGetById(long id, out T item) => TryGetById((ulong)id, out item);

        public bool TryGetById(ulong id, out T item)
        {
            try
            {
                _lock.EnterReadLock();
                int pageIndex = (int) (id * ItemIndexToPageIndexMult);

                if (pageIndex >= Pages.Count)
                {
                    item = default;
                    return false;
                }

                Page<T> page = Pages[pageIndex];
                item = page[(int) (id - ((ulong) page.StartIndex))];
                return true;

            }
            finally { _lock.ExitReadLock(); }
        }

        public T Get(int id) => Get((ulong)id);

        public T Get(ulong id)
        {
            TryGetById(id, out T item);
            return item;
        }

        public void Dispose()
        {
            Clear();
            _lock.Dispose();

            OnAllocate = null;
            OnFree = null;
        }
    }

    public interface IPage
    {
        public int GetIndex();
    }

    public interface IPageable : IDisposable
    {
        public bool IsFree();
        public IPage GetPage();
    }

    public abstract class Pageable : IPageable
    {

        public IPage Page { internal set; get; }
        public long ID { internal set; get; }
        public bool Allocated { internal set; get; }

        public virtual void Dispose()
        {
            this.Allocated = false;
        }

        public bool IsFree() => !Allocated;
        public IPage GetPage() => Page;

    }

    public class Page<T> : IPage where T : Pageable, new()
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
                _data[i] = new()
                {
                    Page = this,
                    ID = StartIndex + i
                };
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

                    for (int bit = 0; bit < _primSize; bit++)
                    {
                        if ((marker & (1UL << bit)) == 0)
                            continue;

                        // This bit is active
                        int index = StartIndex + (i * _primSize) + bit;
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

        public bool TryAllocate(long id, out T allocated) => TryAllocate((ulong)id, out allocated);

        public bool TryAllocate(ulong id, out T allocated)
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

            if (item.IsFree())
                return false;

            int index = (int)item.ID;

            item.Allocated = false;
            item.Dispose();
            _Internal_UpdateBit(index, false);
            List.OnFree?.Invoke(item);

            // Check if empty
            IsEmpty();

            if (AvailableIndex == -1 || index < AvailableIndex)
                Interlocked.Exchange(ref AvailableIndex, index);

            return true;
        }

        public bool TryFree(int id) => TryFree((ulong)id);
        public bool TryFree(long id) => TryFree((ulong)id);

        public bool TryFree(ulong id)
        {
            int index = (int)(id - (ulong) StartIndex);

            if (index < 0 || index >= Size)
                return false;

            T item = this[index];
            return TryFree(item);
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

        public string GetBitString()
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

                    int zeros;

                    #if NET7_0_OR_GREATER
                        zeros = System.Numerics.BitOperations.LeadingZeroCount(marker);
                    #else
                        zeros = Backports.LeadingZeroCount(marker);
                    #endif

                    if (zeros != 0)
                    {
                        firstFree = (i * _primSize) + (_primSize - zeros);
                        break;
                    }
                }

                if (firstFree != -1)
                {
                    Interlocked.Exchange(ref AvailableIndex, firstFree);
                    if (List.PageIndexWithAvailability == -1 || List.PageIndexWithAvailability > Index)
                        Interlocked.Exchange(ref List._pageIndexWithAvailability, Index);
                }

            } finally { _lock.ExitWriteLock(); }

        }

        protected void _Internal_DoAllocate(int index, out T allocated)
        {
            _Internal_UpdateBit(index, true);

            allocated = this[index];
            allocated.Allocated = true;
            List.OnAllocate?.Invoke(allocated);

            // We just completed an allocation, we are not empty.
            Empty = false;

            _Internal_CalculateNextFree();
        }

        protected void _Internal_UpdateBit(int index, bool on)
        {
            try
            {
                _lock.EnterWriteLock();
                int longIndex = (int) (index * _multiplier);
                int bitIndex = index % _primSize;

                if (on)
                    _allocated[longIndex] |= (1UL << bitIndex);
                else
                    _allocated[longIndex] &= ~(1UL << bitIndex);

            } finally { _lock.ExitWriteLock(); }

        }

    }

}
