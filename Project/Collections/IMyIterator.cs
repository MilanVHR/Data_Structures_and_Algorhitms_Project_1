// This interface defines a custom iterator used to traverse elements
// inside your own data structures (like ArrayCollection). It replaces
// the need for built‑in IEnumerable or IEnumerator.
namespace Project.Collections
{
    public interface IMyIterator<T>
    {
        // Returns true if there is another element to read.
        bool HasNext();

        // Returns the next element in the collection.
        // Throws an exception if no more elements exist.
        T Next();

        // Resets the iterator back to the first element.
        void Reset();
    }
}
