using System;
using System.Collections.Generic;

namespace Project.Collections
{
    public class BstNode<T>
    {
        public T Data { get; set; }
        public BstNode<T>? Left { get; set; }
        public BstNode<T>? Right { get; set; }

        public BstNode(T data)
        {
            Data = data;
            Left = null;
            Right = null;
        }
    }

    public class BstIterator<T> : IMyIterator<T>
    {
        private readonly BstNode<T>? _root;
        private BstNode<T>? _current;
        private BstNode<T>?[] _stack;
        private int _stackCount;

        public BstIterator(BstNode<T>? root)
        {
            _root = root;
            _current = root;
            _stack = new BstNode<T>?[8];
            _stackCount = 0;
            PushLeftPath(_current);
        }

        public bool HasNext()
        {
            return _stackCount > 0;
        }

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements in iterator.");

            BstNode<T>? node = Pop();
            if (node == null)
                throw new InvalidOperationException("Invalid iterator state.");

            if (node.Right != null)
                PushLeftPath(node.Right);

            return node.Data;
        }

        public void Reset()
        {
            _current = _root;
            _stackCount = 0;
            PushLeftPath(_current);
        }

        private void PushLeftPath(BstNode<T>? node)
        {
            while (node != null)
            {
                Push(node);
                node = node.Left;
            }
        }

        private void Push(BstNode<T> node)
        {
            if (_stackCount >= _stack.Length)
            {
                int newSize = _stack.Length * 2 + 1;
                BstNode<T>?[] newStack = new BstNode<T>?[newSize];

                for (int i = 0; i < _stackCount; i++)
                    newStack[i] = _stack[i];

                _stack = newStack;
            }

            _stack[_stackCount++] = node;
        }

        private BstNode<T>? Pop()
        {
            if (_stackCount == 0)
                return null;

            _stackCount--;
            BstNode<T>? node = _stack[_stackCount];
            _stack[_stackCount] = null;
            return node;
        }
    }

    public class BstCollection<T> : IMyCollection<T>
    {
        private BstNode<T>? _root;
        private int _count;

        public int Count => _count;

        public void Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item), "Null items are not allowed in this collection.");

            Comparison<T> comparison = GetDefaultComparison();
            _root = Insert(_root, item, comparison);
            _count++;
        }

        public bool Remove(T item)
        {
            if (_root == null)
                return false;

            Comparison<T> comparison = GetDefaultComparison();
            bool removed;
            _root = RemoveNode(_root, item, comparison, out removed);

            if (removed)
                _count--;

            return removed;
        }

        public T? Find(Func<T, bool> predicate)
        {
            var iterator = GetIterator();

            while (iterator.HasNext())
            {
                T item = iterator.Next();
                if (predicate(item))
                    return item;
            }

            return default;
        }

        public IMyCollection<T> Filter(Func<T, bool> predicate)
        {
            IMyCollection<T> filtered = new BstCollection<T>();
            var iterator = GetIterator();

            while (iterator.HasNext())
            {
                T item = iterator.Next();
                if (predicate(item))
                    filtered.Add(item);
            }

            return filtered;
        }

        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            if (_count <= 1)
                return;

            T[] items = new T[_count];
            int index = 0;

            var iterator = GetIterator();
            while (iterator.HasNext())
                items[index++] = iterator.Next();

            for (int i = 1; i < items.Length; i++)
            {
                T key = items[i];
                int j = i - 1;

                while (j >= 0 && comparison(items[j], key) > 0)
                {
                    items[j + 1] = items[j];
                    j--;
                }

                items[j + 1] = key;
            }

            _root = null;
            _count = 0;

            for (int i = 0; i < items.Length; i++)
            {
                _root = Insert(_root, items[i], comparison);
                _count++;
            }
        }

        public R Reduce<R>(R initial, Func<R, T, R> accumulator)
        {
            R result = initial;
            var iterator = GetIterator();

            while (iterator.HasNext())
                result = accumulator(result, iterator.Next());

            return result;
        }

        public IMyIterator<T> GetIterator()
        {
            return new BstIterator<T>(_root);
        }

        private static Comparison<T> GetDefaultComparison()
        {
            return (left, right) =>
            {
                try
                {
                    return Comparer<T>.Default.Compare(left, right);
                }
                catch (ArgumentException)
                {
                    string leftText = left?.ToString() ?? string.Empty;
                    string rightText = right?.ToString() ?? string.Empty;
                    int byText = string.Compare(leftText, rightText, StringComparison.Ordinal);

                    if (byText != 0)
                        return byText;

                    int leftHash = left?.GetHashCode() ?? 0;
                    int rightHash = right?.GetHashCode() ?? 0;
                    return leftHash.CompareTo(rightHash);
                }
            };
        }

        private static BstNode<T> Insert(BstNode<T>? node, T item, Comparison<T> comparison)
        {
            if (node == null)
                return new BstNode<T>(item);

            int cmp = comparison(item, node.Data);

            if (cmp < 0)
                node.Left = Insert(node.Left, item, comparison);
            else
                node.Right = Insert(node.Right, item, comparison);

            return node;
        }

        private static BstNode<T>? RemoveNode(BstNode<T>? node, T item, Comparison<T> comparison, out bool removed)
        {
            if (node == null)
            {
                removed = false;
                return null;
            }

            int cmp = comparison(item, node.Data);

            if (cmp < 0)
            {
                node.Left = RemoveNode(node.Left, item, comparison, out removed);
                return node;
            }

            if (cmp > 0)
            {
                node.Right = RemoveNode(node.Right, item, comparison, out removed);
                return node;
            }

            if (!Equals(node.Data, item))
            {
                node.Right = RemoveNode(node.Right, item, comparison, out removed);
                return node;
            }

            removed = true;

            if (node.Left == null)
                return node.Right;

            if (node.Right == null)
                return node.Left;

            BstNode<T> successor = FindMin(node.Right);
            node.Data = successor.Data;
            bool ignored;
            node.Right = RemoveMin(node.Right, out ignored);

            return node;
        }

        private static BstNode<T> FindMin(BstNode<T> node)
        {
            BstNode<T> current = node;
            while (current.Left != null)
                current = current.Left;

            return current;
        }

        private static BstNode<T>? RemoveMin(BstNode<T>? node, out bool removed)
        {
            if (node == null)
            {
                removed = false;
                return null;
            }

            if (node.Left == null)
            {
                removed = true;
                return node.Right;
            }

            node.Left = RemoveMin(node.Left, out removed);
            return node;
        }

    }
}
