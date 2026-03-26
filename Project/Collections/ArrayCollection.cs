// This file contains the implementation of a dynamic array-based collection.
// It behaves similarly to List<T>

using System;

namespace Project.Collections
{
    // Iterator for ArrayCollection.
    // It walks through the internal array from index 0 to Count.
    public class ArrayIterator<T> : IMyIterator<T>
    {
        private readonly T[] _items;   // Reference to the internal array
        private readonly int _count;   // Number of valid elements
        private int _index;            // Current position

        public ArrayIterator(T[] items, int count)
        {
            _items = items;
            _count = count;
            _index = 0;
        }

        public bool HasNext() => _index < _count;

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements in iterator.");

            return _items[_index++];
        }

        public void Reset() => _index = 0;
    }

    // A manually implemented dynamic array.
    // Automatically grows when capacity is reached.
    public class ArrayCollection<T> : IMyCollection<T>
    {
        private T[] _items;   // Internal storage array
        private int _count;   // Number of elements currently stored

        public ArrayCollection(int capacity = 8)
        {
            if (capacity <= 0)
                capacity = 8;

            _items = new T[capacity];
            _count = 0;
        }

        public int Count => _count;

        // Ensures the internal array has enough space.
        // If full, it doubles the capacity.
        private void EnsureCapacity()
        {
            if (_count < _items.Length)
                return;

            int newSize = _items.Length * 2;
            T[] newArr = new T[newSize];

            for (int i = 0; i < _count; i++)
                newArr[i] = _items[i];

            _items = newArr;
        }

        public void Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item), "Null items are not allowed in this collection.");

            EnsureCapacity();
            _items[_count++] = item;
        }

        public bool Remove(T item)
        {
            // Find index of the item
            int index = -1;
            for (int i = 0; i < _count; i++)
            {
                if (Equals(_items[i], item))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return false; // Item not found

            // Shift elements left to fill the gap
            for (int i = index; i < _count - 1; i++)
                _items[i] = _items[i + 1];

            _count--;
            return true;
        }

        // Finds the first element that satisfies the predicate.
        public T? Find(Func<T, bool> predicate)
        {
            for (int i = 0; i < _count; i++)
            {
                if (predicate(_items[i]))
                    return _items[i];
            }

            return default;
        }

        // Returns an iterator to traverse the collection.
        public IMyIterator<T> GetIterator()
        {
            return new ArrayIterator<T>(_items, _count);
        }

        // Converts the internal array to a trimmed array (no empty slots).
        public T[] ToArray()
        {
            T[] arr = new T[_count];
            for (int i = 0; i < _count; i++)
                arr[i] = _items[i];
            return arr;
        }

        // Creates a new ArrayCollection from an existing array.
        public static ArrayCollection<T> FromArray(T[] arr)
        {
            var col = new ArrayCollection<T>(arr.Length);
            foreach (var item in arr)
                col.Add(item);
            return col;
        }

        // Returns a new ArrayCollection containing predicated elements.
        public IMyCollection<T> Filter(Func<T, bool> predicate)
        {
            var result = new ArrayCollection<T>();
            for (int i = 0; i < _count; i++)
                if (predicate(_items[i]))
                    result.Add(_items[i]);
            return result;
        }

        // Sorts the collection in-place.
        public void Sort(Comparison<T> comparison)
        {
            for (int i = 0; i < _count; i++)
            {
                T key = _items[i]; // Element to insert in sorted part
                int j = i - 1; // Start at last element of the sorted part

                // Shift elements to the right until correct position for key is found
                while (j >= 0 && comparison(_items[j], key) > 0)
                {
                    _items[j + 1] = _items[j];
                    j--;
                }

                _items[j + 1] = key; // Insert element at correct position
            }
        }

        public R Reduce<R>(R initial, Func<R, T, R> accumulator)
        {
            R acc = initial;
            for (int i = 0; i < _count; i++)
                acc = accumulator(acc, _items[i]);
            return acc;
        }
    }
}
