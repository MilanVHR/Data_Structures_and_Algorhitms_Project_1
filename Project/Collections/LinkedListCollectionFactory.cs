// Factory implementation for creating instances of Linked Lists.
// The service layer can use this factory to create linked lists 
//without depending on the specific implementation of the collection.

namespace Project.Collections
{
    public class LinkedListCollectionFactory<T> : IMyCollectionFactory<T>
    {
        // Creates a new empty linked list collection where capacity is ignored due to the nature of linked lists
        public IMyCollection<T> Create(int capacity = 8)
        {
            return new LinkedListCollection<T>();
        }

        // Creates a new linked list from an array
        public IMyCollection<T> CreateFromArray(T[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            IMyCollection<T> collection = new LinkedListCollection<T>();

            foreach (T item in items)
            {
                collection.Add(item);
            }

            return collection;
        }
    }
}