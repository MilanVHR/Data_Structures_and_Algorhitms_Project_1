using System;

namespace Project.Collections
{
    public class ArrayIterator<T> : IMyIterator<T>
    {
        private readonly T[] _items;
        private readonly int _count;
        private int _index;

        public ArrayIterator(T[] items, int count)
        {
            _items = items;
            _count = count;
            _index = 0;
        }

        public bool HasNext() => _index < _count;

        public T Next()
        {
            if (!HasNext()) throw new InvalidOperationException();
            return _items[_index++];
        }

        public void Reset() => _index = 0;
    }

    public class ArrayCollection<T> : IMyCollection<T>
    {
        private T[] _items;
        private int _count;

        public ArrayCollection(int capacity = 8)
        {
            _items = new T[capacity];
        }

        public int Count => _count;

        private void EnsureCapacity()
        {
            if (_count < _items.Length) return;
            Array.Resize(ref _items, _items.Length * 2);
        }

        public void Add(T item)
        {
            EnsureCapacity();
            _items[_count++] = item;
        }

        public void Remove(T item)
        {
            int idx = -1;
            for (int i = 0; i < _count; i++)
            {
                if (Equals(_items[i], item))
                {
                    idx = i;
                    break;
                }
            }
            if (idx == -1) return;

            for (int i = idx; i < _count - 1; i++)
                _items[i] = _items[i + 1];

            _count--;
        }

        public T? FindBy<K>(K key, Func<T, K, bool> comparer)
        {
            for (int i = 0; i < _count; i++)
            {
                if (comparer(_items[i], key))
                    return _items[i];
            }
            return default;
        }

        public IMyIterator<T> GetIterator() => new ArrayIterator<T>(_items, _count);

        public T[] ToArray()
        {
            var arr = new T[_count];
            Array.Copy(_items, arr, _count);
            return arr;
        }

        public static ArrayCollection<T> FromArray(T[] arr)
        {
            var col = new ArrayCollection<T>(arr.Length);
            foreach (var item in arr)
                col.Add(item);
            return col;
        }
    }
}
