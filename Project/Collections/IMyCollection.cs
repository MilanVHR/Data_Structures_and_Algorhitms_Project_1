using System;

namespace Project.Collections
{
    public interface IMyCollection<T>
    {
        void Add(T item);
        void Remove(T item);
        T? FindBy<K>(K key, Func<T, K, bool> comparer);
        int Count { get; }
        IMyIterator<T> GetIterator();
    }
}
