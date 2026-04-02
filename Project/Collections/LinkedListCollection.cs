namespace Project.Collections
{
    public class LinkedListIterator<T> : IMyIterator<T>
    {
        private readonly LinkedList<T> _list;
        private LinkedListNode<T> _current;
        public LinkedListIterator(LinkedList<T> list)
        {
            _list = list;
            _current = null;
        }
        public bool HasNext()
        {
            if (_current == null)
                return _list.First != null;
            return _current.Next != null;
        }
        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements in iterator.");
            _current = _current == null ? _list.First : _current.Next;
            return _current.Value;
        }
        public void Reset()
        {
            _current = null;
        }
    }

    public class LinkedListCollection<T> : IMyCollection<T>
    {
        private readonly LinkedList<T> _list;

        public LinkedListCollection()
        {
            _list = new LinkedList<T>();
        }

        public int Count => _list.Count;

        public void Add(T item)
        {
            _list.AddLast(item);
        }

        public IMyIterator<T> GetIterator()
        {
            return new LinkedListIterator<T>(_list);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public T? Find(Func<T, bool> predicate)
        {
            foreach (var item in _list)
            {
                if (predicate(item))
                    return item;
            }
            return default;
        }
    }
}