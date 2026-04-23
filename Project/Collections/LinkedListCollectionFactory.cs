// Factory implementation for creating instances of Linked Lists.
// The service layer can use this factory to create linked lists 
//without depending on the specific implementation of the collection.

namespace Project.Collections
{
    public class LinkedListCollectionFactory<T> : IMyCollectionFactory<T>
    {
        public string Name => "Linked List";

        public IMyCollection<T> Create()
        {
            return new LinkedListCollection<T>();
        }
    }
}