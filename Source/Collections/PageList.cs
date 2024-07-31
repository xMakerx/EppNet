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
using System.Threading;

namespace EppNet.Collections
{

    public class PageList<T> where T : IPageable, new()
    {
        public readonly int ItemsPerPage;

        protected readonly float _itemIndexToPageIndexMult;
        protected List<Page<T>> _pages;

        internal int _pageIndexWithAvaliability;
        private object _pageLock;

        public PageList(int itemsPerPage)
        {
            this.ItemsPerPage = itemsPerPage;
            this._itemIndexToPageIndexMult = 1f / itemsPerPage;
            this._pages = new List<Page<T>>();
            this._pageIndexWithAvaliability = -1;
            this._pageLock = new object();
        }

        public void DoOnActive(Action<T> action)
        {
            for (int i = 0; i < _pages.Count; i++)
                _pages[i].DoOnActive(action);
        }

        public void Clear()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                Page<T> page = _pages[i];
                page.ClearAll();
            }

            _pages.Clear();
        }

        public int PurgeEmptyPages()
        {
            lock (_pageLock)
            {
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
        }

        public bool TryAllocate(in long id, out T allocated)
        {
            int pageIndex = (int)Math.Floor(id * _itemIndexToPageIndexMult);

            lock (_pageLock)
            {
                if (pageIndex >= _pages.Count)
                {
                    // We don't have a page for this
                    int pagesToAllocate = pageIndex - _pages.Count;

                    for (int i = _pages.Count; i < pagesToAllocate; i++)
                    {
                        Page<T> newPage = new(this, i, ItemsPerPage);
                        _pages.Add(newPage);
                    }
                }

                Page<T> page = _pages[pageIndex];

                int index = Convert.ToInt32(id - page.StartIndex);
                allocated = page[index];
                return allocated.IsFree();
            }

        }

        public bool TryAllocate(out T allocated)
        {
            Page<T> page;

            lock (_pageLock)
            {
                if (_pageIndexWithAvaliability != -1)
                    // We know that this page has availability
                    page = _pages[_pageIndexWithAvaliability];
                else
                {
                    // We don't have a page with availability
                    // Allocate one.
                    page = new Page<T>(this, _pages.Count, ItemsPerPage);
                    _pages.Add(page);
                }

                return page.TryAllocate(out allocated);
            }

        }

        public bool TryFree(T toFree)
        {
            Page<T> page = toFree.Page as Page<T>;
            return page.TryFree(toFree);
        }

        public bool IsAvailable(long id)
        {
            lock (_pageLock)
            {
                int pageIndex = (int)Math.Floor(id * _itemIndexToPageIndexMult);

                if (pageIndex >= _pages.Count)
                    return false;

                Page<T> page = _pages[pageIndex];
                return page[Convert.ToInt32(id - page.StartIndex)].IsFree();
            }
        }

        public bool TryGetById(long id, out T item)
        {
            lock (_pageLock)
            {
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
        public int ItemIdToPageIndex(long id)
        {
            int pageIndex = (int)Math.Floor(id * _itemIndexToPageIndexMult);
            long delta = id - (pageIndex * ItemsPerPage);
            return Convert.ToInt32(delta);
        }
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

    public class Page<T> : List<T>, IPage where T : IPageable, new()
    {

        internal const int _primSize = 64;
        internal const float _multiplier = 1f / ((float)_primSize);

        public readonly PageList<T> List;
        public readonly int Index;
        public readonly int Size;

        public bool Empty { private set; get; }

        public readonly int StartIndex;
        public int AvailableIndex;

        public bool HasFree => AvailableIndex != -1;

        public Action<T> OnAllocate;
        public Action<T> OnFree;

        private readonly long[] _allocated;
        private readonly object _allocationLock;

        internal Page([NotNull] PageList<T> list, int index, int size) : base(size)
        {
            Guard.AgainstNull(list);
            this.List = list;
            this.Index = index;
            this.Size = size;
            this.StartIndex = Index * List.ItemsPerPage;
            this.AvailableIndex = 0;

            this._allocated = new long[(int)Math.Ceiling(Size * _multiplier)];
            this._allocationLock = new();

            int aIndex = 0;
            for (int i = 0; i < size; i++)
            {
                T item = new()
                {
                    Page = this,
                    ID = StartIndex + i
                };

                Add(item);

                if (i % _primSize == 0)
                    _allocated[aIndex++] = 0L;
            }

        }

        public void DoOnActive(Action<T> action)
        {
            lock (_allocationLock)
            {

                if (Empty)
                    return;

                for (int i = 0; i < _allocated.Length; i++)
                {
                    long marker = _allocated[i];

                    // Nothing to iterate
                    if (marker == 0L)
                        continue;

                    for (int bit = 0; bit < _primSize; bit++)
                    {
                        if ((marker & (1 << bit)) != 0)
                        {
                            // We found something!
                            int index = StartIndex + (i * _primSize) + bit;

                            T item = this[index];
                            action.Invoke(item);
                        }
                    }
                }
            }

        }

        public void ClearAll()
        {
            for (int i = 0; i < Count; i++)
                TryFree(this[i]);

            Clear();
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
            lock (_allocationLock)
            {
                for (int i = 0; i < _allocated.Length; i++)
                {
                    if (_allocated[i] != 0L)
                    {
                        Empty = false;
                        break;
                    }
                }

                Empty = true;
                return Empty;
            }
        }

        public int GetIndex() => Index;

        protected void _Internal_CalculateNextFree()
        {
            AvailableIndex = -1;

            lock (_allocationLock)
            {

                bool foundFree = false;

                for (int i = 0; i < _allocated.Length; i++)
                {
                    long marker = _allocated[i];

                    if (marker == long.MaxValue)
                        continue;

                    for (int bit = 0; bit < _primSize; bit++)
                    {
                        if ((marker & (1 << bit)) == 0)
                        {
                            Interlocked.Exchange(ref AvailableIndex, StartIndex + (i * _primSize) + bit);
                            foundFree = true;
                            break;
                        }
                    }

                    if (foundFree)
                    {
                        if (List._pageIndexWithAvaliability == -1 || List._pageIndexWithAvaliability > Index)
                            Interlocked.Exchange(ref List._pageIndexWithAvaliability, Index);
                        break;
                    }
                }
            }

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
            int longIndex = (int)Math.Floor(index * _multiplier);
            int bitIndex = index - (longIndex * _primSize);

            long buffer = _allocated[longIndex];
            
            lock (_allocationLock)
            {
                if (on)
                    _allocated[longIndex] = buffer | (1 << bitIndex);
                else
                    _allocated[longIndex] = buffer & ~(1 << bitIndex);
            }

        }

    }

}
