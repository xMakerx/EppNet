﻿///////////////////////////////////////////////////////
/// Filename: PageList.cs
/// Date: July 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EppNet.Collections
{

    public class PageList<T> where T : IPageable, new()
    {
        public readonly int ItemsPerPage;

        protected readonly float _itemIndexToPageIndexMult;
        protected List<Page<T>> _pages;

        internal int _pageIndexWithAvaliability;

        public PageList(int itemsPerPage)
        {
            this.ItemsPerPage = itemsPerPage;
            this._itemIndexToPageIndexMult = 1f / itemsPerPage;
            this._pages = new List<Page<T>>();
            this._pageIndexWithAvaliability = -1;
        }

        public void DoOnActive(Action<T> action)
        {
            for (int i = 0; i < _pages.Count; i++)
                _pages[i].DoOnActive(action);
        }

        public int PurgeEmptyPages()
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

        public bool TryAllocate(in long id, out T allocated)
        {
            int pageIndex = (int)Math.Floor(id * _itemIndexToPageIndexMult);

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

        public bool TryAllocate(out T allocated)
        {
            Page<T> page;

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

        public bool TryFree(T toFree)
        {
            Page<T> page = toFree.Page as Page<T>;
            return page.TryFree(toFree);
        }

        public bool IsAvailable(long id)
        {
            int pageIndex = (int)Math.Floor(id * _itemIndexToPageIndexMult) - 1;

            if (pageIndex >= _pages.Count)
                return false;

            Page<T> page = _pages[pageIndex];
            return page[Convert.ToInt32(id - page.StartIndex)].IsFree();
        }

        public T Get(long id)
        {
            int pageIndex = (int) Math.Floor(id * _itemIndexToPageIndexMult);

            if (pageIndex > _pages.Count)
                return default;

            Page<T> page = _pages[pageIndex];
            return page[ItemIdToPageIndex(page, id)];
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
        public int ID { set; get; }

        public bool IsFree();
    }

    public class Page<T> : List<T>, IPage where T : IPageable, new()
    {

        internal const int _primSize = 64;
        internal const float _multiplier = 1 / ((float)_primSize);

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

        internal Page([NotNull] PageList<T> list, int index, int size) : base(size)
        {
            Guard.AgainstNull(list);
            this.List = list;
            this.Index = index;
            this.Size = size;
            this.StartIndex = Index * List.ItemsPerPage;
            this.AvailableIndex = 0;

            this._allocated = new long[(int)Math.Ceiling(Size * _multiplier)];

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
                int index = ItemIDToIndex(item);

                item.Dispose();
                _Internal_UpdateBit(index, false);

                OnFree?.Invoke(item);

                // Check if empty
                IsEmpty();

                if (AvailableIndex == -1 || index < AvailableIndex)
                    AvailableIndex = index;

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ItemIDToIndex([NotNull] T item)
        {
            Guard.AgainstNull(item);
            return item.ID - StartIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
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

        public int GetIndex() => Index;

        protected void _Internal_CalculateNextFree()
        {
            AvailableIndex = -1;

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
                        this.AvailableIndex = StartIndex + (i * _primSize) + bit;
                        foundFree = true;
                        break;
                    }
                }

                if (foundFree)
                {
                    if (List._pageIndexWithAvaliability == -1 || List._pageIndexWithAvaliability > Index)
                        List._pageIndexWithAvaliability = Index;
                    break;
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

            if (on)
                _allocated[longIndex] = buffer | (1 << bitIndex);
            else
                _allocated[longIndex] = buffer & ~(1 << bitIndex);
        }

    }

}