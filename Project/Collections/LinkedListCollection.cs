
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

        // Sorts the list using merge sort
        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            if (_head == null || _head.Next == null)
                return;

            _head = MergeSort(_head, comparison);

            // Rebuild _tail after sorting
            _tail = _head;
            while (_tail != null && _tail.Next != null)
            {
                _tail = _tail.Next;
            }
        }

        private LinkedListNode<T>? MergeSort(LinkedListNode<T>? head, Comparison<T> comparison)
        {
            if (head == null || head.Next == null)
                return head;

            LinkedListNode<T> middle = GetMiddle(head);
            LinkedListNode<T>? rightHalf = middle.Next;
            middle.Next = null;

            LinkedListNode<T>? left = MergeSort(head, comparison);
            LinkedListNode<T>? right = MergeSort(rightHalf, comparison);

            return Merge(left, right, comparison);
        }

        private LinkedListNode<T> GetMiddle(LinkedListNode<T> head)
        {
            LinkedListNode<T> slow = head;
            LinkedListNode<T>? fast = head.Next;

            while (fast != null && fast.Next != null)
            {
                slow = slow.Next!;
                fast = fast.Next.Next;
            }

            return slow;
        }

        private LinkedListNode<T>? Merge(LinkedListNode<T>? left, LinkedListNode<T>? right, Comparison<T> comparison)
        {
            if (left == null) return right;
            if (right == null) return left;

            LinkedListNode<T>? head;
            LinkedListNode<T>? tail;

            // Initialize head and tail
            if (comparison(left.Data, right.Data) <= 0)
            {
                head = left;
                left = left.Next;
            }
            else
            {
                head = right;
                right = right.Next;
            }

            tail = head;

            // Merge remaining nodes
            while (left != null && right != null)
            {
                if (comparison(left.Data, right.Data) <= 0)
                {
                    tail.Next = left;
                    left = left.Next;
                }
                else
                {
                    tail.Next = right;
                    right = right.Next;
                }

                tail = tail.Next!;
            }

            // Attach remaining part
            tail.Next = left ?? right;

            return head;
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