using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Rapp.Gen
{
    /// <summary>
    /// An immutable array with structural equality.
    /// </summary>
    /// <remarks>
    /// Values flowing through an incremental generator pipeline are compared between runs to decide
    /// whether downstream work can be skipped. <see cref="ImmutableArray{T}"/> compares by the
    /// identity of the underlying array, so two arrays with equal contents compare unequal and the
    /// generator re-runs on every keystroke. This type compares element by element instead.
    /// </remarks>
    internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
        where T : IEquatable<T>
    {
        private readonly T[]? _items;

        public EquatableArray(T[] items) => _items = items;

        public int Length => _items?.Length ?? 0;

        public T this[int index] => _items![index];

        public bool Equals(EquatableArray<T> other)
        {
            if (_items is null || other._items is null)
            {
                return _items is null && other._items is null;
            }

            if (_items.Length != other._items.Length)
            {
                return false;
            }

            for (var i = 0; i < _items.Length; i++)
            {
                if (!_items[i].Equals(other._items[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

        public override int GetHashCode()
        {
            if (_items is null)
            {
                return 0;
            }

            var hash = 17;
            foreach (var item in _items)
            {
                hash = (hash * 31) + (item?.GetHashCode() ?? 0);
            }

            return hash;
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_items ?? Array.Empty<T>())).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
