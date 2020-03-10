// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace AsmResolver.Lazy
{
    /// <summary>
    /// Provides a base for lists that are lazy initialized.
    /// </summary>
    /// <typeparam name="TItem">The type of elements the list stores.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public abstract class LazyList<TItem> : IList<TItem>
    {
        private readonly List<TItem> _items = new List<TItem>();

        /// <inheritdoc />
        public TItem this[int index]
        {
            get
            {
                EnsureIsInitialized();
                return Items[index];
            }
            set
            {
                EnsureIsInitialized();
                OnSetItem(index, value);
            }
        }

        /// <inheritdoc />
        public virtual int Count
        {
            get
            {
                EnsureIsInitialized();
                return Items.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a value indicating the list is initialized or not.
        /// </summary>
        protected bool IsInitialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the underlying list.
        /// </summary>
        public IList<TItem> Items => _items;

        /// <summary>
        /// Initializes the list. This method is called in a thread-safe manner.
        /// </summary>
        protected abstract void Initialize();

        private void EnsureIsInitialized()
        {
            if (!IsInitialized)
            {
                lock (this)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                        IsInitialized = true;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Add(TItem item) => Insert(Count, item);

        /// <inheritdoc />
        public void Clear()
        {
            OnClearItems();
            IsInitialized = true;
        }

        /// <inheritdoc />
        public bool Contains(TItem item)
        {
            EnsureIsInitialized();
            return Items.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            EnsureIsInitialized();
            Items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(TItem item)
        {
            EnsureIsInitialized();
            int index = Items.IndexOf(item);
            if (index == -1)
                return false;
            OnRemoveItem(index);
            return true;
        }

        /// <inheritdoc />
        public int IndexOf(TItem item)
        {
            EnsureIsInitialized();
            return Items.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, TItem item)
        {
            EnsureIsInitialized();
            OnInsertItem(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            EnsureIsInitialized();
            OnRemoveItem(index);
        }

        protected virtual void OnSetItem(int index, TItem item) => Items[index] = item;
        
        protected virtual void OnInsertItem(int index, TItem item) => Items.Insert(index, item);
        
        protected virtual void OnRemoveItem(int index) => Items.RemoveAt(index);

        protected virtual void OnClearItems() => Items.Clear();

        /// <summary>
        /// Returns an enumerator that enumerates the lazy list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        /// <remarks>
        /// This enumerator only ensures the list is initialized upon calling the <see cref="Enumerator.MoveNext"/> method.
        /// </remarks>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <inheritdoc />
        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Represents an enumerator that enumerates all items in a lazy initialized list.
        /// </summary>
        /// <remarks>
        /// The enumerator only initializes the list when it is needed. If no calls to <see cref="MoveNext"/> were
        /// made, and the lazy list was not initialized yet, it will remain uninitialized.
        /// </remarks>
        public struct Enumerator : IEnumerator<TItem>
        {
            private readonly LazyList<TItem> _list;
            private List<TItem>.Enumerator _enumerator;
            private bool hasEnumerator;
            
            public Enumerator(LazyList<TItem> list)
            {
                _list = list;
                _enumerator = default;
                hasEnumerator = false;
            }

            /// <inheritdoc />
            public TItem Current => hasEnumerator ? _enumerator.Current : default;

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (!hasEnumerator)
                {
                    _list.EnsureIsInitialized();
                    _enumerator = _list._items.GetEnumerator();
                    hasEnumerator = true;
                }

                return _enumerator.MoveNext();
            }

            /// <inheritdoc />
            public void Reset() => throw new NotSupportedException();

            /// <inheritdoc />
            public void Dispose()
            {
                if (hasEnumerator)
                    _enumerator.Dispose();
            }
        }
        
    }
}