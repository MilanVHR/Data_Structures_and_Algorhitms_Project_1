namespace Project.Collections
{
    public class LinkedListCollectionFactory<T> : IMyCollectionFactory<T>
    {
        public IMyCollection<T> Create(int capacity = 8)
        {
            // Capacity is not relevant for linked lists, but we can ignore it
            return new LinkedListCollection<T>();
        }

        public IMyCollection<T> CreateFromArray(T[] items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            IMyCollection<T> collection = Create();

            foreach (var item in items)
                collection.Add(item);

            return collection;
        }
    }
}