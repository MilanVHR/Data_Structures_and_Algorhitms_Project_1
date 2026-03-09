// This interface defines the basic operations for a custom collection.
// This interface is implemented in ArrayCollection (and LATER LinkedList, etc.)
// It replaces List<T> and other built‑in collections.

using System;

namespace Project.Collections
{
    public interface IMyCollection<T>
    {
        // Adds an item to the collection.
        void Add(T item);

        // Removes an item from the collection (if found).
        void Remove(T item);

        // Finds an item by a key using a custom comparer function.
        // Example: FindBy(id, (task, key) => task.Id == key)
        T? FindBy<K>(K key, Func<T, K, int> comparer);

        // Filter returns a new collection that match the predicate.
        IMyCollection<T> Filter(Func<T, bool> predicate);

        // Sorts the collection in-place
        void Sort(Comparison<T> comparison);

        // Reduce/aggregate
        R Reduce<R>(R initial, Func<R, T, R> accumulator);

        // Number of elements currently stored.
        int Count { get; }

        // Returns a custom iterator to loop through the collection.
        IMyIterator<T> GetIterator();
    }
}