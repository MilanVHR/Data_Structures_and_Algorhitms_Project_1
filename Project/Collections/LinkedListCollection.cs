
namespace Project.Collections
{
    public class LinkedListNode<T>
    {
        public T Data { get; set; }
        public LinkedListNode<T>? Next { get; set; }

        public LinkedListNode(T data)
        {
            Data = data;
            Next = null;
        }
    }

    // Iterator for LinkedListCollection that loops through the list
    public class LinkedListIterator<T> : IMyIterator<T>
    {
        private readonly LinkedListNode<T>? _head; // First node in the list
        private LinkedListNode<T>? _current;       // Current position

        public LinkedListIterator(LinkedListNode<T>? head)
        {
            _head = head;
            _current = head;
        }

        public bool HasNext()
        {
            return _current != null;
        }

        public T Next()
        {
            if (_current == null) throw new InvalidOperationException("No more items in the list");

            T value = _current.Data;
            _current = _current.Next;
            return value;
        }

        public void Reset()
        {
            _current = _head;
        }
    }

    public class LinkedListCollection<T> : IMyCollection<T>
    {
        private LinkedListNode<T>? _head; // First node in the list
        private LinkedListNode<T>? _tail; // Last node in the list
        private int _count;

        public int Count => _count;

        // Adds a new item to the end of the list
        public void Add(T item)
        {
            LinkedListNode<T> newNode = new LinkedListNode<T>(item);

            if (_head == null)
            {
                _head = newNode;
                _tail = newNode;
            }
            else
            {
                _tail!.Next = newNode;
                _tail = newNode;
            }

            _count++;
        }

        // Removes the first occurrence of the item
        public bool Remove(T item)
        {
            LinkedListNode<T>? current = _head;
            LinkedListNode<T>? previous = null;

            while (current != null)
            {
                if (Equals(current.Data, item))
                {
                    if (previous == null)
                    {
                        _head = current.Next;
                    }
                    else
                    {
                        previous.Next = current.Next;
                    }

                    if (current == _tail)
                    {
                        _tail = previous;
                    }

                    _count--;
                    return true;
                }

                previous = current;
                current = current.Next;
            }

            return false;
        }

        // Finds the first element that is true for the predicate
        public T? Find(Func<T, bool> predicate)
        {
            LinkedListNode<T>? current = _head;

            while (current != null)
            {
                if (predicate(current.Data))
                    return current.Data;

                current = current.Next;
            }

            return default;
        }

        // Returns a new LinkedListCollection containing filtered elements
        public IMyCollection<T> Filter(Func<T, bool> predicate)
        {
            IMyCollection<T> filtered = new LinkedListCollection<T>();
            LinkedListNode<T>? current = _head;

            while (current != null)
            {
                if (predicate(current.Data))
                    filtered.Add(current.Data);

                current = current.Next;
            }

            return filtered;
        }

        // Bubble sort implementation for linked list
        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            if (_head == null || _head.Next == null)
                return;

            bool swapped;
            do
            {
                swapped = false;
                LinkedListNode<T> current = _head;

                // Walk through the list
                while (current.Next != null)
                {
                    // Compare current node with the next node
                    if (comparison(current.Data, current.Next.Data) > 0)
                    {
                        // Swap the data values
                        T temp = current.Data;
                        current.Data = current.Next.Data;
                        current.Next.Data = temp;

                        swapped = true;
                    }
                    current = current.Next;
                }
            } while (swapped);
        }

        // Aggregates all elements into 1 value
        public R Reduce<R>(R initial, Func<R, T, R> accumulator)
        {
            R result = initial;
            LinkedListNode<T>? current = _head;

            while (current != null)
            {
                result = accumulator(result, current.Data);
                current = current.Next;
            }

            return result;
        }

        // Returns an iterator to loop through the collection
        public IMyIterator<T> GetIterator()
        {
            return new LinkedListIterator<T>(_head);
        }
    }
}