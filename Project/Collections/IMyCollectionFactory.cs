//This interface defines a factory and its responsible for creating instances of IMyCollection<T>.
// The service layer depends on this interface, not on the concrete implementation of the collection.
// This allows you to replace the collection implementation (e.g., ArrayCollection) with another type 
// (e.g., LinkedListCollection) without changing the service layer or other parts of the application.

namespace Project.Collections
{
    public interface IMyCollectionFactory<T>
    {
        IMyCollection<T> Create(int capacity = 8);

        IMyCollection<T> CreateFromArray(T[] items);
    }
}