// Factory implementation for creating instances of ArrayCollection<T>.
// This class implements the IMyCollectionFactory<T> interface and provides methods to create new collections
// with a specified capacity or from an existing array of items. The service layer can use this factory
// to create collections without depending on the specific implementation of the collection, allowing for flexibility and separation of concerns.

using System;

namespace Project.Collections
{
    public class ArrayCollectionFactory<T> : IMyCollectionFactory<T>
    {
        public IMyCollection<T> Create(int capacity = 8)
        {
            return new ArrayCollection<T>(capacity);
        }

        public IMyCollection<T> CreateFromArray(T[] items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            IMyCollection<T> collection = Create(items.Length > 0 ? items.Length : 8);

            foreach (var item in items)
                collection.Add(item);

            return collection;
        }
    }
}