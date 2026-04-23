// Factory implementation for creating instances of ArrayCollection<T>.
// This class implements IMyCollectionFactory<T> so the application can create
// the configured collection type without depending on the concrete class directly.

using System;

namespace Project.Collections
{
    public class ArrayCollectionFactory<T> : IMyCollectionFactory<T>
    {
        public string Name => "Array";

        public IMyCollection<T> Create()
        {
            return new ArrayCollection<T>();
        }
    }
}