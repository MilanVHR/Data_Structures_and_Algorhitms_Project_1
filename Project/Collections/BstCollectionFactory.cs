using System;

namespace Project.Collections
{
	public class BstCollectionFactory<T> : IMyCollectionFactory<T>
	{
		public IMyCollection<T> Create(int capacity = 8)
		{
			return new BstCollection<T>();
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
